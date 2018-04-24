using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace POLift.Droid
{
    using Core.Model;

    class ExerciseGroupAdapter : BaseAdapter<IExerciseGroup>
    {
        Context context;
        ObservableCollection<IExerciseGroup> exercise_groups;

        public ExerciseGroupAdapter(Context context,
            IEnumerable<IExerciseGroup> exercise_groups)
        {
            this.context = context;
            this.exercise_groups = new ObservableCollection<IExerciseGroup>(
                exercise_groups);
        }

        public override IExerciseGroup this[int position]
        {
            get
            {
                return exercise_groups[position];
            }
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return position;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {

            ExerciseGroupAdapterViewHolder holder =
                new ExerciseGroupAdapterViewHolder();

            var inflater = context.GetSystemService(
                Context.LayoutInflaterService).JavaCast<LayoutInflater>();

            View view = inflater.Inflate(Resource.Layout.ExerciseDifficultyItem,
                parent, false);
            holder.Title = view.FindViewById<TextView>(Resource.Id.ExerciseDifficultyItemName);

            view.Tag = holder;

            holder.Title.Text = this[position].ToString();

            return view;
        }

        //Fill in cound here, currently 0
        public override int Count
        {
            get
            {
                return exercise_groups.Count;
            }
        }

    }

    class ExerciseGroupAdapterViewHolder : Java.Lang.Object
    {
        //Your adapter views to re-use
        public TextView Title { get; set; }
    }
}