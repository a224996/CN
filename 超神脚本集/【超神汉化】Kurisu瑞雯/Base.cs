using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace KurisuRiven
{
    // KurisuRiven Base Class
    public static class Base
    {       
        // spell tickcounts
        public static int LastQ;
        public static int LastW;
        public static int LastE;
        public static int LastR;
        public static int LastAA;
        public static int LastWS;

        // can casts checks
        public static bool CanQ;
        public static bool CanW;
        public static bool CanE;
        public static bool CanMV;
        public static bool CanAA;
        public static bool CanWS;
        public static bool CanHD;
        public static bool HasHD;

        // did casts checks
        public static bool DidQ;
        public static bool DidW;
        public static bool DidE;
        public static bool DidWS;
        public static bool DidAA;

        // main vars
        public static Menu Settings;
        public static Spell Q, W, E, R;
        public static Orbwalking.Orbwalker Orbwalker;
        public static Obj_AI_Base LastTarget;
        public static readonly Obj_AI_Hero Me = ObjectManager.Player;

        public static bool UltOn;
        public static bool CanBurst;
        public static int CleaveCount;
        public static int PassiveCount;
        public static float TrueRange;
        public static float ComboDamage;

        // shorten commonly used
        internal static bool GetBool(string item)
        {
            return Settings.Item(item).GetValue<bool>();
        }

        internal static int GetSlider(string item)
        {
            return Settings.Item(item).GetValue<Slider>().Value;
        }

        internal static int GetList(string item)
        {
            return Settings.Item(item).GetValue<StringList>().SelectedIndex;
        }

        internal static void Initialize(EventArgs args)
        {
            if (Me.ChampionName != "Riven")
                return;

            // build 
            OnMenuUpdate();
            RivenEvents();

            Game.OnGameUpdate += OnGameUpdate;
            Drawing.OnDraw += Drawings.OnDraw;

            // load spells
            W = new Spell(SpellSlot.W, 250f);
            E = new Spell(SpellSlot.E, 270f);
            Q = new Spell(SpellSlot.Q, 260f);
            R = new Spell(SpellSlot.R, 1100f);

            // set prediction
            Q.SetSkillshot(0.25f, 100f, 1400f, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.25f, 300, 2200f, false, SkillshotType.SkillshotCone);

            Game.PrintChat("<b><font color=\"#FF9900\">KurisuRiven:</font></b> Loaded!");
        }

        // riven spell queue
        internal static void OnGameUpdate(EventArgs args)
        {
            // get true range
            TrueRange = Me.AttackRange + Me.Distance(Me.BBox.Minimum) + 1;

            HasHD = Items.HasItem(3077) || Items.HasItem(3074);
            CanHD = !DidAA && (Items.CanUseItem(3077) || Items.CanUseItem(3074));

            // check ult
            UltOn = Me.GetSpell(SpellSlot.R).Name != "RivenFengShuiEngine";

            if (Combo.Target != null && R.IsReady())
            {
                var dmg = Helpers.GetDmg("P", true) * 2 + Helpers.GetDmg("Q", true) + Helpers.GetDmg("W", true) +
                          Helpers.GetDmg("I") + Helpers.GetDmg("ITEMS", true);

                CanBurst = dmg >= Combo.Target.Health;
            }

            else
            {
                CanBurst = false;
            }

            ComboDamage = Helpers.GetDmg("P", true)*3 + Helpers.GetDmg("Q", true)*3 + Helpers.GetDmg("W", true) +
                          Helpers.GetDmg("I") + Helpers.GetDmg("R", true) + Helpers.GetDmg("ITEMS");

            Combo.OnGameUpdate();
            Combo.LaneFarm();
            Combo.Flee();
            Combo.SemiHarass();

            Helpers.OnBuffUpdate();
            Helpers.Windslash();

            Orbwalker.SetAttack(CanMV);
            Orbwalker.SetMovement(CanMV);

            // riven spell queue
            if (DidAA && Environment.TickCount - LastAA >= (int)(Me.AttackDelay * 100) + Game.Ping/2 + GetSlider("delay"))
            {
                DidAA = false;
                CanMV = true;
                CanQ = true;
                CanE = true;
                CanW = true;
                CanWS = true;
            }

            if (DidQ && Environment.TickCount - LastQ >= (int)(Me.AttackCastDelay * 1000) + Game.Ping/2 + 57)
            {
                DidQ = false;
                CanMV = true;
                CanAA = true;
            }

            if (DidW && Environment.TickCount - LastW >= 233)
            {
                DidW = false;
                CanMV = true;
                CanAA = true;
            }

            if (DidE && Environment.TickCount - LastE >= 350)
            {
                DidE = false;
                CanMV = true;
            }

            if (!CanW && !(DidAA || DidQ || DidE) && W.IsReady())
            {
                CanW = true;
            }

            if (!CanE && !(DidAA || DidQ || DidW) && E.IsReady())
            {
                CanE = true;
            }

            if (!CanWS && !DidAA && UltOn && R.IsReady())
            {
                CanWS = true;
            }

            if (!CanAA && !(DidQ || DidW || DidE || DidWS) &&
                Environment.TickCount - LastAA >= 1000)
            {
                CanAA = true;
            }

            if (!CanMV && !(DidQ || DidW || DidE || DidWS) &&
                Environment.TickCount - LastAA >= 1000)
            {
                CanMV = true;
            }
        }

        internal static void RivenEvents()
        {
            // anti gapclose
            AntiGapcloser.OnEnemyGapcloser += gapcloser =>
            {
                if (GetBool("antigap") && W.IsReady())
                {
                    if (gapcloser.Sender.IsValidTarget(W.Range))
                        W.Cast();
                }
            };

            // interrupter 2
            Interrupter2.OnInterruptableTarget += (sender, args) =>
            {
                if (GetBool("wint") && W.IsReady())
                {
                    if (sender.IsValidTarget(W.Range))
                        W.Cast();
                }

                if (GetBool("qint") && Q.IsReady() && CleaveCount >= 2)
                {
                    if (sender.IsValidTarget(Q.Range))
                        Q.Cast(sender.ServerPosition);
                }
            };

            // on animation
            Obj_AI_Base.OnPlayAnimation += (sender, args) =>
            {
                if (!(DidQ || DidW || DidWS || DidE) && 
                    sender.IsMe && args.Animation.Contains("Idle"))
                {
                    Orbwalking.LastAATick = Environment.TickCount + Game.Ping/2;
                    CanAA = true;
                }
            };

            // on cast
            Obj_AI_Base.OnProcessSpellCast += (sender, args) =>
            {
                if (!sender.IsMe) 
                    return;

                switch (args.SData.Name)
                {
                    case "RivenTriCleave":
                        DidQ = true;
                        //CanMV = false;
                        LastQ = Environment.TickCount;
                        Utility.DelayAction.Add(100,
                            () => Me.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos));

                        if (GetList("engage") == 1 && HasHD)
                            Helpers.CheckR(Combo.Target);
                        CanQ = false;
                        break;
                    case "RivenMartyr":
                        DidW = true;
                        //CanMV = false;
                        LastW = Environment.TickCount;
                        CanW = false;
                        break;
                    case "RivenFeint":
                        DidE = true;
                        //CanMV = false;
                        LastE = Environment.TickCount;
                        CanE = false;
                        break;
                    case "RivenFengShuiEngine":
                        LastR = Environment.TickCount;
                        if (GetBool("multir3") && CanBurst)
                        {
                            var flashslot = Me.GetSpellSlot("summonerflash");
                            if (Me.Spellbook.CanUseSpell(flashslot) == SpellState.Ready &&
                                Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                            {
                                if (Combo.Target.Distance(Me.ServerPosition) > E.Range + TrueRange + 50 &&
                                    Combo.Target.Distance(Me.ServerPosition) <= E.Range + TrueRange + 500)
                                {
                                    Me.Spellbook.CastSpell(flashslot, Combo.Target.ServerPosition);
                                }
                            }
                        }
                        break;
                    case "rivenizunablade":
                        LastWS = Environment.TickCount;
                        CanWS = false;
                        if (Q.IsReady())
                            Q.Cast(Combo.Target.ServerPosition);
                        break;
                    case "ItemTiamatCleave":
                        CanWS = true;
                        if (GetList("wsmode") == 1 && UltOn && CanWS &&
                            Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                        {
                            if (CanBurst)
                                Utility.DelayAction.Add(150, () => R.Cast(Combo.Target.ServerPosition));
                        }

                        if (GetList("engage") == 1 && !CanBurst)
                            if (Q.IsReady())
                                Q.Cast(Combo.Target.ServerPosition);
                        break;

                    case "summonerflash":
                        if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                            if (W.IsReady() && GetBool("multir3"))
                                W.Cast();
                        break;
                }

                if (args.SData.Name.Contains("BasicAttack"))
                {
                    if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                    {
                        if (CanBurst || !GetBool("usecombow") || !GetBool("usecomboe"))
                        {
                            // delay till after aa
                            Utility.DelayAction.Add(50 + (int)(Me.AttackDelay * 100) + Game.Ping/2 + GetSlider("delay"), delegate
                            {
                                if (Items.CanUseItem(3077))
                                    Items.UseItem(3077);
                                if (Items.CanUseItem(3074))
                                    Items.UseItem(3074);
                            });
                        }
                    }

                    else if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
                    {
                        if (!GetBool("usejunglew") || !GetBool("usejunglee"))
                        {
                            // delay till after aa
                            Utility.DelayAction.Add(50 + (int)(Me.AttackDelay * 100) + Game.Ping/2 + GetSlider("delay"), delegate
                            {
                                if (Items.CanUseItem(3077))
                                    Items.UseItem(3077);
                                if (Items.CanUseItem(3074))
                                    Items.UseItem(3074);
                            });

                        }
                    }
                }

                if (!DidQ && args.SData.Name.Contains("BasicAttack"))
                {
                    DidAA = true;
                    CanAA = false;
                    CanQ = false;
                    CanW = false;
                    CanE = false;
                    CanWS = false;
                    LastAA = Environment.TickCount;
                    LastTarget = (Obj_AI_Base) args.Target;
                }
            };
        }

        internal static void OrbTo(Obj_AI_Base target)
        {
            if (CanAA && CanMV)
            {
                if (target.IsValidTarget(TrueRange + 10))
                {
                    if (!(DidQ || DidW || DidE || DidAA))
                    {
                        CanQ = false;
                        Me.IssueOrder(GameObjectOrder.AttackUnit, target);                          
                    }
                }
            }
        }

        internal static void OnMenuUpdate()
        {
            Settings = new Menu("【超神汉化】K-瑞雯", "kurisuriven", true);

            var tsMenu = new Menu("目标选择", "selector");
            TargetSelector.AddToMenu(tsMenu);
            Settings.AddSubMenu(tsMenu);

            var kMenu = new Menu("走砍", "rorb");
            Orbwalker = new Orbwalking.Orbwalker(kMenu);
            kMenu.AddItem(new MenuItem("fleemode", "逃跑")).SetValue(new KeyBind(65, KeyBindType.Press));
            Settings.AddSubMenu(kMenu);

            var drMenu = new Menu("显示", "drawings");
            drMenu.AddItem(new MenuItem("drawengage", "显示连招范围")).SetValue(true);
            drMenu.AddItem(new MenuItem("drawkill", "显示击杀文本")).SetValue(true);
            drMenu.AddItem(new MenuItem("drawtarg", "显示目标范围")).SetValue(true);
            Settings.AddSubMenu(drMenu);

            var mMenu = new Menu("连招", "combostuff");
            mMenu.AddItem(new MenuItem("useignote", "使用智能点燃")).SetValue(true);
            mMenu.AddItem(new MenuItem("ultwhen", "可击杀使用R")).SetValue(new StringList(new[] { "Hard", "Extreme" }));
            mMenu.AddItem(new MenuItem("wsmode", "智能R模式"))
                .SetValue(new StringList(new[] { "只有击杀", "击杀或打出最大伤害" }, 1));
            mMenu.AddItem(new MenuItem("multir3", "如果可击杀 闪现+连招?")).SetValue(false);
            mMenu.AddItem(new MenuItem("engage", "连招模式"))
                .SetValue(new StringList(new[] { "正常", "优先提亚马特/九头蛇" }));
            Settings.AddSubMenu(mMenu);

            var sMenu = new Menu("技能", "Spells");

            var menuQ = new Menu("Q 菜单", "cleave");
            menuQ.AddItem(new MenuItem("usecomboq", "连招使用")).SetValue(true);
            menuQ.AddItem(new MenuItem("usejungleq", "清野使用")).SetValue(true);
            menuQ.AddItem(new MenuItem("uselaneq", "清线使用")).SetValue(true);
            menuQ.AddItem(new MenuItem("qint", "中断法术")).SetValue(true);
            menuQ.AddItem(new MenuItem("qgap", "防止突进")).SetValue(true);
            sMenu.AddSubMenu(menuQ);

            var menuW = new Menu("W 菜单", "kiburst");
            menuW.AddItem(new MenuItem("usecombow", "连招使用")).SetValue(true);
            menuW.AddItem(new MenuItem("usejunglew", "清野使用")).SetValue(true);
            menuW.AddItem(new MenuItem("uselanew", "清线使用")).SetValue(true);
            menuW.AddItem(new MenuItem("antigap", "中断法术")).SetValue(true);
            menuW.AddItem(new MenuItem("wint", "防止突进")).SetValue(true);
            sMenu.AddSubMenu(menuW);

            var menuE = new Menu("E 菜单", "valor");
            menuE.AddItem(new MenuItem("usecomboe", "连招使用")).SetValue(true);
            menuE.AddItem(new MenuItem("usejunglee", "清野使用")).SetValue(true);
            menuE.AddItem(new MenuItem("uselanee", "清线使用")).SetValue(true);
            menuE.AddItem(new MenuItem("vhealth", "使用血量 % <")).SetValue(new Slider(40, 1));
            sMenu.AddSubMenu(menuE);

            var menuR = new Menu("R 菜单", "blade");
            menuR.AddItem(new MenuItem("user", "连招使用")).SetValue(true);
            menuR.AddItem(new MenuItem("usews", "使用光速QA")).SetValue(true);
            sMenu.AddSubMenu(menuR);

            Settings.AddSubMenu(sMenu);

            var oMenu = new Menu("杂项", "otherstuff");
            oMenu.AddItem(new MenuItem("semiq", "使用 半-Q 骚扰/清兵")).SetValue(true);
            oMenu.AddItem(new MenuItem("forceaa", "清兵强制 AA")).SetValue(false);
            oMenu.AddItem(new MenuItem("useitems", "使用幽梦/破败")).SetValue(true);
            oMenu.AddItem(new MenuItem("keepq", "保存Q")).SetValue(true);
            oMenu.AddItem(new MenuItem("delay", "AA -> Q 延迟")).SetValue(new Slider(0, 0, 200));
            oMenu.AddItem(new MenuItem("autow", "自动 W")).SetValue(true);
            oMenu.AddItem(new MenuItem("wmin", "最少人数")).SetValue(new Slider(2, 1, 5));
            oMenu.AddItem(new MenuItem("debugtrue", "调试真实伤害")).SetValue(false);
            oMenu.AddItem(new MenuItem("debugdmg", "调试连招伤害")).SetValue(false);
            Settings.AddSubMenu(oMenu);


            Settings.AddToMainMenu();
        }
    }
}
