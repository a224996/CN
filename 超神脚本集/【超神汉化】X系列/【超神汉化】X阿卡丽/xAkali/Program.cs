﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
namespace xAkali
{
    class Program
    {
        public static string ChampName = "Akali";
        public static Orbwalking.Orbwalker Orbwalker;
        private static readonly Obj_AI_Hero player = ObjectManager.Player;
        public static Spell Q, E, W, R;
       
        public static SpellSlot IgniteSlot;
        public static Items.Item Dfg, Gunblade, Zhonyas;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }
        public static Menu xMenu;


        private static void Game_OnGameLoad(EventArgs args)
        {
            if (player.BaseSkinName != ChampName) return;

            Q = new Spell(SpellSlot.Q, 600);
            W = new Spell(SpellSlot.W, 700);
            E = new Spell(SpellSlot.E, 325);
            R = new Spell(SpellSlot.R, 800);

            Gunblade = new Items.Item(3146, 700f);
            Dfg = new Items.Item(3128, 750f);
            Zhonyas = new Items.Item(3157, 0f);

            IgniteSlot = player.GetSpellSlot("SummonerDot");


            xMenu = new Menu("【超神汉化】X" + ChampName, ChampName, true);

            xMenu.AddSubMenu(new Menu("走砍", "Orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(xMenu.SubMenu("Orbwalker"));

            var ts = new Menu("目标选择", "Target Selector");
            TargetSelector.AddToMenu(ts);
            xMenu.AddSubMenu(ts);

            xMenu.AddSubMenu(new Menu("连招", "Combo"));
            xMenu.SubMenu("Combo").AddItem(new MenuItem("useQ", "使用Q").SetValue(true));
            xMenu.SubMenu("Combo").AddItem(new MenuItem("useE", "使用E").SetValue(true));
            xMenu.SubMenu("Combo").AddItem(new MenuItem("useR", "使用R").SetValue(true));
            xMenu.SubMenu("Combo").AddItem(new MenuItem("chaseR", "只使用R追人").SetValue(true));
            xMenu.SubMenu("Combo").AddItem(new MenuItem("RRange", "R追人范围").SetValue(new Slider(500, 50, 800)));
            xMenu.SubMenu("Combo").AddItem(new MenuItem("useItems", "使用物品").SetValue(true));
            xMenu.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "热键").SetValue(new KeyBind(32, KeyBindType.Press)));

            xMenu.AddSubMenu(new Menu("骚扰", "Harass"));
            xMenu.SubMenu("Harass").AddItem(new MenuItem("hQ", "使用Q").SetValue(true));
            xMenu.SubMenu("Harass").AddItem(new MenuItem("hE", "使用E").SetValue(true));
            

            xMenu.SubMenu("Harass").AddItem(new MenuItem("HarassActive", "热键").SetValue(new KeyBind('C', KeyBindType.Press)));
            xMenu.SubMenu("Harass").AddItem(new MenuItem("HarassToggle", "锁定").SetValue(new KeyBind('T', KeyBindType.Toggle)));

            xMenu.AddSubMenu(new Menu("清线", "Laneclear"));
            xMenu.SubMenu("Laneclear").AddItem(new MenuItem("laneclearE", "使用E").SetValue(true));
            xMenu.SubMenu("Laneclear").AddItem(new MenuItem("laneclearnum", "小兵数量").SetValue(new Slider(2, 1, 5)));
            xMenu.SubMenu("Laneclear").AddItem(new MenuItem("LaneclearActive", "热键").SetValue(new KeyBind('V', KeyBindType.Press)));




            xMenu.AddSubMenu(new Menu("抢人头", "Killsteal"));
            xMenu.SubMenu("Killsteal").AddItem(new MenuItem("KillQ", "Q").SetValue(true));
            xMenu.SubMenu("Killsteal").AddItem(new MenuItem("KillE", "E").SetValue(true));
            xMenu.SubMenu("Killsteal").AddItem(new MenuItem("KillR", "R").SetValue(true));
            xMenu.SubMenu("Killsteal").AddItem(new MenuItem("KillI", "点燃").SetValue(true));

            xMenu.AddSubMenu(new Menu("显示", "Drawing"));
            xMenu.SubMenu("Drawing").AddItem(new MenuItem("DrawQ", "Q范围").SetValue(true));
            xMenu.SubMenu("Drawing").AddItem(new MenuItem("DrawE", "E范围").SetValue(true));
            xMenu.SubMenu("Drawing").AddItem(new MenuItem("DrawR", "R范围").SetValue(true));
            xMenu.SubMenu("Drawing").AddItem(new MenuItem("DrawAA", "平A范围").SetValue(true));
            xMenu.SubMenu("Drawing").AddItem(new MenuItem("DrawHP", "显示伤害").SetValue(true));

          

            xMenu.AddSubMenu(new Menu("杂项", "Misc"));
            xMenu.SubMenu("Misc").AddItem(new MenuItem("Packet", "封包").SetValue(true));
            xMenu.SubMenu("Misc").AddItem(new MenuItem("AW", "血量小于X使用W").SetValue(new Slider(25, 0, 100)));
            xMenu.SubMenu("Misc").AddItem(new MenuItem("Zhonyas", "血量小于X使用中亚").SetValue(new Slider(5, 0, 100)));
			
			xMenu.AddSubMenu(new Menu("超神汉化", "Chaoshen"));
            xMenu.SubMenu("Chaoshen").AddItem(new MenuItem("Qunhao", "L#汉化群：386289593"));
			
            
            Utility.HpBarDamageIndicator.DamageToUnit = ComboDamage;
            Utility.HpBarDamageIndicator.Enabled = xMenu.Item("DrawHP").GetValue<bool>();


            xMenu.AddToMainMenu();

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnGameUpdate;
            Game.PrintChat("x" + ChampName);
            
        }

        static void Game_OnGameUpdate(EventArgs args)
        {
            if (xMenu.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
            }

            if (xMenu.Item("HarassActive").GetValue<KeyBind>().Active || xMenu.Item("HarassToggle").GetValue<KeyBind>().Active)
            {
                Harass();
            }

            if (xMenu.Item("LaneclearActive").GetValue<KeyBind>().Active)
            {
                Laneclear();
            }
            
            KillSteal();
            AW();
            Z();

        }

     
        public static void AW()
        {
            if (player.Health / player.MaxHealth * 100 < xMenu.Item("AW").GetValue<Slider>().Value)
            {
                W.Cast(player);
            }


        }

        public static void Z()
        {
            if (player.Health / player.MaxHealth * 100 < xMenu.Item("Zhonyas").GetValue<Slider>().Value && Zhonyas.IsReady())
            {
                Zhonyas.Cast(player);
            }


        }
      

        static void Drawing_OnDraw(EventArgs args)
        {
            if (xMenu.Item("DrawQ").GetValue<bool>() == true)
            {
                Utility.DrawCircle(ObjectManager.Player.Position, Q.Range, Color.Red);
            }

            if (xMenu.Item("DrawE").GetValue<bool>() == true)
            {
                Utility.DrawCircle(ObjectManager.Player.Position, E.Range, Color.Orange);
            }

            if (xMenu.Item("DrawR").GetValue<bool>() == true)
            {
                Utility.DrawCircle(player.Position, R.Range, Color.Blue, 5, 30, true);
            }

            if (xMenu.Item("DrawAA").GetValue<bool>() == true)
            {
                Utility.DrawCircle(ObjectManager.Player.Position, ObjectManager.Player.AttackRange, Color.Red);
            }

        }


        public static int ultiCount()
        {
            foreach (BuffInstance buff in player.Buffs)
                if (buff.Name == "AkaliShadowDance")
                    return buff.Count;
            return 0;
        }

        private static float ComboDamage(Obj_AI_Base enemy)
        {

            int UC = ultiCount();
            int jumpCount = (UC - (int)(enemy.Distance(player.Position) / R.Range));
            double damage = 0d;

            if (Dfg.IsReady())
                damage += player.GetItemDamage(enemy, Damage.DamageItems.Dfg) / 1.2;

            if (Gunblade.IsReady())
                damage += player.GetItemDamage(enemy, Damage.DamageItems.Hexgun);

            if (Q.IsReady())
                damage += player.GetSpellDamage(enemy, SpellSlot.Q);

            if (UC > 0) damage += jumpCount > 0 ? player.GetSpellDamage(enemy, SpellSlot.R) * jumpCount : player.GetSpellDamage(enemy, SpellSlot.R);

            if (Dfg.IsReady())
                damage = damage * 1.2;

            if (E.IsReady())
                damage += player.GetSpellDamage(enemy, SpellSlot.E);

            if (player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                damage += player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);

            if (Items.HasItem(3155, (Obj_AI_Hero)enemy))
            {
                damage = damage - 250;
            }

            if (Items.HasItem(3156, (Obj_AI_Hero)enemy))
            {
                damage = damage - 400;
            }
            return (float)damage;
        }

        private static void Laneclear()
        {
            if (xMenu.SubMenu("Laneclear").Item("laneclearE").GetValue<bool>() && E.IsReady())
            {
                if (MinionManager.GetMinions(player.Position, E.Range, MinionTypes.All, MinionTeam.Enemy).Count >= xMenu.SubMenu("Laneclear").Item("laneclearnum").GetValue<Slider>().Value) E.Cast();
                foreach (Obj_AI_Base minion in MinionManager.GetMinions(player.ServerPosition, Q.Range,
                      MinionTypes.All,
                      MinionTeam.Neutral, MinionOrderTypes.MaxHealth))
                    if (player.Distance(minion, false) <= E.Range)
                        E.Cast();
               
            }
        }

        


        public static void KillSteal()

        {
            
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (target == null) return;
            var igniteDmg = player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);

            if (target.IsValidTarget(Q.Range) && Q.IsReady() && xMenu.Item("KillQ").GetValue<bool>() == true && ObjectManager.Player.GetSpellDamage(target, SpellSlot.Q) > target.Health)
            {
                Q.CastOnUnit(target, xMenu.Item("Packet").GetValue<bool>());
            }

            if (target.IsValidTarget(E.Range) && E.IsReady() && xMenu.Item("KillE").GetValue<bool>() == true && ObjectManager.Player.GetSpellDamage(target, SpellSlot.E) > target.Health)
            {
                E.Cast(target, xMenu.Item("Packet").GetValue<bool>());
                
            }

            if (target.IsValidTarget(R.Range) && R.IsReady() && xMenu.Item("KillR").GetValue<bool>() == true && ObjectManager.Player.GetSpellDamage(target, SpellSlot.R) > target.Health)
            {
                R.CastOnUnit(target, xMenu.Item("Packet").GetValue<bool>());
            }



            if (xMenu.Item("KillI").GetValue<bool>() == true && player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
            {
                if (igniteDmg > target.Health && player.Distance(target, false) < 600)
                {
                    player.Spellbook.CastSpell(IgniteSlot, target);
                }

            }




        }

        

        public static void Harass()
        {
            


            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (target == null)
                return;
            {
                if (target.IsValidTarget(Q.Range) && Q.IsReady() && xMenu.Item("hQ").GetValue<bool>() == true)
                {
                    Q.CastOnUnit(target, xMenu.Item("Packet").GetValue<bool>());
                }

                if (target.IsValidTarget(E.Range) && E.IsReady() && xMenu.Item("hE").GetValue<bool>() == true)
                {
                    E.Cast(target, xMenu.Item("Packet").GetValue<bool>());
                   
                }
                


            }
        }

        public static void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (target == null) return;

            float dmg = ComboDamage(target);

            if (dmg > target.Health + 20)
            {
                if (Gunblade.IsReady() && xMenu.Item("useItems").GetValue<bool>())
                {
                    Gunblade.Cast(target);
                }

                if (Dfg.IsReady() && xMenu.Item("useItems").GetValue<bool>() == true)
                {
                    Dfg.Cast(target);
                }

                if (target.IsValidTarget(Q.Range) && Q.IsReady() && xMenu.Item("useQ").GetValue<bool>() == true)
                {
                    Q.CastOnUnit(target, xMenu.Item("Packet").GetValue<bool>());
                }

                    if (xMenu.Item("chaseR").GetValue<bool>())
                    {
                        if (target.IsValidTarget(xMenu.Item("RRange").GetValue<Slider>().Value) && R.IsReady() && xMenu.Item("useR").GetValue<bool>())
                        {
                            R.CastOnUnit(target);

                        }
                        
                    }
                    else
                    {
                        if (target.IsValidTarget(R.Range) && R.IsReady() && xMenu.Item("useR").GetValue<bool>())
                        {
                            R.CastOnUnit(target, xMenu.Item("Packet").GetValue<bool>());
                        }

                    


                }

                if (target.IsValidTarget(E.Range) && E.IsReady() && xMenu.Item("useE").GetValue<bool>() == true)
                {
                    E.Cast(target, xMenu.Item("Packet").GetValue<bool>());
                    
                }
               


                

            }

            else
            {
                if (target.IsValidTarget(Q.Range) && Q.IsReady() && xMenu.Item("useQ").GetValue<bool>() == true)
                {
                    Q.CastOnUnit(target, xMenu.Item("Packet").GetValue<bool>());
                }

                    if (xMenu.Item("chaseR").GetValue<bool>())
                    {
                        if (target.IsValidTarget(Q.Range + 100) && R.IsReady() && xMenu.Item("useR").GetValue<bool>())
                        {
                            R.CastOnUnit(target);

                        }

                    }
                    else
                    {
                        if (target.IsValidTarget(R.Range) && R.IsReady() && xMenu.Item("useR").GetValue<bool>())
                        {
                            R.CastOnUnit(target, xMenu.Item("Packet").GetValue<bool>());
                        }

                    }

                

                if (target.IsValidTarget(E.Range) && E.IsReady() && xMenu.Item("useE").GetValue<bool>() == true)
                {
                    E.Cast(target, xMenu.Item("Packet").GetValue<bool>());
                   
                }
            }
        }



     







    }

}
