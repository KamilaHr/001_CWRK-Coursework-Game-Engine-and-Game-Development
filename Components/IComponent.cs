using System;

namespace OpenGL_Game.Components
{
    [FlagsAttribute]
    enum ComponentTypes {
        COMPONENT_NONE     = 0,
	    COMPONENT_POSITION = 1 << 0,
        COMPONENT_GEOMETRY = 1 << 1,
        COMPONENT_DRONE_AI  = 1 << 2,
        COMPONENT_LIVES
    }

    interface IComponent
    {
        ComponentTypes ComponentType
        {
            get;
        }
    }
}
