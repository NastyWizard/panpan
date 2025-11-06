
using SDL3;

namespace panpan.Util
{
    public class Time
    {
        public static float Elapsed()
        {
            float seconds = SDL.GetTicks() / 1000.0f;
            return seconds;
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