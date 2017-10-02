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
using Java.IO;
using System.IO;
using System.Xml;

namespace POLift
{
    using Model;
    using Service;

    [Activity(Label = "Select Exercise")]
    public class SelectExerciseActivity : ListActivity
    {
        ExerciseAdapter exercise_adapter;

        const int CreateExerciseRequestCode = 3;
        const int ExerciseEditedRequestCode = 4;

        Button CreateExerciseLink;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.SelectExercise);

            CreateExerciseLink = FindViewById<Button>(Resource.Id.CreateExerciseLink);

            this.ListView.Focusable = true;
            this.ListView.ItemsCanFocus = true;

            RefreshExerciseList();

            ListView.ItemClick += ListView_ItemClick;

            CreateExerciseLink.Click += CreateExerciseLink_Click;
        }

        void RefreshExerciseList()
        {
            exercise_adapter = new ExerciseAdapter(this, 
                POLDatabase.TableWhereUndeleted<Exercise>().ToList());
            this.ListAdapter = exercise_adapter;

            exercise_adapter.DeleteButtonClicked += Exercise_adapter_DeleteButtonClicked;
            exercise_adapter.EditButtonClicked += Exercise_adapter_EditButtonClicked;
        }

        private void Exercise_adapter_EditButtonClicked(object sender, ExerciseEventArgs e)
        {
            var intent = new Intent(this, typeof(CreateExerciseActivity));
            intent.PutExtra("edit_exercise_id", e.Exercise.ID);
            StartActivityForResult(intent, ExerciseEditedRequestCode);
        }

        private void Exercise_adapter_DeleteButtonClicked(object sender, ExerciseEventArgs e)
        {
            Helpers.DisplayConfirmation(this, "Are you sure you want to remove the \"" +
                e.Exercise.ToString() + "\" exercise? (this won't have any effect" + 
                " on any routines that use this exercise)", delegate
                {
                    POLDatabase.HideDeletable(e.Exercise);
                    RefreshExerciseList();
                });
        }

        private void CreateExerciseLink_Click(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(CreateExerciseActivity));

            StartActivityForResult(intent, CreateExerciseRequestCode);
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if(resultCode == Result.Ok)
            {
                if (requestCode == CreateExerciseRequestCode ||
                    requestCode == ExerciseEditedRequestCode)
                {
                    //Exercise new_exercise = Exercise.FromXml(data.GetStringExtra("exercise"));
                    int id = data.GetIntExtra("exercise_id", -1);
                    if (id == -1) return;

                    ReturnExercise(id);
                    //Exercise new_exercise = POLDatabase.ReadByID<Exercise>(id);

                    //exercise_adapter.Add(new_exercise);

                    // if the user just created an exercise, just return it back to the CreateRoutineActivity
                    //ReturnExercise(new_exercise);
                }
                else
                {
                    // optionally, we could return the exercise_id

                    // refresh ExerciseListView
                    RefreshExerciseList();
                }
            }
        }

        private void ListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            Exercise exercise = exercise_adapter[e.Position];

            ReturnExercise(exercise);
        }

        void ReturnExercise(Exercise exercise)
        {
            ReturnExercise(exercise.ID);
        }

        void ReturnExercise(int ID)
        {
            Intent result_intent = new Intent();
            result_intent.PutExtra("exercise_id", ID);

            SetResult(Result.Ok, result_intent);

            Finish();
        }
    }
}