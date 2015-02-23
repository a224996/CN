using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.Direct3D9;
using Color = System.Drawing.Color;

namespace xc_TwistedFate
{
    internal class Program
    {
        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        private static Orbwalking.Orbwalker Orbwalker;

        private static Spell Q, W;

        private static Menu Menu;

        private static SpellSlot SFlash;
        private static SpellSlot SIgnite;

        private static Vector2 _yasuoWallCastedPos;
        private static GameObject _yasuoWall;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "TwistedFate")
                return;

            SFlash = Player.GetSpellSlot("SummonerFlash");
            SIgnite = Player.GetSpellSlot("SummonerDot");

            Q = new Spell(SpellSlot.Q, 1450);
            Q.SetSkillshot(0.25f, 40f, 1000f, false, SkillshotType.SkillshotLine);

            W = new Spell(SpellSlot.W, 1200);

            Menu = new Menu("【無爲汉化】xcsoft-卡牌", "xcoft_TF", true);

            var orbwalkerMenu = new Menu("走砍", "Orbwalker");
            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            Menu.AddSubMenu(orbwalkerMenu);

            var ts = Menu.AddSubMenu(new Menu("目标选择器", "Target Selector"));
            TargetSelector.AddToMenu(ts);

            var wMenu = new Menu("选牌", "pickcard");
            wMenu.AddItem(new MenuItem("selectgold", "黄牌").SetValue(new KeyBind('E', KeyBindType.Press)));
            wMenu.AddItem(new MenuItem("selectblue", "蓝牌").SetValue(new KeyBind('T', KeyBindType.Press)));
            wMenu.AddItem(new MenuItem("selectred", "红牌").SetValue(new KeyBind('S', KeyBindType.Press)));
            wMenu.AddItem(new MenuItem("plz1", "-尽量不要用W按键"));
            wMenu.AddItem(new MenuItem("plz2", "-W按键有时是随机选择 :("));
            Menu.AddSubMenu(wMenu);

            var comboMenu  = new Menu("连招模式", "comboop");
            comboMenu.AddItem(new MenuItem("useQ", "使用 Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("qrange", "目标在范围内使用Q").SetValue(new Slider(1200, (int)Orbwalking.GetRealAutoAttackRange(Player), 1450)));
            comboMenu.AddItem(new MenuItem("cconly", "Q只投掷给被硬控敌人").SetValue(false));
            comboMenu.AddItem(new MenuItem("useW", "使用 W").SetValue(true));
            comboMenu.AddItem(new MenuItem("ignoreshield", "忽略有盾的目标").SetValue(false));
            comboMenu.AddItem(new MenuItem("useblue", "使用蓝牌不用黄牌 蓝量(<20%)").SetValue(false));
            Menu.AddSubMenu(comboMenu);

            var harassMenu = new Menu("骚扰设置", "harassop");
            harassMenu.AddItem(new MenuItem("harassUseQ", "使用 Q").SetValue(true));
            harassMenu.AddItem(new MenuItem("harassrange", "骚扰范围").SetValue(new Slider(1200, (int)Orbwalking.GetRealAutoAttackRange(Player), 1450))).ValueChanged +=
            delegate(object sender, OnValueChangeEventArgs eventArgs)
            {
                Render.Circle.DrawCircle(Player.Position, eventArgs.GetNewValue<Slider>().Value, Color.Aquamarine, 5);
            };
            harassMenu.AddItem(new MenuItem("harassmana", "骚扰蓝量 % >").SetValue(new Slider(35, 0, 100)));
            Menu.AddSubMenu(harassMenu);

            var lasthitMenu = new Menu("补兵设置", "lasthitset");
            lasthitMenu.AddItem(new MenuItem("lasthitUseW", "使用蓝牌").SetValue(true));
            lasthitMenu.AddItem(new MenuItem("lasthitbluemana", "蓝量 %<").SetValue(new Slider(20, 0, 100)));
            Menu.AddSubMenu(lasthitMenu);

            var laneclearMenu = new Menu("清兵设置", "laneclearset");
            laneclearMenu.AddItem(new MenuItem("laneclearUseQ", "使用 Q").SetValue(true));
            laneclearMenu.AddItem(new MenuItem("laneclearQmana", "蓝量 % >").SetValue(new Slider(30, 0, 100)));
            laneclearMenu.AddItem(new MenuItem("laneclearQmc", "如果小兵数量>=").SetValue(new Slider(5, 2, 7)));
            laneclearMenu.AddItem(new MenuItem("laneclearUseW", "使用 W").SetValue(true));
            laneclearMenu.AddItem(new MenuItem("laneclearredmc", "使用红牌 小兵数量 >=").SetValue(new Slider(3, 2, 5)));
            laneclearMenu.AddItem(new MenuItem("laneclearbluemana", "使用蓝牌 蓝量 % <").SetValue(new Slider(30, 0, 100)));
            Menu.AddSubMenu(laneclearMenu);

            var jungleclearMenu = new Menu("清野设置", "jungleclearset");
            jungleclearMenu.AddItem(new MenuItem("jungleclearUseQ", "使用 Q").SetValue(true));
            jungleclearMenu.AddItem(new MenuItem("jungleclearQmana", "蓝量 % >").SetValue(new Slider(30, 0, 100)));
            jungleclearMenu.AddItem(new MenuItem("jungleclearUseW", "使用 W").SetValue(true));
            jungleclearMenu.AddItem(new MenuItem("jungleclearbluemana", "使用蓝牌 蓝量 % <").SetValue(new Slider(30, 0, 100)));
            jungleclearMenu.AddItem(new MenuItem("jgtxt", "-自动选择卡牌"));
            Menu.AddSubMenu(jungleclearMenu);

            var AdditionalsMenu = new Menu("附加选项", "additionals");
            AdditionalsMenu.AddItem(new MenuItem("goldR", "大招时落地黄牌").SetValue(true));
            AdditionalsMenu.AddItem(new MenuItem("killsteal", "使用抢人头").SetValue(true));
            AdditionalsMenu.AddItem(new MenuItem("gapcloser", "反突进").SetValue(true));
            AdditionalsMenu.AddItem(new MenuItem("interrupt", "自动打断技能").SetValue(true));
            AdditionalsMenu.AddItem(new MenuItem("usepacket", "使用封包").SetValue(true));
            AdditionalsMenu.AddItem(new MenuItem("autoIgnite", "使用点燃(可击杀)").SetValue(true));
            Menu.AddSubMenu(AdditionalsMenu);

            var Drawings = new Menu("绘制设置", "Drawings");
            Drawings.AddItem(new MenuItem("AAcircle", "AA 范围").SetValue(true));
            Drawings.AddItem(new MenuItem("FAAcircle", "闪现+ AA 范围").SetValue(true));
            Drawings.AddItem(new MenuItem("Qcircle", "Q 范围").SetValue(new Circle(true, Color.LightSkyBlue)));
            Drawings.AddItem(new MenuItem("Rcircle", "R 范围").SetValue(new Circle(true, Color.LightSkyBlue)));
            Drawings.AddItem(new MenuItem("RcircleMap", "R 范围 (小地图)").SetValue(new Circle(true, Color.White)));
            Drawings.AddItem(new MenuItem("drawMinionLastHit", "小兵最后一击").SetValue(new Circle(true, Color.GreenYellow)));
            Drawings.AddItem(new MenuItem("drawMinionNearKill", "附近的小兵").SetValue(new Circle(true, Color.Gray)));
            
            var drawComboDamageMenu = new MenuItem("Draw_ComboDamage", "绘制连招损伤").SetValue(true);
            var drawFill = new MenuItem("Draw_Fill", "绘制连招损伤填补").SetValue(new Circle(true, Color.FromArgb(90, 255, 169, 4)));
            Drawings.AddItem(drawComboDamageMenu);
            Drawings.AddItem(drawFill);
            DamageIndicator.DamageToUnit = GetComboDamage;
            DamageIndicator.Enabled = drawComboDamageMenu.GetValue<bool>();
            DamageIndicator.Fill = drawFill.GetValue<Circle>().Active;
            DamageIndicator.FillColor = drawFill.GetValue<Circle>().Color;
            drawComboDamageMenu.ValueChanged +=
            delegate(object sender, OnValueChangeEventArgs eventArgs)
            {
                DamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
            };
            drawFill.ValueChanged +=
            delegate(object sender, OnValueChangeEventArgs eventArgs)
            {
                DamageIndicator.Fill = eventArgs.GetNewValue<Circle>().Active;
                DamageIndicator.FillColor = eventArgs.GetNewValue<Circle>().Color;
            };

            Drawings.AddItem(new MenuItem("jgpos", "野区位置").SetValue(true));
            Drawings.AddItem(new MenuItem("manaper", "法力值").SetValue(true));
            Drawings.AddItem(new MenuItem("kill", "击杀").SetValue(true));

            Menu.AddSubMenu(Drawings);

            var movement = new MenuItem("movement", "Disable orbwalk movement").SetValue(false);
            movement.ValueChanged +=
            delegate(object sender, OnValueChangeEventArgs eventArgs)
            {
                Orbwalker.SetMovement(!eventArgs.GetNewValue<bool>());
            };

            Menu.AddItem(movement);

            Menu.AddToMainMenu();

            Drawing.OnDraw += Drawing_OnDraw;
            Drawing.OnEndScene += Drawing_OnEndScene;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Obj_AI_Hero.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;

            Orbwalker.SetMovement(!Menu.Item("movement").GetValue<bool>());

            Game.PrintChat("<font color = \"#33CCCC\">[xcsoft] Twisted Fate -</font> Loaded");
        }

        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!Menu.Item("gapcloser").GetValue<bool>())
                return;

            if(gapcloser.Sender.IsValidTarget(W.Range))
            {
                CardSelector.StartSelecting(Cards.Yellow);

                Render.Circle.DrawCircle(gapcloser.Sender.Position, gapcloser.Sender.BoundingRadius, Color.Gold, 5);

                var targetpos = Drawing.WorldToScreen(gapcloser.Sender.Position);

                Drawing.DrawText(targetpos[0] - 40, targetpos[1] + 20, Color.Gold, "Gapcloser");
            }

            if (Player.HasBuff("goldcardpreattack" , true) && gapcloser.Sender.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player) + 10) && gapcloser.Sender.IsTargetable)
                Player.IssueOrder(GameObjectOrder.AttackUnit, gapcloser.Sender);
        }

        static void Interrupter_OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!Menu.Item("interrupt").GetValue<bool>())
                return;

            if (spell.BuffName == "Destiny" && unit.BaseSkinName != "TwistedFate")
                return;

            if (unit.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player) + 400))
            {
                CardSelector.StartSelecting(Cards.Yellow);

                Render.Circle.DrawCircle(unit.Position, unit.BoundingRadius, Color.Gold, 5);

                var targetpos = Drawing.WorldToScreen(unit.Position);

                Drawing.DrawText(targetpos[0] - 40, targetpos[1] + 20, Color.Gold, "Interrupt");
            }
                
            if (Player.HasBuff("goldcardpreattack", true) && unit.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player) + 10) && unit.IsTargetable)
                Player.IssueOrder(GameObjectOrder.AttackUnit, unit);
            
        }

        static void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (args.Target is Obj_AI_Hero || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit)
                args.Process = CardSelector.Status != SelectStatus.Selecting && Environment.TickCount - CardSelector.LastWSent > 300;

            if (!DetectCollision(args.Target))
                args.Process = false;
        }

        static void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                Combo();

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
                harass();

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit)
                Lasthit();

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                LaneClear();
                JungleClear();
            }

            if (Menu.Item("selectgold").GetValue<KeyBind>().Active)
                CardSelector.StartSelecting(Cards.Yellow);

            if (Menu.Item("selectblue").GetValue<KeyBind>().Active)
                CardSelector.StartSelecting(Cards.Blue);

            if (Menu.Item("selectred").GetValue<KeyBind>().Active)
                CardSelector.StartSelecting(Cards.Red);

            killsteal();
            autoignite();
        }

        static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && args.SData.Name == "gate" && Menu.Item("goldR").GetValue<bool>())
            {
                CardSelector.StartSelecting(Cards.Yellow);
            }

            if (sender.IsEnemy && args.SData.Name == "YasuoWMovingWall")
            {
                _yasuoWallCastedPos = sender.ServerPosition.To2D();
            }
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var Qcircle = Menu.Item("Qcircle").GetValue<Circle>();

            if (Q.IsReady() && Qcircle.Active)
                Render.Circle.DrawCircle(Player.Position, Menu.Item("qrange").GetValue<Slider>().Value, Qcircle.Color, 5);

            var Rcircle = Menu.Item("Rcircle").GetValue<Circle>();

            if (Rcircle.Active)
                Render.Circle.DrawCircle(Player.Position, 5500, Rcircle.Color, 5);

            if (Menu.Item("AAcircle").GetValue<bool>())
            {
                if (W.IsReady())
                {
                    Color temp = Color.Gold;
                    var wName = Player.Spellbook.GetSpell(SpellSlot.W).Name;

                    if (wName == "goldcardlock") temp = Color.Gold;
                    else if (wName == "bluecardlock") temp = Color.Blue;
                    else if (wName == "redcardlock") temp = Color.Red;
                    else if (wName == "PickACard") temp = Color.LightGreen;

                    Render.Circle.DrawCircle(Player.Position, Orbwalking.GetRealAutoAttackRange(Player), temp, 5);
                }
                else
                {
                    Color temp = Color.Gold;

                    if (Player.HasBuff("goldcardpreattack", true)) temp = Color.Gold;
                    else if (Player.HasBuff("bluecardpreattack", true)) temp = Color.Blue;
                    else if (Player.HasBuff("redcardpreattack", true)) temp = Color.Red;
                    else temp = Color.Gray;

                    Render.Circle.DrawCircle(Player.Position, Orbwalking.GetRealAutoAttackRange(Player), temp, 5);
                }
            }

            if (Menu.Item("FAAcircle").GetValue<bool>())
            {
                Obj_AI_Hero target = TargetSelector.GetTarget(Orbwalking.GetRealAutoAttackRange(Player) + 400, TargetSelector.DamageType.Magical, false);

                if (target != null && SFlash != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(SFlash) == SpellState.Ready)
                {
                    Render.Circle.DrawCircle(Player.Position, Orbwalking.GetRealAutoAttackRange(Player) + 400, Color.Gold, 5);//AA+Flash Range

                    if (!target.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player)))
                    {
                        Render.Circle.DrawCircle(target.Position, target.BoundingRadius, Color.Gold);

                        var targetpos = Drawing.WorldToScreen(target.Position);

                        Drawing.DrawText(targetpos[0] - 70, targetpos[1] + 20, Color.Gold, "Flash+AA possible");
                    }

                }
                else
                    Render.Circle.DrawCircle(Player.Position, Orbwalking.GetRealAutoAttackRange(Player) + 400, Color.Gray, 5);//AA+Flash Range
            }

            var drawMinionLastHit = Menu.Item("drawMinionLastHit").GetValue<Circle>();
            var drawMinionNearKill = Menu.Item("drawMinionNearKill").GetValue<Circle>();
            if (drawMinionLastHit.Active || drawMinionNearKill.Active)
            {
                var xMinions =
                    MinionManager.GetMinions(Player.Position, Player.AttackRange + Player.BoundingRadius + 300, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth);

                foreach (var xMinion in xMinions)
                {
                    if (drawMinionLastHit.Active && Player.GetAutoAttackDamage(xMinion, true) >= xMinion.Health)
                    {
                        Render.Circle.DrawCircle(xMinion.Position, xMinion.BoundingRadius, drawMinionLastHit.Color, 5);
                    }
                    else if (drawMinionNearKill.Active && Player.GetAutoAttackDamage(xMinion, true) * 2 >= xMinion.Health)
                    {
                        Render.Circle.DrawCircle(xMinion.Position, xMinion.BoundingRadius, drawMinionNearKill.Color, 5);
                    }
                }
            }

            if (Game.MapId == (GameMapId)11 && Menu.Item("jgpos").GetValue<bool>())
            {
                const float circleRange = 100f;
                
                Render.Circle.DrawCircle(new Vector3(7461.018f, 3253.575f, 52.57141f), circleRange, Color.Blue ,5); // blue team :red
                Render.Circle.DrawCircle(new Vector3(3511.601f, 8745.617f, 52.57141f), circleRange, Color.Blue, 5); // blue team :blue
                Render.Circle.DrawCircle(new Vector3(7462.053f, 2489.813f, 52.57141f), circleRange, Color.Blue, 5); // blue team :golems
                Render.Circle.DrawCircle(new Vector3(3144.897f, 7106.449f, 51.89026f), circleRange, Color.Blue, 5); // blue team :wolfs
                Render.Circle.DrawCircle(new Vector3(7770.341f, 5061.238f, 49.26587f), circleRange, Color.Blue, 5); // blue team :wariaths

                Render.Circle.DrawCircle(new Vector3(10930.93f, 5405.83f, -68.72192f), circleRange, Color.Yellow, 5); // Dragon

                Render.Circle.DrawCircle(new Vector3(7326.056f, 11643.01f, 50.21985f), circleRange, Color.Red, 5); // red team :red
                Render.Circle.DrawCircle(new Vector3(11417.6f, 6216.028f, 51.00244f), circleRange, Color.Red, 5); // red team :blue
                Render.Circle.DrawCircle(new Vector3(7368.408f, 12488.37f, 56.47668f), circleRange, Color.Red, 5); // red team :golems
                Render.Circle.DrawCircle(new Vector3(10342.77f, 8896.083f, 51.72742f), circleRange, Color.Red, 5); // red team :wolfs
                Render.Circle.DrawCircle(new Vector3(7001.741f, 9915.717f, 54.02466f), circleRange, Color.Red, 5); // red team :wariaths                    
            }

            if (Menu.Item("manaper").GetValue<bool>())
            {
                var targetpos = Drawing.WorldToScreen(Player.Position);
                var color = Color.Green;
                var manaper = (int)Utility.ManaPercentage(Player);

                if (manaper > 75)
                    color = Color.LightGreen;
                else if (manaper > 45)
                    color = Color.Yellow;
                else if (manaper > 25)
                    color = Color.OrangeRed;
                else if (manaper > -1)
                    color = Color.Red;

                Drawing.DrawText(targetpos[0] - 40, targetpos[1] + 20, color, "Mana:" + manaper + "%");
            }

            if (Menu.Item("kill").GetValue<bool>())
            {
                foreach (Obj_AI_Hero target in ObjectManager.Get<Obj_AI_Hero>().Where(x => x != null && x.IsValid && !x.IsDead && x.IsEnemy && x.Health <= GetComboDamage(x) && x.IsVisible))
                {
                    var targetpos = Drawing.WorldToScreen(target.Position);
                    Render.Circle.DrawCircle(target.Position, target.BoundingRadius, Color.Tomato);
                    Drawing.DrawText(targetpos[0] - 30, targetpos[1] + 40, Color.Tomato, "Killable");
                }
            }
        }

        static void Drawing_OnEndScene(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var Rcirclemap = Menu.Item("RcircleMap").GetValue<Circle>();

            if (Rcirclemap.Active)
                Utility.DrawCircle(Player.Position, 5500, Rcirclemap.Color,1 , 30, true);
        }

        static void Combo()
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical, Menu.Item("ignoreshield").GetValue<bool>());

            if (W.IsReady() && Menu.Item("useW").GetValue<bool>())
            {
                if (target.IsValidTarget(W.Range))
                {
                    if (Menu.Item("useblue").GetValue<bool>())
                    {
                        if (Utility.ManaPercentage(Player) < 20)
                            CardSelector.StartSelecting(Cards.Blue);
                        else
                            CardSelector.StartSelecting(Cards.Yellow);
                    }
                    else
                        CardSelector.StartSelecting(Cards.Yellow);
                }
            }

            if (Q.IsReady() && Menu.Item("useQ").GetValue<bool>())
            {
                if (target.IsValidTarget(Menu.Item("qrange").GetValue<Slider>().Value))
                {
                    var pred = Q.GetPrediction(target);

                    if (Menu.Item("cconly").GetValue<bool>())
                    {
                        if (pred.Hitchance >= HitChance.High && DetectCollision(target))
                        {
                            foreach (var buff in target.Buffs)
                            {
                                if (buff.Type == BuffType.Stun || buff.Type == BuffType.Taunt || buff.Type == BuffType.Snare || buff.Type == BuffType.Suppression || buff.Type == BuffType.Charm || buff.Type == BuffType.Fear || buff.Type == BuffType.Flee || buff.Type == BuffType.Slow)
                                    Q.Cast(target, Menu.Item("usepacket").GetValue<bool>());
                            }
                        }
                    }
                    else if (pred.Hitchance >= HitChance.VeryHigh && DetectCollision(target))
                        Q.Cast(target, Menu.Item("usepacket").GetValue<bool>());
                }
            }
        }

        static void harass()
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (Q.IsReady() && Menu.Item("harassUseQ").GetValue<bool>() && Utility.ManaPercentage(Player) > Menu.Item("harassmana").GetValue<Slider>().Value)
            {
                if (target.IsValidTarget(Menu.Item("harassrange").GetValue<Slider>().Value) && Q.GetPrediction(target).Hitchance >= HitChance.VeryHigh && DetectCollision(target))
                    Q.Cast(target);
            }
        }

        static void Lasthit()
        {
            if (W.IsReady())
            {
                if (Menu.Item("lasthitUseW").GetValue<bool>())
                {
                    if (Utility.ManaPercentage(Player) < Menu.Item("lasthitbluemana").GetValue<Slider>().Value)
                    {
                        var xMinions = MinionManager.GetMinions(Player.Position, 700, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth);

                        foreach (var xMinion in xMinions)
                        {
                            if (Player.GetAutoAttackDamage(xMinion, false) * 3 >= xMinion.Health)
                            {
                                CardSelector.StartSelecting(Cards.Blue);
                            }
                        }
                    }
                }
            }
        }

        static void LaneClear()
        {
            if (Q.IsReady() && Menu.Item("laneclearUseQ").GetValue<bool>() && Utility.ManaPercentage(Player) > Menu.Item("laneclearQmana").GetValue<Slider>().Value)
            {
                var allMinionsQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Enemy);
                var locQ = Q.GetLineFarmLocation(allMinionsQ);

                if (locQ.MinionsHit >= Menu.Item("laneclearQmc").GetValue<Slider>().Value)
                    Q.Cast(locQ.Position, Menu.Item("usepacket").GetValue<bool>());
            }

            if (W.IsReady() && Menu.Item("laneclearUseW").GetValue<bool>())
            {
                var minioncount = MinionManager.GetMinions(Player.Position, 1500).Count;

                if (minioncount > 0)
                {
                    if (Utility.ManaPercentage(Player) > Menu.Item("laneclearbluemana").GetValue<Slider>().Value)
                    {
                        if (minioncount >= Menu.Item("laneclearredmc").GetValue<Slider>().Value)
                            CardSelector.StartSelecting(Cards.Red);
                        else
                            CardSelector.StartSelecting(Cards.Blue);
                    }
                    else
                        CardSelector.StartSelecting(Cards.Blue);
                }
            }
        }

        static void JungleClear()
        {
            var mobs = MinionManager.GetMinions(Player.ServerPosition, Orbwalking.GetRealAutoAttackRange(Player) + 100, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (mobs.Count <= 0)
                return;

            if (Q.IsReady() && Menu.Item("jungleclearUseQ").GetValue<bool>() && Utility.ManaPercentage(Player) > Menu.Item("jungleclearQmana").GetValue<Slider>().Value)
            {
                Q.Cast(mobs[0].Position, Menu.Item("usepacket").GetValue<bool>());
            }

            if (W.IsReady() && Menu.Item("jungleclearUseW").GetValue<bool>())
            {
                if (Utility.ManaPercentage(Player) > Menu.Item("jungleclearbluemana").GetValue<Slider>().Value)
                {
                    if (mobs.Count >= 2)
                        CardSelector.StartSelecting(Cards.Red);
                    else
                        CardSelector.StartSelecting(Cards.Yellow);
                }  
                else
                    CardSelector.StartSelecting(Cards.Blue);
            }
        }

        static float GetComboDamage(Obj_AI_Base enemy)
        {
            var APdmg = 0d;
            var ADdmg = 0d;
            var Truedmg = 0d;
            bool card = false;

            if(Q.IsReady())
                APdmg += Player.GetSpellDamage(enemy, SpellSlot.Q);

            if (W.IsReady())
                APdmg += Player.GetSpellDamage(enemy, SpellSlot.W, 2);
            else
            {
                card = true;
                foreach (var buff in Player.Buffs)
                {
                    if (buff.Name == "bluecardpreattack")
                        APdmg += Player.GetSpellDamage(enemy, SpellSlot.W);
                    else if (buff.Name == "redcardpreattack")
                        APdmg += Player.GetSpellDamage(enemy, SpellSlot.W, 1);
                    else if (buff.Name == "goldcardpreattack")
                        APdmg += Player.GetSpellDamage(enemy, SpellSlot.W, 2);
                    else card = false;
                }
            }

            bool passive = false;
            foreach (var buff in Player.Buffs)
            {
                if (buff.Name == "cardmasterstackparticle")
                {
                    APdmg += Player.GetSpellDamage(enemy, SpellSlot.E);
                    passive = true;
                }

                if (buff.Name == "lichbane")
                {
                    APdmg += Damage.CalcDamage(Player, enemy, Damage.DamageType.Magical, (Player.BaseAttackDamage * 0.75) + ((Player.BaseAbilityDamage + Player.FlatMagicDamageMod) * 0.5));
                    passive = true;
                }

                if (buff.Name == "sheen")
                {
                    ADdmg += Player.GetAutoAttackDamage(enemy, false);
                    passive = true;
                }
            }

            if (!card && passive)
                ADdmg += Player.GetAutoAttackDamage(enemy, false);

            return (float)ADdmg + (float)APdmg + (float)Truedmg;
        }

        static void killsteal()
        {
            if (!Menu.Item("killsteal").GetValue<bool>())
                return;

            foreach (Obj_AI_Hero target in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(Q.Range) && x.IsEnemy && !x.IsDead && !x.HasBuffOfType(BuffType.Invulnerability)))
            {
                if (target != null)
                {
                    if (Q.GetDamage(target) >= target.Health + 20 & Q.GetPrediction(target).Hitchance >= HitChance.VeryHigh)
                    {
                        if (Q.IsReady() && DetectCollision(target))
                            Q.Cast(target, Menu.Item("usepacket").GetValue<bool>());
                    }
                }
            }
        }

        static void autoignite()
        {
            if (Menu.Item("autoIgnite").GetValue<bool>() && SIgnite != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(SIgnite) == SpellState.Ready)
            {
                float ignitedamage = 50 + 20 * Player.Level;

                foreach (Obj_AI_Hero target in ObjectManager.Get<Obj_AI_Hero>().Where(x => x != null && x.IsValid && !x.IsDead && Player.ServerPosition.Distance(x.ServerPosition) < 600 && !x.IsEnemy && (x.Health + x.HPRegenRate * 1) <= ignitedamage))
                {
                    Player.Spellbook.CastSpell(SIgnite, target);
                }
            }
        }

        private static void GameObject_OnCreate(GameObject obj, EventArgs args)
        {
            if (Player.Distance(obj.Position) > 1500 || !ObjectManager.Get<Obj_AI_Hero>().Any(h => h.ChampionName == "Yasuo" && h.IsEnemy && h.IsVisible && !h.IsDead)) return;

            if (obj.IsValid &&System.Text.RegularExpressions.Regex.IsMatch(obj.Name, "_w_windwall.\\.troy",System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                _yasuoWall = obj;
        }

        private static void GameObject_OnDelete(GameObject obj, EventArgs args)
        {
            if (Player.Distance(obj.Position) > 1500 || !ObjectManager.Get<Obj_AI_Hero>().Any(h => h.ChampionName == "Yasuo" && h.IsEnemy && h.IsVisible && !h.IsDead)) return;

            if (obj.IsValid && System.Text.RegularExpressions.Regex.IsMatch(obj.Name, "_w_windwall.\\.troy",System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                _yasuoWall = null;
        }

        private static bool DetectCollision(GameObject target)
        {
            if (_yasuoWall == null || !ObjectManager.Get<Obj_AI_Hero>().Any(h => h.ChampionName == "Yasuo" && h.IsEnemy && h.IsVisible && !h.IsDead)) return true;

            var level = _yasuoWall.Name.Substring(_yasuoWall.Name.Length - 6, 1);
            var wallWidth = (300 + 50 * Convert.ToInt32(level));
            var wallDirection = (_yasuoWall.Position.To2D() - _yasuoWallCastedPos).Normalized().Perpendicular();
            var wallStart = _yasuoWall.Position.To2D() + ((int)(wallWidth / 2)) * wallDirection;
            var wallEnd = wallStart - wallWidth * wallDirection;

            var intersection = wallStart.Intersection(wallEnd, Player.Position.To2D(), target.Position.To2D());

            return !intersection.Point.IsValid();

        }
    }
}
