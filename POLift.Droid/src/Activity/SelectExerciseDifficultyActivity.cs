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
using Android.Support.V4.View;

using Microsoft.Practices.Unity;

namespace POLift.Droid
{
    using Core.Service;
    using Core.Model;

    [Activity(Label = "Select Exercise Difficulty")]
    public class SelectExerciseDifficultyActivity : Activity
    {
        const string DefaultCategory = "other";
        IPOLDatabase Database;

        ViewPager ExercisesDifficultyViewPager;
        ExerciseDifficultyPagerAdapter exercise_difficulty_pager_adapter;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.SelectExerciseDifficulty);

            Database = C.ontainer.Resolve<IPOLDatabase>();

            ExerciseDifficulty.RefreshAllUsages(Database);

            ExercisesDifficultyViewPager = 
                FindViewById<ViewPager>(Resource.Id.ExercisesDifficultyViewPager);

            exercise_difficulty_pager_adapter = new ExerciseDifficultyPagerAdapter(this,
                ExerciseDifficulty.InCategories(Database, DefaultCategory));
            exercise_difficulty_pager_adapter.ListItemClicked += Exercise_difficulty_pager_adapter_ListItemClicked;
            ExercisesDifficultyViewPager.Adapter = exercise_difficulty_pager_adapter;
        }

        private void Exercise_difficulty_pager_adapter_ListItemClicked(object sender, ContainerEventArgs<IExerciseDifficulty> e)
        {
            ReturnExerciseDifficulty(e.Contents);
        }

        void ReturnExerciseDifficulty(IExerciseDifficulty ed)
        {
            ReturnExerciseDifficulty(ed.ID);
        }

        void ReturnExerciseDifficulty(int ed_id)
        {
            Intent result_intent = new Intent();
            result_intent.PutExtra("exercise_difficulty_id", ed_id);

            SetResult(Result.Ok, result_intent);

            Finish();
        }
    }
}