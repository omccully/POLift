﻿using System;
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
using Android.Support.V4.View;

using Microsoft.Practices.Unity;

namespace POLift.Droid
{
    using Service;
    using Core.Model;
    using Core.Service;

    [Activity(Label = "Select Exercise")]
    public class SelectExerciseActivity : Activity
    {
        const string DefaultCategory = "other";
        const string DeletedCategory = "DELETED";

        const int CreateExerciseRequestCode = 3;
        const int ExerciseEditedRequestCode = 4;

        Button CreateExerciseLink;

        ViewPager ExercisesViewPager;
        ExercisesPagerAdapter exercises_pager_adapter;

        IPOLDatabase Database;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Database = C.ontainer.Resolve<IPOLDatabase>();

            // Create your application here
            SetContentView(Resource.Layout.SelectExercise);

            CreateExerciseLink = FindViewById<Button>(Resource.Id.CreateExerciseLink);

            ExercisesViewPager = FindViewById<ViewPager>(Resource.Id.ExercisesViewPager);

            RefreshExerciseList();

            CreateExerciseLink.Click += CreateExerciseLink_Click;

            string routine_name = Intent.GetStringExtra("routine_name");

            if(routine_name != null)
            {
                ExercisesViewPager.PostDelayed(delegate
                {
                    exercises_pager_adapter.GoToCategory(routine_name, ExercisesViewPager);
                }, 50);
            }
        }

        private void Exercises_pager_adapter_ListItemClicked(object sender, ExerciseEventArgs e)
        {
            ReturnExercise(e.Exercise);
        }

        string CurrentCategory()
        {
            return exercises_pager_adapter?.GetCurrentCategory(ExercisesViewPager);
        }

        void RefreshExerciseList()
        {
            Exercise.RefreshAllUsages(Database);
            string category = CurrentCategory();

            exercises_pager_adapter = new ExercisesPagerAdapter(this,
                Exercise.InCategories(Database, 
                    DefaultCategory, DeletedCategory));
            exercises_pager_adapter.ListItemClicked += Exercises_pager_adapter_ListItemClicked;
            exercises_pager_adapter.EditButtonClicked += Exercises_pager_adapter_EditButtonClicked;
            exercises_pager_adapter.DeleteButtonClicked += Exercises_pager_adapter_DeleteButtonClicked;

            if(category != null)
            {
                ExercisesViewPager.PostDelayed(delegate
                {
                    exercises_pager_adapter.GoToCategory(category, ExercisesViewPager);
                }, 50);
            }

            ExercisesViewPager.Adapter = exercises_pager_adapter;
        }

        private void Exercises_pager_adapter_DeleteButtonClicked(object sender, ExerciseEventArgs e)
        {
            AndroidHelpers.DisplayConfirmation(this, "Are you sure you want to remove the \"" +
                e.Exercise.ToString() + "\" exercise? (this won't have any effect" +
                " on any routines that use this exercise)", delegate
                {
                    Database.HideDeletable((Exercise)e.Exercise);
                    RefreshExerciseList();
                });
        }

        private void Exercises_pager_adapter_EditButtonClicked(object sender, ExerciseEventArgs e)
        {
            var intent = new Intent(this, typeof(CreateExerciseActivity));
            intent.PutExtra("edit_exercise_id", e.Exercise.ID);
            StartActivityForResult(intent, ExerciseEditedRequestCode);
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

        

        void ReturnExercise(IExercise exercise)
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