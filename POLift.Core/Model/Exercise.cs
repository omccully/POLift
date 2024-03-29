﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLite.Net.Attributes;

namespace POLift.Core.Model
{
    using Service;
    using System.Text.RegularExpressions;

    public class Exercise : IExercise, IIdentifiable, IDeletable
    {
        [Ignore]
        public IPOLDatabase Database { get; set; }

        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        string _Name;
        [Indexed(Name = "UniqueGroupExercise", Order =1, Unique = true)]
        public string Name
        {
            get
            {
                return _Name;
            } 
            set
            {
                if (String.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("The exercise must have a name");
                }
                this._Name = value.ToLower();
            }
        }

        int _MaxRepCount;
        [Indexed(Name = "UniqueGroupExercise", Order = 2, Unique = true)]
        public int MaxRepCount
        {
            get
            {
                return _MaxRepCount;
            }
            set
            {
                if (value < 2)
                {
                    throw new ArgumentException("Maximum rep count must be at least 2");
                }
                this._MaxRepCount = value;
            }
        }

        float _WeightIncrement;
        [Indexed(Name = "UniqueGroupExercise", Order = 3, Unique = true)]
        public float WeightIncrement
        {
            get
            {
                return _WeightIncrement;
            }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("Weight increment must be at least 1");
                }
                this._WeightIncrement = value;
            }
        }


        int _RestPeriodSeconds;
        [Indexed(Name = "UniqueGroupExercise", Order = 4, Unique = true)]
        public int RestPeriodSeconds
        {
            get
            {
                return _RestPeriodSeconds;
            }
            set
            {
                const int MAX_REST_PERIOD_MINUTES = 10;
                const int MAX_REST_PERIOD = MAX_REST_PERIOD_MINUTES * 60;
                if (value < 0 || value > MAX_REST_PERIOD)
                {
                    throw new ArgumentException($"Rest period must be within 0 seconds and {MAX_REST_PERIOD_MINUTES} minutes");
                }
                this._RestPeriodSeconds = value;
            }
        }

        
        int _ConsecutiveSetsForWeightIncrease;
        [Indexed(Name = "UniqueGroupExercise", Order = 5, Unique = true)]
        public int ConsecutiveSetsForWeightIncrease
        {
            get
            {
                return Math.Max(0, _ConsecutiveSetsForWeightIncrease);
            }
            set
            {
                _ConsecutiveSetsForWeightIncrease = Math.Max(0, value);
            }
        }

        public bool Deleted { get; set; } = false;

        public int PlateMathID
        {
            get
            {
                int index = Array.IndexOf(Service.PlateMath.PlateMathTypes, PlateMath);
                if (index == -1) return 0;
                return index;
            }
            set
            {
                PlateMath = Service.PlateMath.PlateMathTypes[value];
            }
        }

        string _Category;
        public string Category
        {
            get
            {
                return _Category;
            }
            set
            {
                if (value == null)
                {
                    _Category = null;
                }
                else
                {
                    _Category = Helpers.UniformString(value);
                }
            }
        }
        
        [Ignore]
        public IPlateMath PlateMath { get; set; }

        int _Usage = 1;
        public int Usage {
            get
            {
                //if (_Usage == 0)
                //{
                //    RefreshUsage();
                //}

                return _Usage;
            }
            set
            {
                _Usage = value;
            }
        }

        public Exercise() : this("Generic exercise", 6)
        {

        }

        public Exercise(string Name, int MaxRepCount, float WeightIncrement=5.0f, 
            int RestPeriodSeconds=120, IPlateMath plate_math = null)
        {
            this.Name = Name;
            this.MaxRepCount = MaxRepCount;
            this.WeightIncrement = WeightIncrement;
            this.RestPeriodSeconds = RestPeriodSeconds;
            this.PlateMath = plate_math;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(ShortDetails);
           
            if(PlateMath != null)
            {
                sb.AppendLine();
                sb.Append(PlateMath);
            }
            //sb.Append($" (ID {ID})");

            return sb.ToString();
        }

        public string ShortDetails
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(Name);
                sb.Append($"{MaxRepCount} reps");
                if (ConsecutiveSetsForWeightIncrease != 0)
                {
                    if(ConsecutiveSetsForWeightIncrease > 1)
                    {
                        sb.Append($" for {ConsecutiveSetsForWeightIncrease} sets in a row");
                    }

                    sb.Append(" to advance");
                }
                else
                {
                    sb.Append(", manual progression");
                }

                //sb.Append($", {RestPeriodSeconds} sec rest");

                sb.Append($", " + RestPeriodSeconds.SecondsToClock() + " rest");

                //sb.Append($", {Usage} usage");

                return sb.ToString();
            }
        }

        public string CondensedDetails
        {
            get
            {
                string group = $"{MaxRepCount}r; " +
                    $"{ConsecutiveSetsForWeightIncrease}cs; " +
                    $"{RestPeriodSeconds.SecondsToClock()}";

                /*Regex reg = new Regex(".(?!$)");
                group = reg.Replace(group, "$0\u200b");

                foreach(char c in group)
                {
                    System.Diagnostics.Debug.WriteLine($"char = {c} ({(int)c})");
                }*/

                return $"{Name}\n{group}";
            }
        }

        [Ignore]
        public float NextWeight
        {
            get
            {
                IEnumerable<ExerciseResult> eresults =
                    ExerciseResult.LastNOfExercise(Database, this,
                    ConsecutiveSetsForWeightIncrease);

                ExerciseResult most_recent_result = eresults.FirstOrDefault();
                if (most_recent_result == null)
                {
                    if(PlateMath != null && PlateMath.BarWeight != 0)
                    {
                        return PlateMath.BarWeight;
                    }
                    return WeightIncrement;
                }

                float most_recent_weight = most_recent_result.Weight;

                if (ConsecutiveSetsForWeightIncrease == 1)
                {
                    if (most_recent_result.RepCount >= MaxRepCount)
                    {
                        return most_recent_weight + WeightIncrement;
                    }
                }
                else if (ConsecutiveSetsForWeightIncrease > 1 &&
                    eresults.Count() == ConsecutiveSetsForWeightIncrease &&
                    eresults.All(er => er.RepCount >= MaxRepCount) &&
                    eresults.All(er => er.Weight >= most_recent_weight))
                {
                    return most_recent_weight + WeightIncrement;
                }
                
                return most_recent_weight;
            }
        }

        [Ignore]
        public string ExerciseIDs
        {
            get => this.ID.ToString();
            set => throw new NotImplementedException("Exercise.ExerciseIDs.Set not implemented");
        }

        int CalculateUsage()
        {
            return Database.Table<ExerciseResult>()
                .Where(er => er.ExerciseID == this.ID)
                .Count() + 1;
        }

        public bool RefreshUsage()
        {
            int old_usage = _Usage;
            _Usage = CalculateUsage();
            return old_usage != _Usage;
        }

        public static void RefreshAllUsages(IPOLDatabase database)
        {
            IEnumerable<Exercise> exercises =
                database.Table<Exercise>().Where(e => e.Usage == 0);

            foreach(Exercise exercise in exercises)
            {
                exercise.RefreshUsage();
                database.Update(exercise);
            }
        }

        public int SucceedsInARow(int check_count = 0)
        {
            if (check_count == 0) check_count = ConsecutiveSetsForWeightIncrease;

            IEnumerable<ExerciseResult> eresults =
                    ExerciseResult.LastNOfExercise(Database, this,
                    check_count);
            if (eresults.Count() == 0) return 0;

            float most_recent_weight = eresults.ElementAt(0).Weight;

            int count = 0;
            foreach(ExerciseResult exr in eresults)
            {
                if(exr.IsSuccess(most_recent_weight, this))
                {
                    count++;
                }
                else
                {
                    return count;   
                }
            }

            return count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>null if no ExerciseDifficulty in the db</returns>
        public ExerciseDifficulty GetDifficultyRecord()
        {
            ExerciseDifficulty ed = Database.Query<ExerciseDifficulty>(
                "SELECT * FROM ExerciseDifficulty WHERE Name = ? AND RestPeriodSeconds = ?",
                Name, RestPeriodSeconds).FirstOrDefault();
            if (ed != null) ed.Database = Database;
            return ed;
        }

        public override bool Equals(object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Routine return false.
            Exercise r = obj as Exercise;

            return this.Equals(r);
        }

        public bool Equals(Exercise r)
        {
            if ((System.Object)r == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (this.Name == r.Name &&
                this.MaxRepCount == r.MaxRepCount && 
                this.WeightIncrement == r.WeightIncrement &&
                this.RestPeriodSeconds == r.RestPeriodSeconds &&
                this.ConsecutiveSetsForWeightIncrease == r.ConsecutiveSetsForWeightIncrease);
        }

        public static bool operator ==(Exercise a, Exercise b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            // Return true if the fields match:
            return a.Equals(b);
        }

        public static bool operator !=(Exercise a, Exercise b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode() ^ this.MaxRepCount.GetHashCode() ^
                this.WeightIncrement.GetHashCode() ^ this.RestPeriodSeconds.GetHashCode();
        }

        public static Dictionary<int,int> Import(IEnumerable<Exercise> exercises, IPOLDatabase destination)
        {
            // loop through all exercises in imported db, add to 
            // existing DB

            Dictionary<int, int> ExercisesLookup = new Dictionary<int, int>();
            foreach (Exercise exercise in exercises)
            {
                int old_id = exercise.ID;
                exercise.ID = 0;
                destination.InsertOrUpdateNoID(exercise);

                ExercisesLookup[old_id] = exercise.ID;
            }

            return ExercisesLookup;
        }

        public static Dictionary<int, int> PruneByConstaints(IPOLDatabase dab)
        {
            Dictionary<int, int> ExerciseMapping = new Dictionary<int, int>();
            HashSet<Exercise> existing_exercises = new HashSet<Exercise>();

            foreach (Exercise exercise in dab.Table<Exercise>())
            {
                if (existing_exercises.Contains(exercise))
                {

                    throw new NotImplementedException();


                    /*Exercise original;
                    if (existing_exercises.TryGetValue(exercise, out original))
                    {
                        // is a duplicate.
                        if (!exercise.Deleted)
                        {
                            // undelete original if the duplicate was undeleted

                            if (original.Deleted)
                            {
                                original.Deleted = false;
                                dab.Update((Exercise)original);
                            }
                        }

                        ExerciseMapping[exercise.ID] = original.ID;

                        // delete the duplicate
                        dab.Delete<Exercise>(exercise.ID);
                    }
                    else
                    {
                        throw new Exception("Prune error");
                    }*/
                }
            }

            return ExerciseMapping;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns>Ordered list of KVP category => list of exercises</returns>
        public static List<KeyValuePair<string,List<IExercise>>> InCategories(
            IPOLDatabase Database, string DefaultCategory = "other", string DeletedCategory = "DELETED")
        {
            Dictionary<string, List<IExercise>> dict = new Dictionary<string, List<IExercise>>();

            foreach (Exercise ex in Database
                .Table<Exercise>()
                .OrderByDescending(ex => ex.Usage))
            {
                string cat = ex.Deleted ? DeletedCategory :
                    (ex.Category == null ? DefaultCategory : ex.Category);

                if (dict.ContainsKey(cat))
                {
                    dict[cat].Add(ex);
                }
                else
                {
                    dict[cat] = new List<IExercise>() { ex };
                }
            }

            //POLDatabase.Table<Exercise>()
            //    .GroupBy(ex => ex.Category)
            //    .OrderByDescending(group => group.Count());


            return dict.OrderByDescending(kvp =>
                // put deleted category in back
                kvp.Key == DeletedCategory ? 0 : kvp.Value.Count
            ).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Ordered list of KVP category => list of exercises</returns>
        public static List<ExerciseCategory> InExCategories(
            IPOLDatabase Database, string DefaultCategory = "other", string DeletedCategory = "DELETED")
        {
            return InCategories(Database, DefaultCategory, DeletedCategory)
                .Select(kvp => new ExerciseCategory(kvp.Key, kvp.Value)).ToList();
        }
    }
}