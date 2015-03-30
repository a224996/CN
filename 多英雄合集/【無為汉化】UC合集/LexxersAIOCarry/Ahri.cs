using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace UltimateCarry
{
    class Ahri : Champion
    {
        private Menu _menu;

        private Items.Item _itemDFG;

        private Spell _spellQ, _spellW, _spellE, _spellR;

        const float _spellQSpeed = 2500;
        const float _spellQSpeedMin = 400;
        const float _spellQFarmSpeed = 1600;

        public Ahri()
        {
            _menu = Program.Menu;

            var comboMenu = _menu.AddSubMenu(new Menu("团队作战", "TeamFight"));
            comboMenu.AddItem(new MenuItem("comboQ", "使用 Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("comboW", "使用 W").SetValue(true));
            comboMenu.AddItem(new MenuItem("comboE", "使用 E").SetValue(true));
            comboMenu.AddItem(new MenuItem("comboR", "使用 R").SetValue(true));
            comboMenu.AddItem(new MenuItem("comboROnlyUserInitiate", "使用 R 手动开启").SetValue(false));

            var harassMenu = _menu.AddSubMenu(new Menu("骚扰", "Harass"));
            harassMenu.AddItem(new MenuItem("harassQ", "使用 Q").SetValue(true));
            harassMenu.AddItem(new MenuItem("harassE", "使用 E").SetValue(true));
            harassMenu.AddItem(new MenuItem("harassPercent", "蓝量 %").SetValue(new Slider(20)));

            var farmMenu = _menu.AddSubMenu(new Menu("清兵", "LaneClear"));
            farmMenu.AddItem(new MenuItem("farmQ", "使用 Q").SetValue(true));
            farmMenu.AddItem(new MenuItem("farmW", "使用 W").SetValue(false));
            farmMenu.AddItem(new MenuItem("farmPercent", "蓝量 %").SetValue(new Slider(20)));
            farmMenu.AddItem(new MenuItem("farmStartAtLevel", "只用AA 直到升级").SetValue(new Slider(8, 1, 18)));

            var drawMenu = _menu.AddSubMenu(new Menu("绘制", "Drawing"));
            drawMenu.AddItem(new MenuItem("drawQE", "绘制 Q, E 范围").SetValue(new Circle(true, System.Drawing.Color.FromArgb(125, 0, 255, 0))));
            drawMenu.AddItem(new MenuItem("drawW", "绘制 W 范围").SetValue(new Circle(false, System.Drawing.Color.FromArgb(125, 0, 0, 255))));
            var dmgAfterComboItem = new MenuItem("DamageAfterCombo", "绘制 损伤").SetValue(true); //copied from esk0r Syndra
            drawMenu.AddItem(dmgAfterComboItem);


            _itemDFG = Utility.Map.GetMap().Type  == Utility.Map.MapType.TwistedTreeline ? new Items.Item(3188, 750) : new Items.Item(3128, 750);

            _spellQ = new Spell(SpellSlot.Q, 990);
            _spellW = new Spell(SpellSlot.W, 795 - 95);
            _spellE = new Spell(SpellSlot.E, 1000 - 10);
            _spellR = new Spell(SpellSlot.R, 1000 - 100);

            _spellQ.SetSkillshot(.215f, 100, 1600f, false, SkillshotType.SkillshotLine);
            _spellW.SetSkillshot(.71f, _spellW.Range, float.MaxValue, false, SkillshotType.SkillshotLine);
            _spellE.SetSkillshot(.23f, 60, 1500f, true, SkillshotType.SkillshotLine);

            Utility.HpBarDamageIndicator.DamageToUnit = GetComboDamage;
            Utility.HpBarDamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
            dmgAfterComboItem.ValueChanged += delegate(object sender, OnValueChangeEventArgs eventArgs) { Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>(); };

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnGameUpdate;

            PluginLoaded();
        }

        void Game_OnGameUpdate(EventArgs args)
        {            
            switch (Program.Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    LaneClear();
                    break;
                default:
                    break;
            }
        }

        void Harass()
        {
            if (_menu.Item("harassE").GetValue<bool>() && GetManaPercent() >= _menu.Item("harassPercent").GetValue<Slider>().Value)
                CastE();

            if (_menu.Item("harassQ").GetValue<bool>() && GetManaPercent() >= _menu.Item("harassPercent").GetValue<Slider>().Value)
                CastQ();
        }

        void LaneClear()
        {
            _spellQ.Speed = _spellQFarmSpeed;
            var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _spellQ.Range, MinionTypes.All, MinionTeam.NotAlly);

            bool jungleMobs = minions.Any(x => x.Team == GameObjectTeam.Neutral);

            if ((_menu.Item("farmQ").GetValue<bool>() && GetManaPercent() >= _menu.Item("farmPercent").GetValue<Slider>().Value && ObjectManager.Player.Level >= _menu.Item("farmStartAtLevel").GetValue<Slider>().Value) || jungleMobs)
            {
                MinionManager.FarmLocation farmLocation = _spellQ.GetLineFarmLocation(minions);

                if (farmLocation.Position.IsValid())
                    if (farmLocation.MinionsHit >= 2 || jungleMobs)
                        CastQ(farmLocation.Position);
            }

            minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _spellW.Range, MinionTypes.All, MinionTeam.NotAlly);

            if (minions.Count() > 0)
            {
                jungleMobs = minions.Any(x => x.Team == GameObjectTeam.Neutral);

                if ((_menu.Item("farmW").GetValue<bool>() && GetManaPercent() >= _menu.Item("farmPercent").GetValue<Slider>().Value && ObjectManager.Player.Level >= _menu.Item("farmStartAtLevel").GetValue<Slider>().Value) || jungleMobs)
                    CastW(true);
            }
        }

        void CastE()
        {
            if (!_spellE.IsReady())
                return;

            var target = TargetSelector.GetTarget(_spellE.Range, TargetSelector.DamageType.Magical);

            if (target != null)
                _spellE.CastIfHitchanceEquals(target, HitChance.High, Packets());
        }

        void CastQ()
        {
            if (!_spellQ.IsReady())
                return;

            var target = TargetSelector.GetTarget(_spellQ.Range, TargetSelector.DamageType.Magical);

            if (target != null)
            {
                Vector3 predictedPos = Prediction.GetPrediction(target, _spellQ.Delay).UnitPosition; //correct pos currently not possible with spell acceleration
                _spellQ.Speed = GetDynamicQSpeed(ObjectManager.Player.Distance(predictedPos));
                _spellQ.CastIfHitchanceEquals(target, HitChance.High, Packets());
            }
        }

        void CastQ(Vector2 pos)
        {
            if (!_spellQ.IsReady())
                return;

            _spellQ.Cast(pos, Packets());
        }

        void CastW(bool ignoreTargetCheck = false)
        {
            if (!_spellW.IsReady())
                return;

            var target = TargetSelector.GetTarget(_spellW.Range, TargetSelector.DamageType.Magical);

            if (target != null || ignoreTargetCheck)
                _spellW.Cast(ObjectManager.Player.Position, Packets());
        }

        void Combo()
        {
            if (_menu.Item("comboE").GetValue<bool>())
                CastE();

            if (_menu.Item("comboQ").GetValue<bool>())
                CastQ();

            if (_menu.Item("comboW").GetValue<bool>())
                CastW();

            if (_menu.Item("comboR").GetValue<bool>() && _spellR.IsReady())
                if (OkToUlt())
                    _spellR.Cast(Game.CursorPos, Packets());
        }

		List<SpellSlot> GetSpellCombo()
		{
			var spellCombo = new List<SpellSlot>();

			if(_spellQ.IsReady())
				spellCombo.Add(SpellSlot.Q);
			if(_spellW.IsReady())
				spellCombo.Add(SpellSlot.W);
			if(_spellE.IsReady())
				spellCombo.Add(SpellSlot.E);
			if(_spellR.IsReady())
				spellCombo.Add(SpellSlot.R);
			return spellCombo;
		}

        float GetComboDamage(Obj_AI_Base target)
		{
            double comboDamage = (float)ObjectManager.Player.GetComboDamage(target,GetSpellCombo());

			return (float)(comboDamage + ObjectManager.Player.GetAutoAttackDamage(target ));
        }

        bool OkToUlt()
        {
            if (Program.Helper.EnemyTeam.Any(x => x.Distance(ObjectManager.Player.ServerPosition) < 500)) //any enemies around me?
                return true;

            Vector3 mousePos = Game.CursorPos;

            var enemiesNearMouse = Program.Helper.EnemyTeam.Where(x => x.Distance(ObjectManager.Player.ServerPosition) < _spellR.Range && x.Distance(mousePos) < 650);

            if (enemiesNearMouse.Count() > 0)
            {
                if (IsRActive()) //R already active
                    return true;

                bool enoughMana = ObjectManager.Player.Mana > ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).ManaCost + ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).ManaCost + ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).ManaCost;

                if (_menu.Item("comboROnlyUserInitiate").GetValue<bool>() || !(_spellQ.IsReady() && _spellE.IsReady()) || !enoughMana) //dont initiate if user doesnt want to, also dont initiate if Q and E isnt ready or not enough mana for QER combo
                    return false;

                var friendsNearMouse = Program.Helper.OwnTeam.Where(x => x.IsMe || x.Distance(mousePos) < 650); //me and friends near mouse (already in fight)

                if (enemiesNearMouse.Count() == 1) //x vs 1 enemy
                {
                    Obj_AI_Hero enemy = enemiesNearMouse.FirstOrDefault();

                    bool underTower = Utility.UnderTurret(enemy);

                    return GetComboDamage(enemy) / enemy.Health >= (underTower ? 1.25f : 1); //if enemy under tower, only initiate if combo damage is >125% of enemy health
                }
                else //fight if enemies low health or 2 friends vs 3 enemies and 3 friends vs 3 enemies, but not 2vs4
                {
                    int lowHealthEnemies = enemiesNearMouse.Count(x => x.Health / x.MaxHealth <= 0.1); //dont count low health enemies

                    float totalEnemyHealth = enemiesNearMouse.Sum(x => x.Health);

                    return friendsNearMouse.Count() - (enemiesNearMouse.Count() - lowHealthEnemies) >= -1 || ObjectManager.Player.Health / totalEnemyHealth >= 0.8;
                }
            }

            return false;
        }

        void Drawing_OnDraw(EventArgs args)
        {
            if (!ObjectManager.Player.IsDead)
            {
                var drawQE = _menu.Item("drawQE").GetValue<Circle>();
                var drawW = _menu.Item("drawW").GetValue<Circle>();

                if (drawQE.Active)
                    Utility.DrawCircle(ObjectManager.Player.Position, _spellQ.Range, drawQE.Color);

                if (drawW.Active)
                    Utility.DrawCircle(ObjectManager.Player.Position, _spellW.Range, drawW.Color);
            }
        }

        float GetDynamicQSpeed(float distance)
        {
            float accelerationrate = _spellQ.Range / (_spellQSpeedMin - _spellQSpeed); // = -0.476...
            return _spellQSpeed + accelerationrate * distance;
        }

        bool IsRActive()
        {
            return ObjectManager.Player.HasBuff("AhriTumble", true);
        }

        int GetRStacks()
        {
            BuffInstance tumble = ObjectManager.Player.Buffs.FirstOrDefault(x => x.Name == "AhriTumble");
            return tumble != null ? tumble.Count : 0;
        }
    }
}
