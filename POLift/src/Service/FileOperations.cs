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

using System.IO;

namespace POLift.Service
{
    using POLift.Core.Service;

    class FileOperations : IFileOperations
    {
        public void Delete(string file_path)
        {
            File.Delete(file_path);
        }

        public void Write(string file_path, Stream src_stream)
        {
            using (FileStream file_write_stream = File.Create(file_path))
            {
                src_stream.CopyTo(file_write_stream);
            }
        }

        public Stream Read(string file_path)
        {
            return File.OpenRead(file_path);
        }
    }
}