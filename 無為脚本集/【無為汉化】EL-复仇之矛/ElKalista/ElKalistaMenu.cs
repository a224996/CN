using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using System.Drawing;


namespace ElKalista
{

    public class ElKalistaMenu
    {
        public static Menu _menu;

        public static void Initialize()
        {
            _menu = new Menu("【無為汉化】EL-复仇之矛", "menu", true);

            //ElKalista.Orbwalker
            var orbwalkerMenu = new Menu("走砍", "orbwalker");
            Kalista.Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            _menu.AddSubMenu(orbwalkerMenu);

            //ElKalista.TargetSelector
            var targetSelector = new Menu("目标选择", "TargetSelector");
            TargetSelector.AddToMenu(targetSelector);
            _menu.AddSubMenu(targetSelector);

            var cMenu = new Menu("连招", "Combo");
            cMenu.AddItem(new MenuItem("ElKalista.Combo.Q", "使用 Q").SetValue(true));
            cMenu.AddItem(new MenuItem("ElKalista.Combo.E", "使用 E").SetValue(true));
            cMenu.AddItem(new MenuItem("ElKalista.Combo.R", "使用 R").SetValue(true));
            cMenu.AddItem(new MenuItem("ElKalista.sssssssss", ""));
            cMenu.AddItem(new MenuItem("ElKalista.ComboE.Auto", "使用堆叠 E").SetValue(true));
            cMenu.AddItem(new MenuItem("ElKalista.ssssddsdssssss", ""));

            cMenu.AddItem(new MenuItem("ElKalista.hitChance", "Q 命中率").SetValue(new StringList(new[] { "低", "正常", "高", "很高" }, 3)));
            cMenu.AddItem(new MenuItem("ElKalista.SemiR", "半手动 R").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
            cMenu.AddItem(new MenuItem("ComboActive", "连招!").SetValue(new KeyBind(32, KeyBindType.Press)));
            _menu.AddSubMenu(cMenu);

            var hMenu = new Menu("骚扰", "Harass");
            hMenu.AddItem(new MenuItem("ElKalista.Harass.Q", "使用 Q").SetValue(true));
            hMenu.AddItem(new MenuItem("ElKalista.minmanaharass", "蓝量 ")).SetValue(new Slider(55));
            hMenu.AddItem(new MenuItem("ElKalista.hitChance", "Q 命中率").SetValue(new StringList(new[] { "低", "正常", "高", "很高" }, 3)));

            hMenu.SubMenu("AutoHarass").AddItem(new MenuItem("ElKalista.AutoHarass", "[锁定]自动骚扰", false).SetValue(new KeyBind("U".ToCharArray()[0], KeyBindType.Toggle)));
            hMenu.SubMenu("AutoHarass").AddItem(new MenuItem("ElKalista.UseQAutoHarass", "使用 Q").SetValue(true));
            hMenu.SubMenu("AutoHarass").AddItem(new MenuItem("ElKalista.harass.mana", "蓝量")).SetValue(new Slider(55));

            _menu.AddSubMenu(hMenu);

            var lMenu = new Menu("清兵", "Clear");
            lMenu.AddItem(new MenuItem("useQFarm", "使用 Q").SetValue(true));
            lMenu.AddItem(new MenuItem("ElKalista.Count.Minions", "小兵数量 Q >=").SetValue(new Slider(2, 1, 5)));
            lMenu.AddItem(new MenuItem("useEFarm", "使用 E").SetValue(true));
            lMenu.AddItem(new MenuItem("ElKalista.Count.Minions.E", "小兵数量 E >=").SetValue(new Slider(2, 1, 5)));
            lMenu.AddItem(new MenuItem("useEFarmddsddaadsd", ""));
            lMenu.AddItem(new MenuItem("useQFarmJungle", "使用 Q 清野").SetValue(true));
            lMenu.AddItem(new MenuItem("useEFarmJungle", "使用 E 清野").SetValue(true));
            lMenu.AddItem(new MenuItem("useEFarmddssd", ""));
            lMenu.AddItem(new MenuItem("minmanaclear", "蓝量 ")).SetValue(new Slider(55));

            _menu.AddSubMenu(lMenu);


            var itemMenu = new Menu("物品", "Items");
            itemMenu.AddItem(new MenuItem("ElKalista.Items.Youmuu", "使用幽梦").SetValue(true));
            itemMenu.AddItem(new MenuItem("ElKalista.Items.Cutlass", "使用弯刀").SetValue(true));
            itemMenu.AddItem(new MenuItem("ElKalista.Items.Blade", "使用破败").SetValue(true));
            itemMenu.AddItem(new MenuItem("ElKalista.Harasssfsddass.E", ""));
            itemMenu.AddItem(new MenuItem("ElKalista.Items.Blade.EnemyEHP", "敌人血量百分比").SetValue(new Slider(80, 100, 0)));
            itemMenu.AddItem(new MenuItem("ElKalista.Items.Blade.EnemyMHP", "我的血量百分比").SetValue(new Slider(80, 100, 0)));
            _menu.AddSubMenu(itemMenu);


            var setMenu = new Menu("Secret Settings", "SSS");
            setMenu.AddItem(new MenuItem("ElKalista.misc.save", "R 拯救队友").SetValue(true));
            setMenu.AddItem(new MenuItem("ElKalista.misc.allyhp", "队友血量百分比").SetValue(new Slider(25, 100, 0)));
            setMenu.AddItem(new MenuItem("useEFarmddsddsasfsasdsdsaadsd", ""));
            setMenu.AddItem(new MenuItem("ElKalista.E.Auto", "自动使用E").SetValue(true));
            setMenu.AddItem(new MenuItem("ElKalista.E.Stacks", "E 叠层数 >=").SetValue(new Slider(10, 1, 20)));
            setMenu.AddItem(new MenuItem("useEFafsdsgdrmddsddsasfsasdsdsaadsd", ""));
            setMenu.AddItem(new MenuItem("ElKalista.misc.ks", "抢人头模式").SetValue(false));
            setMenu.AddItem(new MenuItem("ElKalista.misc.junglesteal", "偷野模式").SetValue(true));

            _menu.AddSubMenu(setMenu);

            //ElKalista.Misc
            var miscMenu = new Menu("杂项", "Misc");
            miscMenu.AddItem(new MenuItem("ElKalista.Draw.off", "关闭全部").SetValue(false));
            miscMenu.AddItem(new MenuItem("ElKalista.Draw.Q", "显示 Q").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElKalista.Draw.W", "显示 W").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElKalista.Draw.E", "显示 E").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElKalista.Draw.R", "显示 R").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("ElKalista.Draw.Text", "显示文本").SetValue(true));

            var dmgAfterE = new MenuItem("ElKalista.DrawComboDamage", "显示 E 伤害").SetValue(true);
            var drawFill = new MenuItem("ElKalista.DrawColour", "颜色", true).SetValue(new Circle(true, Color.FromArgb(204, 204, 0, 0)));
            miscMenu.AddItem(drawFill);
            miscMenu.AddItem(dmgAfterE);



            EDamage.DamageToUnit = Kalista.GetComboDamage;
            EDamage.Enabled = dmgAfterE.GetValue<bool>();
            EDamage.Fill = drawFill.GetValue<Circle>().Active;
            EDamage.FillColor = drawFill.GetValue<Circle>().Color;

            dmgAfterE.ValueChanged += delegate (object sender, OnValueChangeEventArgs eventArgs)
            {
                EDamage.Enabled = eventArgs.GetNewValue<bool>();
            };

            drawFill.ValueChanged += delegate (object sender, OnValueChangeEventArgs eventArgs)
            {
                EDamage.Fill = eventArgs.GetNewValue<Circle>().Active;
                EDamage.FillColor = eventArgs.GetNewValue<Circle>().Color;
            };

            _menu.AddSubMenu(miscMenu);

            //Here comes the moneyyy, money, money, moneyyyy
            var credits = new Menu("信息", "jQuery");
            credits.AddItem(new MenuItem("ElKalista.Paypal", "如果你想捐赠通过PayPal:"));
            credits.AddItem(new MenuItem("ElKalista.Email", "info@zavox.nl"));
            _menu.AddSubMenu(credits);

            _menu.AddItem(new MenuItem("422442fsaafs4242f", ""));
            _menu.AddItem(new MenuItem("422442fsaafsf", "版本: 1.0.1.6"));
            _menu.AddItem(new MenuItem("fsasfafsfsafsa", "作者 jQuery"));

            _menu.AddToMainMenu();

            Console.WriteLine("Menu Loaded");
        }
    }
}