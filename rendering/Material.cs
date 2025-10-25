using System.ComponentModel;
using System.IO.Pipelines;
using System.Runtime.InteropServices;
using panpan;
using panpan.Assets;
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
        public Material(byte[]? frag = null, byte[]? vert = null)
        {
            if (vert == null)
            {
                vert = Assets.Shaders.standard_vert_hlsl;
            }

            if (frag == null)
            {
                frag = Assets.Shaders.standard_frag_hlsl;
            }

            // convert from hlsl to spirv
            ShaderCross.HLSLInfo vertInfoHLSL = new ShaderCross.HLSLInfo();
            vertInfoHLSL.Entrypoint = "main";
            vertInfoHLSL.Source = System.Text.Encoding.UTF8.GetString(vert);
            vertInfoHLSL.ShaderStage = ShaderCross.ShaderStage.Vertex;
            nuint vertSize;
            var vertPtr = ShaderCross.CompileSPIRVFromHLSL(vertInfoHLSL, out vertSize);

            ShaderCross.HLSLInfo fragInfoHLSL = new ShaderCross.HLSLInfo();
            fragInfoHLSL.Entrypoint = "main";
            fragInfoHLSL.Source = System.Text.Encoding.UTF8.GetString(frag);
            fragInfoHLSL.ShaderStage = ShaderCross.ShaderStage.Fragment;
            nuint fragSize;
            var fragPtr = ShaderCross.CompileSPIRVFromHLSL(fragInfoHLSL, out fragSize);
            //

            nint vertexShader;
            nint fragmentShader;

            // Load Vertex shader
            ShaderCross.SPIRVInfo vertexInfo = new ShaderCross.SPIRVInfo();
            vertexInfo.ByteCode = vertPtr;
            vertexInfo.ByteCodeSize = vertSize;
            vertexInfo.Entrypoint = "main";
            vertexInfo.ShaderStage = ShaderCross.ShaderStage.Vertex;
            var vertexMetadataPtr = ShaderCross.ReflectGraphicsSPIRV(vertexInfo.ByteCode, vertSize, 0);
            ShaderCross.GraphicsShaderMetadata vertexMetadata = Marshal.PtrToStructure<ShaderCross.GraphicsShaderMetadata>(vertexMetadataPtr);

            vertexShader = ShaderCross.CompileGraphicsShaderFromSPIRV(App.GetDevice(), vertexInfo, vertexMetadata, 0);

            SDL.Free(vertexMetadataPtr);

            // Load Fragment shader
            ShaderCross.SPIRVInfo fragInfo = new ShaderCross.SPIRVInfo();
            fragInfo.ByteCode = fragPtr;
            fragInfo.ByteCodeSize = fragSize;
            fragInfo.Entrypoint = "main";
            fragInfo.ShaderStage = ShaderCross.ShaderStage.Fragment;
            var fragMetadataPtr = ShaderCross.ReflectGraphicsSPIRV(fragInfo.ByteCode, fragSize, 0);
            ShaderCross.GraphicsShaderMetadata fragMetadata = Marshal.PtrToStructure<ShaderCross.GraphicsShaderMetadata>(fragMetadataPtr);

            fragmentShader = ShaderCross.CompileGraphicsShaderFromSPIRV(App.GetDevice(), fragInfo, fragMetadata, 0);
            SDL.Free(fragMetadataPtr);

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

            SDL.GPUColorTargetDescription[] colorTargetDescriptions = new SDL.GPUColorTargetDescription[1];
            colorTargetDescriptions[0] = new SDL.GPUColorTargetDescription();
            colorTargetDescriptions[0].Format = SDL.GetGPUSwapchainTextureFormat(App.GetDevice(), App.GetWindow());

            pipelineInfo.TargetInfo.NumColorTargets = 1;
            pipelineInfo.TargetInfo.ColorTargetDescriptions = SDL.StructureArrayToPointer(colorTargetDescriptions);

            pipeline = SDL.CreateGPUGraphicsPipeline(App.GetDevice(), pipelineInfo);

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
