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

    class Exercise : IIdentifiable
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        string _Name;
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

        public bool Deleted = false;

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