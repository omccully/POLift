using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Views;
using POLift.Core.Model;
using POLift.Core.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POLift.Core.ViewModel
{
    public class SelectProgramToDownloadViewModel : ViewModelBase
    {
        private readonly INavigationService navigationService;
        private readonly IPOLDatabase Database;

        public IToaster Toaster;
        public DialogService DialogService;
        public IMainThreadInvoker MainThreadInvoker;
        public IFileOperations FileOperations;

        public event EventHandler ProgramsChanged;

        public bool Secure = true;

        public SelectProgramToDownloadViewModel(INavigationService navigationService,
            IPOLDatabase database)
        {
            this.navigationService = navigationService;
            this.Database = database;
            this.Programs = new ExternalProgram[0];
        }

        ExternalProgram[] _Programs = new ExternalProgram[0];
        public ExternalProgram[] Programs
        {
            get
            {
                return _Programs;
            }
            private set
            {
                _Programs = value;
                ProgramsChanged?.Invoke(this, new EventArgs());
            }
        }

        bool _IsLoading = false;
        public bool IsLoading
        {
            get
            {
                return _IsLoading;
            }
            set
            {
                Set(ref _IsLoading, value);
            }
        }

       TimeSpan WcfTimeout = TimeSpan.FromSeconds(12);
        public void RefreshProgramsList()
        {
            Programs = new ExternalProgram[] { new ExternalProgram() { title = "Loading..." } };

            System.Diagnostics.Debug.WriteLine("RefreshProgramsList()");
            var serviceClient = new POLiftCloudService.ServiceClient();
            serviceClient.Endpoint.Binding.OpenTimeout = WcfTimeout;
            serviceClient.Endpoint.Binding.SendTimeout = WcfTimeout;

            serviceClient.GetAllLiftingProgramsCompleted += (o, e) =>
            {
                System.Diagnostics.Debug.WriteLine("GetAllLiftingProgramsCompleted");
   
                serviceClient.CloseAsync();

                if (e.Error != null)
                {
                    Toaster?.DisplayError("Error getting programs list");
                    System.Diagnostics.Debug.WriteLine("Error getting programs list");
                    return;
                }

                System.Diagnostics.Debug.WriteLine("GetAllLiftingProgramsCompleted " + e.Result);

                Programs = e.Result.Select(lp => new ExternalProgram()
                {
                    title = lp.Title,
                    description = lp.Description,
                    file = lp.FileName
                }).ToArray();
            };

            System.Diagnostics.Debug.WriteLine("serviceClient.GetAllLiftingProgramsAsync()");
            serviceClient.GetAllLiftingProgramsAsync();
        }

        public void SelectExternalProgram(ExternalProgram program, string temp_dir = "", Action go_back_action = null)
        {
            if (program.file == null) return;

            System.Diagnostics.Debug.WriteLine("SelectExternalProgram");
            DialogService?.DisplayConfirmation("Are you sure you want to import the routines " +
                "and exercises for the " + program.title + " lifting program?",
                delegate
                {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    ImportProgramAsync(program, temp_dir, go_back_action);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                });
        }

        void ImportProgramAsync(ExternalProgram program, string temp_dir = "", Action go_back_action = null)
        {
            System.Diagnostics.Debug.WriteLine("ImportFromStream(" + program.file);

            var serviceClient = new POLiftCloudService.ServiceClient();
            serviceClient.Endpoint.Binding.SendTimeout = WcfTimeout;

            serviceClient.DownloadLiftingProgramCompleted += (o, e) =>
            {
                try
                {
                    if (e.Error != null) throw e.Error; // this is caught later on in the event

                    Service.Helpers.ImportFromStream(new MemoryStream(e.Result), Database, temp_dir, FileOperations, false);

                    if (go_back_action == null)
                    {
                        navigationService.GoBack();
                    }
                    else
                    {
                        go_back_action();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                    Toaster?.DisplayError("Error importing program: " + ex.Message);
                }

                serviceClient.CloseAsync();
            };

            serviceClient.DownloadLiftingProgramAsync(program.file);
                
            //await Service.Helpers.ImportFromUrlAsync(program.DownloadUrl, 
            //    Database, temp_dir, FileOperations, false);
        }
    }
}
