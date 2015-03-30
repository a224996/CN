using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
namespace Ezreal
{
    class Program
    {
        public static string ChampName = "Ezreal";
        public static Orbwalking.Orbwalker Orbwalker;
        private static readonly Obj_AI_Hero player = ObjectManager.Player;
        public static Spell Q, W, R;
        public static SpellSlot IgniteSlot;
        public static Items.Item Dfg, Gunblade;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }
        public static Menu xMenu;


        private static void Game_OnGameLoad(EventArgs args)
        {
            if (player.BaseSkinName != ChampName) return;

            Q = new Spell(SpellSlot.Q, 1100);
            W = new Spell(SpellSlot.W, 900);

            R = new Spell(SpellSlot.R, float.MaxValue);

            Q.SetSkillshot(0.5f, 80f, 1200, true, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.5f, 80f, 1200, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(1f, 160f, 2000, false, SkillshotType.SkillshotLine);

            IgniteSlot = player.GetSpellSlot("SummonerDot");

            Dfg = new Items.Item(3128, 750f);
            Gunblade = new Items.Item(3146, 700f);
         
            xMenu = new Menu("【超神汉化】x" + ChampName, ChampName, true);
          
            xMenu.AddSubMenu(new Menu("走砍", "Orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(xMenu.SubMenu("Orbwalker"));
          
            var ts = new Menu("目标选择", "Target Selector");
            TargetSelector.AddToMenu(ts);
            xMenu.AddSubMenu(ts);
           
            xMenu.AddSubMenu(new Menu("连招", "Combo"));
            xMenu.SubMenu("Combo").AddItem(new MenuItem("useQ", "使用Q").SetValue(true));
            xMenu.SubMenu("Combo").AddItem(new MenuItem("useW", "使用W").SetValue(true));
            xMenu.SubMenu("Combo").AddItem(new MenuItem("useR", "使用R").SetValue(true));
            xMenu.SubMenu("Combo").AddItem(new MenuItem("useItems", "使用物品").SetValue(true));
            xMenu.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "热键").SetValue(new KeyBind(32, KeyBindType.Press)));

            xMenu.AddSubMenu(new Menu("骚扰", "Harass"));
            xMenu.SubMenu("Harass").AddItem(new MenuItem("hQ", "使用Q").SetValue(true));
            xMenu.SubMenu("Harass").AddItem(new MenuItem("hW", "使用W").SetValue(true));
            xMenu.SubMenu("Harass").AddItem(new MenuItem("Harassmana", "蓝量控制").SetValue(new Slider(30)));
            xMenu.SubMenu("Harass").AddItem(new MenuItem("HarassActive", "热键").SetValue(new KeyBind('C', KeyBindType.Press)));
            xMenu.SubMenu("Harass").AddItem(new MenuItem("HarassToggle", "锁定").SetValue(new KeyBind('T', KeyBindType.Toggle)));

            xMenu.AddSubMenu(new Menu("抢人头", "Killsteal"));
            xMenu.SubMenu("Killsteal").AddItem(new MenuItem("KillQ", "Q").SetValue(true));
            xMenu.SubMenu("Killsteal").AddItem(new MenuItem("KillW", "W").SetValue(true));
            xMenu.SubMenu("Killsteal").AddItem(new MenuItem("KillR", "R").SetValue(true));
            xMenu.SubMenu("Killsteal").AddItem(new MenuItem("KillI", "点燃").SetValue(true));

            xMenu.AddSubMenu(new Menu("显示", "Drawing"));
            xMenu.SubMenu("Drawing").AddItem(new MenuItem("DrawQ", "Q范围").SetValue(true));
            xMenu.SubMenu("Drawing").AddItem(new MenuItem("DrawW", "W范围").SetValue(true));
            xMenu.SubMenu("Drawing").AddItem(new MenuItem("DrawAA", "平A范围").SetValue(true));
            
    

			xMenu.AddItem(new MenuItem("Packet", "封包").SetValue(true));
			
            xMenu.AddSubMenu(new Menu("超神汉化", "Chaoshen"));
            xMenu.SubMenu("Chaoshen").AddItem(new MenuItem("Qunhao", "L#汉化群：386289593"));     
			
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

            KillSteal();
        }


      

        static void Drawing_OnDraw(EventArgs args)
        {
            if (xMenu.Item("DrawQ").GetValue<bool>() == true)
            {
                Utility.DrawCircle(player.Position, Q.Range, Color.Red);
            }

            if (xMenu.Item("DrawW").GetValue<bool>() == true)
            {
                Utility.DrawCircle(player.Position, W.Range, Color.Orange);
            }
            if (xMenu.Item("DrawAA").GetValue<bool>() == true)
            {
                Utility.DrawCircle(player.Position, player.AttackRange, Color.Blue);
            }
            




        }

        public static void KillSteal()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (target == null) return;
            var igniteDmg = player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);

            if (target.IsValidTarget(Q.Range) && Q.IsReady() && xMenu.Item("KillQ").GetValue<bool>() == true && ObjectManager.Player.GetSpellDamage(target, SpellSlot.Q) > target.Health)
            {
                Q.Cast(target, xMenu.Item("Packet").GetValue<bool>());
            }

            if (target.IsValidTarget(W.Range) && W.IsReady() && xMenu.Item("KillW").GetValue<bool>() == true && ObjectManager.Player.GetSpellDamage(target, SpellSlot.W) > target.Health)
            {
                W.Cast(target, xMenu.Item("Packet").GetValue<bool>());
            }

            if (target.IsValidTarget(3000) && R.IsReady() && xMenu.Item("KillR").GetValue<bool>() == true && ObjectManager.Player.GetSpellDamage(target, SpellSlot.R) > target.Health)
            {
                R.Cast(target, xMenu.Item("Packet").GetValue<bool>());
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
             if (player.Mana / player.MaxMana * 100 > xMenu.SubMenu("Harass").Item("Harassmana").GetValue<Slider>().Value)
                return;
            

            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (target == null)
                return;
            {
                if (target.IsValidTarget(Q.Range) && Q.IsReady() && xMenu.Item("hQ").GetValue<bool>() == true)
                {
                    Q.Cast(target, xMenu.Item("Packet").GetValue<bool>());
                }

                if (target.IsValidTarget(W.Range) && W.IsReady() && xMenu.Item("hW").GetValue<bool>() == true)
                {
                    W.Cast(target, xMenu.Item("Packet").GetValue<bool>());
                }


            }






        }


        public static void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (target == null) return;


            if (Gunblade.IsReady() && xMenu.Item("useItems").GetValue<bool>() == true)
            {
                Gunblade.Cast(target);
            }

            if (Dfg.IsReady() && xMenu.Item("useItems").GetValue<bool>() == true)
            {
                Dfg.Cast(target);
            }

            if (target.IsValidTarget(Q.Range) && Q.IsReady() && xMenu.Item("useQ").GetValue<bool>() == true)
            {
                Q.Cast(target, xMenu.Item("Packet").GetValue<bool>());



            }

            if (target.IsValidTarget(W.Range) && W.IsReady() && xMenu.Item("useW").GetValue<bool>() == true)
            {
                W.Cast(target, xMenu.Item("Packet").GetValue<bool>());
            }



            if (target.IsValidTarget(R.Range) && R.IsReady() && ObjectManager.Player.GetSpellDamage(target, SpellSlot.R) > target.Health && xMenu.Item("useR").GetValue<bool>() == true)
            {
                R.CastOnUnit(target, xMenu.Item("Packet").GetValue<bool>());
            }








        }
    }
}
