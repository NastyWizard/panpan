namespace panpan.Rendering.Util
{
    public static class DefaultShaders
    {
        public static readonly Shader StandardFrag = new(Assets.Shaders.standard_frag_sprv, 0, 1);
        public static readonly Shader StandardVert = new(Assets.Shaders.standard_vert_sprv, 2, 0);

        public static readonly Shader BackbufferFrag = new(Assets.Shaders.backbuffer_frag_sprv, 0, 1);
        public static readonly Shader BackbufferVert = new(Assets.Shaders.backbuffer_vert_sprv, 0, 0);

        public static readonly Shader BackbufferLightingFrag = new(Assets.Shaders.bbLighting_frag_sprv, 1, 3);

        public static readonly Shader StandardFontFrag = new(Assets.Shaders.standardFont_frag_sprv, 0, 1);

        public static readonly Shader StandardNoTexFrag = new(Assets.Shaders.standardNoTex_frag_sprv, 0, 0);
    }

    public static class DefaultMaterials
    {
        public static Material? Standard, StandardNoTexture, StandardFont;
        public static Material? Backbuffer, BackbufferLighting;

        public static void Init()
        {
            Standard = new Material(DefaultShaders.StandardFrag, DefaultShaders.StandardVert);
            StandardNoTexture = new Material(DefaultShaders.StandardNoTexFrag, DefaultShaders.StandardVert);
            StandardFont = new Material(DefaultShaders.StandardFontFrag, DefaultShaders.StandardVert);

            Backbuffer = new Material(DefaultShaders.BackbufferFrag, DefaultShaders.BackbufferVert);
            BackbufferLighting = new Material(DefaultShaders.BackbufferLightingFrag, DefaultShaders.BackbufferVert);
        }
    }
}

