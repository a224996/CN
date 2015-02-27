using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using C = FuckingAwesomeRiven.CheckHandler;
using S = FuckingAwesomeRiven.SpellHandler;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Common.Data;
using SharpDX;

namespace FuckingAwesomeRiven
{
    class Queuer
    {
        public static List<String> Queue = new List<string>();

        public static void doQueue()
        {
            if (Queue.Count == 0) return;
            switch (Queue[0])
            {
                case "AA":
                    break;
                case "Q":
                    qQ();
                    break;
                case "W":
                    qW();
                    break;
                case "E":
                    qE();
                    break;
                case "R":
                    qR();
                    break;
                case "R2":
                    qR2();
                    break;
                case "Flash":
                    Flash();
                    break;
                case "Hydra":
                    Hydra();
                    break;
            }
        }

        public static Obj_AI_Base R2Target = null;
        public static Vector3 EPos = new Vector3();
        public static Vector3 FlashPos = new Vector3();

        public static void add(String spell)
        {
            Queue.Add(spell);
        }

        public static void add(String spell, Obj_AI_Base Target)
        {
            Queue.Add(spell);
            R2Target = Target;
        }

        public static void add(String spell, Vector3 pos, bool isFlash = false)
        {
            Queue.Add(spell);
            if (isFlash)
            {
                FlashPos = pos;
                return;
            }
            EPos = pos;
        }

        public static void remove(String spell)
        {
            if (Queue.Count == 0 || Queue[0] != spell) return;
            Queue.RemoveAt(0);
        }

        public static void qQ()
        {
            if (
                !(Environment.TickCount >=
                  C.LastAa + ObjectManager.Player.AttackCastDelay*1000 + Game.Ping/3 +
                  MenuHandler.Config.Item("bonusCancelDelay").GetValue<Slider>().Value)) return;
            if (!S._spells[SpellSlot.Q].IsReady() && Environment.TickCount > C.LastQ + 300)
            {
                Queue.Remove("Q");
                return;
            }
            if (S._spells[SpellSlot.Q].IsReady())
            {
                S.CastQ(StateHandler.Target);
            }
        }
        public static void qW()
        {
            if (
                !(Environment.TickCount >=
                  C.LastAa + ObjectManager.Player.AttackCastDelay * 1000 + Game.Ping / 3 +
                  MenuHandler.Config.Item("bonusCancelDelay").GetValue<Slider>().Value)) return;
            if (!S._spells[SpellSlot.W].IsReady())
            {
                Queue.Remove("W");
                return;
            }
            if (S._spells[SpellSlot.W].IsReady())
            {
                S.CastW(StateHandler.Target);
            }
        }
        public static void qE()
        {
            if (
                !(Environment.TickCount >=
                  C.LastAa + ObjectManager.Player.AttackCastDelay * 1000 + Game.Ping / 3 +
                  MenuHandler.Config.Item("bonusCancelDelay").GetValue<Slider>().Value)) return;
            if (!S._spells[SpellSlot.E].IsReady() || !EPos.IsValid())
            {
                Queue.Remove("E");
                return;
            }
            if (S._spells[SpellSlot.E].IsReady())
            {
                S.CastE(StateHandler.Target.IsValidTarget() ? StateHandler.Target.Position : EPos);
            }
        }
        public static void qR()
        {
            if (
                !(Environment.TickCount >=
                  C.LastAa + ObjectManager.Player.AttackCastDelay * 1000 + Game.Ping / 3 +
                  MenuHandler.Config.Item("bonusCancelDelay").GetValue<Slider>().Value)) return;
            if (!S._spells[SpellSlot.Q].IsReady() || C.RState)
            {
                Queue.Remove("R");
                return;
            }
            if (S._spells[SpellSlot.R].IsReady())
            {
                S.CastR();
            }
        }
        public static void qR2()
        {
            if (
                !(Environment.TickCount >=
                  C.LastAa + ObjectManager.Player.AttackCastDelay * 1000 + Game.Ping / 3 +
                  MenuHandler.Config.Item("bonusCancelDelay").GetValue<Slider>().Value)) return;
            if (!S._spells[SpellSlot.R].IsReady() || R2Target == null)
            {
                Queue.Remove("R2");
                return;
            }
            if (S._spells[SpellSlot.R].IsReady() && C.RState && R2Target.IsValidTarget())
            {
                var r2 = new Spell(SpellSlot.R, 900);
                r2.SetSkillshot(0.25f, 45, 1200, false, SkillshotType.SkillshotCone);
                r2.Cast(R2Target);
            }
        }

        public static void Hydra()
        {
            if (
                !(Environment.TickCount >=
                  C.LastAa + ObjectManager.Player.AttackCastDelay * 1000 + Game.Ping / 3 +
                  MenuHandler.Config.Item("bonusCancelDelay").GetValue<Slider>().Value)) return;
            if (!ItemData.Tiamat_Melee_Only.GetItem().IsReady() &&
                !ItemData.Ravenous_Hydra_Melee_Only.GetItem().IsReady())
            {
                Queue.Remove("Hydra");
                return;
            }
            if (ItemData.Tiamat_Melee_Only.GetItem().IsReady() || ItemData.Ravenous_Hydra_Melee_Only.GetItem().IsReady())
            {
                ItemData.Tiamat_Melee_Only.GetItem().Cast();
                ItemData.Ravenous_Hydra_Melee_Only.GetItem().Cast();
            }
        }

        public static void Flash()
        {
            if (!S.SummonerDictionary[SpellHandler.summonerSpell.Flash].IsReady() || !FlashPos.IsValid())
            {
                Queue.Remove("Flash");
                return;
            }
            SpellHandler.castFlash(FlashPos);
        }
    }
}
