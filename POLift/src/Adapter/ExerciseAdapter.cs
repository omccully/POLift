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

namespace POLift
{
    using Model;

    class ExerciseAdapter : BaseAdapter<Exercise>
    {
        List<Exercise> exercises;
        Context context;

        public ExerciseAdapter(Context context, IEnumerable<Exercise> exercises)
        {
            this.context = context;
            this.exercises = new List<Exercise>(exercises);
        }

        public override Exercise this[int position]
        {
            get
            {
                return exercises[position];
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
            var view = convertView;
            ExerciseAdapterViewHolder holder = null;

            if (view != null)
                holder = view.Tag as ExerciseAdapterViewHolder;

            if (holder == null)
            {
                holder = new ExerciseAdapterViewHolder();
                var inflater = context.GetSystemService(Context.LayoutInflaterService).JavaCast<LayoutInflater>();
                //replace with your item and your holder items
                //comment back in
                view = inflater.Inflate(Resource.Layout.ExerciseItem, parent, false);
                holder.Title = view.FindViewById<TextView>(Resource.Id.ExerciseItemName);

                view.Tag = holder;
            }


            //fill in your items
            //holder.Title.Text = "new text here";
            holder.Title.Text = this[position].ToString();

            return view;
        }

        public void Add(Exercise ex)
        {
            exercises.Add(ex);
            NotifyDataSetChanged();
        }

        //Fill in cound here, currently 0
        public override int Count
        {
            get
            {
                return exercises.Count;
            }
        }

    }

    class ExerciseAdapterViewHolder : Java.Lang.Object
    {
        //Your adapter views to re-use
        public TextView Title { get; set; }
    }
}