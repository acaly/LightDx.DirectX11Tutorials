using LightDx;
using LightDx.InputAttributes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tutorial04
{
    class Program
    {
        private struct Vertex
        {
            [Position]
            public Vector4 Position;
            [Color]
            public Vector4 Color;
        }

        private struct Constants
        {
            public Matrix4x4 World;
            public Matrix4x4 View;
            public Matrix4x4 Projection;
        }

        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var form = new Form
            {
                ClientSize = new Size(800, 600),
                Text = "Tutorial 4: Buffers, Shaders, and HLSL",
            };

            using (var device = LightDevice.Create(form))
            {
                //---------------------
                // Target & Pipeline
                //---------------------

                var target = new RenderTarget(device.GetDefaultTarget());
                target.Apply();

                Pipeline pipeline = device.CompilePipeline(InputTopology.Triangle,
                    ShaderSource.FromResource("Color_vs.fx", ShaderType.Vertex),
                    ShaderSource.FromResource("Color_ps.fx", ShaderType.Pixel));
                pipeline.Apply();

                //---------------------
                // Vertex buffer
                //---------------------

                var vertexDataProcessor = pipeline.CreateVertexDataProcessor<Vertex>();
                var vertexBuffer = vertexDataProcessor.CreateImmutableBuffer(new[] {
                    new Vertex { Position = new Vector4(-1, -1, 0, 1), Color = new Vector4(0, 1, 0, 1) },
                    new Vertex { Position = new Vector4(0, 1, 0, 1), Color = new Vector4(0, 1, 0, 1) },
                    new Vertex { Position = new Vector4(1, -1, 0, 1), Color = new Vector4(0, 1, 0, 1) },
                });

                //---------------------
                // Index buffer
                //---------------------

                var indexBuffer = pipeline.CreateImmutableIndexBuffer(new uint[] { 0, 1, 2 });

                //---------------------
                // Constant buffer (VS)
                //---------------------

                var constantBuffer = pipeline.CreateConstantBuffer<Constants>();
                pipeline.SetConstant(ShaderType.Vertex, 0, constantBuffer);

                void SetupProjMatrix()
                {
                    constantBuffer.Value.Projection =
                        device.CreatePerspectiveFieldOfView((float)Math.PI / 4).Transpose();
                }
                device.ResolutionChanged += (sender, e) => SetupProjMatrix();

                constantBuffer.Value.World = Matrix4x4.Identity.Transpose();
                SetupProjMatrix();

                //---------------------
                // Camera
                //---------------------

                var camera = new Camera();

                //---------------------
                // Start main loop
                //---------------------

                form.Show();

                device.RunMultithreadLoop(delegate ()
                {
                    // Update matrix

                    constantBuffer.Value.View = camera.GetViewMatrix().Transpose();
                    constantBuffer.Update();

                    // Clear and draw

                    target.ClearAll();
                    indexBuffer.DrawAll(vertexBuffer);

                    device.Present(true);
                });
            }
        }
    }
}
