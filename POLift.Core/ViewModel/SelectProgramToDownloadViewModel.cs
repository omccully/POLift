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

        public SelectProgramToDownloadViewModel(INavigationService navigationService,
            IPOLDatabase database)
        {
            this.navigationService = navigationService;
            this.Database = database;
            this.Programs = new ExternalProgram[0];
        }

        ExternalProgram[] _Programs;
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
                // TODO: notify view of programs list changed
                Programs = await ExternalProgram.QueryProgramsList();
            }
            catch
            {
                Toaster?.DisplayError("Error getting programs list");
            }
        }

        public void SelectExternalProgram(ExternalProgram program)
        {
            DialogService?.DisplayConfirmation("Are you sure you want to import the routines " +
                "and exercises for the " + program.title + " lifting program?",
                delegate
                {
                    ImportProgramAsync(program);
                });
        }

        async Task ImportProgramAsync(ExternalProgram program)
        {
            try
            {
                string url = ProgramToUrl(program);

                await Service.Helpers.ImportFromUrlAsync(url, Database, "", FileOperations, false);

                navigationService.GoBack();
            }
            catch
            {
                Toaster?.DisplayError("Error importing program");
            }

        }

        string ProgramToUrl(ExternalProgram program)
        {
            return "http://crystalmathlabs.com/polift/programs/" + program.file;
        }


    }
}
