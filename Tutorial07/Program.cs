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

namespace Tutorial07
{
    class Program
    {
        private struct VSConstants
        {
            public Matrix4x4 World;
            public Matrix4x4 View;
            public Matrix4x4 Projection;
        }

        private struct PSConstants
        {
            public Vector4 Diffuse;
            public Vector3 LightDir;
        }

        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var form = new Form
            {
                ClientSize = new Size(800, 600),
                Text = "Tutorial 7: 3D Model Rendering",
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
                    ShaderSource.FromResource("Light_vs.fx", ShaderType.Vertex),
                    ShaderSource.FromResource("Light_ps.fx", ShaderType.Pixel));
                pipeline.Apply();

                //---------------------
                // Vertex buffer
                //---------------------

                var vertexDataProcessor = pipeline.CreateVertexDataProcessor<ModelVertex>();
                VertexBuffer vertexBuffer;
                using (var stream = Assembly.GetEntryAssembly().GetManifestResourceStream("Tutorial07.Cube.txt"))
                {
                    vertexBuffer = vertexDataProcessor.CreateImmutableBuffer(Model.ReadModelFile(stream));
                }

                //---------------------
                // Constant buffer (VS)
                //---------------------

                var constantBufferVS = pipeline.CreateConstantBuffer<VSConstants>();
                pipeline.SetConstant(ShaderType.Vertex, 0, constantBufferVS);
                
                void SetupProjMatrix()
                {
                    constantBufferVS.Value.Projection =
                        device.CreatePerspectiveFieldOfView((float)Math.PI / 4).Transpose();
                }
                device.ResolutionChanged += (sender, e) => SetupProjMatrix();

                constantBufferVS.Value.World = Matrix4x4.Identity.Transpose();
                SetupProjMatrix();
                
                //---------------------
                // Constant buffer (PS)
                //---------------------

                var constantBufferPS = pipeline.CreateConstantBuffer<PSConstants>();
                pipeline.SetConstant(ShaderType.Pixel, 0, constantBufferPS);

                constantBufferPS.Value.Diffuse = Color.White.WithAlpha(1);
                constantBufferPS.Value.LightDir = Vector3.Normalize(new Vector3(-3f, -4f, 6f));
                constantBufferPS.Update();

                //---------------------
                // Texture
                //---------------------

                Texture2D tex;
                using (var stream = Assembly.GetEntryAssembly().GetManifestResourceStream("Tutorial07.seafloor.dds"))
                {
                    tex = device.CreateTexture2D(stream);
                }
                pipeline.SetResource(0, tex);

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
                    // Update matrix

                    var time = frameCounter.NextFrame() / 1000;
                    Matrix4x4 rotate = Matrix4x4.CreateRotationX(time * 3) *
                        Matrix4x4.CreateRotationY(time * 6) *
                        Matrix4x4.CreateRotationZ(time * 4);

                    constantBufferVS.Value.World *= rotate;
                    constantBufferVS.Value.View = camera.GetViewMatrix().Transpose();
                    constantBufferVS.Update();

                    // Clear and draw
                    target.ClearAll();
                    vertexBuffer.DrawAll();

                    device.Present(true);
                });
            }
        }
    }
}
