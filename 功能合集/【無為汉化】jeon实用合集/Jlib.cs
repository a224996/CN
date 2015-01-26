#region
using System;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX.Direct3D9;
using SharpDX;
using System.Linq;
using System.Collections.Generic;
#endregion

namespace JeonUtility
{
    class Jlib
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
        public static String getMenuStringValue(String menu)
        {
            return Program.baseMenu.Item(menu).GetValue<StringList>().SelectedValue;
        }

        public static void Drawtext_outlined(Font font, String text, int x, int y, Color color)
        {
            font.DrawText(null, text, x + 1, y + 1, Color.Black);
            font.DrawText(null, text, x, y + 1, Color.Black);
            font.DrawText(null, text, x - 1, y - 1, Color.Black);
            font.DrawText(null, text, x, y - 1, Color.Black);
            font.DrawText(null, text, x, y, color);
        }
        public static void DrawCircleOnMinimap(Vector3 center,
            float radius,
            System.Drawing.Color color,
            int thickness = 5,
            int quality = 30)
        {
            var pointList = new List<Vector3>();
            for (var i = 0; i < quality; i++)
            {
                var angle = i * Math.PI * 2 / quality;
                pointList.Add(
                    new Vector3(
                        center.X + radius * (float)Math.Cos(angle), center.Y + radius * (float)Math.Sin(angle),
                        center.Z));
            }

            for (var i = 0; i < pointList.Count; i++)
            {
                var a = pointList[i];
                var b = pointList[i == pointList.Count - 1 ? 0 : i + 1];

                var aonScreen = Drawing.WorldToMinimap(a);
                var bonScreen = Drawing.WorldToMinimap(b);

                Drawing.DrawLine(aonScreen.X, aonScreen.Y, bonScreen.X, bonScreen.Y, thickness, color);
            }
        }
    }
}
