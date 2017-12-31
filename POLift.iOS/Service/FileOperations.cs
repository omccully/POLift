using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Foundation;
using UIKit;

using POLift.Core.Service;


namespace POLift.iOS.Service
{
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