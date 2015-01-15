#region
using System;
using LeagueSharp.Common;
#endregion

namespace JeonUtility
{
    class Jlib:Program
    {

        public enum test
        {
            show_inventory,
            show_enemybuff,
            show_allybuff,
            show_mebuff
        }



        public static bool getMenuBool(String menu)
        {
            return Program.baseMenu.Item(menu).GetValue<bool>();
        }
        public static int getMenuValue(String menu)
        {
            return Program.baseMenu.Item(menu).GetValue<Slider>().Value;
        }
    }
}
