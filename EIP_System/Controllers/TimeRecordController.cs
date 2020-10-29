using Newtonsoft.Json;
using EIP_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EIP_System.ViewModels;
using Microsoft.Ajax.Utilities;
using System.Text;
using System.Text.Json;
namespace EIP_System.Controllers
{
    public class TimeRecordController : Controller
    {
        EIP_DBEntities db = new EIP_DBEntities();

        //當天日期
        DateTime todayDate = DateTime.Now.Date;

        public ActionResult Index()
        {
            HttpCookie cookie = Request.Cookies["id"];
            int user = Convert.ToInt32(cookie.Value);

            return View();
        }

        public string GetProjectName(string prjId)
        {
            int _prjID = Convert.ToInt32(prjId);
            var prj = db.tProjects.Where(p => p.fProjectId == _prjID).FirstOrDefault();

            if (prj == null)
                return "查無此專案";

            return prj.fProjectName;
        }

        public string GetLevels(string prjId)
        {
            int _prjID = Convert.ToInt32(prjId);

            var levels = from i in db.tLevels
                         where i.fProjectId== _prjID
                         select new { i.fLevelName,i.fLevelId };

            string jsonString = JsonConvert.SerializeObject(levels);
            return jsonString;
        }

        public string GetTask(string levelId)
        {
            int _levelId = Convert.ToInt32(levelId);

            var prjaDetail = from p in db.tProjectDetails
                             where p.fLevelId== _levelId
                             select new { p.fProjectDetailId, p.fTaskName };

            string jsonString = JsonConvert.SerializeObject(prjaDetail);
            return jsonString;
        }

        //todo: 取得登入員工的當月紀錄
        public ActionResult Getdata()
        {
            HttpCookie cookie = Request.Cookies["id"];
            int user = Convert.ToInt32(cookie.Value);

            var recordList = from p in db.tTimeRecords
                             where p.fEmployeeId == user && (p.fDate== todayDate || p.fApprove!="同意")         
                             select new
                             {
                                 prjDetailId =p.fProjectDetailId,
                                 timeRecordId = p.fTimeRecordId,
                                 date = p.fDate.Year + "/" + p.fDate.Month + "/" + p.fDate.Day,
                                 employeeId = p.fEmployeeId,
                                 employeeName = p.tEmployee.fName,
                                 projectId = p.fProjectId,
                                 projectName = p.tProject.fProjectName,
                                 levelName = p.tProjectDetail.tLevel.fLevelName,
                                 taskName = p.tProjectDetail.fTaskName,
                                 time = p.fTime,
                                 approve = p.fApprove,
                                 remarks=p.fRemarks
                             };
            return Json(new { data = recordList }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create(tTimeRecord target)
        {
            //todo:若新增時輸入資料錯誤 驗證
            //var prj = db.tProjects.Where(p => p.fProjectId == target.fProjectId).FirstOrDefault();
            //if (prj == null)
            //    return RedirectToAction("Index");

            HttpCookie cookie = Request.Cookies["id"];
            int user = Convert.ToInt32(cookie.Value);

            tTimeRecord record = new tTimeRecord();
            record.fDate = todayDate;
            record.fEmployeeId = user;                         
            record.fProjectId = target.fProjectId;
            record.fProjectDetailId = target.fProjectDetailId;
            record.fTime = target.fTime;

            db.tTimeRecords.Add(record);
            db.SaveChanges();
            return Json("success", JsonRequestBehavior.AllowGet);
        }

        public JsonResult Delete(int id)
        {
            var del = db.tTimeRecords.Where(p => p.fTimeRecordId == id).FirstOrDefault();
            db.tTimeRecords.Remove(del);
            db.SaveChanges();

            return Json("success", JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Search(DateTime startTime, DateTime endTime)
        {
            HttpCookie cookie = Request.Cookies["id"];
            int user = Convert.ToInt32(cookie.Value);

            var list = from p in db.tTimeRecords
                       where p.fDate >= startTime && p.fDate <= endTime && p.fEmployeeId == user    
                       select new
                       {
                           timeRecordId = p.fTimeRecordId,
                           date = p.fDate.Year + "/" + p.fDate.Month + "/" + p.fDate.Day,
                           employeeId = p.fEmployeeId,
                           employeeName = p.tEmployee.fName,
                           projectId = p.fProjectId,
                           projectName = p.tProject.fProjectName,
                           levelName = p.tProjectDetail.tLevel.fLevelName,
                           taskName = p.tProjectDetail.fTaskName,
                           time = p.fTime,
                           approve = p.fApprove
                       };


            return Json(new { data = list }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult prjChart(DateTime startTime, DateTime endTime)
        {
            HttpCookie cookie = Request.Cookies["id"];
            int user = Convert.ToInt32(cookie.Value);

            var list = from p in db.tTimeRecords
                       where p.fDate >= startTime && p.fDate <= endTime && p.fEmployeeId == user
                       group p by p.tProject.fProjectName into g
                       select new
                       {
                           prjname= g.Key,
                           prjtime = g.Sum(p=>p.fTime)
                       };

            var data = list.ToList();

            return new ContentResult()
            {
                Content = System.Text.Json.JsonSerializer.Serialize(data),
                ContentEncoding = Encoding.UTF8,
                ContentType = "json"
            };
        }

        public ActionResult taskChart(DateTime? startTime, DateTime? endTime)
        {
            HttpCookie cookie = Request.Cookies["id"];
            int user = Convert.ToInt32(cookie.Value);

            //var list = from p in db.tTimeRecords
            //           where p.fDate >= startTime && p.fDate <= endTime && p.fEmployeeId == user
            //           group p by p.tProjectDetail.fProjectDetailId into g
            //           select new
            //           {
            //               taskname = taskName(g.Key),
            //               tasktime = g.Sum(p => p.fTime)
            //           };

            var list = from p in db.tTimeRecords
                       where p.fDate >= startTime && p.fDate <= endTime && p.fEmployeeId == user
                       group p by p.tProjectDetail.fTaskName into g
                       select new
                       {
                           taskname = g.Key,
                           tasktime = g.Sum(p => p.fTime)
                       };

            var data = list.ToList();

            return new ContentResult()
            {
                Content = System.Text.Json.JsonSerializer.Serialize(data),
                ContentEncoding = Encoding.UTF8,
                ContentType = "json"
            };
        }

        //public string taskName(int id)
        //{
        //    string name = db.tProjectDetails.Where(p => p.fProjectDetailId == id).FirstOrDefault().fTaskName;
        //    return name;
        //}

        [HttpPost]
        public ActionResult GetEdit(int fId)
        {
            var task = from p in db.tTimeRecords
                       where p.fTimeRecordId == fId
                       select new
                       {
                           timeRecordId=p.fTimeRecordId,
                           projectDetailId=p.fProjectDetailId,
                           taskName = p.tProjectDetail.fTaskName,
                           levelId=p.tProjectDetail.fLevelId,
                           evelName=p.tProjectDetail.tLevel.fLevelName,
                           time =p.fTime
                       };

            return Json(new { data = task.ToList() }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public string Edit(int timeRecordId, int time)
        {
            var record = db.tTimeRecords.Where(p => p.fTimeRecordId == timeRecordId).FirstOrDefault();
            record.fTime = time;
            record.fApprove = null;

            db.SaveChanges();
            return "success";
        }

        //所有案子的案號
        //var prjList = list.Distinct().ToList();
        //var prjList = (from p in list
        //               select p.projectId).Distinct().ToList();
        ////案子數量
        //var prjNums = prjList.Count();
        ////所有案名
        //string[] prjName = new string[prjNums];
        ////所有案號
        //int[] prjId = new int[prjNums];
        //for (int i = 0; i < prjNums; i++)
        //{
        //    prjId[i] = prjList[i];
        //    int id = prjId[i];

        //    string name = db.tProjects.Where(p => p.fProjectId == id).FirstOrDefault().fProjectName;
        //    prjName[i] = name;
        //}
        ////所有案子的時數
        //int[] prjTime = new int[prjNums];
        //for (int i = 0; i < prjNums; i++)
        //{
        //    int id = prjId[i];
        //    //依案號分群
        //    prjTime[i] = list.Where(p => p.projectId == id).Sum(p => p.time);
        //}
    }
}