
using GlmSharp;
using panpan;
using panpan.Rendering;
using panpan.Scene;

public class BatchAnimator: Animator
{
    private SpriteBatch spriteBatch;
    public Transform Transform {internal set; get;}
    public vec3 Origin;
    public BatchAnimator(SpriteBatch batch, int frameWidth = 0, int frameHeight = 0) : base(null, frameWidth, frameHeight)
    {
        this.spriteBatch = batch;
        this.Transform = new();
        Origin = vec3.Zero;
        submitOnRender = true;
    }

    protected override void SubmitAnimation(Animation anim)
    {
        spriteBatch.SubmitSprite(this.Transform.Position, anim.Frames[(int)anim.CurrentFrame],this.Transform.Scale.xy,new (0,0,this.Transform.Angle), Origin);
    }
}