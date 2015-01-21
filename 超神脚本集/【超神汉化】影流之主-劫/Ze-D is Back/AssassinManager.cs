using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace Zed
{
    internal class AssassinManager
    {
        public AssassinManager()
        {
            Load();
        }

        private static void Load()
        {
            Program.TargetSelectorMenu.AddSubMenu(new Menu("刺客模式", "MenuAssassin"));
            Program.TargetSelectorMenu.SubMenu("MenuAssassin").AddItem(new MenuItem("AssassinActive", "启用").SetValue(true));
            Program.TargetSelectorMenu.SubMenu("MenuAssassin").AddItem(new MenuItem("Ax", ""));
            Program.TargetSelectorMenu.SubMenu("MenuAssassin").AddItem(new MenuItem("AssassinSelectOption", "选择方式: ").SetValue(new StringList(new[] {"单选项", "多选项"})));
            Program.TargetSelectorMenu.SubMenu("MenuAssassin").AddItem(new MenuItem("Ax", ""));
            Program.TargetSelectorMenu.SubMenu("MenuAssassin").AddItem(new MenuItem("AssassinSetClick", "用右键单击添加/删除").SetValue(true));
            Program.TargetSelectorMenu.SubMenu("MenuAssassin").AddItem(new MenuItem("AssassinReset", "重置列表").SetValue(new KeyBind("L".ToCharArray()[0],KeyBindType.Press)));

            Program.TargetSelectorMenu.SubMenu("MenuAssassin").AddSubMenu(new Menu("绘制:", "Draw"));

            Program.TargetSelectorMenu.SubMenu("MenuAssassin").SubMenu("Draw").AddItem(new MenuItem("DrawSearch", "搜索范围").SetValue(new Circle(true,Color.Gray)));
            Program.TargetSelectorMenu.SubMenu("MenuAssassin").SubMenu("Draw").AddItem(new MenuItem("DrawActive", "活跃的敌人").SetValue(new Circle(true,Color.GreenYellow)));
            Program.TargetSelectorMenu.SubMenu("MenuAssassin").SubMenu("Draw").AddItem(new MenuItem("DrawNearest", "附近的敌人").SetValue(new Circle(true,Color.DarkSeaGreen)));
            

            Program.TargetSelectorMenu.SubMenu("MenuAssassin").AddSubMenu(new Menu("刺杀名单:", "AssassinMode"));
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != ObjectManager.Player.Team))
            {
                Program.TargetSelectorMenu.SubMenu("MenuAssassin")
                    .SubMenu("AssassinMode")
                    .AddItem(
                        new MenuItem("Assassin" + enemy.BaseSkinName, enemy.BaseSkinName).SetValue(
                            TargetSelector.GetPriority(enemy) > 3));
            }
            Program.TargetSelectorMenu.SubMenu("MenuAssassin")
                .AddItem(new MenuItem("AssassinSearchRange", "搜索范围")).SetValue(new Slider(1000, 2000));

            Game.OnGameUpdate += OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnWndProc += Game_OnWndProc;
        }

        static void ClearAssassinList()
        {
            foreach (
                var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsEnemy)) 
            {
                
                Program.TargetSelectorMenu.Item("Assassin" + enemy.BaseSkinName).SetValue(false);
                
            }
        }
        private static void OnGameUpdate(EventArgs args)
        {
        }

        private static void Game_OnWndProc(WndEventArgs args)
        {
            
            if (Program.TargetSelectorMenu.Item("AssassinReset").GetValue<KeyBind>().Active && args.Msg == 257)
            {
                ClearAssassinList();
                Game.PrintChat(
                    "<font color='#FFFFFF'>閲嶇疆鍒哄鐨勬竻鍗曟槸瀹屾暣鐨勶紒鐐瑰嚮鏁屼汉娣诲姞/鍒犻櫎.</font>");
            }

            if (args.Msg != 0x201)
            {
                return;
            }

            if (Program.TargetSelectorMenu.Item("AssassinSetClick").GetValue<bool>())
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
                            Program.TargetSelectorMenu.Item("AssassinSelectOption").GetValue<StringList>().SelectedIndex;

                        switch (xSelect)
                        {
                            case 0:
                                ClearAssassinList();
                                Program.TargetSelectorMenu.Item("Assassin" + objAiHero.BaseSkinName).SetValue(true);
                                Game.PrintChat(
                                    string.Format(
                                        "<font color='FFFFFF'>鍔犲叆鏆楁潃鍚嶅崟</font> <font color='#09F000'>{0} ({1})</font>",
                                        objAiHero.Name, objAiHero.BaseSkinName));
                                break;
                            case 1:
                                var menuStatus = Program.TargetSelectorMenu.Item("Assassin" + objAiHero.BaseSkinName).GetValue<bool>();
                                Program.TargetSelectorMenu.Item("Assassin" + objAiHero.BaseSkinName).SetValue(!menuStatus);
                                Game.PrintChat(
                                    string.Format("<font color='{0}'>{1}</font> <font color='#09F000'>{2} ({3})</font>",
                                        !menuStatus ? "#FFFFFF" : "#FF8877",
                                        !menuStatus ? "鍔犲叆鏆楁潃鍚嶅崟:" : "浠庢殫鏉�鍚嶅崟涓垹闄わ副:",
                                        objAiHero.Name, objAiHero.BaseSkinName));
                                break;
                        }
                    }
                }
            }
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Program.TargetSelectorMenu.Item("AssassinActive").GetValue<bool>())
                return;

            var drawSearch = Program.TargetSelectorMenu.Item("DrawSearch").GetValue<Circle>();
            var drawActive = Program.TargetSelectorMenu.Item("DrawActive").GetValue<Circle>();
            var drawNearest = Program.TargetSelectorMenu.Item("DrawNearest").GetValue<Circle>();

            var drawSearchRange = Program.TargetSelectorMenu.Item("AssassinSearchRange").GetValue<Slider>().Value;
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
                                Program.TargetSelectorMenu.Item("Assassin" + enemy.BaseSkinName) != null &&
                                !enemy.IsDead)
                        .Where(
                            enemy => Program.TargetSelectorMenu.Item("Assassin" + enemy.BaseSkinName).GetValue<bool>()))
            {
                if (ObjectManager.Player.Distance(enemy.ServerPosition) < drawSearchRange)
                {
                    if (drawActive.Active)
                        Render.Circle.DrawCircle(enemy.Position, 85f, drawActive.Color);
                }
                else if (ObjectManager.Player.Distance(enemy.ServerPosition) > drawSearchRange &&
                         ObjectManager.Player.Distance(enemy.ServerPosition) < drawSearchRange + 400) 
                {
                    if (drawNearest.Active)
                        Render.Circle.DrawCircle(enemy.Position, 85f, drawNearest.Color);
                }
            }
        }
    }
}
