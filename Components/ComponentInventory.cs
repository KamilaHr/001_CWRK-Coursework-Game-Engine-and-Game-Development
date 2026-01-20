using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGL_Game.Components
{
    class ComponentInventory : IComponent
    {
        public bool HasKey = false;
        public ComponentTypes ComponentType => ComponentTypes.COMPONENT_INVENTORY;
    }
}
