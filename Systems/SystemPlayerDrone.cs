using OpenGL_Game.Components;
using OpenGL_Game.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGL_Game.Systems
{
    class SystemPlayerDrone : System
    {
        private Scenes.GameScene game;

        public SystemPlayerDrone(Scenes.GameScene gameScene)
        {
            game = gameScene;
        }

        public override void OnAction(Entity entity)
        {
            // Only run on drones
            ComponentDroneAI ai = null;
            ComponentPosition dronePos = null;

            foreach (var c in entity.Components)
            {
                if (c.ComponentType == ComponentTypes.COMPONENT_DRONE_AI) ai = (ComponentDroneAI)c;
                if (c.ComponentType == ComponentTypes.COMPONENT_POSITION) dronePos = (ComponentPosition)c;
            }

            if (ai == null || dronePos == null) return;

            // player data
            Vector3 playerPos = game.GetPlayerPos();
            float _dist = (playerPos - dronePos.Position).Length;

            // simple radius check
            float hitDist = game.PlayerRadius + game.DroneRadius;
            if (_dist <= hitDist)
            {
                game.OnPlayerHitByDrone();
            }
        }
    }
}
