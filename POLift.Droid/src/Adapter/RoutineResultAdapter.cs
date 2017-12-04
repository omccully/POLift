using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace POLift.Droid
{
    using Core.Model;

    class RoutineResultAdapter : BaseAdapter<IRoutineResult>
    {
        public readonly ObservableCollection<IRoutineResult> RoutineResults;
        Context context;

        public event EventHandler<ContainerEventArgs<IRoutineResult>> DeleteButtonClicked;
        protected virtual void OnDeleteButtonClicked(ContainerEventArgs<IRoutineResult> e)
        {
            DeleteButtonClicked?.Invoke(this, e);
        }

        public event EventHandler<ContainerEventArgs<IRoutineResult>> EditButtonClicked;
        protected virtual void OnEditButtonClicked(ContainerEventArgs<IRoutineResult> e)
        {
            EditButtonClicked?.Invoke(this, e);
        }

        public RoutineResultAdapter(Context context, IEnumerable<IRoutineResult> routine_results)
        {
            this.context = context;
            this.RoutineResults = new ObservableCollection<IRoutineResult>(routine_results);

            this.RoutineResults.CollectionChanged += RoutineResults_CollectionChanged;
        }

        private void RoutineResults_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NotifyDataSetChanged();
        }

        public override IRoutineResult this[int position]
        {
            get
            {
                return RoutineResults[position];
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
            /*var view = convertView;
            RoutineResultAdapterViewHolder holder = null;

            if (view != null)
                holder = view.Tag as RoutineResultAdapterViewHolder;

            if (holder == null)
            {
                holder = new RoutineResultAdapterViewHolder();
                var inflater = context.GetSystemService(Context.LayoutInflaterService).JavaCast<LayoutInflater>();
                //replace with your item and your holder items
                //comment back in
                view = inflater.Inflate(Resource.Layout.RoutineResult, parent, false);
                holder.Text = view.FindViewById<TextView>(Resource.Id.RoutineResultTextView);
                holder.EditButton = view.FindViewById<ImageButton>(
                    Resource.Id.RoutineResultEditButton);
                holder.DeleteButton = view.FindViewById<ImageButton>(
                    Resource.Id.RoutineResultDeleteButton);

                view.Tag = holder;
            }*/

            // TODO: recycle these views. the problem is that multiple event handlers
            // end up getting hooked up to the same event. might be able to remedy this
            // by only hooking up new event handlers when new holders are created
            // the holder can hold a reference to the object in question (IRoutineResult)
            // so the delegate can obtain the most accurate value for that from the holder object.

            RoutineResultAdapterViewHolder holder = new RoutineResultAdapterViewHolder();
            var inflater = context.GetSystemService(Context.LayoutInflaterService).JavaCast<LayoutInflater>();
            View view = inflater.Inflate(Resource.Layout.RoutineResult, null);
            holder.Text = view.FindViewById<TextView>(Resource.Id.RoutineResultTextView);
            holder.EditButton = view.FindViewById<ImageButton>(
                Resource.Id.RoutineResultEditButton);
            holder.DeleteButton = view.FindViewById<ImageButton>(
                Resource.Id.RoutineResultDeleteButton);

            IRoutineResult rr = this[position];

            //fill in your items
            holder.Text.Text = rr.ToString();
            holder.EditButton.Click += delegate
            {
                OnEditButtonClicked(new ContainerEventArgs<IRoutineResult>(rr));
            };

            holder.DeleteButton.Click += delegate
            {
                OnDeleteButtonClicked(new ContainerEventArgs<IRoutineResult>(rr));
            };

            return view;
        }

        //Fill in cound here, currently 0
        public override int Count
        {
            get
            {
                return RoutineResults.Count;
            }
        }
    }

    class RoutineResultAdapterViewHolder : Java.Lang.Object
    {
        //Your adapter views to re-use
        public TextView Text { get; set; }
        public ImageButton EditButton { get; set; }
        public ImageButton DeleteButton { get; set; }
    }
}