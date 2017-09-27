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
    class Routine
    {
        public readonly string Name;

        Exercise[] _exercises;

        public Routine(string Name, IEnumerable<Exercise> exercises)
        {
            this.Name = Name;
            Exercises = exercises;
        }

        public IEnumerable<Exercise> Exercises
        {
            get
            {
                // return copy of exercises array
                return _exercises.ToArray();
            }
            set
            {
                _exercises = value.ToArray();
            }
        }

        public override string ToString()
        {
            return $"{Name}, {_exercises.Length} exercises";
        }

        public static Routine FromXml(string xml)
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
        }
    }
}