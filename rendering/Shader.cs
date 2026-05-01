namespace panpan.Rendering
{
    public struct Shader
    {
        public byte[] Data;
        public uint NumUnifromBuffers;
        public uint NumSamplers;

        public Shader(byte[] data, uint numUnifromBuffers, uint numSamplers)
        {
            Data = data;
            NumUnifromBuffers = numUnifromBuffers;
            NumSamplers = numSamplers;
        }
    }
}

