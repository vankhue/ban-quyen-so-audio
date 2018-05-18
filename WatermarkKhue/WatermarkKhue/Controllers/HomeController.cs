using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WatermarkKhue.Models;
using System.Web;

namespace WatermarkKhue.Controllers
{

    public class HomeController : Controller
    {
        private WaveSteg file;
        private StagnoHelper sh;
        private string message;

        //Control lấy dữ liệu từ Drive rồi hiện theo dạng list
        [HttpGet]
        public ActionResult GetGoogleDriveFiles()
        {
            return View(GoogleDriveFilesRepository.GetDriveFiles());
        }

        //Control check bản quyền bài hát 
        public ActionResult Check()
        {
            return View();
        }

        //Control đã check xong
        [HttpPost]
        public ActionResult Checked(HttpPostedFileBase file)
        {
            string signature = GoogleDriveFilesRepository.Check(file);
            ViewBag.Message = signature;
            return View();
        }


        //Control gắn Watermark vào và download về máy
        public void DownloadFile(string id)
        {
            string FilePath = GoogleDriveFilesRepository.DownloadGoogleFile(id);
            file = new WaveSteg(new FileStream(FilePath, FileMode.Open, FileAccess.Read));
            sh = new StagnoHelper(file);

            message = "Bản quyền thuộc về công ty TNHH KhueChin, bạn đã tải về lúc: "+ DateTime.Now.ToString("HH:mm:ss dd-MM-yyyy zzz");
            sh.HideMessage(message);
            file.WriteFile(FilePath);

            Response.ContentType = "application/zip";
            Response.AddHeader("Content-Disposition", "attachment; filename=" + Path.GetFileName(FilePath));
            Response.WriteFile(System.Web.HttpContext.Current.Server.MapPath("~/GoogleDriveFiles/" + Path.GetFileName(FilePath)));
            Response.End();
            Response.Flush();
        }
    }
}