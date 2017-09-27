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

namespace POLift
{
    class Exercise
    {
        public readonly string Name;
        public readonly int MaxRepCount, WeightIncrement;

        public Exercise(string name, int MaxRepCount, int WeightIncrement=5)
        {
            if(String.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("The exercise must have a name");
            }
            this.Name = name;

            if(MaxRepCount < 2)
            {
                throw new ArgumentException("Maximum rep count must be greater than minimum rep count");
            }
            this.MaxRepCount = MaxRepCount;

            if(WeightIncrement <= 0)
            {
                throw new ArgumentException("Weight increment must be at least 1");
            }
            this.WeightIncrement = WeightIncrement;
        }

        public override string ToString()
        {
            return $"{Name} for up to {MaxRepCount} reps";
        }

        public static Exercise FromXml(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            return FromXmlNode(doc.GetElementsByTagName("Exercise")[0]);
        }

        public static Exercise FromXmlNode(XmlNode node)
        {
            string name = node.Attributes["name"].InnerText;
            //int min_rep = Int32.Parse(node.Attributes["min_rep"].InnerText);
            int max_rep = Int32.Parse(node.Attributes["max_rep"].InnerText);
            int weight_increment = Int32.Parse(node.Attributes["weight_increment"].InnerText);
            return new Exercise(name, max_rep, weight_increment);
        }

        public XmlElement ToXmlElement()
        {
            XmlDocument doc = new XmlDocument();
            XmlElement exercise_element = doc.CreateElement("Exercise");

            exercise_element.SetAttribute("name", Name);
            //exercise_element.SetAttribute("min_rep", MinRepCount.ToString());
            exercise_element.SetAttribute("max_rep", MaxRepCount.ToString());
            exercise_element.SetAttribute("weight_increment", WeightIncrement.ToString());
            return exercise_element;
        }

        public string ToXml()
        {
            return ToXmlElement().OuterXml;
        }
    }
}