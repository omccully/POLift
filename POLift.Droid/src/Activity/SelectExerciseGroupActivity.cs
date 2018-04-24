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
    using Core.ViewModel;

    [Activity(Label = "SelectExerciseGroupActivity")]
    public class SelectExerciseGroupActivity : Activity
    {
        private SelectExerciseGroupViewModel Vm
        {
            get
            {
                return ViewModelLocator.Default.SelectExerciseName;
            }
        }

        const string DefaultCategory = "other";

        ViewPager ExercisesGroupViewPager;
        ExerciseGroupPagerAdapter exercise_group_pager_adapter;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.SelectExerciseDifficulty);

            //ExerciseDifficulty.RefreshAllUsages(Database);

            ExercisesGroupViewPager =
                FindViewById<ViewPager>(Resource.Id.ExercisesDifficultyViewPager);

            exercise_group_pager_adapter = new ExerciseGroupPagerAdapter(this,
                Vm.ExerciseCategories);
            exercise_group_pager_adapter.ListItemClicked += Exercise_group_pager_adapter_ListItemClicked;
            ExercisesGroupViewPager.Adapter = exercise_group_pager_adapter;
        }

        private void Exercise_group_pager_adapter_ListItemClicked(object sender, ContainerEventArgs<IExerciseGroup> e)
        {
            ReturnExerciseGroup(e.Contents);
        }

        void ReturnExerciseGroup(IExerciseGroup eg)
        {
            Intent result_intent = new Intent();
            result_intent.PutExtra("exercise_ids", eg.ExerciseIDs);
            result_intent.PutExtra("exercise_group_name", eg.Name);

            SetResult(Result.Ok, result_intent);

            Finish();
        }
    }
}