using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using UnderratedAIO.Helpers;
using Color = System.Drawing.Color;
using Orbwalking = UnderratedAIO.Helpers.Orbwalking;

namespace UnderratedAIO.Champions
{
    class Shen
    {
        public static Menu config;
        private static Orbwalking.Orbwalker orbwalker;
        private static readonly Obj_AI_Hero me = ObjectManager.Player;
        public static Spell Q, W, E, EFlash, R;
        public static float currEnergy;
        public static bool haspassive = true;
        public static bool PingCasted = false;
        public static float eEnergy= 120f+5f;
        private const int XOffset = 36;
        private const int YOffset = 9;
        private const int Width = 103;
        private const int Height = 8;
        private static readonly Render.Text Text = new Render.Text(0, 0, "", 11, new ColorBGRA(255, 0, 0, 255), "monospace");

        public Shen()
        {
            if (me.BaseSkinName != "Shen") return;
            InitMenu();
            InitShen();
            Game.PrintChat("<font color='#9933FF'>Soresu </font><font color='#FFFFFF'>- Shen</font>");
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Game_OnDraw;
            Obj_AI_Base.OnCreate += OnCreate;
            AntiGapcloser.OnEnemyGapcloser += OnEnemyGapcloser;
            Interrupter.OnPossibleToInterrupt += OnPossibleToInterrupt;
            Jungle.setSmiteSlot();


        }

        private static void Game_OnDraw(EventArgs args)
        {
            DrawHelper.DrawCircle(config.Item("drawaa").GetValue<Circle>(), me.AttackRange);
            DrawHelper.DrawCircle(config.Item("drawqq").GetValue<Circle>(), Q.Range);
            DrawHelper.DrawCircle(config.Item("drawee").GetValue<Circle>(), E.Range);
            DrawHelper.DrawCircle(config.Item("draweeflash").GetValue<Circle>(), EFlash.Range);
            if (config.Item("drawallyhp").GetValue<bool>()) DrawHealths();
            if (config.Item("drawincdmg").GetValue<bool>()) getIncDmg();
            Utility.HpBarDamageIndicator.DamageToUnit = ComboDamage;
            Utility.HpBarDamageIndicator.Enabled = config.Item("drawcombo").GetValue<bool>();          
        }
        private static void DrawHealths()
        {
            float i = 0;
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly && !hero.IsMe && !hero.IsDead))
            {
                var playername = hero.Name;
                if (playername.Length > 13) playername=playername.Remove(9) + "...";
                var champion = hero.SkinName;
                if (champion.Length > 12) champion = champion.Remove(7) + "...";
                var percent = (int)(hero.Health / hero.MaxHealth * 100);
                var color = Color.Red;
                if (percent > 25) color = Color.Orange;
                if (percent > 50) color = Color.Yellow;
                if (percent > 75) color = Color.Green;
                Drawing.DrawText(Drawing.Width * 0.8f, Drawing.Height * 0.1f + i, color, playername + "(" + champion + ")");
                Drawing.DrawText(Drawing.Width * 0.9f, Drawing.Height * 0.1f + i, color, ((int)hero.Health).ToString() + " (" + percent.ToString() + "%)");
                i += 20f;
            }
        }
        private static void getIncDmg()
        {
            var color = Color.Red;
            float result = CombatHelper.getIncDmg();
            var barPos = me.HPBarPosition;
            var damage = (float)result;
            if (damage == 0) return;
            var percentHealthAfterDamage = Math.Max(0, me.Health - damage) / me.MaxHealth;
            var xPos = barPos.X + XOffset + Width * percentHealthAfterDamage;

            if (damage > me.Health)
            {
                Text.X = (int)barPos.X + XOffset;
                Text.Y = (int)barPos.Y + YOffset - 13;
                Text.text = ((int)(me.Health - damage)).ToString();
                Text.OnEndScene();
            }

            Drawing.DrawLine(xPos, barPos.Y + YOffset, xPos, barPos.Y + YOffset + Height, 3, color);
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            GetPassive();
            currEnergy = me.Mana;
            bool minionBlock = false;
            foreach (Obj_AI_Minion minion in MinionManager.GetMinions(me.Position, me.AttackRange, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.None))
            {
                if (HealthPrediction.GetHealthPrediction(minion, 3000) <= Damage.GetAutoAttackDamage(me, minion, false))
                    minionBlock = true;
            }
			            if (config.Item("useeflash").GetValue<KeyBind>().Active && me.Spellbook.CanUseSpell(me.GetSpellSlot("SummonerFlash")) == SpellState.Ready)
            {
                //Game.PrintChat("flashCombo");
                FlashCombo();
            }
            switch (orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    if (!minionBlock) Harass();
                    Ulti();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    LasthitQ();
                    Clear();
                    Ulti();
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    LasthitQ();
                    Ulti();
                    break;
                default:
                    if (!minionBlock)
                    {
                        Ulti();
                        //AutoQ();
                        //AutoW();
                    }
                    break;

            }
            if (config.Item("autotauntattower").GetValue<bool>())
            {
                var enemy = getEnemiesAtMyTurret(me);
                if (getEnemiesAtMyTurret(me).IsValid && E.CanCast(enemy)) E.Cast(enemy, config.Item("packets").GetValue<bool>()); 
            }
            if (config.Item("useSmite").GetValue<bool>() && Jungle.smiteSlot != SpellSlot.Unknown)
            {
                Jungle.setSmiteSlot();
                var target = Jungle.GetNearest(me.Position);
                bool smiteReady = ObjectManager.Player.Spellbook.CanUseSpell(Jungle.smiteSlot) == SpellState.Ready;
                if (target != null)
                {
                    if (Jungle.smite.CanCast(target) && smiteReady && me.Distance(target.Position) <= Jungle.smite.Range && Jungle.smiteDamage() >= target.Health)
                    {
                        Jungle.CastSmite(target);
                    }
                }
            }
        }

        private static void GetPassive()
        {
            var has=false;
            foreach (BuffInstance buff in me.Buffs)
            {
                if (buff.Name == "shenwayoftheninjaaura")
                {
                    has = true;
                }
            }
            if(has){
                  haspassive = true;
            }else{
                  haspassive = false;
            }         
        }

        private static void OnProcessSpell(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs Spell)
        {
        }
        private static void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!config.Item("useeagc").GetValue<bool>()) return;
            if (gapcloser.Sender.IsValidTarget(E.Range) && E.IsReady() && me.Distance(gapcloser.Sender.Position) < 400) E.Cast(gapcloser.End, config.Item("packets").GetValue<bool>());
        }
        private static void OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!config.Item("useeint").GetValue<bool>()) return;
            if (unit.IsValidTarget(E.Range) && E.IsReady()) E.Cast(unit, config.Item("packets").GetValue<bool>());
        }
        private static void Clear()
        {
            var minions = ObjectManager.Get<Obj_AI_Minion>().Where(m => m.IsValidTarget(400)).ToList();
            if (minions.Count() > 2)
                {
                if (Items.HasItem(3077) && Items.CanUseItem(3077))
                Items.UseItem(3077);
                if (Items.HasItem(3074) && Items.CanUseItem(3074))
                Items.UseItem(3074);
                }
        }
        private static void Ulti()
        {

            if (!R.IsReady() || PingCasted || me.IsDead) return;
            
                foreach (var allyObj in ObjectManager.Get<Obj_AI_Hero>().Where(i => i.IsAlly && !i.IsMe && !i.IsDead && ((Checkinrange(i) && ((i.Health * 100 / i.MaxHealth) <= config.Item("atpercent").GetValue<Slider>().Value)) || (CombatHelper.CheckCriticalBuffs(i) && i.CountEnemysInRange(600)<1))))
                {
                    if (config.Item("user").GetValue<bool>() && R.IsReady() && me.CountEnemysInRange((int)E.Range) < 1 && !config.Item("ult" + allyObj.SkinName).GetValue<bool>())
                    {
                        
                        R.Cast(allyObj);
                        return;
                    }
                    else
                    {
                        Game.PrintChat("<font color='#ff0000'> 使用 Ultimate (R) to help: {0}</font>", allyObj.ChampionName);
                           Packet.S2C.Ping.Encoded(new Packet.S2C.Ping.Struct(allyObj.Position.X, allyObj.Position.Y, allyObj.NetworkId, 0, Packet.PingType.Fallback)).Process();
                    }
                    PingCasted = true;
                    Utility.DelayAction.Add(5000, () => PingCasted = false);
                }
            
        }
        private static bool Checkinrange(Obj_AI_Hero i)
        {
            if (i.CountEnemysInRange(750) >= 1 && i.CountEnemysInRange(750) < 3)
            {
                return true;
            }
            return false;
        }
        private static void AutoQ()
        {
            if (!Q.IsReady() || me.HasBuff("Recall")) return;
            if (config.Item("autoqwithe").GetValue<bool>() || config.Item("autoqwithe").GetValue<bool>() && (currEnergy - me.Spellbook.GetSpell(SpellSlot.Q).ManaCost < eEnergy)) return;
            Obj_AI_Hero target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (target.Distance(me.Position) < Q.Range)
            {
                if (target.IsValidTarget(Q.Range))

                    Q.CastOnUnit(target, config.Item("packets").GetValue<bool>());
                    currEnergy -= me.Spellbook.GetSpell(SpellSlot.Q).ManaCost;
            }
        }

        private static void LasthitQ()
        {
            if (config.Item("autoqwithe").GetValue<bool>() && !(currEnergy - me.Spellbook.GetSpell(SpellSlot.Q).ManaCost > eEnergy)) return;
            var allMinions = MinionManager.GetMinions(me.ServerPosition, Q.Range);
            if (config.Item("autoqls").GetValue<bool>() && Q.IsReady())
            {
                foreach (var minion in allMinions)
                {
                    if (minion.IsValidTarget() &&
                    HealthPrediction.GetHealthPrediction(minion,
                    (int)(me.Distance(minion.Position) * 1000 / 1400)) <
                    me.GetSpellDamage(minion, SpellSlot.Q))
                    {
                        Q.CastOnUnit(minion);
                        currEnergy -= me.Spellbook.GetSpell(SpellSlot.Q).ManaCost;
                        return;
                    }
                }
            }
        }
        private static void Harass(){
            if (config.Item("harassqwithe").GetValue<bool>() && !(currEnergy - me.Spellbook.GetSpell(SpellSlot.Q).ManaCost > eEnergy)) return;
            Obj_AI_Hero target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (Q.IsReady() && config.Item("harassq").GetValue<bool>())
            {
                Q.CastOnUnit(target, config.Item("packets").GetValue<bool>());
                currEnergy -= me.Spellbook.GetSpell(SpellSlot.Q).ManaCost;
            }

        }
        private static void Combo()
        {
            
            var minHit = config.Item("useemin").GetValue<Slider>().Value;
            Obj_AI_Hero target = TargetSelector.GetTarget(E.Range+400, TargetSelector.DamageType.Magical);
            if (config.Item("useItems").GetValue<bool>()) ItemHandler.UseItems(target);
            if (target == null) return;
            if (config.Item("usee").GetValue<bool>() && E.IsReady() && me.Distance(target.Position)<E.Range)
            {
                if (minHit > 1)
                {
                    CastEmin(target, minHit);
                }
                else if (me.Distance(target.Position) > me.AttackRange && E.GetPrediction(target).Hitchance >= HitChance.Low)
                {
                    E.Cast(target, config.Item("packets").GetValue<bool>());
                    
                }
            }
            if (Q.IsReady() && config.Item("useq").GetValue<bool>() && me.Distance(target)<Q.Range && currEnergy - me.Spellbook.GetSpell(SpellSlot.Q).ManaCost >= eEnergy)
            {
                Q.CastOnUnit(target, config.Item("packets").GetValue<bool>());
                currEnergy -= me.Spellbook.GetSpell(SpellSlot.Q).ManaCost;
            }
            bool hasIgnite = me.Spellbook.CanUseSpell(me.GetSpellSlot("SummonerDot")) == SpellState.Ready;
            var ignitedmg = (float)me.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
            if (config.Item("useIgnite").GetValue<bool>() && ignitedmg > target.Health && hasIgnite && !E.CanCast(target) && !Q.CanCast(target))
            {
                me.Spellbook.CastSpell(me.GetSpellSlot("SummonerDot"), target);
            }

        }
        public static Obj_AI_Hero getEnemiesAtMyTurret(Obj_AI_Hero me, float distance=750f)
        {
            var myturret =
                ObjectManager.Get<Obj_AI_Turret>()
                    .Where(turret => turret.IsAlly && !turret.IsDead)
                    .OrderBy(turret => me.Distance(turret.Position)).FirstOrDefault();
            return ObjectManager.Get<Obj_AI_Hero>().Where(hero => !hero.IsDead && hero.IsEnemy && myturret.Distance(hero) < distance).OrderBy(hero => hero.Health).FirstOrDefault();
        }

        public static void CastEmin(Obj_AI_Base target, int min)
        {
            foreach (var enemy in ObjectManager.Get<Obj_AI_Base>().Where(i => i.Distance(me)<E.Range && i.IsEnemy && !i.IsDead && !i.IsMinion))
            {
                E.CastIfWillHit(enemy, min-1, config.Item("packets").GetValue<bool>());
                return;

            }
            if (me.Distance(target.Position) > me.AttackRange && E.IsReady())
                {
                    E.CastIfWillHit(target, 0, config.Item("packets").GetValue<bool>());    
                }
        }

        private static void FlashCombo()
        {
            Obj_AI_Hero target = TargetSelector.GetTarget(EFlash.Range, TargetSelector.DamageType.Magical);
            //System.IO.File.AppendAllText(@"C:\Users\Public\TestFolder\PacketLog.txt", "Target: " + target.Position.ToString() + "\n" + "Me: " + me.Position.ToString() + "\n" + "Best: " + getPosToEflash(target.Position).ToString() + "\n");
            if (config.Item("usee").GetValue<bool>() && E.IsReady() && me.Distance(target.Position) < EFlash.Range && me.Distance(target.Position) > 480 && !((getPosToEflash(target.Position)).IsWall()))
            {
                //Game.PrintChat("ok");
                //Packet.S2C.Ping.Encoded(new Packet.S2C.Ping.Struct(target.Position.X, target.Position.Y, me.NetworkId, 0, Packet.PingType.Fallback)).Process();
                me.Spellbook.CastSpell(me.GetSpellSlot("SummonerFlash"), getPosToEflash(target.Position));
                
                E.Cast(target.Position, config.Item("packets").GetValue<bool>());
                
            }
            if (Q.IsReady() && config.Item("useq").GetValue<bool>() && currEnergy - me.Spellbook.GetSpell(SpellSlot.E).ManaCost >= eEnergy)
            {
                Q.CastOnUnit(target, config.Item("packets").GetValue<bool>());
                currEnergy -= me.Spellbook.GetSpell(SpellSlot.Q).ManaCost;
            }
            ItemHandler.UseItems(target);

        }
		public static Vector3 getPosToEflash(Vector3 target)
        {
            return target + (me.Position - target)/2;
        }
        private static float ComboDamage(Obj_AI_Hero hero)
        {
            float damage = 0;
            if (Q.IsReady() && me.Spellbook.GetSpell(SpellSlot.Q).ManaCost < me.Mana)
                damage += (float)Damage.GetSpellDamage(me, hero, SpellSlot.Q);
            if (E.IsReady() && me.Spellbook.GetSpell(SpellSlot.E).ManaCost < me.Mana)
                damage += (float)Damage.GetSpellDamage(me, hero, SpellSlot.E);
            if (Items.HasItem(3077) && Items.CanUseItem(3077))
            {
                damage += (float)me.GetItemDamage(hero, Damage.DamageItems.Tiamat);
            }
            if (Items.HasItem(3074) && Items.CanUseItem(3074))
            {
                damage += (float)me.GetItemDamage(hero, Damage.DamageItems.Hydra);
            }
            if (Items.HasItem(3144) && Items.CanUseItem(3144))
            {
                damage += (float)me.GetItemDamage(hero, Damage.DamageItems.Bilgewater);
            }
            if (Items.HasItem(3153) && Items.CanUseItem(3153))
            {
                damage += (float)me.GetItemDamage(hero, Damage.DamageItems.Botrk);
            }
            if (Items.HasItem(3146) && Items.CanUseItem(3146))
            {
                damage += (float)me.GetItemDamage(hero, Damage.DamageItems.Hexgun);
            }
            if (Items.HasItem(3091) && Items.CanUseItem(3091))
            {
                damage += (float)me.CalcDamage(hero, Damage.DamageType.Magical, 42);
            }
            if (haspassive)
            {
                var passive = me.MaxHealth / 10 + 4+me.Level * 4;
                damage += (float)me.CalcDamage(hero, Damage.DamageType.Magical, passive);
            }
            if (me.Spellbook.CanUseSpell(me.GetSpellSlot("summonerdot")) == SpellState.Ready && hero.Health - damage < (float)me.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite))
            {
                damage += (float)me.GetSummonerSpellDamage(hero, Damage.SummonerSpell.Ignite);
            }
            return damage;
        }
        public static List<Obj_AI_Base> CheckingCollision(Obj_AI_Base From, Obj_AI_Base Target, Spell Skill, bool Mid = true, bool OnlyHero = false)
        {
            var ListCol = new List<Obj_AI_Base>();
            foreach (var Obj in ObjectManager.Get<Obj_AI_Base>().Where(i => IsValid(i, Skill.Range) && Skill.GetPrediction(i).Hitchance >= HitChance.Medium && ((!OnlyHero && !(i is Obj_AI_Turret)) || (OnlyHero && i is Obj_AI_Hero)) && i != Target))
            {
                var Segment = (Mid ? Obj : Target).Position.To2D().ProjectOn(From.Position.To2D(), (Mid ? Target : Obj).Position.To2D());
                if (Segment.IsOnSegment && Obj.Position.Distance(new Vector3(Segment.SegmentPoint.X, Obj.Position.Y, Segment.SegmentPoint.Y)) <= Skill.Width + Obj.BoundingRadius) ListCol.Add(Obj);
            }
            return ListCol.Distinct().ToList();
        }
        
        public static bool IsValid(Obj_AI_Base Target, float 范围 = float.MaxValue, bool EnemyOnly = true, Vector3 From = default(Vector3))
        {
            if (Target == null || !Target.IsValid || Target.IsDead || !Target.IsVisible || (EnemyOnly && !Target.IsTargetable) || (EnemyOnly && Target.IsInvulnerable) || Target.IsMe) return false;
            if (EnemyOnly ? Target.IsAlly : Target.IsEnemy) return false;
            if ((From != default(Vector3) ? From : me.Position).Distance(Target.Position) > 范围) return false;
            return true;
        }
        private static void OnCreate(GameObject sender, EventArgs args)
        {
            if (config.Item("autowwithe").GetValue<bool>() && !(currEnergy - me.Spellbook.GetSpell(SpellSlot.W).ManaCost > eEnergy)) return;
            if (sender is Obj_SpellMissile && sender.IsValid && config.Item("autow").GetValue<bool>() && W.IsReady())
            {
                var missle = (Obj_SpellMissile)sender;
                var caster = missle.SpellCaster;
                if (caster.IsEnemy)
                {
                    var ShieldBuff = new Int32[] { 60, 100, 140, 180, 200 }[W.Level - 1] + 0.6 * me.FlatMagicDamageMod;
                    if (missle.SData.Name.Contains("BasicAttack"))
                    {
                        if (missle.Target.IsMe && ShieldBuff / 100 * config.Item("wabove").GetValue<Slider>().Value < caster.GetAutoAttackDamage(me, true))
                        {
                            W.Cast();
                            currEnergy -= me.Spellbook.GetSpell(SpellSlot.W).ManaCost;
                        }
                    }
                    else if (missle.Target.IsMe || missle.EndPosition.Distance(me.Position) <= 130)
                    {
                        if (missle.SData.Name == "summonerdot")
                        {
                            if (me.Health <= (caster as Obj_AI_Hero).GetSummonerSpellDamage(me, Damage.SummonerSpell.Ignite) && ShieldBuff < (caster as Obj_AI_Hero).GetSummonerSpellDamage(me, Damage.SummonerSpell.Ignite))
                            {
                                W.Cast();
                                currEnergy -= me.Spellbook.GetSpell(SpellSlot.W).ManaCost;
                            }
                        }
                        else if (ShieldBuff / 100 * config.Item("wabove").GetValue<Slider>().Value < (caster as Obj_AI_Hero).GetSpellDamage(me, (caster as Obj_AI_Hero).GetSpellSlot(missle.SData.Name), 1))
                        {
                            W.Cast();
                            currEnergy -= me.Spellbook.GetSpell(SpellSlot.W).ManaCost;
                        }
                    }
                }
            }
        }
        private static void InitShen()
        {
            Q = new Spell(SpellSlot.Q, 475);
            Q.SetTargetted(0.5f, 1500f);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 600);
            E.SetSkillshot(0f, 50f, 1600f, false, SkillshotType.SkillshotLine);
            EFlash = new Spell(SpellSlot.E, 990);
            EFlash.SetSkillshot(E.Instance.SData.SpellCastTime, E.Instance.SData.LineWidth, E.Speed, false, SkillshotType.SkillshotLine);
            R = new Spell(SpellSlot.R, float.MaxValue);
        }
        private static void InitMenu()
        {
            config = new Menu("【無為汉化】Shen", "SRS_Shen", true);
            // Target Selector
            Menu menuTS = new Menu("目标选择", "tselect");
            TargetSelector.AddToMenu(menuTS);
            config.AddSubMenu(menuTS);

            // Orbwalker
            Menu menuOrb = new Menu("走砍", "orbwalker");
            orbwalker = new Orbwalking.Orbwalker(menuOrb);
            config.AddSubMenu(menuOrb);

            // 显示 settings
            Menu menuD = new Menu("范围 ", "dsettings");
            menuD.AddItem(new MenuItem("drawaa", "显示 AA 范围")).SetValue(new Circle(false, Color.FromArgb(150, 150, 62, 172)));
            menuD.AddItem(new MenuItem("drawqq", "显示 Q 范围")).SetValue(new Circle(false, Color.FromArgb(150, 150, 62, 172)));
            menuD.AddItem(new MenuItem("drawee", "显示 E 范围")).SetValue(new Circle(false, Color.FromArgb(150, 150, 62, 172)));
            menuD.AddItem(new MenuItem("draweeflash", "显示 E+闪现范围")).SetValue(new Circle(true, Color.FromArgb(50, 250, 248, 110)));
            menuD.AddItem(new MenuItem("drawallyhp", "显示队友血量")).SetValue(true);
            menuD.AddItem(new MenuItem("drawincdmg", "显示伤害")).SetValue(true);
            menuD.AddItem(new MenuItem("drawcombo", "显示连招伤害")).SetValue(true);
            config.AddSubMenu(menuD);

            // Combo Settings
            Menu menuC = new Menu("连招", "csettings");
            menuC.AddItem(new MenuItem("useq", "使用 Q")).SetValue(true);
            menuC.AddItem(new MenuItem("usew", "使用 W")).SetValue(true);
            menuC.AddItem(new MenuItem("usee", "使用 E")).SetValue(true);
            menuC.AddItem(new MenuItem("useemin", "尝试使用 E 最小人数")).SetValue(new Slider(1, 1, 5));
            menuC.AddItem(new MenuItem("useItems", "使用物品")).SetValue(true);
            menuC.AddItem(new MenuItem("useIgnite", "使用点燃")).SetValue(true);
            config.AddSubMenu(menuC);

            // Misc Settings
            Menu menuU = new Menu("杂项 ", "usettings");
            menuU.AddItem(new MenuItem("harassq", "骚扰 Q")).SetValue(true);
            menuU.AddItem(new MenuItem("harassqwithe", "保持能量 E")).SetValue(true);
            menuU.AddItem(new MenuItem("autoqls", "补兵 Q")).SetValue(true);
            menuU.AddItem(new MenuItem("autoqwithe", "保持能量 E")).SetValue(true);
            menuU.AddItem(new MenuItem("autow", "试图阻止非射杀法术")).SetValue(true);
            menuU.AddItem(new MenuItem("wabove", "盾屏蔽最小伤害 %")).SetValue(new Slider(50, 0, 100));
            menuU.AddItem(new MenuItem("autowwithe", "保持能量 E")).SetValue(true);
            menuU.AddItem(new MenuItem("autotauntattower", "塔下自动嘲讽范围")).SetValue(true);
            menuU.AddItem(new MenuItem("useeagc", "使用 E 反突进")).SetValue(false);
            menuU.AddItem(new MenuItem("useeint", "使用 E 打断技能")).SetValue(true);
            menuU.AddItem(new MenuItem("useeflash", "闪现+E")).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press));
            menuU.AddItem(new MenuItem("user", "使用 R")).SetValue(true);
            menuU.AddItem(new MenuItem("atpercent", "队友血量")).SetValue(new Slider(20, 0, 100));
            config.AddSubMenu(menuU);
            var sulti = new Menu("不给大招 ", "dontult");
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsAlly))
            {
                if (hero.SkinName != me.SkinName)
                    sulti.AddItem(new MenuItem("ult" + hero.SkinName, hero.SkinName)).SetValue(false);
            }
            config.AddSubMenu(sulti);
            config.AddItem(new MenuItem("packets", "使用封包")).SetValue(false);
            config.AddToMainMenu();
        }
    }
}
