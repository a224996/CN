using System.Collections.Generic;
using System.Linq;
using System;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using System.Media;

namespace GagongSyndra
{
    class Program
    {
        private const string ChampName = "Syndra";
        private static Obj_AI_Hero Player = ObjectManager.Player;
        private static SoundPlayer welcome = new SoundPlayer(Properties.Resources.Welcome);
        private static SoundPlayer ballstotheface = new SoundPlayer(Properties.Resources.BallsToTheFace);
        private static SoundPlayer imkillingthebitch = new SoundPlayer(Properties.Resources.ImKillingTheBitch);
        private static SoundPlayer ohdontyoudare = new SoundPlayer(Properties.Resources.OhDontYouDare);
        private static SoundPlayer ohidiot = new SoundPlayer(Properties.Resources.OhIdiot);
        private static SoundPlayer whosthebitchnow = new SoundPlayer(Properties.Resources.WhosTheBitchNow);
        private static SoundPlayer yourdeadmeatasshole = new SoundPlayer(Properties.Resources.YourDeadMeatAsshole);
        private static SoundPlayer diefucker = new SoundPlayer(Properties.Resources.DieFucker);
        private static SoundPlayer goingsomewhereasshole = new SoundPlayer(Properties.Resources.GoingSomewhereAsshole);
        private static SoundPlayer ilovethisgame = new SoundPlayer(Properties.Resources.ILoveThisGame);
        private static int LastPlayedSound = 0;

        //Collision
        private static int _wallCastT;
        private static Vector2 _yasuoWallCastedPos;
        private static GameObject _yasuoWall;

        //Create spells
        private static List<Spell> SpellList = new List<Spell>();
        private static Spell Q;
        private static Spell W;
        private static Spell E;
        private static Spell R;
        private static Spell QE;
        private static int QWLastcast = 0;

        //Summoner spells
        public static SpellSlot IgniteSlot;
        public static SpellSlot FlashSlot;
        private static int FlashLastCast;

        //Key binds
        public static MenuItem comboKey;
        public static MenuItem harassKey;
        public static MenuItem laneclearKey;
        public static MenuItem lanefreezeKey;
        
        //Items
        public static Items.Item DFG;

        //Orbwalker instance
        private static Orbwalking.Orbwalker _orbwalker;

        private static Menu Menu;
        private static Menu orbwalkerMenu;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.BaseSkinName != ChampName) return;
            
            //Spells data
            Q = new Spell(SpellSlot.Q, 800);
            Q.SetSkillshot(0.65f, 120f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            W = new Spell(SpellSlot.W, 925);
            W.SetSkillshot(0.75f, 120f, 1500f, false, SkillshotType.SkillshotCircle);

            E = new Spell(SpellSlot.E, 700);
            E.SetSkillshot(0.25f, (float)(45 * 0.5), 2500, false, SkillshotType.SkillshotCone);         

            R = new Spell(SpellSlot.R, 675);
            R.SetTargetted(0.5f, 1100f);

            QE = new Spell(SpellSlot.E, 1292);
            QE.SetSkillshot(0.98f, 55f, 9000f, false, SkillshotType.SkillshotLine);


            IgniteSlot = Player.GetSpellSlot("SummonerDot");
            FlashSlot = Player.GetSpellSlot("summonerflash");

            DFG = Utility.Map.GetMap().Type == Utility.Map.MapType.TwistedTreeline ? new Items.Item(3188, 750) : new Items.Item(3128, 750);
            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
            
            //Base menu
            Menu = new Menu("【超神汉化】辛德拉", "GagongSyndra", true);
            orbwalkerMenu = new Menu("走砍", "Orbwalker");
            //TargetSelector
            Menu.AddSubMenu(new Menu("目标选择器", "TargetSelector"));
            TargetSelector.AddToMenu(Menu.SubMenu("TargetSelector"));

            //Orbwalker
            orbwalkerMenu.AddItem(new MenuItem("Orbwalker_Mode", "规律走砍").SetValue(false));
            Menu.AddSubMenu(orbwalkerMenu);
            ChooseOrbwalker(Menu.Item("Orbwalker_Mode").GetValue<bool>()); //uncomment this line

            //Combo
            Menu.AddSubMenu(new Menu("连招", "Combo"));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseQ", "使用Q").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseW", "使用W").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseE", "使用E").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseR", "使用R").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseQE", "使用QE").SetValue(true));
            
            //Harass
            Menu.AddSubMenu(new Menu("骚扰", "Harass"));
            Menu.SubMenu("Harass").AddItem(new MenuItem("UseQH", "使用 Q").SetValue(true));
            Menu.SubMenu("Harass").AddItem(new MenuItem("HarassAAQ", "敌人平A使用Q").SetValue(false));
            Menu.SubMenu("Harass").AddItem(new MenuItem("HarassTurret", "敌人炮塔下禁用骚扰").SetValue(false));
            Menu.SubMenu("Harass").AddItem(new MenuItem("UseWH", "使用 W").SetValue(false));
            Menu.SubMenu("Harass").AddItem(new MenuItem("UseEH", "使用 E").SetValue(false));
            Menu.SubMenu("Harass").AddItem(new MenuItem("UseQEH", "使用 QE").SetValue(false));
            Menu.SubMenu("Harass").AddItem(new MenuItem("HarassMana", "蓝量控制").SetValue(new Slider(0)));
            Menu.SubMenu("Harass").AddItem(new MenuItem("HarassActiveT", "骚扰 (锁定)").SetValue(new KeyBind("Y".ToCharArray()[0], KeyBindType.Toggle,true)));

            //Farming menu:
            Menu.AddSubMenu(new Menu("清兵", "Farm"));
            Menu.SubMenu("Farm").AddItem(new MenuItem("UseQFarm", "使用 Q").SetValue(new StringList(new[] { "Freeze", "LaneClear", "Both", "No" }, 2)));
            Menu.SubMenu("Farm").AddItem(new MenuItem("UseWFarm", "使用 W").SetValue(new StringList(new[] { "Freeze", "LaneClear", "Both", "No" }, 1)));
            Menu.SubMenu("Farm").AddItem(new MenuItem("UseEFarm", "使用 E").SetValue(new StringList(new[] { "Freeze", "LaneClear", "Both", "No" }, 3)));

            //JungleFarm menu:
            Menu.AddSubMenu(new Menu("清野", "JungleFarm"));
            Menu.SubMenu("JungleFarm").AddItem(new MenuItem("UseQJFarm", "使用 Q").SetValue(true));
            Menu.SubMenu("JungleFarm").AddItem(new MenuItem("UseWJFarm", "使用 W").SetValue(true));
            Menu.SubMenu("JungleFarm").AddItem(new MenuItem("UseEJFarm", "使用 E").SetValue(true));

            //Auto KS
            Menu.AddSubMenu(new Menu("抢头", "AutoKS"));
            Menu.SubMenu("AutoKS").AddItem(new MenuItem("UseQKS", "使用Q").SetValue(true));
            Menu.SubMenu("AutoKS").AddItem(new MenuItem("UseWKS", "使用W").SetValue(true));
            Menu.SubMenu("AutoKS").AddItem(new MenuItem("UseEKS", "使用E").SetValue(true));
            Menu.SubMenu("AutoKS").AddItem(new MenuItem("UseQEKS", "使用QE").SetValue(true));
            Menu.SubMenu("AutoKS").AddItem(new MenuItem("UseRKS", "使用R").SetValue(true));
            Menu.SubMenu("AutoKS").AddItem(new MenuItem("AutoKST", "抢头 (锁定)!").SetValue(new KeyBind("U".ToCharArray()[0], KeyBindType.Toggle,true)));
            
            //Auto Flash Kill
            Menu.SubMenu("AutoKS").AddItem(new MenuItem("UseFK1", "闪现Q+E").SetValue(true));
            Menu.SubMenu("AutoKS").AddItem(new MenuItem("UseFK2", "闪现冥火R").SetValue(true));
            Menu.SubMenu("AutoKS").AddSubMenu(new Menu("使用闪现", "FKT"));
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team))
                Menu.SubMenu("AutoKS").SubMenu("FKT").AddItem(new MenuItem("FKT" + enemy.BaseSkinName, enemy.BaseSkinName).SetValue(true));
            Menu.SubMenu("AutoKS").AddItem(new MenuItem("MaxE", "敌人数量").SetValue(new Slider(2, 1, 5)));
            Menu.SubMenu("AutoKS").AddItem(new MenuItem("FKMANA", "确保蓝量").SetValue(false));
            
            //Misc
            Menu.AddSubMenu(new Menu("杂项", "Misc"));
            Menu.SubMenu("Misc").AddItem(new MenuItem("AntiGap", "防突进").SetValue(true));
            Menu.SubMenu("Misc").AddItem(new MenuItem("Interrupt", "打断技能").SetValue(true));
            Menu.SubMenu("Misc").AddItem(new MenuItem("Packets", "封包").SetValue(false));
            Menu.SubMenu("Misc").AddItem(new MenuItem("IgniteALLCD", "所有技能CD使用点燃").SetValue(false));
            if (Menu.Item("Orbwalker_Mode").GetValue<bool>()) Menu.SubMenu("Misc").AddItem(new MenuItem("OrbWAA", "AA while orbwalking").SetValue(true));
            Menu.SubMenu("Misc").AddItem(new MenuItem("Sound1", "启动声音").SetValue(true));
            Menu.SubMenu("Misc").AddItem(new MenuItem("Sound2", "在游戏中的声音").SetValue(true));
            Menu.SubMenu("Misc").AddItem(new MenuItem("YasuoWall", "亚索风墙不使用技能").SetValue(true));
            //QE Settings
            Menu.AddSubMenu(new Menu("QE设置", "QEsettings"));
            Menu.SubMenu("QEsettings").AddItem(new MenuItem("QEDelay", "QE延迟").SetValue(new Slider(0, 0, 150)));
            Menu.SubMenu("QEsettings").AddItem(new MenuItem("QEMR", "QE 最小范围").SetValue(new Slider(100)));
            Menu.SubMenu("QEsettings").AddItem(new MenuItem("UseQEC", "QE鼠标位置").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));

            //R
            Menu.AddSubMenu(new Menu("R 设置", "Rsettings"));
            Menu.SubMenu("Rsettings").AddSubMenu(new Menu("不R可击杀的", "DontRw"));
            Menu.SubMenu("Rsettings").SubMenu("DontRw").AddItem(new MenuItem("DontRwParam", "损害").SetValue(new StringList(new[] { "所有", "任何一个", "不" })));
            Menu.SubMenu("Rsettings").SubMenu("DontRw").AddItem(new MenuItem("DontRwQ", "Q").SetValue(true));
            Menu.SubMenu("Rsettings").SubMenu("DontRw").AddItem(new MenuItem("DontRwW", "W").SetValue(true));
            Menu.SubMenu("Rsettings").SubMenu("DontRw").AddItem(new MenuItem("DontRwE", "E").SetValue(true));
            Menu.SubMenu("Rsettings").SubMenu("DontRw").AddItem(new MenuItem("DontRwA", "1 x AA").SetValue(true));

            Menu.SubMenu("Rsettings").AddSubMenu(new Menu("不使用R", "DontR"));
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team))
                Menu.SubMenu("Rsettings").SubMenu("DontR").AddItem(new MenuItem("DontR" + enemy.BaseSkinName, enemy.BaseSkinName).SetValue(false));
            Menu.SubMenu("Rsettings").AddSubMenu(new Menu("已选择的目标", "DontRbuff"));
            Menu.SubMenu("Rsettings").SubMenu("DontRbuff").AddItem(new MenuItem("DontRbuffUndying", "蛮王-大招").SetValue(true));
            Menu.SubMenu("Rsettings").SubMenu("DontRbuff").AddItem(new MenuItem("DontRbuffJudicator", "大天使大招").SetValue(true));
            Menu.SubMenu("Rsettings").SubMenu("DontRbuff").AddItem(new MenuItem("DontRbuffAlistar", "时光-大招").SetValue(true));
            Menu.SubMenu("Rsettings").SubMenu("DontRbuff").AddItem(new MenuItem("DontRbuffZilean", "牛头-大招").SetValue(true));
            Menu.SubMenu("Rsettings").SubMenu("DontRbuff").AddItem(new MenuItem("DontRbuffZac", "扎克-被动").SetValue(true));
            Menu.SubMenu("Rsettings").SubMenu("DontRbuff").AddItem(new MenuItem("DontRbuffAttrox", "剑魔-被动").SetValue(true));
            Menu.SubMenu("Rsettings").SubMenu("DontRbuff").AddItem(new MenuItem("DontRbuffSivir", "战争女神-盾").SetValue(true));
            Menu.SubMenu("Rsettings").SubMenu("DontRbuff").AddItem(new MenuItem("DontRbuffMorgana", "莫甘娜-盾").SetValue(true));
            Menu.SubMenu("Rsettings").AddSubMenu(new Menu("击杀目标 xx%", "okR"));
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team))
                Menu.SubMenu("Rsettings").SubMenu("okR").AddItem(new MenuItem("okR" + enemy.BaseSkinName, enemy.BaseSkinName).SetValue(new Slider(0)));

            //Drawings
            Menu.AddSubMenu(new Menu("显示", "Drawing"));
            Menu.SubMenu("Drawing").AddItem(new MenuItem("DrawQ", "Q范围").SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
            Menu.SubMenu("Drawing").AddItem(new MenuItem("DrawW", "W范围").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            Menu.SubMenu("Drawing").AddItem(new MenuItem("DrawE", "E范围").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            Menu.SubMenu("Drawing").AddItem(new MenuItem("DrawR", "R范围").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            Menu.SubMenu("Drawing").AddItem(new MenuItem("DrawQE", "QE范围").SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
            Menu.SubMenu("Drawing").AddItem(new MenuItem("DrawQEC", "QE指示").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            Menu.SubMenu("Drawing").AddItem(new MenuItem("DrawQEMAP", "QE目标参数").SetValue(true));
            Menu.SubMenu("Drawing").AddItem(new MenuItem("DrawWMAP", "W目标参数").SetValue(true));
            Menu.SubMenu("Drawing").AddItem(new MenuItem("Gank", "可gank敌人").SetValue(true));
            Menu.SubMenu("Drawing").AddItem(new MenuItem("DrawHPFill", "显示伤害").SetValue(true));
            Menu.SubMenu("Drawing").AddItem(new MenuItem("HUD", "平视显示器").SetValue(true));
            Menu.SubMenu("Drawing").AddItem(new MenuItem("KillText", "击杀文本").SetValue(true));
            Menu.SubMenu("Drawing").AddItem(new MenuItem("KillTextHP", "% HP连招损伤文本").SetValue(true));
			Menu.SubMenu("Drawing").AddItem(new MenuItem("drawing", "连招损伤文本").SetValue(false));
			
			Menu.AddSubMenu(new Menu("超神汉化", "by welai"));
			Menu.SubMenu("by welai").AddItem(new MenuItem("qunhao", "汉化群：386289593"));

            //Add main menu
            Menu.AddToMainMenu();
            if (Menu.Item("Sound1").GetValue<bool>()) PlaySound(welcome);
            GameObject.OnCreate += OnCreate;
            GameObject.OnDelete += OnDelete;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
            if (Menu.Item("Orbwalker_Mode").GetValue<bool>()) Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;

            Game.PrintChat("<font color = \"#FF0020\">Gagong Syndra</font> by <font color = \"#22FF10\">stephenjason89</font>");
            Game.PrintChat("<font color = \"#FF00FF\">Updates by BarackObama</font>");
        }

        private static void ChooseOrbwalker(bool mode)
        {
            if (mode)
            {
                _orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
                comboKey = Menu.Item("Orbwalk");
                harassKey = Menu.Item("Farm");
                laneclearKey = Menu.Item("LaneClear");
                lanefreezeKey = Menu.Item("LaneClear");
                Game.PrintChat("Regular Orbwalker Loaded");
            }
            else
            {
                xSLxOrbwalker.AddToMenu(orbwalkerMenu);
                comboKey = Menu.Item("Combo_Key");
                harassKey = Menu.Item("Harass_Key");
                laneclearKey = Menu.Item("LaneClear_Key");
                lanefreezeKey = Menu.Item("LaneFreeze_Key");
                Game.PrintChat("xSLx Orbwalker Loaded");
            }
        }
        private static void OnCreate(GameObject obj, EventArgs args)
        {
            if (Player.Distance(obj.Position) > 1500 || !ObjectManager.Get<Obj_AI_Hero>().Any(h => h.ChampionName == "Yasuo" && h.IsEnemy && h.IsVisible && !h.IsDead)) return;
            //Yasuo Wall
            if (obj.IsValid &&
                System.Text.RegularExpressions.Regex.IsMatch(
                    obj.Name, "_w_windwall.\\.troy",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                _yasuoWall = obj;
            }
        }

        private static void OnDelete(GameObject obj, EventArgs args)
        {
            if (Player.Distance(obj.Position) > 1500 || !ObjectManager.Get<Obj_AI_Hero>().Any(h => h.ChampionName == "Yasuo" && h.IsEnemy && h.IsVisible && !h.IsDead)) return;
            //Yasuo Wall
            if (obj.IsValid && System.Text.RegularExpressions.Regex.IsMatch(
                obj.Name, "_w_windwall.\\.troy",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                _yasuoWall = null;
            }
        }

        private static bool DetectCollision(GameObject target)
        {
            if (_yasuoWall==null || !Menu.Item("YasuoWall").GetValue<bool>() || !ObjectManager.Get<Obj_AI_Hero>().Any(h => h.ChampionName == "Yasuo" && h.IsEnemy && h.IsVisible && !h.IsDead)) return true;

            var level = _yasuoWall.Name.Substring(_yasuoWall.Name.Length - 6, 1);
            var wallWidth = (300 + 50 * Convert.ToInt32(level));
            var wallDirection = (_yasuoWall.Position.To2D() - _yasuoWallCastedPos).Normalized().Perpendicular();
            var wallStart = _yasuoWall.Position.To2D() + ((int)(wallWidth / 2)) * wallDirection;
            var wallEnd = wallStart - wallWidth * wallDirection;

            var intersection = wallStart.Intersection(wallEnd, Player.Position.To2D(), target.Position.To2D());

            return !intersection.Point.IsValid() || !(Environment.TickCount + Game.Ping + R.Delay - _wallCastT < 4000);
             
        }
        private static void PlaySound(SoundPlayer sound = null)
        {
            if (sound != null)
            {
                try
                {
                    sound.Play();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            else if (Environment.TickCount - LastPlayedSound > 45000 && Menu.Item("Sound2").GetValue<bool>())
            {
                var rnd = new Random();
                switch (rnd.Next(1, 11))
                {
                    case 1:
                        PlaySound(imkillingthebitch);
                        break;
                    case 2:
                        PlaySound(ballstotheface);
                        break;
                    case 3:
                        PlaySound(diefucker);
                        break;
                    case 4:
                        PlaySound(goingsomewhereasshole);
                        break;
                    case 5:
                        PlaySound(ilovethisgame);
                        break;
                    case 6:
                        PlaySound(imkillingthebitch);
                        break;
                    case 7:
                        PlaySound(ohdontyoudare);
                        break;
                    case 8:
                        PlaySound(ohidiot);
                        break;
                    case 9:
                        PlaySound(whosthebitchnow);
                        break;
                    case 10:
                        PlaySound(yourdeadmeatasshole);
                        break;
                }
                LastPlayedSound = Environment.TickCount;
            }
        }
        static void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead) return;

            //Update R Range
            R.Range = R.Level == 3 ? 750f : 675f;

            //Update E Width
            E.Width = E.Level == 5 ? 45f : (float)(45 * 0.5);

            //Update QE Range
            var qeRnew = Menu.Item("QEMR").GetValue<Slider>().Value * .01 * 1292;
            QE.Range = (float) qeRnew;
            
            //Use QE to Mouse Position
            if (Menu.Item("UseQEC").GetValue<KeyBind>().Active && E.IsReady() && Q.IsReady())
            {
                foreach (
                    var enemy in
                        ObjectManager.Get<Obj_AI_Hero>()
                            .Where(
                                enemy =>
                                    enemy.Team != Player.Team && Player.Distance(enemy, true) <= Math.Pow(QE.Range, 2))
                            .Where(
                                enemy =>
                                    enemy.IsValidTarget(QE.Range) && enemy.Distance(Game.CursorPos, true) <= 150 * 150))
                {
                    UseQe(enemy);
                }
            }

            //Combo
            if (comboKey.GetValue<KeyBind>().Active)
                Combo();
            
            
            //Harass
            else if (harassKey.GetValue<KeyBind>().Active || Menu.Item("HarassActiveT").GetValue<KeyBind>().Active)
            {
                if (Menu.Item("HarassTurret").GetValue<bool>() && !harassKey.GetValue<KeyBind>().Active)
                {
                    var turret = ObjectManager.Get<Obj_AI_Turret>().FirstOrDefault(t => t.IsValidTarget(Q.Range));
                    if (turret == null) Harass();
                }
                else Harass();
            }
            
            //Auto KS
            if (Menu.Item("AutoKST").GetValue<KeyBind>().Active)
            {
                AutoKs();
            }
            //Farm
            if (comboKey.GetValue<KeyBind>().Active)
                return;
            var lc = laneclearKey.GetValue<KeyBind>().Active;
            if (lc || lanefreezeKey.GetValue<KeyBind>().Active)
                Farm(lc);
            if (laneclearKey.GetValue<KeyBind>().Active)
                JungleFarm();
        }

        private static void Combo()
        {
            UseSpells(Menu.Item("UseQ").GetValue<bool>(), //Q
                      Menu.Item("UseW").GetValue<bool>(), //W
                      Menu.Item("UseE").GetValue<bool>(), //E
                      Menu.Item("UseR").GetValue<bool>(), //R
                      Menu.Item("UseQE").GetValue<bool>() //QE
                      );
        }

        private static void Harass()
        {
            if (Player.Mana / Player.MaxMana * 100 < Menu.Item("HarassMana").GetValue<Slider>().Value) return;
            UseSpells(Menu.Item("UseQH").GetValue<bool>(), //Q
                      Menu.Item("UseWH").GetValue<bool>(), //W
                      Menu.Item("UseEH").GetValue<bool>(), //E
                      false,                               //R
                      Menu.Item("UseQEH").GetValue<bool>() //QE 
                      );
        }
        private static void Farm(bool laneClear)
        {
            if (!Orbwalking.CanMove(40)) return;
            var rangedMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range + Q.Width + 30,
            MinionTypes.Ranged);
            var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range + Q.Width + 30);
            var rangedMinionsW = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, W.Range + W.Width + 30,
            MinionTypes.Ranged);
            var allMinionsW = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, W.Range + W.Width + 30);
            var useQi = Menu.Item("UseQFarm").GetValue<StringList>().SelectedIndex;
            var useWi = Menu.Item("UseWFarm").GetValue<StringList>().SelectedIndex;
            var useQ = (laneClear && (useQi == 1 || useQi == 2)) || (!laneClear && (useQi == 0 || useQi == 2));
            var useW = (laneClear && (useWi == 1 || useWi == 2)) || (!laneClear && (useWi == 0 || useWi == 2));
            if (useQ && Q.IsReady())
                if (laneClear)
                {
                    var fl1 = Q.GetCircularFarmLocation(rangedMinionsQ, Q.Width);
                    var fl2 = Q.GetCircularFarmLocation(allMinionsQ, Q.Width);
                    if (fl1.MinionsHit >= 3)
                    {
                        Q.Cast(fl1.Position, Menu.Item("Packets").GetValue<bool>());
                    }
                    else if (fl2.MinionsHit >= 2 || allMinionsQ.Count == 1)
                    {
                        Q.Cast(fl2.Position, Menu.Item("Packets").GetValue<bool>());
                    }
                }
                else
                    foreach (var minion in allMinionsQ.Where(minion => !Orbwalking.InAutoAttackRange(minion) &&
                                                                       minion.Health < 0.75 * Player.GetSpellDamage(minion, SpellSlot.Q)))
                        Q.Cast(minion, Menu.Item("Packets").GetValue<bool>());
            if (!useW || !W.IsReady() || allMinionsW.Count <= 3 || !laneClear)
                return;
            if (Player.Spellbook.GetSpell(SpellSlot.W).ToggleState == 1)
            {
                //WObject
                var gObjectPos = GetGrabableObjectPos(false);
                if (gObjectPos.To2D().IsValid() && Environment.TickCount - W.LastCastAttemptT > Game.Ping + 150)
                {
                    W.Cast(gObjectPos, Menu.Item("Packets").GetValue<bool>());
                }
            }
            else if (Player.Spellbook.GetSpell(SpellSlot.W).ToggleState != 1)
            {
                var fl1 = Q.GetCircularFarmLocation(rangedMinionsW, W.Width);
                var fl2 = Q.GetCircularFarmLocation(allMinionsW, W.Width);
                if (fl1.MinionsHit >= 3 && W.IsInRange(fl1.Position.To3D()))
                {
                    W.Cast(fl1.Position, Menu.Item("Packets").GetValue<bool>());
                }
                else if (fl2.MinionsHit >= 1 && W.IsInRange(fl2.Position.To3D()) && fl1.MinionsHit <= 2)
                {
                    W.Cast(fl2.Position, Menu.Item("Packets").GetValue<bool>());
                }
            }
        }
        private static void JungleFarm()
        {
            var useQ = Menu.Item("UseQJFarm").GetValue<bool>();
            var useW = Menu.Item("UseWJFarm").GetValue<bool>();
            var useE = Menu.Item("UseEJFarm").GetValue<bool>();
            var mobs = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, W.Range, MinionTypes.All,
            MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            if (mobs.Count <= 0)
                return;
            var mob = mobs[0];
            if (Q.IsReady() && useQ)
            {
                Q.Cast(mob, Menu.Item("Packets").GetValue<bool>());
            }
            if (W.IsReady() && useW && Environment.TickCount - Q.LastCastAttemptT > 800)
            {
                W.Cast(mob, Menu.Item("Packets").GetValue<bool>());
            }
            if (useE && E.IsReady())
            {
                E.Cast(mob, Menu.Item("Packets").GetValue<bool>());
            }
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {   
            //Last cast time of spells
            if (sender.IsMe)
            {
                if (args.SData.Name == "SyndraQ")
                    Q.LastCastAttemptT = Environment.TickCount;
                if (args.SData.Name == "SyndraW" || args.SData.Name == "syndrawcast")
                    W.LastCastAttemptT = Environment.TickCount;
                if (args.SData.Name == "SyndraE" || args.SData.Name == "syndrae5")
                    E.LastCastAttemptT = Environment.TickCount;
            }
            
            //Harass when enemy do attack
            if (Menu.Item("HarassAAQ").GetValue<bool>() && sender.Type == Player.Type && sender.Team != Player.Team && args.SData.Name.ToLower().Contains("attack") && Player.Distance(sender, true) <= Math.Pow(Q.Range, 2) && Player.Mana / Player.MaxMana * 100 > Menu.Item("HarassMana").GetValue<Slider>().Value)  
            {
                UseQ((Obj_AI_Hero)sender);
            }
            if (!sender.IsValid || sender.Team == ObjectManager.Player.Team || args.SData.Name != "YasuoWMovingWall")
                return;
            _wallCastT = Environment.TickCount;
            _yasuoWallCastedPos = sender.ServerPosition.To2D();
        }
        
        //Anti gapcloser
        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!Menu.Item("AntiGap").GetValue<bool>()) return;

            if (!E.IsReady() || !(Player.Distance(gapcloser.Sender, true) <= Math.Pow(QE.Range, 2)) ||
                !gapcloser.Sender.IsValidTarget(QE.Range))
                return;
            if (Q.IsReady() && Player.Spellbook.GetSpell(SpellSlot.Q).ManaCost + Player.Spellbook.GetSpell(SpellSlot.E).ManaCost <= Player.Mana)
            {
                UseQe(gapcloser.Sender);
            }
            else if (Player.Distance(gapcloser.Sender, true) <= Math.Pow(E.Range, 2))
                E.Cast(gapcloser.End, Menu.Item("Packets").GetValue<bool>());
        }

        //Interrupt dangerous spells
        private static void Interrupter_OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!Menu.Item("Interrupt").GetValue<bool>()) return;

            if (E.IsReady() && Player.Distance(unit, true) <= Math.Pow(E.Range, 2) && unit.IsValidTarget(E.Range))
            {
                if (Q.IsReady())
                    UseQe(unit);
                else
                    E.Cast(unit, Menu.Item("Packets").GetValue<bool>());
            }
            else if (Q.IsReady() && E.IsReady() && Player.Distance(unit, true) <= Math.Pow(QE.Range, 2))
                UseQe((Obj_AI_Hero)unit);
        }

        static void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            var orbwalkAa = false;
            if(Menu.Item("OrbWAA").GetValue<bool>()) orbwalkAa = !Q.IsReady() && (!W.IsReady() || !E.IsReady());
            if (comboKey.GetValue<KeyBind>().Active)
                args.Process = orbwalkAa;
        }

        private static float GetComboDamage(Obj_AI_Base enemy, bool UQ, bool UW, bool UE, bool UR, bool UDFG = true)
        {
            if (enemy == null)
                return 0f;
            var damage = 0d;
            var combomana = 0d;
            var useR = Menu.Item("DontR" + enemy.BaseSkinName) != null && Menu.Item("DontR" + enemy.BaseSkinName).GetValue<bool>() == false;
            
            //Add R Damage
            if (R.IsReady() && UR && useR)
            {
                combomana += Player.Spellbook.GetSpell(SpellSlot.R).ManaCost;
                if (combomana <= Player.Mana) damage += GetRDamage(enemy);
                else combomana -= Player.Spellbook.GetSpell(SpellSlot.R).ManaCost;
            }

            //Add Q Damage
            if (Q.IsReady() && UQ)
            {
                combomana += Player.Spellbook.GetSpell(SpellSlot.Q).ManaCost;
                if (combomana <= Player.Mana) damage += Player.GetSpellDamage(enemy, SpellSlot.Q);
                else combomana -= Player.Spellbook.GetSpell(SpellSlot.Q).ManaCost;
            }

            //Add E Damage
            if (E.IsReady() && UE)
            {
                combomana += Player.Spellbook.GetSpell(SpellSlot.E).ManaCost;
                if (combomana <= Player.Mana) damage += Player.GetSpellDamage(enemy, SpellSlot.E);
                else combomana -= Player.Spellbook.GetSpell(SpellSlot.E).ManaCost;
            }

            //Add W Damage
            if (W.IsReady() && UW)
            {
                combomana += Player.Spellbook.GetSpell(SpellSlot.W).ManaCost;
                if (combomana <= Player.Mana) damage += Player.GetSpellDamage(enemy, SpellSlot.W);
                else combomana -= Player.Spellbook.GetSpell(SpellSlot.W).ManaCost;
            }
            

            //Add damage DFG
            if (UDFG && DFG.IsReady()) damage += Player.GetItemDamage(enemy, Damage.DamageItems.Dfg) / 1.2;

            //DFG multiplier
            return (float)((DFG.IsReady() && UDFG || DfgBuff(enemy)) ? damage * 1.2 : damage);
        }

        private static float GetRDamage(Obj_AI_Base enemy)
        {
            if (!R.IsReady()) return 0f;
            var damage = 45 + R.Level * 45 + Player.FlatMagicDamageMod * 0.2f; 
            return (float)Player.CalcDamage(enemy, Damage.DamageType.Magical, damage) * Player.Spellbook.GetSpell(SpellSlot.R).Ammo;
        }

        private static float GetIgniteDamage(Obj_AI_Base enemy)
        {
            if (IgniteSlot == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(IgniteSlot) != SpellState.Ready) return 0f;
            return (float)Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);
        }

        private static bool DfgBuff(Obj_AI_Base enemy)
        {
            return enemy.HasBuff("deathfiregraspspell", true) || enemy.HasBuff("itemblackfiretorchspell", true);
        }
       
        //Check R Only If QEW on CD
        private static bool RCheck(Obj_AI_Hero enemy)
        {
            double aa = 0;
            if(Menu.Item("DontRwA").GetValue<bool>()) aa = Player.GetAutoAttackDamage(enemy);
            //Menu check
            if (Menu.Item("DontRwParam").GetValue<StringList>().SelectedIndex==2) return true;

            //If can be killed by all the skills that are checked
            if (Menu.Item("DontRwParam").GetValue<StringList>().SelectedIndex == 0 && GetComboDamage(enemy, Menu.Item("DontRwQ").GetValue<bool>(), Menu.Item("DontRwW").GetValue<bool>(), Menu.Item("DontRwE").GetValue<bool>(), false, false) + aa >= enemy.Health) return false;
            //If can be killed by either any of the skills
            if (Menu.Item("DontRwParam").GetValue<StringList>().SelectedIndex == 1 && (GetComboDamage(enemy, Menu.Item("DontRwQ").GetValue<bool>(),false,false,false,false) >= enemy.Health || GetComboDamage(enemy, Menu.Item("DontRwW").GetValue<bool>(),false,false,false,false) >= enemy.Health || GetComboDamage(enemy, Menu.Item("DontRwE").GetValue<bool>(),false,false,false,false) >= enemy.Health || aa>=enemy.Health)) return false;
            
            //Check last cast times
            return Environment.TickCount - Q.LastCastAttemptT > 600 + Game.Ping && Environment.TickCount - E.LastCastAttemptT > 600 + Game.Ping && Environment.TickCount - W.LastCastAttemptT > 600 + Game.Ping;
        }

        private static void AutoKs()
        {
            foreach (
                var enemy in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(enemy => enemy.Team != Player.Team)
                        .Where(
                            enemy =>
                                !enemy.HasBuff("UndyingRage") && !enemy.HasBuff("JudicatorIntervention") &&
                                enemy.IsValidTarget(QE.Range) && Environment.TickCount - FlashLastCast > 650 + Game.Ping)
                )
            {
                if (GetComboDamage(enemy, false, false, Menu.Item("UseQEKS").GetValue<bool>(), false, false) >
                    enemy.Health && Player.Distance(enemy, true) <= Math.Pow(QE.Range, 2))
                {
                    UseSpells(
                        false, //Q
                        false, //W
                        false, //E
                        false, //R
                        Menu.Item("UseQEKS").GetValue<bool>() //QE
                        );
                    PlaySound();
                    //Game.PrintChat("QEKS " + enemy.Name);
                }
                else if (GetComboDamage(enemy, false, Menu.Item("UseWKS").GetValue<bool>(), false, false, false) >
                         enemy.Health && Player.Distance(enemy, true) <= Math.Pow(W.Range, 2))
                {
                    UseSpells(
                        false, //Q
                        Menu.Item("UseWKS").GetValue<bool>(), //W
                        false, //E
                        false, //R
                        false //QE
                        );
                    PlaySound();
                    //Game.PrintChat("WKS " + enemy.Name);
                }
                else if (
                    GetComboDamage(
                        enemy, Menu.Item("UseQKS").GetValue<bool>(), false, Menu.Item("UseEKS").GetValue<bool>(),
                        false, false) > enemy.Health &&
                    Player.Distance(enemy, true) <= Math.Pow(Q.Range + 25f, 2))
                {
                    UseSpells(
                        Menu.Item("UseQKS").GetValue<bool>(), //Q
                        false, //W
                        Menu.Item("UseEKS").GetValue<bool>(), //E
                        false, //R
                        false //QE
                        );
                    PlaySound();
                    //Game.PrintChat("QEKSC " + enemy.Name);
                }
                else if (
                    GetComboDamage(
                        enemy, Menu.Item("UseQKS").GetValue<bool>(), Menu.Item("UseWKS").GetValue<bool>(),
                        Menu.Item("UseEKS").GetValue<bool>(), Menu.Item("UseRKS").GetValue<bool>()) >
                    enemy.Health && Player.Distance(enemy, true) <= Math.Pow(R.Range, 2))
                {
                    UseSpells(
                        Menu.Item("UseQKS").GetValue<bool>(), //Q
                        Menu.Item("UseWKS").GetValue<bool>(), //W
                        Menu.Item("UseEKS").GetValue<bool>(), //E
                        Menu.Item("UseRKS").GetValue<bool>(), //R
                        Menu.Item("UseQEKS").GetValue<bool>() //QE
                        );
                    PlaySound();
                    //Game.PrintChat("QWERKS " + enemy.Name);
                }
                else if (
                    (GetComboDamage(
                        enemy, false, false, Menu.Item("UseEKS").GetValue<bool>(),
                        Menu.Item("UseRKS").GetValue<bool>(), false) > enemy.Health ||
                     GetComboDamage(
                         enemy, false, Menu.Item("UseWKS").GetValue<bool>(),
                         Menu.Item("UseEKS").GetValue<bool>(), false, false) > enemy.Health) &&
                    Player.Distance(enemy, true) <= Math.Pow(QE.Range, 2))
                {
                    UseSpells(
                        false, //Q
                        false, //W
                        false, //E
                        false, //R
                        Menu.Item("UseQEKS").GetValue<bool>() //QE
                        );
                    PlaySound();
                    //Game.PrintChat("QEKS " + enemy.Name);
                }
                //Flash Kill
                var useFlash = Menu.Item("FKT" + enemy.BaseSkinName) != null &&
                               Menu.Item("FKT" + enemy.BaseSkinName).GetValue<bool>();
                var useR = Menu.Item("DontR" + enemy.BaseSkinName) != null &&
                           Menu.Item("DontR" + enemy.BaseSkinName).GetValue<bool>() == false;
                var rflash =
                    GetComboDamage(
                        enemy, Menu.Item("UseQKS").GetValue<bool>(), false, Menu.Item("UseEKS").GetValue<bool>(), false,
                        false) < enemy.Health;
                var ePos = R.GetPrediction(enemy);
                if ((FlashSlot == SpellSlot.Unknown && Player.Spellbook.CanUseSpell(FlashSlot) != SpellState.Ready) ||
                    !useFlash || !(Player.Distance(ePos.UnitPosition, true) <= Math.Pow(Q.Range + 25f + 395, 2)) ||
                    !(Player.Distance(ePos.UnitPosition, true) > Math.Pow(Q.Range + 25f + 200, 2)))
                    continue;
                if (
                    (!(GetComboDamage(
                        enemy, Menu.Item("UseQKS").GetValue<bool>(), false, Menu.Item("UseEKS").GetValue<bool>(), false,
                        false) > enemy.Health) || !Menu.Item("UseFK1").GetValue<bool>()) &&
                    (!(GetComboDamage(enemy, false, false, false, Menu.Item("UseRKS").GetValue<bool>()) > enemy.Health) ||
                     !Menu.Item("UseFK2").GetValue<bool>() ||
                     !(Player.Distance(ePos.UnitPosition, true) <= Math.Pow(R.Range + 390, 2)) ||
                     Environment.TickCount - R.LastCastAttemptT <= Game.Ping + 750 ||
                     Environment.TickCount - QE.LastCastAttemptT <= Game.Ping + 750 ||
                     !(Player.Distance(ePos.UnitPosition, true) > Math.Pow(R.Range + 200, 2))))
                    continue;
                var totmana = 0d;
                if (Menu.Item("FKMANA").GetValue<bool>())
                {
                    totmana = SpellList.Aggregate(
                        totmana, (current, spell) => current + Player.Spellbook.GetSpell(spell.Slot).ManaCost);
                }
                if (totmana > Player.Mana && Menu.Item("FKMANA").GetValue<bool>() &&
                    Menu.Item("FKMANA").GetValue<bool>())
                    continue;
                var nearbyE = ePos.UnitPosition.CountEnemysInRange(1000);
                if (nearbyE > Menu.Item("MaxE").GetValue<Slider>().Value)
                    continue;
                var flashPos = Player.ServerPosition -
                               Vector3.Normalize(Player.ServerPosition - ePos.UnitPosition) * 400;
                if (flashPos.IsWall())
                    continue;
                if (rflash)
                {
                    if (useR)
                    {
                        //Use Ult after flash if can't be killed by QE
                        Player.Spellbook.CastSpell(FlashSlot, flashPos);
                        UseSpells(
                            false, //Q
                            false, //W
                            false, //E
                            Menu.Item("UseRKS").GetValue<bool>(), //R
                            false //QE
                            );
                        PlaySound();
                    }
                }
                else
                {
                    //Q & E after flash
                    Player.Spellbook.CastSpell(FlashSlot, flashPos);
                }
                FlashLastCast = Environment.TickCount;
            }
        }

        private static bool BuffCheck(Obj_AI_Base enemy)
        {
            var buff = 0;
            if (enemy.HasBuff("UndyingRage") && Menu.Item("DontRbuffUndying").GetValue<bool>()) buff++;
            if (enemy.HasBuff("JudicatorIntervention") && Menu.Item("DontRbuffJudicator").GetValue<bool>()) buff++; 
            if (enemy.HasBuff("ZacRebirthReady") && Menu.Item("DontRbuffZac").GetValue<bool>()) buff++;  
            if (enemy.HasBuff("AttroxPassiveReady") && Menu.Item("DontRbuffAttrox").GetValue<bool>()) buff++;  
            if (enemy.HasBuff("Spell Shield") && Menu.Item("DontRbuffSivir").GetValue<bool>()) buff++;  
            if (enemy.HasBuff("Black Shield") && Menu.Item("DontRbuffMorgana").GetValue<bool>()) buff++;
            if (enemy.HasBuff("Chrono Shift") && Menu.Item("DontRbuffZilean").GetValue<bool>()) buff++;
            if (enemy.HasBuff("Ferocious Howl") && Menu.Item("DontRbuffAlistar").GetValue<bool>()) buff++;

            return buff <= 0;
        }
        private static void UseSpells(bool uq, bool uw, bool ue, bool ur, bool uqe)
        {   
            //Set Target
            var qTarget = TargetSelector.GetTarget(Q.Range + 25f, TargetSelector.DamageType.Magical);
            var wTarget = TargetSelector.GetTarget(W.Range + W.Width, TargetSelector.DamageType.Magical);
            var rTarget = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
            var qeTarget = TargetSelector.GetTarget(QE.Range, TargetSelector.DamageType.Magical);
            //Use DFG
            if (DFG.IsReady() && rTarget != null && GetComboDamage(rTarget, uq, uw, ue, ur) + GetIgniteDamage(rTarget) > rTarget.Health && DetectCollision(rTarget))
            {
                //DFG
                if (Player.Distance(rTarget, true) <= Math.Pow(DFG.Range, 2) && GetComboDamage(rTarget, uq, uw, ue, false, false) + GetIgniteDamage(qTarget) < rTarget.Health)
                    if((ur && R.IsReady()) || (uq && Q.IsReady())) DFG.Cast(rTarget);
            }
           
            //Harass Combo Key Override
            if (rTarget != null && (harassKey.GetValue<KeyBind>().Active || laneclearKey.GetValue<KeyBind>().Active) && comboKey.GetValue<KeyBind>().Active && Player.Distance(rTarget, true) <= Math.Pow(R.Range, 2) && BuffCheck(rTarget) && DetectCollision(rTarget))
            {
                    DFG.Cast(qTarget);
                    if (Menu.Item("DontR" + rTarget.BaseSkinName) != null && Menu.Item("DontR" + rTarget.BaseSkinName).GetValue<bool>() == false && ur)
                    {
                        R.CastOnUnit(rTarget, Menu.Item("Packets").GetValue<bool>());
                        R.LastCastAttemptT = Environment.TickCount;
                    }
            }

            if (R.IsReady())
            {
                //R, Ignite 
                foreach (var enemy in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(
                            enemy =>
                                enemy.Team != Player.Team && enemy.IsValidTarget(R.Range) && !enemy.IsDead &&
                                BuffCheck(enemy)))
                {
                    //R
                    var useR = Menu.Item("DontR" + enemy.BaseSkinName).GetValue<bool>() == false && ur;
                    var okR = Menu.Item("okR" + enemy.BaseSkinName).GetValue<Slider>().Value * .01 + 1;
                    if (DetectCollision(enemy) && useR && Player.Distance(enemy, true) <= Math.Pow(R.Range, 2) &&
                        (DfgBuff(enemy) ? GetRDamage(enemy) * 1.2 : GetRDamage(enemy)) > enemy.Health * okR &&
                        RCheck(enemy))
                    {
                        if (
                            !(Player.GetSpellDamage(enemy, SpellSlot.Q) > enemy.Health &&
                              Player.Spellbook.GetSpell(SpellSlot.Q).CooldownExpires - Game.Time < 2 &&
                              Player.Spellbook.GetSpell(SpellSlot.Q).CooldownExpires - Game.Time >= 0 && enemy.IsStunned) &&
                            Environment.TickCount - Q.LastCastAttemptT > 500 + Game.Ping)
                        {
                            R.CastOnUnit(enemy, Menu.Item("Packets").GetValue<bool>());
                            R.LastCastAttemptT = Environment.TickCount;
                        }

                    }
                    //Ignite
                    if (!(Player.Distance(enemy, true) <= 600 * 600) || !(GetIgniteDamage(enemy) > enemy.Health))
                        continue;
                    if (Menu.Item("IgniteALLCD").GetValue<bool>())
                    {
                        if (!Q.IsReady() && !W.IsReady() && !E.IsReady() && !R.IsReady() &&
                            Environment.TickCount - R.LastCastAttemptT > Game.Ping + 750 &&
                            Environment.TickCount - QE.LastCastAttemptT > Game.Ping + 750 &&
                            Environment.TickCount - W.LastCastAttemptT > Game.Ping + 750)
                            Player.Spellbook.CastSpell(IgniteSlot, enemy);
                    }
                    else
                        Player.Spellbook.CastSpell(IgniteSlot, enemy);

                }
            }

            //Use QE
            if (uqe && DetectCollision(qeTarget) && qeTarget != null && Q.IsReady() && (E.IsReady() || (Player.Spellbook.GetSpell(SpellSlot.E).CooldownExpires - Game.Time < 1 && Player.Spellbook.GetSpell(SpellSlot.E).Level > 0)) && Player.Spellbook.GetSpell(SpellSlot.Q).ManaCost + Player.Spellbook.GetSpell(SpellSlot.E).ManaCost <= Player.Mana)
            {
                UseQe(qeTarget);
            }

            //Use Q
            else if (uq && qTarget != null)
            {
                UseQ(qTarget);
            }

            //Use E
            if (ue && E.IsReady() && Environment.TickCount - W.LastCastAttemptT > Game.Ping + 150 && Environment.TickCount - QWLastcast > Game.Ping)
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team).Where(enemy => enemy.IsValidTarget(E.Range))) {
                    if (GetComboDamage(enemy, uq, uw, ue, ur) > enemy.Health && Player.Distance(enemy, true) <= Math.Pow(E.Range, 2))
                        E.Cast(enemy, Menu.Item("Packets").GetValue<bool>());
                    else if (Player.Distance(enemy, true) <= Math.Pow(QE.Range, 2))
                        UseE(enemy);
                }
            //Use W
            if (uw) UseW(qeTarget, wTarget); 
        }
        private static Vector3 GetGrabableObjectPos(bool onlyOrbs)
        {
            if (onlyOrbs) 
                return OrbManager.GetOrbToGrab((int) W.Range);
            foreach (var minion in ObjectManager.Get<Obj_AI_Minion>().Where(minion => minion.IsValidTarget(W.Range)))
                return minion.ServerPosition;
            return OrbManager.GetOrbToGrab((int)W.Range);
        }

        private static void UseQ(Obj_AI_Base target)
        {
            if (!Q.IsReady()) return;
            var pos = Q.GetPrediction(target, true);
            if (pos.Hitchance >= HitChance.VeryHigh)
                Q.Cast(pos.CastPosition, Menu.Item("Packets").GetValue<bool>());
        }
        private static void UseW(Obj_AI_Base qeTarget, Obj_AI_Base wTarget)
        {
            //Use W1
            if (qeTarget != null && W.IsReady() && Player.Spellbook.GetSpell(SpellSlot.W).ToggleState == 1)
            {
                var gObjectPos = GetGrabableObjectPos(false);

                if (gObjectPos.To2D().IsValid() && Environment.TickCount - Q.LastCastAttemptT > Game.Ping + 150 && Environment.TickCount - E.LastCastAttemptT > 750 + Game.Ping && Environment.TickCount - W.LastCastAttemptT > 750 + Game.Ping)
                {
                    var grabsomething = false;
                    if (wTarget != null)
                    {
                        var pos2 = W.GetPrediction(wTarget, true);
                        if (pos2.Hitchance >= HitChance.High) grabsomething = true;
                    }
                    if (grabsomething || qeTarget.IsStunned)
                        W.Cast(gObjectPos, Menu.Item("Packets").GetValue<bool>());
                }
            }
//            Game.PrintChat("wObject: " + OrbManager.WObject(false) + " Target " + wTarget.BaseSkinName + " toggle " + Player.Spellbook.GetSpell(SpellSlot.W).ToggleState + " isready: " + W.IsReady());
            //Use W2
//            if (wTarget == null || Player.Spellbook.GetSpell(SpellSlot.W).ToggleState != 2 || !W.IsReady())
 //               return;
            if (wTarget != null && W.IsReady() && Player.Spellbook.GetSpell(SpellSlot.W).ToggleState == 2)
            {

//                W.UpdateSourcePosition(OrbManager.WObject(false).ServerPosition);
                var pos = W.GetPrediction(wTarget, true);
//                Game.PrintChat("casting w " + Player.Spellbook.GetSpell(SpellSlot.W).ToggleState + " pred"+pos.Hitchance);
                if (pos.Hitchance >= HitChance.High)
                    W.Cast(pos.CastPosition, Menu.Item("Packets").GetValue<bool>());
            }
        }
        private static void UseE(Obj_AI_Base target)
        {
            if (target == null)
                return;
            foreach (var orb in OrbManager.GetOrbs(true).Where(orb => orb.To2D().IsValid() && Player.Distance(orb, true) < Math.Pow(E.Range, 2)))
                {
                    var sp = orb.To2D() + Vector2.Normalize(Player.ServerPosition.To2D() - orb.To2D()) * 100f;
                    var ep = orb.To2D() + Vector2.Normalize(orb.To2D() - Player.ServerPosition.To2D()) * 592;
                    QE.Delay = E.Delay + Player.Distance(orb) / E.Speed;
                    QE.UpdateSourcePosition(orb);
                    var pPo = QE.GetPrediction(target).UnitPosition.To2D();
                    if (pPo.Distance(sp, ep, true, true) <= Math.Pow(QE.Width + target.BoundingRadius, 2))
                        E.Cast(orb, Menu.Item("Packets").GetValue<bool>());                
                }
        }
        
        private static void UseQe(Obj_AI_Base target)
        {
            if (!Q.IsReady() || !E.IsReady() || target == null) return;
            var sPos = Prediction.GetPrediction(target, Q.Delay + E.Delay).UnitPosition;
            if (Player.Distance(sPos, true) > Math.Pow(E.Range, 2))
            {
                var orb = Player.ServerPosition + Vector3.Normalize(sPos - Player.ServerPosition) * E.Range;
                QE.Delay = Q.Delay + E.Delay + Player.Distance(orb) / E.Speed;
                var pos = QE.GetPrediction(target);
                if (pos.Hitchance >= HitChance.Medium)
                {
                    UseQe2(target, orb);
                }
            }
            else
            {
                Q.Width = 40f;
                var pos = Q.GetPrediction(target, true);
                Q.Width = 125f;
                if (pos.Hitchance >= HitChance.VeryHigh)
                    UseQe2(target, pos.UnitPosition);
            }
        }
        private static void UseQe2(Obj_AI_Base target, Vector3 pos)
        {
            if (target == null || !(Player.Distance(pos, true) <= Math.Pow(E.Range, 2)))
                return;
            var sp = pos + Vector3.Normalize(Player.ServerPosition - pos) * 100f;
            var ep = pos + Vector3.Normalize(pos - Player.ServerPosition) * 592;
            QE.Delay = Q.Delay + E.Delay + Player.ServerPosition.Distance(pos) / E.Speed;
            QE.UpdateSourcePosition(pos);
            var pPo = QE.GetPrediction(target).UnitPosition.To2D().ProjectOn(sp.To2D(), ep.To2D());
            if (!pPo.IsOnSegment || !(pPo.SegmentPoint.Distance(target, true) <= Math.Pow(QE.Width + target.BoundingRadius, 2)))
                return;
            var delay = 280 - (int)(Player.Distance(pos) / 2.5) + Menu.Item("QEDelay").GetValue<Slider>().Value;
            Utility.DelayAction.Add(Math.Max(0, delay), () => E.Cast(pos, Menu.Item("Packets").GetValue<bool>()));
            QE.LastCastAttemptT = Environment.TickCount;
            Q.Cast(pos, Menu.Item("Packets").GetValue<bool>());
            UseE(target);
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            var menuItem = Menu.Item("DrawQE").GetValue<Circle>();
            if (menuItem.Active) Render.Circle.DrawCircle(Player.Position, QE.Range, menuItem.Color);
            menuItem = Menu.Item("DrawQEC").GetValue<Circle>();
            if (Menu.Item("drawing").GetValue<bool>())
            {
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team))
                {
                    if (enemy.IsVisible && !enemy.IsDead)
                    {
                        //Draw Combo Damage to Enemy HP bars

                        var hpBarPos = enemy.HPBarPosition;
                        hpBarPos.X += 45;
                        hpBarPos.Y += 18;
                        var killText = "";
                        var combodamage = GetComboDamage(
                            enemy, Menu.Item("UseQ").GetValue<bool>(), Menu.Item("UseW").GetValue<bool>(),
                            Menu.Item("UseE").GetValue<bool>(), Menu.Item("UseR").GetValue<bool>());
                        var PercentHPleftAfterCombo = (enemy.Health - combodamage) / enemy.MaxHealth;
                        var PercentHPleft = enemy.Health / enemy.MaxHealth;
                        if (PercentHPleftAfterCombo < 0)
                            PercentHPleftAfterCombo = 0;
                        double comboXPos = hpBarPos.X - 36 + (107 * PercentHPleftAfterCombo);
                        double currentHpxPos = hpBarPos.X - 36 + (107 * PercentHPleft);
                        var barcolor = Color.FromArgb(100, 0, 220, 0);
                        var barcolorline = Color.WhiteSmoke;
                        if (combodamage + Player.GetSpellDamage(enemy, SpellSlot.Q) +
                            Player.GetAutoAttackDamage(enemy) * 2 > enemy.Health)
                        {
                            killText = "Killable by: Full Combo + 1Q + 2AA";
                            if (combodamage >= enemy.Health)
                                killText = "Killable by: Full Combo";
                            barcolor = Color.FromArgb(100, 255, 255, 0);
                            barcolorline = Color.SpringGreen;
                            var linecolor = barcolor;
                            if (
                                GetComboDamage(
                                    enemy, Menu.Item("UseQ").GetValue<bool>(), Menu.Item("UseW").GetValue<bool>(),
                                    Menu.Item("UseE").GetValue<bool>(), false) > enemy.Health)
                            {
                                killText = "Killable by: Q + W + E";
                                barcolor = Color.FromArgb(130, 255, 70, 0);
                                linecolor = Color.FromArgb(150, 255, 0, 0);
                            }
                            if (Menu.Item("Gank").GetValue<bool>())
                            {
                                var pos = Player.Position +
                                              Vector3.Normalize(enemy.Position - Player.Position) * 100;
                                var myPos = Drawing.WorldToScreen(pos);
                                pos = Player.Position + Vector3.Normalize(enemy.Position - Player.Position) * 350;
                                var ePos = Drawing.WorldToScreen(pos);
                                Drawing.DrawLine(myPos.X, myPos.Y, ePos.X, ePos.Y, 1, linecolor);
                            }
                        }
                        var killTextPos = Drawing.WorldToScreen(enemy.Position);
                        var hPleftText = Math.Round(PercentHPleftAfterCombo * 100) + "%";
                        Drawing.DrawLine(
                            (float) comboXPos, hpBarPos.Y, (float) comboXPos, hpBarPos.Y + 5, 1, barcolorline);
                        if (Menu.Item("KillText").GetValue<bool>())
                            Drawing.DrawText(killTextPos[0] - 105, killTextPos[1] + 25, barcolor, killText);
                        if (Menu.Item("KillTextHP").GetValue<bool>())
                            Drawing.DrawText(hpBarPos.X + 98, hpBarPos.Y + 5, barcolor, hPleftText);
                        if (Menu.Item("DrawHPFill").GetValue<bool>())
                        {
                            var diff = currentHpxPos - comboXPos;
                            for (var i = 0; i < diff; i++)
                            {
                                Drawing.DrawLine(
                                    (float) comboXPos + i, hpBarPos.Y + 2, (float) comboXPos + i,
                                    hpBarPos.Y + 10, 1, barcolor);
                            }
                        }
                    }

                    //Draw QE to cursor circle
                    if (Menu.Item("UseQEC").GetValue<KeyBind>().Active && E.IsReady() && Q.IsReady() && menuItem.Active)
                        Render.Circle.DrawCircle(
                            Game.CursorPos, 150f,
                            (enemy.Distance(Game.CursorPos, true) <= 150 * 150) ? Color.Red : menuItem.Color, 3);
                }
            }

            foreach (var spell in SpellList)
            { // Draw Spell Ranges
                menuItem = Menu.Item("Draw" + spell.Slot).GetValue<Circle>();
                if (menuItem.Active)
                    Render.Circle.DrawCircle(Player.Position, spell.Range, menuItem.Color);
            }

            // Dashboard Indicators
            if (Menu.Item("HUD").GetValue<bool>()) { 
                if (Menu.Item("HarassActiveT").GetValue<KeyBind>().Active) Drawing.DrawText(Drawing.Width * 0.90f, Drawing.Height * 0.68f, Color.Yellow, "Auto Harass : On");
                else Drawing.DrawText(Drawing.Width * 0.90f, Drawing.Height * 0.68f, Color.DarkRed, "Auto Harass : Off");

                if (Menu.Item("AutoKST").GetValue<KeyBind>().Active) Drawing.DrawText(Drawing.Width * 0.90f, Drawing.Height * 0.66f, Color.Yellow, "Auto KS : On");
                else Drawing.DrawText(Drawing.Width * 0.90f, Drawing.Height * 0.66f, Color.DarkRed, "Auto KS : Off");
            }
            // Draw QE MAP
            if (Menu.Item("DrawQEMAP").GetValue<bool>()) { 
                var qeTarget = TargetSelector.GetTarget(QE.Range, TargetSelector.DamageType.Magical);
                var sPos = Prediction.GetPrediction(qeTarget, Q.Delay + E.Delay).UnitPosition;
                var tPos = QE.GetPrediction(qeTarget);
                if (tPos != null && Player.Distance(sPos, true) > Math.Pow(E.Range, 2) && (E.IsReady() || Player.Spellbook.GetSpell(SpellSlot.E).CooldownExpires - Game.Time < 2) && Player.Spellbook.GetSpell(SpellSlot.E).Level>0)
                {
                    var color = Color.Red;
                    var orb = Player.Position + Vector3.Normalize(sPos - Player.Position) * E.Range;
                    QE.Delay = Q.Delay + E.Delay + Player.Distance(orb) / E.Speed;
                    if (tPos.Hitchance >= HitChance.Medium)
                        color = Color.Green;
                    if (Player.Spellbook.GetSpell(SpellSlot.Q).ManaCost +
                        Player.Spellbook.GetSpell(SpellSlot.E).ManaCost > Player.Mana)
                        color = Color.DarkBlue;
                    var pos = Player.Position + Vector3.Normalize(tPos.UnitPosition - Player.Position) * 700;
                    Render.Circle.DrawCircle(pos, Q.Width, color);
                    Render.Circle.DrawCircle(tPos.UnitPosition, Q.Width / 2, color);
                    var sp1 = pos + Vector3.Normalize(Player.Position - pos) * 100f;
                    var sp = Drawing.WorldToScreen(sp1);
                    var ep1 = pos + Vector3.Normalize(pos - Player.Position) * 592;
                    var ep = Drawing.WorldToScreen(ep1);
                    Drawing.DrawLine(sp.X, sp.Y, ep.X, ep.Y, 2, color);
                }
                
            }
            if (!Menu.Item("DrawWMAP").GetValue<bool>() || Player.Spellbook.GetSpell(SpellSlot.W).Level <= 0)
                return;
            var color2 = Color.FromArgb(100, 255, 0, 0);
            var wTarget = TargetSelector.GetTarget(W.Range + W.Width, TargetSelector.DamageType.Magical);
            var pos2 = W.GetPrediction(wTarget, true);
            if (pos2.Hitchance >= HitChance.High)
                color2 = Color.FromArgb(100, 50, 150, 255);
            Render.Circle.DrawCircle(pos2.UnitPosition, W.Width, color2);
        }
    }
}
