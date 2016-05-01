using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

namespace FileDuplicatesFinder
{
    static class FilesHelper
    {
        public static List<string> GetAllFilesFromDirectory_Recursively(string directoryPath)
        {
            var currDirectoryFilesPaths = Directory.GetFiles(directoryPath).ToList();

            foreach(var subDirPath in Directory.GetDirectories(directoryPath))
            {
                var subDirFiles = GetAllFilesFromDirectory_Recursively(subDirPath);

                currDirectoryFilesPaths.AddRange(subDirFiles);
            }

            return currDirectoryFilesPaths;
        }
    }
}
