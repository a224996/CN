﻿using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Support.Util;
using ActiveGapcloser = Support.Util.ActiveGapcloser;

namespace Support.Plugins
{
    public class Janna : PluginBase
    {
        public Janna()
        {
            Q = new Spell(SpellSlot.Q, 1100);
            W = new Spell(SpellSlot.W, 600);
            E = new Spell(SpellSlot.E, 800);
            R = new Spell(SpellSlot.R, 550);

            Q.SetSkillshot(0.25f, 120f, 900f, false, SkillshotType.SkillshotLine);
            GameObject.OnCreate += TowerAttackOnCreate;
            GameObject.OnCreate += RangeAttackOnCreate;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
        }

        private bool IsUltChanneling { get; set; }
        private int LastQInterrupt { get; set; }

        public override void OnUpdate(EventArgs args)
        {
            try
            {
                if (Player.IsChannelingImportantSpell())
                {
                    return;
                }

                if (IsUltChanneling)
                {
                    Orbwalker.SetAttack(true);
                    Orbwalker.SetMovement(true);
                    IsUltChanneling = false;
                }

                if (ComboMode)
                {
                    if (Q.CastCheck(Target, "Combo.Q"))
                    {
                        var pred = Q.GetPrediction(Target);
                        if (pred.Hitchance >= HitChance.High)
                        {
                            Q.Cast(pred.CastPosition);
                            Q.Cast();
                        }
                    }

                    if (W.CastCheck(Target, "Combo.W"))
                    {
                        W.CastOnUnit(Target);
                    }

                    var ally = Helpers.AllyBelowHp(ConfigValue<Slider>("Combo.R.Health").Value, R.Range);
                    if (R.CastCheck(ally, "Combo.R", true, false) && Player.CountEnemiesInRange(1000) > 0)
                    {
                        R.Cast();
                    }
                }

                if (HarassMode)
                {
                    if (W.CastCheck(Target, "Harass.W"))
                    {
                        W.CastOnUnit(Target);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && args.SData.Name == "ReapTheWhirlwind")
            {
                Orbwalker.SetAttack(false);
                Orbwalker.SetMovement(false);
                IsUltChanneling = true;
            }

            if (!E.IsReady() || !E.IsInRange(sender) || IsUltChanneling)
            {
                return;
            }

            // Boost Damage
            // Caster ally / target enemy hero
            if (sender.IsValid<Obj_AI_Hero>() && sender.IsAlly && !sender.IsMe)
            {
                var spell = args.SData.Name;
                var caster = (Obj_AI_Hero) sender;

                if (DamageBoostDatabase.Spells.Any(s => s.Spell == spell) && caster.CountEnemiesInRange(2000) > 0)
                {
                    switch (ConfigValue<Slider>("Misc.E.Spell." + args.SData.Name).Value) // prio 0 = disabled
                    {
                        case 1:
                            if (Player.ManaPercentage() > ConfigValue<Slider>("Mana.E.Priority.1").Value)
                            {
                                E.CastOnUnit(caster);
                            }
                            break;
                        case 2:
                            if (Player.ManaPercentage() > ConfigValue<Slider>("Mana.E.Priority.2").Value)
                            {
                                E.CastOnUnit(caster);
                            }
                            break;
                        case 3:
                            if (Player.ManaPercentage() > ConfigValue<Slider>("Mana.E.Priority.3").Value)
                            {
                                E.CastOnUnit(caster);
                            }
                            break;
                    }
                }
            }
        }

        private void RangeAttackOnCreate(GameObject sender, EventArgs args)
        {
            if (!sender.IsValid<Obj_SpellMissile>() || IsUltChanneling)
            {
                return;
            }

            var missile = (Obj_SpellMissile) sender;

            // Caster ally hero / not me
            if (!missile.SpellCaster.IsValid<Obj_AI_Hero>() || !missile.SpellCaster.IsAlly || missile.SpellCaster.IsMe ||
                missile.SpellCaster.IsMelee())
            {
                return;
            }

            // Target enemy hero
            if (!missile.Target.IsValid<Obj_AI_Hero>() || !missile.Target.IsEnemy)
            {
                return;
            }

            var caster = (Obj_AI_Hero) missile.SpellCaster;

            // only in SBTW mode
            if (E.IsReady() && E.IsInRange(caster) && (ComboMode || HarassMode) &&
                ConfigValue<bool>("Misc.E.AA." + caster.ChampionName))
            {
                E.CastOnUnit(caster);
            }
        }

        private void TowerAttackOnCreate(GameObject sender, EventArgs args)
        {
            if (!E.IsReady() || !ConfigValue<bool>("Misc.E.Tower"))
            {
                return;
            }

            if (sender.IsValid<Obj_SpellMissile>() && !IsUltChanneling)
            {
                var missile = (Obj_SpellMissile) sender;

                // Ally Turret -> Enemy Hero
                if (missile.SpellCaster.IsValid<Obj_AI_Turret>() && missile.SpellCaster.IsAlly &&
                    missile.Target.IsValid<Obj_AI_Hero>() && missile.Target.IsEnemy)
                {
                    var turret = (Obj_AI_Turret) missile.SpellCaster;

                    if (E.IsInRange(turret))
                    {
                        E.CastOnUnit(turret);
                    }
                }
            }
        }

        public override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (gapcloser.Sender.IsAlly)
            {
                return;
            }

            if (Q.CastCheck(gapcloser.Sender, "Gapcloser.Q"))
            {
                var pred = Q.GetPrediction(gapcloser.Sender);
                if (pred.Hitchance >= HitChance.Medium)
                {
                    Q.Cast(pred.CastPosition);
                    Q.Cast();
                }
            }

            if (W.CastCheck(gapcloser.Sender, "Gapcloser.W"))
            {
                W.CastOnUnit(gapcloser.Sender);
            }
        }

        public override void OnPossibleToInterrupt(Obj_AI_Hero target, Interrupter2.InterruptableTargetEventArgs args)
        {
            if ((args.DangerLevel < Interrupter2.DangerLevel.High && target.IsAlly))
            {
                return;
            }

            if (Q.CastCheck(target, "Interrupt.Q"))
            {
                var pred = Q.GetPrediction(target);
                if (pred.Hitchance >= HitChance.Medium)
                {
                    Q.Cast(pred.CastPosition);
                    Q.Cast();
                    LastQInterrupt = Environment.TickCount;
                    return;
                }
            }

            if (!Q.IsReady() && Environment.TickCount - LastQInterrupt > 500 && R.CastCheck(target, "Interrupt.R"))
            {
                R.Cast();
            }
        }

        public override void ComboMenu(Menu config)
        {
            config.AddBool("Combo.Q", "使用 Q", true);
            config.AddBool("Combo.W", "使用 W", true);
            config.AddBool("Combo.R", "使用 R", true);
            config.AddSlider("Combo.R.Health", "Health to Ult", 15, 1, 100);
        }

        public override void HarassMenu(Menu config)
        {
            config.AddBool("Harass.W", "使用 W", true);
        }

        public override void MiscMenu(Menu config)
        {
            config.AddBool("Misc.E.Tower", "在塔下使用E", true);

            // build aa menu
            var aa = config.AddSubMenu(new Menu("使用 E 攻击", "Misc.E.AA.Menu"));
            foreach (var hero in HeroManager.Allies.Where(h => !h.IsMe))
            {
                aa.AddBool("Misc.E.AA." + hero.ChampionName, hero.ChampionName, true);
            }

            // build spell menu
            var dmg = config.AddSubMenu(new Menu("使用 E 的法术", "Misc.E.Spell.Menu"));
            foreach (var spell in
                HeroManager.Allies
                    .Where(h => !h.IsMe)
                    .SelectMany(hero => DamageBoostDatabase.Spells.Where(s => s.Champion == hero.ChampionName)))
            {
                dmg.AddSlider("Misc.E.Spell." + spell.Spell, spell.Champion + " " + spell.Slot, spell.Priority, 0, 3);
            }
        }

        public override void ManaMenu(Menu config)
        {
            config.AddSlider("Mana.E.Priority.1", "E Priority 1", 65, 0, 100);
            config.AddSlider("Mana.E.Priority.2", "E Priority 2", 35, 0, 100);
            config.AddSlider("Mana.E.Priority.3", "E Priority 3", 10, 0, 100);
        }

        public override void InterruptMenu(Menu config)
        {
            config.AddBool("Gapcloser.Q", "使用 Q 防止突进", true);
            config.AddBool("Gapcloser.W", "使用 W 防止突进", true);

            config.AddBool("Interrupt.Q", "使用 Q 打断技能", true);
            config.AddBool("Interrupt.R", "使用 R 打断技能", true);
        }
    }
}