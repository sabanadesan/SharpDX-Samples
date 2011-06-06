// Copyright (c) 2010-2011 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D10;
using SharpDX.DXGI;
using SharpDX.Windows;

using Buffer = SharpDX.Direct3D10.Buffer;
using Device = SharpDX.Direct3D10.Device;
using DriverType = SharpDX.Direct3D10.DriverType;

namespace MiniTri
{
    /// <summary>
    ///   SharpDX port of SlimDX-MiniTri Direct3D 10 Sample
    /// </summary>
    internal static class ProgramWithNoEffect
    {
        [STAThread]
        private static void Main()
        {
            var form = new RenderForm("SharpDX - MiniTri Direct3D 10 Sample");

            // SwapChain description
            var desc = new SwapChainDescription()
                           {
                               BufferCount = 1,
                               ModeDescription = 
                                   new ModeDescription(form.ClientSize.Width, form.ClientSize.Height,
                                                       new Rational(60, 1), Format.R8G8B8A8_UNorm),
                               IsWindowed = true,
                               OutputHandle = form.Handle,
                               SampleDescription = new SampleDescription(1, 0),
                               SwapEffect = SwapEffect.Discard,
                               Usage = Usage.RenderTargetOutput
                           };

            // Create Device and SwapChain
            Device device;
            SwapChain swapChain;
            Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.Debug, desc, out device, out swapChain);


            // Ignore all windows events
            Factory factory = swapChain.GetParent<Factory>();
            factory.MakeWindowAssociation(form.Handle, WindowAssociationFlags.IgnoreAll);

            // New RenderTargetView from the backbuffer
            Texture2D backBuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0);
            var renderView = new RenderTargetView(device, backBuffer);

            // Compile Vertex and Pixel shaders
            var vertexShaderByteCode = ShaderBytecode.CompileFromFile("MiniTri.fx", "VS", "vs_4_0", ShaderFlags.None,
                                                                      EffectFlags.None);
            var vertexShader = new VertexShader(device, vertexShaderByteCode);

            var pixelShaderByteCode = ShaderBytecode.CompileFromFile("MiniTri.fx", "PS", "ps_4_0", ShaderFlags.None,
                                                                     EffectFlags.None);
            var pixelShader = new PixelShader(device, pixelShaderByteCode);

            // Layout from VertexShader input signature
            var layout = new InputLayout(device, ShaderSignature.GetInputSignature(vertexShaderByteCode), new[] { 
                new InputElement("POSITION",0,Format.R32G32B32A32_Float,0,0),
                new InputElement("COLOR",0,Format.R32G32B32A32_Float,16,0)
            });

            // Write vertex data to a datastream
            var stream = new DataStream(32*3, true, true);
            stream.WriteRange(new[]
                                  {
                                      new Vector4(0.0f, 0.5f, 0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                                      new Vector4(0.5f, -0.5f, 0.5f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                                      new Vector4(-0.5f, -0.5f, 0.5f, 1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f)
                                  });
            stream.Position = 0;

            // Instantiate Vertex buiffer from vertex data
            var vertices = new Buffer(device, stream, new BufferDescription()
                                                          {
                                                              BindFlags = BindFlags.VertexBuffer,
                                                              CpuAccessFlags = CpuAccessFlags.None,
                                                              OptionFlags = ResourceOptionFlags.None,
                                                              SizeInBytes = 32*3,
                                                              Usage = ResourceUsage.Default,
                                                          });
            stream.Dispose();

            // Prepare All the stages
            device.InputAssembler.SetInputLayout(layout);
            device.InputAssembler.SetPrimitiveTopology(PrimitiveTopology.TriangleList);
            device.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertices, 32, 0));
            device.VertexShader.Set(vertexShader);
            device.Rasterizer.SetViewports(new Viewport(0, 0, form.ClientSize.Width, form.ClientSize.Height, 0.0f, 1.0f));
            device.PixelShader.Set(pixelShader);
            device.OutputMerger.SetTargets(renderView);

            // Main loop
            RenderLoop.Run(form, () =>
                                      {
                                          device.ClearRenderTargetView(renderView, new Color4(1.0f, 0.0f, 0.0f, 0.0f));
                                          device.Draw(3, 0);
                                          swapChain.Present(0, PresentFlags.None);
                                      });

            // Release all resources
            vertexShaderByteCode.Dispose();
            vertexShader.Dispose();
            pixelShaderByteCode.Dispose();
            pixelShader.Dispose();
            vertices.Dispose();
            layout.Dispose();
            renderView.Dispose();
            backBuffer.Dispose();
            device.ClearState();
            device.Flush();
            device.Dispose();
            device.Dispose();
            swapChain.Dispose();
            factory.Dispose();
        }
    }
}