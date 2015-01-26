#region
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Color = System.Drawing.Color;
#endregion

namespace JeonUtility
{
    class Program
    {
        #region variable declaration
        public static Menu baseMenu;
        public static Obj_AI_Hero Player = ObjectManager.Player;

        public static SpellSlot[] SSpellSlots = { SpellSlot.Summoner1, SpellSlot.Summoner2 };
        public static SpellSlot[] SpellSlots = { SpellSlot.Q, SpellSlot.W, SpellSlot.E, SpellSlot.R };
        public static SpellSlot smiteSlot = SpellSlot.Unknown;
        public static SpellSlot igniteSlot = SpellSlot.Unknown;
        public static Spell smite;
        public static Spell ignite;
        public static Spell healspell = new Spell(SpellSlot.Unknown);
        public static Spell barrierspell = new Spell(SpellSlot.Unknown);
        public static Spell cleansespell = new Spell(SpellSlot.Unknown);
        public static Spell jumpspell;


        public static bool canw2j = false;
        public static bool rdyw2j = false;
        public static bool rdyward = false;
        public static bool text_Isrender = false;
        public static bool textsmite_Isrender = false;

        public static int req_ignitelevel { get { return Jlib.getMenuValue("igniteLv"); } }

        public static float lastTime = 0;

        public static String[] DefSpellstr = { "barrier", "heal", "boost" };

        public static Render.Text text_smite = new Render.Text("AutoSmite!", Player, new Vector2(55, 50), (int)30, ColorBGRA.FromRgba(0xFF0000FF));
        public static Font timefont = new Font(Drawing.Direct3DDevice, new FontDescription { FaceName = "Calibri", Height = 24,  });
        public static Font cnamefont = new Font(Drawing.Direct3DDevice, new FontDescription { FaceName = "Calibri", Height = 32, });
        public static Font minimapfont = new Font(Drawing.Direct3DDevice, new FontDescription { FaceName = "Calibri", Height = 14 });

        public static Render.Text clock = new Render.Text("", new Vector2(Drawing.Width - 100, Drawing.Height * 8 / 100), (int)24, ColorBGRA.FromRgba(0xFFFFFFFF))
        {
            VisibleCondition = c => Jlib.getMenuBool("draw_clock"),
            text = DateTime.Now.ToString("t"),
        };

        public static List<Render.Circle> towerRanges = new List<Render.Circle>();

        public static List<drawgrab> grabs = new List<drawgrab>();
        public class drawgrab
        {
            public Obj_AI_Base Caster = null;
            public float time = 0;
            public Vector3 Start = new Vector3(0, 0, 0);
            public Vector3 End = new Vector3(0, 0, 0);
            public float width = 0f;
            public Color color = Color.White;
        }

        public static List<shacoDeceive> shacoQ = new List<shacoDeceive>();
        public class shacoDeceive
        {
            public Obj_AI_Base Caster = null;
            public float time = 0;
            public Vector3 End = new Vector3(0, 0, 0);

            public Render.Text text { get; set; }

            public shacoDeceive()
            {
                text = new Render.Text("", new Vector2(0, 0), 32
                    , SharpDX.Color.Red)
                {
                    VisibleCondition =
                    condition => Jlib.getMenuBool("tracker_shacoQ"),
                    TextUpdate = () => "Shaco on here!",
                    PositionUpdate = delegate
                    {
                        Vector2 vec2 = Drawing.WorldToScreen(new Vector3(End.X, End.Y + 25, End.Z));
                        return vec2;
                    },
                    OutLined = true,
                    Centered = true
                };
                text.Add();
            }
        }

        public static List<timer_clock> timerlist = new List<timer_clock>();
        public class timer_clock
        {
            public Vector3 Position;
            public string name;
            public int respawntime = 100;
            public int spawntime = 115;
            public int Range = 1000;
            public Render.Text timer { get; set; }
            public Render.Text timer_minimap { get; set; }

            public timer_clock()
            {

                timerlist.Add(this);

                return;
                timer = new Render.Text(Position.To2D(), "", baseMenu.Item("jt_font_size").GetValue<Slider>().Value
                    , SharpDX.Color.White)
                {
                    VisibleCondition =
                    condition => baseMenu.Item("jt_active").GetValue<bool>() && spawntime > 0 && !CheckMonster(name,Position,Range),

                    PositionUpdate = delegate
                    {
                        Vector2 vec2 = Drawing.WorldToScreen(Position);
                        return vec2;
                    },
                    TextUpdate = () => Clockstring(this),
                    OutLined = true,
                    Centered = true
                };

                timer_minimap = new Render.Text(Position.To2D(), "",
                    baseMenu.Item("jt_font_size_minimap").GetValue<Slider>().Value
                    , SharpDX.Color.White)
                {
                    VisibleCondition =
                    condition => baseMenu.Item("jt_active_minimap").GetValue<bool>()&& spawntime > 0 && !CheckMonster(name,Position,Range),

                    PositionUpdate = delegate
                    {
                        Vector2 v2 = Drawing.WorldToMinimap(Position);
                        return v2;
                    },
                    TextUpdate = () => Clockstring(this),
                    OutLined = true,
                    Centered = true
                };

                timer.Add();
                timer_minimap.Add();
            }
        }

        public static List<Fake> fakeList = new List<Fake>();
        public class Fake
        {
            public int id;
            public Vector3 position;
            public float time = 0;
            public Obj_AI_Minion target;

            public Render.Text text_fake { get; set; }

            public Fake()
            {
                text_fake = new Render.Text("", new Vector2(0, 0), 32
                    , SharpDX.Color.Red)
                {
                    VisibleCondition =
                    condition => Jlib.getMenuBool("tracker_fake") &&
                          !target.IsDead,
                    TextUpdate = () => "Fake!",
                    OutLined = true,
                    Centered = true
                };
                text_fake.Add();
            }
        }


        public static List<Waypoints> hero_waypoint = new List<Waypoints>();
        public class Waypoints
        {
            public Obj_AI_Hero hero;
            public List<Vector2> points;
            public float time = 0.0f;
            public int ptime;
            public Render.Sprite icon;
        }


        #region wardtracker
        public static List<Ward> wardlist = new List<Ward>();
        public static string[] wardnames = { "SightWard", "VisionWard", "Jack In The Box", "Cupcake Trap", "Noxious Trap" };
        public enum wardtype
        {
            Pink,
            Green,
            Mushroom,
            ShacoBox,
            Trap,
            Unknown
        }

        public class Ward
        {
            public wardtype type;
            public int id;
            public float time;
            public Vector3 position;
            public bool show = true;
            public float endtiem;
            public Obj_AI_Base target;

            public Render.Text timer { get; set; }

            public Ward()
            {
                timer = new Render.Text("", new Vector2(0, 0), 32
                    , SharpDX.Color.White)
                {
                    VisibleCondition =
                    condition =>
                          (int)(endtiem - Game.Time) > 0 && show &&
                          !target.IsDead && Jlib.getMenuBool("tracker_ward"),
                    PositionUpdate = delegate
                    {
                        Vector2 vec2 = Drawing.WorldToScreen(new Vector3(position.X, position.Y + 25, position.Z));
                        return vec2;
                    },
                    TextUpdate = () => Convert.ToString((int)(endtiem - Game.Time)),
                    OutLined = true,
                    Centered = true
                };
                timer.Add();
            }
        }
        #endregion wardtracker

        #endregion

        
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        private static void OnGameLoad(EventArgs args)
        {
            
            #region Initialize
            string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Game.PrintChat("<font color ='#33FFFF'>JeonUtility v" + version + " </font>Loaded! ({0}x{1})", Drawing.Width, Drawing.Height);
            setSmiteSlot();
            setIgniteSlot();
            setdefSpell();
            clock.Add();
            #endregion

            #region 메뉴 - Menu

            #region 메인메뉴 - Main Menu
            baseMenu = new Menu("【無為汉化】实用功能合集", "JeonUtility", true);
            baseMenu.AddToMainMenu();
            baseMenu.AddItem(new MenuItem("base_stat", "功能生效范围").SetValue(true));
            baseMenu.AddItem(new MenuItem("x%", "坐标_X(%)").SetValue(new Slider(75, 0, 100)));
            baseMenu.AddItem(new MenuItem("y%", "坐标_Y(%)").SetValue(new Slider(55, 0, 100)));

            var menu_smite = new Menu("惩戒", "Smite");
            var menu_ignite = new Menu("点燃", "Ignite");
            var menu_tracker = new Menu("追踪器(CD)", "Tracker");
            var menu_j2w = new Menu("瞬眼", "Jump2Ward");
            var menu_st = new Menu("堆叠伤害", "Stacks");
            var menu_ins = new Menu("物品&大招提醒", "Item & Spell");
            var menu_noti = new Menu("通知器", "Notifier");
            var menu_jtimer = new Menu("打野计时", "JungleTimer");
            var menu_draw = new Menu("范围显示", "Draw");
            #endregion

            #region 스마이트 메뉴 - menu for smite
            baseMenu.AddSubMenu(menu_smite);
            menu_smite.AddItem(new MenuItem("AutoSmite", "自动惩戒").SetValue(true));
            menu_smite.AddItem(new MenuItem("smite_enablekey", "启用按键:").SetValue(new KeyBind('K', KeyBindType.Toggle)));// 32 - Space
            menu_smite.AddItem(new MenuItem("smite_holdkey", "保持键位:").SetValue(new KeyBind(32, KeyBindType.Press)));// 32 - Space
            #endregion

            #region 점화 메뉴 - menu for ignite
            baseMenu.AddSubMenu(menu_ignite);
            menu_ignite.AddItem(new MenuItem("AutoIgnite", "自动点燃").SetValue(true));
            menu_ignite.AddItem(new MenuItem("igniteLv", "启用等级").SetValue(new Slider(1, 1, 18)));
            #endregion

            #region 트래커 메뉴 - menu for tracker
            baseMenu.AddSubMenu(menu_tracker);
            menu_tracker.AddItem(new MenuItem("tracker_enemyspells_on", "显示敌人技能CD").SetValue(true));
            menu_tracker.AddItem(new MenuItem("tracker_enemyspells", "显示版本").SetValue(new StringList(new[] { "新版", "老版" })));
            menu_tracker.AddItem(new MenuItem("tracker_ward", "显示眼位").SetValue(true));
            menu_tracker.AddItem(new MenuItem("tracker_waypoints", "航点").SetValue(true));
            menu_tracker.AddItem(new MenuItem("tracker_lastposition", "最后的位置").SetValue(true));
            menu_tracker.AddItem(new MenuItem("tracker_fake", "假眼(己方)").SetValue(true));
            menu_tracker.AddItem(new MenuItem("tracker_shacoQ", "小丑盒子").SetValue(true));
            #endregion

            #region 점프와드 메뉴 - menu for Jump2Ward
            baseMenu.AddSubMenu(menu_j2w);
            menu_j2w.AddItem(new MenuItem("j2w_bool", "顺眼").SetValue(true));
            menu_j2w.AddItem(new MenuItem("j2w_hkey", "按键 : ").SetValue(new KeyBind('T', KeyBindType.Press)));
            menu_j2w.AddItem(new MenuItem("j2w_info", "信息").SetValue(false));
            #endregion

            #region 스택 메뉴 - menu for stacks
            baseMenu.AddSubMenu(menu_st);
            menu_st.AddItem(new MenuItem("st_bool", "显示法术伤害").SetValue(true));
            menu_st.AddItem(new MenuItem("st_twitch", "自动老鼠(E)").SetValue(false));
            menu_st.AddItem(new MenuItem("st_kalista", "自动复仇之矛(E)").SetValue(false));
            #endregion

            #region 아이템사용 메뉴 - menu for UseItem&Spell
            baseMenu.AddSubMenu(menu_ins);

            menu_ins.AddItem(new MenuItem("useitem", "启用").SetValue(true));

            var menu_Potion = new Menu("药瓶", "Potion");
            menu_ins.AddSubMenu(menu_Potion);
            menu_Potion.AddItem(new MenuItem("useitem_flask", "使用水晶瓶").SetValue(true));
            menu_Potion.AddItem(new MenuItem("useitem_p_fla", "最低血量(%)").SetValue(new Slider(50, 0, 100)));
            menu_Potion.AddItem(new MenuItem("useitem_hppotion", "使用红药").SetValue(true));
            menu_Potion.AddItem(new MenuItem("useitem_p_hp", "最低血量(%)").SetValue(new Slider(50, 0, 100)));
            menu_Potion.AddItem(new MenuItem("useitem_manapotion", "使用蓝药").SetValue(true));
            menu_Potion.AddItem(new MenuItem("useitem_p_mana", "最低蓝量(%)").SetValue(new Slider(50, 0, 100)));

            var menu_items = new Menu("物品", "Items");
            menu_ins.AddSubMenu(menu_items);
            var item_zhonya = new Menu("中亚", "Zhonya");
            menu_items.AddSubMenu(item_zhonya);
            item_zhonya.AddItem(new MenuItem("useitem_zhonya", "使用中亚").SetValue(true));
            item_zhonya.AddItem(new MenuItem("useitem_p_zhonya", "最低血量(%)").SetValue(new Slider(15, 0, 100)));

            var item_seraph = new Menu("Seraph", "Seraph");
            menu_items.AddSubMenu(item_seraph);
            item_seraph.AddItem(new MenuItem("useitem_seraph", "使用Seraph").SetValue(true));
            item_seraph.AddItem(new MenuItem("useitem_p_seraph", "血量(%)").SetValue(new Slider(20, 0, 100)));

            var item_Bilgewater = new Menu("比尔吉沃特弯刀", "Bilgewater");
            menu_items.AddSubMenu(item_Bilgewater);
            item_Bilgewater.AddItem(new MenuItem("useitem_bilgewater", "使用弯刀").SetValue(true));
            item_Bilgewater.AddItem(new MenuItem("useitem_p_bilgewater", "血量(%)").SetValue(new Slider(20, 0, 100)));
            item_Bilgewater.AddItem(new MenuItem("useitem_bilgewater_atg", "反突进").SetValue(true));
            item_Bilgewater.AddItem(new MenuItem("useitem_bilgewater_atg_p", "突进距离 :").SetValue(new Slider(250, 100, 450)));

            var item_botrk = new Menu("破败", "BOTRK");
            menu_items.AddSubMenu(item_botrk);
            item_botrk.AddItem(new MenuItem("useitem_botrk", "使用破败").SetValue(true));
            item_botrk.AddItem(new MenuItem("useitem_p_botrk", "使用破败 Hp(%)").SetValue(new Slider(20, 0, 100)));
            item_botrk.AddItem(new MenuItem("useitem_botrk_atg", "防止突进").SetValue(true));
            item_botrk.AddItem(new MenuItem("useitem_botrk_atg_p", "突进者距离 :").SetValue(new Slider(150, 100, 450)));


            var item_mikaels = new Menu("坩埚", "Mikaels");
            menu_items.AddSubMenu(item_mikaels);
            item_mikaels.AddItem(new MenuItem("useitem_mikaels", "不用？").SetValue(true));
            item_mikaels.AddItem(new MenuItem("useitem_p_mikaels", "最低血量(%)").SetValue(new Slider(15, 0, 100)));
            item_mikaels.AddItem(new MenuItem("useitem_p_mikaels_delay", "延迟(ms)").SetValue(new Slider(100, 0, 1000)));

            #region mikaels_cc
            var menu_mikaels_cc = new Menu("坩埚_净化", "Use On CC");
            item_mikaels.AddSubMenu(menu_mikaels_cc);
            menu_mikaels_cc.AddItem(new MenuItem("mikaels_cc_bool", "启用坩埚净化").SetValue(true));
            menu_mikaels_cc.AddItem(new MenuItem("mikaels_cc_stun", "眩晕").SetValue(true));
            menu_mikaels_cc.AddItem(new MenuItem("mikaels_cc_fear", "减速").SetValue(true));
            menu_mikaels_cc.AddItem(new MenuItem("mikaels_cc_charm", "魅惑").SetValue(true));
            menu_mikaels_cc.AddItem(new MenuItem("mikaels_cc_taunt", "嘲讽").SetValue(true));
            menu_mikaels_cc.AddItem(new MenuItem("mikaels_cc_snare", "陷阱").SetValue(true));
            menu_mikaels_cc.AddItem(new MenuItem("mikaels_cc_silence", "沉默").SetValue(false));
            menu_mikaels_cc.AddItem(new MenuItem("mikaels_cc_polymorph", "变形").SetValue(true));
            #endregion


            var item_qs = new Menu("水银弯刀", "QuickSilver");
            menu_items.AddSubMenu(item_qs);
            item_qs.AddItem(new MenuItem("useitem_qs_bool", "启用水银弯刀").SetValue(true));
            item_qs.AddItem(new MenuItem("useitem_p_qs_delay", "延迟(ms)").SetValue(new Slider(100, 0, 1000)));
            #region qs_cc
            var menu_quicksilver_cc = new Menu("水银弯刀_净化：", "Use On CC");
            item_qs.AddSubMenu(menu_quicksilver_cc);
            menu_quicksilver_cc.AddItem(new MenuItem("qs_cc_stun", "眩晕").SetValue(true));
            menu_quicksilver_cc.AddItem(new MenuItem("qs_cc_fear", "减速").SetValue(true));
            menu_quicksilver_cc.AddItem(new MenuItem("qs_cc_charm", "魅惑").SetValue(true));
            menu_quicksilver_cc.AddItem(new MenuItem("qs_cc_taunt", "嘲讽").SetValue(true));
            menu_quicksilver_cc.AddItem(new MenuItem("qs_cc_snare", "陷阱").SetValue(true));
            menu_quicksilver_cc.AddItem(new MenuItem("qs_cc_silence", "沉默").SetValue(false));
            menu_quicksilver_cc.AddItem(new MenuItem("qs_cc_polymorph", "变形").SetValue(true));
            menu_quicksilver_cc.AddItem(new MenuItem("qs_cc_suppression", "虚弱").SetValue(true));
            menu_quicksilver_cc.AddItem(new MenuItem("qs_cc_zedutl", "劫 大招").SetValue(true));
            menu_quicksilver_cc.AddItem(new MenuItem("qs_cc_fizzult", "小鱼人 大招").SetValue(true));
            #endregion

            var menu_spell = new Menu("法术", "Spell");
            menu_ins.AddSubMenu(menu_spell);
            var menu_hb = new Menu("治疗&屏障", "Heal&Barrier");
            menu_spell.AddSubMenu(menu_hb);
            menu_hb.AddItem(new MenuItem("spell_hb", "施放法术").SetValue(true));
            menu_hb.AddItem(new MenuItem("spell_hb_hp", "血量(%)").SetValue(new Slider(10, 0, 100)));

            var menu_cleanse = new Menu("净化", "Cleanse");
            menu_spell.AddSubMenu(menu_cleanse);
            menu_cleanse.AddItem(new MenuItem("spell_cleanse", "施放法术").SetValue(true));
            menu_cleanse.AddItem(new MenuItem("spell_cleanse_delay", "延时(ms)").SetValue(new Slider(50, 0, 1000)));
           



            #endregion

            #region 알림 메뉴 - menu for notifier
            //baseMenu.AddSubMenu(menu_noti);
            //menu_noti.AddItem(new MenuItem("noti_karthus", "KarthusUlt").SetValue(true));
            //menu_noti.AddItem(new MenuItem("noti_ez", "EzrealUlt").SetValue(true));
            //menu_noti.AddItem(new MenuItem("noti_cait", "CaitUlt").SetValue(true));
            //menu_noti.AddItem(new MenuItem("noti_shen", "ShenUlt").SetValue(true));
            //menu_noti.AddItem(new MenuItem("noti_shenhp", "Notice On Hp(%)").SetValue(new Slider(10, 0, 100)));
            #endregion

            #region 정글타이머 메뉴 - menu for JungleTimer
            baseMenu.AddSubMenu(menu_jtimer);
            menu_jtimer.AddItem(new MenuItem("jt_active", "启用打野计时").SetValue(true));
            menu_jtimer.AddItem(new MenuItem("jt_active_minimap", "在小地图显示").SetValue(false));
            #endregion

            #region 드로잉 메뉴 - menu for Draw
            baseMenu.AddSubMenu(menu_draw);
            menu_draw.AddItem(new MenuItem("draw_turret", "显示炮塔范围").SetValue(true));
            menu_draw.AddItem(new MenuItem("draw_grab", "抓住").SetValue(true));
            menu_draw.AddItem(new MenuItem("draw_globalult", "全局资讯").SetValue(true));
            menu_draw.AddItem(new MenuItem("draw_clock", "时间").SetValue(true));

            #endregion

            #endregion

            #region Events Initialize
            Game.OnGameUpdate += OnGameUpdate;
            Obj_AI_Hero.OnProcessSpellCast += OnSpell;
            GameObject.OnDelete += OnDelete;
            GameObject.OnCreate += OnCreate;
            Drawing.OnEndScene += OnDraw_EndScene;
            //Drawing.OnDraw += OnDraw;
            #endregion

            #region 타워거리 - tower attack range
            foreach (var t in ObjectManager.Get<Obj_AI_Turret>().Where(t => !t.IsDead && t.IsEnemy &&
                (t.Name.StartsWith("Turret_T1") || t.Name.StartsWith("Turret_T2"))))
            {
                towerRanges.Add(new Render.Circle(t, 875, Color.Blue, 5)
                {
                    VisibleCondition = c => Jlib.getMenuBool("draw_turret")
                });
                towerRanges.Last().Add();
            }
            #endregion

            #region waypoint

            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy && hero.IsValid))
            {
                if (!hero_waypoint.Any(t => t.hero.ChampionName == hero.ChampionName))
                {
                    string strTempFilePath = Path.GetTempFileName().Replace("tmp", "") + "png";

                    WebClient wc = new WebClient();
                    wc.DownloadFile("http://ss.op.gg/images/lol/champion/" + hero.ChampionName + ".png", strTempFilePath);
                    wc.Dispose();

                    var bit = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromFile(strTempFilePath);
                    bit = JImage.MakeGrayscale(bit);
                    bit = JImage.CropToCircle(bit);

                    var buffer = new Waypoints()
                    {
                        hero = hero,
                        points = hero.GetWaypoints(),
                        icon = new Render.Sprite(bit, Vector2.Zero)
                        {
                            PositionUpdate = () => Drawing.WorldToMinimap(hero.Position) - new Vector2(11, 11),
                            VisibleCondition = Condition => Jlib.getMenuBool("tracker_lastposition") && !hero.IsVisible && !hero.IsDead,
                            Scale = new Vector2(0.180f, 0.180f)
                        }
                    };
                    buffer.icon.Add();
                    hero_waypoint.Add(buffer);
                }
            }
            #endregion

            #region 타이머 - Timer
            timer_clock Baron = new timer_clock
            {
                Position = new Vector3(4910f, 10268f, -71.24f),
                name = "SRU_Baron",
                respawntime = 420,
                spawntime = 1200
            };
            timer_clock Dragon = new timer_clock
            {
                Position = new Vector3(9836f, 4408f, -71.24f),
                name = "SRU_Dragon",
                respawntime = 360,
                spawntime = 150
            };
            timer_clock top_crab = new timer_clock
            {
                Position = new Vector3(4266f, 9634f, -67.87f),
                name = "Sru_Crab",
                respawntime = 180,
                spawntime = 115,
                Range = 3000
            };
            timer_clock down_crab = new timer_clock
            {
                Position = new Vector3(10524f, 5116f, -62.81f),
                name = "Sru_Crab",
                respawntime = 180,
                Range = 3000
            };

            timer_clock bteam_Razorbeak = new timer_clock { Position = new Vector3(6974f, 5460f, 54f), name = "SRU_Razorbeak" };
            timer_clock bteam_Red = new timer_clock
            {
                Position = new Vector3(7796f, 4028f, 54f),
                name = "SRU_Red",
                respawntime = 300
            };
            timer_clock bteam_Krug = new timer_clock { Position = new Vector3(8394f, 2750f, 50f), name = "SRU_Krug" };
            timer_clock bteam_Blue = new timer_clock
            {
                Position = new Vector3(3832f, 7996f, 52f),
                name = "SRU_Blue",
                respawntime = 300
            };
            timer_clock bteam_Gromp = new timer_clock { Position = new Vector3(2112f, 8372f, 51.7f), name = "SRU_Gromp" };
            timer_clock bteam_Wolf = new timer_clock { Position = new Vector3(3844f, 6474f, 52.46f), name = "SRU_Murkwolf" };

            timer_clock pteam_Razorbeak = new timer_clock { Position = new Vector3(7856f, 9492f, 52.33f), name = "SRU_Razorbeak" };
            timer_clock pteam_Red = new timer_clock
            {
                Position = new Vector3(7124f, 10856f, 56.34f),
                name = "SRU_Red",
                respawntime = 300
            };
            timer_clock pteam_Krug = new timer_clock { Position = new Vector3(6495f, 12227f, 56.47f), name = "SRU_Krug" };
            timer_clock pteam_Blue = new timer_clock
            {
                Position = new Vector3(10850f, 6938f, 51.72f),
                name = "SRU_Blue",
                respawntime = 300
            };
            timer_clock pteam_Gromp = new timer_clock { Position = new Vector3(12766f, 6464f, 51.66f), name = "SRU_Gromp" };
            timer_clock pteam_Wolf = new timer_clock { Position = new Vector3(10958f, 8286f, 62.46f), name = "SRU_Murkwolf" };

            #endregion


        }



        private static void OnDraw_EndScene(EventArgs args)
        {

            if (Jlib.getMenuBool("jt_active_minimap") || Jlib.getMenuBool("jt_active"))
            {
                foreach (var timer in timerlist.Where(t => t.spawntime > 0 && !CheckMonster(t.name, t.Position, t.Range)))
                {
                    if(Jlib.getMenuBool("jt_active"))
                        Jlib.Drawtext_outlined(timefont, Clockstring(timer),
                            (int)Drawing.WorldToScreen(timer.Position).X, (int)Drawing.WorldToScreen(timer.Position).Y,
                            SharpDX.Color.White);

                    if (Jlib.getMenuBool("jt_active_minimap"))
                        Jlib.Drawtext_outlined(minimapfont, Clockstring(timer),
                            (int)Drawing.WorldToMinimap(timer.Position).X - 11, (int)Drawing.WorldToMinimap(timer.Position).Y - 8,
                            SharpDX.Color.White);
                }
            }

            #region Spells
            foreach (var grab in grabs.Where(t => t.Start != Vector3.Zero))
            {
                if (grab.Caster.BaseSkinName == "Lux" && grab.width == 300)
                {
                    Render.Circle.DrawCircle(grab.End, grab.width, Color.White);

                    if (Game.Time > grab.time)
                    {
                        grabs.Remove(grab);
                        break;
                    }

                    return;
                }

                var start = grab.Start;
                var end = grab.End;
                var distance = start.Distance(end);
                var sin = (start.X - end.X) / distance;
                var cos = (start.Y - end.Y) / distance;
                Vector3 line1_start, line2_start, line1_end, line2_end;


                float endX = start.X - grab.width * (float)cos;
                float endY = start.Y + grab.width * (float)sin;
                line1_start = new Vector3(endX, endY, start.Z);

                endX = start.X + grab.width * (float)cos;
                endY = start.Y - grab.width * (float)sin;
                line2_start = new Vector3(endX, endY, start.Z);

                endX = end.X - grab.width * (float)cos;
                endY = end.Y + grab.width * (float)sin;
                line1_end = new Vector3(endX, endY, end.Z);

                endX = end.X + grab.width * (float)cos;
                endY = end.Y - grab.width * (float)sin;
                line2_end = new Vector3(endX, endY, end.Z);



                Drawing.DrawLine(Drawing.WorldToScreen(grab.Start),
                    Drawing.WorldToScreen(grab.End), 1, grab.color);

                Drawing.DrawLine(Drawing.WorldToScreen(line1_start),
                    Drawing.WorldToScreen(line1_end), 1, grab.color);

                Drawing.DrawLine(Drawing.WorldToScreen(line2_start),
                    Drawing.WorldToScreen(line2_end), 1, grab.color);

                Render.Circle.DrawCircle(grab.Start, grab.width, grab.color);
                Render.Circle.DrawCircle(grab.End, grab.width, grab.color);

                if (Game.Time > grab.time)
                {
                    grabs.Remove(grab);
                    break;
                }
            }


            if (Jlib.getMenuBool("draw_globalult"))
            {
                foreach (var t in ObjectManager.Get<Obj_SpellMissile>().Where(t => t.SData.Name == "EzrealTrueshotBarrage" || t.SData.Name == "JinxR"
                    || t.SData.Name == "EnchantedCrystalArrow" || t.SData.Name == "DravenR"))
                {
                    if (t.Position.X > 0 && t.Position.X < 20000)
                    {
                        Drawing.DrawLine(
                            Drawing.WorldToScreen(t.Position.To2D().Extend(t.EndPosition.To2D(), 20000).To3D()),
                            Drawing.WorldToScreen(t.Position.To2D().Extend(t.EndPosition.To2D(), -20000).To3D()), 5, Color.Red);
                        Jlib.DrawCircleOnMinimap(t.Position, 60, Color.Red, 5, 5);
                    }
                }
            }
            #endregion

            #region ShacoQ
            if (shacoQ.Any())
            {
                foreach (var sQ in shacoQ.Where(t => t.End != Vector3.Zero))
                {
                    Render.Circle.DrawCircle(sQ.End, 50, Color.Red);

                    if (Game.Time > sQ.time)
                    {
                        sQ.text.Remove();
                        shacoQ.Remove(sQ);
                        break;
                    }
                }
            }
            #endregion

            #region wardtracker
            foreach (var ward in wardlist.Where(t => Jlib.getMenuBool("tracker_ward")))
            {
                if (ward.endtiem <= Game.Time)
                {
                    ward.timer.Remove();
                    wardlist.Remove(ward);
                    break;
                }


                var ratio = (int)(ward.endtiem - Game.Time) / ward.time;
                var bar_start = new Vector2(Drawing.WorldToScreen(ward.position).X - 20, Drawing.WorldToScreen(ward.position).Y);
                var bar_end = new Vector2(bar_start.X + (40 * ratio), bar_start.Y);
                var bar_out_start = new Vector2(bar_start.X - 1, bar_end.Y - 1);
                var bar_out_end = new Vector2(bar_start.X + 42, bar_start.Y - 1);


                Color color = Color.Pink;

                switch (ward.type)
                {
                    case wardtype.Green:
                        color = Color.Green;
                        break;
                    case wardtype.Pink:
                        color = Color.Pink;
                        break;
                    case wardtype.ShacoBox:
                        color = Color.Red;
                        break;
                    case wardtype.Mushroom:
                        color = Color.Purple;
                        break;
                    case wardtype.Trap:
                        color = Color.White;
                        break;
                }



                if (!ward.target.IsDead || ward.type == wardtype.Unknown)
                {
                    Render.Circle.DrawCircle(ward.position, 60, color, 5);
                    Jlib.DrawCircleOnMinimap(ward.position, 60, color, 5,5);
                    if (ward.type != wardtype.Pink)
                    {
                        Drawing.DrawLine(bar_out_start, bar_out_end, 5, Color.Black);
                        Drawing.DrawLine(bar_start, bar_end, 3, Color.White);
                    }
                }
                else
                {
                    ward.timer.Remove();
                    wardlist.Remove(ward);
                    break;
                }

            }
            #endregion wardtracker

            #region waypoint
            if (Jlib.getMenuBool("tracker_waypoints"))
            {
                foreach (var t in hero_waypoint.Where(t => !t.hero.IsDead))
                {

                    if (t.hero.GetWaypoints().Any())
                        t.points = t.hero.GetWaypoints();

                    for (int i = 0; i < t.points.Count; i++)
                    {
                        if (i < t.points.Count - 1)
                        {
                            if (t.hero.IsVisible)
                            {
                                t.time = 0.0f;
                                t.time += t.points[i].Distance(t.points[i + 1]) / t.hero.MoveSpeed;
                                Drawing.DrawLine(Drawing.WorldToScreen(t.points[i].To3D()), Drawing.WorldToScreen(t.points[i + 1].To3D()), 2, Color.Red);
                            }
                            else
                            {
                                Drawing.DrawLine(Drawing.WorldToScreen(t.points[i].To3D()), Drawing.WorldToScreen(t.points[i + 1].To3D()), 2, Color.Red);
                                Drawing.DrawLine(Drawing.WorldToMinimap(t.points[i].To3D()), Drawing.WorldToMinimap(t.points[i + 1].To3D()), 1, Color.Red);
                            }
                        }
                    }



                    if (t.time > 0.00f && t.points.Any())
                    {
                        if (t.hero.IsVisible)
                        {
                            Render.Circle.DrawCircle(t.points.Last().To3D(), 75, Color.White);
                            var _v = Drawing.WorldToScreen(t.points.Last().To3D());
                            Jlib.Drawtext_outlined(cnamefont, t.hero.ChampionName, (int)_v.X - 25, (int)_v.Y - 15, SharpDX.Color.Gold);
                            Jlib.Drawtext_outlined(timefont, t.time.ToString("0.0"), (int)_v.X - 10, (int)_v.Y + 10, SharpDX.Color.Red);
                        }
                        else
                        {
                            Render.Circle.DrawCircle(t.points.Last().To3D(), 75, Color.White);
                            var _v = Drawing.WorldToScreen(t.points.Last().To3D());
                            Jlib.Drawtext_outlined(cnamefont, t.hero.ChampionName, (int)_v.X - 25, (int)_v.Y - 15, SharpDX.Color.Gold);
                            Jlib.Drawtext_outlined(timefont, t.time.ToString("0.0"), (int)_v.X - 10, (int)_v.Y + 10, SharpDX.Color.Red);
                            if (Environment.TickCount - t.ptime >= 100 && !t.hero.IsVisible)
                            {
                                if (t.time >= 0.1f)
                                    t.time -= 0.1f;
                                else
                                    t.time = 0.01f;
                                t.ptime = Environment.TickCount;
                            }
                        }
                    }

                }
            }
            #endregion
        }


        private static void OnSpell(Obj_AI_Base Caster, GameObjectProcessSpellCastEventArgs args)
        {
            #region Catch hard CC Spell
            if (baseMenu.Item("draw_grab").GetValue<bool>())
            {
                if (args.SData.Name == "ThreshQ")
                {
                    grabs.Add(new drawgrab()
                    {
                        Caster = Caster,
                        Start = args.Start,
                        End = args.Start.Extend(args.End, 1100),
                        time = Game.Time + 1f,
                        width = 70
                    });
                }
                if (args.SData.Name == "RocketGrab")
                {
                    grabs.Add(new drawgrab()
                    {
                        Caster = Caster,
                        Start = args.Start,
                        End = args.Start.Extend(args.End, 1100),
                        time = Game.Time + 1f,
                        width = 70
                    });
                }
                if (args.SData.Name == "DarkBindingMissile")
                {
                    grabs.Add(new drawgrab()
                    {
                        Caster = Caster,
                        Start = args.Start,
                        End = args.Start.Extend(args.End, 1300),
                        time = Game.Time + 1300f / 1200f,
                        width = 70
                    });
                }
                if (args.SData.Name == "BandageToss")
                {
                    grabs.Add(new drawgrab()
                    {
                        Caster = Caster,
                        Start = args.Start,
                        End = args.Start.Extend(args.End, 1100),
                        time = Game.Time + 1100f / 1800f,
                        width = 80
                    });
                }
                if (args.SData.Name == "JavelinToss")
                {
                    grabs.Add(new drawgrab()
                    {
                        Caster = Caster,
                        Start = args.Start,
                        End = args.Start.Extend(args.End, 1500),
                        time = Game.Time + 1500f / 1300f,
                        width = 40
                    });
                }
                if (args.SData.Name == "LuxLightBinding")
                {
                    grabs.Add(new drawgrab()
                    {
                        Caster = Caster,
                        Start = args.Start,
                        End = args.Start.Extend(args.End, 1175),
                        time = Game.Time + 1200f / 1175f,
                        width = 80
                    });
                }
                if (args.SData.Name == "LuxLightStrikeKugel")
                {
                    grabs.Add(new drawgrab()
                    {
                        Caster = Caster,
                        Start = args.Start,
                        End = args.End,
                        time = Game.Time + 1200f / 1175f,
                        width = 300
                    });
                }
            }

            #endregion

            #region Catch ShacoQ
            if (Caster.IsEnemy && (Caster.BaseSkinName == "Shaco" && args.SData.Name == "Deceive"))
            {
                var pos = args.End;
                if (args.Start.Distance(args.End) >= 400)
                    pos = args.Start.Extend(args.End, 400);

                shacoQ.Add(new shacoDeceive()
                {
                    Caster = Caster,
                    End = pos,
                    time = Game.Time + 3.0f
                });
            }
            #endregion
        }
        private static void OnGameUpdate(EventArgs args)
        {


            #region 정글타이머 - JungleTimer

            if (Game.Time - lastTime >= 1 && timerlist.Any(t => t.spawntime >0))
            {
                lastTime = Game.Time;
                foreach (var t in timerlist)
                {
                    if (CheckMonster(t.name, t.Position, t.Range))
                        t.spawntime = t.respawntime;
                    else
                        t.spawntime -= 1;
                }
            }
            #endregion



            #region get info
            float Player_baseAD = Player.BaseAttackDamage;
            float Player_addAD = Player.FlatPhysicalDamageMod;
            float Player_totalAD = Player_baseAD + Player_addAD;
            float Player_totalAP = Player.FlatMagicDamageMod;
            #endregion

            #region 오토스마이트-AutoSmite
            if (Jlib.getMenuBool("AutoSmite") && smiteSlot != SpellSlot.Unknown)
            {
                if ((baseMenu.Item("smite_holdkey").GetValue<KeyBind>().Active || baseMenu.Item("smite_enablekey").GetValue<KeyBind>().Active))
                {
                    double smitedamage;
                    bool smiteReady = false;
                    smitedamage = setSmiteDamage();

                    Drawing.DrawText(Player.HPBarPosition.X + 55, Player.HPBarPosition.Y + 25, System.Drawing.Color.Red, "AutoSmite!");

                    Obj_AI_Base mob = GetNearest(Player.ServerPosition);

                    if (mob != null)
                    {
                        if (Player.Spellbook.CanUseSpell(smiteSlot) == SpellState.Ready && Vector3.Distance(Player.ServerPosition, mob.ServerPosition) < smite.Range)
                        {
                            smiteReady = true;
                        }

                        if (smiteReady && mob.Health < smitedamage)
                        {
                            setSmiteSlot();
                            Player.Spellbook.CastSpell(smiteSlot, mob);
                        }
                    }
                }
            }
            #endregion

            #region 오토이그나이트-AutoIgnite
            if (Jlib.getMenuBool("AutoIgnite") && igniteSlot != SpellSlot.Unknown &&
                Player.Level >= req_ignitelevel)
            {
                float ignitedamage = setigniteDamage();
                foreach (var hero in ObjectManager.Get<Obj_AI_Hero>()
                    .Where(hero => hero != null && hero.IsValid && !hero.IsDead && Player.ServerPosition.Distance(hero.ServerPosition) < ignite.Range
                        && !hero.IsMe && !hero.IsAlly))
                {
                    var Q = new Spell(SpellSlot.Q, 700);

                    if (hero.Buffs.Any(c => c.Name == "timebombenemybuff") && Player.ChampionName == "Zilean")
                    {
                        if (Q.GetDamage(hero) - hero.HPRegenRate * 2 >= hero.Health)
                            return;
                        ignitedamage += (Q.GetDamage(hero) - hero.HPRegenRate * 2);
                    }

                    if (Player.Spellbook.CanUseSpell(igniteSlot) == SpellState.Ready && (hero.Health + hero.HPRegenRate * 2) <= ignitedamage)
                    {
                        setIgniteSlot();
                        Player.Spellbook.CastSpell(igniteSlot, hero);
                    }
                }
            }

            #endregion

            #region 스펠트레커-Spelltracker
            if (Jlib.getMenuBool("tracker_enemyspells_on"))
            {
                foreach (var target in
                    ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero != null && !hero.IsMe && hero.IsValid && (hero.IsHPBarRendered)))
                {

                    int X = 10;
                    int Y = 40;
                    foreach (var sSlot in SSpellSlots)
                    {

                        var spell = target.Spellbook.GetSpell(sSlot);
                        var t = spell.CooldownExpires - Game.Time;
                        if (Jlib.getMenuStringValue("tracker_enemyspells") == "New")
                        {
                            if (t < 0)
                            {
                                Drawing.DrawText(target.HPBarPosition.X + X, target.HPBarPosition.Y + Y + 15, System.Drawing.Color.FromArgb(255, 0, 255, 0), filterspellname(spell.Name));
                            }
                            else
                            {
                                Drawing.DrawText(target.HPBarPosition.X + X, target.HPBarPosition.Y + Y + 15, System.Drawing.Color.Red, "{0}({1})", filterspellname(spell.Name), Convert.ToString((int)t + 1));
                            }

                            Y += 15;
                        }
                        else
                        {
                            if (t < 0)
                            {
                                Drawing.DrawText(target.HPBarPosition.X + X + 85, target.HPBarPosition.Y + Y, System.Drawing.Color.FromArgb(255, 0, 255, 0), filterspellname(spell.Name));
                            }
                            else
                            {
                                Drawing.DrawText(target.HPBarPosition.X + X + 85, target.HPBarPosition.Y + Y, System.Drawing.Color.Red, filterspellname(spell.Name));
                            }

                            Y += 15;
                        }
                    }
                    Y = 40;
                    foreach (var slot in SpellSlots)
                    {
                        var spell = target.Spellbook.GetSpell(slot);
                        var t = spell.CooldownExpires - Game.Time;

                        if (Jlib.getMenuStringValue("tracker_enemyspells") == "New")
                        {
                            if (t < 0)
                            {
                                Drawing.DrawText(target.HPBarPosition.X + X, target.HPBarPosition.Y + Y, System.Drawing.Color.FromArgb(255, 0, 255, 0), Convert.ToString(spell.Level));
                            }
                            else
                            {
                                Drawing.DrawText(target.HPBarPosition.X + X, target.HPBarPosition.Y + Y, System.Drawing.Color.Red, "{0}({1})", Convert.ToString(spell.Level), Convert.ToString((int)t + 1));
                            }
                            X += 40;
                        }
                        else
                        {
                            if (t < 0)
                            {
                                Drawing.DrawText(target.HPBarPosition.X + X, target.HPBarPosition.Y + Y, System.Drawing.Color.FromArgb(255, 0, 255, 0), Convert.ToString(spell.Level));
                            }
                            else
                            {
                                Drawing.DrawText(target.HPBarPosition.X + X, target.HPBarPosition.Y + Y, System.Drawing.Color.Red, Convert.ToString(spell.Level));
                            }
                            X += 20;
                        }
                    }
                }
            }
            #endregion

            #region ward tracker
            foreach (var ward in ObjectManager.Get<Obj_AI_Base>().Where(t =>
                    wardnames.Any(a => a == t.Name) && !t.IsDead && t.IsEnemy))
            {
                if (!wardlist.Any(w => w.id == ward.NetworkId) && ward.Mana > 0 && ward.MaxHealth == 3)
                {
                    wardlist.Add(new Ward
                    {
                        position = ward.Position,
                        type = wardtype.Green,
                        id = ward.NetworkId,
                        time = ward.MaxMana,
                        endtiem = Game.Time + ward.Mana,
                        target = ward
                    });
                }
                if (!wardlist.Any(w => w.id == ward.NetworkId) && ward.MaxHealth == 5)
                {
                    wardlist.Add(new Ward
                    {
                        position = ward.Position,
                        type = wardtype.Pink,
                        id = ward.NetworkId,
                        time = ward.MaxMana,
                        endtiem = float.MaxValue,
                        target = ward
                    });
                }
                if (!wardlist.Any(w => w.id == ward.NetworkId) && ward.Name == "Jack In The Box")
                {
                    wardlist.Add(new Ward
                    {
                        position = ward.Position,
                        type = wardtype.ShacoBox,
                        id = ward.NetworkId,
                        time = ward.MaxMana,
                        endtiem = Game.Time + ward.Mana,
                        target = ward
                    });
                }
                if (!wardlist.Any(w => w.id == ward.NetworkId) && ward.Name == "Cupcake Trap" && ward.SkinName != "JinxMine")
                {
                    wardlist.Add(new Ward
                    {
                        position = ward.Position,
                        type = wardtype.Trap,
                        id = ward.NetworkId,
                        time = 4 * 60,
                        endtiem = Game.Time + 240,
                        target = ward
                    });
                }
                if (!wardlist.Any(w => w.id == ward.NetworkId) && ward.Name == "Noxious Trap")
                {
                    wardlist.Add(new Ward
                    {
                        position = ward.Position,
                        type = wardtype.Mushroom,
                        id = ward.NetworkId,
                        time = ward.MaxMana,
                        endtiem = Game.Time + ward.Mana,
                        target = ward
                    });
                }
            }

            #endregion wardtracker

            #region fake tracker
            if (Jlib.getMenuBool("tracker_fake"))
            {
                if (ObjectManager.Get<Obj_AI_Minion>().Any(t =>
                    (t.BaseSkinName == "Shaco" || t.BaseSkinName == "Leblanc") && !t.IsDead && t.IsEnemy))
                {
                    foreach (var t in ObjectManager.Get<Obj_AI_Minion>().Where(t =>
                        (t.BaseSkinName == "Shaco" || t.BaseSkinName == "Leblanc") && !t.IsDead && t.IsEnemy && t.IsVisible))
                    {
                        if (!fakeList.Any(s => s.id == t.NetworkId) && t.BaseSkinName == "Leblanc")
                        {
                            fakeList.Add(new Fake()
                            {
                                id = t.NetworkId,
                                position = t.Position,
                                target = t,
                                time = Game.Time + 8.0f
                            });
                        }
                        else if (!fakeList.Any(s => s.id == t.NetworkId) && t.BaseSkinName == "Shaco")
                        {
                            fakeList.Add(new Fake()
                            {
                                id = t.NetworkId,
                                position = t.Position,
                                target = t,
                                time = Game.Time + 18.0f
                            });
                        }
                    }
                }
                if (fakeList.Any())
                {
                    foreach (var temp in fakeList.Where(t => t.target.IsDead || Game.Time > t.time))
                    {
                        temp.text_fake.Remove();
                        fakeList.Remove(temp);
                        break;
                    }

                    foreach (var t in fakeList.Where(t => !t.target.IsDead))
                    {
                        t.text_fake.PositionUpdate = delegate
                        {
                            Vector2 vec2 = Drawing.WorldToScreen(new Vector3(t.target.Position.X,
                                t.target.Position.Y + 25, t.target.Position.Z));
                            return vec2;
                        };
                    }

                }
            }
            #endregion fake tracker

            #region 점프와드- Jump2Ward (Jax,Kata,LeeSin)
            if (Jlib.getMenuBool("j2w_bool"))
            {
                List<String> champs = new List<String>();
                champs.Add("LeeSin"); champs.Add("Katarina"); champs.Add("Jax");
                setj2wslots(champs);
                if (canw2j)
                {
                    checkE();
                    checkWard();
                    if (rdyw2j && baseMenu.Item("j2w_hkey").GetValue<KeyBind>().Active)
                    {
                        Vector3 cursor = Game.CursorPos;
                        Vector3 myPos = Player.ServerPosition;
                        Player.IssueOrder(GameObjectOrder.MoveTo, cursor);
                        foreach (var target in ObjectManager.Get<Obj_AI_Base>().Where(ward => ward.IsVisible && ward.IsAlly && !ward.IsMe &&
                            Vector3.DistanceSquared(cursor, ward.ServerPosition) <= 200 * 200 &&
                            Vector3.Distance(ward.Position, Player.Position) <= 700 && ward.Name.IndexOf("Turret") == -1))
                        {
                            jumpspell.CastOnUnit(target);
                        }

                        if (rdyward)
                        {
                            Player.Spellbook.CastSpell(Items.GetWardSlot().SpellSlot, cursor);
                        }
                    }
                }
                if (Jlib.getMenuBool("j2w_info"))
                {
                    Game.PrintChat("Champion : " + Player.ChampionName);
                    Game.PrintChat("Can? : " + canw2j);
                    Game.PrintChat("Spell : " + jumpspell.Slot.ToString());
                    Game.PrintChat("WardStack : " + Items.GetWardSlot().Stacks);
                    baseMenu.Item("j2w_info").SetValue<bool>(false);
                }
            }
            #endregion

            #region 스택 - Stacks
            if (Jlib.getMenuBool("st_twitch") && Player.ChampionName == "Twitch")
            {
                Spell E = new Spell(SpellSlot.E, 1200);


                var _List = ObjectManager.Get<Obj_AI_Hero>().Where(t =>
                    E.IsReady() &&
                    t.Distance(Player.Position) <= 900 &&
                    !t.IsDead && t.IsEnemy && t.HasBuff("TwitchDeadlyVenom"));

                foreach (var target in _List)
                {
                    var Count = target.Buffs.First(buff => buff.DisplayName == "TwitchDeadlyVenom").Count;
                    var damage = getTwitEDmg(target, Count, Player_addAD, Player_totalAP, E.Level);
                    if (damage >= target.Health && E.IsReady())
                        E.Cast();

                    if (Jlib.getMenuBool("st_bool"))
                    {
                        String t_damage = Convert.ToInt64(damage).ToString() + "(" + Count + ")";
                        Drawing.DrawText(target.HPBarPosition.X, target.HPBarPosition.Y - 5, Color.Red, t_damage);
                    }
                }
            }

            if (Jlib.getMenuBool("st_kalista") && Player.ChampionName == "Kalista")
            {
                Spell E = new Spell(SpellSlot.E, 900);

                var _List = ObjectManager.Get<Obj_AI_Hero>().Where(t =>
                    E.IsReady() &&
                    t.Distance(Player.Position) <= 900 &&
                        !t.IsDead && t.IsEnemy && t.HasBuff("KalistaExpungeMarker"));

                foreach (var target in _List)
                {
                    var Count = target.Buffs.First(buff => buff.DisplayName == "KalistaExpungeMarker").Count;
                    var damage = getKaliDmg(target, Count, Player_totalAD, E.Level);

                    if (damage >= target.Health && !target.MagicImmune)
                        E.Cast();

                    if (Jlib.getMenuBool("st_bool"))
                    {
                        String t_damage = Convert.ToInt64(damage).ToString() + "(" + Count + ")";
                        Drawing.DrawText(target.HPBarPosition.X, target.HPBarPosition.Y - 5, Color.Red, t_damage);
                    }
                }

                Obj_AI_Base mob = GetNearest(Player.ServerPosition);
                if (mob != null)
                {
                    foreach (var venoms in mob.Buffs.Where(venoms => venoms.DisplayName == "KalistaExpungeMarker"))
                    {
                        var damage = getKaliDmg(mob, venoms.Count, Player_totalAD, E.Level);
                        if (damage >= mob.Health && Vector3.Distance(mob.Position, Player.Position) <= 900)
                            E.Cast();

                        if (Jlib.getMenuBool("st_bool"))
                        {
                            String t_damage = Convert.ToInt64(damage).ToString() + "(" + venoms.Count + ")";
                            Drawing.DrawText(mob.HPBarPosition.X, mob.HPBarPosition.Y - 5, Color.Red, t_damage);
                        }
                    }
                }
            }
            #endregion
            
            #region Status on hud
            if (Jlib.getMenuBool("base_stat"))
            {
                /*
                 * 오토스마이트
                 * 오토이그나이트
                 * 점프와드
                 * 스택
                 * Items
                 * Spell
                 */


                int x = Jlib.getMenuValue("x%") * Drawing.Width / 100;
                int y = Jlib.getMenuValue("y%") * Drawing.Height / 100;
                int interval = 20;
                int i = 0;

                Drawing.DrawText(x, y, Color.Wheat, "Champion : " + Player.ChampionName);
                i++;


                if (smiteSlot != SpellSlot.Unknown)
                {
                    addText(y + (interval * i), (Jlib.getMenuBool("AutoSmite") && smiteSlot != SpellSlot.Unknown), "AutoSmite");
                    i++;
                }

                if (igniteSlot != SpellSlot.Unknown)
                {
                    addText(y + (interval * i), (Jlib.getMenuBool("AutoIgnite") && igniteSlot != SpellSlot.Unknown), "AutoIgnite");
                    i++;
                }

                if (jumpspell != null)
                {
                    addText(y + (interval * i), (Jlib.getMenuBool("j2w_bool") && jumpspell != null), "Jump2Ward");
                    i++;
                }
                if (healspell.Slot != SpellSlot.Unknown)
                {
                    addText(y + (interval * i), Jlib.getMenuBool("spell_hb"), string.Format("SpellCast(Heal)"));
                    i++;
                }
                if (barrierspell.Slot != SpellSlot.Unknown)
                {
                    addText(y + (interval * i), Jlib.getMenuBool("spell_hb"), string.Format("SpellCast(Barrier)"));
                    i++;
                }
                if (cleansespell.Slot != SpellSlot.Unknown)
                {
                    addText(y + (interval * i), Jlib.getMenuBool("spell_cleanse"), string.Format("SpellCast(Cleanse)"));
                    i++;
                }
                if (Items.HasItem(Convert.ToInt32(ItemId.Crystalline_Flask)))
                {
                    addText(y + (interval * i), (Jlib.getMenuBool("useitem_flask")), string.Format("Use Flask({0}%)", Jlib.getMenuValue("useitem_p_flask")));
                    i++;
                }
                if (Items.HasItem(Convert.ToInt32(ItemId.Health_Potion)))
                {
                    addText(y + (interval * i), (Jlib.getMenuBool("useitem_hppotion")), string.Format("Use HP Potion({0}%)", Jlib.getMenuValue("useitem_p_hp")));
                    i++;
                }
                if (Items.HasItem(2010))
                {
                    addText(y + (interval * i), (Jlib.getMenuBool("useitem_hppotion")), string.Format("Use Cookie ({0}%)", Jlib.getMenuValue("useitem_p_hp")));
                    i++;
                }

                if (Items.HasItem(Convert.ToInt32(ItemId.Mana_Potion)))
                {
                    addText(y + (interval * i), (Jlib.getMenuBool("useitem_manapotion")), string.Format("Use Mana Potion({0}%)", Jlib.getMenuValue("useitem_p_mana")));
                    i++;
                }
                if (Items.HasItem(Convert.ToInt32(ItemId.Zhonyas_Hourglass)))
                {
                    addText(y + (interval * i), (Jlib.getMenuBool("useitem_zhonya")), string.Format("UseZhonya({0}%)", Jlib.getMenuValue("useitem_p_zhonya")));
                    i++;
                }
                if (Items.HasItem(3040))
                {
                    addText(y + (interval * i), (Jlib.getMenuBool("useitem_seraph")), string.Format("UseSeraph({0}%)", Jlib.getMenuValue("useitem_p_seraph")));
                    i++;
                }
                if (Items.HasItem(Convert.ToInt32(ItemId.Bilgewater_Cutlass)))
                {
                    addText(y + (interval * i), (Jlib.getMenuBool("useitem_bilgewater")), string.Format("UseBilgewater({0}%)", Jlib.getMenuValue("useitem_p_bilgewater")));
                    i++;
                }
                if (Items.HasItem(Convert.ToInt32(ItemId.Blade_of_the_Ruined_King)))
                {
                    addText(y + (interval * i), (Jlib.getMenuBool("useitem_botrk")), string.Format("UseBOTRK({0}%)", Jlib.getMenuValue("useitem_p_botrk")));
                    i++;
                }
                if (Items.HasItem(Convert.ToInt32(ItemId.Mikaels_Crucible)))
                {
                    addText(y + (interval * i), (Jlib.getMenuBool("useitem_mikaels")), string.Format("Use Mikaels({0}%{1}", Jlib.getMenuValue("useitem_p_mikaels"),
                        Jlib.getMenuBool("mikaels_cc_bool") ? ",CC)" : ")"));
                    i++;
                }
                if (Items.HasItem(Convert.ToInt32(ItemId.Quicksilver_Sash)))
                {
                    addText(y + (interval * i), (Jlib.getMenuBool("useitem_qs_bool")), string.Format("Use QS(delay:{0})", Jlib.getMenuValue("useitem_p_qs_delay")));
                    i++;
                }
                if (Items.HasItem(Convert.ToInt32(ItemId.Mercurial_Scimitar)))
                {
                    addText(y + (interval * i), (Jlib.getMenuBool("useitem_qs_bool")), string.Format("Use Scimitar(delay:{0})", Jlib.getMenuValue("useitem_p_qs_delay")));
                    i++;
                }
                //champ
                #region stack
                if (Player.ChampionName == "Twitch")
                {
                    addText(y + (interval * i), (Jlib.getMenuBool("st_twitch")), "CastTwitchE");
                    i++;
                }
                if (Player.ChampionName == "Kalista")
                {
                    addText(y + (interval * i), (Jlib.getMenuBool("st_kalista")), "CastKalistaE");
                    i++;
                }
                #endregion

                addText(y + (interval * i), (Jlib.getMenuBool("draw_grab")), string.Format("DrawHardCC"));
                i++;
            }
            #endregion

            #region Clock
           
            if (clock.text != DateTime.Now.ToString("t"))
            {
                clock.text = DateTime.Now.ToString("t");
            }
            if (clock.X != Drawing.Width - 100)
            {
                clock.X = Drawing.Width - 100;
                clock.Y = Drawing.Height * 8 / 100;
            }
            #endregion

            #region Items&spells
            #region Item
            if (!Player.InShop() && Jlib.getMenuBool("useitem"))
            {
                int tempItemid = 3157;
                if (Jlib.getMenuBool("useitem_zhonya") && Items.HasItem(tempItemid) && Items.CanUseItem(tempItemid))
                {
                    foreach (var p_item in Player.InventoryItems.Where(item => item.Id == ItemId.Zhonyas_Hourglass))
                    {
                        if (Player.HealthPercentage() <= (float)Jlib.getMenuValue("useitem_p_zhonya")
                            && !Player.Buffs.Any(buff => buff.DisplayName == "Chrono Shift"))
                            Player.Spellbook.CastSpell(p_item.SpellSlot);
                    }
                }

                tempItemid = 3040;
                if (Jlib.getMenuBool("useitem_seraph") && Items.HasItem(tempItemid) && Items.CanUseItem(tempItemid))
                {

                    foreach (var p_item in Player.InventoryItems.Where(item => Convert.ToInt32(item.Id) == 3040))
                    {
                        if (Player.HealthPercentage() <= (float)Jlib.getMenuValue("useitem_p_seraph")
                            && !Player.Buffs.Any(buff => buff.DisplayName == "Chrono Shift"))
                            Player.Spellbook.CastSpell(p_item.SpellSlot);
                    }
                }

                tempItemid = Convert.ToInt32(ItemId.Bilgewater_Cutlass);
                if (Jlib.getMenuBool("useitem_bilgewater") && Items.HasItem(tempItemid) && Items.CanUseItem(tempItemid))
                {
                    Obj_AI_Hero target = null;
                    foreach (var p_item in Player.InventoryItems.Where(item => item.Id == ItemId.Bilgewater_Cutlass))
                    {
                        if (ObjectManager.Get<Obj_AI_Hero>().Any(h => h.IsEnemy && !h.IsDead && h.IsVisible &&
                            Vector3.Distance(h.Position, Player.Position) <= Jlib.getMenuValue("useitem_bilgewater_atg_p")))
                        {
                            target = ObjectManager.Get<Obj_AI_Hero>().First(h => h.IsEnemy && !h.IsDead && h.IsVisible &&
                            Vector3.Distance(h.Position, Player.Position) <= Jlib.getMenuValue("useitem_bilgewater_atg_p"));
                            Player.Spellbook.CastSpell(p_item.SpellSlot, target);
                        }

                        if (Player.HealthPercentage() <= (float)Jlib.getMenuValue("useitem_p_bilgewater"))
                        {
                            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsEnemy && h.IsValid && h.IsVisible && Vector3.Distance(h.Position, Player.Position) <= 450))
                            {
                                Player.Spellbook.CastSpell(p_item.SpellSlot, hero);
                            }
                        }
                    }
                }

                tempItemid = Convert.ToInt32(ItemId.Blade_of_the_Ruined_King);
                if (Jlib.getMenuBool("useitem_botrk") && Items.HasItem(tempItemid) && Items.CanUseItem(tempItemid))
                {
                    Obj_AI_Hero target = null;
                    Double max_healpoint = 0;

                    foreach (var p_item in Player.InventoryItems.Where(item => item.Id == ItemId.Blade_of_the_Ruined_King))
                    {
                        if (Jlib.getMenuBool("useitem_botrk_atg"))
                        {
                            if (ObjectManager.Get<Obj_AI_Hero>().Any(h => h.IsEnemy && !h.IsDead && h.IsVisible &&
                                Vector3.Distance(h.Position, Player.Position) <= Jlib.getMenuValue("useitem_botrk_atg_p")))
                            {
                                target = ObjectManager.Get<Obj_AI_Hero>().First(h => h.IsEnemy && !h.IsDead && h.IsVisible &&
                                Vector3.Distance(h.Position, Player.Position) <= Jlib.getMenuValue("useitem_botrk_atg_p"));
                                Player.Spellbook.CastSpell(p_item.SpellSlot, target);
                            }
                        }

                        if (Player.HealthPercentage() <= (float)Jlib.getMenuValue("useitem_p_botrk"))
                        {
                            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsEnemy && h.IsValid && h.IsVisible && Vector3.Distance(h.Position, Player.Position) <= 450))
                            {
                                var healpoint = Player.CalcDamage(hero, Damage.DamageType.Physical, hero.MaxHealth * 0.1);
                                if (max_healpoint < healpoint)
                                {
                                    max_healpoint = healpoint;
                                    target = hero;
                                }
                            }
                            Player.Spellbook.CastSpell(p_item.SpellSlot, target);
                        }
                    }
                }

                tempItemid = Convert.ToInt32(ItemId.Mikaels_Crucible);
                if (Jlib.getMenuBool("useitem_mikaels") && Items.HasItem(tempItemid) && Items.CanUseItem(tempItemid))
                {
                    List<BuffType> bufflist = new List<BuffType>();
                    getbufflist(bufflist, ItemId.Mikaels_Crucible);
                    foreach (var p_item in Player.InventoryItems.Where(item => item.Id == ItemId.Mikaels_Crucible))
                    {
                        foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsAlly && !h.IsMe && h.IsValid && h.IsVisible && Vector3.Distance(h.Position, Player.Position) <= 800))
                        {
                            if (hero.HealthPercentage() <= (float)Jlib.getMenuValue("useitem_p_mikaels"))
                                Player.Spellbook.CastSpell(p_item.SpellSlot, hero);

                            if (Jlib.getMenuBool("mikaels_cc_bool"))
                            {
                                foreach (var buff in hero.Buffs)
                                {
                                    if (bufflist.Any(b => b == buff.Type))
                                        Utility.DelayAction.Add(Jlib.getMenuValue("useitem_p_mikaels_delay"), () => { Player.Spellbook.CastSpell(p_item.SpellSlot, hero); });
                                }
                            }
                        }
                    }
                }
                tempItemid = Convert.ToInt32(ItemId.Quicksilver_Sash);
                int tempItemid2 = Convert.ToInt32(ItemId.Mercurial_Scimitar);
                if (Jlib.getMenuBool("useitem_qs_bool"))
                {
                    if ((Items.HasItem(tempItemid) && Items.CanUseItem(tempItemid)) || ((Items.HasItem(tempItemid2) && Items.CanUseItem(tempItemid2))))
                    {
                        List<BuffType> bufflist = new List<BuffType>();
                        getbufflist(bufflist, ItemId.Quicksilver_Sash);
                        foreach (var p_item in Player.InventoryItems.Where(item => (item.Id == ItemId.Quicksilver_Sash || item.Id == ItemId.Mercurial_Scimitar)))
                        {
                            foreach (var buff in Player.Buffs)
                            {
                                Utility.DelayAction.Add(Jlib.getMenuValue("useitem_p_qs_delay"), () =>
                                {
                                    if (bufflist.Any(b => b == buff.Type))
                                        Player.Spellbook.CastSpell(p_item.SpellSlot);
                                    if (buff.DisplayName == "ZedUltExecute" && Jlib.getMenuBool("qs_cc_zedult"))
                                        Player.Spellbook.CastSpell(p_item.SpellSlot);
                                    if (buff.DisplayName == "FizzChurnTheWatersCling" && Jlib.getMenuBool("qs_cc_fizzult"))
                                        Player.Spellbook.CastSpell(p_item.SpellSlot);
                                });
                            }
                        }
                    }
                }
                //potions
                tempItemid = Convert.ToInt32(ItemId.Crystalline_Flask);
                if (Jlib.getMenuBool("useitem_flask") && Items.HasItem(tempItemid) && Items.CanUseItem(tempItemid))
                {
                    foreach (var p_item in Player.InventoryItems.Where(item => item.Id == ItemId.Crystalline_Flask && !Player.HasBuff("ItemCrystalFlask")))
                    {
                        if (Player.HealthPercentage() <= (float)Jlib.getMenuValue("useitem_p_fla"))
                            Player.Spellbook.CastSpell(p_item.SpellSlot);
                    }
                }

                tempItemid = Convert.ToInt32(ItemId.Health_Potion);
                if (Jlib.getMenuBool("useitem_hppotion"))
                {
                    ItemId item = ItemId.Health_Potion;
                    if (Player.InventoryItems.Any(t => (t.Id == ItemId.Health_Potion || Convert.ToInt32(t.Id) == 2010)))
                    {
                        if (Player.InventoryItems.First(t => (t.Id == ItemId.Health_Potion || Convert.ToInt32(t.Id) == 2010)).Id != ItemId.Health_Potion)
                            item = ItemId.Unknown;

                        if (Player.HealthPercentage() <= (float)Jlib.getMenuValue("useitem_p_hp") && Player.InventoryItems.Any(t => (t.Id == ItemId.Health_Potion || Convert.ToInt32(t.Id) == 2010)))
                        {
                            if (!Player.HasBuff("ItemMiniRegenPotion") && item == ItemId.Unknown)
                                Player.Spellbook.CastSpell(Player.InventoryItems.First(t => Convert.ToInt32(t.Id) == 2010).SpellSlot);
                            if (!Player.HasBuff("Health Potion") && item == ItemId.Health_Potion)
                                Player.Spellbook.CastSpell(Player.InventoryItems.First(t => t.Id == ItemId.Health_Potion).SpellSlot);

                        }
                    }
                }



                tempItemid = Convert.ToInt32(ItemId.Mana_Potion);
                if (Jlib.getMenuBool("useitem_manapotion") && Items.HasItem(tempItemid) && Items.CanUseItem(tempItemid))
                {
                    foreach (var p_item in Player.InventoryItems.Where(item => item.Id == ItemId.Mana_Potion && !Player.HasBuff("Mana Potion") && !Player.HasBuff("ItemCrystalFlask")))
                    {
                        if (Player.ManaPercentage() <= (float)Jlib.getMenuValue("useitem_p_mana"))
                        {
                            Player.Spellbook.CastSpell(p_item.SpellSlot);
                        }
                    }
                }
            #endregion
                //spell
                if (Jlib.getMenuBool("spell_hb"))
                {
                    if (Player.HealthPercentage() <= (float)Jlib.getMenuValue("spell_hb_hp"))
                    {
                        if (Player.Spellbook.CanUseSpell(healspell.Slot) == SpellState.Ready)
                            Player.Spellbook.CastSpell(healspell.Slot);
                        if (Player.Spellbook.CanUseSpell(barrierspell.Slot) == SpellState.Ready)
                            Player.Spellbook.CastSpell(barrierspell.Slot);
                    }
                }
                if (Jlib.getMenuBool("spell_cleanse"))
                {
                    if (Player.Buffs.Any(b =>
                        b.Type == BuffType.Stun || b.Type == BuffType.Flee || b.Type == BuffType.Fear
                            || b.Name.Contains("exhaust") || b.Type == BuffType.Taunt))
                    {
                        Utility.DelayAction.Add(Jlib.getMenuValue("spell_cleanse_delay"),
                            () => { Player.Spellbook.CastSpell(cleansespell.Slot); });
                    }
                }
            }
            #endregion


        }


        // Addional Function //
        #region 스마이트함수 - Smite Function

        public static readonly ItemId[] SmitePurple = { ItemId.Rangers_Trailblazer, 
                                                          ItemId.Rangers_Trailblazer_Enchantment_Devourer, 
                                                          ItemId.Rangers_Trailblazer_Enchantment_Juggernaut,
                                                          ItemId.Rangers_Trailblazer_Enchantment_Magus,
                                                          ItemId.Rangers_Trailblazer_Enchantment_Warrior };
        public static readonly ItemId[] SmiteGrey = { ItemId.Poachers_Knife, 
                                                          ItemId.Poachers_Knife_Enchantment_Devourer, 
                                                          ItemId.Poachers_Knife_Enchantment_Juggernaut,
                                                          ItemId.Poachers_Knife_Enchantment_Magus,
                                                          ItemId.Poachers_Knife_Enchantment_Warrior };
        public static readonly ItemId[] SmiteRed = { ItemId.Skirmishers_Sabre, 
                                                          ItemId.Skirmishers_Sabre_Enchantment_Devourer, 
                                                          ItemId.Skirmishers_Sabre_Enchantment_Juggernaut,
                                                          ItemId.Skirmishers_Sabre_Enchantment_Magus,
                                                          ItemId.Skirmishers_Sabre_Enchantment_Warrior };
        public static readonly ItemId[] SmiteBlue = { ItemId.Stalkers_Blade, 
                                                          ItemId.Stalkers_Blade_Enchantment_Devourer, 
                                                          ItemId.Stalkers_Blade_Enchantment_Juggernaut,
                                                          ItemId.Stalkers_Blade_Enchantment_Magus,
                                                          ItemId.Stalkers_Blade_Enchantment_Warrior };
        private static readonly string[] MinionNames =
        {
        "TT_Spiderboss", "TTNGolem", "TTNWolf", "TTNWraith",
            "SRU_Blue", "SRU_Gromp", "SRU_Murkwolf", "SRU_Razorbeak", "SRU_Red", "SRU_Krug", "SRU_Dragon", "SRU_Baron"
        };

        public static void setSmiteSlot()
        {


            foreach (var spell in Player.Spellbook.Spells.Where(spell =>
                String.Equals(spell.Name, smitetype(), StringComparison.CurrentCultureIgnoreCase)))
            {
                smiteSlot = spell.Slot;
                smite = new Spell(smiteSlot, 700);
                return;
            }
        }
        public static string smitetype()
        {
            if (Player.InventoryItems.Any(item => SmiteBlue.Any(t => t == item.Id)))
            {
                return "s5_summonersmiteplayerganker";
            }
            if (Player.InventoryItems.Any(item => SmiteRed.Any(t => t == item.Id)))
            {
                return "s5_summonersmiteduel";
            }
            if (Player.InventoryItems.Any(item => SmiteGrey.Any(t => t == item.Id)))
            {
                return "s5_summonersmitequick";
            }
            if (Player.InventoryItems.Any(item => SmitePurple.Any(t => t == item.Id)))
            {
                return "itemsmiteaoe";
            }
            return "summonersmite";
        }
        public static Obj_AI_Minion GetNearest(Vector3 pos)
        {

            if (ObjectManager.Get<Obj_AI_Minion>().Any(t => MinionNames.Any(name => t.Name.StartsWith(name))
                && !MinionNames.Any(name => t.Name.Contains("Mini")) && t.IsValid && !t.IsDead))
            {
                return ObjectManager.Get<Obj_AI_Minion>().OrderBy(t => Player.Distance(t.Position))
                        .First(minion => minion.IsValid && !minion.IsDead &&
                            MinionNames.Any(name => minion.Name.StartsWith(name)) &&
                            !MinionNames.Any(name => minion.Name.Contains("Mini")));
            }
            else
                return null;


        }
        public static double setSmiteDamage()
        {
            int level = Player.Level;
            int[] damage =
            {
                20*level + 370,
                30*level + 330,
                40*level + 240,
                50*level + 100
            };
            return damage.Max();
        }
        #endregion

        #region 이그나이트 함수 - Ignite
        public static void setIgniteSlot()
        {
            foreach (var spell in Player.Spellbook.Spells.Where(spell => String.Equals(spell.Name, "summonerdot", StringComparison.CurrentCultureIgnoreCase)))
            {
                igniteSlot = spell.Slot;
                ignite = new Spell(smiteSlot, 600);
                return;
            }
        }

        public static float setigniteDamage()
        {
            float dmg = 50 + 20 * Player.Level;
            return dmg;
        }
        #endregion

        #region 트래커함수 - Tracker
        public static string filterspellname(String a)
        {
            switch (a)
            {
                case "s5_summonersmiteplayerganker":
                    a = "BSmite"; break;
                case "s5_summonersmiteduel":
                    a = "RSmite"; break;
                case "s5_summonersmitequick":
                    a = "Smite"; break;
                case "itemsmiteaoe":
                    a = "Smite"; break;
                default:
                    break;
            }
            a = a.Replace("summoner", "").Replace("dot", "ignite");

            return a;
        }

        private static void OnCreate(GameObject sender, EventArgs args)
        {
            if (!sender.Name.Contains(".troy"))
            {
            }

            if (sender.IsValid<Obj_GeneralParticleEmitter>() && !sender.Name.Contains("Turret"))
            {
            }
        }

        private static void OnDelete(GameObject sender, EventArgs args)
        {
            if (towerRanges.Any(t => t.Position == sender.Position))
            {
                towerRanges.Remove(towerRanges.First(t => t.Position == sender.Position));
            }
            if (!sender.Name.Contains(".troy"))
            {

            }
        }
        #endregion

        #region 점프와드함수 - J2W
        public static void setj2wslots(List<String> a)
        {
            foreach (String champname in a)
            {
                if (champname == Player.ChampionName)
                {
                    canw2j = true;
                    switch (champname)
                    {
                        case "LeeSin":
                            jumpspell = new Spell(SpellSlot.W, 700);
                            return;
                        case "Katarina":
                            jumpspell = new Spell(SpellSlot.E, 700);
                            return;
                        case "Jax":
                            jumpspell = new Spell(SpellSlot.Q, 700);
                            return;
                    }
                }
            }
        }
        public static void checkE()
        {
            if (Player.ChampionName == "LeeSin")
            {
                rdyw2j = jumpspell.IsReady() && Player.Spellbook.GetSpell(SpellSlot.W).Name == "BlindMonkWOne";
            }
            else
            {
                rdyw2j = jumpspell.IsReady();
            }
        }

        public static void checkWard()
        {
            var Slot = Items.GetWardSlot();
            rdyward = !(Slot == null || Slot.Stacks == 0);
        }


        #endregion

        #region 스펠함수 - Item & Spell
        public static void getbufflist(List<BuffType> list, ItemId whatitem)
        {
            if (whatitem == ItemId.Mikaels_Crucible)
            {
                if (Jlib.getMenuBool("mikaels_cc_stun"))
                    list.Add(BuffType.Stun);
                if (Jlib.getMenuBool("mikaels_cc_fear"))
                {
                    list.Add(BuffType.Fear);
                    list.Add(BuffType.Flee);
                }
                if (Jlib.getMenuBool("mikaels_cc_charm"))
                    list.Add(BuffType.Charm);
                if (Jlib.getMenuBool("mikaels_cc_taunt"))
                    list.Add(BuffType.Taunt);
                if (Jlib.getMenuBool("mikaels_cc_snare"))
                    list.Add(BuffType.Snare);
                if (Jlib.getMenuBool("mikaels_cc_silence"))
                    list.Add(BuffType.Silence);
                if (Jlib.getMenuBool("mikaels_cc_polymorph"))
                    list.Add(BuffType.Polymorph);
            }
            if (whatitem == ItemId.Quicksilver_Sash)
            {
                if (Jlib.getMenuBool("qs_cc_stun"))
                    list.Add(BuffType.Stun);
                if (Jlib.getMenuBool("qs_cc_fear"))
                {
                    list.Add(BuffType.Fear);
                    list.Add(BuffType.Flee);
                }
                if (Jlib.getMenuBool("qs_cc_charm"))
                    list.Add(BuffType.Charm);
                if (Jlib.getMenuBool("qs_cc_taunt"))
                    list.Add(BuffType.Taunt);
                if (Jlib.getMenuBool("qs_cc_snare"))
                    list.Add(BuffType.Snare);
                if (Jlib.getMenuBool("qs_cc_silence"))
                    list.Add(BuffType.Silence);
                if (Jlib.getMenuBool("qs_cc_polymorph"))
                    list.Add(BuffType.Polymorph);
                if (Jlib.getMenuBool("qs_cc_suppression"))
                    list.Add(BuffType.Suppression);
            }

        }
        public static void setdefSpell()
        {
            foreach (var spell in Player.Spellbook.Spells)
            {
                if (spell.Name.Contains(DefSpellstr[0]))
                    barrierspell = new Spell(spell.Slot);
                if (spell.Name.Contains(DefSpellstr[1]))
                    healspell = new Spell(spell.Slot);
                if (spell.Name.Contains(DefSpellstr[2]))
                    cleansespell = new Spell(spell.Slot);
            }
        }
        #endregion

        #region 스택함수 - Stack
        public static double getKaliDmg(Obj_AI_Base target, int count, double AD, int s_level)
        {
            double[] spell_basedamage = { 0, 20, 30, 40, 50, 60 };
            double[] spell_perdamage = { 0, 0.25, 0.30, 0.35, 0.40, 0.45 };
            double eDmg = AD * 0.60 + spell_basedamage[s_level];
            count -= 1;
            eDmg = eDmg + count * (eDmg * spell_perdamage[s_level]);
            return ObjectManager.Player.GetSpellDamage(target, SpellSlot.E);
            //return Player.CalcDamage(target, Damage.DamageType.Physical,eDmg);
        }
        public static double getTwitEDmg(Obj_AI_Base target, int count, double AD, double AP, int s_level)
        {
            double[] spell_basedamage = { 0, 20, 35, 50, 65, 80 };
            double[] spell_stackdamage = { 0, 15, 20, 25, 30, 35 };
            double eDmg = spell_basedamage[s_level] + count * (spell_stackdamage[s_level] + (AP * 0.2) + (AD * 0.25));
            return Player.CalcDamage(target, Damage.DamageType.Physical, eDmg);
        }
        #endregion

        #region 정글타이머함수 - JungleTimer
        public static bool CheckMonster(String TargetName, Vector3 BasePosition, int Range = 1000)
        {
            var minions = ObjectManager.Get<Obj_AI_Minion>()
                .Where(minion => !minion.IsMinion && !minion.IsDead && minion.Name.Contains(TargetName) && minion.Position.Distance(BasePosition) <= Range);


            if (minions.Any())
                return true;
            else
                return false;
        }
        public static String Clockstring(timer_clock timer)
        {
            var min = timer.spawntime / 60;
            var sec = timer.spawntime - min * 60;
            if (timer.spawntime > 0)
                return String.Format("{0:00}:{1:00}", min, sec);
            else
                return "0";
        }
        #endregion

        #region status 함수 - Status
        public static void addText(float y, bool a, String b)
        {
            Drawing.DrawText(Jlib.getMenuValue("x%") * Drawing.Width / 100, y, a ? Color.FromArgb(0, 255, 0) : Color.Red,
                b + "[" + (a ? "ON" : "OFF") + "]");
        }
        #endregion

    }
}



