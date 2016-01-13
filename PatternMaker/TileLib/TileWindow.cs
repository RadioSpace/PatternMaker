using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

using System.IO;


namespace TileLib
{
    using Device = SharpDX.Direct3D11.Device;
    using Buffer = SharpDX.Direct3D11.Buffer;
    using Color = SharpDX.Mathematics.Interop.RawColor4;

    public partial class TileWindow : Form
    {
        /// <summary>
        /// Device
        /// </summary>
        Device d;
        /// <summary>
        /// Swap Chain
        /// </summary>
        SwapChain sc;

        /// <summary>
        /// render target 
        /// </summary>
        Texture2D target;
        /// <summary>
        /// render target view for binding to the output stream
        /// </summary>
        RenderTargetView targetView;

        /// <summary>
        /// Index Buffer
        /// </summary>
        Buffer ib;

        /// <summary>
        /// pixel shader
        /// </summary>
        PixelShader ps;
        /// <summary>
        /// vertex shader
        /// </summary>
        VertexShader vs;

        //thread stuff

        /// <summary>
        /// the task that handles the Run Method for the main game loop
        /// </summary>
        Task engine;
        /// <summary>
        /// stops the engine
        /// </summary>
        CancellationTokenSource brakes;

        ////////////////

        //toolset objects
        //////////////////////


        public Color clearColor;

        /// <summary>
        /// creates the tile window
        /// </summary>      
        public TileWindow()
        {
            InitializeComponent();

            InitializeGraphics();

            brakes = new CancellationTokenSource();
            engine = new Task(Run, brakes.Token);

            if (d != null)
            {
                engine.Start();
            }
            else
            {
                MessageBox.Show("device was not created");
            }

        }




        private void InitializeGraphics()
        {

            //generate data///////////////////////////////////////////////////////////////////////////////////////////////

            uint[] indices = new uint[]
            {
                0,1,
                1,2,
                2,3,
                3,4,
                4,5,
                5,0,

            };

            


            //create directx objects///////////////////////////////////////////////////////////////////////////////////



            SwapChainDescription scd = new SwapChainDescription()
            {
                BufferCount = 4,
                IsWindowed = true,
                Flags = SwapChainFlags.None,
                ModeDescription = new ModeDescription(ClientSize.Width, ClientSize.Height, new Rational(60, 1), Format.R8G8B8A8_UNorm),
                OutputHandle = Handle,
                SampleDescription = new SampleDescription(8, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };

            try
            {
                Device.CreateWithSwapChain(SharpDX.Direct3D.DriverType.Hardware, DeviceCreationFlags.None, scd, out d, out sc);
            }
            catch (Exception EX)
            { 
                MessageBox.Show("Could not create Device and/or swap chain  :" + EX.Message + "\r\n\r\n" + EX.StackTrace);

                
                return;
            }


            target = Texture2D.FromSwapChain<Texture2D>(sc, 0);
            targetView = new RenderTargetView(d, target);

            ib = Buffer.Create(d, BindFlags.IndexBuffer, indices);

            vs = new VertexShader(d, File.ReadAllBytes("Shaders\\VertexShader.cso"));

            ps = new PixelShader(d, File.ReadAllBytes("Shaders\\PixelShader.cso"));
                       

            //set the objects on the device//////////////////////////////////////////////////////////////////////////////////////

            d.ImmediateContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.LineList;            
            d.ImmediateContext.InputAssembler.SetIndexBuffer(ib, Format.R32_UInt, 0);

            d.ImmediateContext.VertexShader.Set(vs);
            d.ImmediateContext.PixelShader.Set(ps);

            d.ImmediateContext.Rasterizer.SetViewport(0, 0, ClientSize.Width, ClientSize.Height);
            d.ImmediateContext.Rasterizer.State = new RasterizerState(d, new RasterizerStateDescription()
            {
                CullMode = CullMode.None,
                FillMode = FillMode.Solid
               
            });
           

            d.ImmediateContext.OutputMerger.SetRenderTargets(targetView);

            clearColor = new Color(System.Drawing.Color.CornflowerBlue.R/255f,
                System.Drawing.Color.CornflowerBlue.G / 255f,
                System.Drawing.Color.CornflowerBlue.B / 255f,
                System.Drawing.Color.CornflowerBlue.A / 255f);

        }




        void Run()
        {
            while (!brakes.IsCancellationRequested)
            {
                d.ImmediateContext.ClearRenderTargetView(targetView,clearColor);

                d.ImmediateContext.DrawIndexed(12,0,0);

                sc.Present(0, PresentFlags.None);
            }
        }


        protected override void OnClosed(EventArgs e)
        {
            if (brakes != null)
            {
                brakes.Cancel();

                if (engine != null)
                {
                    engine.Wait();

                    engine.Dispose();
                    brakes.Dispose();

                }
            }


            if (d != null) d.Dispose();
            if (sc != null) sc.Dispose();

            if (target != null) target.Dispose();
            if (targetView != null) targetView.Dispose();

            if (ps!= null) ps.Dispose();
            if (vs != null) vs.Dispose();

            if (ib != null) ib.Dispose();

            base.OnClosed(e);
        }
    }

    
}
