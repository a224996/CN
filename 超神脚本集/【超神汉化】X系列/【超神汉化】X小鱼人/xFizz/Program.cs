using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
namespace xFizz
{
    class Program
    {
        public static string ChampName = "Fizz";
        public static Orbwalking.Orbwalker Orbwalker;
        private static readonly Obj_AI_Hero player = ObjectManager.Player;
        public static Spell Q, W, E, E2,R;
        public static SpellSlot IgniteSlot;
        public static Items.Item Dfg;
      

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }
        public static Menu xMenu;


        private static void Game_OnGameLoad(EventArgs args)
        {
            if (player.BaseSkinName != ChampName) return;

            Q = new Spell(SpellSlot.Q, 550);
            W = new Spell(SpellSlot.W, 0);
            E = new Spell(SpellSlot.E, 400);
            E2 = new Spell(SpellSlot.E, 400);
            R = new Spell(SpellSlot.R, 1200);

            E.SetSkillshot(0.5f, 120, 1300, false, SkillshotType.SkillshotCircle);
            E2.SetSkillshot(0.5f, 400, 1300, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.5f, 250f, 1200f, false, SkillshotType.SkillshotLine);

            IgniteSlot = player.GetSpellSlot("SummonerDot");

            Dfg = new Items.Item(3128, 750f);
            

            xMenu = new Menu("【超神汉化】x" + ChampName, ChampName, true);

            xMenu.AddSubMenu(new Menu("走砍", "Orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(xMenu.SubMenu("Orbwalker"));

            var ts = new Menu("目标选择", "Target Selector");
            TargetSelector.AddToMenu(ts);
            xMenu.AddSubMenu(ts);

            xMenu.AddSubMenu(new Menu("连招", "Combo"));
            xMenu.SubMenu("Combo").AddItem(new MenuItem("useQ", "使用Q").SetValue(true));
            xMenu.SubMenu("Combo").AddItem(new MenuItem("QR", "使用Q+R (几乎隐藏R)").SetValue(true));
            xMenu.SubMenu("Combo").AddItem(new MenuItem("useW", "使用W").SetValue(true));
            xMenu.SubMenu("Combo").AddItem(new MenuItem("useE", "使用E").SetValue(true));
            xMenu.SubMenu("Combo").AddItem(new MenuItem("useR", "使用R").SetValue(true));
            xMenu.SubMenu("Combo").AddItem(new MenuItem("useItems", "使用物品").SetValue(true));
            xMenu.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "热键").SetValue(new KeyBind(32, KeyBindType.Press)));

            xMenu.AddSubMenu(new Menu("骚扰", "Harass"));
            xMenu.SubMenu("Harass").AddItem(new MenuItem("hQ", "使用Q").SetValue(true));
            xMenu.SubMenu("Harass").AddItem(new MenuItem("hE", "使用E").SetValue(true));
            xMenu.SubMenu("Harass").AddItem(new MenuItem("Harassmana", "蓝量控制").SetValue(new Slider(30)));
            xMenu.SubMenu("Harass").AddItem(new MenuItem("HarassActive", "热键").SetValue(new KeyBind('C', KeyBindType.Press)));
            xMenu.SubMenu("Harass").AddItem(new MenuItem("HarassToggle", "锁定").SetValue(new KeyBind('T', KeyBindType.Toggle)));

            xMenu.AddSubMenu(new Menu("抢人头", "Killsteal"));
            xMenu.SubMenu("Killsteal").AddItem(new MenuItem("KillQ", "Q").SetValue(true));
            xMenu.SubMenu("Killsteal").AddItem(new MenuItem("KillE", "E").SetValue(true));
            xMenu.SubMenu("Killsteal").AddItem(new MenuItem("KillR", "R").SetValue(true));
            xMenu.SubMenu("Killsteal").AddItem(new MenuItem("KillI", "点燃").SetValue(true));

            xMenu.AddSubMenu(new Menu("显示", "Drawing"));
            xMenu.SubMenu("Drawing").AddItem(new MenuItem("DrawQ", "Q范围").SetValue(true));
            xMenu.SubMenu("Drawing").AddItem(new MenuItem("DrawE", "E范围").SetValue(true));
            xMenu.SubMenu("Drawing").AddItem(new MenuItem("DrawAA", "平A范围").SetValue(true));
            xMenu.SubMenu("Drawing").AddItem(new MenuItem("DrawHP", "显示伤害").SetValue(true));


            xMenu.AddItem(new MenuItem("Packet", "封包").SetValue(true));
			
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

            KillSteal();
        }


     

        static void Drawing_OnDraw(EventArgs args)
        {
            if (xMenu.Item("DrawQ").GetValue<bool>() == true)
            {
                Utility.DrawCircle(player.Position, Q.Range, Color.Red);
            }

            if (xMenu.Item("DrawE").GetValue<bool>() == true)
            {
                Utility.DrawCircle(player.Position, E.Range, Color.Orange);
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

            if (target.IsValidTarget(E.Range) && E.IsReady() && xMenu.Item("KillE").GetValue<bool>() == true && player.GetSpellDamage(target, SpellSlot.E)> target.Health)
            {
                E.Cast(target, xMenu.Item("Packet").GetValue<bool>());
                E2.Cast(target);
            }

            if (target.IsValidTarget(R.Range) && R.IsReady() && xMenu.Item("KillR").GetValue<bool>() && player.GetSpellDamage(target, SpellSlot.R) > target.Health +20)




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
            Obj_AI_Turret closestTower = ObjectManager.Get<Obj_AI_Turret>().Where(tur => tur.IsAlly).OrderBy(tur => tur.Distance(player.Position)).First();
                
                                                
                    
                    
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

                if (target.IsValidTarget(E.Range) && E.IsReady() && xMenu.Item("hE").GetValue<bool>() == true)
                {
                    E.Cast(target, xMenu.Item("Packet").GetValue<bool>());
                    E2.Cast(closestTower.ServerPosition);
                }


            }






        }
        private static float ComboDamage(Obj_AI_Base enemy)
        {
            double damage = 0d;

            if (Dfg.IsReady())
                damage += player.GetItemDamage(enemy, Damage.DamageItems.Dfg) / 1.2;

            if (Q.IsReady())
                damage += player.GetSpellDamage(enemy, SpellSlot.Q);

            if (R.IsReady())
                damage += player.GetSpellDamage(enemy, SpellSlot.R);

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

        public static void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (target == null) return;
            float dmg = ComboDamage(target);

            if (dmg > target.Health + +20)
            {
                    if (Dfg.IsReady() && xMenu.Item("useItems").GetValue<bool>() == true)
                     {
                            Dfg.Cast(target);
                         }
                    if (target.IsValidTarget(Q.Range) && W.IsReady() && xMenu.Item("useW").GetValue<bool>() == true)
                    {
                        W.Cast(player, xMenu.Item("Packet").GetValue<bool>());
                    }
                    if (target.IsValidTarget(Q.Range) && Q.IsReady() && xMenu.Item("useQ").GetValue<bool>() == true)
                    {
                        Q.CastOnUnit(target, xMenu.Item("Packet").GetValue<bool>());



                    }
                if (target.IsValidTarget(R.Range) && R.IsReady())
            {
                R.Cast(target, xMenu.Item("Packet").GetValue<bool>());

            }
            
            
                if (target.IsValidTarget(E.Range) && E.IsReady() && xMenu.Item("useE").GetValue<bool>() == true)
                {
                    E.Cast(target, xMenu.Item("Packet").GetValue<bool>());
                    E2.Cast(target);
                }

            }
            else
            {
                if (xMenu.Item("QR").GetValue<bool>())
                {
                    if (target.IsValidTarget(Q.Range) && Q.IsReady() && xMenu.Item("useQ").GetValue<bool>() == true)
                    {
                        Q.CastOnUnit(target, xMenu.Item("Packet").GetValue<bool>());
                        R.Cast(target, xMenu.Item("Packet").GetValue<bool>());



                    }
                    else
                    {

                        if (target.IsValidTarget(Q.Range) && Q.IsReady() && xMenu.Item("useQ").GetValue<bool>() == true)
                        {
                            Q.CastOnUnit(target, xMenu.Item("Packet").GetValue<bool>());

                        }
                    }
                }
                if (target.IsValidTarget(E.Range) && E.IsReady() && xMenu.Item("useE").GetValue<bool>() == true)
                {
                    E.Cast(target, xMenu.Item("Packet").GetValue<bool>());
                    E2.Cast(target);
                }
            }
          

           

            

            



           








        }
    }
}
