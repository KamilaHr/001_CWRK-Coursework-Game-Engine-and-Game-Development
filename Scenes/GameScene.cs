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
using OpenGL_Game.Networking;

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

        private const string WALL_OBJ = "Geometry/Wall/Wall.obj";
        private const string FLOOR_OBJ = "Geometry/Floor/FloorT.obj";

        private int[,] _maze;
        private Vector3 mazeOrigin;
        private const float PLAYER_RADIUS = 0.35f;
        private const float WALL_HALF = 0.5f;

        private Vector3 playerSpawnPos;
        private readonly Dictionary <Entity, Vector3> droneSpawnPos = new Dictionary <Entity, Vector3>();

        private const float DRONE_RADIUS = 0.35f;

        private bool aiFrozen = false;
        private float aiFreezeTimer = 0.0f;
        private bool mDown = false;

        private const float AI_FREEZE_DURATION = 3.0f;
        public bool AIFrozen => aiFrozen;
        private bool _won = false;

        private bool gameWon = false;
        private float  winTimer = 0.0f;

        private bool doorLocked = false;
        private float doorUnlockTimer = 0.0f;

        private float initialsInputCooldown = 0.0f;


        public Entity PlayerEntity => player;

        private HighscoreClient highscoreClient = new HighscoreClient();
        private List<HighscoreEntry> highscores = new List<HighscoreEntry>();

        private bool enterWasDown = false;
        private bool backWasDown = false;

        private int score = 0;

        private bool enteringInitials = false;
        private string initialsInput = "";
        private bool submittedScore = false;


        public bool IsBlocked(Vector3 pos, float radius) => CollidesWithWall(pos, radius);
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

            sceneManager.Title = "Game";
            sceneManager.renderer = Render;
            sceneManager.updater = Update;

            sceneManager.keyboardDownDelegate -= Keyboard_KeyDown;

            sceneManager.keyboardDownDelegate += Keyboard_KeyDown;


            sceneManager.CursorState = CursorState.Normal;

            GL.Enable(EnableCap.DepthTest);
            GL.DepthMask(true);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);

            camera = new Camera(new Vector3(0, 4, 7), new Vector3(0, 0, 0), (float)(sceneManager.Size.X) / (float)(sceneManager.Size.Y), 0.1f, 100f);

            score = 0;
            enteringInitials = false;
            initialsInput = "";
            submittedScore = false;
            doorLocked = false;
            gameWon = false;


            CreateEntities();
            CreateSystems();

            TryLoadHighscores();
        }

        public void OnPlayerReachedExit()
        {
            if (gameWon) return;
            gameWon = true;
            winTimer = 3.0f;

            if (IsNewHighscore(score))
            {
                enteringInitials = true;
                initialsInput = "";
                submittedScore = false;
                winTimer = 9999.0f;

                initialsInputCooldown = 0.25f;
            }
        }

        private bool IsNewHighscore(int s)
        {
            if (highscores == null || highscores.Count < 10) return true;
            int lowest = highscores[highscores.Count - 1].Score;
            return s > lowest;
        }

        public void ShowDoorLocked()
        {
            doorLocked = true;
            doorUnlockTimer = 2.0f;
        }

        private void TryLoadHighscores()
        {
            try
            {
                highscores = highscoreClient.GetScores();
            }
            catch
            {
                highscores = new List<HighscoreEntry>();
            }
        }


        private void CreateEntities()
        {
            Entity newEntity;

            playerSpawnPos = CellWorld(16, 16);

            newEntity = new Entity("Player");
            newEntity.AddComponent(new ComponentPosition(playerSpawnPos.X, playerSpawnPos.Y, playerSpawnPos.Z));
            newEntity.AddComponent(new ComponentGeometry(
            "Geometry/Player/Cat_PlayerSm.obj"));
            newEntity.AddComponent(new ComponentLives());
            newEntity.AddComponent(new ComponentInventory());
            entityManager.AddEntity(newEntity);
            player = newEntity; 


            BuildMazeWorld();

            Entity key = new Entity("Key");
            Vector3 keyPos = CellWorld(8, 16);
            key.AddComponent(new ComponentPosition(keyPos.X, 0.0f, keyPos.Z));
            key.AddComponent(new ComponentGeometry("Geometry/Key/KeyT.obj"));
            key.AddComponent(new ComponentKey());
            entityManager.AddEntity(key);

            Entity _drone = new Entity("Drone_1");
            Vector3 droneSpawn = CellWorld(3, 16);
            _drone.AddComponent(new ComponentPosition(droneSpawn.X, droneSpawn.Y, droneSpawn.Z));
            _drone.AddComponent(new ComponentGeometry(
            "Geometry/Intergalactic_Spaceship/Intergalactic_Spaceship.obj"));
            _drone.AddComponent(new ComponentScale(0.23f));
            _drone.AddComponent(new ComponentOffSet(0.5f));
            _drone.AddComponent(new ComponentDroneAI());
            var ai1 = _drone.Components.Find(c => c.ComponentType == ComponentTypes.COMPONENT_DRONE_AI) as ComponentDroneAI;
            if (ai1 != null)
            {
                ai1._mode = DroneMode.Chase;
                ai1._Speed = 1.7f;
                ai1.chaseDistance = 1000.0f;
            }
            entityManager.AddEntity(_drone);
            droneSpawnPos[_drone] = droneSpawn;

            Entity _sky = new Entity("Skybox");
            _sky.AddComponent(new ComponentGeometry("Geometry/SkyBox/SkyBox.obj"));
            entityManager.AddEntity(_sky);

            Entity _drone2 = new Entity("Drone_2");
            Vector3 droneSpawn2 = CellWorld(16, 3);
            _drone2.AddComponent(new ComponentPosition(droneSpawn2.X, droneSpawn2.Y, droneSpawn2.Z));
            _drone2.AddComponent(new ComponentGeometry(
            "Geometry/Intergalactic_Spaceship/Intergalactic_Spaceship.obj"));
            _drone2.AddComponent(new ComponentScale(0.23f));
            _drone2.AddComponent(new ComponentOffSet(0.5f));
            _drone2.AddComponent(new ComponentDroneAI());
            var ai2 = _drone2.Components.Find(c => c.ComponentType == ComponentTypes.COMPONENT_DRONE_AI) as ComponentDroneAI;
            if (ai2 != null)
            {
                ai2._mode = DroneMode.Patrol;
                ai2._Speed = 1.1f;
                ai2.chaseDistance = 12.0f;
                ai2.arriveDistance = 0.5f;

                ai2.patrolPoints.Add(CellWorld(16, 3));
                ai2.patrolPoints.Add(CellWorld(16, 16));
                ai2.patrolPoints.Add(CellWorld(3, 16));
                ai2.patrolPoints.Add(CellWorld(3, 3));

                ai2.currentPatrolIndex = 0;
            }
            entityManager.AddEntity(_drone2);
            droneSpawnPos[_drone2] = droneSpawn2;

            Entity _exit = new Entity("Exit");
            Vector3 exitPos = CellWorld(16, 2);
            _exit.AddComponent(new ComponentPosition(exitPos.X, 0.0f, exitPos.Z));
            _exit.AddComponent(new ComponentGeometry("Geometry/Door/DoorS3.obj"));
            _exit.AddComponent(new ComponentExit());
            entityManager.AddEntity(_exit);
        }

        private void CreateSystems()
        {
            Systems.System newSystem;

            //newSystem = new SystemRender();
            //systemManager.AddSystem(newSystem);
            systemManager.AddSystem(new SystemSkyboxRender());
            systemManager.AddSystem(new SystemRender());
            systemManager.AddSystem(new SystemDroneAI(this));
            systemManager.AddSystem(new SystemPlayerDrone(this));
            systemManager.AddSystem(new SystemPlayerKeyReach(this));
            systemManager.AddSystem(new SystemPlayerExit(this));
        }

        private void TrySubmitScore()
        {
            try
            {
                highscoreClient.PostScore(initialsInput, score);
                TryLoadHighscores(); 
            }
            catch
            {
                
            }
        }

        private void HandleInitialsInput()
        {
            var k = sceneManager.KeyboardState;

            for (Keys key = Keys.A; key <= Keys.Z; key++)
            {
                if (k.IsKeyDown(key) && initialsInput.Length < 3)
                {
                    initialsInput += key.ToString();
                    break;
                }
            }

            bool back = k.IsKeyDown(Keys.Backspace);
            if (back && !backWasDown && initialsInput.Length > 0)
                initialsInput = initialsInput.Substring(0, initialsInput.Length - 1);
            backWasDown = back;

            bool enter = k.IsKeyDown(Keys.Enter);
            if (enter && !enterWasDown && initialsInput.Length > 0)
            {
                if (!submittedScore)
                {
                    submittedScore = true;
                    TrySubmitScore();
                }

                enteringInitials = false;
                winTimer = 3.0f; 
            }
            enterWasDown = enter;
        }
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="e">Provides a snapshot of timing values.</param>
        public override void Update(FrameEventArgs e)
        {
            dt = (float)e.Time;

            if (initialsInputCooldown > 0.0f)
                initialsInputCooldown -= dt;


            if (enteringInitials)
                    return;


            if (!gameWon)
            {
                score += (int)(dt * 10.0f);
            }

            if (doorLocked)
            {
                doorUnlockTimer -= dt;
                if (doorUnlockTimer <= 0.0f)
                {
                    doorLocked = false;
                    doorUnlockTimer = 0.0f;
                }
            }

            if (gameWon)
            {
                winTimer -= dt;
                if (winTimer <= 0.0f)
                {
                    sceneManager.StartMenu();
                }
                return;
            }

            var _lives = player?.Components.Find(c => c.ComponentType == ComponentTypes.COMPONENT_LIVES) as ComponentLives;
            if (_lives != null && _lives._timer > 0.0f)
            {
                _lives._timer -= dt;
                if (_lives._timer < 0.0f) _lives._timer = 0.0f;
            }
            
            var keyboard = sceneManager.KeyboardState;
            bool mIsDown = keyboard.IsKeyDown(Keys.M);
            if (mIsDown && !mDown && !aiFrozen)
            {
                aiFrozen = true;
                aiFreezeTimer = AI_FREEZE_DURATION;
            }
            mDown = mIsDown;

            if (aiFrozen)
            {
                aiFreezeTimer -= dt;
                if (aiFreezeTimer <= 0.0f)
                {
                    aiFrozen = false;
                    aiFreezeTimer = 0.0f;
                }
            }

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

                    if (!CollidesWithWall(testPosX, PLAYER_RADIUS))
                        newPos.X = testPosX.X;

                    Vector3 testPosZ = new Vector3(newPos.X, newPos.Y, newPos.Z + _delta.Z);
                    if (!CollidesWithWall(testPosZ, PLAYER_RADIUS))
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

            systemManager.ActionSystems(entityManager);

            GUI.DrawText($"Score: {score}", 30, 70, 30, 255, 255, 255);
            GUI.DrawText($"Player pos: {lastPlayerPos.X: 0.00}, {lastPlayerPos.Y: 0.00}, {lastPlayerPos.Z: 0.00}", 30, 150, 24, 0, 255, 0);
            var _lives = player?.Components.Find(c => c.ComponentType == ComponentTypes.COMPONENT_LIVES) as ComponentLives;
            int livesLeft = _lives != null ? _lives._Lives : 0;

            string _heart = "";
            for (int i = 0; i < livesLeft; i++)
            {
                _heart += "* ";
            }
            GUI.DrawText($"Lives: {_heart}", 30, 190, 28, 255, 50, 50);
            if (aiFrozen)
            {
                GUI.DrawText($"Drones Frozen: {aiFreezeTimer:0.0} s", 30, 230, 24, 255, 255, 0);
            }
            else
            {
                GUI.DrawText("Press M to freeze drones", 30, 230, 24, 180, 180, 180);
            }

            if (doorLocked)
            {
                GUI.DrawText("Door is locked! Find the KEY", 30, 290, 24, 255, 120, 0 );
            }

            if (gameWon)
            {
                GUI.DrawText("YOU ESCAPED!!", 490, 260, 37, 255, 255, 0);
            }

            GUI.DrawText("Open the door to escape!", 30,  260, 24, 255, 255, 255);
            var inv = player?.Components.Find(c => c.ComponentType == ComponentTypes.COMPONENT_INVENTORY) as ComponentInventory;
            if (inv != null && inv.HasKey)
            {
                GUI.DrawText("You have the KEY!", 30,  320, 24, 0, 255, 0);
            }
            else
            {
                GUI.DrawText("You do NOT have the KEY!", 30,  320, 24, 255, 0, 0);
            }

            GUI.DrawText("Highscores:", 30, 360, 24, 255, 255, 255);

            for (int i = 0; i < Math.Min(5, highscores.Count); i++)
            {
                var h = highscores[i];
                GUI.DrawText($"{i + 1}. {h.Initials}  {h.Score}", 30, 390 + i * 25, 22, 255, 255, 0);
            }

            if (enteringInitials)
            {
                GUI.DrawText("NEW HIGHSCORE!", 450, 220, 32, 255, 255, 0);
                GUI.DrawText($"Enter Initials: {initialsInput}", 450, 260, 28, 255, 255, 255);
                GUI.DrawText("Press ENTER to submit", 450, 300, 20, 180, 180, 180);
            }
            GUI.Render();
        }

        /// <summary>
        /// This is called when the game exits.
        /// </summary>
        public override void Close()
        {
            sceneManager.keyboardDownDelegate -= Keyboard_KeyDown;

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

            int _centerX = 7;
            int _centerZ = 7;
            int centerW = 6;
            int centerH = 6;

            EmptyRoom(m, _centerX, _centerZ, centerW, centerH);

            FillRect(m, _centerX + 1, _centerZ + 1, centerW - 2, centerH - 2, 0);

            EmptyRoom(m, 1, 1, 7, 7);
            EmptyRoom(m, 12, 1, 7, 7);
            EmptyRoom(m, 12, 12, 7, 7);
            EmptyRoom(m, 1, 12, 7, 7);

            FillRect(m, 2, 2, 5, 5, 0);
            FillRect(m, 13, 2, 5, 5, 0);
            FillRect(m, 13, 13, 5, 5, 0);
            FillRect(m, 2, 13, 5, 5, 0);

            m[7, 4] = 0;
            m[6, 4] = 0;
            m[7, 15] = 0;
            m[6, 15] = 0;
            m[12, 15] = 0;
            m[13, 15] = 0;
            m[12, 4] = 0;
            m[13, 4] = 0;
            m[7, 9] = 0;
            m[12, 9] = 0;
            m[9, 7] = 0;
            m[9, 12] = 0;

            FillRect(m, 4, 6, 1, 2, 0);
            FillRect(m, 5, 6, 2, 1, 0);
            FillRect(m, 15, 6, 1, 2, 0);
            FillRect(m, 13, 6, 2, 1, 0);
            FillRect(m, 15, 12, 1, 2, 0);
            FillRect(m, 13, 13, 2, 1, 0);
            FillRect(m, 4, 12, 1, 2, 0);
            FillRect(m, 5, 13, 2, 1, 0);


            return m;
        }

        private void FillRect(int[,] m, int x0, int z0, int w, int h, int value)
        {
            for (int z = z0;  z < z0 + h; z++)
            {
                for (int x = x0; x < x0 + w; x++)
                {
                    if (x >= 0 && z >= 0 && x < GRID_W && z < GRID_H)
                    {
                        m[z, x] = value;
                    }
                }
            }
        }

        private void EmptyRoom(int[,] m, int x0,  int z0, int w, int h)
        {
            for (int x = x0; x < x0 + w; x++)
            {
                m[z0, x] = 1;
                m[z0 + h - 1, x] = 1;
            }

            for (int z = z0; z < z0 + h; z++)
            {
                m[z, x0] = 1;
                m[z, x0 + w - 1] = 1;
            }
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

                    _floor.AddComponent(new ComponentPosition(posX, 0.0f, posZ));
                    _floor.AddComponent(new ComponentGeometry(FLOOR_OBJ));
                    entityManager.AddEntity(_floor);

                    if (_maze[z, x] == 1)
                    {
                        Entity _wall = new Entity($"Wall_{x}_{z}");
                        _wall.AddComponent(new ComponentPosition(posX, 0.5f, posZ));
                        
                        _wall.AddComponent(new ComponentGeometry(WALL_OBJ));
                        entityManager.AddEntity(_wall);
                    }
                }
            }
        }

        private bool CollidesWithWall(Vector3 position, float radius)
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

                    if ((distanceX * distanceX + distanceZ * distanceZ) < (radius * radius)) return true;
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
            score = Math.Max(0, score - 50);
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

        private Vector3 CellWorld(int x, int z)
        {
            float startX = -(GRID_W * TILE) * 0.5f;
            float startZ = -(GRID_H * TILE) * 0.5f;
            return new Vector3(startX + (x * TILE), 0.0f, startZ + (z * TILE));
        }

        public void Keyboard_KeyDown(KeyboardKeyEventArgs e)
        {
            if (enteringInitials)
            {
                if (e.Key >= Keys.A && e.Key <= Keys.Z)
                {
                    if (initialsInput.Length < 3)
                        initialsInput += e.Key.ToString();
                    return;
                }

                if (e.Key == Keys.Backspace)
                {
                    if (initialsInput.Length > 0)
                        initialsInput = initialsInput.Substring(0, initialsInput.Length - 1);
                    return;
                }

                if (e.Key == Keys.Enter)
                {
                    if (initialsInput.Length > 0 && !submittedScore)
                    {
                        submittedScore = true;
                        TrySubmitScore();
                    }

                    enteringInitials = false;
                    winTimer = 3.0f;
                    return;
                }

                return; 
            }
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