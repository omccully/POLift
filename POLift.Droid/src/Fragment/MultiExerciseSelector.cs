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

using POLift.Core.Model;
using POLift.Core.Service;

namespace POLift.Droid
{
    public class CheckBoxStateChangeEventArgs : EventArgs
    {
        public CheckBox CheckBox { get; private set; }

        public CheckBoxStateChangeEventArgs(CheckBox cb)
        {
            this.CheckBox = cb;
        }
    }

    public class MultiExerciseSelector
    {
        Dictionary<IExercise, bool> ExercisesMap = 
            new Dictionary<IExercise, bool>();

        public string ExerciseIDs
        {
            get
            {
                return ExercisesMap.Where(kvp => kvp.Value)
                    .Select(kvp => kvp.Key.ID).ToIDString();
            }
        }
        
        public event EventHandler<CheckBoxStateChangeEventArgs> CheckedChanged;

        public MultiExerciseSelector(IEnumerable<IExercise> exercises)
        {
            foreach(IExercise ex in exercises)
            {
                ExercisesMap.Add(ex, true);
            }
        }

        public void AddViews(ViewGroup vg)
        {
            foreach (KeyValuePair<IExercise, bool> pair in ExercisesMap)
            {
                IExercise ex = pair.Key;

                CheckBox cb = new CheckBox(vg.Context);
                cb.LayoutParameters = new ViewGroup.LayoutParams(
                    ViewGroup.LayoutParams.FillParent,
                    ViewGroup.LayoutParams.WrapContent);
                cb.Text = $"{ex.MaxRepCount}r {ex.RestPeriodSeconds}s";
                cb.Checked = pair.Value;
                cb.CheckedChange += delegate
                {
                    System.Diagnostics.Debug.WriteLine(ex + " " + cb.Checked);
                    ExercisesMap[ex] = cb.Checked;
                    CheckedChanged.Invoke(this, new CheckBoxStateChangeEventArgs(cb));
                };
                vg.AddView(cb);
            }
        }

        public bool Accepts(IExercise exercise)
        {
            if (!ExercisesMap.ContainsKey(exercise)) return false;
            return ExercisesMap[exercise];
        }

        public List<IExercise> Filter(IEnumerable<IExercise> exercises)
        {
            return exercises.Where(ex => Accepts(ex)).ToList();
        }
    }
}