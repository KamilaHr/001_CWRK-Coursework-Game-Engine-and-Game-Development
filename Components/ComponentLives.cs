using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGL_Game.Components
{
    class ComponentLives : IComponent
    {
        public int _Lives = 3;
        public float _timer = 0.0f;

        public ComponentTypes ComponentType => ComponentTypes.COMPONENT_LIVES;
    }
}
