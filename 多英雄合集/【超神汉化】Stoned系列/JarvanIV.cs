﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace StonedSeriesAIO
{
    internal class JarvanIV
    {
        private const string Champion = "JarvanIV";

        private static Orbwalking.Orbwalker Orbwalker;

        private static List<Spell> SpellList = new List<Spell>();

        private static Spell Q;

        private static Spell W;

        private static Spell E;

        private static Spell R;

        public static SpellSlot IgniteSlot;

        private static Menu Config;

        private static Obj_AI_Hero Player;

        public JarvanIV()
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;
            if (ObjectManager.Player.BaseSkinName != Champion) return;

            Q = new Spell(SpellSlot.Q, 770);
            W = new Spell(SpellSlot.W, 525);
            E = new Spell(SpellSlot.E, 800);
            R = new Spell(SpellSlot.R, 650);

            IgniteSlot = Player.GetSpellSlot("SummonerDot");

            Q.SetSkillshot(0.25f, 70f, 1450f, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.5f, 175f, int.MaxValue, false, SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            Config = new Menu("【超神汉化】皇子", "StonedJarvan", true);


            var targetSelectorMenu = new Menu("目标选择", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            Config.AddSubMenu(new Menu("走砍", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            Config.AddSubMenu(new Menu("连招", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "使用Q")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "使用W")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "使用E")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UseEQCombo", "使用EQ")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "使用R")).SetValue(true);
            Config.SubMenu("Combo").AddItem(new MenuItem("ActiveCombo", "热键").SetValue(new KeyBind(32, KeyBindType.Press)));

            Config.AddSubMenu(new Menu("显示", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawQ", "Q范围")).SetValue(true);
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawW", "W范围")).SetValue(true);
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawE", "E范围")).SetValue(true);
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawR", "R范围")).SetValue(true);
            Config.SubMenu("Drawings").AddItem(new MenuItem("CircleLag", "延迟线圈").SetValue(true));
            Config.SubMenu("Drawings").AddItem(new MenuItem("CircleThickness", "线圈厚度").SetValue(new Slider(1, 10, 1)));

            Config.AddSubMenu(new Menu("杂项", "Misc"));
            Config.SubMenu("Misc").AddItem(new MenuItem("EQmouse", "向鼠标EQ").SetValue(new KeyBind("G".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("Misc").AddItem(new MenuItem("Ignite", "使用点燃").SetValue(true));
			Config.AddSubMenu(new Menu("超神汉化", "Chaoshen"));
			Config.SubMenu("Chaoshen").AddItem(new MenuItem("Qun", "L#汉化群：386289593"));
            Config.AddToMainMenu();

            Game.OnUpdate += OnGameUpdate;
            Drawing.OnDraw += OnDraw;

            //Obj_AI_Hero.OnCreate += OnCreateObj;
            //Obj_AI_Hero.OnDelete += OnDeleteObj;

            Game.PrintChat("<font color='#FF00BF'>Stoned Jarvan Loaded By</font> <font color='#FF0000'>The</font><font color='#FFFF00'>Kush</font><font color='#40FF00'>Style</font>");
        }

        /* WIP
         * private static void OnCreateObj(GameObject sender, EventArgs args)
         {
             throw new NotImplementedException();
         }
         private static void OnDeleteObj(GameObject sender, EventArgs args)
         {
             throw new NotImplementedException();
         } 
         */



        private static void OnGameUpdate(EventArgs args)
        {
            Player = ObjectManager.Player;
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (target == null) return;

            Orbwalker.SetAttack(true);
            if (Config.Item("ActiveCombo").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            if (Config.Item("EQmouse").GetValue<KeyBind>().Active)
            {
                EQMouse();
            }
            if (Player.Distance(target) <= 600 && IgniteDamage(target) >= target.Health && Config.Item("Ignite").GetValue<bool>())
            {
                Player.Spellbook.CastSpell(IgniteSlot, target);
            }

        }

        private static void EQMouse()
        {
            if (E.IsReady() && Q.IsReady())
            {
                E.Cast(Game.CursorPos);
                Q.Cast(Game.CursorPos);
            }
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (target == null) return;

            if (E.IsReady() && Q.IsReady())
            {
                if (Player.Distance(target) <= E.Range && (Config.Item("UseEQCombo").GetValue<bool>()))
                {
                    E.Cast(target);
                    Q.Cast(target);
                }
            }
            else if (Player.Distance(target) <= E.Range && E.IsReady() && (Config.Item("UseECombo").GetValue<bool>()))
            {
                E.Cast(target);
            }
            else if (Player.Distance(target) <= Q.Range && Q.IsReady() && (Config.Item("UseQCombo").GetValue<bool>()))
            {
                Q.Cast(target);
            }
            if (Player.Distance(target) <= R.Range && (Config.Item("UseRCombo").GetValue<bool>()))
            {
                R.Cast(target);
            }
            if (Player.Distance(target) <= W.Range && (Config.Item("UseWCombo").GetValue<bool>()))
            {
                W.Cast();
            }


        }

        private static void OnDraw(EventArgs args)
        {
            if (Config.Item("CircleLag").GetValue<bool>())
            {
                if (Config.Item("DrawQ").GetValue<bool>())
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.White, Config.Item("CircleThickness").GetValue<Slider>().Value);
                }
                if (Config.Item("DrawW").GetValue<bool>())
                {
                    Render.Circle.DrawCircle(
                        ObjectManager.Player.Position, W.Range, System.Drawing.Color.White,
                        Config.Item("CircleThickness").GetValue<Slider>().Value);
                }
                if (Config.Item("DrawE").GetValue<bool>())
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.White,
                        Config.Item("CircleThickness").GetValue<Slider>().Value);
                }
                if (Config.Item("DrawR").GetValue<bool>())
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range, System.Drawing.Color.White,
                        Config.Item("CircleThickness").GetValue<Slider>().Value);
                }
            }
            else
            {
                if (Config.Item("DrawQ").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.White);
                }
                if (Config.Item("DrawW").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, W.Range, System.Drawing.Color.White);
                }
                if (Config.Item("DrawE").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.White);
                }
                if (Config.Item("DrawR").GetValue<bool>())
                {
                    Drawing.DrawCircle(ObjectManager.Player.Position, R.Range, System.Drawing.Color.White);
                }

            }  
        }

        private static float IgniteDamage(Obj_AI_Hero target)
        {
            if (IgniteSlot == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(IgniteSlot) != SpellState.Ready) return 0f;
            return (float)Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
        }
    }
}
