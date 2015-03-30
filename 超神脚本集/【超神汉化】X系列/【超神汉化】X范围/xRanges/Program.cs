﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
namespace xRanges
{
    class Program
    {
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }
        public static Menu xMenu;
        private static readonly Obj_AI_Hero player = ObjectManager.Player;

        private static void Game_OnGameLoad(EventArgs args)
        {
           

            xMenu = new Menu("【超神汉化】X范围", "xRanges", true);

            
            xMenu.AddSubMenu(new Menu("显示", "Drawing"));
            xMenu.SubMenu("Drawing").AddItem(new MenuItem("DrawER", "显示敌人范围").SetValue(true));
            xMenu.SubMenu("Drawing").AddItem(new MenuItem("DrawAR", "显示队友范围").SetValue(true));
            xMenu.SubMenu("Drawing").AddItem(new MenuItem("DrawAA", "显示自己范围").SetValue(true));

           


           

            xMenu.AddToMainMenu();

            Drawing.OnDraw += Drawing_OnDraw;
            

            Game.PrintChat("xRanges");
        }


        static void Drawing_OnDraw(EventArgs args)
        {
            
         
            
                if (xMenu.Item("DrawAA").GetValue<bool>())
                {
                    Utility.DrawCircle(player.Position, player.AttackRange, Color.Blue);
                }

                if (xMenu.Item("DrawER").GetValue<bool>())
                {
                    foreach (Obj_AI_Hero enemy in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        if (enemy.IsEnemy && enemy.IsVisible && enemy.IsValid && !enemy.IsDead)
                        {
                            Utility.DrawCircle(enemy.Position, enemy.AttackRange, Color.Red);
                        }
                    }
                }
                if (xMenu.Item("DrawAR").GetValue<bool>())
                {

                    foreach (Obj_AI_Hero ally in ObjectManager.Get<Obj_AI_Hero>())
                    {
                        if (ally.IsAlly && ally.IsVisible && ally.IsValid && !ally.IsDead && !ally.PlayerControlled)
                        {
                            Utility.DrawCircle(ally.Position, ally.AttackRange, Color.Green);
                        }
                    }
                }
            
           





        }

    }
}
