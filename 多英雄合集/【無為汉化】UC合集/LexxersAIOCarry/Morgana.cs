using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace UltimateCarry
{
	class Morgana : Champion 
	{
		public static Spell Q;
		public static Spell W;
		public static Spell R;

        public Morgana()
        {
			LoadMenu();
			LoadSpells();

			Drawing.OnDraw += Drawing_OnDraw;
			Game.OnUpdate += Game_OnGameUpdate;
			Game.OnSendPacket += Game_OnGameSendPacket;
			PluginLoaded();
		}

		private void LoadMenu()
		{
			Program.Menu.AddSubMenu(new Menu("团队作战", "TeamFight"));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useQ_TeamFight", "使用 Q").SetValue(true));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useQ_TeamFight_Gapcloser", "使用 Q 防止突进").SetValue(true));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useW_TeamFight_bind", "使用 W 震惊").SetValue(true));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useW_TeamFight_willhit", "使用 W 如果击中").SetValue(new Slider(2, 5, 0)));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useR_TeamFight", "使用 R 如果击中").SetValue(new Slider(2, 5, 0)));


			Program.Menu.AddSubMenu(new Menu("骚扰", "Harass"));
			Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useQ_Harass", "使用 Q").SetValue(true));
			Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useW_Harass_bind", "使用 W 震惊").SetValue(true));
			Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useW_Harass_willhit", "使用 W 如果击中").SetValue(true));

			Program.Menu.AddSubMenu(new Menu("清兵", "LaneClear"));
			Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("useW_LaneClear", "使用 W").SetValue(true));

			Program.Menu.AddSubMenu(new Menu("支持模式", "SupportMode"));
			Program.Menu.SubMenu("SupportMode").AddItem(new MenuItem("hitMinions", "击中小兵").SetValue(false));

			Program.Menu.AddSubMenu(new Menu("绘制", "Drawing"));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Disabled", "禁用全部").SetValue(false));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Q", "绘制 Q").SetValue(true));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_W", "绘制 W").SetValue(true));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_R", "绘制 R").SetValue(true));

		}

		private void LoadSpells()
		{
			Q = new Spell(SpellSlot.Q, 1300);
			Q.SetSkillshot(0.234f, 70f, 1200f, true, SkillshotType.SkillshotLine);

			W = new Spell(SpellSlot.W, 900);
			W.SetSkillshot(0.28f, 175f, float.MaxValue , false, SkillshotType.SkillshotCircle );

			R = new Spell(SpellSlot.R, 600);
		}

		private void Game_OnGameUpdate(EventArgs args)
		{
			switch(Program.Orbwalker.ActiveMode)
			{
				case Orbwalking.OrbwalkingMode.Combo:
					if(Program.Menu.Item("useQ_TeamFight").GetValue<bool>())
						CastQEnemy();
					if(Program.Menu.Item("useQ_TeamFight_Gapcloser").GetValue<bool>())
						CastQEnemyGapClose();
					if(Program.Menu.Item("useW_TeamFight_bind").GetValue<bool>())
						CastWEnemyBind();
					if(Program.Menu.Item("useW_TeamFight_willhit").GetValue<Slider>().Value >= 1)
						CastWEnemyAmount();
					if(Program.Menu.Item("useR_TeamFight").GetValue<Slider>().Value >= 1)
						CastREnemyAmount();
					break;
				case Orbwalking.OrbwalkingMode.Mixed:
					if(Program.Menu.Item("useQ_Harass").GetValue<bool>())
						CastQEnemy();
					if(Program.Menu.Item("useW_Harass_bind").GetValue<bool>())
						CastWEnemyBind();
					if(Program.Menu.Item("useW_Harass_willhit").GetValue<Slider>().Value >= 1)
						CastWEnemyAmount();
					break;
				case Orbwalking.OrbwalkingMode.LaneClear:
					if(Program.Menu.Item("useW_LaneClear").GetValue<bool>())
						CastWMinion();
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

			if(Program.Menu.Item("Draw_R").GetValue<bool>())
				if(R.Level > 0)
					Utility.DrawCircle(ObjectManager.Player.Position, R.Range, R.IsReady() ? Color.Green : Color.Red);
		}

		private void CastQEnemy()
		{
			if(!Q.IsReady())
				return;
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
			if(target == null)
				return;
			if(target.IsValidTarget(Q.Range) && Q.GetPrediction(target).Hitchance >= HitChance.High)
				Q.Cast(target, Packets());
		}

		private void CastQEnemyGapClose()
		{
			if(!Q.IsReady())
				return;
            foreach (var enemy in Program.Helper.EnemyTeam.Where(hero => hero.IsValidTarget(400)).Where(enemy => enemy.IsValidTarget(Q.Range) && Q.GetPrediction(enemy).Hitchance >= HitChance.High))
			{
				Q.Cast(enemy, Packets());
				return;
			}
		}

		private void CastWEnemyBind()
		{
			if(!W.IsReady())
				return;
            foreach (var enemy in Program.Helper.EnemyTeam.Where(hero => (hero.HasBuffOfType(BuffType.Snare) || hero.HasBuffOfType(BuffType.Stun) || hero.HasBuffOfType(BuffType.Taunt) && hero.IsValidTarget(W.Range + (W.Width / 2)))))
			{
				W.Cast(enemy.Position, Packets());
				return;
			}
		}

		private void CastWEnemyAmount()
		{
			if(!W.IsReady())
				return;
            foreach (var enemy in Program.Helper.EnemyTeam.Where(hero => hero.IsValidTarget(W.Range + (W.Width / 2))))
			{
				W.CastIfWillHit(enemy,Program.Menu.Item("useW_TeamFight_willhit").GetValue<Slider>().Value -1, Packets());
				return;
			}
		}

		private void CastREnemyAmount()
		{
			if(!R.IsReady())
				return;
            if (Utility.CountEnemysInRange(ObjectManager.Player, (int)R.Range) >= Program.Menu.Item("useR_TeamFight").GetValue<Slider>().Value)
			{
				R.Cast();
			}
		}

		private void CastWMinion()
		{
			if (!W.IsReady())
				return;
			var minions = MinionManager.GetMinions(ObjectManager.Player.Position,W.Range + (W.Width/2),MinionTypes.All,MinionTeam.NotAlly);
			if (minions.Count == 0)
				return;
			var castPostion = MinionManager.GetBestCircularFarmLocation(minions.Select(minion => minion.ServerPosition.To2D()).ToList(), W.Width, W.Range);
			W.Cast(castPostion.Position, Packets());
		}
	}


}
