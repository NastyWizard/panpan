using System.Runtime.InteropServices;
using System.Text;
using panpan.Rendering.Util;
using SDL3;

namespace panpan.Rendering
{
    public class Material
    {
        nint pipeline;
        public nint Pipeline
        {
            get { return pipeline; }
        }

        private const string LOG_TAG = "panpan-Material";

        public Material(Shader? frag = null, Shader? vert = null)
        {
            if (vert == null)
            {
                vert = DefaultShaders.StandardVert;
            }

            if (frag == null)
            {
                frag = DefaultShaders.StandardFrag;
            }

            nint vertexShader;
            nint fragmentShader;

            byte[] entryPoint = Encoding.UTF8.GetBytes("main\0");

            // Load Vertex shader (SPIR-V bytes)
            SDL.GPUShaderCreateInfo vertexInfo = new();
            unsafe
            {
                fixed (byte* vertPtr = vert.Value.Data)
                fixed (byte* entryPtr = entryPoint)
                {
                    vertexInfo.CodeSize = (nuint)vert.Value.Data.Length;
                    vertexInfo.Code = (nint)vertPtr;
                    vertexInfo.Entrypoint = (nint)entryPtr;
                    vertexInfo.Stage = SDL.GPUShaderStage.Vertex;
                    vertexInfo.Format = SDL.GPUShaderFormat.SPIRV;
                    vertexInfo.NumUniformBuffers = vert.Value.NumUnifromBuffers;
                    vertexInfo.NumSamplers = vert.Value.NumSamplers;

                    vertexShader = SDL.CreateGPUShader(App.GetDevice(), vertexInfo);
                    if (vertexShader == nint.Zero)
                    {
                        Log.Error($"Failed to create vertex shader: {SDL.GetError()}", LOG_TAG);
                    }
                }
            }

            // Load Fragment shader (SPIR-V bytes)
            SDL.GPUShaderCreateInfo fragInfo = new();
            unsafe
            {
                fixed (byte* fragPtr = frag.Value.Data)
                fixed (byte* entryPtr = entryPoint)
                {
                    fragInfo.CodeSize = (nuint)frag.Value.Data.Length;
                    fragInfo.Code = (nint)fragPtr;
                    fragInfo.Entrypoint = (nint)entryPtr;
                    fragInfo.Stage = SDL.GPUShaderStage.Fragment;
                    fragInfo.Format = SDL.GPUShaderFormat.SPIRV;
                    fragInfo.NumUniformBuffers = frag.Value.NumUnifromBuffers;
                    fragInfo.NumSamplers = frag.Value.NumSamplers;

                    fragmentShader = SDL.CreateGPUShader(App.GetDevice(), fragInfo);
                    if (fragmentShader == nint.Zero)
                    {
                        Log.Error($"Failed to create fragment shader: {SDL.GetError()}", LOG_TAG);
                    }
                }
            }

            // Vertex buffer description
            SDL.GPUVertexBufferDescription[] vertexBufferDescriptions = [
                new SDL.GPUVertexBufferDescription()
            ];
            vertexBufferDescriptions[0].Slot = 0;
            vertexBufferDescriptions[0].InputRate = SDL.GPUVertexInputRate.Vertex;
            vertexBufferDescriptions[0].InstanceStepRate = 0;
            unsafe
            {
                vertexBufferDescriptions[0].Pitch = (uint)sizeof(Vertex);
            }

            // Pipeline info
            var pipelineInfo = new SDL.GPUGraphicsPipelineCreateInfo();
            pipelineInfo.VertexShader = vertexShader;
            pipelineInfo.FragmentShader = fragmentShader;
            pipelineInfo.PrimitiveType = SDL.GPUPrimitiveType.TriangleList;
            pipelineInfo.VertexInputState.NumVertexBuffers = 1;
            pipelineInfo.VertexInputState.VertexBufferDescriptions = SDL.StructureArrayToPointer(vertexBufferDescriptions);

            // Vertex attributes
            SDL.GPUVertexAttribute[] vertexAttributes = new SDL.GPUVertexAttribute[3];

            // a_position
            vertexAttributes[0] = new SDL.GPUVertexAttribute();
            vertexAttributes[0].BufferSlot = 0;
            vertexAttributes[0].Location = 0;
            vertexAttributes[0].Format = SDL.GPUVertexElementFormat.Float3;
            vertexAttributes[0].Offset = 0;

            // a_color
            vertexAttributes[1] = new SDL.GPUVertexAttribute();
            vertexAttributes[1].BufferSlot = 0;
            vertexAttributes[1].Location = 1;
            vertexAttributes[1].Format = SDL.GPUVertexElementFormat.Float4;
            vertexAttributes[1].Offset = sizeof(float) * 3;

            // a_uv
            vertexAttributes[2] = new SDL.GPUVertexAttribute();
            vertexAttributes[2].BufferSlot = 0;
            vertexAttributes[2].Location = 2;
            vertexAttributes[2].Format = SDL.GPUVertexElementFormat.Float2;
            vertexAttributes[2].Offset = sizeof(float) * 3 + sizeof(float) * 4;

            pipelineInfo.VertexInputState.NumVertexAttributes = 3;
            pipelineInfo.VertexInputState.VertexAttributes = SDL.StructureArrayToPointer(vertexAttributes);
            // This engine is primarily 2D; most render passes do not provide a depth attachment.
            // Enabling depth without a depth target can prevent rendering on some backends.
            pipelineInfo.DepthStencilState.EnableDepthTest = 1;
            pipelineInfo.DepthStencilState.EnableDepthWrite = 1;
            pipelineInfo.DepthStencilState.CompareOp = SDL.GPUCompareOp.Less;
            

            SDL.GPUColorTargetDescription[] colorTargetDescriptions = new SDL.GPUColorTargetDescription[1];
            colorTargetDescriptions[0] = new SDL.GPUColorTargetDescription();
            colorTargetDescriptions[0].Format = SDL.GetGPUSwapchainTextureFormat(App.GetDevice(), App.GetWindow());
            colorTargetDescriptions[0].BlendState = new SDL.GPUColorTargetBlendState();
            // Configure for basic transparent blend mode (alpha blending)
            colorTargetDescriptions[0].BlendState.SrcColorBlendfactor = SDL.GPUBlendFactor.SrcAlpha;
            colorTargetDescriptions[0].BlendState.DstColorBlendfactor = SDL.GPUBlendFactor.OneMinusSrcAlpha;
            colorTargetDescriptions[0].BlendState.ColorBlendOp = SDL.GPUBlendOp.Add;
            colorTargetDescriptions[0].BlendState.SrcAlphaBlendfactor = SDL.GPUBlendFactor.One;
            colorTargetDescriptions[0].BlendState.DstAlphaBlendfactor = SDL.GPUBlendFactor.OneMinusSrcAlpha;
            colorTargetDescriptions[0].BlendState.AlphaBlendOp = SDL.GPUBlendOp.Add;
            colorTargetDescriptions[0].BlendState.EnableBlend = 1;
            colorTargetDescriptions[0].BlendState.EnableColorWriteMask = 0; // Write to all channels

            pipelineInfo.TargetInfo.NumColorTargets = 1;
            pipelineInfo.TargetInfo.ColorTargetDescriptions = SDL.StructureArrayToPointer(colorTargetDescriptions);
            pipelineInfo.TargetInfo.HasDepthStencilTarget = 1;
            pipelineInfo.TargetInfo.DepthStencilFormat = SDL.GPUTextureFormat.D16Unorm;
            

            //pipelineInfo.RasterizerState.CullMode = SDL.GPUCullMode.Back;

            pipeline = SDL.CreateGPUGraphicsPipeline(App.GetDevice(), pipelineInfo);

            if (pipeline == nint.Zero)
            {
                Log.Error($"Failed to create material: {SDL.GetError()}", LOG_TAG);
            }

            SDL.ReleaseGPUShader(App.GetDevice(), vertexShader);
            SDL.ReleaseGPUShader(App.GetDevice(), fragmentShader);
        }

        public void SetUniformFloat(float[] uniforms)
        {
            unsafe
            {
                fixed (float* ptr = &uniforms[0])
                {
                    uint len = (uint)(sizeof(float) * uniforms.Length);
                    SDL.PushGPUFragmentUniformData(App.GetCommandBuffer(), 0, (nint)ptr, len);
                }
            }
        }
        
    }
}
