using Android.App;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;

namespace POLift
{
    using Model;

    class RoutineEventArgs
    {
        public readonly Routine Routine;

        public RoutineEventArgs(Routine Routine)
        {
            this.Routine = Routine;
        }
    }

    class RoutineAdapter : BaseAdapter<Routine>
    {
        List<Routine> routines;
        Activity context;

        public event EventHandler<RoutineEventArgs> DeleteButtonClicked;
        protected virtual void OnDeleteButtonClicked(RoutineEventArgs e)
        {
            DeleteButtonClicked?.Invoke(this, e);
        }

        public event EventHandler<RoutineEventArgs> EditButtonClicked;
        protected virtual void OnEditButtonClicked(RoutineEventArgs e)
        {
            EditButtonClicked?.Invoke(this, e);
        }

        public RoutineAdapter(Activity context, IEnumerable<Routine> exercises)
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
            throw new NotImplementedException();
            //return position;
        }

        public override long GetItemId(int position)
        {
            //throw new NotImplementedException();
            return position;
        }

        System.Runtime.Serialization.ObjectIDGenerator idg = new System.Runtime.Serialization.ObjectIDGenerator();

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            RoutineAdapterViewHolder holder = new RoutineAdapterViewHolder();

            var view = context.LayoutInflater.Inflate(Resource.Layout.ExerciseItem, null);

            holder.Title = view.FindViewById<TextView>(Resource.Id.ExerciseItemName);
            holder.EditButton = view.FindViewById<Button>(Resource.Id.ExerciseEditButton);
            holder.DeleteButton = view.FindViewById<Button>(Resource.Id.ExerciseDeleteButton);

            view.Tag = holder;

            holder.EditButton.Click += delegate (object sender, EventArgs e)
            {
                // defined here so we have access to position 
                OnEditButtonClicked(new RoutineEventArgs(this[position]));
            };

            holder.DeleteButton.Click += delegate (object sender, EventArgs e)
            {
                // defined here so we have access to position 
                OnDeleteButtonClicked(new RoutineEventArgs(this[position]));
            };

            //fill in your items
            //holder.Title.Text = "new text here";
            holder.Title.Text = this[position].ToString();

            return view;
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
        public Button DeleteButton { get; set; }
    }
}