using System;
using System.Linq;
using System.Collections.Generic;
using Color = System.Drawing.Color;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

using Orbwalk = BrianSharp.Common.Orbwalker;

namespace BrianSharp.Plugin
{
    class Kayle : Common.Helper
    {
        private class RAntiItem
        {
            public float StartTick;
            public float EndTick;
            public RAntiItem(BuffInstance Buff)
            {
                StartTick = Game.Time + (Buff.EndTime - Buff.StartTime) - (R.Level * 0.5f + 1);
                EndTick = Game.Time + (Buff.EndTime - Buff.StartTime);
            }
        }
        private Dictionary<int, RAntiItem> RAntiDetected = new Dictionary<int, RAntiItem>();

        public Kayle()
        {
            Q = new Spell(SpellSlot.Q, 655);
            W = new Spell(SpellSlot.W, 900);
            E = new Spell(SpellSlot.E, 525);
            R = new Spell(SpellSlot.R, 915);
            Q.SetTargetted(0.5f, 1500);
            W.SetTargetted(0.3333f, float.MaxValue);
            R.SetTargetted(0.5f, float.MaxValue);

            var ChampMenu = new Menu("英雄", PlayerName + "_Plugin");
            {
                var ComboMenu = new Menu("连招", "Combo");
                {
                    var HealMenu = new Menu("Heal (W)", "Heal");
                    {
                        foreach (var Obj in ObjectManager.Get<Obj_AI_Hero>().Where(i => i.IsAlly))
                        {
                            var Name = MenuName(Obj);
                            AddItem(HealMenu, Name, Name);
                            AddItem(HealMenu, Name + "HpU", "-> If Hp Under", 40);
                        }
                        ComboMenu.AddSubMenu(HealMenu);
                    }
                    var SaveMenu = new Menu("Save (R)", "Save");
                    {
                        foreach (var Obj in ObjectManager.Get<Obj_AI_Hero>().Where(i => i.IsAlly))
                        {
                            var Name = MenuName(Obj);
                            AddItem(SaveMenu, Name, Name);
                            AddItem(SaveMenu, Name + "HpU", "-> If Hp Under", 30);
                        }
                        ComboMenu.AddSubMenu(SaveMenu);
                    }
                    var AntiMenu = new Menu("Anti (R)", "Anti");
                    {
                        AddItem(AntiMenu, "Zed", "Zed");
                        AddItem(AntiMenu, "Fizz", "Fizz");
                        AddItem(AntiMenu, "Vlad", "Vladimir");
                        AddItem(AntiMenu, "Karthus", "Karthus");
                        ComboMenu.AddSubMenu(AntiMenu);
                    }
                    AddItem(ComboMenu, "Q", "使用 Q");
                    AddItem(ComboMenu, "W", "使用 W");
                    AddItem(ComboMenu, "WSpeed", "-> 速度");
                    AddItem(ComboMenu, "WHeal", "-> 治疗");
                    AddItem(ComboMenu, "E", "使用 E");
                    AddItem(ComboMenu, "EAoE", "-> 集中AOE目标");
                    AddItem(ComboMenu, "R", "使用 R");
                    AddItem(ComboMenu, "RSave", "-> 保存");
                    AddItem(ComboMenu, "RAnti", "-> 大招", new[] { "Off", "Self", "Ally", "Both" }, 3);
                    ChampMenu.AddSubMenu(ComboMenu);
                }
                var HarassMenu = new Menu("骚扰", "Harass");
                {
                    AddItem(HarassMenu, "AutoQ", "使用 Q", "H", KeyBindType.Toggle);
                    AddItem(HarassMenu, "AutoQMpA", "-> 蓝量", 50);
                    AddItem(HarassMenu, "Q", "使用 Q");
                    AddItem(HarassMenu, "E", "使用 E");
                    ChampMenu.AddSubMenu(HarassMenu);
                }
                var ClearMenu = new Menu("清兵/清野", "Clear");
                {
                    var SmiteMob = new Menu("惩戒", "SmiteMob");
                    {
                        AddItem(SmiteMob, "Smite", "使用惩戒");
                        AddItem(SmiteMob, "Baron", "-> 大龙");
                        AddItem(SmiteMob, "Dragon", "-> 小龙");
                        AddItem(SmiteMob, "Red", "-> 红BUFF");
                        AddItem(SmiteMob, "Blue", "-> 蓝BUFF");
                        AddItem(SmiteMob, "Krug", "-> 蛤蟆");
                        AddItem(SmiteMob, "Gromp", "-> 石头人");
                        AddItem(SmiteMob, "Raptor", "-> 四猛禽");
                        AddItem(SmiteMob, "Wolf", "-> 三狼");
                        ClearMenu.AddSubMenu(SmiteMob);
                    }
                    AddItem(ClearMenu, "Q", "使用 Q");
                    AddItem(ClearMenu, "E", "使用 E");
                    ChampMenu.AddSubMenu(ClearMenu);
                }
                var LastHitMenu = new Menu("补兵", "LastHit");
                {
                    AddItem(LastHitMenu, "Q", "使用 Q");
                    ChampMenu.AddSubMenu(LastHitMenu);
                }
                var FleeMenu = new Menu("逃跑", "Flee");
                {
                    AddItem(FleeMenu, "Q", "使用 Q 减速敌人");
                    AddItem(FleeMenu, "W", "使用 W");
                    ChampMenu.AddSubMenu(FleeMenu);
                }
                var MiscMenu = new Menu("杂项", "Misc");
                {
                    var KillStealMenu = new Menu("抢人头", "KillSteal");
                    {
                        AddItem(KillStealMenu, "Q", "使用 Q");
                        AddItem(KillStealMenu, "Ignite", "使用点燃");
                        MiscMenu.AddSubMenu(KillStealMenu);
                    }
                    ChampMenu.AddSubMenu(MiscMenu);
                }
                var DrawMenu = new Menu("绘制", "Draw");
                {
                    AddItem(DrawMenu, "Q", "Q 范围", false);
                    AddItem(DrawMenu, "W", "W 范围", false);
                    AddItem(DrawMenu, "R", "R 范围", false);
                    ChampMenu.AddSubMenu(DrawMenu);
                }
                MainMenu.AddSubMenu(ChampMenu);
            }
            Game.OnUpdate += OnGameUpdate;
            Drawing.OnDraw += OnDraw;
        }

        private void OnGameUpdate(EventArgs args)
        {
            AntiDetect();
            if (Player.IsDead || MenuGUI.IsChatOpen || Player.IsRecalling()) return;
            if (Orbwalk.CurrentMode == Orbwalk.Mode.Combo || Orbwalk.CurrentMode == Orbwalk.Mode.Harass)
            {
                NormalCombo(Orbwalk.CurrentMode.ToString());
            }
            else if (Orbwalk.CurrentMode == Orbwalk.Mode.LaneClear)
            {
                LaneJungClear();
            }
            else if (Orbwalk.CurrentMode == Orbwalk.Mode.LastHit)
            {
                LastHit();
            }
            else if (Orbwalk.CurrentMode == Orbwalk.Mode.Flee) Flee();
            if (GetValue<KeyBind>("Harass", "AutoQ").Active) AutoQ();
            KillSteal();
        }

        private void OnDraw(EventArgs args)
        {
            if (Player.IsDead) return;
            if (GetValue<bool>("Draw", "Q") && Q.Level > 0) Render.Circle.DrawCircle(Player.Position, Q.Range, Q.IsReady() ? Color.Green : Color.Red);
            if (GetValue<bool>("Draw", "W") && W.Level > 0) Render.Circle.DrawCircle(Player.Position, W.Range, W.IsReady() ? Color.Green : Color.Red);
            if (GetValue<bool>("Draw", "R") && R.Level > 0) Render.Circle.DrawCircle(Player.Position, R.Range, R.IsReady() ? Color.Green : Color.Red);
        }

        private void NormalCombo(string Mode)
        {
            if (Mode == "Combo" && GetValue<bool>(Mode, "E") && GetValue<bool>(Mode, "EAoE") && Player.HasBuff("JudicatorRighteousFury"))
            {
                var Target = ObjectManager.Get<Obj_AI_Hero>().FindAll(i => Orbwalk.InAutoAttackRange(i)).MaxOrDefault(i => i.CountEnemiesInRange(150));
                if (Target != null) Orbwalk.ForcedTarget = Target;
            }
            else Orbwalk.ForcedTarget = null;
            if (GetValue<bool>(Mode, "Q"))
            {
                var Target = Q.GetTarget();
                if (Target != null && ((Player.Distance(Target, true) > Math.Pow(Q.Range - 100, 2) && !Target.IsFacing(Player)) || Target.HealthPercentage() > 60) && Q.CastOnUnit(Target, PacketCast)) return;
            }
            if (GetValue<bool>(Mode, "E") && E.IsReady() && E.GetTarget() != null && E.Cast(PacketCast)) return;
            if (Mode == "Combo")
            {
                if (GetValue<bool>(Mode, "W") && W.IsReady())
                {
                    if (GetValue<bool>(Mode, "WHeal"))
                    {
                        var Obj = ObjectManager.Get<Obj_AI_Hero>().FindAll(i => i.IsAlly && i.IsValidTarget(W.Range, false) && GetValue<bool>("Heal", MenuName(i)) && i.HealthPercentage() < GetValue<Slider>("Heal", MenuName(i) + "HpU").Value && !i.InFountain() && !i.IsRecalling() && i.CountEnemiesInRange(W.Range) > 0 && !i.HasBuff("JudicatorIntervention") && !i.HasBuff("Undying Rage")).MinOrDefault(i => i.Health);
                        if (Obj != null && W.CastOnUnit(Obj, PacketCast)) return;
                    }
                    if (GetValue<bool>(Mode, "WSpeed"))
                    {
                        var Target = Q.GetTarget(200);
                        if (Target != null && !Target.IsFacing(Player) && (!Player.HasBuff("JudicatorRighteousFury") || (Player.HasBuff("JudicatorRighteousFury") && !Orbwalk.InAutoAttackRange(Target))) && (!GetValue<bool>(Mode, "Q") || (GetValue<bool>(Mode, "Q") && Q.IsReady() && !Q.IsInRange(Target))) && W.Cast(PacketCast)) return;
                    }
                }
                if (GetValue<bool>(Mode, "R") && R.IsReady())
                {
                    if (GetValue<bool>(Mode, "RSave"))
                    {
                        var Obj = ObjectManager.Get<Obj_AI_Hero>().FindAll(i => i.IsAlly && i.IsValidTarget(R.Range, false) && GetValue<bool>("Save", MenuName(i)) && i.HealthPercentage() < GetValue<Slider>("Save", MenuName(i) + "HpU").Value && !i.InFountain() && !i.IsRecalling() && i.CountEnemiesInRange(R.Range) > 0 && !i.HasBuff("Undying Rage")).MinOrDefault(i => i.Health);
                        if (Obj != null && R.CastOnUnit(Obj, PacketCast)) return;
                    }
                    if (GetValue<StringList>(Mode, "RAnti").SelectedIndex > 0)
                    {
                        var Obj = ObjectManager.Get<Obj_AI_Hero>().FindAll(i => i.IsAlly && i.IsValidTarget(R.Range, false) && RAntiDetected.ContainsKey(i.NetworkId) && Game.Time > RAntiDetected[i.NetworkId].StartTick && !i.HasBuff("Undying Rage")).MinOrDefault(i => i.Health);
                        if (Obj != null && R.CastOnUnit(Obj, PacketCast)) return;
                    }
                }
            }
        }

        private void LaneJungClear()
        {
            var minionObj = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth);
            if (minionObj.Count == 0) return;
            if (GetValue<bool>("Clear", "Q") && Q.IsReady())
            {
                var Obj = minionObj.Find(i => CanKill(i, Q));
                if (Obj == null) Obj = minionObj.Find(i => i.MaxHealth >= 1200);
                if (Obj != null && Q.CastOnUnit(Obj, PacketCast)) return;
            }
            if (GetValue<bool>("Clear", "E") && E.IsReady() && (minionObj.Count > 1 || minionObj.Count(i => i.MaxHealth >= 1200) > 0) && E.Cast(PacketCast)) return;
            if (GetValue<bool>("SmiteMob", "Smite") && Smite.IsReady())
            {
                var Obj = minionObj.Find(i => i.Team == GameObjectTeam.Neutral && CanSmiteMob(i.Name));
                if (Obj != null && CastSmite(Obj)) return;
            }
        }

        private void LastHit()
        {
            if (!GetValue<bool>("LastHit", "Q") || !Q.IsReady()) return;
            var minionObj = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth).FindAll(i => CanKill(i, Q));
            if (minionObj.Count == 0 || Q.CastOnUnit(minionObj.First(), PacketCast)) return;
        }

        private void Flee()
        {
            if (GetValue<bool>("Flee", "W") && W.IsReady() && W.Cast(PacketCast)) return;
            if (GetValue<bool>("Flee", "Q") && Q.CastOnBestTarget(0, PacketCast).IsCasted()) return;
        }

        private void AutoQ()
        {
            if (Player.ManaPercentage() < GetValue<Slider>("Harass", "AutoQMpA").Value || Q.CastOnBestTarget(0, PacketCast).IsCasted()) return;
        }

        private void KillSteal()
        {
            if (GetValue<bool>("KillSteal", "Ignite") && Ignite.IsReady())
            {
                var Target = TargetSelector.GetTarget(600, TargetSelector.DamageType.True);
                if (Target != null && CastIgnite(Target)) return;
            }
            if (GetValue<bool>("KillSteal", "Q") && Q.IsReady())
            {
                var Target = Q.GetTarget();
                if (Target != null && CanKill(Target, Q) && Q.CastOnUnit(Target, PacketCast)) return;
            }
        }

        private void AntiDetect()
        {
            if (Player.IsDead || GetValue<StringList>("Combo", "RAnti").SelectedIndex == 0 || R.Level == 0) return;
            var Key = ObjectManager.Get<Obj_AI_Hero>().Find(i => i.IsAlly && i.IsValidTarget(float.MaxValue, false) && RAntiDetected.ContainsKey(i.NetworkId) && Game.Time > RAntiDetected[i.NetworkId].EndTick);
            if (Key != null) RAntiDetected.Remove(Key.NetworkId);
            foreach (var Obj in ObjectManager.Get<Obj_AI_Hero>().Where(i => i.IsAlly && i.IsValidTarget(float.MaxValue, false) && !RAntiDetected.ContainsKey(i.NetworkId)))
            {
                if ((GetValue<StringList>("Combo", "RAnti").SelectedIndex == 1 && Obj.IsMe) || (GetValue<StringList>("Combo", "RAnti").SelectedIndex == 2 && !Obj.IsMe) || GetValue<StringList>("Combo", "RAnti").SelectedIndex == 3)
                {
                    foreach (var Buff in Obj.Buffs)
                    {
                        if ((Buff.DisplayName == "ZedUltExecute" && GetValue<bool>("Anti", "Zed")) || (Buff.DisplayName == "FizzChurnTheWatersCling" && GetValue<bool>("Anti", "Fizz")) || (Buff.DisplayName == "VladimirHemoplagueDebuff" && GetValue<bool>("Anti", "Vlad")))
                        {
                            RAntiDetected.Add(Obj.NetworkId, new RAntiItem(Buff));
                        }
                        else if (Buff.DisplayName == "KarthusFallenOne" && GetValue<bool>("Anti", "Karthus") && Obj.Health <= ((Obj_AI_Hero)Buff.Caster).GetSpellDamage(Obj, SpellSlot.R) + Obj.Health * 0.2f && Obj.CountEnemiesInRange(R.Range) > 0) RAntiDetected.Add(Obj.NetworkId, new RAntiItem(Buff));
                    }
                }
            }
        }

        private string MenuName(Obj_AI_Hero Obj)
        {
            return Obj.IsMe ? "Self" : Obj.ChampionName;
        }
    }
}