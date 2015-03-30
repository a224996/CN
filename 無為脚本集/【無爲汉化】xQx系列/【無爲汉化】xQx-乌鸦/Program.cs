#region
using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

#endregion

namespace Swain
{
    internal class Program
    {
        public const string ChampionName = "Swain";
        private static readonly Obj_AI_Hero vPlayer = ObjectManager.Player;

        //Orbwalker instance
        public static Orbwalking.Orbwalker Orbwalker;

        //Spells
        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q, W, E, R;
        public static SpellSlot IgniteSlot;
        private static bool UltiActive;
        

        //Menu
        public static Menu Config;
        public static Menu MenuExtras;
        public static Menu MenuTargetedItems;
        public static Menu MenuNonTargetedItems;
        public static Items.Item Fqc = new Items.Item(3188, 750); // Frost Queen's Claim; 
        public static Items.Item Dfg = new Items.Item(3128, 750);

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (vPlayer.BaseSkinName != ChampionName) return;

            

            //Create the spells
             Q = new Spell(SpellSlot.Q, 625);
            W = new Spell(SpellSlot.W, 820);
            E = new Spell(SpellSlot.E, 625);
            R = new Spell(SpellSlot.R, 650);

            Q.SetTargetted(0.5f, float.MaxValue);
            W.SetSkillshot(1.2f, 125f, float.MaxValue, false, SkillshotType.SkillshotCone);
            E.SetTargetted(0.5f, float.MaxValue);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            IgniteSlot = vPlayer.GetSpellSlot("SummonerDot");
            //Create the menu
            Config = new Menu("【無爲汉化】xQx-乌鸦", ChampionName, true);

            //Orbwalker submenu
            Config.AddSubMenu(new Menu("走砍", "Orbwalking"));

            //Add the target selector to the menu as submenu.
            var targetSelectorMenu = new Menu("目标选择器", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);

            Config.AddSubMenu(targetSelectorMenu);

            //Load the orbwalker and add it to the menu as submenu.
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            //Combo menu:
            Config.AddSubMenu(new Menu("连招", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "使用 Q").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "使用 W").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "使用 E").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "使用 R").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseAutoRCombo", "自动 R").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseIgniteCombo", "使用点燃").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseDFGCombo", "使用冥火").SetValue(true));
            Config.SubMenu("Combo")
                .AddItem(
                    new MenuItem("ComboActive", "连招!").SetValue(
                        new KeyBind(Config.Item("Orbwalk").GetValue<KeyBind>().Key, KeyBindType.Press)));

            //Harass menu:
            Config.AddSubMenu(new Menu("骚扰", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "使用 Q").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "使用 W").SetValue(false));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "使用 E").SetValue(false));
            Config.SubMenu("Harass").AddItem(new MenuItem("HarassMana", "最低蓝量:").SetValue(new Slider(50, 100, 0)));

            Config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("HarassActive", "骚扰!").SetValue(
                        new KeyBind(Config.Item("Farm").GetValue<KeyBind>().Key, KeyBindType.Press)));
            Config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("HarassActiveT", "骚扰 (自动)!").SetValue(new KeyBind("Y".ToCharArray()[0],
                        KeyBindType.Toggle)));

            //Farming menu:
            Config.AddSubMenu(new Menu("清线", "LaneClear"));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("UseWLaneClear", "使用 W").SetValue(false));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("UseELaneClear", "使用 E").SetValue(false));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("LaneClearMana", "最低蓝量:").SetValue(new Slider(50, 100, 0)));

            Config.SubMenu("LaneClear")
                .AddItem(
                    new MenuItem("LaneClearActive", "清线!").SetValue(
                        new KeyBind(Config.Item("LaneClear").GetValue<KeyBind>().Key, KeyBindType.Press)));

            //JungleFarm menu:
            Config.AddSubMenu(new Menu("清野", "JungleFarm"));
            Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseQJFarm", "使用 Q").SetValue(true));
            Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseWJFarm", "使用 W").SetValue(true));
            Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseEJFarm", "使用 E").SetValue(true));
            Config.SubMenu("JungleFarm").AddItem(new MenuItem("JungleFarmMana", "最低蓝量:").SetValue(new Slider(50, 100, 0)));
            Config.SubMenu("JungleFarm")
                .AddItem(
                    new MenuItem("JungleFarmActive", "清野!").SetValue(
                        new KeyBind(Config.Item("LaneClear").GetValue<KeyBind>().Key, KeyBindType.Press)));

            //Misc
            // Extras -> Use Items 
            MenuExtras = new Menu("额外", "Extras");
            Config.AddSubMenu(MenuExtras);
            MenuExtras.AddItem(new MenuItem("InterruptSpells", "中断法术").SetValue(true));

            Menu menuUseItems = new Menu("使用物品", "menuUseItems");
            Config.SubMenu("Extras").AddSubMenu(menuUseItems);
            // Extras -> Use Items -> Targeted Items
            MenuTargetedItems = new Menu("攻击物品", "menuTargetItems");
            menuUseItems.AddSubMenu(MenuTargetedItems);

            MenuTargetedItems.AddItem(new MenuItem("item3153", "破败").SetValue(true));
            MenuTargetedItems.AddItem(new MenuItem("item3143", "兰盾").SetValue(true));
            MenuTargetedItems.AddItem(new MenuItem("item3144", "比尔吉沃特弯刀").SetValue(true));

            MenuTargetedItems.AddItem(new MenuItem("item3146", "科技枪").SetValue(true));
            MenuTargetedItems.AddItem(new MenuItem("item3184", "冰锤").SetValue(true));

            // Extras -> Use Items -> AOE Items
            MenuNonTargetedItems = new Menu("AOE物品", "menuNonTargetedItems");
            menuUseItems.AddSubMenu(MenuNonTargetedItems);
            MenuNonTargetedItems.AddItem(new MenuItem("item3180", "奥丁面纱").SetValue(true));
            MenuNonTargetedItems.AddItem(new MenuItem("item3131", "神圣之剑").SetValue(true));
            MenuNonTargetedItems.AddItem(new MenuItem("item3074", "贪欲九头蛇").SetValue(true));
            MenuNonTargetedItems.AddItem(new MenuItem("item3077", "提亚马特").SetValue(true));
            MenuNonTargetedItems.AddItem(new MenuItem("item3142", "幽梦之灵").SetValue(true));

            //Drawings menu:
            Config.AddSubMenu(new Menu("范围", "Drawings"));
            Config.SubMenu("Drawings")
                .AddItem(new MenuItem("QRange", "Q 范围").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            Config.SubMenu("Drawings")
                .AddItem(new MenuItem("WRange", "W 范围").SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
            Config.SubMenu("Drawings")
                .AddItem(new MenuItem("ERange", "E 范围").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            Config.SubMenu("Drawings")
                .AddItem(new MenuItem("RRange", "R 范围").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));

            /* [ Damage After Combo ] */
            var dmgAfterComboItem = new MenuItem("DamageAfterCombo", "连招损伤").SetValue(true);
            Config.SubMenu("Drawings").AddItem(dmgAfterComboItem);

            Utility.HpBarDamageIndicator.DamageToUnit = GetComboDamage;
            Utility.HpBarDamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
            dmgAfterComboItem.ValueChanged += delegate(object sender, OnValueChangeEventArgs eventArgs)
            {
                Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
            };

            new PotionManager();
            Config.AddToMainMenu();

            //Add the events we are going to use:
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            GameObject.OnCreate += OnCreateObject;
            GameObject.OnDelete += OnDeleteObject;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;

            Game.PrintChat(
                String.Format(
                    "<font color='#70DBDB'>xQx </font> <font color='#FFFFFF'>{0}</font> <font color='#70DBDB'> Loaded!</font>",
                    ChampionName));
        }

        private static void Interrupter_OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!Config.Item("InterruptSpells").GetValue<bool>()) return;
            E.Cast(unit);
        }

        private static void OnCreateObject(GameObject sender, EventArgs args)
        {
            if (!(sender.Name.Contains("swain_demonForm"))) return;
            UltiActive = true;
        }
        private static void OnDeleteObject(GameObject sender, EventArgs args)
        {
            if (!(sender.Name.Contains("swain_demonForm"))) return;
            UltiActive = false;
        }

        private static void Combo()
        {
            Orbwalker.SetAttack(!(Q.IsReady() || W.IsReady() || E.IsReady()));
            UseSpells(
                Config.Item("UseQCombo").GetValue<bool>(), 
                Config.Item("UseWCombo").GetValue<bool>(),
                Config.Item("UseECombo").GetValue<bool>(), 
                Config.Item("UseRCombo").GetValue<bool>(),
                Config.Item("UseIgniteCombo").GetValue<bool>()
                );
        }

        private static void Harass()
        {
            var existsMana = vPlayer.MaxMana / 100 * Config.Item("JungleFarmMana").GetValue<Slider>().Value;
            if (vPlayer.Mana <= existsMana) return;

            UseSpells(
                Config.Item("UseQHarass").GetValue<bool>(), 
                Config.Item("UseWHarass").GetValue<bool>(),
                Config.Item("UseEHarass").GetValue<bool>(), 
                false, 
                false
                );
        }

        private static float GetComboDamage(Obj_AI_Base t)
        {
            var fComboDamage = 0d;

            if (Q.IsReady())
                fComboDamage += vPlayer.GetSpellDamage(t, SpellSlot.Q);

            if (W.IsReady())
                fComboDamage += W.Instance.Ammo*vPlayer.GetSpellDamage(t, SpellSlot.W);
   
            if (E.IsReady())
                fComboDamage += vPlayer.GetSpellDamage(t, SpellSlot.E);

            if (IgniteSlot != SpellSlot.Unknown && vPlayer.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                fComboDamage += ObjectManager.Player.GetSummonerSpellDamage(t, Damage.SummonerSpell.Ignite);

            if (Config.Item("item3128").GetValue<bool>() && Items.CanUseItem(3128))
                fComboDamage += ObjectManager.Player.GetItemDamage(t, Damage.DamageItems.Dfg);

            if (R.IsReady() && !UltiActive)
                fComboDamage += vPlayer.GetSpellDamage(t, SpellSlot.R) * 3;

            return (float)fComboDamage;
        }

        private static void UseSpells(bool useQ, bool useW, bool useE, bool useR, bool useIgnite)
        {
            var qTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var wTarget = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
            var eTarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
            var rTarget = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);


            if (useE && eTarget != null && E.IsReady())
            {
                E.Cast(eTarget);
            }

            if (useQ && qTarget != null && Q.IsReady())
            {
                Q.Cast(qTarget);
            }

            if (qTarget != null && Dfg.IsReady())
            {
                Dfg.Cast(qTarget);
            }

            if (useW && wTarget != null && W.IsReady())
            {
                W.Cast(wTarget);
            }

            if (qTarget != null && useIgnite && IgniteSlot != SpellSlot.Unknown &&
                vPlayer.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
            {
                if (vPlayer.Distance(qTarget) < 650 && GetComboDamage(qTarget) > qTarget.Health)
                {
                    vPlayer.Spellbook.CastSpell(IgniteSlot, qTarget);
                    UseItems(qTarget);
                }
            }

            if (useR && rTarget != null && R.IsReady() && !UltiActive)
            {
                R.Cast();
            }

        }

        private static void Farm()
        {
            if (!Orbwalking.CanMove(40)) return;

            var existsMana = vPlayer.MaxMana / 100 * Config.Item("LaneClearMana").GetValue<Slider>().Value;
            if (vPlayer.Mana <= existsMana) return;

            var useW = Config.Item("UseWLaneClear").GetValue<bool>();
            var useE = Config.Item("UseELaneClear").GetValue<bool>();

            if (useW && W.IsReady())
            {
                var minionsW = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, W.Range + W.Width,
                    MinionTypes.Ranged);
                var wPos = W.GetCircularFarmLocation(minionsW);
                if (wPos.MinionsHit >= 3)
                    W.Cast(wPos.Position);
            }
        
            if (useE && E.IsReady())
            {
                var minionsE = MinionManager.GetMinions(vPlayer.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.Health);
                foreach (var vMinion in minionsE)
                {
                    var vMinionEDamage = vPlayer.GetSpellDamage(vMinion, SpellSlot.E);

                    if (vMinion.Health <= vMinionEDamage - 20)
                        E.CastOnUnit(vMinion);
                }
            }
        }

        private static void JungleFarm()
        {
            var existsMana = vPlayer.MaxMana / 100 * Config.Item("JungleFarmMana").GetValue<Slider>().Value;
            if (vPlayer.Mana <= existsMana) return;

            var useQ = Config.Item("UseQJFarm").GetValue<bool>();
            var useW = Config.Item("UseWJFarm").GetValue<bool>();
            var useE = Config.Item("UseEJFarm").GetValue<bool>();

            var mobs = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, W.Range, MinionTypes.All,
                MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (mobs.Count > 0)
            {
                var mob = mobs[0];
                if (useQ && Q.IsReady())
                    Q.CastOnUnit(mob);

                if (useW && W.IsReady())
                    W.Cast(mob);

                if (useE && E.IsReady())
                    E.CastOnUnit(mob);
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (vPlayer.IsDead) return;
            
            Orbwalker.SetAttack(true);

            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            else
            {
                if (Config.Item("HarassActive").GetValue<KeyBind>().Active ||
                    Config.Item("HarassActiveT").GetValue<KeyBind>().Active)
                    Harass();

                if (Config.Item("LaneClearActive").GetValue<KeyBind>().Active)
                    Farm();

                if (Config.Item("JungleFarmActive").GetValue<KeyBind>().Active)
                    JungleFarm();
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            foreach (var spell in SpellList)
            {
                var menuItem = Config.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active && spell.Level > 0)
                    Render.Circle.DrawCircle(vPlayer.Position, spell.Range, menuItem.Color);
            }
        }
        private static InventorySlot GetInventorySlot(int id)
        {
            return ObjectManager.Player.InventoryItems.FirstOrDefault(item => (item.Id == (ItemId)id && item.Stacks >= 1) || (item.Id == (ItemId)id && item.Charges >= 1));
        }

        public static void UseItems(Obj_AI_Hero vTarget)
        {
            if (vTarget != null)
            {
                foreach (MenuItem menuItem in MenuTargetedItems.Items)
                {
                    var useItem = MenuTargetedItems.Item(menuItem.Name).GetValue<bool>();
                    if (useItem)
                    {
                        var itemID = Convert.ToInt16(menuItem.Name.ToString().Substring(4, 4));
                        if (Items.HasItem(itemID) && Items.CanUseItem(itemID) && GetInventorySlot(itemID) != null)
                            Items.UseItem(itemID, vTarget);
                    }
                }

                foreach (MenuItem menuItem in MenuNonTargetedItems.Items)
                {
                    var useItem = MenuNonTargetedItems.Item(menuItem.Name).GetValue<bool>();
                    if (useItem)
                    {
                        var itemID = Convert.ToInt16(menuItem.Name.ToString().Substring(4, 4));
                        if (Items.HasItem(itemID) && Items.CanUseItem(itemID) && GetInventorySlot(itemID) != null)
                            Items.UseItem(itemID);
                    }
                }

            }
        }

       
    }
}
