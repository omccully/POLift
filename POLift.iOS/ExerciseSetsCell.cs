using Foundation;
using System;
using UIKit;

using POLift.Core.Model;

namespace POLift.iOS
{
    public partial class ExerciseSetsCell : UITableViewCell
    {
        IExerciseSets ExerciseSets;

        public ExerciseSetsCell (IntPtr handle) : base (handle)
        {
           
        }

        private void SetCountTextField_EditingChanged(object sender, EventArgs e)
        {
            if(ExerciseSets != null)
            {
                ExerciseSets.ID = 0;
                try
                {
                    ExerciseSets.SetCount = Int32.Parse(SetCountTextField.Text);
                }
                catch
                {

                }
            }
        }

        public void Setup(IExerciseSets exercise_sets)
        {
            SetCountTextField.EditingChanged -= SetCountTextField_EditingChanged;
            SetCountTextField.EditingChanged += SetCountTextField_EditingChanged;

            this.ExerciseSets = exercise_sets;
            SetCountTextField.Text = ExerciseSets.SetCount.ToString();
            ExerciseNameLabel.Text = "sets of " + ExerciseSets.Exercise.Name;
        }
    }
}