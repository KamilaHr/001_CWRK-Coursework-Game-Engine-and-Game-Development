using System;

namespace OpenGL_Game.Components
{
    [FlagsAttribute]
    enum ComponentTypes {
        COMPONENT_NONE     = 0,
	    COMPONENT_POSITION = 1 << 0,
        COMPONENT_GEOMETRY = 1 << 1
    }

    interface IComponent
    {
        ComponentTypes ComponentType
        {
            get;
        }
    }
}
