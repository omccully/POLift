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
using Android.Text;
using Android.Text.Method;
using Java.Lang;

namespace POLift.Droid.Service
{
    /* class TimeSpanKeyListener : NumberKeyListener
     {
         char[] TimeSpanAcceptedChars = new char[]
         {
             '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ':', '.'
         };

         /*public TimeSpanKeyListener()
         {
             char[] base_accepted_chars = base.GetAcceptedChars();
             TimeSpanAcceptedChars = new char[base_accepted_chars.Length + 1];
             Array.Copy(base_accepted_chars, TimeSpanAcceptedChars, base_accepted_chars.Length);
             TimeSpanAcceptedChars[base_accepted_chars.Length] = ':';
         }

         protected override char[] GetAcceptedChars()
         {
             System.Diagnostics.Debug.WriteLine("GetAcceptedChars");
             return TimeSpanAcceptedChars;
         }

         public override ICharSequence FilterFormatted(ICharSequence source, int start, int end, ISpanned dest, int dstart, int dend)
         {
             System.Diagnostics.Debug.WriteLine($"FilterFormatted(\"{source}\",{start},{end},\"{dest}\",{dstart},{dend})");
             ICharSequence base_source = base.FilterFormatted(source, start, end, dest, dstart, dend);
             ICharSequence prefiltered_source = base_source != null ? base_source : source;

             var result = new Java.Lang.String(prefiltered_source.ToString().Replace('.', ':'));
             System.Diagnostics.Debug.WriteLine("result = \"" + result + "\"");
             return result;

         }

         public override InputTypes InputType => InputTypes.ClassNumber;
     }*/

    /* public class TimeSpanKeyListener : IKeyListener
     {
         InputTypes InputType { get; private set; } = InputTypes.ClassNumber;


     }
     */
    public class TimeSpanInputFilter : Java.Lang.Object, IInputFilter
    {
        // public IntPtr Handle { get; set; }

        public ICharSequence FilterFormatted(ICharSequence source, int start, int end, ISpanned dest, int dstart, int dend)
        {
            System.Diagnostics.Debug.WriteLine($"FilterFormatted(\"{source}\",{start},{end},\"{dest}\",{dstart},{dend})");

            bool contains_colon = dest.Count(c => c == ':') > 0;

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (char c in source)
            {
                if (char.IsDigit(c))
                {
                    sb.Append(c);
                }
                else if (!contains_colon)
                {
                    sb.Append(':');
                }
            }

            System.Diagnostics.Debug.WriteLine("result = " + sb.ToString());


            /*var result = new Java.Lang.String(source.Select(c => {
                if (c == ':') colon_seen = true;
                return char.IsDigit(c) ? c : ':';
            }).ToArray());
            System.Diagnostics.Debug.WriteLine("result = " + result);*/
            return new Java.Lang.String(sb.ToString());
        }

        public void Dispose()
        {
            //Android.Text.Input
        }
    }

    class TimeSpanInputMethodService : Android.InputMethodServices.InputMethodService
    {

    }
}
