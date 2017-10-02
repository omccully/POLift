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

namespace POLift.Adapter
{
    using Model;

    class ExerciseSetsAdapter : BaseAdapter<ExerciseSets>
    {
        public List<ExerciseSets> ExerciseSetsList;
        Context context;

        public ExerciseSetsAdapter(Context context, IEnumerable<ExerciseSets> exercise_sets)
        {
            this.context = context;
            this.ExerciseSetsList = new List<ExerciseSets>(exercise_sets);
        }

        public override ExerciseSets this[int position]
        {
            get
            {
                return ExerciseSetsList[position];
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
            ExerciseSetsAdapterViewHolder holder = null;

            if (view != null)
                holder = view.Tag as ExerciseSetsAdapterViewHolder;

            if (holder == null)
            {
                holder = new ExerciseSetsAdapterViewHolder();
                var inflater = context.GetSystemService(Context.LayoutInflaterService).JavaCast<LayoutInflater>();
                //replace with your item and your holder items
                //comment back in
                //view = inflater.Inflate(Resource.Layout.item, parent, false);
                //holder.Title = view.FindViewById<TextView>(Resource.Id.text);

                view = inflater.Inflate(Resource.Layout.ExerciseSetsItem, parent, false);
                holder.Layout = view.FindViewById<LinearLayout>(Resource.Id.ExerciseSetsLayout);
                holder.TextBox = view.FindViewById<EditText>(Resource.Id.SetCountText);
                holder.TextView = view.FindViewById<TextView>(Resource.Id.ExerciseSetsName);

                view.Tag = holder;
            }


            //fill in your items
            //holder.Title.Text = "new text here";
            ExerciseSets es = this[position];
            holder.TextBox.Text = es.SetCount.ToString();
            holder.TextView.Text = es.Exercise.Name;


            return view;
        }

        public void Add(ExerciseSets es)
        {
            ExerciseSetsList.Add(es);
            NotifyDataSetChanged();
        }

        //Fill in cound here, currently 0
        public override int Count
        {
            get
            {
                return ExerciseSetsList.Count;
            }
        }

    }

    class ExerciseSetsAdapterViewHolder : Java.Lang.Object
    {
        //Your adapter views to re-use
        public LinearLayout Layout { get; set; }
        public EditText TextBox { get; set; }
        public TextView TextView { get; set; }
    }
}