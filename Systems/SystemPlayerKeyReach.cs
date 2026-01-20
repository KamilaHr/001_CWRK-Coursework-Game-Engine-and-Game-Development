using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using OpenGL_Game.Components;
using OpenGL_Game.Objects;
using OpenGL_Game.Scenes;


namespace OpenGL_Game.Systems
{
    class SystemPlayerKeyReach : System
    {
        private readonly GameScene scene;
        public SystemPlayerKeyReach(GameScene scene)
        {
            this.scene = scene;
        }
        public override void OnAction(Entity entity)
        {
            var _key = entity.Components.Find(c => c.ComponentType == Components.ComponentTypes.COMPONENT_KEY) as Components.ComponentKey;
            var keyPosition = entity.Components.Find(c => c.ComponentType == Components.ComponentTypes.COMPONENT_POSITION) as Components.ComponentPosition;
            if (_key == null || keyPosition == null) return;
            var player = scene.PlayerEntity;
            var inv = player.Components.Find(c => c.ComponentType == Components.ComponentTypes.COMPONENT_INVENTORY) as Components.ComponentInventory;
            if (inv == null) return;

            if (inv.HasKey) return;

            Vector3 playerPos = scene.GetPlayerPos();
            float distance = (playerPos - keyPosition.Position).Length;

            if (distance <= 0.8f)
            {
                inv.HasKey = true;
                keyPosition.Position = new Vector3(9999, 9999, 9999);
            }
        }
    }
}
