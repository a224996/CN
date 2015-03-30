using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace FuckingAwesomeLeeSinReborn
{
    class StateHandler
    {
        public static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        public static void Combo()
        {
            var target = TargetSelector.GetTarget(1200, TargetSelector.DamageType.Physical);

            if (!target.IsValidTarget())
                return;

            var useQ = Program.Config.Item("CQ").GetValue<bool>();
            var useE = Program.Config.Item("CE").GetValue<bool>();
            var useR = Program.Config.Item("CR").GetValue<bool>();
            var forcePassive = Program.Config.Item("CpassiveCheck").GetValue<bool>();
            var minPassive = Program.Config.Item("CpassiveCheckCount").GetValue<Slider>().Value;

            CheckHandler.UseItems(target);

            if (useR && useQ && CheckHandler.spells[SpellSlot.R].IsReady() && CheckHandler.spells[SpellSlot.Q].IsReady() && (CheckHandler.QState || target.HasQBuff()) &&
                CheckHandler.spells[SpellSlot.R].GetDamage(target) + (CheckHandler.QState ? CheckHandler.spells[SpellSlot.Q].GetDamage(target) : 0) +
                CheckHandler.Q2Damage(target, CheckHandler.spells[SpellSlot.R].GetDamage(target) + (CheckHandler.QState ? CheckHandler.spells[SpellSlot.Q].GetDamage(target) : 0)) > target.Health)
            {
                if (CheckHandler.QState)
                {
                    CheckHandler.spells[SpellSlot.Q].CastIfHitchanceEquals(target, HitChance.High);
                    return;
                }
                CheckHandler.spells[SpellSlot.R].CastOnUnit(target);
                Utility.DelayAction.Add(300, () => CheckHandler.spells[SpellSlot.Q].Cast());
            }

            if (useR && CheckHandler.spells[SpellSlot.R].IsReady() &&
                CheckHandler.spells[SpellSlot.R].GetDamage(target) > target.Health)
            {
                CheckHandler.spells[SpellSlot.R].CastOnUnit(target);
                return;
            }

            if (useQ && !CheckHandler.QState && CheckHandler.spells[SpellSlot.Q].IsReady() && target.HasQBuff() && (CheckHandler.LastQ + 2700 < Environment.TickCount || CheckHandler.spells[SpellSlot.Q].GetDamage(target, 1) > target.Health || target.Distance(Player) > Orbwalking.GetRealAutoAttackRange(Player) + 50))
            {
                CheckHandler.spells[SpellSlot.Q].Cast();
                return;
            }

            if (forcePassive && CheckHandler.PassiveStacks > minPassive && Orbwalking.GetRealAutoAttackRange(Player) > Player.Distance(target))
                return;

            if (CheckHandler.spells[SpellSlot.Q].IsReady() && useQ)
            {
                if (CheckHandler.QState && target.Distance(Player) < CheckHandler.spells[SpellSlot.Q].Range)
                {
                    CastQ(target, Program.Config.Item("smiteQ").GetValue<bool>());
                    return;
                }
            }

            if (CheckHandler.spells[SpellSlot.E].IsReady() && useE)
            {
                if (CheckHandler.EState && target.Distance(Player) < CheckHandler.spells[SpellSlot.E].Range)
                {
                    CheckHandler.spells[SpellSlot.E].Cast();
                    return;
                }
                if (!CheckHandler.EState && target.Distance(Player) > Orbwalking.GetRealAutoAttackRange(Player) + 50)
                {
                    CheckHandler.spells[SpellSlot.E].Cast();
                }
            }
        }

        public static void StarCombo()
        {
            var target = TargetSelector.GetTarget(1200, TargetSelector.DamageType.Physical);
            if (target == null)
            {
                Orbwalking.Orbwalk(null, Game.CursorPos);
                return;
            }

            Orbwalking.Orbwalk(Orbwalking.InAutoAttackRange(target) ? target : null, Game.CursorPos);

            CheckHandler.UseItems(target);

            if (!target.IsValidTarget()) return;

            if (target.HasBuffOfType(BuffType.Knockback) && target.Distance(Player) > 300 && target.HasQBuff() && !CheckHandler.QState)
            {
                CheckHandler.spells[SpellSlot.Q].Cast();
                return;
            }

            if (!CheckHandler.spells[SpellSlot.R].IsReady()) return;

            if (CheckHandler.spells[SpellSlot.Q].IsReady() && CheckHandler.QState)
            {
                CastQ(target, Program.Config.Item("smiteQ").GetValue<bool>());
                return;
            }
            if (target.HasQBuff() && !target.HasBuffOfType(BuffType.Knockback))
            {
                if (target.Distance(Player) < CheckHandler.spells[SpellSlot.R].Range && CheckHandler.spells[SpellSlot.R].IsReady())
                {
                    CheckHandler.spells[SpellSlot.R].CastOnUnit(target);
                    return;
                }
                if (target.Distance(Player) < 600 && CheckHandler.WState)
                {
                    WardjumpHandler.Jump(Player.Position.Extend(target.Position, Player.Position.Distance(target.Position) - 50));
                }
            }
        }

        public static void Harass()
        {
            var target = TargetSelector.GetTarget(1200, TargetSelector.DamageType.Physical);

            if (!target.IsValidTarget())
                return;

            var useQ = Program.Config.Item("HQ").GetValue<bool>();
            var useE = Program.Config.Item("HE").GetValue<bool>();
            var forcePassive = Program.Config.Item("HpassiveCheck").GetValue<bool>();
            var minPassive = Program.Config.Item("HpassiveCheckCount").GetValue<Slider>().Value;


            if (!CheckHandler.QState && CheckHandler.LastQ + 200 < Environment.TickCount && useQ && !CheckHandler.QState && CheckHandler.spells[SpellSlot.Q].IsReady() && target.HasQBuff() && (CheckHandler.LastQ + 2700 < Environment.TickCount || CheckHandler.spells[SpellSlot.Q].GetDamage(target, 1) > target.Health || target.Distance(Player) > Orbwalking.GetRealAutoAttackRange(Player) + 50))
            {
                CheckHandler.spells[SpellSlot.Q].Cast();
                return;
            }

            if (forcePassive && CheckHandler.PassiveStacks > minPassive && Orbwalking.GetRealAutoAttackRange(Player) > Player.Distance(target))
                return;

            if (CheckHandler.spells[SpellSlot.Q].IsReady() && useQ && CheckHandler.LastQ + 200 < Environment.TickCount)
            {
                if (CheckHandler.QState && target.Distance(Player) < CheckHandler.spells[SpellSlot.Q].Range)
                {
                    CastQ(target);
                    return;
                }
            }

            if (CheckHandler.spells[SpellSlot.E].IsReady() && useE && CheckHandler.LastE + 200 < Environment.TickCount)
            {
                if (CheckHandler.EState && target.Distance(Player) < CheckHandler.spells[SpellSlot.E].Range)
                {
                    CheckHandler.spells[SpellSlot.E].Cast();
                    return;
                }
                if (!CheckHandler.EState && target.Distance(Player) > Orbwalking.GetRealAutoAttackRange(Player) + 50)
                {
                    CheckHandler.spells[SpellSlot.E].Cast();
                }
            }
        }

        public static void Wave()
        {
            var target =
                MinionManager.GetMinions(1100)
                    .FirstOrDefault();

            if (!target.IsValidTarget() || target == null)
            {
                return;
            }


            CheckHandler.UseItems(target, true);

            var useQ = Program.Config.Item("QWC").GetValue<bool>();
            var useE = Program.Config.Item("EWC").GetValue<bool>();

            if (useQ && !CheckHandler.QState && CheckHandler.spells[SpellSlot.Q].IsReady() && target.HasQBuff() && (CheckHandler.LastQ + 2700 < Environment.TickCount || CheckHandler.spells[SpellSlot.Q].GetDamage(target, 1) > target.Health || target.Distance(Player) > Orbwalking.GetRealAutoAttackRange(Player) + 50))
            {
                CheckHandler.spells[SpellSlot.Q].Cast();
                return;
            }

            if (CheckHandler.spells[SpellSlot.Q].IsReady() && useQ && CheckHandler.LastQ + 200 < Environment.TickCount)
            {
                if (CheckHandler.QState && target.Distance(Player) < CheckHandler.spells[SpellSlot.Q].Range)
                {
                    CheckHandler.spells[SpellSlot.Q].Cast(target);
                    return;
                }
            }

            if (CheckHandler.spells[SpellSlot.E].IsReady() && useE && CheckHandler.LastE + 200 < Environment.TickCount)
            {
                if (CheckHandler.EState && target.Distance(Player) < CheckHandler.spells[SpellSlot.E].Range)
                {
                    CheckHandler.spells[SpellSlot.E].Cast();
                }
            }
        }

        public static void JungleClear()
        {
            var target =
                MinionManager.GetMinions(1100, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth)
                    .FirstOrDefault();

            if (!target.IsValidTarget() || target ==  null)
            {
                Wave();
                return;
            }

            var useQ = Program.Config.Item("QJ").GetValue<bool>();
            var useW = Program.Config.Item("WJ").GetValue<bool>();
            var useE = Program.Config.Item("EJ").GetValue<bool>();

            CheckHandler.UseItems(target, true);

            if (CheckHandler.PassiveStacks > 0 || CheckHandler.LastSpell + 400 > Environment.TickCount)
                return;

            if (CheckHandler.spells[SpellSlot.Q].IsReady() && useQ)
            {
                if (CheckHandler.QState && target.Distance(Player) < CheckHandler.spells[SpellSlot.Q].Range && CheckHandler.LastQ + 200 < Environment.TickCount)
                {
                    CheckHandler.spells[SpellSlot.Q].Cast(target);
                    CheckHandler.LastSpell = Environment.TickCount;
                    return;
                }
                CheckHandler.spells[SpellSlot.Q].Cast();
                CheckHandler.LastSpell = Environment.TickCount;
                return;
            }

            if (CheckHandler.spells[SpellSlot.W].IsReady() && useW)
            {
                if (CheckHandler.WState && target.Distance(Player) < Orbwalking.GetRealAutoAttackRange(Player))
                {
                    CheckHandler.spells[SpellSlot.W].CastOnUnit(Player);
                    CheckHandler.LastSpell = Environment.TickCount;
                    return;
                }
                if (CheckHandler.WState)
                    return;
                CheckHandler.spells[SpellSlot.W].Cast();
                CheckHandler.LastSpell = Environment.TickCount;
                return;
            }

            if (CheckHandler.spells[SpellSlot.E].IsReady() && useE)
            {
                if (CheckHandler.EState && target.Distance(Player) < CheckHandler.spells[SpellSlot.E].Range)
                {
                    CheckHandler.spells[SpellSlot.E].Cast();
                    CheckHandler.LastSpell = Environment.TickCount;
                    return;
                }
                if (CheckHandler.EState)
                    return;
                CheckHandler.spells[SpellSlot.E].Cast();
                CheckHandler.LastSpell = Environment.TickCount;
            }
        }

        public static void CastQ(Obj_AI_Base target, bool smiteQ = false)
        {
            var qData = CheckHandler.spells[SpellSlot.Q].GetPrediction(target);
            if (CheckHandler.spells[SpellSlot.Q].IsReady() &&
                target.IsValidTarget(CheckHandler.spells[SpellSlot.Q].Range) && qData.Hitchance != HitChance.Collision)
            {
                CheckHandler.spells[SpellSlot.Q].CastIfHitchanceEquals(target, HitChance.High);
            }
            else if (CheckHandler.spells[SpellSlot.Q].IsReady() &&
                target.IsValidTarget(CheckHandler.spells[SpellSlot.Q].Range) && qData.CollisionObjects.Count(a => a.NetworkId != target.NetworkId && a.IsMinion) == 1 && Player.GetSpellSlot(CheckHandler.SmiteSpellName()).IsReady())
            {
                Player.Spellbook.CastSpell(Player.GetSpellSlot(CheckHandler.SmiteSpellName()), qData.CollisionObjects.Where(a => a.NetworkId != target.NetworkId && a.IsMinion).ToList()[0]);
                CheckHandler.spells[SpellSlot.Q].Cast(qData.CastPosition);
            }
        }
    }
}
