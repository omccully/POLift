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
    using Model;
    using Service;

    class WarmupSet
    {
        int PercentOfWeight;
        int PercentOfRestPeriod;

        public readonly int Reps;
        public readonly string Notes;

        public WarmupSet(int reps, int percent_of_weight, int percent_of_rest_period, string notes="")
        {
            Reps = reps;
            PercentOfWeight = percent_of_weight;
            PercentOfRestPeriod = percent_of_rest_period;
            Notes = notes;
        }

        public int GetWeight(Exercise ex, int max_weight)
        {
            return Helpers.GetClosestToIncrement((max_weight * PercentOfWeight) / 100, ex.WeightIncrement);
        }

        public int GetRestPeriod(Exercise ex)
        {
            return (ex.RestPeriodSeconds * PercentOfRestPeriod) / 100;
        }
    }

    [Activity(Label = "Warmup")]
    public class WarmupRoutineActivity : Activity
    {
        Button WarmupSetFinishedButton;
        EditText WorkingSetWeightEditText;
        TextView WarmupRoutineTextView;
        TextView TimeLeftTextView;

        Exercise first_exercise = null;
        int working_weight = 0;

        WarmupSet[] warmup_sets =
        {
            new WarmupSet(8, 50, 50),
            new WarmupSet(8, 50, 50),
            new WarmupSet(4, 70, 50),
            new WarmupSet(1, 90, 50)
        };

        int _warmup_set_index = 0;
        int warmup_set_index
        {
            get
            {
                return _warmup_set_index;
            }
            set
            {
                _warmup_set_index = value;

                if(warmup_sets.Length >= value)
                {
                    WarmupRoutineTextView.Text = "Finished";
                    return;
                }

                WarmupSet ws = warmup_sets[value];
                string txt = $"{ws.Reps} reps at a weight of ";
                txt += ws.GetWeight(first_exercise, working_weight).ToString();
                if(!String.IsNullOrEmpty(ws.Notes))
                {
                    txt += $" ({ws.Notes})";
                }
                
                WarmupRoutineTextView.Text = txt;
            }
        }

        bool Finished
        {
            get
            {
                return warmup_sets.Length >= warmup_set_index;
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here

            WarmupSetFinishedButton = FindViewById<Button>(Resource.Id.WarmupSetFinishedButton);
            WorkingSetWeightEditText = FindViewById<EditText>(Resource.Id.WorkingSetWeightEditText);
            WarmupRoutineTextView = FindViewById<TextView>(Resource.Id.WarmupRoutineTextView);
            TimeLeftTextView = FindViewById<TextView>(Resource.Id.TimeLeftTextView);

            int id = Intent.GetIntExtra("exercise_id", -1);
            if(id != -1)
            {
                first_exercise = POLDatabase.ReadByID<Exercise>(id);
            }

            if(first_exercise == null)
            {
                Helpers.DisplayError(this, "Error (" + id + ")");
                Finish();
                return;
            }

            working_weight = Intent.GetIntExtra("weight", 0);
            WorkingSetWeightEditText.Text = working_weight.ToString();
            WorkingSetWeightEditText.TextChanged += WorkingSetWeightEditText_TextChanged;

            WarmupSetFinishedButton.Click += WarmupSetFinishedButton_Click;

            // set index and display the stuff
            warmup_set_index = 0; 
        }

        private void WarmupSetFinishedButton_Click(object sender, EventArgs e)
        {
            warmup_set_index++;

            if(Finished)
            {

                Finish();
            }

        }

        private void WorkingSetWeightEditText_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            try
            {
                working_weight = Int32.Parse(WorkingSetWeightEditText.Text);
            }
            catch (FormatException)
            {

            }
        }
    }
}