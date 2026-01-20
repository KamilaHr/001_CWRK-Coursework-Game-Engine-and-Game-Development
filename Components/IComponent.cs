using System;

namespace OpenGL_Game.Components
{
    [FlagsAttribute]
    enum ComponentTypes {
        COMPONENT_NONE     = 0,
	    COMPONENT_POSITION = 1 << 0,
        COMPONENT_GEOMETRY = 1 << 1,
        COMPONENT_DRONE_AI  = 1 << 2,
        COMPONENT_LIVES = 1 << 3,
        COMPONENT_OFF_SET = 1 << 4,
        COMPONENT_SCALE = 1 << 5,
        COMPONENT_EXIT = 1 << 6,
        COMPONENT_KEY = 1 << 7,
        COMPONENT_INVENTORY = 1 << 8
    }

    interface IComponent
    {
        ComponentTypes ComponentType
        {
            get;
        }
    }
}
