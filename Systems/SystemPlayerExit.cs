using OpenGL_Game.Components;
using OpenGL_Game.Objects;
using OpenGL_Game.Scenes;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace OpenGL_Game.Systems
{
    class SystemPlayerExit : System
    {
        private readonly GameScene scene;
        public SystemPlayerExit(GameScene scene)
        {
            this.scene = scene;
        }

        public override void OnAction(Entity entity)
        {
            var _exit = entity.Components.Find(c => c.ComponentType == Components.ComponentTypes.COMPONENT_EXIT) as ComponentExit;
            var exitPosition = entity.Components.Find(c => c.ComponentType == Components.ComponentTypes.COMPONENT_POSITION) as ComponentPosition;
            if (_exit == null || exitPosition == null) return;

            var player = scene.PlayerEntity;
            var inv = player.Components.Find(c => c.ComponentType == Components.ComponentTypes.COMPONENT_INVENTORY) as ComponentInventory;
            if (inv == null) return;

            Vector3 playerPosition = scene.GetPlayerPos();
            float distanceToExit = (playerPosition - exitPosition.Position).Length;

            if (distanceToExit <= 0.8f)
            {
                if (!inv.HasKey)
                {
                    scene.ShowDoorLocked();
                    return;
                }
                scene.OnPlayerReachedExit();
            }
        }
    }
}

