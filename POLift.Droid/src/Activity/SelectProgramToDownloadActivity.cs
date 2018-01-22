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

namespace POLift.Droid
{
    using Service;
    using Core.Service;
    using Core.Model;

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

            RefreshProgramsList();
        }

        async Task RefreshProgramsList()
        {
            try
            {
                this.Programs = await ExternalProgram.QueryProgramsList();
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

            AndroidHelpers.DisplayConfirmation(this, "Are you sure you want to import the routines" +
                $" and exercises for the \"{program.title}\" lifting program?",
                delegate
                {
                    ImportProgramAsync(program);
                });
        }

        async Task ImportProgramAsync(ExternalProgram program)
        {
            try
            {
                string url = ExternalProgram.FileUrl(program.file);

                Log.Debug("POLift", $"Selected program: {program.title}, {program.description}, {program.file}");
                //Helpers.ImportFromUri(Android.Net.Uri.Parse(url), Database, this.ContentResolver, FilesDir.Path, false);

                await Helpers.ImportFromUrlAsync(url, Database, FilesDir.Path, new FileOperations(), false);

                RunOnUiThread(delegate
                {
                    SetResult(Result.Ok);
                    Finish();
                });
            }
            catch (Exception ex)
            {
                RunOnUiThread(delegate
                {
                    Toast.MakeText(this, "Error importing program", ToastLength.Long).Show();
                });
                
                Log.Debug("POLift", "Error importing program: " + ex);
            }
        }
    }
}