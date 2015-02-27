using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Common.Data;
using SharpDX;
using SH = FuckingAwesomeRiven.SpellHandler;

namespace FuckingAwesomeRiven
{
    class CheckHandler
    {

        public static int LastQ, LastQ2, LastW, LastE, LastAa, LastPassive, LastFr, LastTiamat, LastR2, LastECancelSpell, LastTiamatCancel;

        public static bool CanQ,
            CanW,
            CanE,
            CanR,
            CanAa,
            CanMove,
            CanSr,
            MidQ,
            MidW,
            MidE,
            MidAa,
            RState,
            BurstFinished;
        public static int PassiveStacks, QCount, FullComboState;

        public static void init()
        {
            CanAa = true;
            CanMove = true;
            CanQ = true;
            CanW = true;
            CanE = true;
            CanR = true;
            RState = false;

            LastQ = Environment.TickCount;
            LastQ2 = Environment.TickCount;
            LastW = Environment.TickCount;
            LastE = Environment.TickCount;
            LastAa = Environment.TickCount;
            LastPassive = Environment.TickCount;
            LastFr = Environment.TickCount;
            GameObject.OnCreate += GameObject_OnCreate;
        }

        static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            return;
        }

        public static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var spell = args.SData;

            if (!sender.IsMe)
                return;

            if (spell.Name == "ItemTiamatCleave")
            {
                LastTiamat = Environment.TickCount;
            }

            if (!MidQ && spell.Name.Contains("RivenBasicAttack"))
            {
                Queuer.remove("AA");
                if (MenuHandler.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.None &&  MenuHandler.Config.Item("QAA").GetValue<StringList>().SelectedIndex == 1 && MenuHandler.Config.Item("normalCombo").GetValue<KeyBind>().Active && SH._spells[SpellSlot.Q].IsReady() && MenuHandler.getMenuBool("CQ"))
                {
                    Queuer.add("Q");
                }
                LastAa = Environment.TickCount;
                LastTiamatCancel = Environment.TickCount + (int)ObjectManager.Player.AttackCastDelay;
                LastPassive = Environment.TickCount;
                if (PassiveStacks >= 1)
                {
                    PassiveStacks = PassiveStacks - 1;
                }
                MidAa = true;
                CanMove = false;
                CanAa = false;
            }

            if (spell.Name.Contains("RivenTriCleave"))
            {
                Queuer.remove("Q");
                LastQ = Environment.TickCount;
                LastPassive = Environment.TickCount;
                LastECancelSpell = Environment.TickCount + 50;
                if (PassiveStacks <= 2)
                {
                    PassiveStacks = PassiveStacks + 1;
                }

                if (QCount <= 1)
                {
                    LastQ2 = Environment.TickCount;
                    QCount = QCount + 1;
                }
                else if (QCount == 2)
                {
                    QCount = 0;
                }
                Utility.DelayAction.Add(350, Orbwalking.ResetAutoAttackTimer);
                Utility.DelayAction.Add(40, () => SH.animCancel(StateHandler.Target));
                
                MidQ = true;
                CanMove = false;
                CanQ = false;
                FullComboState = 0;
                BurstFinished = true;
            }

            if (spell.Name.Contains("RivenMartyr"))
            {
                Queuer.remove("W");
                Utility.DelayAction.Add(40, () => SH.animCancel(StateHandler.Target));
                LastW = Environment.TickCount;
                LastPassive = Environment.TickCount;
                LastECancelSpell = Environment.TickCount + 50;
                LastTiamatCancel = Environment.TickCount + (int)ObjectManager.Player.AttackCastDelay;
                if (LastPassive <= 2)
                {
                    PassiveStacks = PassiveStacks + 1;
                }
                MidW = true;
                CanW = false;
                FullComboState = 2;
            }

            if (spell.Name.Contains("RivenFeint"))
            {
                Queuer.remove("E");
                Queuer.EPos = new Vector3();
                LastE = Environment.TickCount;
                PassiveStacks = Environment.TickCount;
                LastTiamatCancel = Environment.TickCount + 50;

                if (LastPassive <= 2)
                {
                    PassiveStacks = PassiveStacks + 1;
                }

                MidE = true;
                CanE = false;
            }

            if (spell.Name.Contains("RivenFengShuiEngine"))
            {
                Queuer.remove("R");
                if (MenuHandler.Config.Item("autoCancelR1").GetValue<bool>()) Queuer.add("E", Game.CursorPos);
                LastFr = Environment.TickCount;
                LastPassive = Environment.TickCount;
                LastECancelSpell = Environment.TickCount + 50;

                if (PassiveStacks <= 2)
                {
                    PassiveStacks = PassiveStacks + 1;
                }

                RState = true;
                FullComboState = 1;
            }

            if (spell.Name.Contains("rivenizunablade"))
            {
                Queuer.remove("R2");
                if (MenuHandler.Config.Item("autoCancelR1").GetValue<bool>()) Queuer.add("Q");
                Queuer.R2Target = null;
                LastPassive = Environment.TickCount;

                if (PassiveStacks <= 2)
                {
                    PassiveStacks = PassiveStacks + 1;
                }
                LastR2 = Environment.TickCount;
                RState = false;
                CanSr = false;
                FullComboState = 3;
            }
        }

        public static void Checks()
        {
            if (MidQ && Environment.TickCount - LastQ >= (ObjectManager.Player.AttackCastDelay * 1000) - Game.Ping/2 - 200)
            {
                MidQ = false;
                CanMove = true;
                CanAa = true;
            }

            if (MidW && Environment.TickCount - LastW >= 266.7)
            {
                MidW = false;
                CanMove = true;
            }

            if (MidE && Environment.TickCount - LastE >= 500)
            {
                MidE = false;
                CanMove = true;
            }

            if (PassiveStacks != 0 && Environment.TickCount - LastPassive >= 5000)
            {
                PassiveStacks = 0;
            }

            if (QCount != 0 && Environment.TickCount - LastQ >= 4000)
            {
                QCount = 0;
            }

            if (!CanW && !(MidAa || MidQ || MidE) && SH._spells[SpellSlot.W].IsReady())
            {
                CanW = true;
            }

            if (!CanE && !(MidAa || MidQ || MidW) && SH._spells[SpellSlot.E].IsReady())
            {
                CanE = true;
            }

            if (RState && Environment.TickCount - LastFr >= 15000)
            {
                RState = false;
            }

            if (MidAa && Environment.TickCount >= LastAa + ObjectManager.Player.AttackCastDelay * 1000 + Game.Ping / 3 + MenuHandler.Config.Item("bonusCancelDelay").GetValue<Slider>().Value)
            {
                CanMove = true;
                CanQ = true;
                CanW = true;
                CanE = true;
                CanSr = true;
                MidAa = false;
            }
            if (!(MidAa || MidQ || MidE || MidW) &&
                Environment.TickCount + Game.Ping / 2 >= LastAa + ObjectManager.Player.AttackCastDelay * 1000)
            {
                CanMove = true;
            }

            if (!CanAa && !(MidQ || MidE || MidW) &&
                Environment.TickCount + Game.Ping / 2 + 25 >= LastAa + ObjectManager.Player.AttackDelay * 1000)
            {
                CanAa = true;
            }
        }

    }
}
