using OpenTK.Graphics.OpenGL;
using OpenGL_Game.Components;
using OpenGL_Game.Systems;
using OpenGL_Game.Managers;
using OpenGL_Game.Objects;
using System;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;
using SkiaSharp;

namespace OpenGL_Game.Scenes
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    class GameScene : Scene
    {
        public static float dt = 0;
        EntityManager entityManager;
        SystemManager systemManager;
        public Camera camera;
        public static GameScene gameInstance;

        private bool escapeWasDown = false;

        public GameScene(SceneManager sceneManager) : base(sceneManager)
        {
            gameInstance = this;
            entityManager = new EntityManager();
            systemManager = new SystemManager();

            // Set the title of the window
            sceneManager.Title = "Game";
            // Set the Render and Update delegates to the Update and Render methods of this class
            sceneManager.renderer = Render;
            sceneManager.updater = Update;
            // Set Keyboard events to go to a method in this class
            sceneManager.keyboardDownDelegate += Keyboard_KeyDown;

            // Enable Depth Testing
            GL.Enable(EnableCap.DepthTest);
            GL.DepthMask(true);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);

            // Set Camera
            camera = new Camera(new Vector3(0, 4, 7), new Vector3(0, 0, 0), (float)(sceneManager.Size.X) / (float)(sceneManager.Size.Y), 0.1f, 100f);

            CreateEntities();
            CreateSystems();

            // TODO: Add your initialization logic here
        }

        private void CreateEntities()
        {
            Entity newEntity;

            //Moon
            newEntity = new Entity("Moon");
            newEntity.AddComponent(new ComponentPosition(-2.0f, 0.0f, 0.0f));
            newEntity.AddComponent(new ComponentGeometry("Geometry/Moon/moon.obj"));
            entityManager.AddEntity(newEntity);

            //Wraith Raider Starship
            newEntity = new Entity("Wraith_Raider_Starship");
            newEntity.AddComponent(new ComponentPosition(2.0f, 0.0f, 0.0f));
            newEntity.AddComponent(new ComponentGeometry("Geometry/Wraith_Raider_Starship/Wraith_Raider_Starship.obj"));
            entityManager.AddEntity(newEntity);

            /*//Wraith Raider Starship_2
            newEntity = new Entity("Wraith_Raider_Starship");
            newEntity.AddComponent(new ComponentPosition(0.0f, 0.0f, 0.0f));
            newEntity.AddComponent(new ComponentGeometry("Geometry/Wraith_Raider_Starship/Wraith_Raider_Starship.obj"));
            entityManager.AddEntity(newEntity);*/

            //Intergalactic Spaceship
            newEntity = new Entity("Intergalactic_Spaceship");
            newEntity.AddComponent(new ComponentPosition(0.0f, 0.0f, 0.0f));
            newEntity.AddComponent(new ComponentGeometry(
            "Geometry/Intergalactic_Spaceship/Intergalactic_Spaceship.obj"));
            entityManager.AddEntity(newEntity);

            //Satellite
            newEntity = new Entity("Satellite");
            newEntity.AddComponent(new ComponentPosition(0.0f, 0.0f, 0.0f));
            newEntity.AddComponent(new ComponentGeometry(
            "Geometry/Satellite/satellite_obj.obj"));
            entityManager.AddEntity(newEntity);
        }

        private void CreateSystems()
        {
            Systems.System newSystem;

            newSystem = new SystemRender();
            systemManager.AddSystem(newSystem);
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="e">Provides a snapshot of timing values.</param>
        public override void Update(FrameEventArgs e)
        {
            dt = (float)e.Time;
            
            var keyboard = sceneManager.KeyboardState;
            bool escapeIsDown = keyboard.IsKeyDown(Keys.Escape);

            if (escapeIsDown && !escapeWasDown)
            {
                sceneManager.StartMenu();
                return;
            }

            escapeWasDown = escapeIsDown;

            systemManager.ActionSystems(entityManager);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="e">Provides a snapshot of timing values.</param>
        public override void Render(FrameEventArgs e)
        {
            GL.Viewport(0, 0, sceneManager.Size.X, sceneManager.Size.Y);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Action ALL systems
            systemManager.ActionSystems(entityManager);

            // Render score
            GUI.DrawText("Score: 000", 30, 70, 30, 255, 255, 255);
            GUI.Render();
        }

        /// <summary>
        /// This is called when the game exits.
        /// </summary>
        public override void Close()
        {
            sceneManager.keyboardDownDelegate -= Keyboard_KeyDown;

            // Need to remove assets (except Text) from Resource Manager
        }

        public void Keyboard_KeyDown(KeyboardKeyEventArgs e)
        {
            switch (e.Key)
            {
                case Keys.Up:
                    camera.MoveForward(0.1f);
                    break;
                case Keys.Down:
                    camera.MoveForward(-0.1f);
                    break;
                case Keys.Left:
                    camera.RotateY(-0.01f);
                    break;
                case Keys.Right:
                    camera.RotateY(0.01f);
                    break;
            }
        }
    }
}