
using GlmSharp;
using panpan.Rendering;
using panpan.Scene;
using panpan.Util;

namespace panpanExample
{
    public class FireFly : Light
    {
        private vec2 startPos;
        private vec2 targetPos; 
        private vec2 secondLightOffset; 

        private float angle;
        private float speed;
        private float turnSpeed; 
        public FireFly(int x, int y): base(x, y)
        {
            speed = 0.2f;
            turnSpeed = 1.0f;
            var r = new Random();
            angle = r.NextInt64(0,359);
            secondLightOffset = new vec2(r.NextInt64(-128,128), r.NextInt64(-128,128));
            startPos = new vec2(x, y);
            FindNewTargetPos();
        }

        public override void DrawLights()
        {
            float targetAngle = MathF.Atan2(targetPos.y - Position.y, targetPos.x - Position.x) * 180.0f / 3.141f;
            float diff = targetAngle - angle;

            while(diff < -180)
            {
                diff += 360;
            }
            while(diff > 180)
            {
                diff -= 360;
            }

            if (diff > 0){
                angle += turnSpeed;
            } else if (diff < 0) {
                angle -= turnSpeed;
            }

            Position.xy += vec2.FromAngle(PMath.DegToRad(angle)) * speed;

            if(vec2.Distance(Position.xy, targetPos) < 16.0f)
            {
                FindNewTargetPos();
            }
            vec2 p = new vec2(MathF.Floor(Position.x), MathF.Floor(Position.y)) + new vec2(-16,16);
            Draw.Sprite(GameTextures.lightTex32_weak, p);
            Draw.Sprite(GameTextures.lightTex64_weak, p + secondLightOffset);
        }

        private void FindNewTargetPos()
        {
            var r = new Random();
            targetPos = startPos + new vec2(r.NextInt64(-128, 128));
        }
    }
}