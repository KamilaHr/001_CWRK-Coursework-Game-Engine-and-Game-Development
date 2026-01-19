using OpenTK.Graphics.OpenGL;
using OpenGL_Game.Managers;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SkiaSharp;
using System.Numerics;
using OpenTK.Mathematics;

namespace OpenGL_Game.Scenes
{
    class MainMenuScene : Scene
    {
        private bool enterWasDown = false;

        public MainMenuScene(SceneManager sceneManager) : base(sceneManager)
        {
            // Set the title of the window
            sceneManager.Title = "Main Menu";
            // Set the Render and Update delegates to the Update and Render methods of this class
            sceneManager.renderer = Render;
            sceneManager.updater = Update;

            sceneManager.mouseDelegate += Mouse_BottonPressed;
            sceneManager.keyboardDownDelegate += Keyboard_KeyDown;

            GL.ClearColor(0.2f, 0.75f, 1.0f, 1.0f);
        }

        private void Keyboard_KeyDown(KeyboardKeyEventArgs e)
        {
            if (e.Key == Keys.Enter)
            {
                sceneManager.StartNewGame();
            }
            else if (e.Key == Keys.Escape)
            {
                sceneManager.Close();
            }
        }

        public override void Update(FrameEventArgs e)
        {
            var keyboard =  sceneManager.KeyboardState;
            bool enterIsDown = keyboard.IsKeyDown(Keys.Enter);

            if (enterIsDown && !enterWasDown)
            {
                sceneManager.StartNewGame();
                return;
            }
            enterWasDown = enterIsDown;
        }

        public override void Render(FrameEventArgs e)
        {
            GL.Viewport(0, 0, sceneManager.Size.X, sceneManager.Size.Y);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, sceneManager.Size.X, 0, sceneManager.Size.Y, -1, 1);

            //Display the Title using an outlined text
            SKPaint paint = new SKPaint();
            paint.TextSize = 100;
            paint.StrokeWidth = 2;
            paint.TextAlign = SKTextAlign.Center;
            paint.IsAntialias = true;
            paint.Color = SKColors.Yellow;
            paint.Style = SKPaintStyle.Fill;
            GUI.DrawText("Main Menu", sceneManager.Size.X * 0.5f, 150, paint);
            paint.Color = SKColors.DarkBlue;
            paint.Style = SKPaintStyle.Stroke;
            GUI.DrawText("Main Menu", sceneManager.Size.X * 0.5f, 150, paint);

            paint.TextSize = 36;
            paint.StrokeWidth = 2;
            paint.TextAlign = SKTextAlign.Center;
            paint.IsAntialias = true;

            paint.Color = SKColors.FloralWhite;
            paint.Style = SKPaintStyle.Fill;
            GUI.DrawText("Press ENTER to Start", sceneManager.Size.X * 0.5f, 260, paint);

            paint.Color = SKColors.DarkBlue;
            paint.Style = SKPaintStyle.Stroke;
            GUI.DrawText("Press ENTER to Start", sceneManager.Size.X * 0.5f, 260, paint);

            GUI.Render();
        }

        public void Mouse_BottonPressed(MouseButtonEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButton.Left:
                    sceneManager.StartNewGame();
                    break;
            }
        }

        public override void Close()
        {
            sceneManager.mouseDelegate -= Mouse_BottonPressed;
            sceneManager.keyboardDownDelegate -= Keyboard_KeyDown;
        }
    }
}