
using SDL3;

namespace panpan.Util
{
    public class Time
    {
        private static double deltaTime;
        private static ulong lastFrameTime;

        public static float DeltaTime => (float)deltaTime;
        public static float Elapsed()
        {
            float seconds = SDL.GetTicks() / 1000.0f;
            return seconds;
        }

        public static void Update()
        {
            ulong now = SDL.GetPerformanceCounter();
            deltaTime = (double)((now-lastFrameTime)*1000 / (double)SDL.GetPerformanceFrequency())/1000.0;//Time.Elapsed() - lastFrameTime;
            lastFrameTime = now;//Time.Elapsed();
            if(deltaTime > 1.0)
                deltaTime = 1.0;
        }
    }

    public delegate void TimerCallback();
    public class PTimer
    {
        private TimerCallback? timerCompleteCallback;
        private Thread timerThread;
        private bool running;
        private bool complete;
        private float duration;
        private float startTime;

        public float Duration => duration;
        public float StartTime => startTime;
        public bool Running => running;
        public bool Complete => complete;

        public PTimer(float durationSeconds, TimerCallback? timerCompleteCallback)
        {
            this.duration = durationSeconds;
            this.timerCompleteCallback = timerCompleteCallback;
            timerThread = new Thread(Update);
            running = false;
            complete = false;
        }

        private void Update()
        {
            while (running)
            {
                if (Time.Elapsed() - startTime >= duration)
                {
                    running = false;
                    complete = true;
                    if (timerCompleteCallback != null)
                    {
                        timerCompleteCallback();
                    }
                }
            }
        }

        public void Start()
        {
            Restart();
            timerThread.Start();
        }

        public void Restart()
        {
            running = true;
            complete = false;
            startTime = Time.Elapsed();
        }
    }
}