using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using Dropbox.Api;
using Dropbox.Api.Files;    

namespace Dropbox_Example
{
    class Test_Dropbox_Helper
    {
        static void Main(string[] args)
        {
            DropboxHelper helper = new DropboxHelper();

            // method 1 : Create a file named "abcd.txt" in dropbox with the content (given in last paramter)
            var task = Task.Run(() => helper.Upload("", "abcd.txt", "File created programmatically!"));
            task.Wait();

            // method 2 : Create a file in dropbox as "uploaded.txt" with the same content as the content in the your local file  => D:\x.txt (1st argument)
            task = Task.Run(() => helper.SaveFileToDropBox(@"D:\x.txt", "uploaded.txt"));
            task.Wait();

            // create a folder named xyz in dropbox
            task = Task.Run(() => helper.CreateFolder("xyz"));
            task.Wait();

            // download the file named "abcd.txt" to D:\y.txt 
            task = Task.Run(() => helper.Download("", "abcd.txt", @"D:\y.txt"));
            task.Wait();
            
            Console.ReadLine();
        }
    }
    class DropboxHelper
    {
        static string ACCESS_TOKEN = "LDnssKMNJmAAAAAAAAAACdZuYd_M7AGf0CqODYUvFVFwkyuOdn4zAOHQZa834eXo";
        public static DropboxClient dropbox_client = new DropboxClient(ACCESS_TOKEN);

        public async Task ListRootFolder()
        {
            var list = await dropbox_client.Files.ListFolderAsync(string.Empty);
            foreach (var item in list.Entries.Where(i => i.IsFolder))
                Console.WriteLine("D  {0}/", item.Name);
            foreach (var item in list.Entries.Where(i => i.IsFile))
                Console.WriteLine("F{0,8} {1}", item.AsFile.Size, item.Name);
        }

        /*
         * Argument 1 : folder name in dropbox
         * Argument 2 : file name in dropbox
         * Argument 3 : content of file to be uploaded
         */
        public async Task Upload(string folder, string file, string content)
        {
            using (var mem = new MemoryStream(Encoding.UTF8.GetBytes(content)))
            {
                var updated = await dropbox_client.Files.UploadAsync(folder + "/" + file, WriteMode.Overwrite.Instance, body: mem);
                Console.WriteLine("Saved {0}/{1} rev {2}", folder, file, updated.Rev);
            }
        }

        /*
         * Argument 1 : Name of the folder to be created in dropbox (it will be created in root folder)
         */
        public async Task CreateFolder(string foldername)
        {
            var list = await dropbox_client.Files.ListFolderAsync(string.Empty);
            foreach (var item in list.Entries.Where(i => i.IsFolder))
            {
                if (item.Name.Equals(foldername))
                {
                    Console.WriteLine("Error: folder with name {0} already exists!",foldername);
                    return;
                }
            }
            Dropbox.Api.Files.CreateFolderArg folderArg = new CreateFolderArg("/"+foldername);
            await dropbox_client.Files.CreateFolderAsync(folderArg);
        }

        /*
         * Argument 1 : full path of the file you want to upload to dropbox (example: D:\folder\file.txt) 
         * Argument 2 : name of the uploaded file in dropbox (example: sample.txt)
         */ 
        public async Task SaveFileToDropBox(string filename,string dropboxFileName)
        {
            string file_data = GetFileContents(filename);
            await Upload("", dropboxFileName, file_data);
        }

        /*
         * Argument 1 : folder name in dropbox 
         * Argument 2 : name of file to be downloaded from dropbox 
         * Argument 3 : where the downloaded file should be saved
         */
        public async Task Download(string folder, string file, string local_path)
        {
            System.IO.StreamWriter writer = new StreamWriter(local_path);
            using (var response = await dropbox_client.Files.DownloadAsync(folder + "/" + file))
            {
                string file_Data = await response.GetContentAsStringAsync();
                Console.WriteLine(file_Data);
                writer.Write(file_Data);
            }
            writer.Close();
            Console.WriteLine("File downloaded to '{0}'", local_path);
        }

        private static string GetFileContents(string filename)
        {
            System.IO.StreamReader reader = new StreamReader(filename);
            String file_data = "", line;
            while ((line = reader.ReadLine()) != null)
            {
                file_data += line;
            }
            reader.Close();
            return file_data;
        }
    }
}
