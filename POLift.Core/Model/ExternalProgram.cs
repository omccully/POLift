using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace POLift.Core.Model
{
    using Service;

    public class ExternalProgram
    {
        public static string FileUrl(string file)
        {
            return "http://crystalmathlabs.com/polift/programs/" + file;
        }

        public static string ProgramsListUrl = "http://crystalmathlabs.com/polift/programs_list.php";

        public string title;
        public string description;
        public string file;

        public static async Task<ExternalProgram[]> QueryProgramsList(string url = null)
        {
            string response = await Helpers.HttpQueryAsync(
                url == null ? ProgramsListUrl : url);

            return JsonConvert.DeserializeObject<ExternalProgram[]>(response);
        }
    }
}
