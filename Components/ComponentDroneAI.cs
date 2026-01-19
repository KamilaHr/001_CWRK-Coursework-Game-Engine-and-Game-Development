using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGL_Game.Components
{
    class ComponentDroneAI : IComponent
    {
        public float _Speed = 1.5f;
        public float StopDistance = 0.5f;

        public ComponentTypes ComponentType => ComponentTypes.COMPONENT_DRONE_AI;
    }
}
