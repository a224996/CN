#region License

/*
 Copyright 2014 - 2014 Nikita Bernthaler
 Ranges.cs is part of SFXUtility.
 
 SFXUtility is free software: you can redistribute it and/or modify
 it under the terms of the GNU General Public License as published by
 the Free Software Foundation, either version 3 of the License, or
 (at your option) any later version.
 
 SFXUtility is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 GNU General Public License for more details.
 
 You should have received a copy of the GNU General Public License
 along with SFXUtility. If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

namespace SFXUtility.Feature
{
    #region

    using System;
    using System.Drawing;
    using Class;
    using IoCContainer;
    using LeagueSharp;
    using LeagueSharp.Common;

    #endregion

    internal class Ranges : Base
    {
        #region Fields

        private const float ExperienceRange = 1400f;
        private const float TurretRange = 900f;

        #endregion

        #region Constructors

        public Ranges(IContainer container)
            : base(container)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        #endregion

        #region Properties

        public override bool Enabled
        {
            get { return Menu != null && Menu.Item(Name + "Enabled").GetValue<bool>(); }
        }

        public override string Name
        {
            get { return "無為汉化─范围"; }
        }

        #endregion

        #region Methods

        private void DrawAttack()
        {
            var drawFriendly = Menu.Item(Name + "AttackFriendly").GetValue<bool>();
            var drawEnemy = Menu.Item(Name + "AttackEnemy").GetValue<bool>();
            var drawSelf = Menu.Item(Name + "AttackSelf").GetValue<bool>();

            if (!drawFriendly && !drawEnemy && !drawSelf)
                return;

            var color = Menu.Item(Name + "AttackColor").GetValue<Color>();
            var circleThickness = BaseMenu.Item("MiscCircleThickness").GetValue<Slider>().Value;

            var distanceLimitEnabled = Menu.Item(Name + "DistanceEnabled").GetValue<bool>();
            var distanceLimit = Menu.Item(Name + "DistanceLimit").GetValue<Slider>().Value;

            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                var radius = hero.BoundingRadius + hero.AttackRange;
                if (hero.IsValid && !hero.IsDead && hero.IsVisible && hero.IsOnScreen(radius))
                {
                    if ((hero.IsAlly && drawFriendly || hero.IsMe && drawSelf || hero.IsEnemy && drawEnemy) &&
                        !(hero.IsMe && !drawSelf))
                    {
                        if (!distanceLimitEnabled || hero.Distance(ObjectManager.Player.Position) <= distanceLimit)
                        {
                            Render.Circle.DrawCircle(hero.Position, radius, color, circleThickness);
                        }
                    }
                }
            }
        }

        private void DrawExperience()
        {
            var drawFriendly = Menu.Item(Name + "ExperienceFriendly").GetValue<bool>();
            var drawEnemy = Menu.Item(Name + "ExperienceEnemy").GetValue<bool>();
            var drawSelf = Menu.Item(Name + "ExperienceSelf").GetValue<bool>();

            if (!drawFriendly && !drawEnemy && !drawSelf)
                return;

            var color = Menu.Item(Name + "ExperienceColor").GetValue<Color>();
            var circleThickness = BaseMenu.Item("MiscCircleThickness").GetValue<Slider>().Value;

            var distanceLimitEnabled = Menu.Item(Name + "DistanceEnabled").GetValue<bool>();
            var distanceLimit = Menu.Item(Name + "DistanceLimit").GetValue<Slider>().Value;

            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero.IsValid && !hero.IsDead && hero.IsVisible && hero.IsOnScreen(ExperienceRange))
                {
                    if ((hero.IsAlly && drawFriendly || hero.IsMe && drawSelf || hero.IsEnemy && drawEnemy) &&
                        !(hero.IsMe && !drawSelf))
                    {
                        if (!distanceLimitEnabled || hero.Distance(ObjectManager.Player.Position) <= distanceLimit)
                        {
                            Render.Circle.DrawCircle(hero.Position, ExperienceRange, color, circleThickness);
                        }
                    }
                }
            }
        }

        private void DrawSpell()
        {
            var drawFriendlyQ = Menu.Item(Name + "SpellFriendlyQ").GetValue<bool>();
            var drawFriendlyW = Menu.Item(Name + "SpellFriendlyW").GetValue<bool>();
            var drawFriendlyE = Menu.Item(Name + "SpellFriendlyE").GetValue<bool>();
            var drawFriendlyR = Menu.Item(Name + "SpellFriendlyR").GetValue<bool>();
            var drawFriendly = drawFriendlyQ || drawFriendlyW || drawFriendlyE || drawFriendlyR;

            var drawEnemyQ = Menu.Item(Name + "SpellEnemyQ").GetValue<bool>();
            var drawEnemyW = Menu.Item(Name + "SpellEnemyW").GetValue<bool>();
            var drawEnemyE = Menu.Item(Name + "SpellEnemyE").GetValue<bool>();
            var drawEnemyR = Menu.Item(Name + "SpellEnemyR").GetValue<bool>();
            var drawEnemy = drawEnemyQ || drawEnemyW || drawEnemyE || drawEnemyR;

            var drawSelfQ = Menu.Item(Name + "SpellSelfQ").GetValue<bool>();
            var drawSelfW = Menu.Item(Name + "SpellSelfW").GetValue<bool>();
            var drawSelfE = Menu.Item(Name + "SpellSelfE").GetValue<bool>();
            var drawSelfR = Menu.Item(Name + "SpellSelfR").GetValue<bool>();
            var drawSelf = drawSelfQ || drawSelfW || drawSelfE || drawSelfR;

            if (!drawFriendly && !drawEnemy && !drawSelf)
                return;

            var circleThickness = BaseMenu.Item("MiscCircleThickness").GetValue<Slider>().Value;

            var distanceLimitEnabled = Menu.Item(Name + "DistanceEnabled").GetValue<bool>();
            var distanceLimit = Menu.Item(Name + "DistanceLimit").GetValue<Slider>().Value;

            var spellMaxRange = Menu.Item(Name + "SpellMaxRange").GetValue<Slider>().Value;

            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero.IsValid && !hero.IsDead && hero.IsVisible)
                {
                    if (!distanceLimitEnabled || hero.Distance(ObjectManager.Player.Position) <= distanceLimit)
                    {
                        var color =
                            Menu.Item(Name + "Spell" + (hero.IsMe ? "Self" : (hero.IsEnemy ? "Enemy" : "Friendly")) +
                                      "Color").GetValue<Color>();
                        if ((hero.IsAlly && drawFriendlyQ || hero.IsEnemy && drawEnemyQ || hero.IsMe && drawSelfQ) &&
                            !(hero.IsMe && !drawSelfQ))
                        {
                            var range = hero.Spellbook.GetSpell(SpellSlot.Q).SData.CastRange;
                            if (range <= spellMaxRange && hero.IsOnScreen(range))
                                Render.Circle.DrawCircle(hero.Position, range, color, circleThickness);
                        }
                        if ((hero.IsAlly && drawFriendlyW || hero.IsEnemy && drawEnemyW || hero.IsMe && drawSelfW) &&
                            !(hero.IsMe && !drawSelfW))
                        {
                            var range = hero.Spellbook.GetSpell(SpellSlot.W).SData.CastRange;
                            if (range <= spellMaxRange && hero.IsOnScreen(range))
                                Render.Circle.DrawCircle(hero.Position, range, color, circleThickness);
                        }
                        if ((hero.IsAlly && drawFriendlyE || hero.IsEnemy && drawEnemyE || hero.IsMe && drawSelfE) &&
                            !(hero.IsMe && !drawSelfE))
                        {
                            var range = hero.Spellbook.GetSpell(SpellSlot.E).SData.CastRange;
                            if (range <= spellMaxRange && hero.IsOnScreen(range))
                                Render.Circle.DrawCircle(hero.Position, range, color, circleThickness);
                        }
                        if ((hero.IsAlly && drawFriendlyR || hero.IsEnemy && drawEnemyR || hero.IsMe && drawSelfR) &&
                            !(hero.IsMe && !drawSelfR))
                        {
                            var range = hero.Spellbook.GetSpell(SpellSlot.R).SData.CastRange;
                            if (range <= spellMaxRange && hero.IsOnScreen(range))
                                Render.Circle.DrawCircle(hero.Position, range, color, circleThickness);
                        }
                    }
                }
            }
        }

        private void DrawTurret()
        {
            var drawFriendly = Menu.Item(Name + "TurretFriendly").GetValue<bool>();
            var drawEnemy = Menu.Item(Name + "TurretEnemy").GetValue<bool>();

            if (!drawFriendly && !drawEnemy)
                return;

            var circleThickness = BaseMenu.Item("MiscCircleThickness").GetValue<Slider>().Value;

            var distanceLimitEnabled = Menu.Item(Name + "DistanceEnabled").GetValue<bool>();
            var distanceLimit = Menu.Item(Name + "DistanceLimit").GetValue<Slider>().Value;

            foreach (Obj_AI_Turret turret in ObjectManager.Get<Obj_AI_Turret>())
            {
                if (turret.IsValid && !turret.IsDead && turret.IsVisible && turret.IsOnScreen(TurretRange))
                {
                    if (turret.IsAlly && drawFriendly || turret.IsEnemy && drawEnemy)
                    {
                        if (!distanceLimitEnabled || turret.Distance(ObjectManager.Player.Position) <= distanceLimit)
                        {
                            Render.Circle.DrawCircle(turret.Position, TurretRange,
                                Menu.Item(Name + "Turret" + (turret.IsAlly ? "Friendly" : "Enemy") + "Color")
                                    .GetValue<Color>(), circleThickness);
                        }
                    }
                }
            }
        }

        private void OnDraw(EventArgs args)
        {
            try
            {
                if (!Enabled)
                    return;

                DrawExperience();
                DrawAttack();
                DrawTurret();
                DrawSpell();
            }
            catch (Exception ex)
            {
                Logger.WriteBlock(ex.Message, ex.ToString());
            }
        }

        private void OnGameLoad(EventArgs args)
        {
            try
            {
                Logger.Prefix = string.Format("{0} - {1}", BaseName, Name);

                Menu = new Menu(Name, Name);

                var experienceMenu = new Menu("经验", Name + "Experience");
                experienceMenu.AddItem(new MenuItem(Name + "ExperienceColor", "颜色").SetValue(Color.Gray));
                experienceMenu.AddItem(new MenuItem(Name + "ExperienceSelf", "自己").SetValue(true));
                experienceMenu.AddItem(new MenuItem(Name + "ExperienceFriendly", "队友").SetValue(true));
                experienceMenu.AddItem(new MenuItem(Name + "ExperienceEnemy", "敌人").SetValue(true));

                var attackMenu = new Menu("攻击", Name + "Attack");
                attackMenu.AddItem(new MenuItem(Name + "AttackColor", "队友").SetValue(Color.Yellow));
                attackMenu.AddItem(new MenuItem(Name + "AttackSelf", "自己").SetValue(true));
                attackMenu.AddItem(new MenuItem(Name + "AttackFriendly", "队友").SetValue(true));
                attackMenu.AddItem(new MenuItem(Name + "AttackEnemy", "敌人").SetValue(true));

                var turretMenu = new Menu("炮塔", Name + "Turret");
                turretMenu.AddItem(new MenuItem(Name + "TurretFriendlyColor", "队友颜色").SetValue(Color.DarkGreen));
                turretMenu.AddItem(new MenuItem(Name + "TurretEnemyColor", "敌人颜色").SetValue(Color.DarkRed));
                turretMenu.AddItem(new MenuItem(Name + "TurretFriendly", "队友").SetValue(true));
                turretMenu.AddItem(new MenuItem(Name + "TurretEnemy", "敌人").SetValue(true));

                var spellMenu = new Menu("法术", Name + "Spell");
                spellMenu.AddItem(
                    new MenuItem(Name + "SpellMaxRange", "最大法术范围ㄧ").SetValue(new Slider(1000, 500, 3000)));


                var spellSelfMenu = new Menu("自己", Name + "SpellSelf");
                spellSelfMenu.AddItem(new MenuItem(Name + "SpellSelfColor", "颜色").SetValue(Color.Purple));
                spellSelfMenu.AddItem(new MenuItem(Name + "SpellSelfQ", "Q").SetValue(true));
                spellSelfMenu.AddItem(new MenuItem(Name + "SpellSelfW", "W").SetValue(true));
                spellSelfMenu.AddItem(new MenuItem(Name + "SpellSelfE", "E").SetValue(true));
                spellSelfMenu.AddItem(new MenuItem(Name + "SpellSelfR", "R").SetValue(true));

                spellMenu.AddSubMenu(spellSelfMenu);

                var spellFriendlyMenu = new Menu("队友", Name + "SpellFriendly");
                spellFriendlyMenu.AddItem(new MenuItem(Name + "SpellFriendlyColor", "颜色").SetValue(Color.Green));
                spellFriendlyMenu.AddItem(new MenuItem(Name + "SpellFriendlyQ", "Q").SetValue(true));
                spellFriendlyMenu.AddItem(new MenuItem(Name + "SpellFriendlyW", "W").SetValue(true));
                spellFriendlyMenu.AddItem(new MenuItem(Name + "SpellFriendlyE", "E").SetValue(true));
                spellFriendlyMenu.AddItem(new MenuItem(Name + "SpellFriendlyR", "R").SetValue(true));

                spellMenu.AddSubMenu(spellFriendlyMenu);

                var spellEnemyMenu = new Menu("敌人", Name + "SpellEnemy");
                spellEnemyMenu.AddItem(new MenuItem(Name + "SpellEnemyColor", "颜色").SetValue(Color.Red));
                spellEnemyMenu.AddItem(new MenuItem(Name + "SpellEnemyQ", "Q").SetValue(true));
                spellEnemyMenu.AddItem(new MenuItem(Name + "SpellEnemyW", "W").SetValue(true));
                spellEnemyMenu.AddItem(new MenuItem(Name + "SpellEnemyE", "E").SetValue(true));
                spellEnemyMenu.AddItem(new MenuItem(Name + "SpellEnemyR", "R").SetValue(true));

                spellMenu.AddSubMenu(spellEnemyMenu);

                var distanceMenu = new Menu("距离", Name + "Distance");
                distanceMenu.AddItem(new MenuItem(Name + "DistanceEnabled", "极限距离").SetValue(true));
                distanceMenu.AddItem(
                    new MenuItem(Name + "DistanceLimit", "距离限制").SetValue(new Slider(1500, 500, 3000)));

                Menu.AddSubMenu(experienceMenu);
                Menu.AddSubMenu(attackMenu);
                Menu.AddSubMenu(turretMenu);
                Menu.AddSubMenu(spellMenu);
                Menu.AddSubMenu(distanceMenu);

                Menu.AddItem(new MenuItem(Name + "Enabled", "启用").SetValue(false));

                BaseMenu.AddSubMenu(Menu);

                Drawing.OnDraw += OnDraw;

                Initialized = true;
            }
            catch (Exception ex)
            {
                Logger.WriteBlock(ex.Message, ex.ToString());
            }
        }

        #endregion
    }
}