using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;

using Color = System.Drawing.Color;

namespace Rekt_Sai
{
    public class Config
    {
        public const string MENU_NAME = "【超神汉化】HT挖掘机";
        public static MenuWrapper Menu { get; private set; }

        public static Dictionary<string, MenuWrapper.BoolLink> BoolLinks { get; private set; }
        public static Dictionary<string, MenuWrapper.CircleLink> CircleLinks { get; private set; }
        public static Dictionary<string, MenuWrapper.KeyBindLink> KeyLinks { get; private set; }
        public static Dictionary<string, MenuWrapper.SliderLink> SliderLinks { get; private set; }
        public static Dictionary<string, MenuWrapper.StringListLink> StringListLinks { get; private set; }

        private static void ProcessLink(string key, object value)
        {
            if (value is MenuWrapper.BoolLink)
                BoolLinks.Add(key, value as MenuWrapper.BoolLink);
            else if (value is MenuWrapper.CircleLink)
                CircleLinks.Add(key, value as MenuWrapper.CircleLink);
            else if (value is MenuWrapper.KeyBindLink)
                KeyLinks.Add(key, value as MenuWrapper.KeyBindLink);
            else if (value is MenuWrapper.SliderLink)
                SliderLinks.Add(key, value as MenuWrapper.SliderLink);
            else if (value is MenuWrapper.StringListLink)
                StringListLinks.Add(key, value as MenuWrapper.StringListLink);
        }

        static Config()
        {
            Menu = new MenuWrapper(MENU_NAME);

            BoolLinks = new Dictionary<string, MenuWrapper.BoolLink>();
            CircleLinks = new Dictionary<string, MenuWrapper.CircleLink>();
            KeyLinks = new Dictionary<string, MenuWrapper.KeyBindLink>();
            SliderLinks = new Dictionary<string, MenuWrapper.SliderLink>();
            StringListLinks = new Dictionary<string, MenuWrapper.StringListLink>();

            SetupMenu();
        }

        private static void SetupMenu()
        {
            // ----- Combo
            var subMenu = Menu.MainMenu.AddSubMenu("连招");
            ProcessLink("comboUseQ", subMenu.AddLinkedBool("使用Q"));
            ProcessLink("comboUseW", subMenu.AddLinkedBool("使用W"));
            ProcessLink("comboUseE", subMenu.AddLinkedBool("使用E"));
            ProcessLink("comboUseQBurrow", subMenu.AddLinkedBool("使用Q(遁地)"));
            ProcessLink("comboUseEBurrow", subMenu.AddLinkedBool("使用E(遁地)"));
            ProcessLink("comboUseItems", subMenu.AddLinkedBool("使用物品"));
            ProcessLink("comboUseIgnite", subMenu.AddLinkedBool("使用点燃"));
            ProcessLink("comboUseSmite", subMenu.AddLinkedBool("使用惩戒"));
            ProcessLink("comboActive", subMenu.AddLinkedKeyBind("热键", 32, KeyBindType.Press));

            // Harass
            subMenu = Menu.MainMenu.AddSubMenu("骚扰");
            ProcessLink("harassUseQ", subMenu.AddLinkedBool("使用Q"));
            ProcessLink("harassUseE", subMenu.AddLinkedBool("使用E"));
            ProcessLink("harassUseQBurrow", subMenu.AddLinkedBool("使用Q (遁地)"));
            ProcessLink("harassUseItems", subMenu.AddLinkedBool("使用物品"));
            ProcessLink("harassActive", subMenu.AddLinkedKeyBind("热键", 'C', KeyBindType.Press));

            // WaveClear
            subMenu = Menu.MainMenu.AddSubMenu("补兵");
            ProcessLink("waveUseQ", subMenu.AddLinkedBool("使用Q"));
            ProcessLink("waveNumQ", subMenu.AddLinkedSlider("小兵数量", 2, 1, 10));
            ProcessLink("waveUseE", subMenu.AddLinkedBool("使用E"));
            ProcessLink("waveUseQBurrow", subMenu.AddLinkedBool("使用Q(遁地)"));
            ProcessLink("waveUseItems", subMenu.AddLinkedBool("使用物品"));
            ProcessLink("waveActive", subMenu.AddLinkedKeyBind("热键", 'V', KeyBindType.Press));

            // ----- JungleClear
            subMenu = Menu.MainMenu.AddSubMenu("清野");
            ProcessLink("jungleUseQ", subMenu.AddLinkedBool("使用Q"));
            ProcessLink("jungleUseW", subMenu.AddLinkedBool("使用W"));
            ProcessLink("jungleUseE", subMenu.AddLinkedBool("使用E"));
            ProcessLink("jungleUseQBurrow", subMenu.AddLinkedBool("使用Q(遁地)"));
            ProcessLink("jungleUseItems", subMenu.AddLinkedBool("使用物品"));
            ProcessLink("jungleActive", subMenu.AddLinkedKeyBind("热键", 'V', KeyBindType.Press));

            // ----- Flee
            subMenu = Menu.MainMenu.AddSubMenu("逃跑");
            ProcessLink("fleeNothing", subMenu.AddLinkedBool("Nothing yet Kappa"));
            ProcessLink("fleeActive", subMenu.AddLinkedKeyBind("热键", 'T', KeyBindType.Press));

            // ----- Items
            subMenu = Menu.MainMenu.AddSubMenu("物品");
            ProcessLink("itemsTiamat", subMenu.AddLinkedBool("使用提亚马特"));
            ProcessLink("itemsHydra", subMenu.AddLinkedBool("使用九头蛇"));
            ProcessLink("itemsCutlass", subMenu.AddLinkedBool("使用小弯刀"));
            ProcessLink("itemsBotrk", subMenu.AddLinkedBool("使用破败"));
            ProcessLink("itemsRanduin", subMenu.AddLinkedBool("使用兰顿"));

            // Drawings
            subMenu = Menu.MainMenu.AddSubMenu("显示");
            ProcessLink("drawRangeQ", subMenu.AddLinkedCircle("Q范围(遁地)", true, Color.FromArgb(150, Color.IndianRed), SpellManager.QBurrowed.Range));
            ProcessLink("drawRangeE", subMenu.AddLinkedCircle("E范围(遁地)", true, Color.FromArgb(150, Color.Azure), SpellManager.EBurrowed.Range));
        }
    }
}
