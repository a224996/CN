using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace PennyJinx
{
    internal class PennyJinx
    {
        private const String ChampName = "Jinx";
        public static Obj_AI_Hero Player;
        public static Spell Q, W, E, R;
        public static Menu Menu;
        private static Orbwalking.Orbwalker _orbwalker;
        private static readonly StringList QMode = new StringList(new[] {"AOE mode", "Range mode", "Both"}, 2);
        public static Render.Sprite Sprite;
        public static PennyJinx Instance;
        public static float LastCheck;

        public PennyJinx()
        {
            Instance = this;
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static HitChance CustomHitChance
        {
            get { return GetHitchance(); }
        }

        private void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;
            if (Player.ChampionName != ChampName)
            {
                return;
            }

            SetUpMenu();
            SetUpSpells();
            Game.PrintChat("<font color='#7A6EFF'>PennyJinx</font> v 1.0.2.2 <font color='#FFFFFF'>Loaded!</font>");

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Orbwalking.BeforeAttack += OrbwalkingBeforeAttack;
            Interrupter2.OnInterruptableTarget += Interrupter_OnInterruptable;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
        }

        private void Interrupter_OnInterruptable(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (sender.IsValidTarget() && IsMenuEnabled("Interrupter") && E.IsReady())
            {
                E.CastIfHitchanceEquals(sender, CustomHitChance, Packets());
            }
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            var sender = gapcloser.Sender;
            if (sender.IsValidTarget() && IsMenuEnabled("AntiGP") && E.IsReady())
            {
                E.Cast(gapcloser.End, Packets());
            }

        }

        private static void OrbwalkingBeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            var target = args.Target;
            if (!target.IsValidTarget())
            {
                return;
            }

            if (!(target is Obj_AI_Minion) ||
                (_orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.LaneClear &&
                 _orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.LastHit))
            {
                return;
            }

            var t2 = (Obj_AI_Minion) target;
            QSwitchLc(t2);
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            Auto();

            if (Menu.Item("ManualR").GetValue<KeyBind>().Active)
            {
                ManualR();
            }

            switch (_orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    ComboLogic();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    HarrassLogic();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    WUsageFarm();
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    WUsageFarm();
                    break;
            }

            UseSpellOnTeleport(E);
            AutoPot();

            // Cleanser.cleanserByBuffType();
            //Cleanser.cleanserBySpell();
        }

        #region Drawing

        private static void Drawing_OnDraw(EventArgs args)
        {
            var drawQ = Menu.Item("DrawQ").GetValue<Circle>();
            var drawW = Menu.Item("DrawW").GetValue<Circle>();
            var drawE = Menu.Item("DrawE").GetValue<Circle>();
            var drawR = Menu.Item("DrawR").GetValue<Circle>();
            var qRange = IsFishBone()
                ? 525f + ObjectManager.Player.BoundingRadius + 65f
                : 525f + ObjectManager.Player.BoundingRadius + 65f + GetFishboneRange() + 20f;
            if (drawQ.Active)
            {
                Render.Circle.DrawCircle(Player.Position, qRange, drawQ.Color);
            }

            if (drawW.Active)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color);
            }

            if (drawE.Active)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color);
            }

            if (drawR.Active)
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color);
            }
        }

        #endregion


        #region Various

        public void UseSpellOnTeleport(Spell spell)
        {
            if (!IsMenuEnabled("EOnTP") || (Environment.TickCount - LastCheck) < 1500)
            {
                return;
            }

            LastCheck = Environment.TickCount;
            var player = ObjectManager.Player;
            if (!spell.IsReady())
            {
                return;
            }

            foreach (
                var targetPosition in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(
                            obj =>
                                obj.Distance(player) < spell.Range && obj.Team != player.Team &&
                                obj.HasBuff("teleport_target", true)))
            {
                spell.Cast(targetPosition.ServerPosition);
            }
        }

        private static void AutoPot()
        {
            if (ObjectManager.Player.HasBuff("Recall") || Player.InFountain() && Player.InShop())
            {
                return;
            }

            //Health Pots
            if (IsMenuEnabled("APH") && GetPerValue(false) <= Menu.Item("APH_Slider").GetValue<Slider>().Value &&
                !Player.HasBuff("RegenerationPotion", true))
            {
                UseItem(2003);
            }
            //Mana Pots
            if (IsMenuEnabled("APM") && GetPerValue(true) <= Menu.Item("APM_Slider").GetValue<Slider>().Value &&
                !Player.HasBuff("FlaskOfCrystalWater", true))
            {
                UseItem(2004);
            }

            //Summoner Heal
            if (!IsMenuEnabled("APHeal") || !(GetPerValue(false) <= Menu.Item("APHeal_Slider").GetValue<Slider>().Value))
            {
                return;
            }

            var heal = Player.GetSpellSlot("summonerheal");
            if (heal != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(heal) == SpellState.Ready)
            {
                Player.Spellbook.CastSpell(heal);
            }
        }

        private static void TakeLantern()
        {
            /**
            foreach (var interactPkt in from obj in ObjectManager.Get<GameObject>()
                where
                    obj.Name.Contains("ThreshLantern") &&
                    obj.Position.Distance(ObjectManager.Player.ServerPosition) <= 500 && obj.IsAlly
                select new PKT_InteractReq
                {
                    NetworkId = Player.NetworkId,
                    TargetNetworkId = obj.NetworkId
                })
            {
                //Credits to Trees
                Game.SendPacket(interactPkt.Encode(), PacketChannel.C2S, PacketProtocolFlags.Reliable);
                return;
            
             * }
             * */

        }

        private static void SwitchLc()
        {
            if (!Q.IsReady())
            {
                return;
            }

            if (IsFishBone())
            {
                Q.Cast();
            }
        }

        private static void SwitchNoEn()
        {
            if (!IsMenuEnabled("SwitchQNoEn"))
            {
                return;
            }

            var range = IsFishBone()
                ? 525f + ObjectManager.Player.BoundingRadius + 65f
                : 525f + ObjectManager.Player.BoundingRadius + 65f + GetFishboneRange() + 20f;
            if (Player.CountEnemysInRange((int) range) != 0)
            {
                return;
            }

            if (IsFishBone())
            {
                Q.Cast();
            }
        }

        #endregion

        #region Combo/Harrass/Auto

        private void Auto()
        {
            if (GetEMode() == 0)
            {
                ECast_DZ();
            }
            else
            {
                ECast();
            }
            SwitchNoEn();
            AutoWHarass();
            AutoWEmpaired();
        }

        private void HarrassLogic()
        {
            WCast(_orbwalker.ActiveMode);
            QManager("H");
        }

        private void ComboLogic()
        {
            WCast(_orbwalker.ActiveMode);
            RCast();
            QManager("C");
            if (GetEMode() == 0)
            {
                ECast_DZ();
            }
            else
            {
                ECast();
            }
        }

        #endregion

        #region Farm

        private static void QSwitchLc(Obj_AI_Minion t2)
        {
            if (!IsMenuEnabled("UseQLC") || !Q.IsReady() || GetPerValue(true) < GetSliderValue("QManaLC"))
            {
                return;
            }

            if (CountEnemyMinions(t2, 150) < GetSliderValue("MinQMinions"))
            {
                SwitchLc();
            }
            else
            {
                if (!IsFishBone() && GetPerValue(true) >= GetSliderValue("QManaLC"))
                {
                    Q.Cast();
                }
            }
        }

        private static void WUsageFarm()
        {
            var mode = _orbwalker.ActiveMode;
            var wMana = mode == Orbwalking.OrbwalkingMode.LaneClear
                ? GetSliderValue("WManaLC")
                : GetSliderValue("WManaLH");
            var wEnabled = mode == Orbwalking.OrbwalkingMode.LaneClear
                ? IsMenuEnabled("UseWLC")
                : IsMenuEnabled("UseWLH");
            var mList = MinionManager.GetMinions(Player.Position, W.Range);
            var location = W.GetLineFarmLocation(mList);
            if (GetPerValue(true) >= wMana && wEnabled)
            {
                W.Cast(location.Position);
            }
        }

        #endregion

        #region Spell Casting

        private static void QManager(String mode)
        {
            if (!Q.IsReady())
            {
                return;
            }

            var aaRange = GetMinigunRange(null) + GetFishboneRange() + 25f;
            var target = TargetSelector.GetTarget(aaRange, TargetSelector.DamageType.Physical);
            var jinxBaseRange = GetMinigunRange(target);

            if (!target.IsValidTarget(aaRange + GetFishboneRange() + 25f))
            {
                return;
            }

            switch (Menu.Item("QMode").GetValue<StringList>().SelectedIndex)
            {
                //AOE Mode
                case 0:
                    if (IsFishBone() && GetPerValue(true) <= GetSliderValue("QMana" + mode))
                    {
                        Q.Cast();
                        return;
                    }
                    if (target.CountEnemysInRange(150) > 1)
                    {
                        if (!IsFishBone())
                        {
                            Q.Cast();
                        }
                    }
                    else
                    {
                        if (IsFishBone())
                        {
                            Q.Cast();
                        }
                    }
                    break;
                //Range Mode
                case 1:
                    if (IsFishBone())
                    {
                        //Switching to Minigun
                        if (Player.Distance(target) < jinxBaseRange ||
                            GetPerValue(true) <= GetSliderValue("QMana" + mode))
                        {
                            Q.Cast();
                        }
                    }
                    else
                    {
                        //Switching to rockets
                        if (Player.Distance(target) > jinxBaseRange &&
                            GetPerValue(true) >= GetSliderValue("QMana" + mode))
                        {
                            Q.Cast();
                        }
                    }
                    break;
                //Both
                case 2:
                    if (IsFishBone())
                    {
                        //Switching to Minigun
                        if (Player.Distance(target) < jinxBaseRange ||
                            GetPerValue(true) <= GetSliderValue("QMana" + mode))
                        {
                            Q.Cast();
                        }
                    }
                    else
                    {
                        //Switching to rockets
                        if (Player.Distance(target) > jinxBaseRange &&
                            GetPerValue(true) >= GetSliderValue("QMana" + mode) ||
                            target.CountEnemysInRange(150) > 1)
                        {
                            Q.Cast();
                        }
                    }
                    break;
            }
        }

        private static void WCast(Orbwalking.OrbwalkingMode mode)
        {
            if (mode != Orbwalking.OrbwalkingMode.Combo && mode != Orbwalking.OrbwalkingMode.Mixed || !W.IsReady())
            {
                return;
            }

            if (Player.CountEnemysInRange((int) Player.AttackRange) != 0)
            {
                return;
            }

            //If the mode is combo then we use the WManaC, if the mode is Harrass we use the WManaH
            var str = (mode == Orbwalking.OrbwalkingMode.Combo) ? "C" : "H";
            //Get a target in W range
            var wTarget = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);
            if (!wTarget.IsValidTarget(W.Range))
            {
                return;
            }

            var wMana = GetSliderValue("WMana" + str);
            if (GetPerValue(true) >= wMana && IsMenuEnabled("UseW"+str))
            {
                W.CastIfHitchanceEquals(wTarget, CustomHitChance, Packets());
            }
        }

        private static void ECast()
        {
            //Credits to Marksman
            //http://github.com/Esk0r/Leaguesharp/

            foreach (
                var enemy in
                    ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsValidTarget(E.Range - 150)))
            {
                if (!IsMenuEnabled("AutoE") || !E.IsReady() || !enemy.HasBuffOfType(BuffType.Slow))
                {
                    return;
                }

                var castPosition =
                    Prediction.GetPrediction(
                        new PredictionInput
                        {
                            Unit = enemy,
                            Delay = 0.7f,
                            Radius = 120f,
                            Speed = 1750f,
                            Range = 900f,
                            Type = SkillshotType.SkillshotCircle
                        }).CastPosition;
                if (GetSlowEndTime(enemy) >= (Game.Time + E.Delay + 0.5f))
                {
                    E.Cast(castPosition);
                }

                if (IsMenuEnabled("AutoE") && E.IsReady() &&
                    (enemy.HasBuffOfType(BuffType.Stun) || enemy.HasBuffOfType(BuffType.Snare) ||
                     enemy.HasBuffOfType(BuffType.Charm) || enemy.HasBuffOfType(BuffType.Fear) ||
                     enemy.HasBuffOfType(BuffType.Taunt)))
                {
                    E.CastIfHitchanceEquals(enemy, HitChance.High);
                }
            }
        }

        private static void ECast_DZ()
        {
            if (!E.IsReady())
            {
                return;
            }

            foreach (
                var enemy in
                    ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsValidTarget(E.Range - 140f) && (IsEmpaired(h))))
            {
                //E necessary mana. If the mode is combo: Combo mana, if not AutoE mana
                var eMana = _orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo
                    ? GetSliderValue("EManaC")
                    : GetSliderValue("AutoE_Mana");

                if ((!IsMenuEnabled("UseEC") && _orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo) || (!IsMenuEnabled("AutoE") && _orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo))
                {
                    return;
                }


                //If it is slowed & moving
                if (IsEmpairedLight(enemy) && IsMoving(enemy))
                {
                    //Has enough E Mana ?
                    if (GetPerValue(true) >= eMana)
                    {
                        //Casting using predictions
                        E.CastIfHitchanceEquals(enemy, CustomHitChance, Packets());
                        return;
                    }
                }
                //If the empairement ends later, cast the E
                if (GetPerValue(true) >= eMana)
                {
                    //Casting using predictions
                    E.CastIfHitchanceEquals(enemy, CustomHitChance, Packets());
                }
            }
        }
        private static void RCast()
        {
            //TODO R Collision
            if (!R.IsReady())
            {
                return;
            }

            var rTarget = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
            if (!rTarget.IsValidTarget(R.Range))
            {
                return;
            }
            //If is killable with W and AA
            //Or the ally players in there are > 0
            if (IsKillableWaa(rTarget) ||
                CountAllyPlayers(rTarget, 900) > 0 || Player.Distance(rTarget) < (W.Range/2))
            {
                return;
            }

            //Check for Mana && for target Killable. Also check for hitchance
            if (GetPerValue(true) >= GetSliderValue("RManaC") && IsMenuEnabled("UseRC") &&
                R.GetDamage(rTarget) >=
                HealthPrediction.GetHealthPrediction(rTarget, (int) (Player.Distance(rTarget)/2000f)*1000))
            {
                R.CastIfHitchanceEquals(rTarget, CustomHitChance, Packets());
            }
        }
        private static void ManualR()
        {
            //TODO R Collision
            if (!R.IsReady())
            {
                return;
            }

            var rTarget = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
            if (!rTarget.IsValidTarget(R.Range))
            {
                return;
            }


            //Check for Mana && for target Killable. Also check for hitchance
            if (R.GetDamage(rTarget) >=
                HealthPrediction.GetHealthPrediction(rTarget, (int)(Player.Distance(rTarget) / 2000f) * 1000))
            {
                R.CastIfHitchanceEquals(rTarget, CustomHitChance, Packets());
            }
        }
        #endregion

        #region AutoSpells

        private static void AutoWHarass()
        {
            //Uses W in Harrass, factoring hitchance
            if (!IsMenuEnabled("AutoW") || Player.IsRecalling())
            {
                return;
            }

            var wTarget = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);
            var autoWMana = GetSliderValue("AutoW_Mana");
            if (!wTarget.IsValidTarget())
            {
                return;
            }

            if (GetPerValue(true) >= autoWMana || IsKillableWaa(wTarget))
            {
                W.CastIfHitchanceEquals(wTarget, CustomHitChance, Packets());
            }
        }

        private void AutoWEmpaired()
        {
            if (!IsMenuEnabled("AutoWEmp") || Player.IsRecalling())
            {
                return;
            }

            //Uses W on whoever is empaired
            foreach (
                var enemy in
                    (from enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsValidTarget(W.Range))
                        let autoWMana = GetSliderValue("AutoWEmp_Mana")
                        where GetPerValue(true) >= autoWMana
                        select enemy).Where(enemy => IsEmpaired(enemy) || IsEmpairedLight(enemy)))
            {
                W.CastIfHitchanceEquals(enemy, CustomHitChance, Packets());
            }
        }

        #endregion

        #region Utility

        public static void UseItem(int id, Obj_AI_Hero target = null)
        {
            if (Items.HasItem(id) && Items.CanUseItem(id))
            {
                Items.UseItem(id, target);
            }
        }

        private static bool Packets()
        {
            return false;
        }

        private static float GetFishboneRange()
        {
            return 50 + 25*ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Level;
        }

        private static float GetMinigunRange(GameObject target)
        {
            return 525f + ObjectManager.Player.BoundingRadius + (target != null ? target.BoundingRadius : 0);
        }

        private static HitChance GetHitchance()
        {
            switch (Menu.Item("C_Hit").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    return HitChance.Low;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.High;
                case 3:
                    return HitChance.VeryHigh;
                default:
                    return HitChance.Medium;
            }
        }

        private static bool IsKillableWaa(Obj_AI_Hero wTarget)
        {
            if (Player.Distance(wTarget) > W.Range)
            {
                return false;
            }

            return (Player.GetAutoAttackDamage(wTarget) + W.GetDamage(wTarget) >
                    HealthPrediction.GetHealthPrediction(
                        wTarget,
                        (int)
                            ((Player.Distance(wTarget)/W.Speed)*1000 +
                             (Player.Distance(wTarget)/Orbwalking.GetMyProjectileSpeed())*1000) + (Game.Ping/2)) &&
                    Player.Distance(wTarget) <= Orbwalking.GetRealAutoAttackRange(null));
        }


        private static int CountAllyPlayers(Obj_AI_Hero from, float distance)
        {
            return
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(h => h.IsAlly && !h.IsMe && h.Distance(from) <= distance)
                    .ToList()
                    .Count;
        }

        private static int CountEnemyMinions(Obj_AI_Base from, float distance)
        {
            return MinionManager.GetMinions(from.Position, distance).ToList().Count;
        }

        private static bool IsFishBone()
        {
            return Player.AttackRange > 565f;
        }

        private static int GetEMode()
        {
            return Menu.Item("EMode").GetValue<StringList>().SelectedIndex;
        }

        private static bool IsEmpaired(Obj_AI_Hero enemy)
        {
            return (enemy.HasBuffOfType(BuffType.Stun) || enemy.HasBuffOfType(BuffType.Snare) ||
                    enemy.HasBuffOfType(BuffType.Charm) || enemy.HasBuffOfType(BuffType.Fear) ||
                    enemy.HasBuffOfType(BuffType.Taunt) || IsEmpairedLight(enemy));
        }

        private static bool IsEmpairedLight(Obj_AI_Hero enemy)
        {
            return (enemy.HasBuffOfType(BuffType.Slow));
        }

        private static float GetEmpairedEndTime(Obj_AI_Base target)
        {
            return
                target.Buffs.OrderByDescending(buff => buff.EndTime - Game.Time)
                    .Where(buff => GetEmpairedBuffs().Contains(buff.Type))
                    .Select(buff => buff.EndTime)
                    .FirstOrDefault();
        }

        private static float GetSlowEndTime(Obj_AI_Base target)
        {
            return
                target.Buffs.OrderByDescending(buff => buff.EndTime - Game.Time)
                    .Where(buff => buff.Type == BuffType.Slow)
                    .Select(buff => buff.EndTime)
                    .FirstOrDefault();
        }

        public static bool IsMenuEnabled(String opt)
        {
            return Menu.Item(opt).GetValue<bool>();
        }

        public static int GetSliderValue(String opt)
        {
            return Menu.Item(opt).GetValue<Slider>().Value;
        }
        public static bool GetKeyBindValue(String opt)
        {
            return Menu.Item(opt).GetValue<KeyBind>().Active;
        }

        private static float GetPerValue(bool mana)
        {
            return mana ? Player.ManaPercentage() : Player.HealthPercentage();
        }

        private static bool IsMoving(Obj_AI_Base obj)
        {
            return obj.Path.Count() > 1;
        }

        private static List<BuffType> GetEmpairedBuffs()
        {
            return new List<BuffType>
            {
                BuffType.Stun,
                BuffType.Snare,
                BuffType.Charm,
                BuffType.Fear,
                BuffType.Taunt,
                BuffType.Slow
            };
        }

        #endregion

        #region Menu and spells setup

        private static void SetUpSpells()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 1500f);
            E = new Spell(SpellSlot.E, 900f);
            R = new Spell(SpellSlot.R, 2000f);
            W.SetSkillshot(0.6f, 60f, 3300f, true, SkillshotType.SkillshotLine);
            E.SetSkillshot(1.1f, 120f, 1750f, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.6f, 140f, 1700f, false, SkillshotType.SkillshotLine);
        }

        private static void SetUpMenu()
        {

            Menu = new Menu("【無為汉化】PJ-金克斯", "PJinx", true);

            var orbMenu = new Menu("走砍", "OW");
            _orbwalker = new Orbwalking.Orbwalker(orbMenu);
            var tsMenu = new Menu("目标选择", "TS");
            TargetSelector.AddToMenu(tsMenu);
            Menu.AddSubMenu(orbMenu);
            Menu.AddSubMenu(tsMenu);
            var comboMenu = new Menu("[PJ] 连招", "Combo");
            {
                comboMenu.AddItem(new MenuItem("UseQC", "使用 Q 连招").SetValue(true));
                comboMenu.AddItem(new MenuItem("UseWC", "使用 W 连招").SetValue(true));
                comboMenu.AddItem(new MenuItem("UseEC", "使用 E 连招").SetValue(true));
                comboMenu.AddItem(new MenuItem("UseRC", "使用 R 连招").SetValue(true));
                comboMenu.AddItem(new MenuItem("QMode", "Q 使用模式").SetValue(QMode));
                comboMenu.AddItem(
                    new MenuItem("EMode", "E 模式").SetValue(new StringList(new[] { "PJ金克斯", "ADC合集" })));
            }
            var manaManagerCombo = new Menu("蓝量管理", "mm_Combo");
            {
                manaManagerCombo.AddItem(new MenuItem("QManaC", "Q 蓝量").SetValue(new Slider(15)));
                manaManagerCombo.AddItem(new MenuItem("WManaC", "W 蓝量").SetValue(new Slider(35)));
                manaManagerCombo.AddItem(new MenuItem("EManaC", "E 蓝量").SetValue(new Slider(25)));
                manaManagerCombo.AddItem(new MenuItem("RManaC", "R 蓝量").SetValue(new Slider(5)));
            }
            comboMenu.AddSubMenu(manaManagerCombo);
            Menu.AddSubMenu(comboMenu);

            var harassMenu = new Menu("[PJ] 骚扰", "Harass");
            {
                harassMenu.AddItem(new MenuItem("UseQH", "使用 Q 骚扰").SetValue(true));
                harassMenu.AddItem(new MenuItem("UseWH", "使用 W 骚扰").SetValue(true));
            }
            var manaManagerHarrass = new Menu("蓝量管理", "mm_Harrass");
            {
                manaManagerHarrass.AddItem(new MenuItem("QManaH", "Q 蓝量").SetValue(new Slider(15)));
                manaManagerHarrass.AddItem(new MenuItem("WManaH", "W 蓝量").SetValue(new Slider(35)));
            }
            harassMenu.AddSubMenu(manaManagerHarrass);
            Menu.AddSubMenu(harassMenu);

            var farmMenu = new Menu("[PJ] 发育", "Farm");
            {
                farmMenu.AddItem(new MenuItem("UseQLC", "使用 Q 清兵").SetValue(true));
                farmMenu.AddItem(new MenuItem("UseWLH", "使用 W 补兵").SetValue(false));
                farmMenu.AddItem(new MenuItem("UseWLC", "使用 W 清兵").SetValue(false));
                farmMenu.AddItem(new MenuItem("MinQMinions", "最低蓝量 使用Q").SetValue(new Slider(0, 4, 6)));
            }
            var manaManagerFarm = new Menu("蓝量管理", "mm_Farm");
            {
                manaManagerFarm.AddItem(new MenuItem("QManaLC", "Q 清兵蓝量").SetValue(new Slider(15)));
                manaManagerFarm.AddItem(new MenuItem("WManaLH", "W 补兵蓝量").SetValue(new Slider(35)));
                manaManagerFarm.AddItem(new MenuItem("WManaLC", "W 清兵蓝量").SetValue(new Slider(35)));
            }

            farmMenu.AddSubMenu(manaManagerFarm);
            Menu.AddSubMenu(farmMenu);

            var miscMenu = new Menu("[PJ] 杂项", "Misc");
            {
                miscMenu.AddItem(new MenuItem("AntiGP", "反突进").SetValue(true));
                miscMenu.AddItem(new MenuItem("EOnTP", "E 在传送位置").SetValue(true));
                miscMenu.AddItem(new MenuItem("Interrupter", "使用打断技能").SetValue(true));
                miscMenu.AddItem(new MenuItem("SwitchQNoEn", "没有敌人切换到机枪").SetValue(true));
                miscMenu.AddItem(new MenuItem("C_Hit", "击中几率").SetValue(new StringList(new[] {"低", "中", "高", "非常高"}, 2)));
                miscMenu.AddItem(new MenuItem("ManualR", "手动 R").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
            }
            Menu.AddSubMenu(miscMenu);

            var autoMenu = new Menu("[PJ] 自动骚扰", "Auto");
            {
                autoMenu.AddItem(new MenuItem("AutoE", "自动 E 慢/不动").SetValue(true));
                autoMenu.AddItem(new MenuItem("AutoE_Mana", "蓝量").SetValue(new Slider(35)));
                autoMenu.AddItem(new MenuItem("AutoW", "自动 W").SetValue(true));
                autoMenu.AddItem(new MenuItem("AutoW_Mana", "蓝量").SetValue(new Slider(40)));
                autoMenu.AddItem(new MenuItem("AutoWEmp", "自动 W 慢/不动").SetValue(true));
                autoMenu.AddItem(new MenuItem("AutoWEmp_Mana", "蓝量").SetValue(new Slider(40)));
            }
            Menu.AddSubMenu(autoMenu);


            Cleanser.OnLoad();
            ItemManager.OnLoad(Menu);
            PotionManager.OnLoad(Menu);

            var drawMenu = new Menu("[PJ] 绘制", "Drawing");
            {
                drawMenu.AddItem(new MenuItem("DrawQ", "绘制 Q").SetValue(new Circle(true, Color.Red)));
                drawMenu.AddItem(
                    new MenuItem("DrawW", "绘制 W").SetValue(new Circle(true, Color.MediumPurple)));
                drawMenu.AddItem(
                    new MenuItem("DrawE", "绘制 E").SetValue(new Circle(true, Color.MediumPurple)));
                drawMenu.AddItem(new MenuItem("DrawR", "绘制 R").SetValue(new Circle(true, Color.MediumPurple)));
                miscMenu.AddItem(new MenuItem("SpriteDraw", "绘制R可击杀").SetValue(false));
            }
            Menu.AddSubMenu(drawMenu);

            Menu.AddToMainMenu();
        }

        #endregion
    }
}
