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
        // TODO: group exercises with the same rep count and rest period

        Dictionary<IExercise, bool> ExerciseCheckedMap = 
            new Dictionary<IExercise, bool>();

        public string ExerciseIDs
        {
            get
            {
                return ExerciseCheckedMap.Where(kvp => kvp.Value)
                    .Select(kvp => kvp.Key.ID).ToIDString();
            }
        }
        
        public event EventHandler<CheckBoxStateChangeEventArgs> CheckedChanged;

        public MultiExerciseSelector(IEnumerable<IExercise> exercises)
        {
            foreach (IExercise ex in exercises)
            {
                if(ex.Usage > 1) ExerciseCheckedMap.Add(ex, true);
            }
        }

        public void AddViews(ViewGroup vg)
        {
            foreach (KeyValuePair<IExercise, bool> pair in ExerciseCheckedMap)
            {
                IExercise ex = pair.Key;

                CheckBox cb = new CheckBox(vg.Context);
                cb.LayoutParameters = new ViewGroup.LayoutParams(
                    ViewGroup.LayoutParams.FillParent,
                    ViewGroup.LayoutParams.WrapContent);
                cb.Text = $"{ex.MaxRepCount}r {ex.RestPeriodSeconds.SecondsToClock()} ({ex.Usage - 1})";
                cb.Checked = pair.Value;
                cb.CheckedChange += delegate
                {
                    System.Diagnostics.Debug.WriteLine(ex + " " + cb.Checked);
                    ExerciseCheckedMap[ex] = cb.Checked;
                    CheckedChanged.Invoke(this, new CheckBoxStateChangeEventArgs(cb));
                };
                vg.AddView(cb);
            }
        }

        public bool Accepts(IExercise exercise)
        {
            if (!ExerciseCheckedMap.ContainsKey(exercise)) return false;
            return ExerciseCheckedMap[exercise];
        }

        public List<IExercise> Filter(IEnumerable<IExercise> exercises)
        {
            return exercises.Where(ex => Accepts(ex)).ToList();
        }
    }
}