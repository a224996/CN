using LeagueSharp;
using LeagueSharp.Common;

namespace Viktor
{
    /// <Credits:>
    /// Hellsing:
    /// Update Checker
    /// Healthpred 
    /// 
    /// blm95:
    /// DmgInd
    /// </Credits:>
    internal class Loader
    {
        private static void Main(string[] args)
        {
            if (args != null)
            {
                Utils.ClearConsole();

                CustomEvents.Game.OnGameLoad += eventArgs =>
                {
                    if (ObjectManager.Player.ChampionName != "Viktor")
                        return;

                    Spells.Init();
                    Config.Init();
                    Drawing.Init();
                    Mechanics.Init();
                    UpdateChecker.Initialize("Apollo16/LeagueSharp/tree/master/Viktor");
                    Notifications.AddNotification("Apollo's " + ObjectManager.Player.ChampionName + " Loaded", 5000);
                };
            }
        }
    }
}
