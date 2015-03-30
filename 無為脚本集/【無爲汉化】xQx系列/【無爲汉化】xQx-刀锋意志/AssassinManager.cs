using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace Irelia
{
    internal class AssassinManager
    {
        public AssassinManager()
        {
            Load();
        }

        private static void Load()
        {
            Program.Config.AddSubMenu(new Menu("�̿�ģʽ", "MenuAssassin"));
            Program.Config.SubMenu("MenuAssassin").AddItem(new MenuItem("AssassinActive", "����").SetValue(true));
            
            foreach (
                var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != ObjectManager.Player.Team))
            {
                Program.Config.SubMenu("MenuAssassin")
                    
                    .AddItem(
                        new MenuItem("Assassin" + enemy.ChampionName, enemy.ChampionName).SetValue(
                            TargetSelector.GetPriority(enemy) > 3));
            }


            Program.Config.SubMenu("MenuAssassin")
                .AddItem(
                    new MenuItem("AssassinSelectOption", "��: ").SetValue(
                        new StringList(new[] { "��ѡ��", "��ѡ��" })));
            Program.Config.SubMenu("MenuAssassin")
                .AddItem(new MenuItem("AssassinSetClick", "���/ɾ�����").SetValue(true));
            Program.Config.SubMenu("MenuAssassin")
                .AddItem(
                    new MenuItem("AssassinReset", "�����б�").SetValue(new KeyBind("T".ToCharArray()[0],
                        KeyBindType.Press)));

            Program.Config.SubMenu("MenuAssassin").AddSubMenu(new Menu("����:", "Draw"));

            Program.Config.SubMenu("MenuAssassin")
                .SubMenu("Draw")
                .AddItem(new MenuItem("DrawSearch", "������Χ").SetValue(new Circle(true, Color.GreenYellow)));
            Program.Config.SubMenu("MenuAssassin")
                .SubMenu("Draw")
                .AddItem(new MenuItem("DrawActive", "��Ծ�ĵ���").SetValue(new Circle(true, Color.GreenYellow)));
            Program.Config.SubMenu("MenuAssassin")
                .SubMenu("Draw")
                .AddItem(new MenuItem("DrawNearest", "����ĵ���").SetValue(new Circle(true, Color.DarkSeaGreen)));
            Program.Config.SubMenu("MenuAssassin")
                .SubMenu("Draw")
                .AddItem(new MenuItem("DrawStatus", "��ʾ״̬").SetValue(true));


            
            Program.Config.SubMenu("MenuAssassin")
                .AddItem(new MenuItem("AssassinSearchRange", "������Χ")).SetValue(new Slider(1000, 2000));

            Game.OnUpdate += OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnWndProc += Game_OnWndProc;
        }

        static void ClearAssassinList()
        {
            foreach (
                var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != ObjectManager.Player.Team))
            {
                Program.Config.Item("Assassin" + enemy.ChampionName).SetValue(false);
            }
        }
        private static void OnGameUpdate(EventArgs args)
        {
        }

        private static void Game_OnWndProc(WndEventArgs args)
        {

            if (Program.Config.Item("AssassinReset").GetValue<KeyBind>().Active && args.Msg == 257)
            {
                ClearAssassinList();
                Game.PrintChat(
                    "<font color='#FFFFFF'>重置刺客的清单是完整的！点击敌人添加/删除.</font>");
            }

            if (args.Msg != (uint)WindowsMessages.WM_LBUTTONDOWN)
            {
                return;
            }

            if (Program.Config.Item("AssassinSetClick").GetValue<bool>())
            {
                foreach (var objAiHero in from hero in ObjectManager.Get<Obj_AI_Hero>()
                                          where hero.IsValidTarget()
                                          select hero
                                              into h
                                              orderby h.Distance(Game.CursorPos) descending
                                              select h
                                                  into enemy
                                                  where enemy.Distance(Game.CursorPos) < 150f
                                                  select enemy)
                {
                    if (objAiHero != null && objAiHero.IsVisible && !objAiHero.IsDead)
                    {
                        var xSelect =
                            Program.Config.Item("AssassinSelectOption").GetValue<StringList>().SelectedIndex;

                        switch (xSelect)
                        {
                            case 0:
                                ClearAssassinList();
                                Program.Config.Item("Assassin" + objAiHero.ChampionName).SetValue(true);
                                Game.PrintChat(
                                    string.Format(
                                        "<font color='FFFFFF'>加入暗杀名单</font> <font color='#09F000'>{0} ({1})</font>",
                                        objAiHero.Name, objAiHero.ChampionName));
                                break;
                            case 1:
                                var menuStatus =
                                    Program.Config.Item("Assassin" + objAiHero.ChampionName)
                                        .GetValue<bool>();
                                Program.Config.Item("Assassin" + objAiHero.ChampionName)
                                    .SetValue(!menuStatus);
                                Game.PrintChat(
                                    string.Format("<font color='{0}'>{1}</font> <font color='#09F000'>{2} ({3})</font>",
                                        !menuStatus ? "#FFFFFF" : "#FF8877",
                                        !menuStatus ? "加入暗杀名单:" : "从暗杀名单中删除︱:",
                                        objAiHero.Name, objAiHero.ChampionName));
                                break;
                        }
                    }
                }
            }
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Program.Config.Item("AssassinActive").GetValue<bool>())
                return;

            if (Program.Config.Item("DrawStatus").GetValue<bool>())
            {
                var enemies = ObjectManager.Get<Obj_AI_Hero>().Where(xEnemy => xEnemy.IsEnemy);
                var objAiHeroes = enemies as Obj_AI_Hero[] ?? enemies.ToArray();
                Drawing.DrawText(Drawing.Width * 0.89f, Drawing.Height * 0.58f, Color.GreenYellow, "刺杀名单");
                Drawing.DrawText(Drawing.Width * 0.89f, Drawing.Height * 0.58f, Color.GhostWhite, "__________");
                for (int i = 0; i < objAiHeroes.Count(); i++)
                {
                    var xCaption = objAiHeroes[i].ChampionName;
                    var xWidth = Drawing.Width*0.90f;
                    if (Program.Config.Item("Assassin" + objAiHeroes[i].ChampionName).GetValue<bool>())
                    {
                        xCaption = "+ " + xCaption;
                        xWidth = Drawing.Width*0.8910f;

                    }
                    Drawing.DrawText(xWidth, Drawing.Height*0.58f + (float) (i + 1)*15, Color.Gainsboro,
                        xCaption);
                }
            }

            var drawSearch = Program.Config.Item("DrawSearch").GetValue<Circle>();
            var drawActive = Program.Config.Item("DrawActive").GetValue<Circle>();
            var drawNearest = Program.Config.Item("DrawNearest").GetValue<Circle>();

            var drawSearchRange = Program.Config.Item("AssassinSearchRange").GetValue<Slider>().Value;
            if (drawSearch.Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, drawSearchRange, drawSearch.Color);
            }

            foreach (
                var enemy in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(enemy => enemy.Team != ObjectManager.Player.Team)
                        .Where(
                            enemy =>
                                enemy.IsVisible &&
                                Program.Config.Item("Assassin" + enemy.ChampionName) != null &&
                                !enemy.IsDead)
                        .Where(
                            enemy => Program.Config.Item("Assassin" + enemy.ChampionName).GetValue<bool>()))
            {
                if (ObjectManager.Player.Distance(enemy) < drawSearchRange)
                {
                    if (drawActive.Active)
                        Render.Circle.DrawCircle(enemy.Position, 85f, drawActive.Color);
                }
                else if (ObjectManager.Player.Distance(enemy) > drawSearchRange &&
                         ObjectManager.Player.Distance(enemy) < drawSearchRange + 400)
                {
                    if (drawNearest.Active)
                        Render.Circle.DrawCircle(enemy.Position, 85f, drawNearest.Color);
                }
            }
        }
    }
}
