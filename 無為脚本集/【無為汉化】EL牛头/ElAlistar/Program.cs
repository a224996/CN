using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace ElAlistar
{
    internal class Program
    {
        private static String hero = "Alistar";
        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        private static Menu _menu;
        private static Orbwalking.Orbwalker _orbwalker;
        private static Spell _q, _w, _e, _r;
        private static SpellSlot _ignite;

        #region Main

        private static void Main(string[] args)
        {
            LeagueSharp.Common.CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        #endregion

        #region Gameloaded 

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (!Player.ChampionName.Equals(hero, StringComparison.CurrentCultureIgnoreCase))
                return;

            Notifications.AddNotification("ElAlistar by jQuery v1.0.0.8", 10000);
            Notifications.AddNotification("Do you like mexican because I'll wrap you in my arms and make you my baerito.", 10000);

            #region Spell Data

            // set spells
            _q = new Spell(SpellSlot.Q, 365);
            _w = new Spell(SpellSlot.W, 650);
            _e = new Spell(SpellSlot.E, 575);
            _r = new Spell(SpellSlot.R, 0);

            // init ignite
            _ignite = Player.GetSpellSlot("summonerdot");

            #endregion

            //subscribe to event
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;

            try
            {
                InitializeMenu();
            }
            catch (Exception ex) {}
        }


        #endregion

        #region OnGameUpdate

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (_menu.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
            }

            if (_menu.Item("HarassActive").GetValue<KeyBind>().Active)
            {
                Harass();
            }


            var target = TargetSelector.GetTarget(_w.Range, TargetSelector.DamageType.Physical);
            if (Interrupter2.IsCastingInterruptableSpell(target) &&
                Interrupter2.GetInterruptableTargetData(target).DangerLevel == Interrupter2.DangerLevel.High &&
                target.IsValidTarget(_w.Range))
            {
                _w.Cast();
            }

            SelfHealing();
            HealAlly();
        }

        #endregion

        private static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender,
            Interrupter2.InterruptableTargetEventArgs args)
        {
            if (args.DangerLevel != Interrupter2.DangerLevel.Medium || sender.Distance(ObjectManager.Player) > _w.Range)
                return;

            if (sender.IsValidTarget(_w.Range) && args.DangerLevel == Interrupter2.DangerLevel.High && _q.IsReady())
            {
                _q.Cast();
                _q.Cast(ObjectManager.Player);
            }
            else if (sender.IsValidTarget(_w.Range) && args.DangerLevel == Interrupter2.DangerLevel.High && _w.IsReady() &&
                     !_q.IsReady())
            {
                _w.Cast();
                _w.Cast(ObjectManager.Player);
            }
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!gapcloser.Sender.IsValidTarget(_w.Range))
            {
                return;
            }

            if (gapcloser.Sender.Distance(ObjectManager.Player) > _w.Range)
            {
                return;
            }

            if (gapcloser.Sender.IsValidTarget(_w.Range))
            {
                if (_menu.Item("Interrupt").GetValue<bool>() && _w.IsReady())
                {
                    _w.Cast(ObjectManager.Player);
                    _w.Cast(gapcloser.Sender);
                }

                if (_menu.Item("Interrupt").GetValue<bool>() && !_w.IsReady())
                {
                    _q.Cast(ObjectManager.Player);
                    _q.Cast(gapcloser.Sender);
                }
            }
        }

        /*private static void AliSec()
        {
            var pushDistance = 650;
            var turretRange = 1000;

            foreach (var k in ObjectManager.Player.Position.GetEnemiesInRange(650))
            {
                var pushPos = ObjectManager.Player.Position.Extend(k.Position,
                    ObjectManager.Player.Position.Distance(k.Position) + pushDistance);
                foreach (
                    var t in
                        ObjectManager.Get<Obj_Turret>()
                            .Where(
                                turret =>
                                    turret.IsAlly && !turret.IsDead && turret.Health > 0 &&
                                    turret.Position.Distance(ObjectManager.Player.Position) < 1500))
                {
                    if (t.Position.Distance(pushPos) < turretRange)
                    {
                        ObjectManager.Player.Spellbook.CastSpell(SpellSlot.W, k);
                    }
                }
            }
        }*/

        #region Harass

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(_w.Range, TargetSelector.DamageType.Physical);
            if (target == null || !target.IsValid)
            {
                return;
            }

            if (_menu.Item("HarassQ").GetValue<bool>() && _q.IsReady())
            {
                _q.CastOnUnit(target);
            } 
        }

        #endregion

        #region Combo

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(_w.Range, TargetSelector.DamageType.Physical);
            if (target == null || !target.IsValid)
            {
                return;
            }

            // Check mana before combo
            SpellDataInst Qmana = Player.Spellbook.GetSpell(SpellSlot.Q);
            SpellDataInst Wmana = Player.Spellbook.GetSpell(SpellSlot.W);

            if (_q.IsReady() && _w.IsReady() && Qmana.ManaCost + Wmana.ManaCost <= Player.Mana)
            {
                _w.CastOnUnit(target);
                var comboTime = Math.Max(0, Player.Distance(target) - 500) * 10 / 25 + 25;
                Utility.DelayAction.Add((int) comboTime, () => _q.Cast());
            }


            // if killable with just W
            if (!_q.IsReady() && _w.IsReady() && _w.IsKillable(target, 1) &&
                ObjectManager.Player.Distance(target, false) < _w.Range + target.BoundingRadius)
            {
                _w.CastOnUnit(target, true);
            }

            if (_menu.Item("SelfHeal").GetValue<bool>() &&
                (Player.Health / Player.MaxHealth) * 100 <= _menu.Item("SelfHperc").GetValue<Slider>().Value &&
                _e.IsReady())
            {
                _e.Cast(Player);
            }

            if (Player.CountEnemiesInRange(_w.Range) >= _menu.Item("rcount").GetValue<Slider>().Value &&
                _menu.Item("RCombo").GetValue<bool>() &&
                (Player.Health / Player.MaxHealth) * 100 <= _menu.Item("UltHP").GetValue<Slider>().Value)
            {
                _r.Cast();
            }

            // if w is on CD and in Q range cast q
            if (_q.IsReady() && !_w.IsReady())
            {
                _q.CastOnUnit(target);
            }

            //ignite when killable
            if (Player.Distance(target) <= 600 && IgniteDamage(target) >= target.Health &&
                _menu.Item("UseIgnite").GetValue<bool>())
            {
                Player.Spellbook.CastSpell(_ignite, target);
            }
        }

        #endregion

        #region SelfHealing

        private static void SelfHealing()
        {
            SpellDataInst Emana = Player.Spellbook.GetSpell(SpellSlot.E);
            
            if (Player.HasBuff("Recall") || Utility.InFountain(Player)) return;
            if (_menu.Item("SelfHeal").GetValue<bool>() &&
                (Player.Health / Player.MaxHealth) * 100 <= _menu.Item("SelfHperc").GetValue<Slider>().Value && Player.ManaPercentage() >= _menu.Item("minmanaE").GetValue<Slider>().Value  &&
                _e.IsReady())
            {
                _e.Cast(Player);
            }
        }

        #endregion

        #region HealAlly

        private static void HealAlly()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsAlly && !h.IsMe))
            {
                if (Player.HasBuff("Recall") || Utility.InFountain(Player)) return;
                if (_menu.Item("HealAlly").GetValue<bool>() &&
                    (hero.Health / hero.MaxHealth) * 100 <= _menu.Item("HealAllyHP").GetValue<Slider>().Value && Player.ManaPercentage() >= _menu.Item("minmanaE").GetValue<Slider>().Value &&
                    _e.IsReady() &&
                    hero.Distance(Player.ServerPosition) <= _e.Range)
                    _e.Cast(hero);
            }
        }

        #endregion

        #region GetComboDamage   

        private static float GetComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;

            if (_q.IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q);
            }

            if (_w.IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.W);
            }

            if (_ignite != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(_ignite) == SpellState.Ready)
            {
                damage += ObjectManager.Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);
            }

            return (float) damage;
        }

        #endregion

        #region Ignite

        private static float IgniteDamage(Obj_AI_Hero target)
        {
            if (_ignite == SpellSlot.Unknown || Player.Spellbook.CanUseSpell(_ignite) != SpellState.Ready)
            {
                return 0f;
            }
            return (float) Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
        }

        #endregion

        #region Menu Config

        private static void InitializeMenu()
        {
            _menu = new Menu("【無為汉化】EL牛头", hero, true);

            //Orbwalker
            var orbwalkerMenu = new Menu("走砍", "orbwalker");
            _orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            _menu.AddSubMenu(orbwalkerMenu);

            //TargetSelector
            var targetSelector = new Menu("目标选择", "TargetSelector");
            TargetSelector.AddToMenu(targetSelector);
            _menu.AddSubMenu(targetSelector);

            //Combo
            var comboMenu = _menu.AddSubMenu(new Menu("连招", "Combo"));
            comboMenu.AddItem(new MenuItem("QCombo", "[Combo] 使用 Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("WCombo", "[Combo] 使用 W").SetValue(true));
            comboMenu.AddItem(new MenuItem("RCombo", "[Combo] 使用 R").SetValue(true));
            comboMenu.AddItem(new MenuItem("UltHP", "使用大招 血量小于").SetValue(new Slider(50, 1, 100)));
            comboMenu.AddItem(new MenuItem("rcount", "使用大招 敌人数 >= ")).SetValue(new Slider(2, 1, 5));
            comboMenu.AddItem(new MenuItem("UseIgnite", "敌人可击杀使用点燃").SetValue(true));
            comboMenu.AddItem(new MenuItem("ComboActive", "连招!").SetValue(new KeyBind(32, KeyBindType.Press)));

            //Harass
            var harassMenu = _menu.AddSubMenu(new Menu("骚扰", "H"));
            harassMenu.AddItem(new MenuItem("HarassQ", "[Harass] 使用 Q").SetValue(true));
            harassMenu.AddItem(new MenuItem("HarassActive", "骚扰!").SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));

            // AliSec to tower
            //var aliSecMenu = _menu.AddSubMenu(new Menu("AliSec", "AliSec"));
            //aliSecMenu.AddItem(new MenuItem("AliSec", "[AliSec] Enemy to tower").SetValue(true));
            //aliSecMenu.AddItem(new MenuItem("AliSecActive", "AliSec to tower").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
            
            //self healing
            var healMenu = _menu.AddSubMenu(new Menu("E设置", "SH"));
            healMenu.AddItem(new MenuItem("SelfHeal", "自动治疗自己").SetValue(true));
            healMenu.AddItem(new MenuItem("SelfHperc", "血量 >= ").SetValue(new Slider(25, 1, 100)));

            healMenu.AddItem(new MenuItem("HealAlly", "自动治疗队友").SetValue(true));
            healMenu.AddItem(new MenuItem("HealAllyHP", "血量>= ").SetValue(new Slider(25, 1, 100)));
            healMenu.AddItem(new MenuItem("minmanaE", "最小蓝量")).SetValue(new Slider(55));

            //Misc
            var miscMenu = _menu.AddSubMenu(new Menu("显示", "Misc"));
            miscMenu.AddItem(new MenuItem("Drawingsoff", "关闭所有").SetValue(false));
            miscMenu.AddItem(new MenuItem("DrawQ", "显示 Q").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("DrawW", "显示 W").SetValue(new Circle()));
            miscMenu.AddItem(new MenuItem("DrawE", "显示 E").SetValue(new Circle()));

            //Interupt
            var interruptMenu = _menu.AddSubMenu(new Menu("打断技能", "I"));
            interruptMenu.AddItem(new MenuItem("Interrupt", "打断及嗯呢该").SetValue(true));
            interruptMenu.AddItem(new MenuItem("InterruptQ", "使用 Q").SetValue(true));
            interruptMenu.AddItem(new MenuItem("InterruptW", "使用 W").SetValue(true));

            //Here comes the moneyyy, money, money, moneyyyy
            var credits = _menu.AddSubMenu(new Menu("信息", "jQuery"));
            credits.AddItem(new MenuItem("ElKennen.Paypal", "if you would like to donate via paypal:"));
            credits.AddItem(new MenuItem("Elkennen.Email", "info@zavox.nl"));

            _menu.AddItem(new MenuItem("422442fsaafs4242f", ""));
            _menu.AddItem(new MenuItem("422442fsaafsf", "Version: 1.0.0.9"));
            _menu.AddItem(new MenuItem("fsasfafsfsafsa", "Made By jQuery"));

            _menu.AddToMainMenu();
        }

        #endregion

        #region Drawings

        private static void Drawing_OnDraw(EventArgs args)
        {

            var drawOff = _menu.Item("Drawingsoff").GetValue<bool>();
            var drawQ = _menu.Item("DrawQ").GetValue<Circle>();
            var drawW = _menu.Item("DrawW").GetValue<Circle>();
            var drawE = _menu.Item("DrawE").GetValue<Circle>();


            if (drawOff)
                return;

            if (drawQ.Active)
                if (_q.Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, _q.Range, _q.IsReady() ? Color.Green : Color.Red);

            if (drawW.Active)
                if (_w.Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, _w.Range, _w.IsReady() ? Color.Green : Color.Red);

            if (drawE.Active)
                if (_e.Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, _e.Range, _e.IsReady() ? Color.Green : Color.Red);
        }

        #endregion
    }
}