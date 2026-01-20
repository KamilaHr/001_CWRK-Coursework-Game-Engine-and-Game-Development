using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace OpenGL_Game.Components
{
    class ComponentOffSet : IComponent
    {
        public float OffSet;
        public ComponentOffSet(float offset) { OffSet = offset; }
        public ComponentTypes ComponentType => ComponentTypes.COMPONENT_OFF_SET;
    }
}
