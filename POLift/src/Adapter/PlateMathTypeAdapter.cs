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
    using Service;

    class PlateMathTypeAdapter : BaseAdapter<PlateMath>
    {
        List<PlateMath> PlateMathTypes;
        Context context;

        public PlateMathTypeAdapter(Context context, IEnumerable<PlateMath> maths)
        {
            this.context = context;
            PlateMathTypes = new List<PlateMath>(maths);
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return position;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override PlateMath this[int position]
        {
            get
            {
                return PlateMathTypes[position];
            }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = convertView;
            PlateMathTypeAdapterViewHolder holder = null;

            if (view != null)
                holder = view.Tag as PlateMathTypeAdapterViewHolder;

            if (holder == null)
            {
                holder = new PlateMathTypeAdapterViewHolder();
                var inflater = context.GetSystemService(Context.LayoutInflaterService).JavaCast<LayoutInflater>();
                //replace with your item and your holder items
                //comment back in
                view = inflater.Inflate(Resource.Layout.PlateMathItem, parent, false);
                holder.Title = view.FindViewById<TextView>(Resource.Id.PlateMathTextView);
                view.Tag = holder;
            }


            //fill in your items
            PlateMath pm = this[position];
            if(pm == null)
            {
                holder.Title.Text = "None";
            }
            else
            {
                holder.Title.Text = pm.ToString();
            }  

            return view;
        }

        //Fill in cound here, currently 0
        public override int Count
        {
            get
            {
                return PlateMathTypes.Count;
            }
        }
    }

    class PlateMathTypeAdapterViewHolder : Java.Lang.Object
    {
        //Your adapter views to re-use
        public TextView Title { get; set; }
    }
}