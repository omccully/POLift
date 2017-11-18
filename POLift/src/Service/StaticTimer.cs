using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace POLift.Service
{
    static class StaticTimer
    {
        public delegate void TimerTickedCallback(int ticks_until_elapsed);
        public delegate void TimerElapsedCallback();

        public static Timer timer;
        public static TimerTickedCallback TickedCallback;
        public static TimerElapsedCallback ElapsedCallback;

        // kill timer after 10 mins
        public static int LowestTickRemaining = -600;

        private static object TicksRemainingLocker = new object();
        volatile static int _TicksRemaining;
        public static int TicksRemaining
        {
            get
            {
                int val;
                lock (TicksRemainingLocker)
                {
                    //System.Diagnostics.Debug.Write("n");
                     val = _TicksRemaining;
                    //return _TicksRemaining;
                   // System.Diagnostics.Debug.Write("x");
                }
                return val;
            }
            set
            {
                lock (TicksRemainingLocker)
                {
                    //System.Diagnostics.Debug.Write("t");
                    _TicksRemaining = value;
                    //System.Diagnostics.Debug.Write("i");
                }
            }
        }

        public static void StartTimer(double tick_time_ms, int ticks_until_elapsed,
            TimerTickedCallback ticked_cb, TimerElapsedCallback elapsed_cb)
        {
            StopTimer();

            timer = new Timer(tick_time_ms);
            StaticTimer.TicksRemaining = ticks_until_elapsed;
            StaticTimer.TickedCallback = ticked_cb;
            StaticTimer.ElapsedCallback = elapsed_cb;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
            //System.Diagnostics.Debug.WriteLine($"StartTimer({tick_time_ms},{ticks_until_elapsed} ..)");
        }

        public static bool IsRunning
        {
            get
            {
                return timer != null;
            }
        }

        public static bool IsRunningPositive
        {
            get
            {
                return IsRunning && TicksRemaining > 0;
            }
        }

        public static void StopTimer()
        {
            if (timer != null)
            {
                timer.Elapsed -= Timer_Elapsed;
                timer.Stop();
            }

            timer = null;
            TickedCallback = null;
            ElapsedCallback = null;
        }

        public static void AddTicks(int ticks)
        {
            TicksRemaining += ticks;
        }

        public static void SubtractTicks(int ticks)
        {
            TicksRemaining -= ticks;
        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("StaticTimer.Timer_Elapsed(object sender, ElapsedEventArgs e)");
            TicksRemaining--;
            int tue = TicksRemaining;

            TickedCallback?.Invoke(tue);

            if (tue == 0)
            {
                // timer elapsed
                ElapsedCallback?.Invoke();

                //StopTimer();
            }

            if (tue < LowestTickRemaining)
            {
                StopTimer();
            }
        }
    }
}