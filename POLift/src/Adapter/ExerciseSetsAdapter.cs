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

namespace POLift.Adapter
{
    using Model;
    using Service;

    class ExerciseSetsAdapter : BaseAdapter<IExerciseSets>
    {
        public ObservableCollection<IExerciseSets> ExerciseSets;
        Context context;
        int locked_sets;

        public ExerciseSetsAdapter(Context context, IEnumerable<IExerciseSets> exercise_sets, int locked_sets=0)
        {
            this.context = context;
            this.ExerciseSets = new ObservableCollection<IExerciseSets>(exercise_sets);
            this.locked_sets = locked_sets;
            ExerciseSets.CollectionChanged += ExerciseSets_CollectionChanged;
        }

        void ExerciseSets_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NotifyDataSetChanged();
        }

        public override IExerciseSets this[int position]
        {
            get
            {
                return ExerciseSets[position];
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
            //var view = convertView;
            ExerciseSetsAdapterViewHolder holder = new ExerciseSetsAdapterViewHolder();
            var inflater = context.GetSystemService(Context.LayoutInflaterService).JavaCast<LayoutInflater>();
            //replace with your item and your holder items
            //comment back in
            //view = inflater.Inflate(Resource.Layout.item, parent, false);
            //holder.Title = view.FindViewById<TextView>(Resource.Id.text);

            View view = inflater.Inflate(Resource.Layout.ExerciseSetsItem, parent, false);
            holder.Layout = view.FindViewById<LinearLayout>(Resource.Id.ExerciseSetsLayout);
            holder.TextBox = view.FindViewById<EditText>(Resource.Id.SetCountText);
            holder.TextView = view.FindViewById<TextView>(Resource.Id.ExerciseSetsName);
            holder.MoveUpButton = view.FindViewById<ImageButton>(Resource.Id.ExerciseSetsMoveUpButton);

            view.Tag = holder;
            
            //fill in your items
            //holder.Title.Text = "new text here";
            IExerciseSets es = this[position];
            holder.TextBox.Text = es.SetCount.ToString();
            holder.TextView.Text = " sets of " + es.Exercise.Name;
            //holder.TextView.Text = " sets of " + es.Exercise;

            if(position <= locked_sets)
            {
                holder.MoveUpButton.Enabled = false;
            }
            else
            {
                holder.MoveUpButton.Click += delegate
                {
                    if (position > 0)
                    {
                        // swap elements at position and position-1
                        IExerciseSets temp = this[position];
                        this.ExerciseSets[position] = this[position - 1];
                        this.ExerciseSets[position - 1] = temp;
                    }
                };
            }

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
                                    ExerciseSets.RemoveAt(position);
                                    NotifyDataSetChanged();
                                });
                        }
                    }
                    catch (FormatException) { }
                };
            }
            
            return view;
        }

        public void RemoveZeroSets()
        {
            ExerciseSets.RemoveAll(ex_sets => ex_sets.SetCount == 0);
        }

        public void Regroup(IPOLDatabase database)
        {
            List<IExerciseSets> new_exercise_sets =
                POLift.Model.ExerciseSets.Regroup(
                    this.ExerciseSets,
                    database);

            this.ExerciseSets.Clear();
            foreach (IExerciseSets es in new_exercise_sets)
            {
                this.ExerciseSets.Add(es);
            }
        }

        //Fill in cound here, currently 0
        public override int Count
        {
            get
            {
                return ExerciseSets.Count;
            }
        }
    }

    class ExerciseSetsAdapterViewHolder : Java.Lang.Object
    {
        //Your adapter views to re-use
        public LinearLayout Layout { get; set; }
        public EditText TextBox { get; set; }
        public TextView TextView { get; set; }
        public ImageButton MoveUpButton { get; set; }
    }
}