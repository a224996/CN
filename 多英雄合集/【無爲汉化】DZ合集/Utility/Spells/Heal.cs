using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DZAIO.Utility.Helpers;
using LeagueSharp;
using LeagueSharp.Common;

namespace DZAIO.Utility.Spells
{
    class Heal : ISummonerSpell
    {
        public void OnLoad(){}
        public string GetDisplayName()
        {
            return "Heal";
        }

        public void AddToMenu(Menu menu)
        {
            menu.AddItem(
                new MenuItem("daio.activator.summonerspells." + GetName() + ".hpercent", "Health %").SetValue(
                    new Slider(25, 1)));
        }

        public bool RunCondition()
        {
            return GetSummonerSpell().IsReady() &&
                   MenuHelper.isMenuEnabled("dzaio.activator.summonerspells." + GetName() + ".enabled") &&
                   ObjectManager.Player.HealthPercentage() <=
                   MenuHelper.getSliderValue("dzaio.activator.summonerspells." + GetName() + ".hpercent") &&
                   ObjectManager.Player.CountEnemiesInRange(ObjectManager.Player.AttackRange) >= 1;
        }

        public void Execute()
        {
                GetSummonerSpell().Cast();
        }

        public SummonerSpell GetSummonerSpell()
        {
            return SummonerSpells.Heal;
        }

        public string GetName()
        {
            return GetSummonerSpell().Names.First().ToLowerInvariant();
        }
    }
}
