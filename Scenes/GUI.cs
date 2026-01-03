using System;
using System.IO;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SkiaSharp;
using OpenGL_Game.OBJLoader;
using OpenGL_Game.Managers;

namespace OpenGL_Game.Scenes
{
    static class GUI
    {
        static int pgmID;
        static int uniform_stex;
        static int uniform_mmodelviewproj;

        static Geometry geometry;

        static private int textTexture;//The location of the texture on the graphics card

        static private int m_width, m_height;
        static public Vector2 guiPosition = Vector2.Zero;

        static private SKSurface surface;
        static private SKCanvas canvas;
        static public SKColor clearColour = SKColors.Transparent;
        static SKImageInfo info;

        public static void DrawText(String text, float x, float y, SKPaint paint)
        {
            canvas.DrawText(text, x, y, paint);

            CreateTexture();
        }

        public static void DrawText(String text, float x, float y, float size, byte red, byte green, byte blue)
        {
            SKPaint paint = new SKPaint();
            paint.TextSize = size;
            paint.StrokeWidth = 1;
            paint.IsAntialias = true;
            paint.TextAlign = SKTextAlign.Left;
            paint.Color = new SKColor(red, green, blue);
            paint.Style = SKPaintStyle.Fill;

            DrawText(text, x, y, paint);
        }


        private static void CreateTexture()
        {
            canvas.Flush();
            surface.Flush();

            // Enable the texture
            GL.Enable(EnableCap.Texture2D);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
            GL.BindTexture(TextureTarget.Texture2D, textTexture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
            SKImage textImage = surface.Snapshot();
            SKBitmap bmp = SKBitmap.FromImage(textImage);
            //FileStream file = File.Create("C://temp//canvas.png");
            //bmp.Encode(file, SKEncodedImageFormat.Png, 100);
            //file.Close();
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp.Width, bmp.Height, 0,
                            OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp.GetPixels());

            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Disable(EnableCap.Texture2D);

            // Dispose of data to avoid memory leak
            textImage.Dispose();
            bmp.Dispose();
        }

        //Called by SceneManager onLoad, and when screen size is changed
        public static void SetUpGUI(int width, int height)
        {
            info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
            surface = SKSurface.Create(info);
            canvas = surface.Canvas;
            canvas.Clear(clearColour);

            m_width = width;
            m_height = height;

            //Load the texture into the Graphics Card
            if (textTexture > 0)
            {
                GL.DeleteTexture(textTexture);
                textTexture = 0;
            }
            textTexture = GL.GenTexture();

            geometry = ResourceManager.LoadGeometry("Geometry/Text/text.obj");

            pgmID = GL.CreateProgram();
            int vsID;
            int fsID;
            LoadShader("Shaders/text.vert", ShaderType.VertexShader, pgmID, out vsID);
            LoadShader("Shaders/text.frag", ShaderType.FragmentShader, pgmID, out fsID);
            GL.LinkProgram(pgmID);

            GL.GetProgram(pgmID, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetProgramInfoLog(pgmID);
                Console.WriteLine(infoLog);
            }

            Console.WriteLine(GL.GetProgramInfoLog(pgmID));

            uniform_stex = GL.GetUniformLocation(pgmID, "s_texture");
            uniform_mmodelviewproj = GL.GetUniformLocation(pgmID, "ModelViewProjMat");
        }

        static void LoadShader(String filename, ShaderType type, int program, out int address)
        {
            address = GL.CreateShader(type);
            using (StreamReader sr = new StreamReader(filename))
            {
                GL.ShaderSource(address, sr.ReadToEnd());
            }
            GL.CompileShader(address);


            GL.GetShader(address, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(address);
                Console.WriteLine(infoLog);
            }

            GL.AttachShader(program, address);
        }

        static public void Render()
        {
            GL.UseProgram(pgmID);

            GL.Uniform1(uniform_stex, 0);
            GL.ActiveTexture(TextureUnit.Texture0);

            Matrix4 modelViewProjection = Matrix4.CreateTranslation(Vector3.Zero);
            GL.UniformMatrix4(uniform_mmodelviewproj, false, ref modelViewProjection);

            // Enable the texture
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);
            GL.Enable(EnableCap.Texture2D);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            geometry.Render(0, textTexture);

            GL.UseProgram(0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Disable(EnableCap.Texture2D);

            canvas.Clear(clearColour);
        }
    }
}
