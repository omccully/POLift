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

namespace POLift.Droid
{
    using Core.Model;
    using static Android.Views.View;

    class ExercisesPagerAdapter : PagerAdapter
    {
        public event EventHandler<ExerciseEventArgs> DeleteButtonClicked;
        protected virtual void OnDeleteButtonClicked(ExerciseEventArgs e)
        {
            DeleteButtonClicked?.Invoke(this, e);
        }

        public event EventHandler<ExerciseEventArgs> EditButtonClicked;
        protected virtual void OnEditButtonClicked(ExerciseEventArgs e)
        {
            EditButtonClicked?.Invoke(this, e);
        }

        public event EventHandler<ExerciseEventArgs> ListItemClicked;
        protected virtual void OnListItemClicked(ExerciseEventArgs e)
        {
            ListItemClicked?.Invoke(this, e);
        }

        Activity context;
        List<ExerciseCategory> exercises_in_categories;

        public ExercisesPagerAdapter(Activity context, 
            List<ExerciseCategory> exercises_in_categories)
        {
            this.context = context;
            this.exercises_in_categories = exercises_in_categories;

        }

        public override int Count
        {
            get
            {
                return exercises_in_categories.Count;
            }
        }

        public override bool IsViewFromObject(View view, Java.Lang.Object obj)
        {
            return view == obj;
        }

        [Obsolete]
        public override Java.Lang.Object InstantiateItem(View container, int position)
        { 
            
            List<IExercise> exercises = exercises_in_categories[position].Exercises;

            ListView list_view = new ListView(context);

            list_view.Focusable = true;
            list_view.ItemsCanFocus = true;
            list_view.ItemClick += delegate (object sender, AdapterView.ItemClickEventArgs e)
            {
                OnListItemClicked(new ExerciseEventArgs(exercises[e.Position]));
            };

            ExerciseAdapter exercise_adapter = new ExerciseAdapter(context, exercises);

            exercise_adapter.EditButtonClicked += Exercise_adapter_EditButtonClicked;
            exercise_adapter.DeleteButtonClicked += Exercise_adapter_DeleteButtonClicked;

            list_view.Adapter = exercise_adapter;

            ViewPager view_pager = container.JavaCast<ViewPager>();
            
            view_pager.AddView(list_view);

            return list_view;
        }

        private void Exercise_adapter_EditButtonClicked(object sender, ExerciseEventArgs e)
        {
            OnEditButtonClicked(e);
        }

        private void Exercise_adapter_DeleteButtonClicked(object sender, ExerciseEventArgs e)
        {
            OnDeleteButtonClicked(e);
        }

        [Obsolete]
        public override void DestroyItem(View container, int position, Java.Lang.Object view)
        {
            ViewPager view_pager = container.JavaCast<ViewPager>();
            view_pager.RemoveView(view as View);
        }

        public override Java.Lang.ICharSequence GetPageTitleFormatted(int position)
        {

            return new Java.Lang.String(GetPageTitleFormattedCLI(position));
        }

        public string GetPageTitleFormattedCLI(int position)
        {
            return exercises_in_categories[position].Name;
        }

        public string GetCurrentCategory(ViewPager view_pager)
        {
            return GetPageTitleFormattedCLI(view_pager.CurrentItem);
        }


        public int IndexOfCategory(string category)
        {
            return exercises_in_categories.FindIndex(kvp => category == kvp.Name);
        }

        public void GoToCategory(string category, ViewPager view_pager)
        {
            int index = IndexOfCategory(category);
            if(index != -1)
            {
                view_pager.SetCurrentItem(index, false);
            }
        }
    }
}