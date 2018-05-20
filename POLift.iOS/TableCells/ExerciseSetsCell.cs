using Foundation;
using System;
using UIKit;

using POLift.Core.Model;

namespace POLift.iOS
{
    public partial class ExerciseSetsCell : UITableViewCell
    {
        public Action<ExerciseSetsCell, IExerciseSets> SetCountZeroed = null;

        IExerciseSets ExerciseSets;

        public ExerciseSetsCell (IntPtr handle) : base (handle)
        {
           
        }

        private void SetCountTextField_EditingChanged(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("SetCountTextField_EditingChanged");
            if(ExerciseSets != null)
            {
                ExerciseSets.ID = 0;
                try
                {
                    int set_count = Int32.Parse(SetCountTextField.Text);
                    ExerciseSets.SetCount = set_count;

                    if(set_count == 0)
                    {
                        SetCountZeroed?.Invoke(this, ExerciseSets);
                    }
                }
                catch
                {

                }
            }
        }

        public void DismissKeyboard()
        {
            SetCountTextField.ResignFirstResponder();
        }

        public void Setup(IExerciseSets exercise_sets, bool EditEnabled=true)
        {
            SetCountTextField.EditingChanged -= SetCountTextField_EditingChanged;
            SetCountTextField.EditingChanged += SetCountTextField_EditingChanged;
            SetCountTextField.AddDoneButtonToNumericKeyboard();

            //SetCountTextField.TouchDown += SetCountTextField_TouchDown;
            SetCountTextField.EditingDidBegin += SetCountTextField_EditingDidBegin;

            this.ExerciseSets = exercise_sets;
            SetCountTextField.Text = ExerciseSets.SetCount.ToString();
            ExerciseNameLabel.Text = "sets of " + ExerciseSets.Exercise.CondensedDetails + 
                (EditEnabled ? "" : " (locked)");
            //Console.WriteLine("ExerciseNameLabel.Text = " + ExerciseNameLabel.Text);
            ExerciseNameLabel.PreferredMaxLayoutWidth = 220;
            SetCountTextField.Enabled = EditEnabled;

            if(!EditEnabled) this.BackgroundColor = UIColor.LightGray;

            base.SelectionStyle = UITableViewCellSelectionStyle.None;
        }

        private void SetCountTextField_EditingDidBegin(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("SetCountTextField_EditingDidBegin");
            //SetCountTextField.SelectAll(null);
        }
    }
}