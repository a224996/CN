using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Common.Data;

namespace FuckingAwesomeDraven
{       enum Spells
        {
            Q, W, E, R
        }
    class Program
    {
        
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        public static Dictionary<Spells, Spell> spells = new Dictionary<Spells, Spell>()
        {
            {Spells.Q, new Spell(SpellSlot.Q, 0)},  {Spells.W, new Spell(SpellSlot.W, 0)},  {Spells.E, new Spell(SpellSlot.E, 1100)},  {Spells.R, new Spell(SpellSlot.R, 20000)}, 
        };

        public static Orbwalking.Orbwalker Orbwalker;

        public static Menu Config;
        public static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        static void Game_OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != "Draven")
            {
                Notifications.AddNotification(new Notification("Not Draven? Draaaaaaaaaven.", 5));
                return;
            }

            Config = new Menu("【無為汉化】FA德莱文", "FuckingAwesomeDraven", true);

            Orbwalker = new Orbwalking.Orbwalker(Config.AddSubMenu(new Menu("走砍", "Orbwalking")));

            TargetSelector.AddToMenu(Config.AddSubMenu(new Menu("目标选择", "Target Selector")));

            var ComboMenu = Config.AddSubMenu(new Menu("连招", "Combo"));

            ComboMenu.AddItem(new MenuItem("UQC", "使用 Q").SetValue(true));
            ComboMenu.AddItem(new MenuItem("UWC", "使用 W").SetValue(true));
            ComboMenu.AddItem(new MenuItem("UEC", "使用 E").SetValue(true));
            ComboMenu.AddItem(new MenuItem("URC", "使用 R").SetValue(true));
            ComboMenu.AddItem(new MenuItem("URCM", "R 模式").SetValue(new StringList(new []{"超出范围击杀", "击杀(任何时间)"}, 0)));
            ComboMenu.AddItem(new MenuItem("forceR", "对目标使用 R").SetValue(new KeyBind('T', KeyBindType.Press)));

            var HarassMenu = Config.AddSubMenu(new Menu("骚扰", "Harass"));

            HarassMenu.AddItem(new MenuItem("UQH", "使用 Q").SetValue(true));
            HarassMenu.AddItem(new MenuItem("UWH", "使用 W").SetValue(true));
            HarassMenu.AddItem(new MenuItem("UEH", "使用 E").SetValue(true));

            var JungleMenu = Config.AddSubMenu(new Menu("发育", "MinionClear"));

            JungleMenu.AddItem(new MenuItem("sdfsdf", "清野"));
            JungleMenu.AddItem(new MenuItem("UQJ", "使用 Q").SetValue(true));
            JungleMenu.AddItem(new MenuItem("UWJ", "使用 W").SetValue(true));
            JungleMenu.AddItem(new MenuItem("UEJ", "使用 E").SetValue(true));
            JungleMenu.AddItem(new MenuItem("sdffdsdf", "清兵"));
            JungleMenu.AddItem(new MenuItem("UQWC", "使用 Q").SetValue(true));
            JungleMenu.AddItem(new MenuItem("WCM", "最小法力值 (%)").SetValue(new Slider(20, 0, 100)));

            // Axe Menu
            var axe = Config.AddSubMenu(new Menu("接斧头", "Axe Catching"));

            axe.AddItem(new MenuItem("catching", "启用接斧").SetValue(new KeyBind('M', KeyBindType.Toggle)));
            axe.AddItem(new MenuItem("useWCatch", "使用 W 接斧(智能)").SetValue(false));
            axe.AddItem(
                new MenuItem("catchRadiusMode", "接斧半径模式").SetValue(
                    new StringList(new[] {"鼠标模式", "扇形模式"})));
            axe.AddItem(new MenuItem("sectorAngle", "扇形范围").SetValue(new Slider(177, 1, 360)));
            axe.AddItem(new MenuItem("catchRadius", "接斧半径").SetValue(new Slider(600, 300, 1500)));
            axe.AddItem(new MenuItem("ignoreTowerReticle", "忽略塔").SetValue(true));
            axe.AddItem(new MenuItem("clickRemoveAxes", "点击卸下斧头").SetValue(true));

            Antispells.init();

            var draw = Config.AddSubMenu(new Menu("显示", "Draw"));
            draw.AddItem(new MenuItem("DABR", "禁用所有").SetValue(false));
            draw.AddItem(new MenuItem("DE", "显示 E 范围").SetValue(new Circle(false, System.Drawing.Color.White)));
            draw.AddItem(new MenuItem("DR", "显示 R 范围").SetValue(new Circle(false, System.Drawing.Color.White)));
            draw.AddItem(new MenuItem("DCS", "显示接斧状态").SetValue(new Circle(true, System.Drawing.Color.White)));
            draw.AddItem(new MenuItem("DCA", "显示当前斧头").SetValue(new Circle(false, System.Drawing.Color.White)));
            draw.AddItem(new MenuItem("DCR", "显示接斧半径").SetValue(new Circle(true, System.Drawing.Color.White)));
            draw.AddItem(new MenuItem("DAR", "显示接斧点").SetValue(new Circle(true, System.Drawing.Color.White)));
            draw.AddItem(new MenuItem("DKM", "显示可击杀的小兵").SetValue(new Circle(true, System.Drawing.Color.White)));

            var Info = Config.AddSubMenu(new Menu("信息", "info"));
            Info.AddItem(new MenuItem("Msddsds", "if you would like to donate via paypal"));
            Info.AddItem(new MenuItem("Msdsddsd", "you can do so by sending money to:"));
            Info.AddItem(new MenuItem("Msdsadfdsd", "jayyeditsdude@gmail.com"));

            Config.AddItem(new MenuItem("Mgdgdfgsd", "Version: 0.0.4-0"));
            Config.AddItem(new MenuItem("Msd", "Made By FluxySenpai"));

            Config.AddToMainMenu();

            Notifications.AddNotification(new Notification("Fucking Awesome Draven - Loaded", 5));
            Notifications.AddNotification("Who wants some Draven?", 5);

            spells[Spells.E].SetSkillshot(250f, 130f, 1400f, false, SkillshotType.SkillshotLine);
            spells[Spells.R].SetSkillshot(400f, 160f, 2000f, false, SkillshotType.SkillshotLine);

            Orbwalker.SetAttack(false);
            Orbwalker.SetMovement(false);

            GameObject.OnCreate += AxeCatcher.OnCreate;
            GameObject.OnDelete += AxeCatcher.OnDelete;
            Obj_AI_Hero.OnProcessSpellCast += AxeCatcher.Obj_AI_Hero_OnProcessSpellCast;
            Drawing.OnDraw += eventArgs => AxeCatcher.Draw();
            Game.OnGameUpdate += Game_OnGameUpdate;
            Game.OnWndProc += AxeCatcher.GameOnOnWndProc;
        }

        static void Game_OnGameUpdate(EventArgs args)
        {
            if (Config.Item("forceR").GetValue<KeyBind>().Active && TargetSelector.GetTarget(3000, TargetSelector.DamageType.Physical).IsValidTarget()) spells[Spells.R].Cast(TargetSelector.GetTarget(3000, TargetSelector.DamageType.Physical));

            AxeCatcher.catchAxes();

            switch (Orbwalker.ActiveMode)
            {
                    case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                    case Orbwalking.OrbwalkingMode.LaneClear:
                    WaveClear();
                    Jungle();
                    break;
                    case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;
            }
        }

        public static void Combo()
        {
            var Q = Config.Item("UQC").GetValue<bool>();
            var W = Config.Item("UWC").GetValue<bool>();
            var E = Config.Item("UEC").GetValue<bool>();
            var R = Config.Item("URC").GetValue<bool>();

            var t = AxeCatcher.GetTarget();
            if (!t.IsValidTarget() || !t.IsValid<Obj_AI_Hero>()) return;
            var Target = (Obj_AI_Hero) t;

            if (Target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player) + 200))
            {
                if (ItemData.Youmuus_Ghostblade.GetItem().IsReady())
                    ItemData.Youmuus_Ghostblade.GetItem().Cast();
            }

            if (Q && AxeCatcher.LastAa + 300 < Environment.TickCount && spells[Spells.Q].IsReady() &&
                AxeCatcher.AxeSpots.Count + AxeCatcher.CurrentAxes < 2 && Target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)))
            {
                spells[Spells.Q].Cast();
            }

            if (W && !ObjectManager.Player.HasBuff("dravenfurybuff", true) && !ObjectManager.Player.HasBuff("dravenfurybuff") &&
                spells[Spells.W].IsReady() && Target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)))
            {
                spells[Spells.W].Cast();
            }

            if (E && spells[Spells.E].IsReady() && AxeCatcher.CanMakeIt(500) && Target.IsValidTarget(spells[Spells.E].Range))
            {
                spells[Spells.E].Cast(Target);
            }

            var t2 = TargetSelector.GetTarget(3000, TargetSelector.DamageType.Physical);
            if (R && spells[Spells.R].IsReady() && t2.IsValidTarget(spells[Spells.R].Range))
            {
                switch (Config.Item("URCM").GetValue<StringList>().SelectedIndex)
                {
                    case 1:
                        if (getRCalc(t2)) spells[Spells.R].Cast(t2);
                        break;
                    case 0: 
                        if (getRCalc(t2) && t2.Distance(Player) > 800) spells[Spells.R].Cast(t2);
                        break;
                }
            }
        }

        public static void Harass()
        {
            var Q = Config.Item("UQH").GetValue<bool>();
            var W = Config.Item("UWH").GetValue<bool>();
            var E = Config.Item("UEH").GetValue<bool>();

            var t = AxeCatcher.GetTarget();
            if (!t.IsValidTarget() || !t.IsValid<Obj_AI_Hero>()) return;
            var Target = (Obj_AI_Hero) t;

            if (Q && AxeCatcher.LastAa + 300 < Environment.TickCount && spells[Spells.Q].IsReady() &&
                AxeCatcher.AxeSpots.Count + AxeCatcher.CurrentAxes < 2 && Target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)))
            {
                spells[Spells.Q].Cast();
            }
            if (W && !ObjectManager.Player.HasBuff("dravenfurybuff", true) && !ObjectManager.Player.HasBuff("dravenfurybuff") &&
                spells[Spells.W].IsReady() && Target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)))
            {
                spells[Spells.W].Cast();
            }
            if (E && spells[Spells.E].IsReady() && AxeCatcher.CanMakeIt(500) && Target.IsValidTarget(spells[Spells.E].Range))
            {
                spells[Spells.E].Cast(Target);
            }
        }

        public static void Jungle()
        {
            var Q = Config.Item("UQJ").GetValue<bool>();
            var W = Config.Item("UWJ").GetValue<bool>();
            var E = Config.Item("UEJ").GetValue<bool>();

            var Target = MinionManager.GetMinions(
                Player.Position, 700, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth).FirstOrDefault();

            if (Q && AxeCatcher.LastAa + 300 < Environment.TickCount && spells[Spells.Q].IsReady() &&
                AxeCatcher.AxeSpots.Count + AxeCatcher.CurrentAxes < 2 && Target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)))
            {
                spells[Spells.Q].Cast();
            }
            if (W && !ObjectManager.Player.HasBuff("dravenfurybuff", true) && !ObjectManager.Player.HasBuff("dravenfurybuff") &&
                spells[Spells.W].IsReady() && Target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)))
            {
                spells[Spells.W].Cast();
            }
            if (E && spells[Spells.E].IsReady() && AxeCatcher.CanMakeIt(500) && Target.IsValidTarget(spells[Spells.E].Range))
            {
                spells[Spells.E].Cast(Target);
            }
        }

        public static void WaveClear()
        {
            var Q = Config.Item("UQWC").GetValue<bool>();
            var Target = MinionManager.GetMinions(
                Player.Position, 700, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth).FirstOrDefault(a => !a.Name.ToLower().Contains("ward"));
            if (Config.Item("WCM").GetValue<Slider>().Value > (Player.Mana / Player.MaxMana * 100))
                return;
            if (Q && AxeCatcher.LastAa + 300 < Environment.TickCount && spells[Spells.Q].IsReady() &&
                AxeCatcher.AxeSpots.Count + AxeCatcher.CurrentAxes < 2 && Target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)))
            {
                spells[Spells.Q].Cast();
            }
        }

        public static bool getRCalc(Obj_AI_Hero target)
        {
            return false;
            int totalUnits =
                spells[Spells.R].GetPrediction(target)
                    .CollisionObjects.Count(a => a.IsValidTarget());
            float distance = ObjectManager.Player.Distance(target);
            var damageReduction = ((totalUnits > 7)) ? 0.4 : (totalUnits == 0) ? 1.0 : (1 - (((totalUnits) * 8)/100));
            return spells[Spells.R].GetDamage(target) * damageReduction >= (target.Health + (distance / 2000) * target.HPRegenRate);
        }
    }
}
