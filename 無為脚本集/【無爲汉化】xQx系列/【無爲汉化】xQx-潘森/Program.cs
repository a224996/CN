#region
using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
#endregion

namespace Pantheon
{
    internal class Program
    {
        public const string ChampionName = "Pantheon";
        private static readonly Obj_AI_Hero Player = ObjectManager.Player;
        //Orbwalker instance
        public static Orbwalking.Orbwalker Orbwalker;

        //Spells
        public static List<Spell> SpellList = new List<Spell>();
        public static Spell Q;
        public static Spell E;
        public static Spell W;
        public static Spell R;

        private static SpellSlot IgniteSlot;
        private static readonly Items.Item Tiamat = new Items.Item(3077, 450);

        public static float EDelay = 0;
        private static bool UsingE = false;
        private static Spell[] junglerLevel = {E, Q, W, Q, Q, R, Q, E, Q, E, R, E, W, E, W, R, W, W};
        private static Spell[] topLanerLevel = {Q, E, Q, W, Q, R, Q, E, Q, E, R, E, W, E, W, R, W, W};

        public static int DelayTick = 0;

        //Menu
        public static Menu Config;
        public static Menu MenuExtras;
        public static Menu MenuTargetedItems;
        public static Menu MenuNonTargetedItems;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.BaseSkinName != "Pantheon") return;
            if (Player.IsDead) return;


            
            Q = new Spell(SpellSlot.Q, 620f);
            W = new Spell(SpellSlot.W, 620f);
            E = new Spell(SpellSlot.E, 640f);
            R = new Spell(SpellSlot.R, 2000f);

            Q.SetTargetted(0.2f, 1700f);
            W.SetTargetted(0.2f, 1700f);
            E.SetSkillshot(0.25f, 15f * 2 * (float)Math.PI / 180, 2000f, false, SkillshotType.SkillshotCone);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            IgniteSlot = Player.GetSpellSlot("SummonerDot");
            
            Config = new Menu("【無爲汉化】xQx-潘森", "Pantheon", true);

            var targetSelectorMenu = new Menu("目标选择器", "TargetSelector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);
            
            Config.AddSubMenu(new Menu("走砍", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));
            
            // Combo
            Config.AddSubMenu(new Menu("连招", "Combo"));
            {
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "使用 Q").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "使用 W").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "使用 E").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "使用 R").SetValue(true));
                Config.SubMenu("Combo")
                    .AddItem(
                        new MenuItem("ComboActive", "连招!").SetValue(
                            new KeyBind(Config.Item("Orbwalk").GetValue<KeyBind>().Key, KeyBindType.Press)));
            }
            
            Config.AddSubMenu(new Menu("骚扰", "Harass"));
            {
            Config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "使用 Q").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "使用 W").SetValue(true));
                Config.SubMenu("Harass")
                    .AddItem(new MenuItem("HarassMana", "最低蓝量: ").SetValue(new Slider(50, 100, 0)));
                Config.SubMenu("Harass")
                    .AddItem(
                        new MenuItem("HarassActiveT", "骚扰 (切换)!").SetValue(new KeyBind("H".ToCharArray()[0],
                            KeyBindType.Toggle)));
                Config.SubMenu("Harass")
                    .AddItem(
                        new MenuItem("HarassActive", "骚扰!").SetValue(new KeyBind("C".ToCharArray()[0],
                            KeyBindType.Press)));
            }
            // Lane Clear
            Config.AddSubMenu(new Menu("清线", "LaneClear"));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("UseQLaneClear", "使用 Q").SetValue(false));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("UseELaneClear", "使用 E").SetValue(false));
            Config.SubMenu("LaneClear")
                .AddItem(new MenuItem("LaneClearMana", "最低蓝量%: ").SetValue(new Slider(50, 100, 0)));
            Config.SubMenu("LaneClear")
                .AddItem(
                    new MenuItem("LaneClearActive", "清兵!").SetValue(new KeyBind("V".ToCharArray()[0],
                        KeyBindType.Press)));

            // Jungling Farm
            Config.AddSubMenu(new Menu("清野", "JungleFarm"));
            {
            Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseQJungleFarm", "使用 Q").SetValue(true));
            Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseEJungleFarm", "使用 E").SetValue(false));
                Config.SubMenu("JungleFarm")
                    .AddItem(new MenuItem("JungleFarmMana", "最低蓝量: ").SetValue(new Slider(50, 100, 0)));
                Config.SubMenu("JungleFarm")
                    .AddItem(
                        new MenuItem("JungleFarmActive", "清野!").SetValue(new KeyBind("V".ToCharArray()[0],
                            KeyBindType.Press)));
            }
            // Extras -> Use Items 
            MenuExtras = new Menu("额外", "Extras");
            {
            Config.AddSubMenu(MenuExtras);
            MenuExtras.AddItem(new MenuItem("InterruptSpells", "中断法术").SetValue(true));
            MenuExtras.AddItem(new MenuItem("AutoLevelUp", "自动加点").SetValue(true));
            }

            Menu menuUseItems = new Menu("使用物品", "menuUseItems");
            {
                Config.SubMenu("Extras").AddSubMenu(menuUseItems);

            // Extras -> Use Items -> Targeted Items
            MenuTargetedItems = new Menu("攻击物品", "menuTargetItems");
                {
            menuUseItems.AddSubMenu(MenuTargetedItems);
            MenuTargetedItems.AddItem(new MenuItem("item3153", "破败").SetValue(true));
            MenuTargetedItems.AddItem(new MenuItem("item3143", "兰盾").SetValue(true));
            MenuTargetedItems.AddItem(new MenuItem("item3144", "比尔吉沃特弯刀").SetValue(true));
            MenuTargetedItems.AddItem(new MenuItem("item3146", "科技枪").SetValue(true));
            MenuTargetedItems.AddItem(new MenuItem("item3184", "冰锤").SetValue(true));
                }
            // Extras -> Use Items -> AOE Items
            MenuNonTargetedItems = new Menu("AOE物品", "menuNonTargetedItems");
                {
            menuUseItems.AddSubMenu(MenuNonTargetedItems);
            MenuNonTargetedItems.AddItem(new MenuItem("item3180", "奥丁面纱").SetValue(true));
            MenuNonTargetedItems.AddItem(new MenuItem("item3131", "神圣之剑").SetValue(true));
            MenuNonTargetedItems.AddItem(new MenuItem("item3074", "贪欲九头蛇").SetValue(true));
            MenuNonTargetedItems.AddItem(new MenuItem("item3077", "提亚马特 ").SetValue(true));
            MenuNonTargetedItems.AddItem(new MenuItem("item3142", "幽梦之灵").SetValue(true));
                }
            }
            // Drawing
            Config.AddSubMenu(new Menu("范围", "Drawings"));
            {
                Config.SubMenu("Drawings")
                    .AddItem(
                        new MenuItem("QRange", "Q 范围").SetValue(new Circle(false,
                            System.Drawing.Color.FromArgb(255, 255, 255, 255))));
                Config.SubMenu("Drawings")
                    .AddItem(
                        new MenuItem("WRange", "W 范围").SetValue(new Circle(false,
                            System.Drawing.Color.FromArgb(255, 255, 255, 255))));
                Config.SubMenu("Drawings")
                    .AddItem(
                        new MenuItem("ERange", "E 范围").SetValue(new Circle(false,
                            System.Drawing.Color.FromArgb(255, 255, 255, 255))));
                Config.SubMenu("Drawings")
                    .AddItem(
                        new MenuItem("RRange", "R 范围").SetValue(new Circle(false,
                            System.Drawing.Color.FromArgb(255, 255, 255, 255))));

                var dmgAfterComboItem = new MenuItem("DamageAfterCombo", "连招损伤").SetValue(true);
                Config.SubMenu("Drawings").AddItem(dmgAfterComboItem);

                Utility.HpBarDamageIndicator.DamageToUnit = GetComboDamage;
                Utility.HpBarDamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
                dmgAfterComboItem.ValueChanged += delegate(object sender, OnValueChangeEventArgs eventArgs)
                {
                    Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
                };

            }
            new PotionManager();
            Config.AddToMainMenu();

            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnProcessSpellCast += Game_OnProcessSpell;
            GameObject.OnDelete += Game_OnObjectDelete;

            CustomEvents.Unit.OnLevelUp += CustomEvents_Unit_OnLevelUp;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPosibleToInterrupt;
            WelcomeMessage();
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            foreach (var spell in SpellList)
            {
                var menuItem = Config.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active && spell.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, spell.Range, menuItem.Color);
            }

            Render.Circle.DrawCircle(Player.Position, 30f, System.Drawing.Color.Red, 1, true);
        }

        private static void Game_OnProcessSpell(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs spell)
        {
            if (!unit.IsMe) return;
            UsingE = false;
            if (spell.SData.Name.ToLower() != "pantheone") return;
            UsingE = true;
            Utility.DelayAction.Add(750, () => UsingE = false);
        }
        private static void Game_OnObjectDelete(GameObject sender, EventArgs args)
        {
            if (!sender.Name.Contains("Pantheon_") || !sender.Name.Contains("_E_cas.troy")) return;
            UsingE = false;
        }

        public static void CustomEvents_Unit_OnLevelUp(Obj_AI_Base sender, CustomEvents.Unit.OnLevelUpEventArgs args)
        {
            if (sender.NetworkId != Player.NetworkId)
                return;
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            //if (!Orbwalking.CanMove(100)) return;

            Orbwalker.SetAttack(!SoundActive());
            Orbwalker.SetMovement(!SoundActive());
            
            if (DelayTick - Environment.TickCount <= 250)
            {
                UseSummoners();
                DelayTick = Environment.TickCount;
            }
            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
            }

            if (Config.Item("HarassActive").GetValue<KeyBind>().Active ||
                Config.Item("HarassActiveT").GetValue<KeyBind>().Active) 
            {
                var existsMana = Config.Item("HarassMana").GetValue<Slider>().Value;
                if (Player.ManaPercentage() >= existsMana)
                    Harass();
            }

            if (Config.Item("LaneClearActive").GetValue<KeyBind>().Active)
            {
                var existsMana = Config.Item("LaneClearMana").GetValue<Slider>().Value;
                if (Player.ManaPercentage() >= existsMana)
                    LaneClear();
            }

            if (Config.Item("JungleFarmActive").GetValue<KeyBind>().Active)
            {
                var existsMana = Config.Item("JungleFarmMana").GetValue<Slider>().Value;
                if (Player.ManaPercentage() >= existsMana)
                    JungleFarm();
            }
        }

        private static float GetComboDamage(Obj_AI_Hero t)
        {
            var fComboDamage = 0d;

            if (Q.IsReady())
                fComboDamage += Player.GetSpellDamage(t, SpellSlot.Q);

            if (W.IsReady())
                fComboDamage += Player.GetSpellDamage(t, SpellSlot.W);

            if (E.IsReady())
                fComboDamage += Player.GetSpellDamage(t, SpellSlot.E);

            if (R.IsReady())
                fComboDamage += Player.GetSpellDamage(t, SpellSlot.R);

            if (IgniteSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                fComboDamage += Player.GetSummonerSpellDamage(t, Damage.SummonerSpell.Ignite);

            return (float)fComboDamage;
        }

        private static void Combo()
        {
            if (SoundActive()) return;

            var qTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            var wTarget = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);
            var eTarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);

            var useQ = Config.Item("UseQCombo").GetValue<bool>();
            var useW = Config.Item("UseWCombo").GetValue<bool>();
            var useE = Config.Item("UseECombo").GetValue<bool>();

            if (Q.IsReady() && useQ && qTarget != null)
            {
                Q.CastOnUnit(qTarget);
            }

            if (W.IsReady() && useW && wTarget != null)
            {
                if (!Utility.UnderTurret(wTarget, true))
                    W.CastOnUnit(wTarget);
            }

            if (E.IsReady() && useE && eTarget != null && !Player.HasBuff("sound", true) && !W.IsReady())
            {
                E.Cast(eTarget.Position);
                EDelay = Environment.TickCount + 1000;
            }

            if (eTarget != null && !Player.HasBuff("sound", true))
                UseItems(eTarget);
            
            if (qTarget != null && IgniteSlot != SpellSlot.Unknown &&
                Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready &&
                Player.GetSummonerSpellDamage(qTarget, Damage.SummonerSpell.Ignite) > qTarget.Health)
            {
                Player.Spellbook.CastSpell(IgniteSlot, qTarget);
            }
        }

        private static void Harass()
        {
            if (SoundActive()) return;

            var qTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            var eTarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);

            var useQ = Config.Item("UseQHarass").GetValue<bool>();
            var useE = Config.Item("UseEHarass").GetValue<bool>();

            if (qTarget != null && Q.IsReady() && useQ)
            {
                Q.CastOnUnit(qTarget);
            }

            if (eTarget != null && E.IsReady() && useE && !W.IsReady())
            {
                E.Cast(eTarget.Position);
            }
        }

        private static void JungleFarm()
        {
            if (SoundActive()) return;

            var useQ = Config.Item("UseQJungleFarm").GetValue<bool>();
            var useE = Config.Item("UseEJungleFarm").GetValue<bool>();

            var mobs = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, E.Range, MinionTypes.All,
                MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (mobs.Count <= 0) return;

            var mob = mobs[0];
            if (useQ && Q.IsReady() && mobs.Count >= 1)
                Q.CastOnUnit(mob);

            if (useE && E.IsReady() && mobs.Count >= 2 && 
                (LastCastedSpell.LastCastPacketSent.Slot != SpellSlot.E ||
                 Environment.TickCount - LastCastedSpell.LastCastPacketSent.Tick > 150))
            { 
                E.Cast(mob.Position);
            }

            if (Tiamat.IsReady() && Config.Item("JungleFarmUseTiamat").GetValue<bool>())
            {
                if (mobs.Count >= 2)
                    Tiamat.Cast(Player);
            }
        }

        private static void LaneClear()
        {
            if (SoundActive()) return;
            var useQ = Config.Item("UseQLaneClear").GetValue<bool>();
            var useE = Config.Item("UseELaneClear").GetValue<bool>();


            if (useQ && Q.IsReady())
            {
                var minionsQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.NotAlly);
                foreach (Obj_AI_Base vMinion in
                    from vMinion in minionsQ
                    let vMinionEDamage = Player.GetSpellDamage(vMinion, SpellSlot.Q)
                    select vMinion)
                {
                    Q.CastOnUnit(vMinion);
                }
            }
        }

        private static float GetComboDamage(Obj_AI_Base vTarget)
        {
            var fComboDamage = 0d;

            if (Q.IsReady())
                fComboDamage += Player.GetSpellDamage(vTarget, SpellSlot.Q);

            if (E.IsReady())
                fComboDamage += Player.GetSpellDamage(vTarget, SpellSlot.E);

            if (Items.CanUseItem(3128))
                fComboDamage += Player.GetItemDamage(vTarget, Damage.DamageItems.Botrk);

            if (IgniteSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                fComboDamage += Player.GetSummonerSpellDamage(vTarget, Damage.SummonerSpell.Ignite);

            return (float)fComboDamage;
        }

        private static void Interrupter_OnPosibleToInterrupt(Obj_AI_Base vTarget, InterruptableSpell args)
        {
            var interruptSpells = Config.Item("InterruptSpells").GetValue<KeyBind>().Active;
            if (!interruptSpells) return;

            if (Player.Distance(vTarget) < Orbwalking.GetRealAutoAttackRange(ObjectManager.Player))
            {
                if (W.IsReady())
                    W.Cast();
            }
        }

        private static InventorySlot GetInventorySlot(int id)
        {
            return ObjectManager.Player.InventoryItems.FirstOrDefault(
                item => (item.Id == (ItemId) id && item.Stacks >= 1) || (item.Id == (ItemId) id && item.Charges >= 1));
        }

        public static void UseItems(Obj_AI_Hero vTarget)
        {
            if (vTarget == null) return;

            foreach (var itemID in from menuItem in MenuTargetedItems.Items
                                   let useItem = MenuTargetedItems.Item(menuItem.Name).GetValue<bool>()
                                   where useItem
                                   select Convert.ToInt16(menuItem.Name.Substring(4, 4))
                                   into itemId where Items.HasItem(itemId) && Items.CanUseItem(itemId) && GetInventorySlot(itemId) != null select itemId)
            {
                Items.UseItem(itemID, vTarget);
            }

            foreach (var itemID in from menuItem in MenuNonTargetedItems.Items
                                   let useItem = MenuNonTargetedItems.Item(menuItem.Name).GetValue<bool>()
                                   where useItem
                                   select Convert.ToInt16(menuItem.Name.Substring(4, 4))
                                   into itemId where Items.HasItem(itemId) && Items.CanUseItem(itemId) && GetInventorySlot(itemId) != null select itemId)
            {
                Items.UseItem(itemID);
            }
        }
        private static void UseSummoners()
        {
        }
        private static void WelcomeMessage()
        {
            Game.PrintChat(
                String.Format(
                    "<font color='#70DBDB'>xQx</font> <font color='#FFFFFF'>{0}</font> <font color='#70DBDB'>Loaded!</font>",
                    ChampionName));
        }
        
        public static bool SoundActive()
        {
            if (ObjectManager.Player.HasBuff("pantheonesound"))
                UsingE = true;
            return UsingE || ObjectManager.Player.IsChannelingImportantSpell();
        }

    }
}
