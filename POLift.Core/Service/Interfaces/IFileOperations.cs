using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace POLift.Core.Service
{
    public interface IFileOperations
    {
        void Delete(string file_path);

        void Write(string file_path, Stream src_stream);

        Stream Read(string file_path);
    }
}
