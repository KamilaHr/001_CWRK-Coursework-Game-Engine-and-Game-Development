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
    class SystemDroneAI : System
    {
        const ComponentTypes MASK =
            ComponentTypes.COMPONENT_POSITION | ComponentTypes.COMPONENT_DRONE_AI;

        private readonly GameScene scene;

        public SystemDroneAI(GameScene scene)
        {
            this.scene = scene;
        }

        public override void OnAction(Entity entity)
        {
            if ((entity.Mask & MASK) != MASK) return;
            if (scene.AIFrozen) return;
            if (!entity.Name.StartsWith("Drone_")) return;

            var posComp = GetComponent(entity, ComponentTypes.COMPONENT_POSITION) as ComponentPosition;
            var ai = GetComponent(entity, ComponentTypes.COMPONENT_DRONE_AI) as ComponentDroneAI;
            if (posComp == null || ai == null) return;

            Vector3 dronePos = posComp.Position;
            Vector3 playerPos = scene.GetPlayerPos();

            Vector3 targetPos = playerPos;

            if (ai._mode == DroneMode.Patrol && ai.patrolPoints != null && ai.patrolPoints.Count > 0)
            {
                targetPos = ai.patrolPoints[ai.currentPatrolIndex];

                float distToPlayer = (playerPos - dronePos).Length;
                if (distToPlayer <= ai.chaseDistance)
                {
                    targetPos = playerPos;
                }
            }

            Vector3 toTarget = targetPos - dronePos;
            float dist = toTarget.Length;

            float stopDist = ai.StopDistance;
            if (ai._mode == DroneMode.Patrol && targetPos != playerPos)
                stopDist = ai.arriveDistance;

            if (dist <= stopDist)
            {
                if (ai._mode == DroneMode.Patrol && targetPos != playerPos && ai.patrolPoints.Count > 0)
                {
                    ai.currentPatrolIndex = (ai.currentPatrolIndex + 1) % ai.patrolPoints.Count;
                }
                return;
            }

            toTarget /= dist; 
            float step = ai._Speed * GameScene.dt;

            Vector3 delta = toTarget * step;

            Vector3 _fullMove = dronePos + delta;
            float r = scene.DroneRadius;

            if (!scene.IsBlocked(_fullMove, r))
            {
                posComp.Position = _fullMove;
                return;
            }

            Vector3 tryX = new Vector3(dronePos.X + delta.X, dronePos.Y, dronePos.Z);
            if (!scene.IsBlocked(tryX, r))
            {
                posComp.Position = tryX;
                return;
            }

            Vector3 tryZ = new Vector3(dronePos.X, dronePos.Y, dronePos.Z + delta.Z);
            if (!scene.IsBlocked(tryZ, r))
            {
                posComp.Position = tryZ;
                return;
            }
        }
    }
}