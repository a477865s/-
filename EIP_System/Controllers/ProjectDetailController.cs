using EIP_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Microsoft.Ajax.Utilities;


namespace EIP_System.Controllers
{
    public class ProjectDetailController : Controller
    {
        EIP_DBEntities db = new EIP_DBEntities();
        DateTime todayDate = DateTime.Now.Date;

        public ActionResult Index(int prjId)
        {
            //取的此案所屬部門的員工----->團隊成員下拉式選單
            var dep = db.tProjects.Where(p => p.fProjectId == prjId).FirstOrDefault().fDepartment;

            var emps = from p in db.tEmployees
                       where p.fDepartment == dep
                       select new
                       {
                           empId = p.fEmployeeId,
                           empName = p.fName
                       };

            //團隊成員下拉式選單來源
            var empslist = emps.ToList();
            ViewBag.Emps = new MultiSelectList(empslist, "empId", "empName");

            TempData["prjId"] = prjId;

            ViewBag.PrjName = db.tProjects.Where(p => p.fProjectId == prjId).FirstOrDefault().fProjectName;
            ViewBag.Member = db.tTeamMembers.Where(p => p.fProjectId == prjId).ToList();
            ViewBag.Mem = db.tTeamMembers.Where(p => p.fProjectId == prjId).Select(p => new { p.fProjectId, p.fEmployeeId, p.tEmployee.fName, p.tEmployee.fDepartment }).ToList();
            ViewBag.Todo = db.tProjectDetails.Where(p => p.fProjectId == prjId).Where(p => p.fStatus == "未開始").Count();
            ViewBag.Doing = db.tProjectDetails.Where(p => p.fProjectId == prjId).Where(p => p.fStatus == "進行中").Count();
            ViewBag.Acceptance = db.tProjectDetails.Where(p => p.fProjectId == prjId).Where(p => p.fStatus == "待驗收").Count();
            ViewBag.Finish = db.tProjectDetails.Where(p => p.fProjectId == prjId).Where(p => p.fStatus == "驗收完成").Count();

            return View();
        }
        public ActionResult GetaData(int fId)
        {
            var list = from p in db.tProjectDetails
                       where p.fProjectId == fId
                       select new
                       {
                           prjDetailId = p.fProjectDetailId,
                           levelName = p.tLevel.fLevelName,
                           taskName = p.fTaskName,
                           empId = p.fEmployeeId,
                           empName = p.tEmployee.fName,
                           status = p.fStatus,
                           startTime = p.fStartTime.Value.Year + "/" + p.fStartTime.Value.Month + "/" + p.fStartTime.Value.Day,
                           deadline = p.fDeadline.Value.Year + "/" + p.fDeadline.Value.Month + "/" + p.fDeadline.Value.Day
                           //remark = p.fRemarks
                       };
            return Json(new { data = list }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetaLastData(int fId)
        {
            //取得最新五筆任務
            var lastList = from p in db.tProjectDetails
                       where (p.fProjectId == fId && p.fDeadline >= todayDate)
                       select new
                       {
                           prjDetailId = p.fProjectDetailId,
                           levelName = p.tLevel.fLevelName,
                           taskName = p.fTaskName,
                           empId = p.fEmployeeId,
                           empName = p.tEmployee.fName,
                           status = p.fStatus,
                           startTime = p.fStartTime.Value.Year + "/" + p.fStartTime.Value.Month + "/" + p.fStartTime.Value.Day,
                           deadline = p.fDeadline.Value.Year + "/" + p.fDeadline.Value.Month + "/" + p.fDeadline.Value.Day
                       };

            if (lastList.Count() == 0)
            {
                //如果沒有近期任務
                var otherList = from p in db.tProjectDetails
                           where p.fProjectId == fId
                           select new
                           {
                               prjDetailId = p.fProjectDetailId,
                               levelName = p.tLevel.fLevelName,
                               taskName = p.fTaskName,
                               empId = p.fEmployeeId,
                               empName = p.tEmployee.fName,
                               status = p.fStatus,
                               startTime = p.fStartTime.Value.Year + "/" + p.fStartTime.Value.Month + "/" + p.fStartTime.Value.Day,
                               deadline = p.fDeadline.Value.Year + "/" + p.fDeadline.Value.Month + "/" + p.fDeadline.Value.Day
                           };
                return Json(new { data = otherList.Take(5) }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                //如果有近期任務
                return Json(new { data = lastList.Take(5) }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GetEdit(int fId)
        {
            var task = from p in db.tProjectDetails
                       where p.fProjectDetailId == fId
                       select new
                       {
                           prjDetailId = p.fProjectDetailId,
                           leveId = p.tLevel.fLevelId,
                           levelName = p.tLevel.fLevelName,
                           taskName = p.fTaskName,
                           empId = p.fEmployeeId,
                           empName = p.tEmployee.fName,
                           status = p.fStatus,
                           startTime = p.fStartTime,
                           deadline = p.fDeadline,
                           remark = p.fRemarks,
                           pmId = p.tProject.fEmployeeId
                       };

            return Json(new { data = task.ToList() }, JsonRequestBehavior.AllowGet);
        }

        public string Test()
        {
            return "";
        }

        public ActionResult Create(int? prjId)
        {
            return View();
        }

        [HttpPost]
        public string Create(tProjectDetail target)
        {
            TempData.Keep();
            var prjId = TempData["prjId"] as int?;

            if (target.fTaskName == null || target.fEmployeeId==null)
            {
                //任務名稱不為空
                return "error";
            }

            tProjectDetail prjDetail = new tProjectDetail();
            prjDetail.fProjectId = target.fProjectId;
            prjDetail.fLevelId = target.fLevelId;
            prjDetail.fTaskName = target.fTaskName;
            prjDetail.fEmployeeId = target.fEmployeeId;
            prjDetail.fStatus = target.fStatus;
            prjDetail.fStartTime = target.fStartTime;
            prjDetail.fDeadline = target.fDeadline;
            prjDetail.fRemarks = target.fRemarks;
            prjDetail.fTimes = 0;
            db.tProjectDetails.Add(prjDetail);
            db.SaveChanges();

            updateProgress(prjDetail.fProjectId);

            var startTime = prjDetail.fStartTime;
            var deadline = prjDetail.fDeadline;

            //發通知給任務負責人
            tNotify notify = new tNotify();
            notify.fEmployeeId = Convert.ToInt32(prjDetail.fEmployeeId);
            notify.fTitle = prjDetail.tProject.fProjectName;
            notify.fContent = "請到我的任務查看新任務";
            notify.fType = 0;
            notify.fTime = DateTime.Now;
            db.tNotifies.Add(notify);
            db.SaveChanges();

            //存到行事曆上
            tCalendar task = new tCalendar();
            task.fEmployeeId = prjDetail.fEmployeeId;
            task.fStart = Convert.ToDateTime(startTime).ToString("yyyy-MM-dd");
            task.fEnd = Convert.ToDateTime(deadline).ToString("yyyy-MM-dd");
            task.fTitle = db.tProjects.Where(p => p.fProjectId == prjDetail.fProjectId).FirstOrDefault().fProjectName;
            task.fContent = prjDetail.fTaskName;
            task.fSort = "1";
            db.tCalendars.Add(task);
            db.SaveChanges();

            return "success";
        }

        //工作項目頁面編輯
        [HttpPost]
        public string Edit(tProjectDetail target)
        {
            int id = target.fProjectDetailId;
            var prjDetail = db.tProjectDetails.Where(p => p.fProjectDetailId == id).FirstOrDefault();

            prjDetail.fLevelId = target.fLevelId;
            prjDetail.fTaskName = target.fTaskName;
            prjDetail.fEmployeeId = target.fEmployeeId;
            prjDetail.fStatus = target.fStatus;
            prjDetail.fStartTime = target.fStartTime;
            prjDetail.fDeadline = target.fDeadline;
            prjDetail.fRemarks = target.fRemarks;

            updateProgress(prjDetail.fProjectId);

            db.SaveChanges();
            return "success";
        }

        //我的頁面狀態編輯
        [HttpPost]
        public string EditStatus(int? prjDetailId, string fStatus)/*tProjectDetail target*/
        {
            int id = Convert.ToInt32(prjDetailId);
            //int id = target.fProjectDetailId;
            var prjDetail = db.tProjectDetails.Where(p => p.fProjectDetailId == id).FirstOrDefault();
            prjDetail.fStatus = fStatus;
            //prjDetail.fStatus = target.fStatus;
            db.SaveChanges();

            //任務名稱
            var taskName = db.tProjectDetails.Where(p => p.fProjectDetailId == id).FirstOrDefault().fTaskName;
            //案名
            var prjName = db.tProjectDetails.Where(p => p.fProjectDetailId == id).FirstOrDefault().tProject.fProjectName;
            //主管
            var pmId = db.tProjectDetails.Where(p => p.fProjectDetailId == id).FirstOrDefault().tProject.fEmployeeId;
            //訊息
            var content = taskName + "任務已已完成，待請主管驗收";

            //發訊息給主管
            if (prjDetail.fStatus == "待驗收")
            {
                tNotify notify = new tNotify();
                notify.fEmployeeId = pmId;
                notify.fTitle = prjName;
                notify.fContent = content;
                notify.fType = 0;
                notify.fTime = DateTime.Now;
                db.tNotifies.Add(notify);
                db.SaveChanges();
            }
            return "success";
        }

        public JsonResult Delete(int fId)
        {
            //todo:封包刪除及更新進度條

            //檢查是否存在工時紀錄
            var records = db.tTimeRecords.Where(p => p.fProjectDetailId == fId).ToList();
            if (records.Count() > 0)
            {
                return Json("error", JsonRequestBehavior.AllowGet);
            }

            var task = db.tProjectDetails.Where(p => p.fProjectDetailId == fId).FirstOrDefault();

            int id = task.tProject.fProjectId;

            db.tProjectDetails.Remove(task);
            db.SaveChanges();

            updateProgress(id);

            return Json("success", JsonRequestBehavior.AllowGet);
        }

        public string GetLevels(string prjId)
        {
            int _prjId = Convert.ToInt32(prjId);

            var levels = from i in db.tLevels
                         where i.fProjectId == _prjId
                         select new { i.fLevelName, i.fLevelId };
            string jsonString = JsonConvert.SerializeObject(levels);
            return jsonString;
        }

        public string GetEmps(string prjId)
        {
            int _prjId = Convert.ToInt32(prjId);

            var list = from p in db.tTeamMembers
                       where p.fProjectId == _prjId
                       select new
                       {
                           Id = p.fEmployeeId,
                           Name = p.tEmployee.fName
                       };

            string jsonString = JsonConvert.SerializeObject(list);
            return jsonString;
        }

        //更新進度條
        public void updateProgress(int fId)
        {
            //找出此案號所有的任務
            var prjDetailList = db.tProjectDetails.Where(p => p.fProjectId == fId).ToList();
            //分子
            double finished = 0;
            double progress = 0;

            //分母
            double sum = prjDetailList.Count();
            if (sum != 0)
            {
                //分子
                finished = prjDetailList.Where(p => p.fStatus == "驗收完成").Count();
                //進度
                progress = finished / sum;
            }
            else
            {
                progress = 0;
            }

            var project = db.tProjects.Where(p => p.fProjectId == fId).FirstOrDefault();
            project.fProgress = progress;
            db.SaveChanges();
        }

        //========我的任務頁面========//
        public ActionResult MyTaskIndex()
        {
            HttpCookie cookie = Request.Cookies["id"];
            int empId = Convert.ToInt32(cookie.Value);

            var list = from p in db.tProjectDetails
                       where p.fEmployeeId == empId && p.tProject.fStatus == "進行中"
                       select new { status = p.fStatus };

            ViewBag.Todo = list.Where(p => p.status == "未開始").Count();
            ViewBag.Doing = list.Where(p => p.status == "進行中").Count();
            ViewBag.Acceptance = list.Where(p => p.status == "待驗收").Count();
            ViewBag.Finish = list.Where(p => p.status == "驗收完成").Count();

            return View();
        }

        public ActionResult GetMyTaskData()
        {
            HttpCookie cookie = Request.Cookies["id"];
            int empId = Convert.ToInt32(cookie.Value);

            var list = from p in db.tProjectDetails
                       where p.fEmployeeId == empId && p.tProject.fStatus == "進行中"
                       select new
                       {
                           prjDetailId = p.fProjectDetailId,
                           prjId = p.fProjectId,
                           prjName = p.tProject.fProjectName,
                           levelName = p.tLevel.fLevelName,
                           taskName = p.fTaskName,
                           empId = p.fEmployeeId,
                           empName = p.tEmployee.fName,
                           status = p.fStatus,
                           startTime = p.fStartTime.Value.Year + "/" + p.fStartTime.Value.Month + "/" + p.fStartTime.Value.Day,
                           deadline = p.fDeadline.Value.Year + "/" + p.fDeadline.Value.Month + "/" + p.fDeadline.Value.Day,
                           remark = p.fRemarks
                       };

            return Json(new { data = list }, JsonRequestBehavior.AllowGet);
        }

        //====================================團隊成員===================================//

        public string CreateTeamMember(int? prjId, int[] empId)
        {
            for (int i = 0; i < empId.Length; i++) 
            {
                tTeamMember emp = new tTeamMember();
                emp.fProjectId = Convert.ToInt32(prjId);
                emp.fEmployeeId = empId[i];
                db.tTeamMembers.Add(emp);
                db.SaveChanges();
            }
            return "success";
        }


        //===============新增多筆 ===============================//

        public string demoInsert(int prjId)
        {
            string[] taskName = { "流程圖","資料庫建置","介面設計","訂單系統","介面整合" };
            int[] empId = { 117,118,118,119,119};
            int[] levelId = {
                db.tLevels.Where(p=>p.fProjectId== prjId).FirstOrDefault().fLevelId,
                db.tLevels.Where(p=>p.fProjectId== prjId).FirstOrDefault().fLevelId,
                db.tLevels.Where(p=>p.fProjectId== prjId).OrderBy(P=>P.fLevelId).Skip(1).FirstOrDefault().fLevelId,
                db.tLevels.Where(p=>p.fProjectId== prjId).OrderBy(P=>P.fLevelId).Skip(1).FirstOrDefault().fLevelId,
                db.tLevels.Where(p=>p.fProjectId== prjId).OrderBy(P=>P.fLevelId).Skip(2).FirstOrDefault().fLevelId
            };

            //DateTime[] startTime = { };
            //DateTime[] deadline = { };

            for(int i=0;i<5;i++)
            {
                tProjectDetail prjDetail = new tProjectDetail();
                prjDetail.fProjectId = prjId;
                prjDetail.fLevelId = levelId[i];
                prjDetail.fTaskName = taskName[i];
                prjDetail.fEmployeeId = empId[i];
                prjDetail.fStatus = "未開始";
                //prjDetail.fStartTime = startTime[i];
                //prjDetail.fDeadline = deadline[i];
                prjDetail.fTimes = 0;
                db.tProjectDetails.Add(prjDetail);
                db.SaveChanges();

                updateProgress(prjId);
            }
            return "success";
        }
    }
}