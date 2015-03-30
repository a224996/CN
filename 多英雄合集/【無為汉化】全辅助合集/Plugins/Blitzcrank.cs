using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Support.Util;
using ActiveGapcloser = Support.Util.ActiveGapcloser;

namespace Support.Plugins
{
    public class Blitzcrank : PluginBase
    {
        public Blitzcrank()
        {
            Q = new Spell(SpellSlot.Q, 900);
            W = new Spell(SpellSlot.W, 0);
            E = new Spell(SpellSlot.E, AttackRange);
            R = new Spell(SpellSlot.R, 600);

            Q.SetSkillshot(0.25f, 70f, 1800f, true, SkillshotType.SkillshotLine);
        }

        private bool BlockQ
        {
            get
            {
                if (!Q.IsReady())
                {
                    return true;
                }

                if (!ConfigValue<bool>("Misc.Q.Block"))
                {
                    return false;
                }

                if (!Target.IsValidTarget())
                {
                    return true;
                }

                if (Target.HasBuff("BlackShield"))
                {
                    return true;
                }

                if (Helpers.AllyInRange(1200)
                    .Any(ally => ally.Distance(Target) < ally.AttackRange + ally.BoundingRadius))
                {
                    return true;
                }

                return Player.Distance(Target) < ConfigValue<Slider>("Misc.Q.Block.Distance").Value;
            }
        }

        public override void OnUpdate(EventArgs args)
        {
            try
            {
                if (ComboMode)
                {
                    if (Q.CastCheck(Target, "ComboQ") && !BlockQ)
                    {
                        Q.Cast(Target);
                    }

                    if (E.CastCheck(Target))
                    {
                        if (E.Cast())
                        {
                            Orbwalking.ResetAutoAttackTimer();
                            Player.IssueOrder(GameObjectOrder.AttackUnit, Target);
                        }
                    }

                    if (E.IsReady() && Target.IsValidTarget() && Target.HasBuff("RocketGrab"))
                    {
                        if (E.Cast())
                        {
                            Orbwalking.ResetAutoAttackTimer();
                            Player.IssueOrder(GameObjectOrder.AttackUnit, Target);
                        }
                    }

                    if (W.IsReady() && ConfigValue<bool>("ComboW") && Player.CountEnemiesInRange(1500) > 0)
                    {
                        W.Cast();
                    }

                    if (R.CastCheck(Target, "ComboR"))
                    {
                        if (Helpers.EnemyInRange(ConfigValue<Slider>("ComboCountR").Value, R.Range))
                        {
                            R.Cast();
                        }
                    }
                }

                if (HarassMode)
                {
                    if (Q.CastCheck(Target, "HarassQ") && !BlockQ)
                    {
                        Q.Cast(Target);
                    }

                    if (E.CastCheck(Target))
                    {
                        if (E.Cast())
                        {
                            Orbwalking.ResetAutoAttackTimer();
                            Player.IssueOrder(GameObjectOrder.AttackUnit, Target);
                        }
                    }

                    if (E.IsReady() && Target.IsValidTarget() && Target.HasBuff("RocketGrab"))
                    {
                        if (E.Cast())
                        {
                            Orbwalking.ResetAutoAttackTimer();
                            Player.IssueOrder(GameObjectOrder.AttackUnit, Target);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override void OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe)
            {
                return;
            }

            if (!target.IsValid<Obj_AI_Hero>() && !target.Name.ToLower().Contains("ward"))
            {
                return;
            }

            if (!E.IsReady())
            {
                return;
            }

            if (E.Cast())
            {
                Orbwalking.ResetAutoAttackTimer();
                Player.IssueOrder(GameObjectOrder.AttackUnit, target);
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
                if (E.Cast())
                {
                    Orbwalking.ResetAutoAttackTimer();
                    Player.IssueOrder(GameObjectOrder.AttackUnit, gapcloser.Sender);
                }
            }

            if (R.CastCheck(gapcloser.Sender, "GapcloserR"))
            {
                R.Cast();
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
                if (E.Cast())
                {
                    Orbwalking.ResetAutoAttackTimer();
                    Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                }
            }

            if (Q.CastCheck(Target, "InterruptQ"))
            {
                Q.Cast(target);
            }

            if (R.CastCheck(target, "InterruptR"))
            {
                R.Cast();
            }
        }

        public override void ComboMenu(Menu config)
        {
            config.AddBool("ComboQ", "使用 Q", true);
            config.AddBool("ComboW", "使用 W", true);
            config.AddBool("ComboR", "使用 R", true);
            config.AddSlider("ComboCountR", "几个敌人使用大招", 2, 1, 5);
        }

        public override void MiscMenu(Menu config)
        {
            config.AddBool("Misc.Q.Block", "对目标AQ", true);
            config.AddSlider("Misc.Q.Block.Distance", "Q限制的距离", 400, 0, 800);
        }

        public override void HarassMenu(Menu config)
        {
            config.AddBool("HarassQ", "使用 Q", true);
        }

        public override void InterruptMenu(Menu config)
        {
            config.AddBool("GapcloserE", "使用 E 防止突进", true);
            config.AddBool("GapcloserR", "使用 R 防止突进", true);
            config.AddBool("InterruptQ", "使用 Q 打断技能", true);
            config.AddBool("InterruptE", "使用 E 打断技能", true);
            config.AddBool("InterruptR", "使用 R 打断技能", true);
        }
    }
}