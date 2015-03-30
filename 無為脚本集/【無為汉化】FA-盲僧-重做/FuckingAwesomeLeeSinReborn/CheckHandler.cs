using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Common.Data;
using SharpDX;

namespace FuckingAwesomeLeeSinReborn
{
    static class CheckHandler
    {
        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        public static int LastQ, LastQ2, LastW, LastW2, LastE, LastE2, LastR, LastWard, LastSpell, PassiveStacks;
        public static bool CheckQ = true;
        public static Dictionary<SpellSlot, Spell> spells = new Dictionary<SpellSlot, Spell>()
        {
            {SpellSlot.Q, new Spell(SpellSlot.Q, 1100)}, {SpellSlot.W, new Spell(SpellSlot.W, 700)}, {SpellSlot.E, new Spell(SpellSlot.E, 430)}, {SpellSlot.R, new Spell(SpellSlot.R, 375)}
        }; 
        public static bool QState { get { return spells[SpellSlot.Q].Instance.Name == "BlindMonkQOne"; } }
        public static bool WState { get { return spells[SpellSlot.W].Instance.Name == "BlindMonkWOne"; } }
        public static bool EState { get { return spells[SpellSlot.E].Instance.Name == "BlindMonkEOne"; } }

        public static void Init()
        {
            GameObject.OnDelete += Obj_AI_Hero_OnCreate;
            GameObject.OnCreate += GameObject_OnCreate;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
        }

        static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
        }

        static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (unit.IsMe)
            {
                if (PassiveStacks == 0)
                    return;
                PassiveStacks = PassiveStacks - 1;
            }
        }

        public static readonly int[] SmitePurple = { 3713, 3726, 3725, 3726, 3723 };
        public static readonly int[] SmiteGrey = { 3711, 3722, 3721, 3720, 3719 };
        public static readonly int[] SmiteRed = { 3715, 3718, 3717, 3716, 3714 };
        public static readonly int[] SmiteBlue = { 3706, 3710, 3709, 3708, 3707 };

        public static string SmiteSpellName()
        {
            if (SmiteBlue.Any(a => Items.HasItem(a)))
            {
                return "s5_summonersmiteplayerganker";
            }
            if (SmiteRed.Any(a => Items.HasItem(a)))
            {
                return "s5_summonersmiteduel";
            }
            if (SmiteGrey.Any(a => Items.HasItem(a)))
            {
                return "s5_summonersmitequick";
            }
            if (SmitePurple.Any(a => Items.HasItem(a)))
            {
                return "itemsmiteaoe";
            }
            return "summonersmite";
        }

        public static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe)
                return;
            if (args.SData.Name.ToLower().Contains("ward") || args.SData.Name.ToLower().Contains("totem"))
            {
                LastWard = Environment.TickCount;
            }
            switch (args.SData.Name)
            {
                case "BlindMonkQOne":
                    LastQ = Environment.TickCount;
                    LastSpell = Environment.TickCount;
                    PassiveStacks = 2;
                    break;
                case "BlindMonkWOne":
                    LastW = Environment.TickCount;
                    LastSpell = Environment.TickCount;
                    PassiveStacks = 2;
                    break;
                case "BlindMonkEOne":
                    LastE = Environment.TickCount;
                    LastSpell = Environment.TickCount;
                    PassiveStacks = 2;
                    break;
                case "blindmonkqtwo":
                    LastQ2 = Environment.TickCount;
                    LastSpell = Environment.TickCount;
                    PassiveStacks = 2;
                    CheckQ = false;
                    break;
                case "blindmonkwtwo":
                    LastW2 = Environment.TickCount;
                    LastSpell = Environment.TickCount;
                    PassiveStacks = 2;
                    break;
                case "blindmonketwo":
                    LastQ = Environment.TickCount;
                    LastSpell = Environment.TickCount;
                    PassiveStacks = 2;
                    break;
                case "BlindMonkRKick":
                    LastR = Environment.TickCount;
                    LastSpell = Environment.TickCount;
                    PassiveStacks = 2;
                    if (InsecHandler.FlashR)
                    {
                        Player.Spellbook.CastSpell(Player.GetSpellSlot("summonerflash"), InsecHandler.FlashPos);
                        InsecHandler.FlashPos = new Vector3();
                        InsecHandler.FlashR = false;
                    }
                    break;
            }
        }

        static void Obj_AI_Hero_OnCreate(GameObject sender, EventArgs args)
        {
            if (sender.Position.Distance(Player.Position) > 200) return;
            if (sender.Name == "blindMonk_Q_resonatingStrike_tar_blood.troy")
            {
                CheckQ = true;
            }
        }

        public static double Q2Damage(Obj_AI_Base target, float subHp = 0, bool monster = false)
        {
            var damage = (50 + (spells[SpellSlot.Q].Level * 30)) + (0.09 * Player.FlatPhysicalDamageMod) + ((target.MaxHealth - (target.Health - subHp)) * 0.08);
            if (monster && damage > 400) return Player.CalcDamage(target, Damage.DamageType.Physical, 400);
            return Player.CalcDamage(target, Damage.DamageType.Physical, damage);
        }

        public static bool HasQBuff(this Obj_AI_Base unit)
        {
            return (unit.HasBuff("BlindMonkQOne", true) || unit.HasBuff("blindmonkqonechaos", true));
        }
        public static bool HasEBuff(this Obj_AI_Base unit)
        {
            return (unit.HasBuff("BlindMonkEOne", true) || unit.HasBuff("BlindMonkEOne"));
        }

        public static Obj_AI_Base BuffedEnemy { get
        {
            return ObjectManager.Get<Obj_AI_Base>().FirstOrDefault(unit => unit.IsEnemy && unit.HasQBuff());
        } }

        public static void UseItems(Obj_AI_Base target, bool minions = false)
        {
            if (ItemData.Ravenous_Hydra_Melee_Only.GetItem().IsReady() &&
                ItemData.Ravenous_Hydra_Melee_Only.Range > Player.Distance(target))
            {
                ItemData.Ravenous_Hydra_Melee_Only.GetItem().Cast();
            }
            if (ItemData.Tiamat_Melee_Only.GetItem().IsReady() &&
                ItemData.Tiamat_Melee_Only.Range > Player.Distance(target))
            {
                ItemData.Tiamat_Melee_Only.GetItem().Cast();
            }
            if(minions) return;
            if (ItemData.Blade_of_the_Ruined_King.GetItem().IsReady() &&
                ItemData.Blade_of_the_Ruined_King.Range > Player.Distance(target))
            {
                ItemData.Blade_of_the_Ruined_King.GetItem().Cast(target);
            }
            if (ItemData.Youmuus_Ghostblade.GetItem().IsReady() &&
                Orbwalking.GetRealAutoAttackRange(Player) > Player.Distance(target))
            {
                ItemData.Youmuus_Ghostblade.GetItem().Cast();
            }
        }
    }
}
