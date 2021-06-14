using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CSharpSnackisApp.Models.Toolbox
{
    public static class FileDelete
    {
        public static void DeleteImage(string path)
        {
            if(!string.IsNullOrEmpty(path))
            {
                var file = Directory.GetFiles("./wwwroot/img/").Select(x => path).FirstOrDefault();
                File.Delete($"./wwwroot/img/{file}");
            }
        }
    }
}
