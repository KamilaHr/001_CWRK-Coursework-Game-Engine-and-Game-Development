using System;
using OpenTK.Graphics.OpenGL;
using OpenGL_Game.Scenes;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Audio.OpenAL; // NEW for Audio
using System.Diagnostics;

namespace OpenGL_Game.Managers
{
    class SceneManager : GameWindow
    {
        Scene scene;
        public static int width = 1200, height = 800;
        public static int windowXPos = 200, windowYPos = 80;

        public delegate void SceneDelegate(FrameEventArgs e);
        public SceneDelegate renderer;
        public SceneDelegate updater;

        public delegate void KeyboardDelegate(KeyboardKeyEventArgs e);
        public KeyboardDelegate keyboardDownDelegate;
        public KeyboardDelegate keyboardUpDelegate;

        public delegate void MouseDelegate(MouseButtonEventArgs e);
        public MouseDelegate mouseDelegate;

        public SceneManager() : base(GameWindowSettings.Default, new NativeWindowSettings()
                                { ClientSize = (width, height), Location = (windowXPos, windowYPos) })
        {
            var device = ALC.OpenDevice(null);  // NEW for Audio
            ALContextAttributes att = new ALContextAttributes();  // NEW for Audio
            var context = ALC.CreateContext(device, att);  // NEW for Audio
            ALC.MakeContextCurrent(context);  // NEW for Audio

            var version = AL.Get(ALGetString.Version);  // NEW for Audio
            var vendor = AL.Get(ALGetString.Vendor);  // NEW for Audio
            var renderer = AL.Get(ALGetString.Renderer);  // NEW for Audio
            Console.WriteLine(version);  // NEW for Audio
            Console.WriteLine(vendor);  // NEW for Audio
            Console.WriteLine(renderer);  // NEW for Audio
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);
            //if (e.Key == Keys.Escape) this.Close();
            Debug.WriteLine($"Key Down: {e.Key}");
            if (keyboardDownDelegate != null) keyboardDownDelegate.Invoke(e);
        }

        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (keyboardUpDelegate != null) keyboardUpDelegate.Invoke(e);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if(mouseDelegate != null) mouseDelegate.Invoke(e);
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.Enable(EnableCap.DepthTest);
            GL.DepthMask(true);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            //Load the GUI
            GUI.SetUpGUI(width, height);

            StartMenu();
            this.Focus();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            updater(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            renderer(e);

            GL.Flush();
            SwapBuffers();
        }

        public void StartNewGame()
        {
            if(scene != null) scene.Close();
            scene = new GameScene(this);
            this.Focus();
        }

        public void StartMenu()
        {
            if (scene != null) scene.Close();
            scene = new MainMenuScene(this);
            this.Focus();
        }

        public static int WindowWidth
        {
            get { return width; }
        }

        public static int WindowHeight
        {
            get { return height; }
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, e.Width, e.Height);
            SceneManager.width = e.Width;
            SceneManager.height = e.Height;

            //Load the GUI
            GUI.SetUpGUI(e.Width, e.Height);
        }
    }

}