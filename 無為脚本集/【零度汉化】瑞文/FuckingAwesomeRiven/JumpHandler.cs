using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace FuckingAwesomeRiven
{
    public class jumpPosition
    {
        public jumpPosition(Vector3 direction, Vector3 position)
        {
            directionPos = direction;
            jumpPos = position;
        }
        public Vector3 directionPos;
        public Vector3 jumpPos;
    }
    class JumpHandler
    {
        public static List<Vector3> jumpPositionsList2 = new List<Vector3>();
        public static List<jumpPosition> allJumpPos = new List<jumpPosition>();

        public static void load()
        {
            allJumpPos = new List<jumpPosition>()
            {
                new jumpPosition(new Vector3(3900f, 1210f, 95.74805f), new Vector3(4142f, 1234f, 95.74805f)),
                new jumpPosition(new Vector3(4490f, 1272f, 95.74807f), new Vector3(4444f, 1254f, 95.74805f)),
                new jumpPosition(new Vector3(6666f, 1474f, 49.986f), new Vector3(6790f, 1482f, 49.59703f)),
                new jumpPosition(new Vector3(7314f, 1486f, 49.4455f), new Vector3(7052f, 1486f, 49.44687f)),
                new jumpPosition(new Vector3(7720f, 1604f, 49.44771f), new Vector3(7724f, 1722f, 49.4488f)),
                new jumpPosition(new Vector3(7744f, 2220f, 51.14706f), new Vector3(7756f, 2072f, 51.1414f)),
            };
        }

        public static bool initJump;
        public static bool jumping;
        public static jumpPosition selectedPos;

        public static void jump()
        {

            if (CheckHandler.QCount != 2)
            {
                SpellHandler.CastQ();
                return;
            }

            if (initJump && selectedPos != null && !jumping)
            {
                jumping = true;
                if (!SpellHandler._spells[SpellSlot.E].IsReady())
                {
                    ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, selectedPos.directionPos);
                    Utility.DelayAction.Add(100 + Game.Ping/2,
                        () => ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, selectedPos.jumpPos));
                    Utility.DelayAction.Add(300 + Game.Ping/2, () =>
                    {
                        SpellHandler.CastQ();
                        jumping = false;
                    });
                }
                else
                {
                    ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, selectedPos.directionPos);
                    Utility.DelayAction.Add(100 + Game.Ping / 2,
                        () => SpellHandler.CastE(selectedPos.jumpPos));
                    Utility.DelayAction.Add(200 + Game.Ping / 2, () =>
                    {
                        SpellHandler.CastQ();
                        jumping = false;
                    }); 
                }
                initJump = false;
            }

            if (initJump || jumping) return;
            selectedPos = null;
                foreach (var jumpPos in allJumpPos)
                {
                    if (ObjectManager.Player.Distance(jumpPos.jumpPos) < 80)
                    {
                        selectedPos = jumpPos;
                        initJump = true;
                        break;
                    }
                }
        }

        public static void onDraw()
        {
            foreach (var pos in allJumpPos)
            {
                Render.Circle.DrawCircle(pos.directionPos, 30, Color.White);
                Drawing.DrawLine(Drawing.WorldToScreen(pos.directionPos), Drawing.WorldToScreen(pos.jumpPos), 2, Color.White);
                Render.Circle.DrawCircle(pos.jumpPos, 30, Color.White);
            }        
        }

        public static void drawCircles()
        {
            foreach (var pos in allJumpPos)
            {
                Render.Circle.DrawCircle(pos.directionPos, 30, Color.White);
                Drawing.DrawLine(Drawing.WorldToScreen(pos.directionPos), Drawing.WorldToScreen(pos.jumpPos), 2, Color.White);
                Render.Circle.DrawCircle(pos.jumpPos, 30, Color.White);
            }
            for (int i = 0; i == jumpPositionsList2.Count-1; i++)
            {
                Render.Circle.DrawCircle(jumpPositionsList2[i], 30, Color.Blue);
            }
        }

        public static void addPos()
        {
            jumpPositionsList2.Add(ObjectManager.Player.Position);
            if (jumpPositionsList2.Count == 2)
            {
                allJumpPos.Add(new jumpPosition(jumpPositionsList2[0], jumpPositionsList2[1]));
                jumpPositionsList2 = new List<Vector3>();
                Game.PrintChat("Added new Jump Position {0} ", Game.Time);
            }
            else if (jumpPositionsList2.Count > 2)
            {
                jumpPositionsList2 = new List<Vector3>();
                Game.PrintChat("Error: recreate jump pos");
            }
        }

        public static void clearPrevious()
        {
            if (allJumpPos.Count == 0) return;
            allJumpPos.Remove(allJumpPos[allJumpPos.Count - 1]);
            Game.PrintChat("Removed Previous");
        }

        public static void clearCurrent()
        {
            jumpPositionsList2 = new List<Vector3>();
            Game.PrintChat("Cleared Current Position!");
        }

        public static void printToConsole()
        {
            Console.Clear();
            foreach (var j in allJumpPos)
            {
                Console.WriteLine("new jumpPosition(new Vector3({0}f, {1}f, {2}f), new Vector3({3}f, {4}f, {5}f)), ", j.directionPos.X, j.directionPos.Y, j.directionPos.Z, j.jumpPos.X, j.jumpPos.Y, j.jumpPos.Z);
            }
        }
    }
}
