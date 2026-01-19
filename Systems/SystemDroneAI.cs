using OpenGL_Game.Components;
using OpenGL_Game.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;
namespace OpenGL_Game.Systems
{
    class SystemDroneAI : System
    {
        private Scenes.GameScene game;

        public SystemDroneAI(Scenes.GameScene gameScene)
        {
            game = gameScene;
        }

        public override void OnAction(Entity entity)
        {
            ComponentDroneAI ai = null;
            ComponentPosition pos = null;

            foreach (var c in entity.Components)
            {
                if (c.ComponentType == ComponentTypes.COMPONENT_DRONE_AI) ai = (ComponentDroneAI)c;
                if (c.ComponentType == ComponentTypes.COMPONENT_POSITION) pos = (ComponentPosition)c;
            }

            if (ai == null || pos == null) return;

            Vector3 playerPos = game.GetPlayerPos();
            Vector3 dronePos = pos.Position;

            Vector3 toPlayer = playerPos - dronePos;
            float _dist = toPlayer.Length;

            if (_dist < ai.StopDistance || _dist <= 0.0001f) return;

            toPlayer.Normalize();

            float move = ai._Speed * Scenes.GameScene.dt;
            Vector3 delta = toPlayer * move;

            Vector3 newPos = dronePos;

            Vector3 tryX = new Vector3(newPos.X + delta.X, newPos.Y, newPos.Z);
            if (!game.IsBlocked(tryX)) newPos.X = tryX.X;

            Vector3 tryZ = new Vector3(newPos.X, newPos.Y, newPos.Z + delta.Z);
            if (!game.IsBlocked(tryZ)) newPos.Z = tryZ.Z;

            pos.Position = newPos;
        }
    }
}