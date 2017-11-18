using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Util;

using Newtonsoft.Json;

using Microsoft.Practices.Unity;

namespace POLift
{
    using Service;

    [Activity(Label = "SelectProgramToDownloadActivity")]
    public class SelectProgramToDownloadActivity : ListActivity
    {
        ExternalProgram[] Programs = new ExternalProgram[0];

        IPOLDatabase Database;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            Database = C.ontainer.Resolve<IPOLDatabase>();
            ListView.ItemClick += ListView_ItemClick;

            QueryProgramsList();
        }

        

        async Task QueryProgramsList()
        {
            try
            {
                string url = "http://crystalmathlabs.com/polift/programs_list.php";
                string response = await Helpers.HttpQueryAsync(url);

                Log.Debug("POLift", response);

                this.Programs =
                    JsonConvert.DeserializeObject<ExternalProgram[]>(response);
                Log.Debug("POLift", "programs deserialized");
                string[] titles = this.Programs.Select(p => p.title).ToArray();

                ArrayAdapter<string> adp = new ArrayAdapter<string>(this,
                    Resource.Layout.ProgramItem, titles);

                this.RunOnUiThread(delegate
                {
                    this.ListAdapter = adp;
                });
                
                Log.Debug("POLift", "programs list adapter set");
            }
            catch(Exception e)
            {
                Toast.MakeText(this, "Error getting programs list", ToastLength.Long).Show();
                Log.Debug("POLift", "Error getting programs list: " + e);
            }
        }

        private void ListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            ExternalProgram program = Programs[e.Position];

            Helpers.DisplayConfirmation(this, "Are you sure you want to import the routines" +
                $" and exercises for the \"{program.title}\" lifting program?",
                delegate
                {
                    ImportProgram(program);
                });
        }

        void ImportProgram(ExternalProgram program)
        {
            try
            {
                string url = "http://crystalmathlabs.com/polift/programs/" + program.file;

                Log.Debug("POLift", $"Selected program: {program.title}, {program.description}, {program.file}");
                //Helpers.ImportFromUri(Android.Net.Uri.Parse(url), Database, this.ContentResolver, FilesDir.Path, false);

                Helpers.ImportFromUrl(url, Database, FilesDir.Path, false);

                SetResult(Result.Ok);
                Finish();
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, "Error importing program", ToastLength.Long).Show();
                Log.Debug("POLift", "Error importing program: " + ex);
            }
        }

        class ExternalProgram
        {
            public string title;
            public string description;
            public string file;
        }
    }
}