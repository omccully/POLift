using Android.App;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace POLift
{
    using Model;

    class RoutineEventArgs
    {
        public readonly IRoutine Routine;

        public RoutineEventArgs(IRoutine Routine)
        {
            this.Routine = Routine;
        }
    }

    class RoutineAdapter : BaseAdapter<IRoutine>
    {
        ObservableCollection<IRoutine> Routines;
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

        public RoutineAdapter(Activity context, IEnumerable<IRoutine> routines)
        {
            this.context = context;
            this.Routines = new ObservableCollection<IRoutine>(routines);
            Routines.CollectionChanged += Routines_CollectionChanged;
        }

        void Routines_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NotifyDataSetChanged();
        }

        public override IRoutine this[int position]
        {
            get
            {
                return Routines[position];
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
            holder.EditButton = view.FindViewById<ImageButton>(Resource.Id.ExerciseEditButton);
            holder.DeleteButton = view.FindViewById<ImageButton>(Resource.Id.ExerciseDeleteButton);

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

        //Fill in cound here, currently 0
        public override int Count
        {
            get
            {
                return Routines.Count;
            }
        }

    }

    class RoutineAdapterViewHolder : Java.Lang.Object
    {
        //Your adapter views to re-use
        public TextView Title { get; set; }
        public ImageButton EditButton { get; set; }
        public ImageButton DeleteButton { get; set; }
    }
}