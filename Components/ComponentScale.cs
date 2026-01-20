using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGL_Game.Components
{
    class ComponentScale : IComponent
    {
        public float Scale;
        public ComponentScale(float _scale) { Scale = _scale; }
        public ComponentTypes ComponentType => ComponentTypes.COMPONENT_SCALE;
    }
}
