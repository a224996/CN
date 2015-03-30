#region
using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
#endregion

namespace Wukong
{
    internal class Program
    {
        public const string ChampionName = "MonkeyKing";
        private static readonly Obj_AI_Hero Player = ObjectManager.Player;

        //Orbwalker instance
        public static Orbwalking.Orbwalker Orbwalker;

        //Spells
        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q;
        public static Spell E;
        public static Spell R;

        private static readonly Items.Item Tiamat = new Items.Item(3077, 450);
        private static readonly SpellSlot IgniteSlot = Player.GetSpellSlot("SummonerDot");

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
            if (Player.BaseSkinName != "MonkeyKing")
                return;
            if (Player.IsDead)
                return;

            Q = new Spell(SpellSlot.Q, 375f);
            E = new Spell(SpellSlot.E, 640f);
            R = new Spell(SpellSlot.R, 375f);

            E.SetTargetted(0.5f, 2000f);

            SpellList.Add(Q);
            SpellList.Add(E);
            SpellList.Add(R);

            //Create the menu
            Config = new Menu("���o��������xQx-����", "MonkeyKing", true);

            var targetSelectorMenu = new Menu("Ŀ��ѡ��", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            Config.AddSubMenu(new Menu("�߿�", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));
            Orbwalker.SetAttack(true);

            var menuCombo = new Menu("����", "Combo");
            // Combo
            Config.AddSubMenu(menuCombo);
            menuCombo.AddItem(new MenuItem("UseQCombo", "ʹ�� Q").SetValue(true));
      
            menuCombo.AddItem(new MenuItem("UseECombo", "ʹ�� E").SetValue(true));
            menuCombo.AddItem(new MenuItem("UseEComboTurret", "��ֹ���� E").SetValue(true));
            menuCombo.AddItem(new MenuItem("UseRCombo", "ʹ�� R").SetValue(true));
            menuCombo.AddItem(new MenuItem("UseRComboEnemyCount", "��������:").SetValue(new Slider(1, 5, 0)));
            menuCombo.AddItem(
                new MenuItem("ComboActive", "����!").SetValue(
                    new KeyBind(Config.Item("Orbwalk").GetValue<KeyBind>().Key, KeyBindType.Press)));

            // Harass
            Config.AddSubMenu(new Menu("ɧ��", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "ʹ�� Q").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "ʹ�� E").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseEHarassTurret", "��ֹ���� E").SetValue(true));
            Config.SubMenu("Harass")
                .AddItem(new MenuItem("HarassMana", "�������: ").SetValue(new Slider(50, 100, 0)));
            Config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("HarassActive", "ɧ��").SetValue(
                        new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));

            // Lane Clear
            Config.AddSubMenu(new Menu("����", "LaneClear"));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("UseQLaneClear", "ʹ�� Q").SetValue(false));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("UseELaneClear", "ʹ�� E").SetValue(false));
            Config.SubMenu("LaneClear")
                .AddItem(new MenuItem("LaneClearMana", "�����������: ").SetValue(new Slider(50, 100, 0)));

            var menuLaneClearItems = new Menu("ʹ����Ʒ", "menuLaneClearItems");
            Config.SubMenu("LaneClear").AddSubMenu(menuLaneClearItems);
            menuLaneClearItems.AddItem(new MenuItem("LaneClearUseTiamat", "��������").SetValue(true));

            Config.SubMenu("LaneClear")
                .AddItem(
                    new MenuItem("LaneClearActive", "���").SetValue(
                        new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            // Jungling Farm
            Config.AddSubMenu(new Menu("��Ұ", "JungleFarm"));
            Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseQJungleFarm", "ʹ�� Q").SetValue(true));
            Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseEJungleFarm", "ʹ�� E").SetValue(false));
            Config.SubMenu("JungleFarm")
                .AddItem(new MenuItem("JungleFarmMana", "�������: ").SetValue(new Slider(50, 100, 0)));

            var menuJungleFarmItems = new Menu("ʹ����Ʒ", "menuJungleFarmItems");
            Config.SubMenu("JungleFarm").AddSubMenu(menuJungleFarmItems);
            menuLaneClearItems.AddItem(new MenuItem("JungleFarmUseTiamat", "��������").SetValue(true));

            Config.SubMenu("JungleFarm")
                .AddItem(
                    new MenuItem("JungleFarmActive", "��Ұ").SetValue(
                        new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            // Extras -> Use Items 
            MenuExtras = new Menu("����", "Extras");
            Config.AddSubMenu(MenuExtras);
            MenuExtras.AddItem(new MenuItem("InterruptSpells", "�жϷ���").SetValue(true));
            MenuExtras.AddItem(new MenuItem("AutoLevelUp", "�Զ��ӵ�").SetValue(true));

            var menuUseItems = new Menu("ʹ����Ʒ", "menuUseItems");
            Config.SubMenu("Extras").AddSubMenu(menuUseItems);
            // Extras -> Use Items -> Targeted Items
            MenuTargetedItems = new Menu("������Ʒ", "menuTargetItems");
            menuUseItems.AddSubMenu(MenuTargetedItems);

            MenuTargetedItems.AddItem(new MenuItem("item3153", "�ư�").SetValue(true));
            MenuTargetedItems.AddItem(new MenuItem("item3143", "����").SetValue(true));
            MenuTargetedItems.AddItem(new MenuItem("item3144", "�ȶ��������䵶").SetValue(true));

            MenuTargetedItems.AddItem(new MenuItem("item3146", "�Ƽ�ǹ").SetValue(true));
            MenuTargetedItems.AddItem(new MenuItem("item3184", "���� ").SetValue(true));

            // Extras -> Use Items -> AOE Items
            MenuNonTargetedItems = new Menu("AOE��Ʒ", "menuNonTargetedItems");
            menuUseItems.AddSubMenu(MenuNonTargetedItems);
            MenuNonTargetedItems.AddItem(new MenuItem("item3180", "�¶���ɴ").SetValue(true));
            MenuNonTargetedItems.AddItem(new MenuItem("item3131", "��ʥ֮��").SetValue(true));
            MenuNonTargetedItems.AddItem(new MenuItem("item3074", "̰����ͷ��").SetValue(true));
            MenuNonTargetedItems.AddItem(new MenuItem("item3077", "��������").SetValue(true));
            MenuNonTargetedItems.AddItem(new MenuItem("item3142", "����֮��").SetValue(true));

            // Drawing
            Config.AddSubMenu(new Menu("��Χ", "Drawings"));
            Config.SubMenu("Drawings")
                .AddItem(
                    new MenuItem("QRange", "Q ��Χ").SetValue(
                        new Circle(false, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings")
                .AddItem(
                    new MenuItem("ERange", "E ��Χ").SetValue(
                        new Circle(false, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings")
                .AddItem(
                    new MenuItem("RRange", "R ��Χ").SetValue(
                        new Circle(false, System.Drawing.Color.FromArgb(255, 255, 255, 255))));

            // new PotionManager();
            Config.AddToMainMenu();

            Utility.HpBarDamageIndicator.DamageToUnit = GetComboDamage;
            Utility.HpBarDamageIndicator.Enabled = true;

            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPosibleToInterrupt;

            Game.PrintChat(
                String.Format(
                    "<font color='#70DBDB'>xQx | </font> <font color='#FFFFFF'>" +
                    "{0}</font> <font color='#70DBDB'> Loaded!</font>", ChampionName));
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            foreach (var spell in SpellList)
            {
                var menuItem = Config.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active)
                    Render.Circle.DrawCircle(Player.Position, spell.Range, menuItem.Color, 1);
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (!Orbwalking.CanMove(100))
                return;

            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
            }

            if (Config.Item("HarassActive").GetValue<KeyBind>().Active)
            {
                var vMana = Config.Item("HarassMana").GetValue<Slider>().Value;
                if (Player.ManaPercentage() >= vMana)
                    Harass();
            }

            if (Config.Item("LaneClearActive").GetValue<KeyBind>().Active)
            {
                var vMana = Config.Item("LaneClearMana").GetValue<Slider>().Value;
                if (Player.ManaPercentage() >= vMana)
                    LaneClear();
            }

            if (Config.Item("JungleFarmActive").GetValue<KeyBind>().Active)
            {
                var vMana = Config.Item("JungleFarmMana").GetValue<Slider>().Value;
                if (Player.ManaPercentage() >= vMana)
                    JungleFarm();
            }
        }

        private static void Combo()
        {
            Obj_AI_Hero t;

            var useQ = Config.Item("UseQCombo").GetValue<bool>() && Q.IsReady();
            var useE = Config.Item("UseECombo").GetValue<bool>() && E.IsReady();
            var useR = Config.Item("UseRCombo").GetValue<bool>() && R.IsReady();

            if (useQ)
            {
                t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
                if (t.IsValidTarget())
                    Q.Cast();
            }

            if (useE)
            {
                t = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
                if (t.IsValidTarget())
                {
                    if (Config.Item("UseEComboTurret").GetValue<bool>())
                    {
                        if (!t.UnderTurret())
                            E.CastOnUnit(t);
                    }
                    else
                        E.CastOnUnit(t);
                }
            }

            if (IgniteSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
            {
                t = TargetSelector.GetTarget(500, TargetSelector.DamageType.Magical);
                if (t.IsValidTarget() && Player.GetSummonerSpellDamage(t, Damage.SummonerSpell.Ignite) > t.Health)
                {
                    Player.Spellbook.CastSpell(IgniteSlot, t);
                }
            }

            if (useR)
            {
                if (Player.CountEnemiesInRange(R.Range) >= Config.Item("UseRComboEnemyCount").GetValue<Slider>().Value)
                {
                    R.Cast();
                }
            }

            t = TargetSelector.GetTarget(500, TargetSelector.DamageType.Magical);
            if (t.IsValidTarget())
            {
                UseItems(t);
            }
        }

        private static void Harass()
        {
            var eTarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);

            var useQ = Config.Item("UseQHarass").GetValue<bool>() && Q.IsReady();
            var useE = Config.Item("UseEHarass").GetValue<bool>() && E.IsReady();

            if (useQ)
            {
                var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
                if (t.IsValidTarget())
                    Q.Cast();

            }

            if (useE)
            {
                var t = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
                if (t.IsValidTarget())
                {
                    if (Config.Item("UseEHarassTurret").GetValue<bool>())
                    {
                        if (!t.UnderTurret())
                            E.CastOnUnit(eTarget);
                    }
                    else
                        E.CastOnUnit(eTarget);
                }
            }
        }

        private static void JungleFarm()
        {
            var useQ = Config.Item("UseQJungleFarm").GetValue<bool>();
            var useE = Config.Item("UseEJungleFarm").GetValue<bool>();

            var mobs = MinionManager.GetMinions(
                Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (mobs.Count <= 0)
                return;

            var mob = mobs[0];
            if (useQ && Q.IsReady() && mobs.Count >= 1)
                Q.Cast();

            if (useE && E.IsReady() && mobs.Count >= 2)
                E.CastOnUnit(mob);

            if (Tiamat.IsReady() && Config.Item("JungleFarmUseTiamat").GetValue<bool>())
            {
                if (mobs.Count >= 2)
                    Tiamat.Cast(Player);
            }
        }

        private static void LaneClear()
        {
            var useQ = Config.Item("UseQLaneClear").GetValue<bool>() && Q.IsReady();
            var useE = Config.Item("UseELaneClear").GetValue<bool>() && E.IsReady();

            if (useQ)
            {
                var minionsQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range);

                foreach (var vMinion in from vMinion in minionsQ
                    let vMinionEDamage = Player.GetSpellDamage(vMinion, SpellSlot.Q)
                    where vMinion.Health <= vMinionEDamage && vMinion.Health > Player.GetAutoAttackDamage(vMinion)
                    select vMinion)
                {
                    Q.Cast();
                }
            }

            if (useE)
            {
                var allMinionsE = MinionManager.GetMinions(Player.ServerPosition, E.Range);
                var locE = E.GetCircularFarmLocation(allMinionsE);
                if (allMinionsE.Count == allMinionsE.Count(m => Player.Distance(m) < E.Range) && locE.MinionsHit >= 2 &&
                    locE.Position.IsValid())
                    E.Cast(locE.Position);
            }

            if (Tiamat.IsReady() && Config.Item("LaneClearUseTiamat").GetValue<bool>())
            {
                var allMinions = MinionManager.GetMinions(
                    Player.ServerPosition, Orbwalking.GetRealAutoAttackRange(Player));
                var locTiamat = E.GetCircularFarmLocation(allMinions);
                if (locTiamat.MinionsHit >= 3)
                    Tiamat.Cast(Player);
            }
        }

        private static float GetComboDamage(Obj_AI_Base vTarget)
        {
            var fComboDamage = 0d;

            if (Q.IsReady())
                fComboDamage += Player.GetSpellDamage(vTarget, SpellSlot.Q);

            if (E.IsReady())
                fComboDamage += Player.GetSpellDamage(vTarget, SpellSlot.E);

            if (R.IsReady())
                fComboDamage += Player.GetSpellDamage(vTarget, SpellSlot.R);

            if (Items.CanUseItem(3128))
                fComboDamage += Player.GetItemDamage(vTarget, Damage.DamageItems.Botrk);

            if (IgniteSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                fComboDamage += Player.GetSummonerSpellDamage(vTarget, Damage.SummonerSpell.Ignite);

            return (float) fComboDamage;
        }

        private static void Interrupter_OnPosibleToInterrupt(Obj_AI_Base vTarget, InterruptableSpell args)
        {
            var interruptSpells = Config.Item("InterruptSpells").GetValue<KeyBind>().Active;
            if (!interruptSpells)
                return;

            if (Player.Distance(vTarget) < Orbwalking.GetRealAutoAttackRange(Player))
            {
                R.Cast();
            }
        }

        private static InventorySlot GetInventorySlot(int id)
        {
            return
                Player.InventoryItems.FirstOrDefault(
                    item =>
                        (item.Id == (ItemId) id && item.Stacks >= 1) || (item.Id == (ItemId) id && item.Charges >= 1));
        }

        public static void UseItems(Obj_AI_Hero vTarget)
        {
            if (vTarget == null)
                return;

            foreach (var itemId in from menuItem in MenuTargetedItems.Items
                let useItem = MenuTargetedItems.Item(menuItem.Name).GetValue<bool>()
                where useItem
                select Convert.ToInt16(menuItem.Name.Substring(4, 4))
                into itemId
                where Items.HasItem(itemId) && Items.CanUseItem(itemId) && GetInventorySlot(itemId) != null
                select itemId)
            {
                Items.UseItem(itemId, vTarget);
            }

            foreach (var itemId in from menuItem in MenuNonTargetedItems.Items
                let useItem = MenuNonTargetedItems.Item(menuItem.Name).GetValue<bool>()
                where useItem
                select Convert.ToInt16(menuItem.Name.Substring(4, 4))
                into itemId
                where Items.HasItem(itemId) && Items.CanUseItem(itemId) && GetInventorySlot(itemId) != null
                select itemId)
            {
                Items.UseItem(itemId);
            }
        }
    }
}
