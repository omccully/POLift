﻿using System;
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

    public class Exercise : IIdentifiable, IDeletable
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        string _Name;
        [Indexed(Name = "UniqueGroup", Order =1, Unique = true)]
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
                this._Name = value;
            }
        }

        int _MaxRepCount;
        [Indexed(Name = "UniqueGroup", Order = 2, Unique = true)]
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

        int _WeightIncrement;
        [Indexed(Name = "UniqueGroup", Order = 3, Unique = true)]
        public int WeightIncrement
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
        [Indexed(Name = "UniqueGroup", Order = 4, Unique = true)]
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

        public bool Deleted { get; set; } = false;

        public Exercise() : this("Generic exercise", 6)
        {

        }

        public Exercise(string Name, int MaxRepCount, int WeightIncrement=5, int RestPeriodSeconds=120)
        {
            this.Name = Name;
            this.MaxRepCount = MaxRepCount;
            this.WeightIncrement = WeightIncrement;
            this.RestPeriodSeconds = RestPeriodSeconds;
        }

        public override string ToString()
        {
            return $"{Name} for up to {MaxRepCount} reps, {RestPeriodSeconds} second rest (ID {ID})";
        }

        [Ignore]
        public int NextWeight
        {
            get
            {
                ExerciseResult most_recent_result = 
                    ExerciseResult.MostRecentResultOf(this);
                if (most_recent_result == null) return WeightIncrement;

                int weight = most_recent_result.Weight;

                if(most_recent_result.RepCount >= MaxRepCount)
                {
                    weight += WeightIncrement;
                }

                return weight;
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
                this.RestPeriodSeconds == r.RestPeriodSeconds);
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


        /*public static Exercise FromXml(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            return FromXmlNode(doc.GetElementsByTagName("Exercise")[0]);
        }

        public static Exercise FromXmlNode(XmlNode node)
        {
            string name = node.Attributes["name"].InnerText;
            int max_rep = Int32.Parse(node.Attributes["max_rep"].InnerText);
            int weight_increment = Int32.Parse(node.Attributes["weight_increment"].InnerText);
            return new Exercise(name, max_rep, weight_increment);
        }

        public XmlElement ToXmlElement()
        {
            XmlDocument doc = new XmlDocument();
            XmlElement exercise_element = doc.CreateElement("Exercise");

            exercise_element.SetAttribute("name", Name);
            exercise_element.SetAttribute("max_rep", MaxRepCount.ToString());
            exercise_element.SetAttribute("weight_increment", WeightIncrement.ToString());
            return exercise_element;
        }

        public string ToXml()
        {
            return ToXmlElement().OuterXml;
        }*/
    }
}