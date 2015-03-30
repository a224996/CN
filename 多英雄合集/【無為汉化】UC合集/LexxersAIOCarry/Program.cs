using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace UltimateCarry
{
	class Program
	{
		public const int LocalVersion = 81; //for update
        public const String Version = "2.0.*";

		public static Champion Champion;
		public static Menu Menu;
		public static Orbwalking.Orbwalker Orbwalker;
		public static Azir.Orbwalking.Orbwalker Azirwalker;
        public static Helper Helper;

		// ReSharper disable once UnusedParameter.Local
		private static void Main(string[] args)
		{
			CustomEvents.Game.OnGameLoad  += Game_OnGameLoad;
		}

		private static void Game_OnGameLoad(EventArgs args)
		{
			//AutoUpdater.InitializeUpdater();
			Chat.Print("Ultimate Carry Version by Lexxes updated by Bolsudo for patch 4.21 loading ...");
			Helper = new Helper();

			Menu = new Menu("【無為汉化】Lexxers合集", "UltimateCarry_" + ObjectManager.Player.ChampionName, true);

			var targetSelectorMenu = new Menu("目标选择器", "TargetSelector");
            TargetSelector.AddToMenu(targetSelectorMenu);
			Menu.AddSubMenu(targetSelectorMenu);
			if (ObjectManager.Player.ChampionName == "Azir")
			{
				var orbwalking = Menu.AddSubMenu(new Menu("皇帝走砍", "Orbwalking"));
				Azirwalker = new Azir.Orbwalking.Orbwalker(orbwalking);
				Menu.Item("FarmDelay").SetValue(new Slider(125, 100, 200));
			}
			else
			{
				var orbwalking = Menu.AddSubMenu(new Menu("走砍", "Orbwalking"));
				Orbwalker = new Orbwalking.Orbwalker(orbwalking);
				Menu.Item("FarmDelay").SetValue(new Slider(0, 0, 200));
			}
            var bushRevealer = new AutoBushRevealer();
		//var overlay = new Overlay();
		
			try
			{
				// ReSharper disable once AssignNullToNotNullAttribute
				var handle = System.Activator.CreateInstance(null, "UltimateCarry." + ObjectManager.Player.ChampionName);
				Champion = (Champion) handle.Unwrap();
			}
			// ReSharper disable once EmptyGeneralCatchClause
			catch (Exception)
			{
				//Champion = new Champion(); //Champ not supported
			}
					
			Menu.AddToMainMenu();
            Chat.Print("You have the latest version.");
            Chat.Print("Ultimate Carry loaded! (If something is not working please report it)");
		}
	}
}
