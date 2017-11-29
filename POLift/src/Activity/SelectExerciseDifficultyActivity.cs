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

namespace POLift
{
    using Service;
    using Model;

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

            ExercisesDifficultyViewPager = 
                FindViewById<ViewPager>(Resource.Id.ExercisesDifficultyViewPager);

            exercise_difficulty_pager_adapter = new ExerciseDifficultyPagerAdapter(this,
                ExercisesInCategories());
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

        List<KeyValuePair<string, List<IExerciseDifficulty>>> ExercisesInCategories()
        {
            Dictionary<string, List<IExerciseDifficulty>> dict = 
                new Dictionary<string, List<IExerciseDifficulty>>();

            foreach (ExerciseDifficulty ex in Database
                .Table<ExerciseDifficulty>().OrderByDescending(ed => ed.Usage))
            {
                string cat = (ex.Category == null ? DefaultCategory : ex.Category);

                if (dict.ContainsKey(cat))
                {
                    dict[cat].Add(ex);
                }
                else
                {
                    dict[cat] = new List<IExerciseDifficulty>() { ex };
                }
            }

            //POLDatabase.Table<Exercise>()
            //    .GroupBy(ex => ex.Category)
            //    .OrderByDescending(group => group.Count());


            return dict.OrderByDescending(kvp =>
                kvp.Value.Count
            ).ToList();
        }
    }
}