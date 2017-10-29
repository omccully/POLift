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

using Microsoft.Practices.Unity;

namespace POLift
{
    using Android.Text;
    using Model;
    using Service;

    [Activity(Label = "Warmup")]
    public class WarmupRoutineActivity : PerformRoutineBaseActivity
    {
        IExercise FirstExercise = null;

        const string WarmupSetIndexKey = "warmup_set_index";

        IWarmupSet[] WarmupSets =
        {
            new WarmupSet(8, 50, 50),
            new WarmupSet(8, 50, 50),
            new WarmupSet(4, 70, 50),
            new WarmupSet(1, 90, 50)
        };


        IWarmupSet NextWarmupSet
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

        bool WarmupFinished
        {
            get
            {
                return WarmupSetIndex >= WarmupSets.Length;
            }
        }

        Dialog error_dialog;
        Dialog back_button_dialog;

        IPOLDatabase Database;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here

            Database = C.ontainer.Resolve<IPOLDatabase>();

            int id = Intent.GetIntExtra("exercise_id", -1);
            if (id != -1)
            {
                FirstExercise = Database.ReadByID<Exercise>(id);
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

            int warmup_set_index_intent = Intent.GetIntExtra("warmup_set_index", 0);
            if (savedInstanceState == null)
            {
                WarmupSetIndex = warmup_set_index_intent;
            }
            else
            {
                WarmupSetIndex = savedInstanceState.GetInt("warmup_set_index", warmup_set_index_intent);
            }
            // set index and display the stuff

            RoutineDetails.Visibility = ViewStates.Gone;
            WeightLabel.Text = "Working set weight: ";
            ReportResultButton.Text = "Set completed";
            RepResultLabel.Visibility = ViewStates.Gone;
            RepResultEditText.Visibility = ViewStates.Gone;
            ModifyRestOfRoutineButton.Visibility = ViewStates.Gone;
            NextExerciseView.Visibility = ViewStates.Gone;

            //IMadeAMistakeButton.Visibility = ViewStates.Gone;
            IMadeAMistakeButton.Text = "Skip warmup routine";
            IMadeAMistakeButton.Click += IMadeAMistakeButton_Click;

            //var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            //SetActionBar(toolbar);
            //ActionBar.Title = $"{FirstExercise.Name} warmup";
        }

        private void IMadeAMistakeButton_Click(object sender, EventArgs e)
        {
            SetResult(Result.Canceled);
            Finish();
        }

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

        void RefreshWarmupInfo()
        {
            if (WarmupFinished)
            {
                NextExerciseView.Text = "Finished";
                return;
            }

            string txt = $"Warmup exercise {WarmupSetIndex+1}/{WarmupSets.Count()}: ";
            txt += $"{NextWarmupSet.Reps} reps of {FirstExercise.Name} at a weight of ";

            try
            {
                int weight = NextWarmupSet.GetWeight(FirstExercise, WeightInput);
                txt += weight.ToString();

                if (FirstExercise.PlateMath != null)
                {
                    txt += " (" + FirstExercise.PlateMath.PlateCountsToString(weight) + ")";
                }
            }
            catch (FormatException)
            {
                txt += "??";
            }


            if (!String.IsNullOrEmpty(NextWarmupSet.Notes))
            {
                txt += $" ({NextWarmupSet.Notes})";
            }

            NextWarmupView.Text = txt;
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

            TryShowFullScreenAd();
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

        protected override void SaveStateToIntent(Intent intent)
        {
            base.SaveStateToIntent(intent);

            intent.PutExtra("exercise_id", FirstExercise.ID);

            try
            {
                intent.PutExtra("working_set_weight", WeightInput);
            }
            catch (FormatException)
            {
            }

            intent.PutExtra("warmup_set_index", WarmupSetIndex);
        }

        protected override void BuildArtificialTaskStack(Android.Support.V4.App.TaskStackBuilder stackBuilder)
        {
            base.BuildArtificialTaskStack(stackBuilder);

            Intent perform_routine_intent = new Intent(this, typeof(PerformRoutineActivity));
            //perform_routine_intent.PutExtra("routine_id", )
        }
    }
}