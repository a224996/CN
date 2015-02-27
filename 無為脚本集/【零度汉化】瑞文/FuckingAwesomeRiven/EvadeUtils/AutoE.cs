using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace FuckingAwesomeRiven.EvadeUtils
{
    class AutoE
    {
        public static void init()
        {
            var poop = MenuHandler.Config.AddSubMenu(new Menu("自动 E", "AutoE"));
            poop.AddItem(new MenuItem("EnabledautoE", "启用").SetValue(false));
            poop.AddItem(new MenuItem("donteCC", "禁用对硬控").SetValue(true));
            foreach (var champions in ObjectManager.Get<Obj_AI_Hero>().Where(a => a.IsEnemy))
            {
                var afdsdf = poop.AddSubMenu(new Menu(champions.ChampionName, champions.ChampionName + "kek"));
                foreach (var s in TargetSpellDatabase.Spells)
                {
                    if (champions.ChampionName.ToLower() == s.ChampionName && s.Type != SpellType.Self && s.Type != SpellType.AutoAttack)
                    {
                        afdsdf.AddItem(new MenuItem(s.Name, s.Name + " - " + s.Spellslot).SetValue(true));
                    }
                }
            }
        }
        public static void autoE(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsValidTarget() || !sender.IsChampion(sender.BaseSkinName) || sender.Distance(ObjectManager.Player) > 1500 || !SpellHandler._spells[LeagueSharp.SpellSlot.E].IsReady() || args.SData.IsAutoAttack()) return;
            
            var sData = TargetSpellDatabase.GetByName(args.SData.Name);

            if (MenuHandler.Config.Item(sData.Name) == null)
            {
                Console.WriteLine("Menu missing");
                return;
            }

            if ((MenuHandler.Config.Item("donteCC").GetValue<bool>() && sData.Type != SpellType.AutoAttack && sData.CcType != CcType.No) ||
                !MenuHandler.Config.Item(sData.Name).GetValue<bool>() || sData.Type == SpellType.Self) return;



            if (sData.Type == SpellType.Skillshot)
            {
                var sShot = SpellDatabase.GetByName(args.SData.Name);
                if (sShot.Type == SkillShotType.SkillshotMissileLine || sShot.Type == SkillShotType.SkillshotLine)
                {
                    var value = sShot.Range/sShot.Radius;
                    for (int i = 0; i < value; i++)
                    {
                        var vector = sender.Position.Extend(args.End, (i*sShot.Radius));
                        if (ObjectManager.Player.Distance(vector) < sShot.Radius)
                        {
                            SpellHandler.CastE((MenuHandler.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.None)
                            ? (StateHandler.Target.IsValidTarget() ? StateHandler.Target.Position : Game.CursorPos)
                            : Game.CursorPos);
                            break;
                        }
                    }
                }
                if (sShot.Type == SkillShotType.SkillshotCircle)
                {
                    if (sShot.MissileSpeed == int.MaxValue) return;
                    if (ObjectManager.Player.Position.Distance(args.End) < sShot.Radius)
                    {
                        SpellHandler.CastE((MenuHandler.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.None)
                            ? (StateHandler.Target.IsValidTarget() ? StateHandler.Target.Position : Game.CursorPos)
                            : Game.CursorPos);
                    }
                }
                if (args.End.Distance(ObjectManager.Player.Position) < 100)
                {
                    SpellHandler.CastE((MenuHandler.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.None)
                        ? (StateHandler.Target.IsValidTarget() ? StateHandler.Target.Position : Game.CursorPos)
                        : Game.CursorPos);
                    return;
                }
            }


            if (sData.Type == SpellType.Targeted && args.Target.IsMe)
            {
                SpellHandler.CastE((MenuHandler.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.None)
                    ? (StateHandler.Target.IsValidTarget() ? StateHandler.Target.Position : Game.CursorPos)
                    : Game.CursorPos);
            }
        }
    }
}
