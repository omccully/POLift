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
    using Service;

    class ExerciseSetsAdapter : BaseAdapter<ExerciseSets>
    {
        public List<ExerciseSets> ExerciseSetsList;
        Context context;
        int locked_sets;

        public ExerciseSetsAdapter(Context context, IEnumerable<ExerciseSets> exercise_sets, int locked_sets=0)
        {
            this.context = context;
            this.ExerciseSetsList = new List<ExerciseSets>(exercise_sets);
            this.locked_sets = locked_sets;
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
            ExerciseSetsAdapterViewHolder holder = new ExerciseSetsAdapterViewHolder();
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
            
            //fill in your items
            //holder.Title.Text = "new text here";
            ExerciseSets es = this[position];
            holder.TextBox.Text = es.SetCount.ToString();
            holder.TextView.Text = " sets of " + es.Exercise.Name;
            //holder.TextView.Text = " sets of " + es.Exercise;


            if (position < locked_sets)
            {
                holder.TextBox.Enabled = false;
                holder.TextView.Text += " (locked)";
            }
            else
            {
                holder.TextBox.TextChanged += delegate
                {
                    try
                    {
                        es.SetCount = Int32.Parse(holder.TextBox.Text);
                        if (es.SetCount == 0)
                        {
                            Helpers.DisplayConfirmation(context, "Would you like to delete this exercise?",
                                delegate {
                                    ExerciseSetsList.RemoveAt(position);
                                    NotifyDataSetChanged();
                                });
                        }
                    }
                    catch (FormatException) { }
                };
            }
            

            


            return view;
        }

        public void Add(ExerciseSets es)
        {
            ExerciseSetsList.Add(es);
            NotifyDataSetChanged();
        }

        public void RemoveZeroSets()
        {
            if(ExerciseSetsList.RemoveAll(ex_sets => ex_sets.SetCount == 0) > 0)
            {
                NotifyDataSetChanged();
            }
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