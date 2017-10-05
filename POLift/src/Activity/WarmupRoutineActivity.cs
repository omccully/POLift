using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace POLift
{
    using Android.Text;
    using Model;
    using Service;

    [Activity(Label = "Warmup")]
    public class WarmupRoutineActivity : PerformRoutineBaseActivity
    {
        Exercise FirstExercise = null;

        const string WarmupSetIndexKey = "warmup_set_index";

        WarmupSet[] WarmupSets =
        {
            new WarmupSet(8, 50, 50),
            new WarmupSet(8, 50, 50),
            new WarmupSet(4, 70, 50),
            new WarmupSet(1, 90, 50)
        };


        WarmupSet NextWarmupSet
        {
            get
            {
                if (WarmupFinished) return null;
                return WarmupSets[WarmupSetIndex];
            }
        }

        int _warmup_set_index = 0;
        int WarmupSetIndex
        {
            get
            {
                return _warmup_set_index;
            }
            set
            {
                _warmup_set_index = value;
                RefreshWarmupInfo();
            }
        }

        void RefreshWarmupInfo()
        {
            if (WarmupFinished)
            {
                NextExerciseView.Text = "Finished";
                return;
            }

            string txt = $"{NextWarmupSet.Reps} reps of {FirstExercise.Name} at a weight of ";

            try
            {
                int weight = NextWarmupSet.GetWeight(FirstExercise, WeightInput);
                txt += weight.ToString();

                if (FirstExercise.PlateMath != null)
                {
                    txt += " (" + FirstExercise.PlateMath.PlateCountsToString(weight) + ")";
                }
            }
            catch(FormatException)
            {
                txt += "??";
            }
            

            if (!String.IsNullOrEmpty(NextWarmupSet.Notes))
            {
                txt += $" ({NextWarmupSet.Notes})";
            }

            NextExerciseView.Text = txt;
        }

        bool WarmupFinished
        {
            get
            {
                return WarmupSetIndex >= WarmupSets.Length;
            }
        }

        Dialog error_dialog;
        Dialog back_button_dialog;

        public override void OnBackPressed()
        {
            back_button_dialog = Helpers.DisplayConfirmation(this, 
                "Are you sure you want to end this warmup session? " +
                " You will lose all of your progress in this warmup.",
                delegate
                {
                    base.OnBackPressed();
                });
        }

        protected override void WeightEditText_TextChanged(object sender, TextChangedEventArgs e)
        {
            // base.WeightEditText_TextChanged(sender, e);

            RefreshWarmupInfo();
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here

            int id = Intent.GetIntExtra("exercise_id", -1);
            if (id != -1)
            {
                FirstExercise = POLDatabase.ReadByID<Exercise>(id);
            }

            WeightInput = Intent.GetIntExtra("working_set_weight", 0);

            if (FirstExercise == null)
            {
                error_dialog = Helpers.DisplayError(this, "Error (" + id + ")",
                    delegate
                    {
                        Finish();
                    });

                return;
            }

            if(savedInstanceState == null)
            {
                WarmupSetIndex = 0;
            }
            else
            {
                WarmupSetIndex = savedInstanceState.GetInt("warmup_set_index", 0);
            }
            // set index and display the stuff

            WeightLabel.Text = "Working set weight: ";
            ReportResultButton.Text = "Set completed";
            RepResultLabel.Visibility = ViewStates.Gone;
            RepResultEditText.Visibility = ViewStates.Gone;

        }

        protected override void ReportResultButton_Click(object sender, EventArgs e)
        {
            // warmup set completed button clicked
            WarmupSetIndex++;

            if (WarmupFinished)
            {
                SetResult(Result.Ok);
                Finish();
                return;
            }

            StartTimer(NextWarmupSet.GetRestPeriod(FirstExercise));
        }


        protected override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutInt("warmup_set_index", WarmupSetIndex);

            SaveTimerState(outState);

            base.OnSaveInstanceState(outState);
        }

        protected override void OnPause()
        {
            // dismiss dialog boxes to prevent window leaks
            error_dialog?.Dismiss();
            back_button_dialog?.Dismiss();

            base.OnPause();
        }
    }
}