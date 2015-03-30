using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace FuckingAwesomeLeeSinReborn
{
    class AutoSmite
    {
        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        private static bool _checkForSmite;

        public static double SmiteDmg()
        {
            int[] dmg =
            {
                20*Player.Level + 370, 30*Player.Level + 330, 40*+Player.Level + 240, 50*Player.Level + 100
            };
            return Player.GetSpellSlot(CheckHandler.SmiteSpellName()).IsReady() ? dmg.Max() : 0;
        }
        public static void Init()
        {
            Game.OnUpdate += args => Tick();
            Drawing.OnDraw += args => Draw();
        }
        private static void Tick()
        {
            if (!Program.Config.Item("smiteEnabled").GetValue<KeyBind>().Active)
                return;

            var selectedMinion = MinionManager.GetMinions(1100, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth)
                .FirstOrDefault(
                    minion =>
                        minion.Health > 0 && Program.Config.Item(minion.BaseSkinName) != null &&
                        Program.Config.Item(minion.BaseSkinName).GetValue<bool>());
            if (selectedMinion == null)
            {
                return;
            }
            if (SmiteDmg() >= selectedMinion.Health && Player.Distance(selectedMinion) <= 700 || _checkForSmite && Player.Distance(selectedMinion) < 100)
            {
                Player.Spellbook.CastSpell(Player.GetSpellSlot(CheckHandler.SmiteSpellName()), selectedMinion);
                _checkForSmite = false;
            }
            if (!CheckHandler.spells[SpellSlot.Q].IsReady())
                return;

            if (selectedMinion.HasQBuff() && CheckHandler.Q2Damage(selectedMinion, (float) SmiteDmg(), true) + SmiteDmg() > selectedMinion.Health && !CheckHandler.QState)
            {
                CheckHandler.spells[SpellSlot.Q].Cast();
                _checkForSmite = true;
            }
            if (CheckHandler.Q2Damage(selectedMinion, (float)SmiteDmg() + CheckHandler.spells[SpellSlot.Q].GetDamage(selectedMinion), true) + SmiteDmg() + CheckHandler.spells[SpellSlot.Q].GetDamage(selectedMinion) > selectedMinion.Health && CheckHandler.QState)
            {
                CheckHandler.spells[SpellSlot.Q].Cast(selectedMinion);
            }
        }
        private static void Draw()
        {
            if (!Program.Config.Item("smiteEnabled").GetValue<KeyBind>().Active || !Program.Config.Item("DS").GetValue<Circle>().Active)
                return;
            var lowFps = Program.Config.Item("LowFPS").GetValue<bool>();
            var lowFpsMode = Program.Config.Item("LowFPSMode").GetValue<StringList>().SelectedIndex + 1;
            Render.Circle.DrawCircle(Player.Position, 700, Program.Config.Item("DS").GetValue<Circle>().Color, lowFps ? lowFpsMode : 5);
        }
    }
}
