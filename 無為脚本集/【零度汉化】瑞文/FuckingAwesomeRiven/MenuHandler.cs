using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp.Common;
using SharpDX;

namespace FuckingAwesomeRiven
{
    class MenuHandler
    {

        public static Orbwalking.Orbwalker Orbwalker;
        public static Menu Config;

        public static void initMenu()
        {
            Config = new Menu("【零度汉化】瑞文", "KappaChino", true);

            Orbwalker = new Orbwalking.Orbwalker(Config.AddSubMenu(new Menu("走砍", "OW")));
            TargetSelector.AddToMenu(Config.AddSubMenu(new Menu("目标选择器", "Target Selector")));

            var combo = Config.AddSubMenu(new Menu("连招", "Combo"));
            combo.AddItem(new MenuItem("xdxdxdxd", "-- 正常的连招"));

            var enabledCombos = combo.AddSubMenu(new Menu("击杀连招", "Killable Combos"));
            enabledCombos.AddItem(new MenuItem("dfsjknfsj", "随着 R2"));
            enabledCombos.AddItem(new MenuItem("QWR2KS", "Q - W - R2").SetValue(true));
            enabledCombos.AddItem(new MenuItem("QR2KS", "Q - R2").SetValue(true));
            enabledCombos.AddItem(new MenuItem("WR2KS", "W - R2").SetValue(true));
            enabledCombos.AddItem(new MenuItem("dfdgdgdfggfdsj", ""));
            enabledCombos.AddItem(new MenuItem("dfgdhnfdsf", "没有 R2"));
            enabledCombos.AddItem(new MenuItem("QWKS", "Q - W").SetValue(true));
            enabledCombos.AddItem(new MenuItem("QKS", "Q").SetValue(true));
            enabledCombos.AddItem(new MenuItem("WKS", "W").SetValue(true));

            combo.AddItem(new MenuItem("CQ", "使用 Q").SetValue(true));
            combo.AddItem(new MenuItem("UseQ-GC", "使用 Q 防突进").SetValue(true));
            combo.AddItem(new MenuItem("使用 R2", "使用 R2").SetValue(true));
            combo.AddItem(new MenuItem("CW", "使用 W").SetValue(true));
            combo.AddItem(new MenuItem("CE", "使用 E").SetValue(true));
            combo.AddItem(new MenuItem("UseE-AA", "只有超出AA范围使用E").SetValue(true));
            combo.AddItem(new MenuItem("UseE-GC", "使用 E 防突进").SetValue(true));
            combo.AddItem(new MenuItem("CR", "使用 R [很快]").SetValue(true));
            combo.AddItem(new MenuItem("CR2", "使用 R2").SetValue(true));
            combo.AddItem(new MenuItem("magnet", "吸引目标").SetValue(false));
            combo.AddItem(new MenuItem("bdsfdfffsf", ""));
            combo.AddItem(new MenuItem("bdsfdsf", "-- 突发连招"));
            combo.AddItem(new MenuItem("BFl", "使用闪现").SetValue(false));
            combo.AddItem(new MenuItem("bdsfdsff", "E - R - 闪现 - W - Q"));
            combo.AddItem(new MenuItem("bdsfdsfddd", "E - R - W - Q"));

            var farm = Config.AddSubMenu(new Menu("发育", "Farming"));
            farm.AddItem(new MenuItem("fnjdsjkn", "          补兵"));
            farm.AddItem(new MenuItem("QLH", "使用 Q").SetValue(true));
            farm.AddItem(new MenuItem("WLH", "使用 W").SetValue(true));
            farm.AddItem(new MenuItem("10010321223", "          清野"));
            farm.AddItem(new MenuItem("QJ", "使用 Q").SetValue(true));
            farm.AddItem(new MenuItem("WJ", "使用 W").SetValue(true));
            farm.AddItem(new MenuItem("EJ", "使用 E").SetValue(true));
            farm.AddItem(new MenuItem("5622546001", "          清兵"));
            farm.AddItem(new MenuItem("QWC", "使用 Q").SetValue(true));
            farm.AddItem(new MenuItem("QWC-LH", "Q 最后一击").SetValue(true));
            farm.AddItem(new MenuItem("QWC-AA", "Q -> AA").SetValue(true));
            farm.AddItem(new MenuItem("WWC", "使用 W").SetValue(true));


            var draw = Config.AddSubMenu(new Menu("显示", "Draw"));

            draw.AddItem(new MenuItem("DQ", "显示 Q 范围").SetValue(new Circle(false, System.Drawing.Color.White)));
            draw.AddItem(new MenuItem("DW", "显示 W 范围").SetValue(new Circle(false, System.Drawing.Color.White)));
            draw.AddItem(new MenuItem("DE", "显示 E 范围").SetValue(new Circle(false, System.Drawing.Color.White)));
            draw.AddItem(new MenuItem("DR", "显示 R 范围").SetValue(new Circle(false, System.Drawing.Color.White)));
            draw.AddItem(new MenuItem("DD", "显示损伤 [很快]").SetValue(new Circle(false, System.Drawing.Color.White)));

            var misc = Config.AddSubMenu(new Menu("杂项", "Misc"));
            misc.AddItem(new MenuItem("keepQAlive", "保持 Q ").SetValue(true));
            misc.AddItem(new MenuItem("QFlee", "Q 逃离").SetValue(true));
            misc.AddItem(new MenuItem("EFlee", "E 逃离").SetValue(true));

            var Keybindings = Config.AddSubMenu(new Menu("按键设置", "KB"));
            Keybindings.AddItem(new MenuItem("normalCombo", "正常连招").SetValue(new KeyBind(32, KeyBindType.Press)));
            Keybindings.AddItem(new MenuItem("burstCombo", "爆发连招").SetValue(new KeyBind('M', KeyBindType.Press)));
            Keybindings.AddItem(new MenuItem("jungleCombo", "清野").SetValue(new KeyBind('V', KeyBindType.Press)));
            Keybindings.AddItem(new MenuItem("waveClear", "清兵").SetValue(new KeyBind('V', KeyBindType.Press)));
            Keybindings.AddItem(new MenuItem("lastHit", "补兵").SetValue(new KeyBind('X', KeyBindType.Press)));
            Keybindings.AddItem(new MenuItem("flee", "逃跑").SetValue(new KeyBind('Z', KeyBindType.Press)));

            var Info = Config.AddSubMenu(new Menu("信息", "info"));
            Info.AddItem(new MenuItem("Msddsds", "如果你想捐赠通过PayPal"));
            Info.AddItem(new MenuItem("Msdsddsd", "你也可以转账:"));
            Info.AddItem(new MenuItem("Msdsadfdsd", "jayyeditsdude@gmail.com"));
            Info.AddItem(new MenuItem("debug", "调试模式")).SetValue(false);

            Config.AddItem(new MenuItem("Mgdgdfgsd", "版本: 0.0.1 测试"));
            Config.AddItem(new MenuItem("Msd", "作者:Fluxy "));
			Config.AddItem(new MenuItem("wuwei", "汉化:無為 "));
			Config.AddItem(new MenuItem("qun", "汉化群:386289593 "));


            Config.AddToMainMenu();
        }

        public static bool getMenuBool(String s)
        {
            return Config.Item(s).GetValue<bool>();
        }
    }
}
