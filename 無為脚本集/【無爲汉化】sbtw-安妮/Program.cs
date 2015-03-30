#region

/*
 * Credits to:
 * Eskor
 * Roach_
 * Both for helping me alot doing this Assembly and start On L# 
 * lepqm for cleaning my shit up
 * iMeh Code breaker 101
 */
using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

#endregion

namespace Annie
{
    internal class Program
    {
        public const string CharName = "Annie";
        public static Orbwalking.Orbwalker Orbwalker;
        public static List<Spell> SpellList = new List<Spell>();
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static Spell R1;
        public static float DoingCombo;
        public static SpellSlot IgniteSlot;
        public static SpellSlot FlashSlot;
        public static Menu Config;

        private static int StunCount
        {
            get
            {
                foreach (var buff in
                    ObjectManager.Player.Buffs.Where(
                        buff => buff.Name == "pyromania" || buff.Name == "pyromania_particle"))
                {
                    switch (buff.Name)
                    {
                        case "pyromania":
                            return buff.Count;
                        case "pyromania_particle":
                            return 4;
                    }
                }

                return 0;
            }
        }

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != CharName)
            {
                return;
            }

            IgniteSlot = ObjectManager.Player.GetSpellSlot("SummonerDot");
            FlashSlot = ObjectManager.Player.GetSpellSlot("SummonerFlash");

            Q = new Spell(SpellSlot.Q, 625f);
            W = new Spell(SpellSlot.W, 625f);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 600f);
            R1 = new Spell(SpellSlot.R, 900f);

            Q.SetTargetted(0.25f, 1400f);
            W.SetSkillshot(0.60f, 50f * (float) Math.PI / 180, float.MaxValue, false, SkillshotType.SkillshotCone);
            R.SetSkillshot(0.20f, 200f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            R1.SetSkillshot(0.25f, 200f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(R);
            SpellList.Add(R1);

            Config = new Menu("【無爲汉化】sbtw-安妮", CharName, true);

            Config.AddSubMenu(new Menu("走砍", "Orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalker"));

            var targetSelectorMenu = new Menu("目标选择器", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);

            Config.AddSubMenu(targetSelectorMenu);
            Config.AddSubMenu(new Menu("连招设置", "combo"));
            Config.SubMenu("combo").AddItem(new MenuItem("qCombo", "使用 Q")).SetValue(true);
            Config.SubMenu("combo").AddItem(new MenuItem("wCombo", "使用 W")).SetValue(true);
            Config.SubMenu("combo").AddItem(new MenuItem("rCombo", "使用 R")).SetValue(true);
            Config.SubMenu("combo").AddItem(new MenuItem("itemsCombo", "使用点燃")).SetValue(true);
            Config.SubMenu("combo")
                .AddItem(new MenuItem("flashCombo", "几个目标使用闪现->R（晕）"))
                .SetValue(new Slider(4, 5, 1));

            Config.AddSubMenu(new Menu("骚扰（混合模式）设置", "harass"));
            Config.SubMenu("harass")
                .AddItem(new MenuItem("qFarmHarass", "补兵与叠晕(Q)").SetValue(true));
            Config.SubMenu("harass").AddItem(new MenuItem("qHarass", "骚扰使用 Q")).SetValue(true);
            Config.SubMenu("harass").AddItem(new MenuItem("wHarass", "骚扰使用 W")).SetValue(true);

            Config.AddSubMenu(new Menu("发育设置", "lasthit"));
            Config.SubMenu("lasthit").AddItem(new MenuItem("qFarm", "补兵使用 (Q)").SetValue(true));
            Config.SubMenu("lasthit").AddItem(new MenuItem("wFarm", "清兵使用 (W)").SetValue(true));
            Config.SubMenu("lasthit")
                .AddItem(new MenuItem("saveqStun", "当有晕时不使用技能").SetValue(true));
            Config.AddSubMenu(new Menu("绘制设置", "draw"));

            Config.AddSubMenu(new Menu("杂项", "misc"));
            Config.SubMenu("misc").AddItem(new MenuItem("PCast", "使用封包").SetValue(true));
            Config.SubMenu("misc").AddItem(new MenuItem("autoShield", "自动屏蔽 AAs").SetValue(false));
            Config.SubMenu("misc").AddItem(new MenuItem("suppMode", "支持模式").SetValue(false));
            Config.SubMenu("misc").AddItem(new MenuItem("FountainPassive", "对方进泉水自动眩晕").SetValue(true));
            Config.SubMenu("misc").AddItem(new MenuItem("LanePassive", "对方越塔自动眩晕").SetValue(true));
            Config.SubMenu("misc")
                .AddItem(new MenuItem("LanePassivePercent", "最低蓝量%").SetValue(new Slider(60)));

            Config.SubMenu("draw")
                .AddItem(
                    new MenuItem("QDraw", "Q范围").SetValue(
                        new Circle(true, Color.FromArgb(128, 178, 0, 0))));
            Config.SubMenu("draw")
                .AddItem(
                    new MenuItem("WDraw", "W范围").SetValue(
                        new Circle(false, Color.FromArgb(128, 32, 178, 170))));
            Config.SubMenu("draw")
                .AddItem(
                    new MenuItem("RDraw", "R范围").SetValue(
                        new Circle(true, Color.FromArgb(128, 128, 0, 128))));
            Config.SubMenu("draw")
                .AddItem(
                    new MenuItem("R1Draw", "闪现+R范围").SetValue(
                        new Circle(true, Color.FromArgb(128, 128, 0, 128))));

            Config.AddToMainMenu();

            Drawing.OnDraw += OnDraw;
            Game.OnUpdate += OnGameUpdate;
            GameObject.OnCreate += OnCreateObject;
            Orbwalking.BeforeAttack += OrbwalkingBeforeAttack;

            Game.PrintChat("Annie# Loaded");
        }

        private static void OnDraw(EventArgs args)
        {
            // Utility.DrawCircle(R1.GetPrediction(SimpleTs.GetTarget(900, SimpleTs.DamageType.Magical)).CastPosition, 250,
            //     Color.Aquamarine);
            foreach (var spell in SpellList)
            {
                var menuItem = Config.Item(spell.Slot + "Draw").GetValue<Circle>();
                if (menuItem.Active)
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, spell.Range, menuItem.Color);
                }
            }
        }

        private static void OnCreateObject(GameObject sender, EventArgs args)
        {
            if (sender.IsAlly || !(sender is Obj_SpellMissile) || !Config.Item("autoShield").GetValue<bool>())
            {
                return;
            }

            var missile = (Obj_SpellMissile) sender;
            if (!(missile.SpellCaster is Obj_AI_Hero) || !(missile.Target.IsMe))
            {
                return;
            }

            if (E.IsReady())
            {
                E.Cast();
            }
            else if (!ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(missile.SpellCaster.NetworkId).IsMelee())
            {
                var ecd = (int) (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).CooldownExpires - Game.Time) *
                          1000;
                if ((int) Vector3.Distance(missile.Position, ObjectManager.Player.ServerPosition) /
                    ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(missile.SpellCaster.NetworkId)
                        .BasicAttack.MissileSpeed * 1000 > ecd)
                {
                    Utility.DelayAction.Add(ecd, () => E.Cast(ObjectManager.Player, true));
                }
            }
        }

        private static void OnGameUpdate(EventArgs args)
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var flashRtarget = TargetSelector.GetTarget(900, TargetSelector.DamageType.Magical);

            ChargeStun();
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Orbwalker.SetAttack(false);
                    Combo(target, flashRtarget);
                    Orbwalker.SetAttack(true);
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    if (Config.Item("suppMode").GetValue<bool>())
                    {
                        Farm(false);
                    }
                    Harass(target);
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    Farm(false);
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    Farm(true);
                    break;
            }
        }

        private static void ChargeStun()
        {
            if (StunCount == 4 || ObjectManager.Player.IsDead)
            {
                return;
            }

            if (Config.Item("FountainPassive").GetValue<bool>() && ObjectManager.Player.InFountain())
            {
                if (E.IsReady())
                {
                    E.Cast();
                    return;
                }

                if (W.IsReady())
                {
                    W.Cast(Game.CursorPos);
                }
                return;
            }

            if (Config.Item("LanePassive").GetValue<bool>() && E.IsReady() &&
                ObjectManager.Player.ManaPercentage() >= Config.Item("LanePassivePercent").GetValue<Slider>().Value)
            {
                E.Cast();
            }
        }

        private static void OrbwalkingBeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            args.Process = Environment.TickCount > DoingCombo;
        }

        private static void Harass(Obj_AI_Base target)
        {
            if (Config.Item("qHarass").GetValue<bool>() && Q.IsReady())
            {
                Q.Cast(target, Config.Item("PCast").GetValue<bool>());
            }
            if (Config.Item("wHarass").GetValue<bool>() && W.IsReady())
            {
                W.Cast(target, Config.Item("PCast").GetValue<bool>());
            }
        }

        private static void Combo(Obj_AI_Base target, Obj_AI_Base flashRtarget)
        {
            if ((target == null && flashRtarget == null) || Environment.TickCount < DoingCombo ||
                (!Q.IsReady() && !W.IsReady() && !R.IsReady()))
            {
                return;
            }
            if (Config.Item("itemsCombo").GetValue<bool>() && target != null)
            {
                Items.UseItem(3128, target);
            }


            var useQ = Config.Item("qCombo").GetValue<bool>();
            var useW = Config.Item("wCombo").GetValue<bool>();
            var useR = Config.Item("rCombo").GetValue<bool>();
            switch (StunCount)
            {
                case 3:
                    if (target == null)
                    {
                        return;
                    }
                    if (Q.IsReady() && useQ)
                    {
                        DoingCombo = Environment.TickCount;
                        Q.Cast(target, Config.Item("PCast").GetValue<bool>());
                        Utility.DelayAction.Add(
                            (int) (ObjectManager.Player.Distance(target, false) / Q.Speed * 1000 - Game.Ping / 2.0) +
                            250, () =>
                            {
                                if (R.IsReady() &&
                                    !(ObjectManager.Player.GetSpellDamage(target, SpellSlot.R) > target.Health))
                                {
                                    R.Cast(target, false, true);
                                }
                            });
                    }
                    else if (W.IsReady() && useW)
                    {
                        W.Cast(target);
                        DoingCombo = Environment.TickCount + 250f;
                    }


                    break;
                case 4:
                    if (ObjectManager.Player.Spellbook.CanUseSpell(FlashSlot) == SpellState.Ready && R.IsReady() &&
                        target == null)
                    {
                        var position = R1.GetPrediction(flashRtarget, true).CastPosition;

                        if (ObjectManager.Player.Distance(position) > 600 &&
                            GetEnemiesInRange(flashRtarget.ServerPosition, 250) >=
                            Config.Item("flashCombo").GetValue<Slider>().Value)
                        {
                            ObjectManager.Player.Spellbook.CastSpell(FlashSlot, position);
                        }

                        Items.UseItem(3128, flashRtarget);
                        R.Cast(flashRtarget, false, true);

                        if (W.IsReady() && useW)
                        {
                            W.Cast(flashRtarget, false, true);
                        }
                        else if (Q.IsReady() && useQ)
                        {
                            Q.Cast(flashRtarget, Config.Item("PCast").GetValue<bool>());
                        }
                    }
                    else if (target != null)
                    {
                        if (R.IsReady() && useR &&
                            !(ObjectManager.Player.GetSpellDamage(target, SpellSlot.R) * 0.6 > target.Health))
                        {
                            R.Cast(target, false, true);
                        }

                        if (W.IsReady() && useW)
                        {
                            W.Cast(target, false, true);
                        }

                        if (Q.IsReady() && useQ)
                        {
                            Q.Cast(target, Config.Item("PCast").GetValue<bool>());
                        }
                    }
                    break;
                default:
                    if (Q.IsReady() && useQ)
                    {
                        Q.Cast(target, Config.Item("PCast").GetValue<bool>());
                    }

                    if (W.IsReady() && useW)
                    {
                        W.Cast(target, false, true);
                    }

                    break;
            }

            if (IgniteSlot != SpellSlot.Unknown && target != null &&
                ObjectManager.Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready &&
                ObjectManager.Player.Distance(target, false) < 600 &&
                ObjectManager.Player.GetSpellDamage(target, IgniteSlot) > target.Health)
            {
                ObjectManager.Player.Spellbook.CastSpell(IgniteSlot, target);
            }
        }

        private static void Farm(bool laneclear)
        {
            var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);
            var jungleMinions = MinionManager.GetMinions(
                ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral);
            minions.AddRange(jungleMinions);

            if (laneclear && Config.Item("wFarm").GetValue<bool>() && W.IsReady())
            {
                if (minions.Count > 0)
                {
                    W.Cast(W.GetLineFarmLocation(minions).Position.To3D());
                }
            }
            if (((!Config.Item("qFarm").GetValue<bool>() ||
                  !Orbwalker.ActiveMode.Equals(Orbwalking.OrbwalkingMode.LastHit)) &&
                 (!Config.Item("qFarmHarass").GetValue<bool>() ||
                  !Orbwalker.ActiveMode.Equals(Orbwalking.OrbwalkingMode.Mixed)) &&
                 !Orbwalker.ActiveMode.Equals(Orbwalking.OrbwalkingMode.LaneClear)) ||
                Config.Item("saveqStun").GetValue<bool>() && StunCount == 4 || !Q.IsReady())
            {
                return;
            }
            foreach (var minion in
                from minion in
                    minions.OrderByDescending(Minions => Minions.MaxHealth)
                        .Where(minion => minion.IsValidTarget(Q.Range))
                let predictedHealth = Q.GetHealthPrediction(minion)
                where
                    predictedHealth < ObjectManager.Player.GetSpellDamage(minion, SpellSlot.Q) * 0.85 &&
                    predictedHealth > 0
                select minion)
            {
                Q.CastOnUnit(minion, Config.Item("PCast").GetValue<bool>());
            }
        }

        private static int GetEnemiesInRange(Vector3 pos, float range)
        {
            //var Pos = pos;
            return
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(hero => hero.Team != ObjectManager.Player.Team)
                    .Count(hero => Vector3.Distance(pos, hero.ServerPosition) <= range);
        }
    }
}