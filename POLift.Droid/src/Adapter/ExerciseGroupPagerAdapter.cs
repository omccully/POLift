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


    class ExerciseGroupPagerAdapter : PagerAdapter
    {
        public event EventHandler<ContainerEventArgs<IExerciseGroup>> ListItemClicked;
        protected virtual void OnListItemClicked(ContainerEventArgs<IExerciseGroup> e)
        {
            ListItemClicked?.Invoke(this, e);
        }

        Activity context;
        List<ExerciseGroupCategory> exercise_groups_in_categories;

        public ExerciseGroupPagerAdapter(Activity context,
            List<ExerciseGroupCategory> exercise_groups_in_categories)
        {
            this.context = context;
            this.exercise_groups_in_categories = exercise_groups_in_categories;
        }

        public override int Count
        {
            get
            {
                return exercise_groups_in_categories.Count;
            }
        }

        public override bool IsViewFromObject(View view, Java.Lang.Object obj)
        {
            return view == obj;
        }

        [Obsolete]
        public override Java.Lang.Object InstantiateItem(View container, int position)
        {
            List<IExerciseGroup> exercise_groups =
                exercise_groups_in_categories[position].ExerciseGroups;

            ListView list_view = new ListView(context);

            list_view.Focusable = true;
            list_view.ItemsCanFocus = true;
            list_view.ItemClick += delegate (object sender, AdapterView.ItemClickEventArgs e)
            {
                OnListItemClicked(new ContainerEventArgs<IExerciseGroup>(exercise_groups[e.Position]));
            };

            ExerciseGroupAdapter exercise_group_adapter =
                new ExerciseGroupAdapter(context, exercise_groups);

            list_view.Adapter = exercise_group_adapter;

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
            return exercise_groups_in_categories[position].Name;
        }

        public string GetCurrentCategory(ViewPager view_pager)
        {
            return GetPageTitleFormattedCLI(view_pager.CurrentItem);
        }

        public int IndexOfCategory(string category)
        {
            return exercise_groups_in_categories.FindIndex(kvp => category == kvp.Name);
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