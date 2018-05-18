using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Web;

namespace WatermarkKhue.Models
{

    public class GoogleDriveFilesRepository
    {
        public static string[] Scopes = { DriveService.Scope.Drive };

         //Tạo Drive API service.
        public static DriveService GetService()
        {
             //Lấy Credentials từ file client_secret.json 
            UserCredential credential;
            using (var stream = new FileStream(@"D:\GIAU_TIN_CUOI_KY\client_secret.json", FileMode.Open, FileAccess.Read))
            {
                String FolderPath = @"D:\GIAU_TIN_CUOI_KY";
                String FilePath = Path.Combine(FolderPath, "DriveServiceCredentials.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(FilePath, true)).Result;
            }

            //Bước tạo Drive API service.
            DriveService service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "WatermarkKhue",
            });
            return service;
        }

        //Lấy tất cả các file từ Google Drive
        public static List<GoogleDriveFiles> GetDriveFiles()
        {
            DriveService service = GetService();
            
            //Xác định các tham số yêu cầu của file
            FilesResource.ListRequest FileListRequest = service.Files.List();
            FileListRequest.Fields = "nextPageToken, files(id, name, size, version, createdTime)";

            //Lấy list các file
            IList<Google.Apis.Drive.v3.Data.File> files = FileListRequest.Execute().Files;
            List<GoogleDriveFiles> FileList = new List<GoogleDriveFiles>();

            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    GoogleDriveFiles File = new GoogleDriveFiles {
                        Id = file.Id,
                        Name = file.Name,
                        Size = file.Size,
                        Version = file.Version,
                        CreatedTime = file.CreatedTime
                    };
                    FileList.Add(File);
                }
            }
            return FileList;
        }

        //Kiểm tra Bản Quyền Số của một file .wav bất kỳ từ máy
        public static string Check(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                Google.Apis.Drive.v3.DriveService service = GetService();

                string path = Path.Combine(HttpContext.Current.Server.MapPath("~"),
                Path.GetFileName(file.FileName));
                file.SaveAs(path);

                String signature = "";
                WaveSteg filess = new WaveSteg(new FileStream(path, FileMode.Open, FileAccess.Read));
                StagnoHelper sh = new StagnoHelper(filess);
                signature = sh.ExtractMessage();

                return signature;

            }
            return "";
        }

        //Download file từ Google Drive 
        public static string DownloadGoogleFile(string fileId)
        {
            DriveService service = GetService();

            string FolderPath = System.Web.HttpContext.Current.Server.MapPath("/GoogleDriveFiles/");
            FilesResource.GetRequest request = service.Files.Get(fileId);

            string FileName = request.Execute().Name;
            string FilePath = System.IO.Path.Combine(FolderPath, FileName);

            MemoryStream stream1 = new MemoryStream();
            request.Download(stream1);
            return FilePath;
        }

        //Lưu file vào PathServer
        private static void SaveStream(MemoryStream stream, string FilePath)
        {
            using (System.IO.FileStream file = new FileStream(FilePath, FileMode.Create, FileAccess.ReadWrite))
            {
                stream.WriteTo(file);
            }
        }

    }
}