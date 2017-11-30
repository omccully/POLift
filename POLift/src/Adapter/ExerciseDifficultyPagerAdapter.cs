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

namespace POLift
{
    using Core.Model;
    using static Android.Views.View;

    class ExerciseDifficultyPagerAdapter : PagerAdapter
    {
        public event EventHandler<ContainerEventArgs<IExerciseDifficulty>> ListItemClicked;
        protected virtual void OnListItemClicked(ContainerEventArgs<IExerciseDifficulty> e)
        {
            ListItemClicked?.Invoke(this, e);
        }

        Activity context;
        List<KeyValuePair<string, List<IExerciseDifficulty>>> exercise_difficulties_in_categories;

        public ExerciseDifficultyPagerAdapter(Activity context,
            List<KeyValuePair<string, List<IExerciseDifficulty>>> exercise_difficulties_in_categories)
        {
            this.context = context;
            this.exercise_difficulties_in_categories = exercise_difficulties_in_categories;
        }

        public override int Count
        {
            get
            {
                return exercise_difficulties_in_categories.Count;
            }
        }

        public override bool IsViewFromObject(View view, Java.Lang.Object obj)
        {
            return view == obj;
        }

        [Obsolete]
        public override Java.Lang.Object InstantiateItem(View container, int position)
        {
            List<IExerciseDifficulty> exercise_difficulties = exercise_difficulties_in_categories[position].Value;

            ListView list_view = new ListView(context);

            list_view.Focusable = true;
            list_view.ItemsCanFocus = true;
            list_view.ItemClick += delegate (object sender, AdapterView.ItemClickEventArgs e)
            {
                OnListItemClicked(new ContainerEventArgs<IExerciseDifficulty>(exercise_difficulties[e.Position]));
            };

            ExerciseDifficultyAdapter exercise_difficulty_adapter = 
                new ExerciseDifficultyAdapter(context, exercise_difficulties);

            list_view.Adapter = exercise_difficulty_adapter;

            ViewPager view_pager = container.JavaCast<ViewPager>();

            view_pager.AddView(list_view);

            return list_view;
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
            return exercise_difficulties_in_categories[position].Key;
        }

        public string GetCurrentCategory(ViewPager view_pager)
        {
            return GetPageTitleFormattedCLI(view_pager.CurrentItem);
        }


        public int IndexOfCategory(string category)
        {
            return exercise_difficulties_in_categories.FindIndex(kvp => category == kvp.Key);
        }

        public void GoToCategory(string category, ViewPager view_pager)
        {
            int index = IndexOfCategory(category);
            if (index != -1)
            {
                view_pager.SetCurrentItem(index, false);
            }
        }
    }
}