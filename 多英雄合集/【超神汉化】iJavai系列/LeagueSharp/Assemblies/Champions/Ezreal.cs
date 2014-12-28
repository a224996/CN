using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using Assemblies.Utilitys;
using LeagueSharp;
using LeagueSharp.Common;

namespace Assemblies.Champions {
    internal class Ezreal : Champion {
        public Ezreal() {
            if (player.ChampionName != "Ezreal") {
                return;
            }
            loadMenu();
            loadSpells();

            Drawing.OnDraw += onDraw;
            Game.OnGameUpdate += onUpdate;
            Game.PrintChat("[Assemblies] - Ezreal Loaded.");
        }

        private void loadSpells() {
            Q = new Spell(SpellSlot.Q, 1200);
            Q.SetSkillshot(0.25f, 60f, 2000f, true, SkillshotType.SkillshotLine);

            W = new Spell(SpellSlot.W, 1050);
            W.SetSkillshot(0.25f, 80f, 2000f, false, SkillshotType.SkillshotLine);

            R = new Spell(SpellSlot.R, 3000);
            R.SetSkillshot(1f, 160f, 2000f, false, SkillshotType.SkillshotLine);
        }

        private void loadMenu() {
            menu.AddSubMenu(new Menu("连招", "combo"));
            menu.SubMenu("combo").AddItem(new MenuItem("useQC", "使用Q").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("useWC", "使用W").SetValue(true));
            menu.SubMenu("combo").AddItem(new MenuItem("useRC", "使用R").SetValue(true));

            menu.AddSubMenu(new Menu("骚扰", "harass"));
            menu.SubMenu("harass").AddItem(new MenuItem("useQH", "使用Q").SetValue(true));
            menu.SubMenu("harass").AddItem(new MenuItem("useWH", "使用W").SetValue(false));

            menu.AddSubMenu(new Menu("清线", "laneclear"));
            menu.SubMenu("laneclear").AddItem(new MenuItem("useQLC", "使用Q").SetValue(true));
            menu.SubMenu("laneclear").AddItem(new MenuItem("AutoQLC", "清线自动Q").SetValue(false));
            menu.SubMenu("laneclear").AddItem(new MenuItem("useQLCH", "清线时骚扰").SetValue(false));


            menu.AddSubMenu(new Menu("补兵", "lastHit"));
            menu.SubMenu("lastHit").AddItem(new MenuItem("lastHitq", "使用Q").SetValue(false));
            menu.SubMenu("lastHit").AddItem(new MenuItem("autoLastHit", "补兵自动Q").SetValue(false));

            menu.AddSubMenu(new Menu("抢人头", "killsteal"));
            menu.SubMenu("killsteal").AddItem(new MenuItem("useQK", "使用Q").SetValue(true));

            menu.AddSubMenu(new Menu("击中几率", "hitchance"));
            menu.SubMenu("hitchance")
                .AddItem(
                    new MenuItem("hitchanceSetting", "击中几率").SetValue(
                        new StringList(new[] {"低", "中等", "高", "特高"})));

            menu.AddSubMenu(new Menu("显示", "drawing"));
            menu.SubMenu("drawing").AddItem(new MenuItem("drawQ", "Q范围").SetValue(false));
            menu.SubMenu("drawing").AddItem(new MenuItem("drawW", "W范围").SetValue(false));
            menu.SubMenu("drawing").AddItem(new MenuItem("drawR", "R范围").SetValue(false));

            menu.AddSubMenu(new Menu("杂项", "misc"));
            menu.SubMenu("misc").AddItem(new MenuItem("usePackets", "封包").SetValue(true));
            menu.SubMenu("misc").AddItem(new MenuItem("useNE", "敌人在范围内不R").SetValue(false));
            menu.SubMenu("misc")
                .AddItem(new MenuItem("NERange", "不R范围").SetValue(new Slider(450, 450, 1400)));
							
			menu.AddSubMenu(new Menu("超神汉化", "system"));
            menu.SubMenu("system").AddItem(new MenuItem("systemQ", "L#汉化群：386289593"));
        }

        private void onUpdate(EventArgs args) {
            if (player.IsDead) return;

            if (menu.Item("useQK").GetValue<bool>()) {
                if (Q.IsKillable(TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical)))
                    castQ();
            }
            laneClear();
            lastHit();
            switch (xSLxOrbwalker.CurrentMode) {
                case xSLxOrbwalker.Mode.Combo:
                    if (menu.Item("useQC").GetValue<bool>())
                        castQ();
                    if (menu.Item("useWC").GetValue<bool>())
                        castW();
                    if (menu.Item("useRC").GetValue<bool>()) {
                        Obj_AI_Hero target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
                        if (getUnitsInPath(player, target, R)) {
                            PredictionOutput prediction = R.GetPrediction(target, true);
                            if (target.IsValidTarget(R.Range) && R.IsReady() && prediction.Hitchance >= HitChance.High) {
                                sendSimplePing(target.Position);
                                R.Cast(target, getPackets(), true);
                            }
                        }
                    }
                    break;
                case xSLxOrbwalker.Mode.Harass:
                    if (menu.Item("useQH").GetValue<bool>())
                        castQ();
                    if (menu.Item("useWH").GetValue<bool>())
                        castW();
                    break;
            }
        }

        private void lastHit() {
            //TODO - get minions around you
            //Check if minion is killable with Q && isInRange
            //Also check if orbwalking mode == lasthit
            var autoQ = menu.Item("autoLastHit").GetValue<bool>();
            var lastHitNormal = menu.Item("lastHitq").GetValue<bool>();

            foreach (
                Obj_AI_Base minion in
                    MinionManager.GetMinions(player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.NotAlly)) {
                if (autoQ && Q.IsReady() && Q.IsKillable(minion))
                    Q.Cast(minion.Position, getPackets());
                if (lastHitNormal && Q.IsReady() && xSLxOrbwalker.CurrentMode == xSLxOrbwalker.Mode.Lasthit &&
                    Q.IsKillable(minion))
                    Q.Cast(minion.Position, getPackets());
            }
        }

        private void laneClear() {
            List<Obj_AI_Base> minionforQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range,
                MinionTypes.All, MinionTeam.NotAlly);
            var useQ = menu.Item("useQLC").GetValue<bool>();
            var useAutoQ = menu.Item("AutoQLC").GetValue<bool>();
            MinionManager.FarmLocation qPosition = Q.GetLineFarmLocation(minionforQ);
            if (useQ && xSLxOrbwalker.CurrentMode == xSLxOrbwalker.Mode.LaneClear && Q.IsReady() &&
                qPosition.MinionsHit >= 1) {
                Q.Cast(qPosition.Position, getPackets());
            }
            if (useAutoQ && Q.IsReady() && qPosition.MinionsHit >= 1) {
                Q.Cast(qPosition.Position, getPackets());
            }
            if (menu.Item("useQLCH").GetValue<bool>() && xSLxOrbwalker.CurrentMode == xSLxOrbwalker.Mode.LaneClear) {
                castQ();
            }
        }

        private HitChance getHitchance() {
            switch (menu.Item("hitchanceSetting").GetValue<StringList>().SelectedIndex) {
                case 0:
                    return HitChance.Low;
                case 1:
                    return HitChance.Medium;
                case 2:
                    return HitChance.High;
                case 3:
                    return HitChance.VeryHigh;
                default:
                    return HitChance.High;
            }
        }

        private bool getPackets() {
            return menu.Item("usePackets").GetValue<bool>();
        }

        private void onDraw(EventArgs args) {
            if (menu.Item("drawQ").GetValue<bool>()) {
                Utility.DrawCircle(player.Position, Q.Range, Color.Cyan);
            }
            if (menu.Item("drawW").GetValue<bool>()) {
                Utility.DrawCircle(player.Position, W.Range, Color.Crimson);
            }
            if (menu.Item("drawR").GetValue<bool>()) {
                Utility.DrawCircle(player.Position, R.Range, Color.Purple, 5, 30, true);
            }
        }

        private void castQ() {
            Obj_AI_Hero qTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (!Q.IsReady() || qTarget == null || player.Distance(qTarget) > 1200) return;

            if (qTarget.IsValidTarget(Q.Range) && qTarget.IsVisible && !qTarget.IsDead &&
                Q.GetPrediction(qTarget).Hitchance >= getHitchance()) {
                Q.Cast(qTarget, getPackets());
            }
        }

        private void castW() {
            Obj_AI_Hero wTarget = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);
            if (!W.IsReady() || wTarget == null) return;
            if (wTarget.IsValidTarget(W.Range) || W.GetPrediction(wTarget).Hitchance >= getHitchance()) {
                W.Cast(wTarget, getPackets());
            }
        }
    }
}