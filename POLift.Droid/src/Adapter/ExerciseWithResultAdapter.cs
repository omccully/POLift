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
    using Core.Service;

    class ExerciseWithResultAdapter : BaseAdapter<ExerciseWithResult>
    {
        public ObservableCollection<ExerciseWithResult> ExercisesWithResults { get; private set; }
        Context context;

        public ExerciseWithResultAdapter(Context context, IEnumerable<ExerciseWithResult> exercises_with_results)
        {
            this.context = context;
            ExercisesWithResults = new ObservableCollection<ExerciseWithResult>(exercises_with_results);
            ExercisesWithResults.CollectionChanged += ExercisesWithResults_CollectionChanged;
        }

        private void ExercisesWithResults_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NotifyDataSetChanged();
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return position;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override ExerciseWithResult this[int position]
        {
            get
            {
                return ExercisesWithResults[position];
            }
        }

        public int ExercisesCompleted
        {
            get
            {
                int i = 0;
                foreach(ExerciseWithResult ewr in ExercisesWithResults)
                {
                    if (ewr.Result == null) return i;
                    i++;
                }
                return i;
            }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            ExerciseWithResultViewHolder holder = new ExerciseWithResultViewHolder();
            var inflater = context.GetSystemService(Context.LayoutInflaterService).JavaCast<LayoutInflater>();

            View view = inflater.Inflate(Resource.Layout.PerformRoutineListItem, parent, false);
            holder.ExerciseName = view.FindViewById<TextView>(Resource.Id.ExerciseNameTextView);
            holder.RepCount = view.FindViewById<EditText>(Resource.Id.RepCountEditText);
            holder.Weight = view.FindViewById<EditText>(Resource.Id.WeightEditText);
           
            view.Tag = holder;

            ExerciseWithResult ewr = this[position];
            holder.ExerciseName.Text = ewr.Exercise.Name;

            int ec = ExercisesCompleted;
            if (ec == position) // active
            {
                holder.Weight.Enabled = true;
                holder.RepCount.Enabled = true;
            }
            else
            {
                if(ec > position)
                {
                    // completed
                    holder.Weight.Text = ewr.Result.Weight.ToString();
                    holder.RepCount.Text = ewr.Result.RepCount.ToString();
                }
                else
                {
                    // upcoming
                    holder.Weight.Text = ewr.ExpectedWeight.ToString();
                    holder.RepCount.Text = "";
                }

                holder.Weight.Enabled = false;
                holder.RepCount.Enabled = false;
            }


            return view;
        }


        //Fill in cound here, currently 0
        public override int Count
        {
            get
            {
                return ExercisesWithResults.Count;
            }
        }

    }

    class ExerciseWithResultViewHolder : Java.Lang.Object
    {
        //Your adapter views to re-use
        public TextView ExerciseName;
        public EditText RepCount;
        public EditText Weight;
    }
}