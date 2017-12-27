using Foundation;
using System;
using UIKit;
using POLift.Core.Model;

namespace POLift.iOS
{
    public partial class ExerciseCell : UITableViewCell
    {
        EventHandler EditClicked;

        public ExerciseCell (IntPtr handle) : base (handle)
        {
        }

        public void Setup(IExercise exercise, EventHandler edit_handler)
        {
            EditClicked = edit_handler;

            EditExerciseButton.TouchUpInside += EditClicked;


            this.ExerciseLabel.Text = exercise.ToString();
        }

        public override void PrepareForReuse()
        {
            if(EditClicked != null)
            {
                EditExerciseButton.TouchUpInside -= EditClicked;
            }

            EditClicked = null;

            base.PrepareForReuse();
        }
    }
}