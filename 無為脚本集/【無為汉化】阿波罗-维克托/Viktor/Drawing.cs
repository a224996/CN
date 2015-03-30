using System;
using System.Collections.Generic;
using System.Drawing;
using LeagueSharp;
using LeagueSharp.Common;

namespace Viktor
{
    internal class Drawing
    {
        private static readonly Obj_AI_Hero Player = ObjectManager.Player;
        private static readonly Dictionary<SpellSlot, Spell> Spell = Spells.Spell;

        public static void Init()
        {
            DamageInd();
            LeagueSharp.Drawing.OnDraw += OnDraw;
        }

        private static void DamageInd()
        {
            var drawComboDamageBool = Config.ViktorConfig.Item("apollo.viktor.draw.ind.bool");
            var drawComboDamageFill = Config.ViktorConfig.Item("apollo.viktor.draw.ind.fill");
            DamageIndicator.DamageToUnit = Damages.ComboDmg;
            DamageIndicator.Enabled = drawComboDamageBool.GetValue<bool>();
            DamageIndicator.Fill = drawComboDamageFill.GetValue<Circle>().Active;
            DamageIndicator.FillColor = drawComboDamageFill.GetValue<Circle>().Color;
            drawComboDamageBool.ValueChanged +=
                delegate(object sender, OnValueChangeEventArgs eventArgs)
                {
                    DamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
                };
            drawComboDamageFill.ValueChanged +=
                delegate(object sender, OnValueChangeEventArgs eventArgs)
                {
                    DamageIndicator.Fill = eventArgs.GetNewValue<Circle>().Active;
                    DamageIndicator.FillColor = eventArgs.GetNewValue<Circle>().Color;
                };
        }

        private static void OnDraw(EventArgs args)
        {
            if (args == null || Player.IsDead)
                return;

            var drawCd = Config.ViktorConfig.Item("apollo.viktor.draw.cd").GetValue<Circle>();

            var cdQ = !Spell[SpellSlot.Q].IsReady() && drawCd.Active;
            var drawQ = Config.ViktorConfig.Item("apollo.viktor.draw.q").GetValue<Circle>();
            if (drawQ.Active && Spell[SpellSlot.Q].Level > 0)
                Render.Circle.DrawCircle(Player.Position, Spell[SpellSlot.Q].Range, cdQ ? drawCd.Color : drawQ.Color);

            var cdW = !Spell[SpellSlot.W].IsReady() && drawCd.Active;
            var drawW = Config.ViktorConfig.Item("apollo.viktor.draw.w").GetValue<Circle>();
            if (drawW.Active && Spell[SpellSlot.W].Level > 0)
                Render.Circle.DrawCircle(Player.Position, Spell[SpellSlot.W].Range, cdW ? drawCd.Color : drawW.Color);

            var cdE = !Spell[SpellSlot.E].IsReady() && drawCd.Active;
            var drawE = Config.ViktorConfig.Item("apollo.viktor.draw.e").GetValue<Circle>();
            if (drawE.Active && Spell[SpellSlot.E].Level > 0)
                Render.Circle.DrawCircle(Player.Position, Spell[SpellSlot.E].Range + Spells.ECastRange, cdE ? drawCd.Color : drawE.Color);

            var cdR = !Spell[SpellSlot.R].IsReady() && drawCd.Active;
            var drawR = Config.ViktorConfig.Item("apollo.viktor.draw.r").GetValue<Circle>();
            if (drawR.Active && Spell[SpellSlot.R].Level > 0)
                Render.Circle.DrawCircle(Player.Position, Spell[SpellSlot.R].Range, cdR ? drawCd.Color : drawR.Color);
            /*if (Mechanics.ChaosStorm != null)
                Render.Circle.DrawCircle(Mechanics.ChaosStorm.Position.To2D().To3D(), 0, Color.Red, (int)Spell[SpellSlot.R].Width);*/
        }
    }
}
