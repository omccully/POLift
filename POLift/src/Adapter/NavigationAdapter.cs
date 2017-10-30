using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

    class NavigationAdapter : BaseAdapter<INavigation>
    {
        public ObservableCollection<INavigation> Navigations
            { get; private set; }
        Context context;

        public NavigationAdapter(Context context, 
            IEnumerable<INavigation> navigations)
        {
            this.context = context;
            this.Navigations = 
                new ObservableCollection<INavigation>(navigations);
            this.Navigations.CollectionChanged += Navigations_CollectionChanged;
        }

        private void Navigations_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NotifyDataSetChanged();
        }

        public override INavigation this[int position]
        {
            get
            {
                return Navigations[position];
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
            NavigationAdapterViewHolder holder = null;

            if (view != null)
                holder = view.Tag as NavigationAdapterViewHolder;

            if (holder == null)
            {
                holder = new NavigationAdapterViewHolder();
                var inflater = context.GetSystemService(Context.LayoutInflaterService).JavaCast<LayoutInflater>();
                //replace with your item and your holder items
                //comment back in
                view = inflater.Inflate(Resource.Layout.NavigationItem, parent, false);
                holder.Title = view.FindViewById<TextView>(Resource.Id.navigation_text);
                holder.Icon = view.FindViewById<ImageView>(Resource.Id.navigation_icon);


                view.Tag = holder;
            }


            //fill in your items
            holder.Title.Text = Navigations[position].Text;
            holder.Icon.SetImageResource(Navigations[position].IconResourceID);

            return view;
        }

        //Fill in cound here, currently 0
        public override int Count
        {
            get
            {
                return Navigations.Count();
            }
        }

    }

    class NavigationAdapterViewHolder : Java.Lang.Object
    {
        //Your adapter views to re-use
        public ImageView Icon { get; set; }
        public TextView Title { get; set; }
    }
}