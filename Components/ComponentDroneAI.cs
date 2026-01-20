using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace OpenGL_Game.Components
{
    class ComponentDroneAI : IComponent
    {
        public float _Speed = 1.5f;
        public float StopDistance = 0.5f;

        public DroneMode _mode = DroneMode.Chase;

        public List<Vector3> patrolPoints = new List<Vector3>();
        public int currentPatrolIndex = 0;
        public float arriveDistance = 0.4f;

        public float chaseDistance = 4.0f;

        public ComponentTypes ComponentType => ComponentTypes.COMPONENT_DRONE_AI;
    }

    public enum DroneMode
    {
        Chase,
        Patrol
    }
}
