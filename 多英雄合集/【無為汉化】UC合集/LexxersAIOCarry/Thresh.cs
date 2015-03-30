using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace UltimateCarry
{
	class Thresh : Champion
	{
		public Spell Q;
		public Spell W;
		public Spell E;
		public Spell R;

		public int QFollowTick = 0;
		public const int QFollowTime = 3000;
        public Thresh()
        {
			LoadMenu();
			LoadSpells();
			Game.OnUpdate += Game_OnGameUpdate;
			Drawing.OnDraw += Drawing_OnDraw;
			Game.OnSendPacket += Game_OnGameSendPacket;
			Interrupter.OnPossibleToInterrupt += Interrupter_OnPosibleToInterrupt;
			PluginLoaded();
		}

		private void LoadMenu()
		{
			Program.Menu.AddSubMenu(new Menu("团队作战", "TeamFight"));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useQ_TeamFight", "使用 Q").SetValue(true));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useQ_TeamFight_follow", "遵循 Q").SetValue(true));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useW_TeamFight_shield", "W 对于盾").SetValue(true));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useW_TeamFight_enagage", "W 对于英雄").SetValue(true));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useE_TeamFight", "E 给自己").SetValue(true));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useR_TeamFight", "使用 R 如果击中").SetValue(new Slider(2, 5, 0)));
			
			Program.Menu.AddSubMenu(new Menu("骚扰", "Harass"));
			Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useQ_Harass", "使用 Q").SetValue(true));
			Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useW_Harass_safe", "W 安全的朋友").SetValue(true));
			Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useE_Harass", "E 离开").SetValue(true));
			AddManaManager("Harass",50);

			Program.Menu.AddSubMenu(new Menu("清兵", "LaneClear"));
			Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("useE_LaneClear", "使用 E").SetValue(true));
			AddManaManager("LaneClear", 20);

			Program.Menu.AddSubMenu(new Menu("支持模式", "SupportMode"));
			Program.Menu.SubMenu("SupportMode").AddItem(new MenuItem("hitMinions", "击中小兵").SetValue(false));

			Program.Menu.AddSubMenu(new Menu("支持模式", "Passive"));
			Program.Menu.SubMenu("Passive").AddItem(new MenuItem("useQ_Interupt", "Q 打断技能").SetValue(false));
			Program.Menu.SubMenu("Passive").AddItem(new MenuItem("useW_Interupt", "W 打断技能").SetValue(false));

			Program.Menu.AddSubMenu(new Menu("绘制", "Drawing"));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Disabled", "禁用全部").SetValue(false));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Q", "绘制 Q").SetValue(true));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_W", "绘制 W").SetValue(true));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_E", "绘制 E").SetValue(true));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_R", "绘制 R").SetValue(true));
		}

		private void LoadSpells()
		{

			Q = new Spell(SpellSlot.Q, 1025);
			Q.SetSkillshot(0.5f, 50f, 1900, true, SkillshotType.SkillshotCircle);

			W = new Spell(SpellSlot.W, 950);

			E = new Spell(SpellSlot.E, 400);

			R = new Spell(SpellSlot.R, 400);
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

			if(Program.Menu.Item("Draw_R").GetValue<bool>())
				if(R.Level > 0)
					Utility.DrawCircle(ObjectManager.Player.Position, R.Range, R.IsReady() ? Color.Green : Color.Red);
		}

		private void Interrupter_OnPosibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
		{
			if (unit.IsAlly)
				return;
			if(Program.Menu.Item("useE_Interupt").GetValue<bool>())
				if(E.IsReady())
					if(unit.IsValidTarget(W.Range))
					{
						E.Cast(unit, Packets());
						return;
					}
			if(!Program.Menu.Item("useQ_Interupt").GetValue<bool>() || !unit.IsValidTarget(Q.Range) || Q.GetPrediction(unit).Hitchance < HitChance.Low || Environment.TickCount - QFollowTick < QFollowTime || !Q.IsReady())
				return;
			Q.Cast(unit, Packets());
			QFollowTick = Environment.TickCount;
			LastQTarget = (Obj_AI_Hero)unit;
		}

		private void Game_OnGameUpdate(EventArgs args)
		{
			if (LastQTarget != null)
				if (Environment.TickCount - QFollowTick >= QFollowTime)
					LastQTarget = null;

			switch(Program.Orbwalker.ActiveMode)
			{
				case Orbwalking.OrbwalkingMode.Combo:
					if (Program.Menu.Item("useQ_TeamFight").GetValue<bool>() && Environment.TickCount - QFollowTick >= QFollowTime)
					{
                        var target = Cast_BasicLineSkillshot_Enemy(Q, TargetSelector.DamageType.Magical);
						if (target != null)
						{
							QFollowTick = Environment.TickCount;
							LastQTarget = target;
						}
					}
					if(Program.Menu.Item("useQ_TeamFight_follow").GetValue<bool>() && Environment.TickCount <= QFollowTick + QFollowTime && LastQTarget != null)
						Q.Cast();
					if(Program.Menu.Item("useW_TeamFight_shield").GetValue<bool>())
						Cast_Shield_onFriend(W,50,true);
					if (Program.Menu.Item("useW_TeamFight_enagage").GetValue<bool>())
						EngageFriendLatern();
					if (Program.Menu.Item("useE_TeamFight").GetValue<bool>())
						Cast_E("ToMe");
					if(Program.Menu.Item("useR_TeamFight").GetValue<Slider>().Value >= 1)
                        if (Utility.CountEnemiesInRange((int)R.Range) >= Program.Menu.Item("useR_TeamFight").GetValue<Slider>().Value)
							R.Cast();
					break;
				case Orbwalking.OrbwalkingMode.Mixed:
					if(Program.Menu.Item("useQ_Harass").GetValue<bool>() && Environment.TickCount - QFollowTick >= QFollowTime)
					{
                        var target = Cast_BasicLineSkillshot_Enemy(Q, TargetSelector.DamageType.Magical);
						if(target != null)
						{
							QFollowTick = Environment.TickCount;
							LastQTarget = target;
						}
					}
					if(Program.Menu.Item("useE_Harass").GetValue<bool>())
						Cast_E();
					if (Program.Menu.Item("useW_Harass").GetValue<bool>())
						SafeFriendLatern();
					break;
				case Orbwalking.OrbwalkingMode.LaneClear:
                    if (Program.Menu.Item("useE_LaneClear").GetValue<bool>())
						Cast_BasicLineSkillshot_AOE_Farm(E);
					break;
			}
		}

		private void EngageFriendLatern()
		{
			if (!W.IsReady())
				return;
			var bestcastposition = new Vector3(0f, 0f, 0f);
            foreach (var friend in Program.Helper.OwnTeam.Where(hero => !hero.IsMe && hero.Distance(ObjectManager.Player) <= W.Range + 300 && hero.Distance(ObjectManager.Player) <= W.Range - 300 && hero.Health / hero.MaxHealth * 100 >= 20 && Utility.CountEnemiesInRange(150) >= 1))
			{
				var center = ObjectManager.Player.Position;
				const int points = 36;
				var radius = W.Range;

				const double slice = 2 * Math.PI / points;
				for(var i = 0; i < points; i++)
				{
					var angle = slice * i;
					var newX = (int)(center.X + radius * Math.Cos(angle));
					var newY = (int)(center.Y + radius * Math.Sin(angle));
					var p = new Vector3(newX, newY, 0);
					if(p.Distance(friend.Position) <= bestcastposition.Distance(friend.Position))
						bestcastposition = p;
				}
				if (friend.Distance(ObjectManager.Player) <= W.Range)
				{
					W.Cast(bestcastposition, Packets());
					return;
				}
			}
			if(bestcastposition.Distance(new Vector3(0f, 0f, 0f)) >= 100)
				W.Cast(bestcastposition, Packets());
		}

		private void SafeFriendLatern()
		{
			if(!W.IsReady())
				return;
			var bestcastposition = new Vector3(0f, 0f, 0f);
			foreach(
				var friend in
                    Program.Helper.OwnTeam
						.Where(
							hero =>
								!hero.IsMe && hero.Distance(ObjectManager.Player) <= W.Range + 300 &&
								hero.Distance(ObjectManager.Player) <= W.Range - 200 && hero.Health / hero.MaxHealth * 100 >= 20 && !hero.IsDead))
			{
                foreach (var enemy in Program.Helper.EnemyTeam)
				{
					if(friend == null || !(friend.Distance(enemy) <= 300))
						continue;
					var center = ObjectManager.Player.Position;
					const int points = 36;
					var radius = W.Range;

					const double slice = 2 * Math.PI / points;
					for(var i = 0; i < points; i++)
					{
						var angle = slice * i;
						var newX = (int)(center.X + radius * Math.Cos(angle));
						var newY = (int)(center.Y + radius * Math.Sin(angle));
						var p = new Vector3(newX, newY, 0);
						if(p.Distance(friend.Position) <= bestcastposition.Distance(friend.Position))
							bestcastposition = p;
					}
					if(friend.Distance(ObjectManager.Player) <= W.Range)
					{
						W.Cast(bestcastposition, Packets());
						return;
					}
				}
				if(bestcastposition.Distance(new Vector3(0f, 0f, 0f)) >= 100)
					W.Cast(bestcastposition, Packets());
			}
		}

		private void Cast_E(string mode = "")
		{
			if (!E.IsReady() || !ManaManagerAllowCast(E))
				return;
            var target = TargetSelector.GetTarget(E.Range - 10, TargetSelector.DamageType.Magical);
			if(target == null)
				return;
			E.Cast(mode == "ToMe" ? GetReversePosition(ObjectManager.Player.Position, target.Position) : target.Position,
				Packets());
		}


		public Obj_AI_Hero LastQTarget
		{
			get;
			set;
		}
	}
}
