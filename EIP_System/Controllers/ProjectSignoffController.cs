using EIP_System.Models;
using EIP_System.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EIP_System.Controllers
{
    public class ProjectSignoffController : Controller
    {
        EIP_DBEntities db = new EIP_DBEntities();

        //===========================工時紀錄=============================//
        public ActionResult Index()
        {
            var budgets = db.tBudgets.Where(p => p.fagree == "待審核").ToList();
            List<CVM_BudgetLevel> list = new List<CVM_BudgetLevel>();

            for (int i = 0; i < budgets.Count(); i++)
            {
                CVM_BudgetLevel bl = new CVM_BudgetLevel();
                bl.budget = budgets[i];
                bl.levels = db.tLevels.Where(p => p.fProjectId == bl.budget.fProjectId).ToList();
                list.Add(bl);
            }

            return View(list);
        }


        public ActionResult Getdata()
        {
            //取得未通過紀錄
            var recordlist = from p in db.tTimeRecords
                             where p.fApprove == null
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

            return Json(new { data = recordlist }, JsonRequestBehavior.AllowGet);
        }


        //審核工時紀錄
        public ActionResult Edit(int recordId, int approve,string reason)
        {
            var record = db.tTimeRecords.Where(p => p.fTimeRecordId == recordId).FirstOrDefault();
            var task = db.tProjectDetails.Where(p => p.fProjectDetailId == record.fProjectDetailId).FirstOrDefault();
            var level = db.tLevels.Where(p => p.fLevelId == task.fLevelId).FirstOrDefault();

            if (approve == 1)
            {
                record.fApprove = "同意";
                //時數存入tProjectDetail
                task.fTimes += record.fTime;
                //人事成本存入tlevel
                level.fSpendCost += record.fTime * 200;
            }

            if (approve == 0)
            {
                record.fApprove = "不同意";
                record.fRemarks = reason;

                //發小鈴鐺訊息給員工
                tNotify notify = new tNotify();
                notify.fEmployeeId = record.fEmployeeId;
                notify.fTitle = "工時紀錄審查不通過";
                notify.fContent = "請盡快修正紀錄";
                notify.fType = 0;
                notify.fTime = DateTime.Now;
                db.tNotifies.Add(notify);
                db.SaveChanges();
            }

            db.SaveChanges();

            return Json("success", JsonRequestBehavior.AllowGet);
        }

        public ActionResult histroyRecord()
        {
            var employid = db.tEmployees.Select(p => new{ p.fEmployeeId,p.fName}).ToList();
            ViewBag.emp = employid;
            return View();
        }

        [HttpPost]
        public ActionResult Search(string emp,DateTime startTime, DateTime endTime)
        {
            var list = from p in db.tTimeRecords
                       where p.fDate >= startTime && p.fDate <= endTime && (p.fEmployeeId.ToString().Contains(emp)|| p.tEmployee.fName.Contains(emp))
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

        //===========================審核預算書============================//
        //public ActionResult GetBudget()
        //{
        //    var budgets = db.tBudgets.Where(p => p.fagree != "審核通過").ToList();
        //    List<CVM_BudgetLevel> list = null;

        //    for (int i = 0; i < budgets.Count(); i++)
        //    {
        //        CVM_BudgetLevel bl = new CVM_BudgetLevel();
        //        bl.budget = budgets[i];
        //        bl.levels = db.tLevels.Where(p=>p.fProjectId== bl.budget.fProjectId).ToList();
        //        list.Add(bl);
        //    }

        //    return View(list);
        //}

        //===========================驗收任務=============================//

        public ActionResult GetTask()
        {
            //todo:取得此登入員工為專案負責人的驗收任務
            var list = from p in db.tProjectDetails
                       where p.fStatus == "待驗收"
                       select new
                       {
                           prjDetailId = p.fProjectDetailId,
                           projectId = p.fProjectId,
                           projectName = p.tProject.fProjectName,
                           levelName = p.tLevel.fLevelName,
                           taskName = p.fTaskName,
                           empName = p.tEmployee.fName,
                           deadline = p.fDeadline.Value.Year + "/" + p.fDeadline.Value.Month + "/" + p.fDeadline.Value.Day,
                       };
            return Json(new { data = list }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetPrjAcceptanceTask(int? prjId)
        {
            //取得此案的待驗收任務
            var list = from p in db.tProjectDetails
                       where p.fStatus == "待驗收" &&p.fProjectId==prjId
                       select new
                       {
                           prjDetailId = p.fProjectDetailId,
                           projectId = p.fProjectId,
                           projectName = p.tProject.fProjectName,
                           levelName = p.tLevel.fLevelName,
                           taskName = p.fTaskName,
                           empName = p.tEmployee.fName,
                           deadline = p.fDeadline.Value.Year + "/" + p.fDeadline.Value.Month + "/" + p.fDeadline.Value.Day,
                       };
            return Json(new { data = list }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Acceptance(int prjDetailId,int approve) 
        {
            var task = db.tProjectDetails.Where(p => p.fProjectDetailId == prjDetailId).FirstOrDefault();

            if (approve == 1)
            {
                task.fStatus = "驗收完成";

                //發小鈴鐺訊息給負責人
                tNotify notify = new tNotify();
                notify.fEmployeeId = Convert.ToInt32(task.fEmployeeId);
                notify.fTitle = task.tProject.fProjectName;
                notify.fContent = task.fTaskName+ "任務驗收完成";
                notify.fType = 0;
                notify.fTime = DateTime.Now;
                db.tNotifies.Add(notify);
                db.SaveChanges();
            }

            if (approve == 0)
            {
                task.fStatus = "進行中";

                //發小鈴鐺訊息給負責人
                tNotify notify = new tNotify();
                notify.fEmployeeId = Convert.ToInt32(task.fEmployeeId);
                notify.fTitle = task.tProject.fProjectName;
                notify.fContent = task.fTaskName + "任務驗收未通過";
                notify.fType = 0;
                notify.fTime = DateTime.Now;
                db.tNotifies.Add(notify);
                db.SaveChanges();
            }

            db.SaveChanges();

            int id = task.tProject.fProjectId;
            updateProgress(id);

            return Json("success", JsonRequestBehavior.AllowGet);
        }

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
    }
}