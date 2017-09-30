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
    using Service;

    class RoutineAdapter : BaseAdapter<Routine>
    {
        List<Routine> routines;
        Context context;

        public RoutineAdapter(Context context, IEnumerable<Routine> exercises)
        {
            this.context = context;
            this.routines = new List<Routine>(exercises);
        }

        public override Routine this[int position]
        {
            get
            {
                return routines[position];
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
            RoutineAdapterViewHolder holder = null;

            if (view != null)
                holder = view.Tag as RoutineAdapterViewHolder;

            if (holder == null)
            {
                holder = new RoutineAdapterViewHolder();
                var inflater = context.GetSystemService(Context.LayoutInflaterService).JavaCast<LayoutInflater>();
                //replace with your item and your holder items
                //comment back in
                view = inflater.Inflate(Resource.Layout.ExerciseItem, parent, false);
                holder.Title = view.FindViewById<TextView>(Resource.Id.ExerciseItemName);
                holder.EditButton = view.FindViewById<Button>(Resource.Id.ExerciseEditButton);

                view.Tag = holder;
            }

            view.Clickable = true;
            holder.Title.Clickable = true;

            holder.EditButton.Click += delegate (object sender, EventArgs e)
            {
                // defined here so we have access to position 
                Helpers.DisplayError(context, $"button {position} clicked");
            };

            //fill in your items
            //holder.Title.Text = "new text here";
            holder.Title.Text = this[position].ToString();

            return view;
        }



        private void EditButton_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public void Add(Routine r)
        {
            routines.Add(r);
            NotifyDataSetChanged();
        }

        //Fill in cound here, currently 0
        public override int Count
        {
            get
            {
                return routines.Count;
            }
        }

    }

    class RoutineAdapterViewHolder : Java.Lang.Object
    {
        //Your adapter views to re-use
        public TextView Title { get; set; }
        public Button EditButton { get; set; }
    }
}