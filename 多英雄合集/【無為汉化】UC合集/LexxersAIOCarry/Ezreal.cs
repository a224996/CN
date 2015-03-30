using System;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace UltimateCarry
{
	class Ezreal : Champion
	{
		public Spell Q;
		public Spell W;
		public Spell E;
		public Spell R;

		public Ezreal()
		{
			LoadMenu();
			LoadSpells();

			Drawing.OnDraw += Drawing_OnDraw;
			Game.OnUpdate += Game_OnGameUpdate;
			PluginLoaded();
		}

		private void LoadMenu()
		{
			Program.Menu.AddSubMenu(new Menu("团队作战", "TeamFight"));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useQ_TeamFight", "使用 Q").SetValue(true));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useW_TeamFight", "使用 W").SetValue(true));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useR_TeamFight", "使用 R").SetValue(true));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("minimumRRange_Teamfight", "R 最小范围.").SetValue(new Slider(500, 900, 0)));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("minimumRHit_Teamfight", "R 最少击中.").SetValue(new Slider(2, 5, 1)));

			Program.Menu.AddSubMenu(new Menu("骚扰", "Harass"));
			Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useQ_Harass", "使用 Q").SetValue(true));
			Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useW_Harass", "使用 W").SetValue(true));
			AddManaManager("Harass", 40);

			Program.Menu.AddSubMenu(new Menu("清兵", "LaneClear"));
			Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("useQ_LaneClear_minion", "使用 Q 小兵").SetValue(true));
			Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("useQ_LaneClear_enemy", "使用 Q 英雄").SetValue(true));
			AddManaManager("LaneClear", 20);

			Program.Menu.AddSubMenu(new Menu("补兵", "LastHit"));
			Program.Menu.SubMenu("补兵").AddItem(new MenuItem("useQ_LastHit", "使用 Q").SetValue(true));
			AddManaManager("LastHit", 60);

			Program.Menu.AddSubMenu(new Menu("绘制", "Drawing"));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Disabled", "禁用全部").SetValue(false));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Q", "绘制 Q").SetValue(true));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_W", "绘制 W").SetValue(true));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_E", "绘制 E").SetValue(true));
	
		}

		private void LoadSpells()
		{
			Q = new Spell(SpellSlot.Q, 1200);
			Q.SetSkillshot(0.25f, 60f, 2000f, true, SkillshotType.SkillshotLine);

			W = new Spell(SpellSlot.W, 1050);
			W.SetSkillshot(0.25f, 80f, 2000f, false, SkillshotType.SkillshotLine);

			E = new Spell(SpellSlot.E, 475);
			E.SetSkillshot(0.25f, 80f, 1600f, false, SkillshotType.SkillshotCircle);

			R = new Spell(SpellSlot.R, 3000);
			R.SetSkillshot(1f, 160f, 2000f, false, SkillshotType.SkillshotLine);

		}

		private void Game_OnGameUpdate(EventArgs args)
		{
			switch(Program.Orbwalker.ActiveMode)
			{
				case Orbwalking.OrbwalkingMode.Combo:
					if (Program.Menu.Item("useQ_TeamFight").GetValue<bool>())
						Cast_BasicLineSkillshot_Enemy(Q);
					if(Program.Menu.Item("useW_TeamFight").GetValue<bool>())
                        Cast_BasicLineSkillshot_Enemy(W, TargetSelector.DamageType.Magical);
					if(Program.Menu.Item("useR_TeamFight").GetValue<bool>())
						CastREnemy();
					break;
				case Orbwalking.OrbwalkingMode.Mixed:
					if(Program.Menu.Item("useQ_Harass").GetValue<bool>())
						Cast_BasicLineSkillshot_Enemy(Q);
					if(Program.Menu.Item("useW_Harass").GetValue<bool>())
                        Cast_BasicLineSkillshot_Enemy(W, TargetSelector.DamageType.Magical);
					break;
				case Orbwalking.OrbwalkingMode.LaneClear:
					if(Program.Menu.Item("useQ_LaneClear_enemy").GetValue<bool>())
						Cast_BasicLineSkillshot_Enemy(Q);
					if(Program.Menu.Item("useQ_LaneClear_minion").GetValue<bool>())
						Cast_Basic_Farm(Q,true);
					break;
				case Orbwalking.OrbwalkingMode.LastHit:
					if(Program.Menu.Item("useQ_LastHit").GetValue<bool>())
						Cast_Basic_Farm(Q,true);
					break;
			}
		}

		private void Drawing_OnDraw(EventArgs args)
		{
			if(Program.Menu.Item("Draw_Disabled").GetValue<bool>())
				return;

			if(Program.Menu.Item("Draw_Q").GetValue<bool>())
				if(Q.Level > 0)
					Utility.DrawCircle(ObjectManager.Player.Position, Q.Range, Q.IsReady() ? Color.Green : Color.Red);

			if(Program.Menu.Item("Draw_W").GetValue<bool>())
				if(W.Level > 0)
					Utility.DrawCircle(ObjectManager.Player.Position, W.Range, W.IsReady() ? Color.Green : Color.Red);

			if(Program.Menu.Item("Draw_E").GetValue<bool>())
				if(E.Level > 0)
					Utility.DrawCircle(ObjectManager.Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);
		}

		private void CastREnemy()
		{
			if(!R.IsReady())
				return;
			var minRange = Program.Menu.Item("minimumRRange_Teamfight").GetValue<Slider>().Value;
			var minHit = Program.Menu.Item("minimumRHit_Teamfight").GetValue<Slider>().Value;

            var target = TargetSelector.GetTarget(2000, TargetSelector.DamageType.Physical);
			if(target == null)
				return;
			if(target.Distance(ObjectManager.Player) >= minRange)
				R.CastIfWillHit(target, minHit - 1, Packets());
		}

	}
}
