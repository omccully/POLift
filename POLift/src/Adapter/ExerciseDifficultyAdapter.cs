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

namespace POLift
{
    using Core.Model;

    class ExerciseDifficultyAdapter : BaseAdapter<IExerciseDifficulty>
    {
        Context context;
        ObservableCollection<IExerciseDifficulty> exercise_difficulties;

        public ExerciseDifficultyAdapter(Context context, 
            IEnumerable<IExerciseDifficulty> exercise_difficulties)
        {
            this.context = context;
            this.exercise_difficulties = new ObservableCollection<IExerciseDifficulty>(
                exercise_difficulties);
        }

        public override IExerciseDifficulty this[int position]
        {
            get
            {
                return exercise_difficulties[position];
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
            
            ExerciseDifficultyAdapterViewHolder holder = 
                new ExerciseDifficultyAdapterViewHolder();

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
                return exercise_difficulties.Count;
            }
        }

    }

    class ExerciseDifficultyAdapterViewHolder : Java.Lang.Object
    {
        //Your adapter views to re-use
        public TextView Title { get; set; }
    }
}