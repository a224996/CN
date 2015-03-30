#region

using System;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

#endregion

namespace Sion
{
    internal class Program
    {
        private static Menu Config;

        public static Orbwalking.Orbwalker Orbwalker;

        public static Spell Q;
        public static Spell E;

        public static Vector2 QCastPos = new Vector2();

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.BaseSkinName != "Sion")
            {
                return;
            }

            //Spells
            Q = new Spell(SpellSlot.Q, 1050);
            Q.SetSkillshot(0.6f, 100f, float.MaxValue, false, SkillshotType.SkillshotLine);
            Q.SetCharged("SionQ", "SionQ", 500, 720, 0.5f);

            E = new Spell(SpellSlot.E, 800);
            E.SetSkillshot(0.25f, 80f, 1800, false, SkillshotType.SkillshotLine);

            //Make the menu
            Config = new Menu("【初见汉化】塞恩", "Sion", true);

            //Orbwalker submenu
            Config.AddSubMenu(new Menu("|初见汉化-走砍|", "Orbwalking"));

            //Add the target selector to the menu as submenu.
            var targetSelectorMenu = new Menu("|初见汉化-目标选择|", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            //Load the orbwalker and add it to the menu as submenu.
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            //Combo menu:
            Config.AddSubMenu(new Menu("|初见汉化-连招|", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "使用 Q").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "使用 W").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "使用 E").SetValue(true));
            Config.SubMenu("Combo")
                .AddItem(
                    new MenuItem("ComboActive", "连招").SetValue(
                        new KeyBind(Config.Item("Orbwalk").GetValue<KeyBind>().Key, KeyBindType.Press)));


            Config.AddSubMenu(new Menu("|初见汉化-R设置|", "R"));
            Config.SubMenu("R").AddItem(new MenuItem("AntiCamLock", "避免镜头锁定").SetValue(true));
            Config.SubMenu("R").AddItem(new MenuItem("MoveToMouse", "移动鼠标（利用漏洞）").SetValue(false));//Disabled by default since its not legit Keepo
            Config.AddSubMenu(new Menu("|初见汉化-群号|", "by chujian"));

Config.SubMenu("by chujian").AddItem(new MenuItem("qunhao", "汉化群：386289593"));
Config.SubMenu("by chujian").AddItem(new MenuItem("qunhao1", "交流群：333399"));

            Config.AddToMainMenu();

            Game.PrintChat("Sion Loaded!");
            Game.OnUpdate += Game_OnGameUpdate;
            Game.OnProcessPacket += Game_OnGameProcessPacket;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnProcessSpellCast += ObjAiHeroOnOnProcessSpellCast;
        }


        private static void ObjAiHeroOnOnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && args.SData.Name == "SionQ")
            {
                QCastPos = args.End.To2D();
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            Utility.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.White);
        }

        private static void Game_OnGameProcessPacket(GamePacketEventArgs args)
        {
            if (Config.Item("AntiCamLock").GetValue<bool>() && args.PacketData[0] == 0x07)
            {
                var gp = new GamePacket(args.PacketData);
                if (gp.ReadInteger(2) == ObjectManager.Player.NetworkId && gp.ReadByte(8) == 0x61 &&gp.ReadByte(8) == 0x44)
                {
                    args.Process = false;
                }
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            //Casting R
            if (ObjectManager.Player.HasBuff("SionR"))
            {
                if (Config.Item("MoveToMouse").GetValue<bool>())
                {
                    var p = ObjectManager.Player.Position.To2D().Extend(Game.CursorPos.To2D(), 500);
                    ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, p.To3D());
                }
                return;
            }

            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                var qTarget = TargetSelector.GetTarget(
                    !Q.IsCharging ? Q.ChargedMaxRange / 2 : Q.ChargedMaxRange, TargetSelector.DamageType.Physical);

                var eTarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);

                if (qTarget != null && Config.Item("UseQCombo").GetValue<bool>())
                {
                    if (Q.IsCharging)
                    {
                        var start = ObjectManager.Player.ServerPosition.To2D();
                        var end = start.Extend(QCastPos, Q.Range);
                        var direction = (end - start).Normalized();
                        var normal = direction.Perpendicular();

                        var points = new List<Vector2>();
                        var hitBox = qTarget.BoundingRadius;
                        points.Add(start + normal * (Q.Width + hitBox));
                        points.Add(start - normal * (Q.Width + hitBox));
                        points.Add(end + Q.ChargedMaxRange * direction - normal * (Q.Width + hitBox));
                        points.Add(end + Q.ChargedMaxRange * direction + normal * (Q.Width + hitBox));

                        for (var i = 0; i <= points.Count - 1; i++)
                        {
                            var A = points[i];
                            var B = points[i == points.Count - 1 ? 0 : i + 1];

                            if (qTarget.ServerPosition.To2D().Distance(A, B, true, true) < 50 * 50)
                            {
                                Packet.C2S.ChargedCast.Encoded(
                                    new Packet.C2S.ChargedCast.Struct(
                                        (SpellSlot) ((byte) Q.Slot), Game.CursorPos.X, Game.CursorPos.X,
                                        Game.CursorPos.X)).Send();
                            }
                        }
                        return;
                    }

                    if (Q.IsReady())
                    {
                        Q.StartCharging(qTarget.ServerPosition);
                    }
                }

                if (qTarget != null && Config.Item("UseWCombo").GetValue<bool>())
                {
                    ObjectManager.Player.Spellbook.CastSpell(SpellSlot.W, ObjectManager.Player);
                }

                if (eTarget != null && Config.Item("UseECombo").GetValue<bool>())
                {
                    E.Cast(eTarget);
                }
            }
        }
    }
}
