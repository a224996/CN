using System.Collections.Generic;

using LeagueSharp.Common;

using Color = System.Drawing.Color;

namespace Kalista
{
    public class Config
    {
        private static bool initialized = false;
        private const string MENU_TITLE = "【無爲汉化】H-复仇之矛 ";

        private static MenuWrapper _menu;

        private static Dictionary<string, MenuWrapper.BoolLink> _boolLinks = new Dictionary<string, MenuWrapper.BoolLink>();
        private static Dictionary<string, MenuWrapper.CircleLink> _circleLinks = new Dictionary<string, MenuWrapper.CircleLink>();
        private static Dictionary<string, MenuWrapper.KeyBindLink> _keyLinks = new Dictionary<string, MenuWrapper.KeyBindLink>();
        private static Dictionary<string, MenuWrapper.SliderLink> _sliderLinks = new Dictionary<string, MenuWrapper.SliderLink>();

        public static MenuWrapper Menu { get { return _menu; } }

        public static Dictionary<string, MenuWrapper.BoolLink> BoolLinks { get { return _boolLinks; } }
        public static Dictionary<string, MenuWrapper.CircleLink> CircleLinks { get { return _circleLinks; } }
        public static Dictionary<string, MenuWrapper.KeyBindLink> KeyLinks { get { return _keyLinks; } }
        public static Dictionary<string, MenuWrapper.SliderLink> SliderLinks { get { return _sliderLinks; } }

        private static void ProcessLink(string key, object value)
        {
            if (value is MenuWrapper.BoolLink)
                _boolLinks.Add(key, value as MenuWrapper.BoolLink);
            else if (value is MenuWrapper.CircleLink)
                _circleLinks.Add(key, value as MenuWrapper.CircleLink);
            else if (value is MenuWrapper.KeyBindLink)
                _keyLinks.Add(key, value as MenuWrapper.KeyBindLink);
            else if (value is MenuWrapper.SliderLink)
                _sliderLinks.Add(key, value as MenuWrapper.SliderLink);
        }

        static Config()
        {
            // Create menu
            _menu = new MenuWrapper(MENU_TITLE);

            // Combo
            var subMenu = _menu.MainMenu.AddSubMenu("连招");
            ProcessLink("comboUseQ", subMenu.AddLinkedBool("使用 Q"));
            ProcessLink("comboUseE", subMenu.AddLinkedBool("使用 E"));
            ProcessLink("comboNumE", subMenu.AddLinkedSlider("堆叠 E 层数", 5, 1, 20));
            ProcessLink("comboUseItems", subMenu.AddLinkedBool("使用物品"));
            ProcessLink("comboUseIgnite", subMenu.AddLinkedBool("使用点燃"));
            ProcessLink("comboActive", subMenu.AddLinkedKeyBind("连招！", 32, KeyBindType.Press));

            // Harass
            subMenu = _menu.MainMenu.AddSubMenu("骚扰");
            ProcessLink("harassUseQ", subMenu.AddLinkedBool("使用 Q"));
            ProcessLink("harassMana", subMenu.AddLinkedSlider("最低蓝量(%)", 30));
            ProcessLink("harassActive", subMenu.AddLinkedKeyBind("骚扰！", 'T', KeyBindType.Press));

            // WaveClear
            subMenu = _menu.MainMenu.AddSubMenu("清线");
            ProcessLink("waveUseQ", subMenu.AddLinkedBool("使用 Q"));
            ProcessLink("waveNumQ", subMenu.AddLinkedSlider("小兵数量", 3, 1, 10));
            ProcessLink("waveUseE", subMenu.AddLinkedBool("使用 E"));
            ProcessLink("waveNumE", subMenu.AddLinkedSlider("小兵数量", 2, 1, 10));
            ProcessLink("waveMana", subMenu.AddLinkedSlider("最低蓝量(%)", 30));
            ProcessLink("waveActive", subMenu.AddLinkedKeyBind("清线", 'V', KeyBindType.Press));

            // JungleClear
            subMenu = _menu.MainMenu.AddSubMenu("清野");
            ProcessLink("jungleUseE", subMenu.AddLinkedBool("使用 E"));
            ProcessLink("jungleActive", subMenu.AddLinkedKeyBind("打野", 'V', KeyBindType.Press));

            // Flee
            subMenu = _menu.MainMenu.AddSubMenu("逃跑");
            ProcessLink("fleeWalljump", subMenu.AddLinkedBool("跳墙"));
            ProcessLink("fleeAA", subMenu.AddLinkedBool("智能AA"));
            ProcessLink("fleeActive", subMenu.AddLinkedKeyBind("逃跑", 'Z', KeyBindType.Press));

            // Misc
            subMenu = _menu.MainMenu.AddSubMenu("杂项");
            ProcessLink("miscKillstealE", subMenu.AddLinkedBool("E 抢人头"));
            ProcessLink("miscBigE", subMenu.AddLinkedBool("使用 E 补刀"));
            ProcessLink("miscUseR", subMenu.AddLinkedBool("使用 R 拯救灵魂绑定者"));
            ProcessLink("miscAutoE", subMenu.AddLinkedBool("当你AA无法杀死时 自动 E"));
            ProcessLink("miscAutoEchamp", subMenu.AddLinkedBool("当敌人身上有1+叠层 小兵可杀死 自动E "));

            // Spell settings
            subMenu = _menu.MainMenu.AddSubMenu("法术");
            ProcessLink("spellReductionE", subMenu.AddLinkedSlider("E 伤害减免", 20));

            // Items
            subMenu = _menu.MainMenu.AddSubMenu("物品");
            ProcessLink("itemsCutlass", subMenu.AddLinkedBool("使用小弯刀"));
            ProcessLink("itemsBotrk", subMenu.AddLinkedBool("使用破败"));
            ProcessLink("itemsYoumuu", subMenu.AddLinkedBool("使用幽梦"));

            // Drawings
            subMenu = _menu.MainMenu.AddSubMenu("显示");
            ProcessLink("drawDamageE", subMenu.AddLinkedCircle("显示E堆叠伤害", true, Color.FromArgb(150, Color.Green), 0));
            ProcessLink("drawRangeQ", subMenu.AddLinkedCircle("Q 范围", true, Color.FromArgb(150, Color.IndianRed), SpellManager.Q.Range));
            ProcessLink("drawRangeW", subMenu.AddLinkedCircle("W 范围", true, Color.FromArgb(150, Color.MediumPurple), SpellManager.W.Range));
            ProcessLink("drawRangeEsmall", subMenu.AddLinkedCircle("E 范围 (leaving)", false, Color.FromArgb(150, Color.DarkRed), SpellManager.E.Range - 200));
            ProcessLink("drawRangeEactual", subMenu.AddLinkedCircle("E 实际范围", true, Color.FromArgb(150, Color.DarkRed), SpellManager.E.Range));
            ProcessLink("drawRangeR", subMenu.AddLinkedCircle("R 范围", false, Color.FromArgb(150, Color.Red), SpellManager.R.Range));
        }
    }
}
