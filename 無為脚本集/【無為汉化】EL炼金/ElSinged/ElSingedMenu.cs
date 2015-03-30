using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using System.Drawing;


namespace ElSinged
{

    public class ElSingedMenu
    {
        public static Menu _menu;

        public static void Initialize()
        {
            _menu = new Menu("【無為汉化】EL炼金", "menu", true);

            //ElSinged.Orbwalker
            var orbwalkerMenu = new Menu("走砍", "orbwalker");
            Singed._orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            _menu.AddSubMenu(orbwalkerMenu);

            //ElSinged.TargetSelector
            var targetSelector = new Menu("目标选择", "TargetSelector");
            TargetSelector.AddToMenu(targetSelector);
            _menu.AddSubMenu(targetSelector);

            var cMenu = new Menu("连招", "Combo");
            cMenu.AddItem(new MenuItem("ElSinged.Combo.Q", "使用 Q").SetValue(true));
            cMenu.AddItem(new MenuItem("ElSinged.Combo.W", "使用 W").SetValue(true));
            cMenu.AddItem(new MenuItem("ElSinged.Combo.E", "使用 E").SetValue(true));
            cMenu.AddItem(new MenuItem("ElSinged.Combo.R", "使用 R").SetValue(true));
            cMenu.AddItem(new MenuItem("exploit", "快速开关Q[危险]").SetValue(false));
            cMenu.AddItem(new MenuItem("delayms", "延迟").SetValue(new Slider(150, 0, 1000)));
            cMenu.AddItem(new MenuItem("ElSinged.Coffasfsafsambo.R", ""));
            cMenu.AddItem(new MenuItem("ElSinged.Combo.R.Count", "使用 R 敌人数 >= ")).SetValue(new Slider(2, 1, 5));
            cMenu.AddItem(new MenuItem("ElSinged.Combo.Ignite", "使用点燃").SetValue(true));
            cMenu.AddItem(new MenuItem("ElSinged.hitChance", "W 命中率").SetValue(new StringList(new[] { "低", "正常", "高", "很高" }, 3)));
            cMenu.AddItem(new MenuItem("ComboActive", "连招!").SetValue(new KeyBind(32, KeyBindType.Press)));
            _menu.AddSubMenu(cMenu);

            var hMenu = new Menu("骚扰", "Harass");
            hMenu.AddItem(new MenuItem("ElSinged.Harass.Q", "使用 Q").SetValue(true));
            hMenu.AddItem(new MenuItem("ElSinged.Harass.W", "使用 W").SetValue(true));
            hMenu.AddItem(new MenuItem("ElSinged.Harass.E", "使用 E").SetValue(true));
            hMenu.AddItem(new MenuItem("ElSinged.hitChance", "W 命中率").SetValue(new StringList(new[] { "低", "正常", "高", "很高" }, 3)));
            _menu.AddSubMenu(hMenu);

            var lcMenu = new Menu("发育", "Laneclear");
            lcMenu.AddItem(new MenuItem("ElSinged.Laneclear.Q", "使用 Q").SetValue(true));
            lcMenu.AddItem(new MenuItem("ElSinged.Laneclear.E", "使用 E").SetValue(true));
            _menu.AddSubMenu(lcMenu);

            //ElSinged.Misc
            var miscMenu = new Menu("显示", "Misc");
            miscMenu.AddItem(new MenuItem("ElSinged.Draw.off", "全部关闭").SetValue(false));
            miscMenu.AddItem(new MenuItem("ElSinged.Draw.Q", "显示 Q").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElSinged.Draw.W", "显示 W").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElSinged.Draw.E", "显示 E").SetValue(new Circle()));
            _menu.AddSubMenu(miscMenu);

            //Here comes the moneyyy, money, money, moneyyyy
            var credits = new Menu("信息", "jQuery");
            credits.AddItem(new MenuItem("ElSinged.Paypal", "if you would like to donate via paypal:"));
            credits.AddItem(new MenuItem("ElSinged.Email", "info@zavox.nl"));
            _menu.AddSubMenu(credits);

            _menu.AddItem(new MenuItem("422442fsaafs4242f", ""));
            _menu.AddItem(new MenuItem("422442fsaafsf", "Version: 1.0.0.3"));
            _menu.AddItem(new MenuItem("fsasfafsfsafsa", "Made By jQuery"));

            _menu.AddToMainMenu();

            Console.WriteLine("Menu Loaded");
        }
    }
}