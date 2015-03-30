using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace Viktor
{
    internal class Config
    {
        public static Menu ViktorConfig;
        public static Menu TargetSelectorMenu;
        public static Orbwalking.Orbwalker Orbwalker;
        public static void Init()
        {
            ViktorConfig = new Menu("【無為汉化】阿波罗-维克托", "apollo.viktor", true);

            //Orbwalker
            ViktorConfig.AddSubMenu(new Menu("走砍", "apollo.viktor.orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(ViktorConfig.SubMenu("apollo.viktor.orbwalker"));

            //Targetselector
            TargetSelectorMenu = new Menu("目标选择", "apollo.viktor.targettelector");
            TargetSelector.AddToMenu(TargetSelectorMenu);
            ViktorConfig.AddSubMenu(TargetSelectorMenu);

            //Combo
            var combo = new Menu("连招", "apollo.viktor.combo");
            {
                var q = new Menu("Q", "apollo.viktor.combo.q");
                q.AddItem(new MenuItem("apollo.viktor.combo.q.bool", "连招使用").SetValue(true));
                q.AddItem(new MenuItem("apollo.viktor.combo.q.dont", "只有在AA范围内使用").SetValue(false));
                combo.AddSubMenu(q);

                var w = new Menu("W", "apollo.viktor.combo.w");
                w.AddItem(new MenuItem("apollo.viktor.combo.w.bool", "连招使用").SetValue(true));
                w.AddItem(new MenuItem("apollo.viktor.combo.w.stunned", "对晕倒的敌人").SetValue(true));
                w.AddItem(new MenuItem("apollo.viktor.combo.w.slow", "对被减速的敌人").SetValue(true));
                w.AddItem(new MenuItem("apollo.viktor.combo.w.hit", "最少敌人数").SetValue(new Slider(3, 1, 5)));
                combo.AddSubMenu(w);

                var e = new Menu("E", "apollo.viktor.combo.e");
                e.AddItem(new MenuItem("apollo.viktor.combo.e.bool", "连招使用").SetValue(true));
                e.AddItem(new MenuItem("apollo.viktor.combo.e.pre", "E 命中率").SetValue(new StringList((new[] { "低", "正常", "高", "非常高" }), 2)));
                combo.AddSubMenu(e);

                var r = new Menu("R", "apollo.viktor.combo.r");
                r.AddItem(new MenuItem("apollo.viktor.combo.r.bool", "连招使用").SetValue(true));
                r.AddItem(new MenuItem("apollo.viktor.combo.r.kill", "如果可击杀敌人").SetValue(true));
                r.AddItem(new MenuItem("apollo.viktor.combo.r.hit", "最少敌人数").SetValue(new Slider(3, 1, 5)));
                r.AddItem(
                    new MenuItem("apollo.viktor.combo.r.minhp", "如果目标血量%").SetValue(
                        new Slider(10)));
                combo.AddSubMenu(r);

                combo.AddItem(new MenuItem("apollo.viktor.combo.ignite.bool", "使用点燃").SetValue(true));

                ViktorConfig.AddSubMenu(combo);
            }

            //Harass
            var harass = new Menu("骚扰", "apollo.viktor.harass");
            {
                var q = new Menu("Q", "apollo.viktor.harass.q");
                q.AddItem(new MenuItem("apollo.viktor.harass.q.bool", "骚扰使用").SetValue(true));
                harass.AddSubMenu(q);

                var e = new Menu("E", "apollo.viktor.harass.e");
                e.AddItem(new MenuItem("apollo.viktor.harass.e.bool", "骚扰使用").SetValue(true));
                e.AddItem(new MenuItem("apollo.viktor.harass.e.pre", "E 命中率").SetValue(new StringList((new[] { "低", "正常", "高", "非常高" }), 3)));
                harass.AddSubMenu(e);

                harass.AddItem(
                    new MenuItem("apollo.viktor.harass.key", "骚扰锁定").SetValue(
                        new KeyBind('T', KeyBindType.Toggle)));
                harass.AddItem(new MenuItem("apollo.viktor.harass.mana", "蓝量%").SetValue(new Slider(30)));

                ViktorConfig.AddSubMenu(harass);
            }

            //Laneclear
            var laneclear = new Menu("发育", "apollo.viktor.laneclear");
            {
                var q = new Menu("Q", "apollo.viktor.laneclear.q");
                q.AddItem(new MenuItem("apollo.viktor.laneclear.q.bool", "清兵使用").SetValue(true));
                q.AddItem(new MenuItem("apollo.viktor.laneclear.q.lasthit", "补兵使用").SetValue(false));
                q.AddItem(new MenuItem("apollo.viktor.laneclear.q.canon", "清野使用").SetValue(true));
                laneclear.AddSubMenu(q);

                var e = new Menu("E", "apollo.viktor.laneclear.e");
                e.AddItem(new MenuItem("apollo.viktor.laneclear.e.bool", "清兵使用").SetValue(true));
                e.AddItem(
                    new MenuItem("apollo.viktor.laneclear.e.hit", "最少小兵").SetValue(new Slider(3, 1, 10)));
                laneclear.AddSubMenu(e);

                laneclear.AddItem(new MenuItem("apollo.viktor.laneclear.mana", "蓝量%").SetValue(new Slider(30)));

                ViktorConfig.AddSubMenu(laneclear);
            }

            //Killsteal
            var killsteal = new Menu("抢人头", "apollo.viktor.ks");
            {
                killsteal.AddItem(new MenuItem("apollo.viktor.ks.e.bool", "使用 E").SetValue(true));

                ViktorConfig.AddSubMenu(killsteal);
            }

            //AntiGapcloser
            var gapcloser = new Menu("防突进", "apollo.viktor.gapcloser");
            {
                gapcloser.AddItem(new MenuItem("apollo.viktor.gapcloser.w.bool", "使用 W").SetValue(true));
                ViktorConfig.AddSubMenu(gapcloser);
            }

            //Interrupter
            var interrupter = new Menu("打断技能", "apollo.viktor.interrupt");
            {
                interrupter.AddItem(new MenuItem("apollo.viktor.interrupt.w.bool", "使用 W").SetValue(true));
                interrupter.AddItem(new MenuItem("apollo.viktor.interrupt.r.bool", "使用 R").SetValue(false));

                ViktorConfig.AddSubMenu(interrupter);
            }

            //Drawings
            var draw = new Menu("显示", "apollo.viktor.draw");
            {
                draw.AddItem(
                    new MenuItem("apollo.viktor.draw.q", "显示 Q 范围").SetValue(new Circle(true, Color.AntiqueWhite)));
                draw.AddItem(
                    new MenuItem("apollo.viktor.draw.w", "显示 W 范围").SetValue(new Circle(true, Color.AntiqueWhite)));
                draw.AddItem(
                    new MenuItem("apollo.viktor.draw.e", "显示 E 范围").SetValue(new Circle(true, Color.AntiqueWhite)));
                draw.AddItem(
                    new MenuItem("apollo.viktor.draw.r", "显示 R 范围").SetValue(new Circle(true, Color.AntiqueWhite)));
                draw.AddItem(
                    new MenuItem("apollo.viktor.draw.cd", "显示CD").SetValue(new Circle(false, Color.DarkRed)));
                draw.AddItem(
                    new MenuItem("apollo.viktor.draw.ind.bool", "显示连招伤害", true).SetValue(true));
                draw.AddItem(
                    new MenuItem("apollo.viktor.draw.ind.fill", "显示连招补充伤害", true).SetValue(
                        new Circle(true, Color.FromArgb(90, 255, 169, 4))));

                ViktorConfig.AddSubMenu(draw);
            }

            //PacketCast
            {
                ViktorConfig.AddItem(new MenuItem("apollo.viktor.packetcast", "封包").SetValue(false));
            }

            ViktorConfig.AddToMainMenu();
        }
    }
}
