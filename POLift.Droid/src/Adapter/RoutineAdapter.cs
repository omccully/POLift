using Android.App;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Diagnostics;

namespace POLift.Droid
{
    using Core.Model;

    class RoutineEventArgs
    {
        public readonly IRoutine Routine;

        public RoutineEventArgs(IRoutine Routine)
        {
            this.Routine = Routine;
        }
    }

    class RoutineAdapter : BaseAdapter<IRoutineWithLatestResult>
    {
        public ObservableCollection<IRoutineWithLatestResult> Data;
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

        public RoutineAdapter(Activity context, IEnumerable<IRoutineWithLatestResult> data)
        {
            this.context = context;
            this.Data = new ObservableCollection<IRoutineWithLatestResult>(data);
            Data.CollectionChanged += Data_CollectionChanged;
        }

        void Data_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NotifyDataSetChanged();
        }

        public override IRoutineWithLatestResult this[int position]
        {
            get
            {
                return Data[position];
            }
        }


        public override Java.Lang.Object GetItem(int position)
        {
            throw new NotImplementedException();
            //return position;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            RoutineAdapterViewHolder holder = new RoutineAdapterViewHolder();

            var view = context.LayoutInflater.Inflate(Resource.Layout.RoutineItem, null);

            holder.Title = view.FindViewById<TextView>(Resource.Id.RoutineItemName);
            holder.Subtext = view.FindViewById<TextView>(Resource.Id.RoutineMoreDetails);
            holder.EditButton = view.FindViewById<ImageButton>(Resource.Id.RoutineEditButton);
            holder.DeleteButton = view.FindViewById<ImageButton>(Resource.Id.RoutineDeleteButton);

            view.Tag = holder;

            holder.EditButton.Click += delegate (object sender, EventArgs e)
            {
                // defined here so we have access to position 
                OnEditButtonClicked(new RoutineEventArgs(this[position].Routine));
            };

            holder.DeleteButton.Click += delegate (object sender, EventArgs e)
            {
                // defined here so we have access to position 
                OnDeleteButtonClicked(new RoutineEventArgs(this[position].Routine));
            };


            //fill in your items
            //holder.Title.Text = "new text here";
            holder.Title.Text = this[position].Routine.ToString();

            IRoutineResult latest = this[position].LatestResult;
            if(latest == null)
            {
                holder.Subtext.Text = "Never performed";
            }
            else
            {
                holder.Subtext.Text = latest.RelativeTimeDetails;
            }
            
            if(holder.Subtext.Text.Contains("Uncompleted") &&
                !holder.Subtext.Text.Contains("day"))
            {

                holder.Subtext.SetTextColor(Android.Graphics.Color.Red);
            }
            else
            {
                holder.Subtext.SetTextColor(Android.Graphics.Color.White);
            }

            return view;
        }

        //Fill in cound here, currently 0
        public override int Count
        {
            get
            {
                return Data.Count;
            }
        }

    }

    class RoutineAdapterViewHolder : Java.Lang.Object
    {
        //Your adapter views to re-use
        public TextView Title { get; set; }
        public TextView Subtext { get; set; }
        public ImageButton EditButton { get; set; }
        public ImageButton DeleteButton { get; set; }
    }
}