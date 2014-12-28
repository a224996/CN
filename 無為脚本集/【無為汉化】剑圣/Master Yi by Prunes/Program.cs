using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using LX_Orbwalker;

namespace AlienHack_YiSharp
{
    internal class Program
    {
        public static Spell Q, W, E, R;
        public static List<Spell> SpellList = new List<Spell>();
        public static Obj_AI_Hero Player = ObjectManager.Player, TargetObj = null;
        public static SpellSlot IgniteSlot;
        public static Items.Item Tiamat = new Items.Item(3077, 375);
        public static Items.Item Hydra = new Items.Item(3074, 375);
        public static Items.Item BladeOfRuinKing = new Items.Item(3153, 450);
        public static Items.Item BlidgeWater = new Items.Item(3144, 450);
        public static Items.Item Youmuu = new Items.Item(3142, 200);
        public static Menu Config;
        public static String Name;


        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        private static void OnGameLoad(EventArgs args)
        {
            Name = Player.ChampionName;
            if (Name != "MasterYi") return;

            Q = new Spell(SpellSlot.Q, 600);
            W = new Spell(SpellSlot.W, 175);
            E = new Spell(SpellSlot.E, 175);
            R = new Spell(SpellSlot.R, 175);
            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
            IgniteSlot = ObjectManager.Player.GetSpellSlot("summonerdot");

            Config = new Menu("【無為汉化】无极剑圣", "AlienHack_" + Name, true);

            //Lxorbwalker
            var orbwalkerMenu = new Menu("走砍", "LX_Orbwalker");
            LXOrbwalker.AddToMenu(orbwalkerMenu);
            Config.AddSubMenu(orbwalkerMenu);

            //Add the target selector to the menu as submenu.
            var targetSelectorMenu = new Menu("目标选择器", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);


            //LaneClear
            Config.AddSubMenu(new Menu("清兵", "LaneClear"));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("UseQLaneClear", "使用 Q").SetValue(true));


            //Harass menu:
            Config.AddSubMenu(new Menu("骚扰", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "使用 Q").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "使用 E").SetValue(false));

            //Combo menu:
            Config.AddSubMenu(new Menu("连招", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("SelectedTarget", "重点选择的目标").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "使用 Q").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "使用 E").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "使用 R").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "W时取消AA").SetValue(true));

            //Misc
            Config.AddSubMenu(new Menu("杂项", "Misc"));
            Config.SubMenu("Misc")
                .AddItem(new MenuItem("MinQRange", "Q的最小范围").SetValue(new Slider(0, 0, 600)));
            Config.SubMenu("Misc").AddItem(new MenuItem("AutoTiamat", "自动提亚马特").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("AutoBOTRK", "自动破败").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("GapBOTRK", "追不上再使用破败").SetValue(true));
            Config.SubMenu("Misc")
                .AddItem(new MenuItem("MinBOTRK", "剩余血量使用破败 < x%").SetValue(new Slider(0, 0, 100)));
            Config.SubMenu("Misc").AddItem(new MenuItem("AutoYoumuu", "自动幽魂利刃").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("AutoIgnite", "自动点燃").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("AutoQSteal", "自动Q隐身").SetValue(true));

            //Draw
            Config.AddSubMenu(new Menu("范围", "Draw"));
            Config.SubMenu("Draw").AddItem(new MenuItem("QRange", "Q 范围").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));

            Config.AddToMainMenu();
            // End Menu

            Game.PrintChat("AlienHack [YiSharp - WujuMaster] Loaded!");
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            LXOrbwalker.AfterAttack += AfterAttack;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            var menuItem = Config.Item("QRange").GetValue<Circle>();
            if (menuItem.Active) {
                Utility.DrawCircle(Player.Position, getQRange(), Color.Red);
                Utility.DrawCircle(Player.Position, Q.Range, Color.Cyan);
            }
        }

        private static void AfterAttack(Obj_AI_Base unit, Obj_AI_Base target)
        {
            if (!unit.IsMe || target.IsMinion)
                return;

            if (IsTiamat() && (LXOrbwalker.CurrentMode == LXOrbwalker.Mode.Combo || LXOrbwalker.CurrentMode == LXOrbwalker.Mode.Harass) &&
                target.IsValidTarget(Tiamat.Range))
            {
                Tiamat.Cast();
                LXOrbwalker.ResetAutoAttackTimer();
            }

            if (IsHydra() && (LXOrbwalker.CurrentMode == LXOrbwalker.Mode.Combo || LXOrbwalker.CurrentMode == LXOrbwalker.Mode.Harass) && target.IsValidTarget(Hydra.Range))
            {
                Hydra.Cast();
                LXOrbwalker.ResetAutoAttackTimer();
            }

            if (IsWCombo() && LXOrbwalker.CurrentMode == LXOrbwalker.Mode.Combo && LXOrbwalker.InAutoAttackRange(target))
            {
                W.Cast();
                Utility.DelayAction.Add(100, () => Player.IssueOrder(GameObjectOrder.AttackTo, target));
                LXOrbwalker.ResetAutoAttackTimer();
            }
        }

        private static void Ks()
        {
            List<Obj_AI_Hero> nearChamps = (from champ in ObjectManager.Get<Obj_AI_Hero>()
                where Player.Distance(champ.ServerPosition) <= 600 && champ.IsEnemy
                select champ).ToList();
            nearChamps.OrderBy(x => x.Health);

            foreach (Obj_AI_Hero target in nearChamps)
            {
                //ignite
                if (target != null && IsIgnite() && Player.Distance(target.ServerPosition) <= 600)
                {
                    if (Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite) > target.Health)
                    {
                        Player.Spellbook.CastSpell(IgniteSlot, target);
                    }
                }

                if (Player.Distance(target.ServerPosition) <= Q.Range &&
                    (Player.GetSpellDamage(target, SpellSlot.Q)) > target.Health)
                {
                    if (Q.IsReady())
                    {
                        Q.Cast(target);
                    }
                }
            }
        }


        private static int getQRange()
        {
            return Config.Item("MinQRange").GetValue<Slider>().Value;
        }

        private static int getBOTKRPercent()
        {
            return Config.Item("MinBOTRK").GetValue<Slider>().Value;
        }

        private static bool getBOTRKGap()
        {
            return Config.Item("GapBOTRK").GetValue<bool>();
        }

        private static bool IsQSteal()
        {
            if (Config.Item("AutoQSteal").GetValue<bool>())
            {
                return Q.IsReady();
            }
            return false;
        }

        private static bool IsTiamat()
        {
            if (Config.Item("AutoTiamat").GetValue<bool>())
            {
                return Tiamat.IsReady();
            }
            return false;
        }

        private static bool IsIgnite()
        {
            if (Config.Item("AutoIgnite").GetValue<bool>())
            {
                if (Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                {
                    //Game.PrintChat("Ignite Enabled");
                    return true;
                }
            }
            return false;
        }

        private static bool IsHydra()
        {
            if (Config.Item("AutoTiamat").GetValue<bool>())
            {
                return Hydra.IsReady();
            }
            return false;
        }

        private static bool IsBOTRK()
        {
            if (Config.Item("AutoBOTRK").GetValue<bool>())
            {
                return BladeOfRuinKing.IsReady();
            }
            return false;
        }

        private static bool IsBilge()
        {
            if (Config.Item("AutoBOTRK").GetValue<bool>())
            {
                return BlidgeWater.IsReady();
            }
            return false;
        }

        private static bool IsYoumuu()
        {
            if (Config.Item("AutoYoumuu").GetValue<bool>())
            {
                return Youmuu.IsReady();
            }
            return false;
        }

        private static bool IsQLaneClear()
        {
            if (Config.Item("UseQLaneClear").GetValue<bool>())
            {
                return Q.IsReady();
            }
            return false;
        }

        private static bool IsQHarass()
        {
            if (Config.Item("UseQHarass").GetValue<bool>())
            {
                return Q.IsReady();
            }
            return false;
        }

        private static bool IsEHarass()
        {
            if (Config.Item("UseEHarass").GetValue<bool>())
            {
                return E.IsReady();
            }
            return false;
        }

        private static bool IsQCombo()
        {
            if (Config.Item("UseQCombo").GetValue<bool>())
            {
                return Q.IsReady();
            }
            return false;
        }

        private static bool IsWCombo()
        {
            if (Config.Item("UseWCombo").GetValue<bool>())
            {
                return W.IsReady();
            }
            return false;
        }

        private static bool IsECombo()
        {
            if (Config.Item("UseECombo").GetValue<bool>())
            {
                return E.IsReady();
            }
            return false;
        }

        private static bool IsRCombo()
        {
            if (Config.Item("UseRCombo").GetValue<bool>())
            {
                return R.IsReady();
            }
            return false;
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            switch (LXOrbwalker.CurrentMode)
            {
                case LXOrbwalker.Mode.LaneClear:
                    DoLaneClear();
                    break;
                case LXOrbwalker.Mode.Harass:
                    DoHarass();
                    break;
                case LXOrbwalker.Mode.Combo:
                    DoCombo();
                    break;
            }

            Ks();
        }

        private static void DoCombo()
        {
		//xSalice for target selector
            var focusSelected = Config.Item("SelectedTarget").GetValue<bool>();
            Obj_AI_Hero target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);

            if (TargetSelector.GetSelectedTarget() != null)
                if (focusSelected && TargetSelector.GetSelectedTarget().Distance(Player.ServerPosition) < Q.Range)
                    target = TargetSelector.GetSelectedTarget();

            if (target == null) return;

            if (IsQCombo() && Player.Distance(target) >= getQRange())
            {
                Q.Cast(target);
            }

            if (IsECombo() && E.Range >= Player.Distance(target))
            {
                E.Cast();
            }

            if (IsRCombo() && R.Range >= Player.Distance(target))
            {
                R.Cast();
            }

            if (IsBOTRK() && BladeOfRuinKing.Range >= Player.Distance(target))
            {
                if (getBOTRKGap())
                {
                    if (!LXOrbwalker.InAutoAttackRange(target))
                        BladeOfRuinKing.Cast(target);
                }
                else
                {
                    if ((Player.Health/Player.MaxHealth)*100 <= getBOTKRPercent())
                    {
                        BladeOfRuinKing.Cast(target);
                    }
                }
            }

            if (IsBilge() && BlidgeWater.Range >= Player.Distance(target))
            {
                if (getBOTRKGap())
                {
                    if (!LXOrbwalker.InAutoAttackRange(target))
                        BlidgeWater.Cast(target);
                }
                else
                {
                    BlidgeWater.Cast(target);
                }
            }

            if (IsYoumuu() && Youmuu.Range >= Player.Distance(target))
            {
                Youmuu.Cast();
            }
        }

        private static void DoHarass()
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (target == null) return;

            if (IsQHarass() && Player.Distance(target) >= getQRange())
            {
                Q.Cast(target);
            }

            if (IsEHarass() && E.Range >= Player.Distance(target))
            {
                E.Cast();
            }
        }

        private static void DoLaneClear()
        {
            //Find All Minion
            List<Obj_AI_Base> allMinions = MinionManager.GetMinions(Player.ServerPosition, Q.Range,
                MinionTypes.All, MinionTeam.NotAlly);
            List<Obj_AI_Base> jungleMinions = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All,
                MinionTeam.Neutral);
            allMinions.AddRange(jungleMinions);

            //Auto Q
            if (IsQLaneClear() && allMinions.Count > 0)
            {
                foreach (
                    Obj_AI_Base minion in
                        allMinions.Where(minion => minion.IsValidTarget())
                            .Where(minion => Q.Range >= Player.Distance(minion))
                            .OrderBy(minion => Player.Distance(minion)))
                {
                    Q.Cast(minion);
                    break;
                }
            }

            //Auto Tiamat
            if (IsTiamat() && allMinions.Count > 0)
            {
                foreach (
                    Obj_AI_Base minion in
                        allMinions.Where(minion => minion.IsValidTarget())
                            .Where(minion => Tiamat.Range >= Player.Distance(minion)))
                {
                    Tiamat.Cast();
                    break;
                }
            }

            //Auto Hydra
            if (IsHydra() && allMinions.Count > 0)
            {
                foreach (
                    Obj_AI_Base minion in
                        allMinions.Where(minion => minion.IsValidTarget())
                            .Where(minion => Hydra.Range >= Player.Distance(minion)))
                {
                    Hydra.Cast();
                    break;
                }
            }
        }
    }
}