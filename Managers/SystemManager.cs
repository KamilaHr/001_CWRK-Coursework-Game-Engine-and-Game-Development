using System.Collections.Generic;
using OpenGL_Game.Systems;
using OpenGL_Game.Objects;

namespace OpenGL_Game.Managers
{
    class SystemManager
    {
        List<Systems.System> systemList = new List<Systems.System>();

        public SystemManager()
        {
        }

        public void ActionSystems(EntityManager entityManager)
        {
            List<Entity> entityList = entityManager.Entities();
            foreach(Systems.System system in systemList)
            {
                foreach(Entity entity in entityList)
                {
                    system.OnAction(entity);
                }
            }
        }

        public void AddSystem(Systems.System system)
        {
            //ISystem result = FindSystem(system.Name);
            //Debug.Assert(result != null, "System '" + system.Name + "' already exists");
            systemList.Add(system);
        }

        private Systems.System FindSystem(string name)
        {
            return systemList.Find(delegate(Systems.System system)
            {
                return system.Name == name;
            }
            );
        }
    }
}
