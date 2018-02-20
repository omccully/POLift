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
                //Service.Helpers.HttpQueryAsync("https://apple.com");

                //Programs = await ExternalProgram.QueryProgramsList(
                //    "https://crystalmathlabs.com/polift/programs_list.php");
                Programs = await ExternalProgram.QueryProgramsList();

                

            }
            catch
            {
                Toaster?.DisplayError("Error getting programs list");
            }
        }

        public void SelectExternalProgram(ExternalProgram program, string temp_dir = "")
        {
            System.Diagnostics.Debug.WriteLine("SelectExternalProgram");
            DialogService?.DisplayConfirmation("Are you sure you want to import the routines " +
                "and exercises for the " + program.title + " lifting program?",
                delegate
                {
                    ImportProgramAsync(program, temp_dir);
                    /*MainThreadInvoker.Invoke(delegate
                    {
                        Toaster.DisplayMessage("Lifting program finished downloading");
                    });*/
                });
        }

        async Task ImportProgramAsync(ExternalProgram program, string temp_dir = "")
        {
            try
            {
                string url = ProgramToUrl(program);
                System.Diagnostics.Debug.WriteLine("ImportFromUrlAsync(" + url);
                
                await Service.Helpers.ImportFromUrlAsync(url, Database, temp_dir, FileOperations, false);

                navigationService.GoBack();
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                Toaster?.DisplayError("Error importing program");
            }

        }

        string ProgramToUrl(ExternalProgram program)
        {
            return "https://crystalmathlabs.com/polift/programs/" + program.file;
        }


    }
}
