using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace UltimateCarry
{
	class Zyra : Champion
	{
		public Spell Q;
		public Spell W;
		public Spell E;
		public Spell R;
		public Spell Passive;

        public Zyra()
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
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useE_TeamFight", "使用 E").SetValue(true));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useR_TeamFight_willhit", "使用 R 如果击中").SetValue(new Slider(2, 5, 0)));

			Program.Menu.AddSubMenu(new Menu("骚扰", "Harass"));
			Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useQ_Harass", "使用 Q").SetValue(true));
			Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useE_Harass", "使用 E").SetValue(true));

			Program.Menu.AddSubMenu(new Menu("清兵", "LaneClear"));
			Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("useQ_LaneClear", "使用 Q").SetValue(true));
			Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("useE_LaneClear", "使用 E").SetValue(true));

			Program.Menu.AddSubMenu(new Menu("支持模式", "Passive"));
			Program.Menu.SubMenu("Passive").AddItem(new MenuItem("useW_Passive", "Plant on Spelllocations").SetValue(true));

			Program.Menu.AddSubMenu(new Menu("支持模式", "SupportMode"));
			Program.Menu.SubMenu("SupportMode").AddItem(new MenuItem("hitMinions", "击中小兵").SetValue(false));

			Program.Menu.AddSubMenu(new Menu("绘制", "Drawing"));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Disabled", "禁用全部").SetValue(false));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Q", "绘制 Q").SetValue(true));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_W", "绘制 W").SetValue(true));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_E", "绘制 E").SetValue(true));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_R", "绘制 R").SetValue(true));
		}

		private void LoadSpells()
		{
		
			Q = new Spell(SpellSlot.Q, 800);
			Q.SetSkillshot(0.8f, 60f, float.MaxValue , false, SkillshotType.SkillshotCircle); // small width for better hits

			W = new Spell(SpellSlot.W, 825);

			E = new Spell(SpellSlot.E, 1100);
			E.SetSkillshot(0.5f, 70f, 1400f, false, SkillshotType.SkillshotLine);

			R = new Spell(SpellSlot.R, 700);
			R.SetSkillshot(0.5f, 500f, float.MaxValue, false, SkillshotType.SkillshotCircle);

			Passive = new Spell(SpellSlot.Q, 1470);
			Passive.SetSkillshot(0.5f, 70f, 1400f, false, SkillshotType.SkillshotLine);
		}

		private void Drawing_OnDraw(EventArgs args)
		{
			if(Program.Menu.Item("Draw_Disabled").GetValue<bool>())
				return;

			if (ZyraisZombie())
			{
				Utility.DrawCircle(ObjectManager.Player.Position, Passive.Range, Passive.IsReady() ? Color.Green : Color.Red);
				return;
			}
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

		private bool ZyraisZombie()
		{
			return ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Name ==
			       ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).Name ||
			       ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Name ==
			       ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).Name;
		}

		private void Game_OnGameUpdate(EventArgs args)
		{

			if (ZyraisZombie())
			{
				CastPassive();
				return;
			}
			switch(Program.Orbwalker.ActiveMode)
			{

				case Orbwalking.OrbwalkingMode.Combo:
					if(Program.Menu.Item("useQ_TeamFight").GetValue<bool>())
						CastQEnemy();
					if(Program.Menu.Item("useE_TeamFight").GetValue<bool>())
						CastEEnemy();
					if(Program.Menu.Item("useR_TeamFight_willhit").GetValue<Slider>().Value >= 1)
						CastREnemy();
					break;
				case Orbwalking.OrbwalkingMode.Mixed:
					if(Program.Menu.Item("useQ_Harass").GetValue<bool>())
						CastQEnemy();
					if(Program.Menu.Item("useE_Harass").GetValue<bool>())
						CastEEnemy();
					break;
				case Orbwalking.OrbwalkingMode.LaneClear:
					if(Program.Menu.Item("useQ_LaneClear").GetValue<bool>())
						CastQMinion();
					if(Program.Menu.Item("useE_LaneClear").GetValue<bool>())
						CastEMinion();
					break;

			}
		}

		private void CastEMinion()
		{
			if(!E.IsReady())
				return;
			var minions = MinionManager.GetMinions(ObjectManager.Player.Position, E.Range, MinionTypes.All, MinionTeam.NotAlly);
			if(minions.Count == 0)
				return;
			var castPostion = MinionManager.GetBestLineFarmLocation(minions.Select(minion => minion.ServerPosition.To2D()).ToList(), E.Width, E.Range);
			E.Cast(castPostion.Position, Packets());
			if(!Program.Menu.Item("useW_Passive").GetValue<bool>())
				return;
			var pos = castPostion.Position.To3D();
			Utility.DelayAction.Add(50, () => W.Cast(new Vector3(pos.X - 5, pos.Y - 5, pos.Z), Packets()));
			Utility.DelayAction.Add(150, () => W.Cast(new Vector3(pos.X + 5, pos.Y + 5, pos.Z), Packets()));
		}

		private void CastQMinion()
		{
			if(!Q.IsReady())
				return;
			var minions = MinionManager.GetMinions(ObjectManager.Player.Position, Q.Range + (Q.Width / 2), MinionTypes.All, MinionTeam.NotAlly);
			if(minions.Count == 0)
				return;
			var castPostion = MinionManager.GetBestCircularFarmLocation(minions.Select(minion => minion.ServerPosition.To2D()).ToList(), Q.Width, Q.Range);
			Q.Cast(castPostion.Position, Packets());
			if(Program.Menu.Item("useW_Passive").GetValue<bool>())
			{
				var pos = castPostion.Position.To3D();
				Utility.DelayAction.Add(50, () => W.Cast(new Vector3(pos.X -5,pos.Y -5 ,pos.Z), Packets()));
				Utility.DelayAction.Add(150, () => W.Cast(new Vector3(pos.X + 5, pos.Y + 5, pos.Z), Packets()));
			}
		}

		private void CastREnemy()
		{
			if(!R.IsReady())
				return;
			var minHit = Program.Menu.Item("useR_TeamFight_willhit").GetValue<Slider>().Value;
			if (minHit == 0)
				return;
            var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
			if (!target.IsValidTarget(R.Range)) 
				return;
			R.CastIfWillHit(target, minHit - 1, Packets());
		}

		private void CastQEnemy()
		{
			if(!Q.IsReady())
				return;
            var target = TargetSelector.GetTarget(Q.Range + (Q.Width / 2), TargetSelector.DamageType.Magical);
			if (!target.IsValidTarget(Q.Range)) 
				return;
			Q.CastIfHitchanceEquals(target, HitChance.High, Packets());
			if(Program.Menu.Item("useW_Passive").GetValue<bool>())
			{
				var pos = Q.GetPrediction(target ).CastPosition;
				Utility.DelayAction.Add(50, () => W.Cast(new Vector3(pos.X - 2, pos.Y - 2, pos.Z), Packets()));
				Utility.DelayAction.Add(150, () => W.Cast(new Vector3(pos.X + 2, pos.Y + 2, pos.Z), Packets()));
			}
		}

		private void CastEEnemy()
		{
			if(!E.IsReady())
				return;
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
			if(!target.IsValidTarget(E.Range))
				return;
			E.CastIfHitchanceEquals(target, HitChance.High, Packets());
			if(Program.Menu.Item("useW_Passive").GetValue<bool>())
			{
				var pos = E.GetPrediction(target).CastPosition;
				Utility.DelayAction.Add(50, () => W.Cast(new Vector3(pos.X - 5, pos.Y - 5, pos.Z), Packets()));
				Utility.DelayAction.Add(150, () => W.Cast(new Vector3(pos.X + 5, pos.Y + 5, pos.Z), Packets()));
			}
		}

		private void CastPassive()
		{
			if(!Passive.IsReady())
				return;
            var target = TargetSelector.GetTarget(Passive.Range, TargetSelector.DamageType.Magical);
			if(!target.IsValidTarget(E.Range))
				return;
			Passive.CastIfHitchanceEquals(target, HitChance.High, Packets());
		}
	}
}
