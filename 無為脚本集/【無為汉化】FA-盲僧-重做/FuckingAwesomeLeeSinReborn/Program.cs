using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace FuckingAwesomeLeeSinReborn
{
    class Program
    {
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameStart;
        }

        public static Orbwalking.Orbwalker Orbwalker;
        public static Menu Config;

        static void Game_OnGameStart(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != "LeeSin")
            {
                Notifications.AddNotification(new Notification("not lee sin huh? wanna go m9?", 2));
                return;
            }
            Config = new Menu("【無為汉化】FA-盲僧", "you-stealing-me-src-m9?", true);

            Orbwalker = new Orbwalking.Orbwalker(Config.AddSubMenu(new Menu("走砍", "Orbwalker")));
            TargetSelector.AddToMenu(Config.AddSubMenu(new Menu("目标选择", "Target Selector")));

            var combo = Config.AddSubMenu(new Menu("连招", "Combo"));
            combo.AddItem(new MenuItem("CQ", "使用Q").SetValue(true));
            combo.AddItem(new MenuItem("smiteQ", "惩戒再Q").SetValue(false));
            combo.AddItem(new MenuItem("CE", "使用E").SetValue(true));
            combo.AddItem(new MenuItem("CR", "使用R (击杀)").SetValue(false));
            combo.AddItem(new MenuItem("CpassiveCheck", "等待被动").SetValue(false));
            combo.AddItem(new MenuItem("CpassiveCheckCount", "最小堆叠").SetValue(new Slider(1,1,2)));
            combo.AddItem(new MenuItem("starCombo", "连招").SetValue(new KeyBind('T', KeyBindType.Press)));
            combo.AddItem(new MenuItem("starsadasCombo", "Q -> 跳眼 -> W -> R -> Q2"));
            
            var harass = Config.AddSubMenu(new Menu("骚扰", "Harass"));
            harass.AddItem(new MenuItem("HQ", "使用Q").SetValue(true));
            harass.AddItem(new MenuItem("HE", "使用E").SetValue(false));
            harass.AddItem(new MenuItem("HpassiveCheck", "等待被动").SetValue(false));
            harass.AddItem(new MenuItem("HpassiveCheckCount", "最小堆叠").SetValue(new Slider(1, 1, 2)));

            var insec = Config.AddSubMenu(new Menu("大招设置", "Insec"));
            insec.AddItem(new MenuItem("insecOrbwalk", "走砍").SetValue(true));
            insec.AddItem(new MenuItem("clickInsec", "点击R").SetValue(true));
            insec.AddItem(new MenuItem("sdgdsgsg", "点击敌人然后点击队友"));
            insec.AddItem(new MenuItem("ddfhdhdg", "塔/小兵/英雄"));
            insec.AddItem(new MenuItem("mouseInsec", "R使用pos").SetValue(false));
            insec.AddItem(new MenuItem("easyInsec", "简易R").SetValue(true));
            insec.AddItem(new MenuItem("sdgdsgsdfdssg", "点击敌人然后移动鼠标"));
            insec.AddItem(new MenuItem("ddfhdffdsdfdhdg", "(它会走到R的目标)"));
            insec.AddItem(new MenuItem("q2InsecRange", "使用Q2 如果有队友在范围内(所有)").SetValue(true));
            insec.AddItem(new MenuItem("q1InsecRange", "使用Q1 我们单位在中心距离").SetValue(false));
            insec.AddItem(new MenuItem("flashInsec", "闪现跳眼R").SetValue(false));
            insec.AddItem(new MenuItem("insec", "启用").SetValue(new KeyBind('Y', KeyBindType.Press)));

            var autoSmite = Config.AddSubMenu(new Menu("自动惩戒", "Auto Smite"));
            autoSmite.AddItem(new MenuItem("smiteEnabled", "启用").SetValue(new KeyBind("M".ToCharArray()[0], KeyBindType.Toggle)));
            autoSmite.AddItem(new MenuItem("SRU_Red", "红BUFF").SetValue(true));
            autoSmite.AddItem(new MenuItem("SRU_Blue", "蓝Buff").SetValue(true));
            autoSmite.AddItem(new MenuItem("SRU_Dragon", "小龙").SetValue(true));
            autoSmite.AddItem(new MenuItem("SRU_Baron", "大龙").SetValue(true));

            var farm = Config.AddSubMenu(new Menu("发育设置", "Farming"));
            farm.AddItem(new MenuItem("10010321223", "          清野"));
            farm.AddItem(new MenuItem("QJ", "使用Q").SetValue(true));
            farm.AddItem(new MenuItem("WJ", "使用W").SetValue(true));
            farm.AddItem(new MenuItem("EJ", "使用E").SetValue(true));
            farm.AddItem(new MenuItem("5622546001", "           清兵"));
            farm.AddItem(new MenuItem("QWC", "使用Q").SetValue(true));
            farm.AddItem(new MenuItem("EWC", "使用E").SetValue(true));

            var draw = Config.AddSubMenu(new Menu("显示设置", "Draw"));
            draw.AddItem(new MenuItem("LowFPS", "简易线圈").SetValue(false));
            draw.AddItem(new MenuItem("LowFPSMode", "低FPS设置").SetValue(new StringList(new []{"高", "中", "低"}, 2)));
            draw.AddItem(new MenuItem("DQ", "显示 Q 范围").SetValue(new Circle(false, Color.White)));
            draw.AddItem(new MenuItem("DW", "显示 W 范围").SetValue(new Circle(false, Color.White)));
            draw.AddItem(new MenuItem("DE", "显示 E 范围").SetValue(new Circle(false, Color.White)));
            draw.AddItem(new MenuItem("DR", "显示 R 范围").SetValue(new Circle(false, Color.White)));
            draw.AddItem(new MenuItem("DS", "显示惩戒范围").SetValue(new Circle(false, Color.PowderBlue)));
            draw.AddItem(new MenuItem("DWJ", "显示跳眼").SetValue(true));
            draw.AddItem(new MenuItem("DES", "显示逃脱点").SetValue(true));

            var escape = Config.AddSubMenu(new Menu("逃跑设置", "Escape Settings"));
            escape.AddItem(new MenuItem("escapeMode", "利用野怪逃跑").SetValue(true));
            escape.AddItem(new MenuItem("Wardjump", "插眼逃跑").SetValue(new KeyBind('Z', KeyBindType.Press)));
            escape.AddItem(new MenuItem("alwaysJumpMaxRange", "总是跳最大范围").SetValue(true));
            escape.AddItem(new MenuItem("jumpChampions", "跳队友").SetValue(true));
            escape.AddItem(new MenuItem("jumpMinions", "跳小兵").SetValue(true));
            escape.AddItem(new MenuItem("jumpWards", "跳眼").SetValue(true));

            var info = Config.AddSubMenu(new Menu("信息", "info"));
            info.AddItem(new MenuItem("Msddsds", "if you would like to donate via paypal"));
            info.AddItem(new MenuItem("Msdsddsd", "you can do so by sending money to:"));
            info.AddItem(new MenuItem("Msdsadfdsd", "jayyeditsdude@gmail.com"));

            Config.AddItem(new MenuItem("Mgdgdfgsd", "Version: 0.0.1-5 BETA"));
            Config.AddItem(new MenuItem("Msd", "Made By FluxySenpai"));


            Config.AddToMainMenu();
            
            CheckHandler.spells[SpellSlot.Q].SetSkillshot(0.25f, 65f, 1800f, true, SkillshotType.SkillshotLine);

            CheckHandler.Init();
            JumpHandler.Load();
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnWndProc += InsecHandler.OnClick;
            AutoSmite.Init();
            Obj_AI_Base.OnProcessSpellCast += CheckHandler.Obj_AI_Hero_OnProcessSpellCast;
            Notifications.AddNotification(new Notification("Fucking Awesome Lee Sin:", 2));
            Notifications.AddNotification(new Notification("REBORN", 2));
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            var lowFps = Config.Item("LowFPS").GetValue<bool>();
            var lowFpsMode = Config.Item("LowFPSMode").GetValue<StringList>().SelectedIndex + 1;
            if (Config.Item("DQ").GetValue<Circle>().Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, CheckHandler.spells[SpellSlot.Q].Range, Config.Item("DQ").GetValue<Circle>().Color, lowFps ? lowFpsMode : 5);
            }
            if (Config.Item("DW").GetValue<Circle>().Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, CheckHandler.spells[SpellSlot.W].Range, Config.Item("DW").GetValue<Circle>().Color , lowFps ? lowFpsMode : 5);
            }
            if (Config.Item("DE").GetValue<Circle>().Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, CheckHandler.spells[SpellSlot.E].Range, Config.Item("DE").GetValue<Circle>().Color , lowFps ? lowFpsMode : 5);
            }
            if (Config.Item("DR").GetValue<Circle>().Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, CheckHandler.spells[SpellSlot.R].Range, Config.Item("DR").GetValue<Circle>().Color , lowFps ? lowFpsMode : 5);
            }
            WardjumpHandler.Draw();
            InsecHandler.Draw();
        }

        static void Game_OnGameUpdate(EventArgs args)
        {
            if (CheckHandler.LastSpell + 3000 <= Environment.TickCount)
            {
                CheckHandler.PassiveStacks = 0;
            }
            if (Config.Item("starCombo").GetValue<KeyBind>().Active)
            {
                StateHandler.StarCombo();
                return;
            }
            if (Config.Item("insec").GetValue<KeyBind>().Active)
            {
                InsecHandler.DoInsec();
                return;
            }
                InsecHandler.FlashPos = new Vector3();
                InsecHandler.FlashR = false;

            if (Config.Item("Wardjump").GetValue<KeyBind>().Active)
            {
                WardjumpHandler.DrawEnabled = Config.Item("DWJ").GetValue<bool>();
                WardjumpHandler.Jump(Game.CursorPos, Config.Item("alwaysJumpMaxRange").GetValue<bool>(), true);
                return;
            }
            WardjumpHandler.DrawEnabled = false;

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                StateHandler.Combo();
                return;
                case Orbwalking.OrbwalkingMode.LaneClear:
                StateHandler.JungleClear();
                return;
                case Orbwalking.OrbwalkingMode.Mixed:
                StateHandler.Harass();
                return;
            }
        }
    }
}
 