using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Views;
using POLift.Core.Model;
using POLift.Core.Service;
using System;
using System.Collections.Generic;
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


        public async Task RefreshProgramsList()
        {
            try
            {
                Programs = await ExternalProgram.QueryProgramsList();
            }
            catch
            {
                Toaster?.DisplayError("Error getting programs list");
            }
        }

        public void SelectExternalProgram(ExternalProgram program, string temp_dir = "", Action go_back_action = null)
        {
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

        async Task ImportProgramAsync(ExternalProgram program, string temp_dir = "", Action go_back_action = null)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("ImportFromUrlAsync(" + program.DownloadUrl);
                
                await Service.Helpers.ImportFromUrlAsync(program.DownloadUrl, 
                    Database, temp_dir, FileOperations, false);

                if(go_back_action == null)
                {
                    navigationService.GoBack();
                }
                else
                {
                    go_back_action();
                }
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                Toaster?.DisplayError("Error importing program: " + e.Message);
            }
        }
    }
}
