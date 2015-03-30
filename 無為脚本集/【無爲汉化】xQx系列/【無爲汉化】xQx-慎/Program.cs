#region
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Xml.Linq;
using System.IO;
using System.Speech.Synthesis;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
#endregion

namespace Shen
{
    internal class Program
    {
        public const string ChampionName = "Shen";
        //Orbwalker instance
        public static Orbwalking.Orbwalker Orbwalker;
        public static SpeechSynthesizer voice = new SpeechSynthesizer();

        //Spells
        public static List<Spell> SpellList = new List<Spell>();
        public static Spell Q, W, E, R;

        private static SpellSlot IgniteSlot = ObjectManager.Player.GetSpellSlot("SummonerDot");
        private static SpellSlot SmiteSlot = ObjectManager.Player.GetSpellSlot("SummonerSmite");
        private static SpellSlot FlashSlot = ObjectManager.Player.GetSpellSlot("SummonerFlash");
        private static SpellSlot TeleportSlot = ObjectManager.Player.GetSpellSlot("SummonerTeleport");

        private static Obj_AI_Hero xUltiableAlly;
        private Timer xTimer = new Timer();
        private double pbUnit;
        private int pbWidth, pbHeight, pbComplete;

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
            if (ObjectManager.Player.BaseSkinName != ChampionName) 
                return;
            
            if (ObjectManager.Player.IsDead) 
                return;

            Q = new Spell(SpellSlot.Q, 520f);
            Q.SetTargetted(0.15f, float.MaxValue);
            SpellList.Add(Q);

            W = new Spell(SpellSlot.W);

            E = new Spell(SpellSlot.E, 500f);
            E.SetSkillshot(0.25f, 150f, float.MaxValue, false, SkillshotType.SkillshotLine);
            SpellList.Add(E);

            R = new Spell(SpellSlot.R);

            //Create the menu
            Config = new Menu("【無爲汉化】xQx-慎", "Shen", true);

            var targetSelectorMenu = new Menu("目标选择器", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            Config.AddSubMenu(new Menu("走砍", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));
            Orbwalker.SetAttack(true);

            // Combo
            Config.AddSubMenu(new Menu("连招", "Combo"));
            {
                /* [ Don't Use Ult ] */
                Config.SubMenu("Combo").AddSubMenu(new Menu("禁止使用大招给", "DontUlt"));
                foreach (var ally in ObjectManager.Get<Obj_AI_Hero>().Where(ally => ally.IsAlly && !ally.IsMe))
                    Config.SubMenu("Combo").SubMenu("DontUlt").AddItem(new MenuItem("DontUlt" + ally.BaseSkinName, ally.BaseSkinName).SetValue(false));

                /* [ Ult Priority ] */
                Config.SubMenu("Combo").AddSubMenu(new Menu("优先使用大招", "UltPriority"));
                foreach (var ally in ObjectManager.Get<Obj_AI_Hero>().Where(ally => ally.IsAlly && !ally.IsMe))
                    Config.SubMenu("Combo").SubMenu("UltPriority").AddItem(new MenuItem("UltPriority" + ally.BaseSkinName, ally.BaseSkinName).SetValue(new Slider(1, 1, 5)));

                Config.SubMenu("Combo").AddItem(new MenuItem("ComboUseQ", "使用 Q").SetValue(true));
                Config.SubMenu("Combo").AddItem(new MenuItem("ComboUseW", "使用 W").SetValue(true));
                Config.SubMenu("Combo").AddItem(new MenuItem("ComboUseE", "使用 E").SetValue(true));
                Config.SubMenu("Combo").AddItem(new MenuItem("ComboUseEF", "使用闪现 + E").SetValue(new KeyBind("T".ToCharArray()[0],KeyBindType.Press)));
                Config.SubMenu("Combo").AddItem(new MenuItem("ComboUseRE", "使用 R").SetValue(true));
                Config.SubMenu("Combo").AddItem(new MenuItem("ComboUseRK", "使用 R 按键:").SetValue(new KeyBind("U".ToCharArray()[0], KeyBindType.Press)));
                Config.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "连招!").SetValue(new KeyBind(Config.Item("Orbwalk").GetValue<KeyBind>().Key, KeyBindType.Press)));
            }

            /* [ Harass ] */
            Config.AddSubMenu(new Menu("骚扰", "Harass"));
            {
                Config.SubMenu("Harass").AddItem(new MenuItem("HarassUseQ", "使用 Q").SetValue(true));
                Config.SubMenu("Harass").AddItem(new MenuItem("HarassUseQT", "使用 Q (自动)").SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Toggle)));
                Config.SubMenu("Harass").AddItem(new MenuItem("HarassEnergy", "最低能量:").SetValue(new Slider(50, 100, 0)));
                Config.SubMenu("Harass").AddItem(new MenuItem("HarassActive", "骚扰!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            }

            /* [  Lane Clear ] */
            Config.AddSubMenu(new Menu("清线", "LaneClear"));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("LaneClearUseQ", "使用 Q").SetValue(false));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("LaneClearEnergy", "最低能量:").SetValue(new Slider(50, 100, 0)));
            Config.SubMenu("LaneClear").AddItem(new MenuItem("LaneClearActive", "清线q").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            /* [  Jungling Farm ] */
            Config.AddSubMenu(new Menu("清野", "JungleFarm"));
            {
                Config.SubMenu("JungleFarm").AddItem(new MenuItem("JungleFarmUseQ", "使用 Q").SetValue(true));
                Config.SubMenu("JungleFarm").AddItem(new MenuItem("JungleFarmUseW", "使用 W").SetValue(false));
                Config.SubMenu("JungleFarm").AddItem(new MenuItem("JungleFarmEnergy", "最低能量:").SetValue(new Slider(50, 100, 0)));
                Config.SubMenu("JungleFarm").AddItem(new MenuItem("JungleFarmActive", "清野").SetValue(new KeyBind("V".ToCharArray()[0],KeyBindType.Press)));
            }

            // Extras
            //Config.AddSubMenu(new Menu("Extras", "Extras"));
            //Config.SubMenu("Extras").AddItem(new MenuItem("InterruptSpells", "Interrupt Spells").SetValue(true));

            // Extras -> Use Items 
            MenuExtras = new Menu("额外", "Extras");
            Config.AddSubMenu(MenuExtras);
            MenuExtras.AddItem(new MenuItem("InterruptSpellsE", "打断: E").SetValue(true));
            MenuExtras.AddItem(new MenuItem("InterruptSpellsEF", "打断: 闪现+E").SetValue(true));

            Menu menuUseItems = new Menu("使用物品", "menuUseItems");
            {
                Config.SubMenu("Extras").AddSubMenu(menuUseItems);
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
                    MenuNonTargetedItems.AddItem(new MenuItem("item3077", "提亚马特").SetValue(true));
                    MenuNonTargetedItems.AddItem(new MenuItem("item3142", "幽梦之灵").SetValue(true));
                }
            }

            /* [ Drawing ] */
            Config.AddSubMenu(new Menu("范围", "Drawings"));
            {
                Config.SubMenu("Drawings").AddItem(new MenuItem("DrawQ", "Q 范围").SetValue(new Circle(true, System.Drawing.Color.Gray)));
                Config.SubMenu("Drawings").AddItem(new MenuItem("DrawE", "E 范围").SetValue(new Circle(false, System.Drawing.Color.Gray)));
                Config.SubMenu("Drawings").AddItem(new MenuItem("DrawEF", "闪现+E 范围").SetValue(new Circle(false, System.Drawing.Color.Gray)));
                Config.SubMenu("Drawings").AddItem(new MenuItem("DrawRswnp", "显示需要大招的人").SetValue(true));
            }

            /* [ Speech ] */
            Config.AddSubMenu(new Menu("语音", "Speech"));
            {
                var xKey = char.ConvertFromUtf32((int)Config.Item("ComboUseRK").GetValue<KeyBind>().Key);
                
                Config.SubMenu("Speech").AddSubMenu(new Menu("语音测试", "SpeechTest"));
                {
                    Config.SubMenu("Speech").SubMenu("SpeechTest").AddItem(new MenuItem("SpeechText", "EZ 需要你帮助时语音 " + xKey + " 极限!"));
                    Config.SubMenu("Speech").SubMenu("SpeechTest").AddItem(new MenuItem("SpeechButton", "现在测试").SetValue(false))
                        .ValueChanged += (sender, e) =>
                        {
                            if (e.GetNewValue<bool>())
                            {
                                Speech();
                                Config.Item("SpeechButton").SetValue(false);
                            }
                        };
                }

                Config.SubMenu("Speech").AddItem(new MenuItem("SpeechVolume", "体积").SetValue(new Slider(50, 10, 100)));//.ValueChanged += (sender, eventArgs) => { Game.PrintChat("xQx 鎱庝辅鍔犺浇鎴愬姛锛佹眽鍖朾y Bbyyyyy锛丵Q缇や辅361630847"); };
                Config.SubMenu("Speech").AddItem(new MenuItem("SpeechRate", "率").SetValue(new Slider(3, -10, 10)));
                Config.SubMenu("Speech").AddItem(new MenuItem("SpeechGender", "性别").SetValue(new StringList(new[] {"男声", "女声"}, 1)));
                Config.SubMenu("Speech").AddItem(new MenuItem("SpeechRepeatTime", "重复").SetValue(new StringList(new[] {"重复1次 ", "重复2次", "重复3次", "重复每次"}, 1)));
                Config.SubMenu("Speech").AddItem(new MenuItem("SpeechRepeatDelay", "重复延迟时间.").SetValue(new Slider(3, 1, 5)));
                Config.SubMenu("Speech").AddItem(new MenuItem("SpeechActive", "启用").SetValue(true));
            }

            new PotionManager();
            Config.AddToMainMenu();

            Game.OnUpdate += Game_OnGameUpdate;
            
            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPosibleToInterrupt;

            Game.PrintChat(String.Format("<font color='#70DBDB'>xQx | </font> <font color='#FFFFFF'>{0}</font> <font color='#70DBDB'> Loaded!</font>", ChampionName));

            Speech();
        }

        public static void Speech()
        {
            if (!Config.Item("SpeechActive").GetValue<bool>())
                return;
            var xSpeechText = Config.Item("SpeechText").DisplayName.ToString();
            var xSpeechVolume = Config.Item("SpeechVolume").GetValue<Slider>().Value;
            var xSpeechRate = Config.Item("SpeechRate").GetValue<Slider>().Value;
            var xSpeechGender = Config.Item("SpeechGender").GetValue<StringList>().SelectedIndex;
            var xSpeechRepeatTime = Config.Item("SpeechRepeatTime").GetValue<StringList>().SelectedIndex;
            var xSpeechRepeatDelay = Config.Item("SpeechRepeatDelay").GetValue<Slider>().Value;

            try
            {
                switch (xSpeechGender)
                {
                    case 0:
                        voice.SelectVoiceByHints(VoiceGender.Male);
                        break;
                    case 1:
                        voice.SelectVoiceByHints(VoiceGender.Female);
                        break;
                }
                voice.Volume = xSpeechVolume;
                voice.Rate = xSpeechRate;
                voice.SpeakAsync(xSpeechText);
            }
            catch (Exception e)
            {
                Game.PrintChat(e.Message);
            }
            
        }

        public static bool InShopRange(Obj_AI_Hero xAlly)
        {
            return (
                from shop in ObjectManager.Get<Obj_Shop>()
                where shop.IsAlly
                select shop).Any<Obj_Shop>(shop => Vector2.Distance(xAlly.Position.To2D(), shop.Position.To2D()) < 1250f);
        }

        public static int CountAlliesInRange(int range, Vector3 point)
        {
            return (
                from units in ObjectManager.Get<Obj_AI_Hero>()
                where units.IsAlly && units.IsVisible && !units.IsDead
                select units).Count<Obj_AI_Hero>(
                    units => Vector2.Distance(point.To2D(), units.Position.To2D()) <= (float) range);
        }

        public static int CountEnemysInRange(int range, Vector3 point)
        {
            return (
                from units in ObjectManager.Get<Obj_AI_Hero>()
                where units.IsValidTarget()
                select units).Count<Obj_AI_Hero>(
                    units => Vector2.Distance(point.To2D(), units.Position.To2D()) <= (float) range);
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            var drawQ = Config.Item("DrawQ").GetValue<Circle>();
            if (drawQ.Active && Q.Level > 0)
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, drawQ.Color);

            var drawE = Config.Item("DrawE").GetValue<Circle>();
            if (drawE.Active && Q.Level > 0)
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, drawE.Color);
        }

        static void DrawHelplessAllies()
        {
            var drawRswnp = Config.Item("DrawRswnp").GetValue<bool>();
            if (drawRswnp && (R.IsReady() || ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).CooldownExpires < 2))
                
            {
                var xHeros = ObjectManager.Get<Obj_AI_Hero>().Where(xQ => !xQ.IsMe && xQ.IsVisible && !xQ.IsDead);
                var xAlly = xHeros.Where(xQ => xQ.IsAlly && !xQ.IsMe && !Config.Item("DontUlt" + xQ.BaseSkinName).GetValue<bool>()).OrderBy(xQ => xQ.Health).FirstOrDefault();
                var xEnemy = xHeros.Where(xQ => xQ.IsEnemy);

                
                if (xAlly.Health < xAlly.Level * 30 && !InShopRange(xAlly))
                {
                    foreach (var x1 in xEnemy.Where(x1 => x1.Distance(xAlly) < 600))
                    {
                        Game.PrintChat(xAlly.ChampionName + " -> " + x1.ChampionName);
                         xUltiableAlly = xAlly;
                        var xKey = char.ConvertFromUtf32((int)Config.Item("ComboUseRK").GetValue<KeyBind>().Key);

                    //Drawing.DrawText(Drawing.Width * 0.44f, Drawing.Height * 0.80f, System.Drawing.Color.Red, "Q is not ready! You can not Jump!");
                   // Game.PrintChat( xAlly.BaseSkinName + ":-> " + xPriority + " Needs Your Help! Press " + xKey + " for Ultimate!");

                    Drawing.DrawText(Drawing.Width * 0.40f, Drawing.Height * 0.80f, System.Drawing.Color.White, xAlly.BaseSkinName + " Needs Your Help! Press " + xKey + " for Ultimate!");
                    }
                    /*
                    var xPriority = Config.Item("UltPriority" + xAlly.BaseSkinName).GetValue<Slider>().Value;
                    xUltiableAlly = xAlly;
                    var xKey = char.ConvertFromUtf32((int)Config.Item("ComboUseRK").GetValue<KeyBind>().Key);
                    Drawing.DrawText(Drawing.Width * 0.40f, Drawing.Height * 0.80f, System.Drawing.Color.White, xAlly.BaseSkinName + " Needs Your Help! Press " + xKey + " for Ultimate!");
                    */
                    
                }
            }            
        }
        private static void Game_OnGameUpdate(EventArgs args)
        {
            DrawHelplessAllies();
            if (!Orbwalking.CanMove(100)) return;

            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
            }

            if (Config.Item("ComboUseRK").GetValue<KeyBind>().Active)
            {
                ComboUseRWithKey();
            }

            if (Config.Item("ComboUseEF").GetValue<KeyBind>().Active)
            {
                ComboFlashE();
            }

            if (Config.Item("HarassActive").GetValue<KeyBind>().Active)
            {
                var existsMana = ObjectManager.Player.MaxMana / 100 * Config.Item("HarassEnergy").GetValue<Slider>().Value;
                if (ObjectManager.Player.Mana >= existsMana)
                    Harass();
            }

            if (Config.Item("LaneClearActive").GetValue<KeyBind>().Active)
            {
                var existsMana = ObjectManager.Player.MaxMana / 100 * Config.Item("LaneClearEnergy").GetValue<Slider>().Value;
                if (ObjectManager.Player.Mana >= existsMana)
                    LaneClear();
            }

            if (Config.Item("JungleFarmActive").GetValue<KeyBind>().Active)
            {
                var existsMana = ObjectManager.Player.MaxMana / 100 * Config.Item("JungleFarmEnergy").GetValue<Slider>().Value;
                if (ObjectManager.Player.Mana >= existsMana)
                    JungleFarm();
            }
        }

        private static void Combo()
        {
            var useQ = Config.Item("ComboUseQ").GetValue<bool>();
            var useW = Config.Item("ComboUseW").GetValue<bool>();
            var useE = Config.Item("ComboUseE").GetValue<bool>();

            if (Q.IsReady() && useQ)
            {
                var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
                if (t.IsValidTarget())
                    Q.CastOnUnit(t);
            }

            if (W.IsReady() && useW)
            {
                var t = TargetSelector.GetTarget(Q.Range / 2, TargetSelector.DamageType.Magical);
                if (t.IsValidTarget())
                    W.CastOnUnit(ObjectManager.Player);
            }

            if (E.IsReady() && useE)
            {
                var t = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
                if (t.IsValidTarget())
                {
                    E.Cast(t.Position);
                    UseItems(t);
                }
            }
        }

        private static void ComboUseRWithKey()
        {
            if (R.IsReady())
                R.CastOnUnit(xUltiableAlly);
            //Packet.C2S.Cast.Encoded(new Packet.C2S.Cast.Struct(xUltiableAlly.NetworkId, SpellSlot.R)).Send();

        }

        private static void ComboFlashE()
        {
            ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
			
            var fqTarget = TargetSelector.GetTarget(Q.Range + 430, TargetSelector.DamageType.Physical);

            if (ObjectManager.Player.Distance(fqTarget) > E.Range && E.IsReady() && fqTarget != null &&
                FlashSlot != SpellSlot.Unknown &&
                ObjectManager.Player.Spellbook.CanUseSpell(FlashSlot) == SpellState.Ready)
            {
                ObjectManager.Player.Spellbook.CastSpell(FlashSlot, fqTarget.ServerPosition);
                Utility.DelayAction.Add(100, () => E.Cast(fqTarget.Position));
            }
        }
        private static void Harass()
        {
            var useQ = Config.Item("HarassUseQ").GetValue<bool>();

            if (Q.IsReady() && useQ)
            {
                var t = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
                if (t.IsValidTarget())
                    Q.CastOnUnit(t);
            }
        }

        private static void JungleFarm()
        {
            var useQ = Config.Item("JungleFarmUseQ").GetValue<bool>();
            var useW = Config.Item("JungleFarmUseW").GetValue<bool>();

            var mobs = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All,
                MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (mobs.Count <= 0) return;

            var mob = mobs[0];
            if (useQ && Q.IsReady() && mob.Health < ObjectManager.Player.GetSpellDamage(mob, SpellSlot.Q) + 30)
            {
                Q.CastOnUnit(mob);
            }

            if (useW && W.IsReady())
            {
                W.CastOnUnit(ObjectManager.Player);
            }
        }

        private static void LaneClear()
        {
            var useQ = Config.Item("LaneClearUseQ").GetValue<bool>();
            var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);

            if (useQ && Q.IsReady() && allMinionsQ.Count > 0)
            {
                if (allMinionsQ[0].Health < ObjectManager.Player.GetSpellDamage(allMinionsQ[0], SpellSlot.Q) + 20)
                    Q.CastOnUnit(allMinionsQ[0]);
            }
        }

        private static void Interrupter_OnPosibleToInterrupt(Obj_AI_Base t, InterruptableSpell args)
        {
            var interruptSpells = Config.Item("InterruptSpells").GetValue<KeyBind>().Active;
            if (!interruptSpells) return;

            if (ObjectManager.Player.Distance(t) < Q.Range)
            {
                E.Cast(t.Position);
            }
        }

        private static InventorySlot GetInventorySlot(int id)
        {
            return ObjectManager.Player.InventoryItems.FirstOrDefault(
                item => (item.Id == (ItemId)id && item.Stacks >= 1) || (item.Id == (ItemId)id && item.Charges >= 1));
        }

        public static void UseItems(Obj_AI_Hero vTarget)
        {
            if (vTarget == null) return;

            foreach (var itemID in from menuItem in MenuTargetedItems.Items
                                   let useItem =
                                        MenuTargetedItems.Item(menuItem.Name).GetValue<bool>()
                                   where useItem
                                   select Convert.ToInt16(menuItem.Name.Substring(4, 4))
                                       into itemId
                                       where Items.HasItem(itemId) &&
                                             Items.CanUseItem(itemId) && GetInventorySlot(itemId) != null
                                       select itemId)
            {
                Items.UseItem(itemID, vTarget);
            }

            foreach (var itemID in from menuItem in MenuNonTargetedItems.Items
                                   let useItem =
                                        MenuNonTargetedItems.Item(menuItem.Name).GetValue<bool>()
                                   where useItem
                                   select Convert.ToInt16(menuItem.Name.Substring(4, 4))
                                       into itemId
                                       where Items.HasItem(itemId) &&
                                             Items.CanUseItem(itemId) && GetInventorySlot(itemId) != null
                                       select itemId)
            {
                Items.UseItem(itemID);
            }
        }
    }
}
