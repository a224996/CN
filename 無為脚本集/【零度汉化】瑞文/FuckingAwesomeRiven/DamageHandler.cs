using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace FuckingAwesomeRiven
{
    class DamageHandler
    {
        public static double rBonus {get {return (ObjectManager.Player.FlatPhysicalDamageMod + ObjectManager.Player.BaseAttackDamage) * 0.2;}}

        public static double passiveDamage(Obj_AI_Base target, bool calcR)
        {
            return ObjectManager.Player.CalcDamage(target, Damage.DamageType.Physical , (20 + ((Math.Floor((double) ObjectManager.Player.Level / 3)) * 5) / 100) *
                (ObjectManager.Player.BaseAttackDamage + ObjectManager.Player.FlatPhysicalDamageMod + (calcR ? rBonus : 0)));
        }
        public static double qDamage(Obj_AI_Base target, bool calcR)
        {
            return ObjectManager.Player.CalcDamage(target, Damage.DamageType.Physical,
                new double[] { 10, 30, 50, 70, 90 }[SpellHandler._spells[SpellSlot.Q].Level] + ((ObjectManager.Player.BaseAttackDamage + ObjectManager.Player.FlatPhysicalDamageMod + (calcR ? rBonus : 0)) / 100) *
                                new double[] { 40, 45, 50, 55, 60 }[SpellHandler._spells[SpellSlot.Q].Level]);
        }
        public static double wDamage(Obj_AI_Base target, bool calcR)
        {
            return ObjectManager.Player.CalcDamage(target, Damage.DamageType.Physical,
                new double[] { 50, 80, 110, 140, 170 }[SpellHandler._spells[SpellSlot.W].Level] + 1 * ObjectManager.Player.FlatPhysicalDamageMod + (calcR ? rBonus : 0));
        }
        public static double rDamage(Obj_AI_Base target, int healthMod, bool calcR)
        {
            return
                ObjectManager.Player.CalcDamage(
                    target, Damage.DamageType.Physical,
                    new double[] { 80, 120, 160 }[SpellHandler._spells[SpellSlot.R].Level] + 0.6 * ObjectManager.Player.FlatPhysicalDamageMod) *
                ((target.MaxHealth - (target.Health - healthMod) / target.MaxHealth * 2.67 + 1));
        }
    }
}
