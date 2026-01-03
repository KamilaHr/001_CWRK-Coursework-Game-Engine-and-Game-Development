using OpenGL_Game.Components;
using OpenGL_Game.Objects;
using System.Collections.Generic;

namespace OpenGL_Game.Systems
{
    abstract class System
    {
        public IComponent GetComponent(Entity entity, ComponentTypes componentType)
        {
            List<IComponent> components = entity.Components;

            IComponent iComponent = components.Find(delegate (IComponent component)
            {
                return component.ComponentType == componentType;
            });

            return iComponent;
        }


        public abstract void OnAction(Entity entity);

        // Property signatures: 
        public string Name
        {
            get;
        }
    }
}
