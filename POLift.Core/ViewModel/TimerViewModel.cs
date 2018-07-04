using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;

using POLift.Core.Service;

namespace POLift.Core.ViewModel
{
    public enum TimerState
    {
        Skipped,
        RunningPositive,
        Elapsed
    }

    public class TimerViewModel : ViewModelBase, ITimerViewModel
    {
        public IVibrator Vibrator;
        public Timer Timer;
        // public Action<Action> MainThreadInvoker;
        public IMainThreadInvoker MainThreadInvoker;
        public KeyValueStorage KeyValueStorage;

        public INotificationService TimerFinishedNotificationService { get; set; }

        public const string NotificationText = "Lifting rest period finished";
        public const string NotificationSubText = "Start your next set whenever you are ready";

        public TimerViewModel()
        {

        }

        bool _TimerEnabled = false;
        public bool TimerEnabled
        {
            get
            {
                return _TimerEnabled;
            }
            set
            { 
                Set(ref _TimerEnabled, value);
                
                UpdateGUIByTimerState();
            }
        }

        
        DateTime EndTime { get; set; }

        public int SecondsLeft
        {
            get
            {
                TimeSpan span = EndTime - DateTime.Now;

                return (int)span.TotalSeconds + 
                    (AddSecCount * 30) + (SubSecCount * -30);
            }
            set
            {
                EndTime = DateTime.Now.AddSeconds(value);

                AddSecCount = 0;
                SubSecCount = 0;
            }
        }

        void UpdateGUIByTimerState()
        {
            if (TimerEnabled && SecondsLeft > 0)
            {
                Sub30SecEnabled = true;
                Add30SecEnabled = true;
                SkipTimerEnabled = true;

                TimerRunningPositive = true;

                TimerState = TimerState.RunningPositive;

                TimerIsStartable = false;
                System.Diagnostics.Debug.WriteLine("Timer unstartable");
            }
            else
            {
                Sub30SecEnabled = false;
                Add30SecEnabled = false;
                SkipTimerEnabled = false;

                TimerRunningPositive = false;

                TimerState = TimerState.Elapsed;

                TimerIsStartable = true;
                System.Diagnostics.Debug.WriteLine("Timer startable");
            }
        }

        TimerState _TimerState = TimerState.Skipped;
        public TimerState TimerState
        {
            get
            {
                return _TimerState;
            }
            set
            {
                Set(ref _TimerState, value);
            }
        }


        bool _TimerRunningPositive = false;
        public bool TimerRunningPositive
        {
            get
            {
                return _TimerRunningPositive;
            }
            set
            {
                Set(ref _TimerRunningPositive, value);
            }
        }

        string _TimerStatus;
        public string TimerStatus
        {
            get
            {
                return _TimerStatus;
            }
            set
            {
                Set(ref _TimerStatus, value);
                System.Diagnostics.Debug.WriteLine("TimerStatus = " + _TimerStatus);
            }
        }

        bool _Sub30SecEnabled;
        public bool Sub30SecEnabled
        {
            get
            {
                return _Sub30SecEnabled;
            }
            set
            {
                Set(ref _Sub30SecEnabled, value);
            }
        }

        bool _Add30SecEnabled;
        public bool Add30SecEnabled
        {
            get
            {
                return _Add30SecEnabled;
            }
            set
            {
                Set(ref _Add30SecEnabled, value);
            }
        }


        bool _SkipTimerEnabled;
        public bool SkipTimerEnabled
        {
            get
            {
                return _SkipTimerEnabled;
            }
            set
            {
                Set(ref _SkipTimerEnabled, value);
            }
        }

        void SetCountDownText(int seconds_left)
        {
            string tiptext = "";

            if (seconds_left > 0)
            {
                tiptext = "Resting";
                //TimerStatus = "Resting for another " +
                 //  seconds_left + " seconds.";
                // orange
            }
            else
            {
                //TimerStatus = "The timer has been done for " +
                //    Math.Abs(seconds_left) + " seconds." + Environment.NewLine +
                //    "Start your next set whenever you're ready";
                tiptext = "Start your next set";
               // green
            }

            TimerStatus = tiptext + Environment.NewLine + seconds_left.SecondsToClock();
        }

        bool _TimerIsStartable = true;
        public bool TimerIsStartable
        {
            get
            {
                return _TimerIsStartable;
            }
            set
            {
                Set(ref _TimerIsStartable, value);
            }
        }

        public void StartTimer(int seconds_left)
        {
            Notified = false;
            SecondsLeft = seconds_left;

            TimerEnabled = true;

            System.Diagnostics.Debug.WriteLine("Trying to start timer...");
            if (!Timer.IsRunning)
            {
                Timer.Start(TimerTicked, 1000);
                System.Diagnostics.Debug.WriteLine("Started timer");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Timer already running");
            }

            TimerFinishedNotificationService.Cancel();
            
            SetCountDownText(seconds_left);

            UpdateGUIByTimerState();

            SaveState();
        }



        bool Notified = false;
        void TimerTicked()
        {
            System.Diagnostics.Debug.WriteLine("TimerTicked()");

            if (!TimerEnabled || SecondsLeft < -600)
            {
                // this should never happen.
                System.Diagnostics.Debug.WriteLine("Error: disabling timer");

                Timer?.Cancel();
                return;
            }

            bool notify = !Notified && SecondsLeft <= 0;

            this.MainThreadInvoker.Invoke(delegate
            {
                SetCountDownText(SecondsLeft);

                if (notify) TimerElapsed();
            });
        }

        void TimerElapsed()
        {
            Notified = true; 

            System.Diagnostics.Debug.WriteLine("TimerElapsed()");

            UpdateGUIByTimerState();
            System.Diagnostics.Debug.WriteLine("call UpdateGUIByTimerState()");
           
            //StartTimerNotification
            TimerFinishedNotificationService?.Notify();

            Vibrator?.Vibrate();
        }

        public void CancelTimer(string msg = "")
        {
            TimerEnabled = false;
            Timer.Cancel();

            TimerStatus = msg;

            // set text color White
            TimerState = TimerState.Skipped;

            SecondsLeft = 0;
        }

        public void SkipTimer()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("SkipTimer()");
                CancelTimer("Timer skipped " + System.Environment.NewLine +
                     "Start your next set");

                Vibrator?.Vibrate();

                SaveState();
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
                
            }
        }

        RelayCommand _SkipTimerCommand;
        public RelayCommand SkipTimerCommand
        {
            get
            {
                return _SkipTimerCommand ??
                    (_SkipTimerCommand = new RelayCommand(SkipTimer));
            }
        }

        public void Add30Sec()
        {
            AddSecCount++;

            SetCountDownText(SecondsLeft);

            if (SecondsLeft <= 0)
            {
                UpdateGUIByTimerState();
                Vibrator?.Vibrate();
            }

            SaveState();
        }

        RelayCommand _Add30SecCommand;
        public RelayCommand Add30SecCommand
        {
            get
            {
                return _Add30SecCommand ??
                    (_Add30SecCommand = new RelayCommand(Add30Sec));
            }
        }

        public void Sub30Sec()
        {
            SubSecCount++;

            SetCountDownText(SecondsLeft);            

            if (SecondsLeft <= 0)
            {
                UpdateGUIByTimerState();
                Vibrator?.Vibrate();
            }

            SaveState();
        }

        RelayCommand _Sub30SecCommand;
        public RelayCommand Sub30SecCommand
        {
            get
            {
                return _Sub30SecCommand ??
                    (_Sub30SecCommand = new RelayCommand(Sub30Sec));
            }
        }

        
        string _Add30SecButtonText = "+30 sec";
        public string Add30SecButtonText
        {
            get
            {
                return _Add30SecButtonText;
            }
            set
            {
                Set(ref _Add30SecButtonText, value);
            }
        }

        string _Sub30SecButtonText = "-30 sec";
        public string Sub30SecButtonText
        {
            get
            {
                return _Sub30SecButtonText;
            }
            set
            {
                Set(ref _Sub30SecButtonText, value);
            }
        }


        void CancelTheTimerControlCounts()
        {
            int min = Math.Min(AddSecCount, SubSecCount);
           
            if (min > 0)
            {
                _add_sec_count -= min;
                _sub_sec_count -= min;
                UpdateAdd30SecButtonCounts();
            }
        }

        void UpdateAdd30SecButtonCounts()
        {
            if (_add_sec_count == 0)
            {
                Add30SecButtonText = "+30 sec";
            }
            else
            {
                Add30SecButtonText = $"+30 sec (x{_add_sec_count})";
            }
        }

        void UpdateSub30SecButtonCounts()
        {
            if (_sub_sec_count == 0)
            {
                Sub30SecButtonText = "-30 sec";
            }
            else
            {
                Sub30SecButtonText = $"-30 sec (x{_sub_sec_count})";
            }
        }

        void UpdateTimerControlButtonCounts()
        {
            UpdateAdd30SecButtonCounts();
            UpdateSub30SecButtonCounts();
        }

        int _add_sec_count = 0;
        int AddSecCount
        {
            get
            {
                return _add_sec_count;
            }
            set
            {
                _add_sec_count = value;

                // this may modify _add_sec_count
                CancelTheTimerControlCounts();

                UpdateTimerControlButtonCounts();
            }
        }

        int _sub_sec_count = 0;
        int SubSecCount
        {
            get
            {
                return _sub_sec_count;
            }
            set
            {
                _sub_sec_count = value;

                // this may modify _add_sec_count
                CancelTheTimerControlCounts();

                UpdateSub30SecButtonCounts();
            }
        }

        //public const string RestPeriodSecondsRemainingKey = "rest_period_seconds_remaining";

        public const string Add30SecCountKey = "timer_add_30_sec_count";
        public const string Sub30SecCountKey = "timer_sub_30_sec_count";
        public const string EndTimeKey = "timer_end_time";

        public void RestoreState(KeyValueStorage kvs)
        {
            string end_time_str = kvs.GetString(EndTimeKey, null);
            if(end_time_str != null)
            {
                this.EndTime = DateTime.Parse(end_time_str);
            }

            this.AddSecCount = kvs.GetInteger(Add30SecCountKey);
            this.SubSecCount = kvs.GetInteger(Sub30SecCountKey);

            if (SecondsLeft > -500)
            {
                System.Diagnostics.Debug.WriteLine("Restoring timer, SecondsLeft = " + SecondsLeft);
                
                TimerEnabled = true;
                if(SecondsLeft < 0)
                {
                    Notified = true;
                }

                if (!Timer.IsRunning)
                {
                    System.Diagnostics.Debug.WriteLine("Starting timer raw");
                    Timer.Start(TimerTicked, 1000);
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Not restoring timer, SecondsLeft = " + SecondsLeft);
                TimerStatus = "Start your next set";
                this.AddSecCount = 0;
                this.SubSecCount = 0;
            }
        }

        public void SaveState(KeyValueStorage kvs)
        {
            if(TimerEnabled)
            {
                kvs.SetValue(EndTimeKey, EndTime.ToString());
                kvs.SetValue(Add30SecCountKey, this.AddSecCount);
                kvs.SetValue(Sub30SecCountKey, this.SubSecCount);
            }
            else
            {
                kvs.SetValue(EndTimeKey, (string)null);
                kvs.SetValue(Add30SecCountKey, 0);
                kvs.SetValue(Sub30SecCountKey, 0);
            }
        }

        void SaveState()
        {
            SaveState(KeyValueStorage);
        }

        public void RestoreState()
        {
            RestoreState(KeyValueStorage);
        }
    }
}
