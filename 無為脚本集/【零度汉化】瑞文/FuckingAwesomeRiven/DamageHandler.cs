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

        public static double getComboDmg(bool useR, Obj_AI_Hero target)
        {
            if (!target.IsValidTarget()) return 0;
            double dmg = 0;
            double baseAD = ObjectManager.Player.BaseAttackDamage;
            double bonusAD = ObjectManager.Player.FlatPhysicalDamageMod + (useR && !CheckHandler.RState && SpellHandler._spells[SpellSlot.R].IsReady()
                ? 0.2*(ObjectManager.Player.BaseAttackDamage + ObjectManager.Player.FlatPhysicalDamageMod)
                : 0);
            int passiveCount = 0;


            if (SpellHandler._spells[SpellSlot.Q].IsReady())
            {
                dmg += (3 - CheckHandler.QCount) * (new double[] { 10, 30, 50, 70, 90 }[SpellHandler._spells[SpellSlot.Q].Level - 1] +
                                ((baseAD + bonusAD) / 100) *
                                new double[] { 40, 45, 50, 55, 60 }[SpellHandler._spells[SpellSlot.Q].Level -1]);
                passiveCount += 3 - CheckHandler.QCount;
            }


            if (SpellHandler._spells[SpellSlot.W].IsReady())
            {
                dmg += (new double[] {50, 80, 110, 140, 170}[SpellHandler._spells[SpellSlot.W].Level -1 ]) + 1 * bonusAD;
                passiveCount++;
            }


            if (SpellHandler._spells[SpellSlot.E].IsReady())
            {
                passiveCount++;
            }
            if (SpellHandler._spells[SpellSlot.R].IsReady() && useR)
            {
                passiveCount++;
            }


            dmg += passiveCount*
                   (5 +
                    Math.Max(5*Math.Floor((double) ((ObjectManager.Player.Level + 2)/3)) + 10,
                        10*Math.Floor((double) ((ObjectManager.Player.Level + 2)/3)) - 15)*(baseAD + bonusAD)/100);

            dmg += (baseAD + bonusAD)*passiveCount;


            if (useR && SpellHandler._spells[SpellSlot.R].IsReady())
            {
                var targethp = (target.MaxHealth - target.Health > 0) ? target.MaxHealth - target.Health : 1;
                dmg += new double[] { 80, 120, 160 }[SpellHandler._spells[SpellSlot.R].Level - 1] + 0.6 * bonusAD
                * ((targethp)/target.MaxHealth*2.67 + 1);
            }

            dmg += (baseAD + bonusAD);

            return ObjectManager.Player.CalcDamage(target, Damage.DamageType.Physical, dmg);
        }

        public static double rBonus {get {return (ObjectManager.Player.FlatPhysicalDamageMod + ObjectManager.Player.BaseAttackDamage) * 0.2;}}

        public static double passiveDamage(Obj_AI_Base target, bool calcR)
        {
            return ((20 + ((Math.Floor((double) ObjectManager.Player.Level / 3)) * 5)) / 100) *
                (ObjectManager.Player.BaseAttackDamage + ObjectManager.Player.FlatPhysicalDamageMod + (calcR ? rBonus : 0));
        }
        public static double qDamage(Obj_AI_Base target, bool calcR)
        {
            return 
                new double[] { 10, 30, 50, 70, 90 }[SpellHandler._spells[SpellSlot.Q].Level] + ((ObjectManager.Player.BaseAttackDamage + ObjectManager.Player.FlatPhysicalDamageMod + (calcR ? rBonus : 0)) / 100) *
                                new double[] { 40, 45, 50, 55, 60 }[SpellHandler._spells[SpellSlot.Q].Level];
        }
        public static double wDamage(Obj_AI_Base target, bool calcR)
        {
            return 
                new double[] { 50, 80, 110, 140, 170 }[SpellHandler._spells[SpellSlot.W].Level] + 1 * ObjectManager.Player.FlatPhysicalDamageMod + (calcR ? rBonus : 0);
        }
        public static double rDamage(Obj_AI_Base target, int healthMod = 0)
        {
            var health = (target.MaxHealth - (target.Health - healthMod)) > 0 ? (target.MaxHealth - (target.Health - healthMod)) : 1;
            return
                    (new double[] { 80, 120, 160 }[SpellHandler._spells[SpellSlot.R].Level] + 0.6 * ObjectManager.Player.FlatPhysicalDamageMod) *
                                (health / target.MaxHealth * 2.67 + 1);
        }
    }
}
