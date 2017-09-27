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
    [Activity(Label = "Select Exercise")]
    public class SelectExerciseActivity : ListActivity
    {
        List<Exercise> ExerciseList;
        ExerciseAdapter exercise_adapter;

        static int CreateExerciseRequestCode = 3;

        Button CreateExerciseLink;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.SelectExercise);

            ExerciseList = GetDefaultExercises();

            exercise_adapter = new ExerciseAdapter(this, ExerciseList);
            this.ListAdapter = exercise_adapter;

            ListView.ItemClick += ListView_ItemClick;

            CreateExerciseLink = FindViewById<Button>(Resource.Id.CreateExerciseLink);
            CreateExerciseLink.Click += CreateExerciseLink_Click;
        }

        List<Exercise> GetDefaultExercises()
        {
            List<Exercise> result = new List<Exercise>();

            using (Stream input = Assets.Open("default_exercises.xml"))
            {
                using (StreamReader sr = new StreamReader(input))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(sr.ReadToEnd());

                    foreach (XmlNode node in doc.GetElementsByTagName("Exercise"))
                    {
                        result.Add(Exercise.FromXmlNode(node));
                    }
                }
            }

            return result;
        }

        private void CreateExerciseLink_Click(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(CreateExerciseActivity));

            StartActivityForResult(intent, CreateExerciseRequestCode);
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if(resultCode == Result.Ok && requestCode == CreateExerciseRequestCode)
            {
                Exercise new_exercise = Exercise.FromXml(data.GetStringExtra("exercise"));
                exercise_adapter.Add(new_exercise);
                ReturnExercise(new_exercise);
            }
        }

        private void ListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            Exercise exercise = exercise_adapter[e.Position];

            ReturnExercise(exercise);
        }

        void ReturnExercise(Exercise exercise)
        {
            Intent result_intent = new Intent();
            result_intent.PutExtra("exercise", exercise.ToXml());

            SetResult(Result.Ok, result_intent);

            Finish();
        }
    }
}