using System;
using LeagueSharp.Common;

namespace DZAIO.Utility.Spells
{
    interface ISummonerSpell
    {
        void OnLoad();
        String GetDisplayName();
        void AddToMenu(Menu menu);
        bool RunCondition();
        void Execute();
        SummonerSpell GetSummonerSpell();
        String GetName();
    }
}
