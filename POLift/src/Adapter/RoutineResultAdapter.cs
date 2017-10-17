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

namespace POLift
{
    using Model;

    class RoutineResultAdapter : BaseAdapter<IRoutineResult>
    {
        public readonly ObservableCollection<IRoutineResult> RoutineResults;
        Context context;

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
            var view = convertView;
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
                view.Tag = holder;
            }


            IRoutineResult rr = this[position];

            //fill in your items
            holder.Text.Text = rr.ToString();

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
    }
}