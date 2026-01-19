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
using System.Collections.Generic;

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
        private Entity player;

        private Vector3 lastPlayerPos;

        private bool escapeWasDown = false;

        private const int GRID_W = 20;
        private const int GRID_H = 20;
        private const float TILE = 1.0f;

        private const string WALL_OBJ = "Geometry/Moon/moon.obj";
        private const string FLOOR_OBJ = "Geometry/Moon/moon.obj";

        private int[,] _maze;
        private Vector3 mazeOrigin;
        private const float PLAYER_RADIUS = 0.35f;
        private const float WALL_HALF = 0.5f;

        private Vector3 playerSpawnPos;
        private readonly Dictionary <Entity, Vector3> droneSpawnPos = new Dictionary <Entity, Vector3>();

        private const float DRONE_RADIUS = 0.35f;

        public bool IsBlocked(Vector3 pos) => CollidesWithWall(pos);
        public Vector3 GetPlayerPos()
        {
            var positionComp = player?.Components.Find(c => c.ComponentType == ComponentTypes.COMPONENT_POSITION) as ComponentPosition;
            return positionComp != null ? positionComp.Position : Vector3.Zero;
        }

        public float PlayerRadius => PLAYER_RADIUS;
        public float DroneRadius => DRONE_RADIUS;

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

            sceneManager.CursorState = CursorState.Normal;

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
            /*newEntity = new Entity("Moon");
            newEntity.AddComponent(new ComponentPosition(-2.0f, 0.0f, 0.0f));
            newEntity.AddComponent(new ComponentGeometry("Geometry/Moon/moon.obj"));
            entityManager.AddEntity(newEntity);

            //Wraith Raider Starship
            newEntity = new Entity("Wraith_Raider_Starship");
            newEntity.AddComponent(new ComponentPosition(2.0f, 0.0f, 0.0f));
            newEntity.AddComponent(new ComponentGeometry("Geometry/Wraith_Raider_Starship/Wraith_Raider_Starship.obj"));
            entityManager.AddEntity(newEntity);

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
            entityManager.AddEntity(newEntity);*/

            playerSpawnPos = new Vector3(-8.0f, 0.0f, -8.0f);

            newEntity = new Entity("Player");
            newEntity.AddComponent(new ComponentPosition(playerSpawnPos.X, playerSpawnPos.Y, playerSpawnPos.Z));
            newEntity.AddComponent(new ComponentGeometry(
            "Geometry/Player/Cat_playerS.obj"));
            newEntity.AddComponent(new ComponentLives());
            entityManager.AddEntity(newEntity);
            player = newEntity; 


            BuildMazeWorld();

            Entity _drone = new Entity("Drone_1");
            Vector3 droneSpawn = new Vector3(-6.0f, 0.0f, -6.0f);
            _drone.AddComponent(new ComponentPosition(droneSpawn.X, droneSpawn.Y, droneSpawn.Z));
            _drone.AddComponent(new ComponentGeometry(
            "Geometry/Intergalactic_Spaceship/Intergalactic_Spaceship.obj"));
            _drone.AddComponent(new ComponentDroneAI());
            entityManager.AddEntity(_drone);

            droneSpawnPos[_drone] = droneSpawn;
        }

        private void CreateSystems()
        {
            Systems.System newSystem;

            newSystem = new SystemRender();
            systemManager.AddSystem(newSystem);
            systemManager.AddSystem(new SystemDroneAI(this));
            systemManager.AddSystem(new SystemPlayerDrone(this));
            systemManager.AddSystem(new SystemRender());
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="e">Provides a snapshot of timing values.</param>
        public override void Update(FrameEventArgs e)
        {
            dt = (float)e.Time;

            var _lives = player?.Components.Find(c => c.ComponentType == ComponentTypes.COMPONENT_LIVES) as ComponentLives;
            if (_lives != null && _lives._timer > 0.0f)
            {
                _lives._timer -= dt;
                if (_lives._timer < 0.0f) _lives._timer = 0.0f;
            }
            
            var keyboard = sceneManager.KeyboardState;
            bool escapeIsDown = keyboard.IsKeyDown(Keys.Escape);

            if (escapeIsDown && !escapeWasDown)
            {
                sceneManager.StartMenu();
                return;
            }

            escapeWasDown = escapeIsDown;

            if (player != null)
            {
                ComponentPosition _position = null;
                foreach (var _comp in player.Components)
                {
                    if (_comp.ComponentType == ComponentTypes.COMPONENT_POSITION)
                    {
                        _position = (ComponentPosition)_comp;
                        break;
                    }
                }
                if (_position != null)
                {
                    float moveSpeed = 2.0f;
                    float _move = moveSpeed * dt;

                    if (_move > 0.1f) _move = 0.1f;

                    Vector3 _delta = Vector3.Zero;

                    if (keyboard.IsKeyDown(Keys.W)) _delta += new Vector3 (0, 0, -_move);
                    if (keyboard.IsKeyDown(Keys.S)) _delta += new Vector3 (0, 0, _move);
                    if (keyboard.IsKeyDown(Keys.A)) _delta += new Vector3 (-_move, 0, 0);
                    if (keyboard.IsKeyDown(Keys.D)) _delta += new Vector3 (_move, 0, 0);

                    //_position.Position += _delta;


                    Vector3 newPos = _position.Position;
                    Vector3 testPosX = new Vector3(newPos.X + _delta.X, newPos.Y, newPos.Z);

                    if (!CollidesWithWall(testPosX))
                        newPos.X = testPosX.X;

                    Vector3 testPosZ = new Vector3(newPos.X, newPos.Y, newPos.Z + _delta.Z);
                    if (!CollidesWithWall(testPosZ))
                        newPos.Z = testPosZ.Z;
                    _position.Position = newPos;

                    lastPlayerPos = _position.Position;

                    Vector3 _playerPos = _position.Position;
                    Vector3 _cameraOffset = new Vector3(0.0f, 6.0f, 12.0f);

                    camera.cameraPosition = _playerPos + _cameraOffset;

                    camera.cameraDirection = (_playerPos - camera.cameraPosition);
                    camera.cameraDirection.Normalize();

                    camera.UpdateView();
                }
            }

            //systemManager.ActionSystems(entityManager);
        }

        
        /// <param name="e">Provides a snapshot of timing values.</param>
        public override void Render(FrameEventArgs e)
        {
            GL.Viewport(0, 0, sceneManager.Size.X, sceneManager.Size.Y);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Action ALL systems
            systemManager.ActionSystems(entityManager);

            // Render score
            GUI.DrawText("Score: 000", 30, 70, 30, 255, 255, 255);
            GUI.DrawText($"Player pos: {lastPlayerPos.X: 0.00}, {lastPlayerPos.Y: 0.00}, {lastPlayerPos.Z: 0.00}", 30, 150, 24, 0, 255, 0);
            var _lives = player?.Components.Find(c => c.ComponentType == ComponentTypes.COMPONENT_LIVES) as ComponentLives;
            int livesLeft = _lives != null ? _lives._Lives : 0;

            string _heart = "";
            for (int i = 0; i < livesLeft; i++)
            {
                _heart += "* ";
            }
            GUI.DrawText($"Lives: {_heart}", 30, 190, 28, 255, 50, 50);
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

        private int[,] CreateMaze()
        {
            int[,] m = new int[GRID_H, GRID_W];

            for (int z = 0; z < GRID_H; z++)
            {
                for (int x = 0; x < GRID_W; x++)
                {
                    bool _border = (x == 0 || z == 0 || x == GRID_W - 1 || z == GRID_H - 1);
                    m[z, x] = _border ? 1 : 0;
                }
            }

            for (int x = 2; x < GRID_W - 2; x++) m[5, x] = 1;
            for (int z = 2; z < GRID_H - 2; z++) m[z, 10] = 1;

            m[5, 3] = 0;
            m[5, 15] = 0;
            m[8, 10] = 0;
            m[14, 10] = 0;

            return m;
        }

        private void BuildMazeWorld()
        {
            _maze = CreateMaze();

            float startX = - (GRID_W * TILE) * 0.5f;
            float startZ = - (GRID_H * TILE) * 0.5f;

            mazeOrigin = new Vector3(startX, 0.0f, startZ);

            for (int z = 0; z < GRID_H; z++)
            {
                for (int x = 0; x < GRID_W; x++)
                {
                    Entity _floor = new Entity($"Tile_{x}_{z}");
                    float posX = startX + (x * TILE);
                    float posZ = startZ + (z * TILE);

                    _floor.AddComponent(new ComponentPosition(posX, -2.0f, posZ));
                    _floor.AddComponent(new ComponentGeometry(FLOOR_OBJ));
                    entityManager.AddEntity(_floor);

                    if (_maze[z, x] == 1)
                    {
                        Entity _wall = new Entity($"Wall_{x}_{z}");
                        _wall.AddComponent(new ComponentPosition(posX, 0.0f, posZ));
                        
                        _wall.AddComponent(new ComponentGeometry(WALL_OBJ));
                        entityManager.AddEntity(_wall);
                    }
                }
            }
        }

        private bool CollidesWithWall(Vector3 position)
        {
            int mazeX = (int)MathF.Floor((position.X - mazeOrigin.X) / TILE);
            int mazeZ = (int)MathF.Floor((position.Z - mazeOrigin.Z) / TILE);

            for (int z = mazeZ - 1; z <= mazeZ + 1; z++)
            {
                for (int x = mazeX - 1; x <= mazeX + 1; x++)
                {
                    if (x < 0 || z < 0 || x >= GRID_W || z >= GRID_H) continue;
                    if (_maze[z, x] != 1) continue;

                    float wallCenterX = mazeOrigin.X + (x * TILE);
                    float wallCenterZ = mazeOrigin.Z + (z * TILE);

                    float closestX = MathF.Max(wallCenterX - WALL_HALF, MathF.Min(position.X, wallCenterX + WALL_HALF));
                    float closestZ = MathF.Max(wallCenterZ - WALL_HALF, MathF.Min(position.Z, wallCenterZ + WALL_HALF));

                    float distanceX = position.X - closestX;
                    float distanceZ = position.Z - closestZ;

                    if ((distanceX * distanceX + distanceZ * distanceZ) < (PLAYER_RADIUS * PLAYER_RADIUS)) return true;
                }
            }
            return false;
        }

        public void OnPlayerHitByDrone()
        {
            var _lives = player?.Components.Find(c => c.ComponentType == ComponentTypes.COMPONENT_LIVES) as ComponentLives;
            if (_lives == null)
                return;

            if (_lives._timer > 0.0f) return;

            _lives._Lives -= 1;
            _lives._timer = 1.0f;

            ResetPositionAfterHit();

            if (_lives._Lives <= 0)
            {
                sceneManager.StartMenu();
            }
        }

        private void ResetPositionAfterHit()
        {
            var _playerPosition = player.Components.Find(c => c.ComponentType == ComponentTypes.COMPONENT_POSITION) as ComponentPosition;
            if (_playerPosition != null) _playerPosition.Position = playerSpawnPos;
            
            foreach (var droneE in droneSpawnPos)
            {
                var drone = droneE.Key;
                var _spawn = droneE.Value;

                var dp = drone.Components.Find(c => c.ComponentType == ComponentTypes.COMPONENT_POSITION) as ComponentPosition;
                if (dp != null) dp.Position = _spawn;
            }

            camera.cameraPosition = playerSpawnPos + new Vector3(0.0f, 6.0f, 12.0f);
            camera.cameraDirection = (playerSpawnPos - camera.cameraPosition);
            camera.cameraDirection.Normalize();
            camera.UpdateView();
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