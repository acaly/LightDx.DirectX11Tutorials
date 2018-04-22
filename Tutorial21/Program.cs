using LightDx;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tutorial21
{
    class Program
    {
        private struct MatrixBuffer
        {
            public Matrix4x4 World;
            public Matrix4x4 View;
            public Matrix4x4 Projection;
        }

        private struct CameraBuffer
        {
            public Vector3 CameraPosition;
        }

        private struct LightBuffer
        {
            public Vector4 DiffuseColor;
            public Vector4 SpecularColor;
            public float SpecularPower;
            public Vector3 LightDirection;
        }

        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var form = new Form
            {
                ClientSize = new Size(800, 600),
                Text = "Tutorial 21: Specular Mapping",
            };

            using (var device = LightDevice.Create(form))
            {
                //---------------------
                // Target & Pipeline
                //---------------------

                var target = new RenderTarget(device.GetDefaultTarget(),
                    device.CreateDefaultDepthStencilTarget());
                target.Apply();

                Pipeline pipeline = device.CompilePipeline(InputTopology.Triangle,
                    ShaderSource.FromResource("SpecMap_vs.fx", ShaderType.Vertex),
                    ShaderSource.FromResource("SpecMap_ps.fx", ShaderType.Pixel));
                pipeline.Apply();

                //---------------------
                // Vertex buffer
                //---------------------

                var vertexDataProcessor = pipeline.CreateVertexDataProcessor<ModelVertex>();
                VertexBuffer vertexBuffer;
                using (var stream = Assembly.GetEntryAssembly().GetManifestResourceStream("Tutorial21.cube.txt"))
                {
                    vertexBuffer = vertexDataProcessor.CreateImmutableBuffer(Model.ReadModelFile(stream));
                }

                //---------------------
                // Constant buffer (Matrix, VS)
                //---------------------

                var matrixBuffer = pipeline.CreateConstantBuffer<MatrixBuffer>();
                pipeline.SetConstant(ShaderType.Vertex, 0, matrixBuffer);
                
                void SetupProjMatrix()
                {
                    matrixBuffer.Value.Projection =
                        device.CreatePerspectiveFieldOfView((float)Math.PI / 4).Transpose();
                }
                device.ResolutionChanged += (sender, e) => SetupProjMatrix();

                matrixBuffer.Value.World = Matrix4x4.Identity.Transpose();
                SetupProjMatrix();

                //---------------------
                // Constant buffer (Camera, VS)
                //---------------------

                var cameraBuffer = pipeline.CreateConstantBuffer<CameraBuffer>();
                pipeline.SetConstant(ShaderType.Vertex, 1, cameraBuffer);

                //---------------------
                // Constant buffer (Light, PS)
                //---------------------

                var lightBuffer = pipeline.CreateConstantBuffer<LightBuffer>();
                pipeline.SetConstant(ShaderType.Pixel, 0, lightBuffer);

                lightBuffer.Value.DiffuseColor = Color.White.WithAlpha(1);
                lightBuffer.Value.LightDirection = Vector3.Normalize(new Vector3(0, 0, 1));
                lightBuffer.Value.SpecularColor = Color.White.WithAlpha(1);
                lightBuffer.Value.SpecularPower = 16;
                lightBuffer.Update();

                //---------------------
                // Texture
                //---------------------

                Texture2D CreateTextureFromResource(string name)
                {
                    using (var stream = Assembly.GetEntryAssembly().GetManifestResourceStream($"Tutorial21.{name}.dds"))
                    {
                        return device.CreateTexture2D(stream);
                    }
                }
                var tex1 = CreateTextureFromResource("stone02");
                var tex2 = CreateTextureFromResource("bump02");
                var tex3 = CreateTextureFromResource("spec02");
                pipeline.SetResource(0, tex1);
                pipeline.SetResource(1, tex2);
                pipeline.SetResource(2, tex3);

                //---------------------
                // Camera
                //---------------------

                var camera = new Camera();

                //---------------------
                // Start main loop
                //---------------------

                form.Show();

                var frameCounter = new FrameCounter();
                frameCounter.Start();

                device.RunMultithreadLoop(delegate ()
                {
                    // Update matrix buffer

                    var time = frameCounter.NextFrame() / 1000;
                    Matrix4x4 rotate = Matrix4x4.CreateRotationX(time * 3) *
                        Matrix4x4.CreateRotationY(time * 6) *
                        Matrix4x4.CreateRotationZ(time * 4);

                    matrixBuffer.Value.World *= rotate;
                    matrixBuffer.Value.View = camera.GetViewMatrix().Transpose();
                    matrixBuffer.Update();

                    // Update camera buffer

                    cameraBuffer.Value.CameraPosition = camera.Position;
                    cameraBuffer.Update();

                    // Clear and draw

                    target.ClearAll();
                    vertexBuffer.DrawAll();

                    device.Present(true);
                });
            }
        }
    }
}
