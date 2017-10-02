using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace POLift.Service
{
   /* class RestPeriodTimerEventArgs : EventArgs
    {
        public RestPeriodTimerEventArgs(object target)
        {

        }


    }*/


    /// <summary>
    /// 
    /// </summary>
    static class StaticTimer
    {
        /// <summary>
        /// 
        /// </summary>
        /// 

        // when you leave activity, reassign these event handlers
        // to some static 

        /*private EventHandler<ElapsedEventArgs> _Ticked;
        public event EventHandler<ElapsedEventArgs> Ticked
        {
            add
            {
                _Ticked = value;
            }
            remove
            {
                _Ticked -= value;
            }
        }

        private EventHandler<ElapsedEventArgs> _Elapsed;
        public event EventHandler<ElapsedEventArgs> Elapsed
        {
            add
            {
                _Elapsed = value;
            }
            remove
            {
                _Elapsed -= value;
            }
        }*/

        
        
        public delegate void TimerTickedCallback(int ticks_until_elapsed);
        public delegate void TimerElapsedCallback();

        static Timer timer;
        public static TimerTickedCallback TickedCallback;
        public static TimerElapsedCallback ElapsedCallback;

        public static void StartTimer(double tick_time_ms, int ticks_until_elapsed, 
            TimerTickedCallback ticked_cb, TimerElapsedCallback elapsed_cb)
        {
            StopTimer();

            timer = new Timer(tick_time_ms);
            StaticTimer.ticks_until_elapsed = ticks_until_elapsed;
            StaticTimer.TickedCallback = ticked_cb;
            StaticTimer.ElapsedCallback = elapsed_cb;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        public static bool IsRunning
        {
            get
            {
                return timer != null;
            }
        }

        public static void StopTimer()
        {
            if(timer != null)
            {
                timer.Elapsed -= Timer_Elapsed;
                timer.Stop();
            }

            timer = null;
            TickedCallback = null;
            ElapsedCallback = null;
        }


        static object ticks_until_elapsed_locker = new object();

        public static void AddTicks(int ticks)
        {
            lock (ticks_until_elapsed_locker)
            {
                ticks_until_elapsed += ticks;
            }
        }

        public static void SubtractTicks(int ticks)
        {
            lock (ticks_until_elapsed_locker)
            {
                ticks_until_elapsed -= ticks;
            }
        }

        volatile static int ticks_until_elapsed;
        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            int tue = 0;
            lock (ticks_until_elapsed_locker)
            {
                ticks_until_elapsed--;
                tue = ticks_until_elapsed;
            }

            
            if(tue > 0)
            {
                // timer has not elapsed yet

                TickedCallback?.Invoke(tue);
            }
            else
            {
                // timer elapsed
                ElapsedCallback?.Invoke();

                StopTimer();
            }
        }


    }
}