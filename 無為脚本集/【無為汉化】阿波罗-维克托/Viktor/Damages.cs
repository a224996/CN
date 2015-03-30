using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;

namespace Viktor
{
    internal class Damages
    {
        private static readonly Obj_AI_Hero Player = ObjectManager.Player;
        private static readonly Dictionary<SpellSlot, Spell> Spell = Spells.Spell;
        public static class Dmg
        {
            static readonly int[] QDmg = { 40, 60, 80, 100, 120 };
            static readonly int[] EDmg = { 70, 115, 160, 205, 250 };
            static readonly int[] RDmg = { 360, 670, 980 };
            static readonly int[] QaaDmg = { 20, 25, 30, 35, 40, 45, 50, 55, 60, 70, 80, 90, 110, 130, 150, 170, 190, 210 };
            public static float Q(Obj_AI_Base enemy)
            {
                return
                    (float)
                        Player.CalcDamage(
                            enemy, Damage.DamageType.Magical,
                            QDmg[Spell[SpellSlot.Q].Level] + (Player.TotalMagicalDamage * .2));
            }

            public static float Qaa(Obj_AI_Base enemy)
            {
                return
                    (float)
                        Player.CalcDamage(
                            enemy, Damage.DamageType.Magical,
                            QaaDmg[Player.Level] + (Player.TotalMagicalDamage * .5) + Player.TotalAttackDamage());
            }

            public static float E(Obj_AI_Base enemy)
            {
                var name = Player.Spellbook.GetSpell(SpellSlot.E).Name == "kek"; //todo get spell name
                var addDmg = name ? 1.4 : 1;
                return
                    ((float)
                        Player.CalcDamage(
                            enemy, Damage.DamageType.Magical,
                            EDmg[Spell[SpellSlot.E].Level] + (Player.TotalMagicalDamage * .7))) * (float)addDmg;
            }

            public static float R(Obj_AI_Base enemy)
            {
                return
                    (float)
                        Player.CalcDamage(
                            enemy, Damage.DamageType.Magical,
                            RDmg[Spell[SpellSlot.R].Level] + (Player.TotalMagicalDamage * 1.95));
            }
        }
        public static class ManaCost
        {
            static readonly int[] QMana = { 45, 45, 50, 55, 60, 65 };
            static readonly int[] WMana = { 65, 65, 65, 65, 65, 65 };
            static readonly int[] EMana = { 70, 70, 80, 90, 100, 110 };
            static readonly int[] RMana = { 100, 100, 100, 100 };

            public static float Q
            {
                get { return QMana[Spell[SpellSlot.Q].Level]; }
            }

            public static float W
            {
                get { return WMana[Spell[SpellSlot.W].Level]; }
            }

            public static float E
            {
                get { return EMana[Spell[SpellSlot.E].Level]; }
            }

            public static float R
            {
                get { return RMana[Spell[SpellSlot.R].Level]; }
            }
        }

        public static float ComboDmg(Obj_AI_Base enemy)
        {
            var dmg = 0d;
            var mana = Player.Mana;
            var usedMana = 0f;

            if (Spell[SpellSlot.Q].IsReady() && mana >= usedMana + ManaCost.Q)
            {
                dmg += Dmg.Q(enemy);
                dmg += Dmg.Qaa(enemy);
                usedMana += ManaCost.Q;
            }
            if (Spell[SpellSlot.E].IsReady() && mana >= usedMana + ManaCost.E)
            {
                dmg += Dmg.E(enemy);
                usedMana += ManaCost.E;
            }
            if (Spell[SpellSlot.R].IsReady() && mana >= usedMana + ManaCost.R)
            {
                dmg += Dmg.R(enemy);
            }
            if (Mechanics.IgniteSlot != SpellSlot.Unknown && Mechanics.IgniteSlot.IsReady())
            {
                dmg += Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);
            }

            return (float)dmg;
        }
    }
}
