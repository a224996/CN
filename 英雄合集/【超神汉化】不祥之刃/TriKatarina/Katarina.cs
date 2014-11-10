using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using TriKatarina.Logic;
using TriKatarina.Logic.Thoughts;
using Triton;
using Triton.Constants;
using Triton.Logic;
using Triton.Plugins;

namespace TriKatarina
{
    class Katarina : ChampionPluginBase
    {

        private int _wardJumpRange = 600;
        private Brain _brain;
        private static Katarina _instance;
        private ThoughtContext _thoughtContext;

        public static Katarina Instance
        {
            get { return _instance ?? (_instance = new Katarina()); }
        }

        public void Run()
        {
        }

        public override bool Initialize()
        {
            if (!base.Initialize())
                return false;
            
            _thoughtContext = new ThoughtContext() {Plugin = this};

            _brain = new Brain();

            _brain.Thoughts.Add(new AnalyzeThought());
            _brain.Thoughts.Add(new AcquireTargetThought());
            _brain.Thoughts.Add(new MoveToMouseThought());
            _brain.Thoughts.Add(new KillStealThought());
            _brain.Thoughts.Add(new WardJumpThought());
            _brain.Thoughts.Add(new FullComboThought());
            _brain.Thoughts.Add(new HarassThought());
            _brain.Thoughts.Add(new FarmThought());

            return true;
        }

        public override void SetupConfig()
        {
            base.SetupConfig();
            
            _config.AddSubMenu(new Menu("璧扮爫", "Orbwalking", false));

            Menu comboMenu = new Menu("杩炴嫑", "Combo");
            comboMenu.AddItem(new MenuItem("ComboKey", "杩炴嫑").SetValue(new KeyBind(32, KeyBindType.Press)));
            comboMenu.AddItem(new MenuItem("StopUlt", "鐩爣姝讳骸鍋滅敤R").SetValue(true));
            comboMenu.AddItem(new MenuItem("AutoE", "璺濈涓嶅鑷姩E").SetValue(false));
            comboMenu.AddItem(new MenuItem("ComboDetonateQ", "婵€娲籕鏍囪").SetValue(false));
            comboMenu.AddItem(new MenuItem("ComboItems", "浣跨敤鐗╁搧").SetValue(false));
            comboMenu.AddItem(new MenuItem("ComboMoveToMouse", "杩炴嫑璺熼殢榧犳爣").SetValue(true));

            Menu harassMenu = new Menu("楠氭壈", "Harass");
            harassMenu.AddItem(new MenuItem("HarassKey", "楠氭壈").SetValue(new KeyBind('C', KeyBindType.Press)));
            harassMenu.AddItem(new MenuItem("HarrassQWE", "妯″紡").SetValue(new StringList(new string[] { "Q+W+E", "Q+W" })));
            harassMenu.AddItem(new MenuItem("HarassDetonateQ", "婵€娲籕鏍囪").SetValue(true));
            harassMenu.AddItem(new MenuItem("WHarass", "浣跨敤W ").SetValue(true));
            harassMenu.AddItem(new MenuItem("HarassMoveToMouse", "楠氭壈璺熼殢榧犳爣").SetValue(true));

            Menu drawMenu = new Menu("鏄剧ず", "Drawing");
            drawMenu.AddItem(new MenuItem("DisableAllDrawing", "绂佺敤鏄剧ず").SetValue(false));
            drawMenu.AddItem(new MenuItem("DrawQ", "Q鑼冨洿").SetValue(true));
            drawMenu.AddItem(new MenuItem("DrawW", "W鑼冨洿").SetValue(false));
            drawMenu.AddItem(new MenuItem("DrawE", "E鑼冨洿").SetValue(false));
            drawMenu.AddItem(new MenuItem("DrawKill", "鏄剧ず鍑绘潃鏂囨湰").SetValue(true));
			Config.AddSubMenu(new Menu("瓒呯姹夊寲", "by weilai"));
				Config.SubMenu("by weilai").AddItem(new MenuItem("qunhao", "姹夊寲缇わ細386289593"));
				Config.SubMenu("by weilai").AddItem(new MenuItem("qunhao2", "濞冨▋缇わ細13497795"));
            Menu farmMenu = new Menu("琛ュ叺", "Farming");
            farmMenu.AddItem(new MenuItem("FarmKey", "琛ュ叺").SetValue(new KeyBind('X', KeyBindType.Press)));
            farmMenu.AddItem(new MenuItem("QFarm", "浣跨敤Q").SetValue(true));
            farmMenu.AddItem(new MenuItem("WFarm", "浣跨敤W").SetValue(true));
            farmMenu.AddItem(new MenuItem("EFarm", "浣跨敤 E").SetValue(false));
            farmMenu.AddItem(new MenuItem("FarmMoveToMouse", "琛ュ叺璺熼殢榧犳爣").SetValue(false));

            Menu ksMenu = new Menu("鍑绘潃", "KillStealing");
            ksMenu.AddItem(new MenuItem("KillSteal", "鎶㈠ご").SetValue(true));
            ksMenu.AddItem(new MenuItem("KsUseUlt", "浣跨敤R").SetValue(true));
            ksMenu.AddItem(new MenuItem("KsUseItems", "浣跨敤鐗╁搧").SetValue(true));

            Menu miscMenu = new Menu("鏉傞」", "Misc");
            miscMenu.AddItem(new MenuItem("WardJumpKey", "鐬溂").SetValue(new KeyBind('G', KeyBindType.Press)));
            miscMenu.AddItem(new MenuItem("packets", "浣跨敤灏佸寘").SetValue(true));

            _orbwalker = new Orbwalking.Orbwalker(_config.SubMenu("Orbwalking"));

            Menu targetSelectorMenu = new Menu("鐩爣閫夋嫨", "TargetSelector", false);
            SimpleTs.AddToMenu(targetSelectorMenu);
			
            _config.AddSubMenu(targetSelectorMenu);

            _config.AddSubMenu(comboMenu);
            _config.AddSubMenu(harassMenu);
            _config.AddSubMenu(farmMenu);
            _config.AddSubMenu(ksMenu);
            _config.AddSubMenu(miscMenu);
            _config.AddSubMenu(drawMenu);

            _config.AddToMainMenu();
        }

        public override void SetupSpells()
        {
            RegisterSpell("Q", new Spell(SpellSlot.Q, 675f));
            RegisterSpell("W", new Spell(SpellSlot.W, 375f));
            RegisterSpell("E", new Spell(SpellSlot.E, 700f));
            RegisterSpell("R", new Spell(SpellSlot.R, 550f));

            Q.SetTargetted(400, 1400);
        }

        public override void OnGameUpdate(EventArgs args)
        {
            _brain.Think(_thoughtContext);
        }

        public override void OnPacketSend(GamePacketEventArgs args)
        {
            if (_thoughtContext.CastingUlt && !Config.Item("WardJumpKey").GetValue<KeyBind>().Active && args.Channel == PacketChannel.C2S)
            {
                var gamePacket = new GamePacket(args.PacketData);
                switch ((C2SPacketOpcodes) gamePacket.Header)
                {
                    case C2SPacketOpcodes.Move:
                        var movePacket = Packet.C2S.Move.Decoded(args.PacketData);
                        if (movePacket.SourceNetworkId == ObjectManager.Player.NetworkId)
                        {
                            if (Config.Item("StopUlt").GetValue<bool>())
                            {
                                if ((!Q.IsReady() && !W.IsReady() && !E.IsReady()) && _thoughtContext.Target != null &&
                                    _thoughtContext.Target.Unit.IsValid &&
                                    _thoughtContext.Target.Unit.Health >
                                    (_thoughtContext.Target.DamageContext.QDamage + _thoughtContext.Target.DamageContext.WDamage +
                                     _thoughtContext.Target.DamageContext.EDamage))
                                    args.Process = false;
                            }
                        }
                        break;
                    case C2SPacketOpcodes.Cast:
                        var castPacket = Packet.C2S.Cast.Decoded(args.PacketData);
                        
                        if (castPacket.SourceNetworkId == ObjectManager.Player.NetworkId && castPacket.Slot == SpellSlot.R)
                            _thoughtContext.CastingUlt = true;

                        break;
                }
            }            
        }

        public override void OnDraw(EventArgs args)
        {
            if (Config.Item("DisableAllDrawing").GetValue<bool>())
                return;

            if (Config.Item("DrawQ").GetValue<bool>())
                Utility.DrawCircle(ObjectManager.Player.Position, Q.Range, Color.FromArgb(255, 178, 0, 0), 5, 30, false);
            
            if (Config.Item("DrawW").GetValue<bool>())
                Utility.DrawCircle(ObjectManager.Player.Position, W.Range, Color.FromArgb(255, 178, 0, 0), 5, 30, false);

            if (Config.Item("DrawE").GetValue<bool>())
                Utility.DrawCircle(ObjectManager.Player.Position, E.Range, Color.FromArgb(255, 178, 0, 0), 5, 30, false);

            if (Config.Item("DrawKill").GetValue<bool>())
                _thoughtContext.Targets.ForEach(x=>x.DrawText());
        }

        public override void OnWndProc(WndEventArgs args)
        {
            if (args.Msg == 0x204 && _thoughtContext.CastingUlt)
                _thoughtContext.CastingUlt = false;
        }

        public override string ChampionName
        {
            get { return "Katarina"; }
        }
    }
}
