using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using OpenGL_Game.Components;
using OpenTK.Graphics.OpenGL4;
using OpenGL_Game.Objects;
using OpenGL_Game.Scenes;
using OpenGL_Game.OBJLoader;


namespace OpenGL_Game.Systems
{
    class SystemSkyboxRender : System
    {
        const ComponentTypes MASK = (ComponentTypes.COMPONENT_GEOMETRY);

        int pgmID, vsID, fsID;
        int uView, uProj;

        public SystemSkyboxRender()
        {
            pgmID = GL.CreateProgram();
            LoadShader("Shaders/skybox.vert", ShaderType.VertexShader, pgmID, out vsID);
            LoadShader("Shaders/skybox.frag", ShaderType.FragmentShader, pgmID, out fsID);

            GL.LinkProgram(pgmID);

            uView = GL.GetUniformLocation(pgmID, "ViewMat");
            uProj = GL.GetUniformLocation(pgmID, "ProjMat");
        }

        void LoadShader(string filePath, ShaderType type, int programID, out int shaderID)
        {
            shaderID = GL.CreateShader(type);
            using (StreamReader sr = new StreamReader(filePath))
                GL.ShaderSource(shaderID, sr.ReadToEnd());

            GL.CompileShader(shaderID);
            GL.AttachShader(programID, shaderID);
        }

        public override void OnAction(Entity entity)
        {
            if ((entity.Mask & MASK) != MASK) return;

            if (entity.Name != "Skybox") return;

            var geometryComp = GetComponent(entity, ComponentTypes.COMPONENT_GEOMETRY) as ComponentGeometry;
            Geometry geometry = geometryComp.Geometry();

            GL.DepthMask(false);
            GL.DepthFunc(DepthFunction.Lequal);
            GL.Disable(EnableCap.CullFace);

            GL.UseProgram(pgmID);

            Matrix4 view = GameScene.gameInstance.camera.view;
            Matrix4 proj = GameScene.gameInstance.camera.projection;

            //GL.UniformMatrix4(uView, false, ref view);
            Matrix4 viewNoTr = new Matrix4(new Matrix3(view));

            float t = (float)DateTime.Now.TimeOfDay.TotalSeconds;
            Matrix4 _rotation = Matrix4.CreateRotationY(t * 0.05f);

            Matrix4 viewRotOnly = _rotation * viewNoTr;

            GL.UniformMatrix4(uView, false, ref viewRotOnly);
            GL.UniformMatrix4(uProj, false, ref proj);

            geometry.Render(-1);

            GL.UseProgram(0);

            GL.DepthMask(true);
            GL.DepthFunc(DepthFunction.Less);
            GL.Enable(EnableCap.CullFace);
        }
    }
}
