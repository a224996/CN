using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace FuckingAwesomeThresh
{
    static class CheckHandler
    {
        public static Dictionary<SpellSlot, Spell> Spells = new Dictionary<SpellSlot, Spell>()
        {
            {SpellSlot.Q, new Spell(SpellSlot.Q, 1025)},
            {SpellSlot.W, new Spell(SpellSlot.W, 950)},
            {SpellSlot.E, new Spell(SpellSlot.E, 400)},
            {SpellSlot.R, new Spell(SpellSlot.R, 400)}
        };

        public static int qTimer;

        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        public static void Init()
        {
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            GameObject.OnCreate += GameObject_OnCreate;
        }

        static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (sender.Position.Distance(Player.Position) > 1200)
                return;
            if (sender.Name == "Thresh_Base_Q_stab_tar.troy")
            {
                qTimer = Environment.TickCount + 1500;
            }
        }

        static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe)
                return;
            if (args.SData.Name.ToLower().Contains("summonerflash"))
            {
                var target = TargetSelector.GetTarget(400, TargetSelector.DamageType.Physical);
                if (target == null)
                    return;
                var pos = target.Position.Extend(sender.Position, sender.Distance(target) + 200);
                Spells[SpellSlot.E].Cast(pos);
            }
        }

        public static String ThreshQBuff = "threshqfakeknockup";

        public static bool HasAntiCc(this Obj_AI_Base u)
        {
            return (u.HasBuff("BlackShield") || u.HasBuff("SivirE") || u.HasBuff("NocturneShroudofDarkness"));
        }

        public static Obj_AI_Base GetQUnit()
        {
            return ObjectManager.Get<Obj_AI_Base>().FirstOrDefault(a => a.IsEnemy && a.Distance(ObjectManager.Player) < 3000 && a.CheckBuff(ThreshQBuff));
        }

        public static BuffInstance GetBuffData(this Obj_AI_Base u, String s)
        {
            return u.Buffs.FirstOrDefault(a => a.Name == s || a.DisplayName == s);
        }

        public static bool CheckBuff(this Obj_AI_Base u, String s)
        {
            return u.HasBuff(s) || u.HasBuff(s, true);
        }
    }
}
