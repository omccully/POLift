using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using SQLite;

namespace POLift.Model
{
    using Service;

    class Routine : IIdentifiable, IDeletable
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        string _Name;
        [Indexed(Name = "UniqueGroup", Order = 1, Unique = true)]
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
                    throw new ArgumentException("The routine must have a name");
                }
                this._Name = value;
            }
        }


         [Ignore]
         public IEnumerable<ExerciseSets> ExerciseSets
         {
             get
             {
                // read exercises from DB based on the IDs
                if(ExerciseSetIDs == null)
                {
                    return new List<ExerciseSets>();
                }

                return ExerciseSetIDs.Split(',').Select(id_str =>
                {
                    try
                    {
                        int id = Int32.Parse(id_str);
                        return POLDatabase.ReadByID<ExerciseSets>(id);
                    }
                    catch (FormatException)
                    {
                        return null;
                    }
                }).Where(e => e != null);
            }
             set
             {
                ExerciseSetIDs = String.Join(",", value.Select(e => e.ID).ToArray());
            }
         }

        string _ExerciseSetIDs;
        [Indexed(Name = "UniqueGroup", Order = 2, Unique = true)]
        public string ExerciseSetIDs
        {
            get
            {
                return _ExerciseSetIDs;
            }
            set
            {
                _ExerciseSetIDs = value;
            }
        }

        public bool Deleted { get; set; } = false;

        public Routine(string Name, string exercise_set_ids)
        {
            this.Name = Name;
            ExerciseSetIDs = exercise_set_ids;
        }

        public Routine(string Name, IEnumerable<ExerciseSets> exercise_sets)
        {
            this.Name = Name;
            ExerciseSets = exercise_sets;
        }

        public Routine() : this("Generic routine", "")
        {

        }

        public override string ToString()
        {
            int exercises = ExerciseSets.Count();
            int total_sets = ExerciseSets.Sum(e => e.SetCount);

            return $"{Name}, {exercises} exercises, {total_sets} sets (ID {ID})";
        }

        [Ignore]
        public List<Exercise> Exercises
        {
            get
            {
                List<Exercise> results = new List<Exercise>();

                foreach (ExerciseSets sets in this.ExerciseSets)
                {
                    Exercise ex = sets.Exercise;

                    for(int i = 0; i < sets.SetCount; i++)
                    {
                        results.Add(ex);
                    }
                }

                return results;
            }
        }


        public override bool Equals(object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Routine return false.
            Routine r = obj as Routine;

            return this.Equals(r);
        }

        public bool Equals(Routine r)
        {
            if ((System.Object)r == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (this.ExerciseSetIDs == r.ExerciseSetIDs &&
                this.Name == r.Name);
        }

        public static bool operator==(Routine a, Routine b)
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

        public static bool operator !=(Routine a, Routine b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return this.ExerciseSetIDs.GetHashCode() ^ this.Name.GetHashCode();
        }



        /*public static Routine FromXml(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            return FromXmlNode(doc.GetElementsByTagName("Routine")[0]);
        }

        public static Routine FromXmlNode(XmlNode node)
        {
            string name = node.Attributes["name"].InnerText;
            List<Exercise> exercises = new List<Exercise>();
            foreach (XmlNode exercise_node in node.ChildNodes)
            {
                exercises.Add(Exercise.FromXmlNode(exercise_node));
            }

            return new Routine(name, exercises);
        }

        public XmlElement ToXmlElement()
        {
            XmlDocument doc = new XmlDocument();
            XmlElement routine_element = doc.CreateElement("Routine");

            routine_element.SetAttribute("name", Name);

            foreach(Exercise ex in Exercises)
            {
                XmlElement xml_element = ex.ToXmlElement();

                routine_element.InnerXml += xml_element.OuterXml;
            }

            return routine_element;
            //doc.AppendChild(exercise_element);
            //return doc.OuterXml;
        }

        public string ToXml()
        {
            return ToXmlElement().OuterXml;
        }*/
    }
}