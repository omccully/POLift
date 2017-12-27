using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;

using Xamarin.Forms;
using POLift.Core.Service;

namespace POLift.Core.ViewModel
{
    public class TimerViewModel : ViewModelBase, ITimerViewModel
    {
        public IVibrator Vibrator;
        public Timer Timer;
        // public Action<Action> MainThreadInvoker;
        public IMainThreadInvoker MainThreadInvoker;

        public TimerViewModel()
        {

        }

        bool _TimerEnabled = false;
        bool TimerEnabled
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

        int _SecondsLeft;
        public int SecondsLeft
        {
            get
            {
                return _SecondsLeft;
            }
            set
            {
                Set(ref _SecondsLeft, value);

                UpdateGUIByTimerState();
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

                TimerIsStartable = false;
                System.Diagnostics.Debug.WriteLine("Timer unstartable");
            }
            else
            {
                Sub30SecEnabled = false;
                Add30SecEnabled = false;
                SkipTimerEnabled = false;

                TimerRunningPositive = false;

                TimerIsStartable = true;
                System.Diagnostics.Debug.WriteLine("Timer startable");
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
            if (seconds_left > 0)
            {
                TimerStatus = "Resting for another " +
                   seconds_left + " seconds.";
                //CountDownTextView.SetTextColor(Android.Graphics.Color.Orange);
            }
            else
            {
                TimerStatus = "The timer has been done for " +
                    Math.Abs(seconds_left) + " seconds." + Environment.NewLine +
                    "Start your next set whenever you're ready";
               // CountDownTextView.SetTextColor(Android.Graphics.Color.Green);
            }
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
            SecondsLeft = seconds_left;
            TimerEnabled = true;

            if (!Timer.IsRunning)
            {
                Timer.Start(TimerTicked, 1000);
            }
            
            SetCountDownText(seconds_left);

            UpdateGUIByTimerState();

            AddSecCount = 0;
            SubSecCount = 0;
        }

        void TimerTicked()
        {
            System.Diagnostics.Debug.WriteLine("TimerTicked()");

            if (!TimerEnabled)
            {
                // this should never happen.
                System.Diagnostics.Debug.WriteLine("Error: timer wasn't enabled");

                Timer?.Cancel();
                return;
            }

            this.MainThreadInvoker.Invoke(delegate
            {
                SecondsLeft--;

                if (SecondsLeft == 0) TimerElapsed();

                SetCountDownText(SecondsLeft);
            });
        }

        void TimerElapsed()
        {
            System.Diagnostics.Debug.WriteLine("TimerElapsed()");
            Vibrator?.Vibrate();

            //this.MainThreadInvoker.Invoke(delegate
            //{
            //    UpdateGUIByTimerState();
            //    System.Diagnostics.Debug.WriteLine("call UpdateGUIByTimerState()");
            //});

            //StartTimerNotification
        }

        void SkipTimer()
        {
            TimerEnabled = false;
            Timer.Cancel();
            TimerStatus = "Timer skipped. " + System.Environment.NewLine +
                 "Start your next set whenever you're ready";
            // set text color White

            SecondsLeft = 0;

            Vibrator?.Vibrate();
        }

        RelayCommand _SkipTimerCommand;
        public RelayCommand SkipTimerCommand
        {
            get
            {
                return _SkipTimerCommand ??
                    (_SkipTimerCommand = new RelayCommand(delegate
                    {
                        SkipTimer();
                    }));
            }
        }

        RelayCommand _Add30SecCommand;
        public RelayCommand Add30SecCommand
        {
            get
            {
                return _Add30SecCommand ??
                    (_Add30SecCommand = new RelayCommand(delegate
                    {
                        SecondsLeft += 30;
                        if (SecondsLeft <= 0)
                        {
                            UpdateGUIByTimerState();
                            Vibrator?.Vibrate();
                        }

                        AddSecCount++;
                    }));
            }
        }

        RelayCommand _Sub30SecCommand;
        public RelayCommand Sub30SecCommand
        {
            get
            {
                return _Sub30SecCommand ??
                    (_Sub30SecCommand = new RelayCommand(delegate
                    {
                        SecondsLeft -= 30;
                        if (SecondsLeft <= 0)
                        {
                            UpdateGUIByTimerState();
                            Vibrator?.Vibrate();
                        }

                        SubSecCount++;
                    }));
            }
        }

        
        string _Add30SecButtonText;
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

        string _Sub30SecButtonText;
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



    }
}
