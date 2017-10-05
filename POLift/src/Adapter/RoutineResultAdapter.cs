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

    class RoutineResultAdapter : BaseAdapter<RoutineResult>
    {
        List<RoutineResult> routine_results;
        Context context;

        public RoutineResultAdapter(Context context, IEnumerable<RoutineResult> routine_results)
        {
            
            this.routine_results = new List<RoutineResult>(routine_results);
            this.context = context;
        }


        public override RoutineResult this[int position]
        {
            get
            {
                return routine_results[position];
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


            RoutineResult rr = this[position];

            //fill in your items
            holder.Text.Text = rr.ToString();

            return view;
        }

        public void RemoveIndex(int pos)
        {
            routine_results.RemoveAt(pos);
        }

        //Fill in cound here, currently 0
        public override int Count
        {
            get
            {
                return routine_results.Count;
            }
        }
    }

    class RoutineResultAdapterViewHolder : Java.Lang.Object
    {
        //Your adapter views to re-use
        public TextView Text { get; set; }
    }
}