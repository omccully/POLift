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
    using Core.ViewModel;

    [Activity(Label = "SelectProgramToDownloadActivity")]
    public class SelectProgramToDownloadActivity : ListActivity
    {
        private SelectProgramToDownloadViewModel Vm
        {
            get
            {
                return ViewModelLocator.Default.SelectProgramToDownload;
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            ListView.ItemClick += ListView_ItemClick;

            Vm.DialogService = new DialogService(
                new DialogBuilderFactory(this),
                ViewModelLocator.Default.KeyValueStorage);

            Vm.Toaster = new Toaster(this);

            //SetProgramsListView();

            Vm.ProgramsChanged += Vm_ProgramsChanged;
            Vm.RefreshProgramsList();

            //RefreshProgramsList();
        }

        private void Vm_ProgramsChanged(object sender, EventArgs e)
        {
            SetProgramsListView();
        }

        void SetProgramsListView()
        {
            string[] titles = Vm.Programs
                .Select(p => p.title).ToArray();
            ArrayAdapter<string> adp = new ArrayAdapter<string>(this,
                    Resource.Layout.ProgramItem, titles);

            this.RunOnUiThread(delegate
            {
                this.ListAdapter = adp;
            });
        }

        private void ListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            Vm.SelectExternalProgram(Vm.Programs[e.Position], FilesDir.Path,
                delegate
                {
                    RunOnUiThread(delegate
                    {
                        SetResult(Result.Ok);
                        Finish();
                    });
                });
        }

        protected override void OnDestroy()
        {
            Vm.ProgramsChanged -= Vm_ProgramsChanged;

            base.OnDestroy();
        }
    }
}