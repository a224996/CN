﻿using System;
using LeagueSharp;
using LeagueSharp.Common;
using Support.Util;
using ActiveGapcloser = Support.Util.ActiveGapcloser;

namespace Support.Plugins
{
    public class Taric : PluginBase
    {
        public Taric()
        {
            Q = new Spell(SpellSlot.Q, 750);
            W = new Spell(SpellSlot.W, 200);
            E = new Spell(SpellSlot.E, 625);
            R = new Spell(SpellSlot.R, 200);
        }

        public override void OnUpdate(EventArgs args)
        {
            if (ComboMode)
            {
                var ally = Helpers.AllyBelowHp(ConfigValue<Slider>("ComboHealthQ").Value, Q.Range);
                if (Q.CastCheck(ally, "ComboQ", true, false))
                {
                    Q.Cast(ally);
                }

                if (W.CastCheck(Target, "ComboW"))
                {
                    W.Cast();
                }

                if (E.CastCheck(Target, "ComboE"))
                {
                    E.Cast(Target);
                }

                if (R.CastCheck(Target, "ComboR"))
                {
                    R.Cast();
                }
            }

            if (HarassMode)
            {
                var ally = Helpers.AllyBelowHp(ConfigValue<Slider>("HarassHealthQ").Value, Q.Range);
                if (Q.CastCheck(ally, "HarassQ", true, false))
                {
                    Q.Cast(ally);
                }

                if (E.CastCheck(Target, "HarassE"))
                {
                    E.Cast(Target);
                }
            }
        }

        public override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (gapcloser.Sender.IsAlly)
            {
                return;
            }

            if (E.CastCheck(gapcloser.Sender, "GapcloserE"))
            {
                E.Cast(gapcloser.Sender);
            }
        }

        public override void OnPossibleToInterrupt(Obj_AI_Hero target, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (args.DangerLevel < Interrupter2.DangerLevel.High || target.IsAlly)
            {
                return;
            }

            if (E.CastCheck(target, "InterruptE"))
            {
                E.Cast(target);
            }
        }

        public override void ComboMenu(Menu config)
        {
            config.AddBool("ComboQ", "使用 Q", true);
            config.AddBool("ComboW", "使用 W", true);
            config.AddBool("ComboE", "使用 E", true);
            config.AddBool("ComboR", "使用 R", true);
            config.AddSlider("ComboHealthQ", "健康回复", 20, 1, 100);
        }

        public override void HarassMenu(Menu config)
        {
            config.AddBool("HarassQ", "使用 Q", true);
            config.AddBool("HarassE", "使用 E", true);
            config.AddSlider("HarassHealthQ", "健康回复", 20, 1, 100);
        }

        public override void InterruptMenu(Menu config)
        {
            config.AddBool("GapcloserE", "使用 E 防止突进", true);

            config.AddBool("InterruptE", "使用 E 打断技能", true);
        }
    }
}