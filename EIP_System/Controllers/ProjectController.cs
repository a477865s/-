using EIP_System.Models;
using EIP_System.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EIP_System.Controllers
{
    public class ProjectController : Controller
    {
        EIP_DBEntities db = new EIP_DBEntities();

        //當天日期
        DateTime todayDate = Convert.ToDateTime("00:00:00");


        //======================專案表(tProject)=====================//
        public ActionResult List()
        {
            string key = Request.Form["txtKey"];
            List<CVM_ProjectTeamMember> list = null;

            if (string.IsNullOrEmpty(key))
            {
                var projects = db.tProjects.ToList();
                list = new List<CVM_ProjectTeamMember>();

                for (int i = 0; i < projects.Count(); i++)
                {
                    CVM_ProjectTeamMember prjmeb = new CVM_ProjectTeamMember();
                    prjmeb.project = projects[i];
                    prjmeb.members = db.tTeamMembers.Where(p => p.fProjectId == prjmeb.project.fProjectId).ToList();
                    list.Add(prjmeb);
                }
            }
            else
            {
                var projects = (from p in db.tProjects
                                where p.fProjectName.Contains(key) || p.fProjectId.ToString().Contains(key)
                                select p).ToList();

                list = new List<CVM_ProjectTeamMember>();

                for (int i = 0; i < projects.Count(); i++)
                {
                    CVM_ProjectTeamMember prjmeb = new CVM_ProjectTeamMember();
                    prjmeb.project = projects[i];
                    prjmeb.members = db.tTeamMembers.Where(p => p.fProjectId == prjmeb.project.fProjectId).ToList();
                    list.Add(prjmeb);
                }
            }
            return View(list);
        }

        [HttpPost]
        public string Create(tProject target)
        {
            //------新增專案-----------//
            tProject prj = new tProject();
            prj.fProjectId = target.fProjectId;
            prj.fProjectName = target.fProjectName;
            prj.fClient = target.fClient;
            prj.fPrice = target.fPrice;
            prj.fCreatdDate = target.fCreatdDate;
            prj.fDateline = target.fDateline;
            prj.fDepartment = target.fDepartment;
            prj.fEmployeeId = target.fEmployeeId;
            prj.fProgress = 0;

            prj.fStatus = "進行中";

            db.tProjects.Add(prj);
            db.SaveChanges();

            //------新增專案預算書------//
            tBudget budget = new tBudget();
            budget.fProjectId = target.fProjectId;
            budget.fManagementFeePct = 0.2;
            budget.fSalaryHour = 200;
            budget.fagree = "未申請";
            budget.fBudget = 0;

            db.tBudgets.Add(budget);
            db.SaveChanges();

            //---------新增階段表------//
            //預設為三階段//
            //todo:讓使用者自訂階段數量及名稱

            tLevel level1 = new tLevel();
            level1.fProjectId = prj.fProjectId;
            level1.fLevelName = "規劃";
            level1.fEstimateTime = 0;
            level1.fSpendCost = 0;

            tLevel level2 = new tLevel();
            level2.fProjectId = prj.fProjectId;
            level2.fLevelName = "開發";
            level2.fEstimateTime = 0;
            level2.fSpendCost = 0;

            tLevel level3 = new tLevel();
            level3.fProjectId = prj.fProjectId;
            level3.fLevelName = "測試";
            level3.fEstimateTime = 0;
            level3.fSpendCost = 0;

            db.tLevels.Add(level1);
            db.tLevels.Add(level2);
            db.tLevels.Add(level3);
            db.SaveChanges();

            return "success";
        }


        public ActionResult Edit(int? fPRJId)
        {
            var project = db.tProjects.Where(p => p.fProjectId == fPRJId).FirstOrDefault();
            return View(project);
        }

        [HttpPost]
        public ActionResult Edit(int? fPRJId, tProject target)
        {
            var project = db.tProjects.Where(p => p.fProjectId == fPRJId).FirstOrDefault();
            project.fPrice = target.fPrice;
            project.fCreatdDate = target.fCreatdDate;
            project.fDateline = target.fDateline;
            project.fDepartment = target.fDepartment;
            project.fEmployeeId = target.fEmployeeId;
            project.fStatus = target.fStatus;
            db.SaveChanges();

            return View(project);
        }

        public ActionResult Delete(int? fId)
        {
            if (fId == null)
                return RedirectToAction("List");

            //todo:擔心不一致 需要封包
            //------刪除階段(多個)----------//
            var levels = db.tLevels.Where(m => m.fProjectId == fId).ToList();
            if (levels != null)
            {
                for (int i = 0; i < levels.Count; i++)
                {
                    db.tLevels.Remove(levels[i]);
                }
            }

            //------刪除專案預算書(一個)------//
            var budget = db.tBudgets.Where(m => m.fProjectId == fId).FirstOrDefault();
            if (budget != null)
                db.tBudgets.Remove(budget);

            //todo:tProjectDetail create完成後再確認一次刪除
            //-----刪除專案工作項目(多個)-----//
            var prjDetail = db.tProjectDetails.Where(m => m.fProjectId == fId).ToList();
            if (prjDetail != null)
            {
                for (int i = 0; i < prjDetail.Count; i++)
                {
                    db.tProjectDetails.Remove(prjDetail[i]);
                }
            }

            //---------刪除專案(一個)--------//
            var prj = db.tProjects.Where(m => m.fProjectId == fId).FirstOrDefault();

            if (prj == null)
                return RedirectToAction("List");

            db.tProjects.Remove(prj);
            db.SaveChanges();

            return RedirectToAction("List");
        }

        public string GetEmpName(string empId)
        {
            int _empId = Convert.ToInt32(empId);
            var emp = db.tEmployees.Where(p => p.fEmployeeId == _empId).FirstOrDefault();

            if (emp == null)
                return "無此員工";
            return emp.fName;
        }

        //======================專案預算書==========================//
        //public ActionResult EditBudget(int? prjId)
        //{
        //    if (TempData.ContainsKey("prjId"))
        //    {
        //        var prjId_fromCreateLevel = TempData["prjId"] as int?;

        //        HttpCookie cookie = Request.Cookies["id"];
        //        int empId = Convert.ToInt32(cookie.Value);

        //        CVM_BudgetLevel bl = new CVM_BudgetLevel();
        //        bl.budget = db.tBudgets.Where(p => p.fProjectId == prjId_fromCreateLevel).FirstOrDefault();
        //        bl.levels = db.tLevels.Where(m => m.fProjectId == prjId_fromCreateLevel).ToList();

        //        ViewBag.PassDate = GetapplyDate(bl);
        //        return View(bl);
        //    }
        //    else
        //    {
        //        HttpCookie cookie = Request.Cookies["id"];
        //        int empId = Convert.ToInt32(cookie.Value);

        //        CVM_BudgetLevel bl = new CVM_BudgetLevel();
        //        bl.budget = db.tBudgets.Where(p => p.fProjectId == prjId).FirstOrDefault();
        //        bl.levels = db.tLevels.Where(m => m.fProjectId == prjId).ToList();

        //        ViewBag.PassDate = GetapplyDate(bl);
        //        return View(bl);
        //    }
        //}

        public ActionResult EditBudget(int? prjId)
        {
            HttpCookie cookie = Request.Cookies["id"];
            int empId = Convert.ToInt32(cookie.Value);

            CVM_BudgetLevel bl = new CVM_BudgetLevel();
            bl.budget = db.tBudgets.Where(p => p.fProjectId == prjId).FirstOrDefault();
            bl.levels = db.tLevels.Where(m => m.fProjectId == prjId).ToList();

            ViewBag.ApplyDate = GetapplyDate(bl);
            return View(bl);
        }

        public string GetapplyDate(CVM_BudgetLevel bl)
        {
            string applyDate = "未申請";
            if (bl.budget.fApplydate !=null)
            {
                applyDate = bl.budget.fApplydate.Value.ToString("yyyy/MM/dd");
            }
            return applyDate;
        }

        [HttpPost]
        public string EditBudget(int? prjId, CVM_BudgetLevel target)
        {
            //---------編輯階預算書--------//
            var budget = db.tBudgets.Where(p => p.fProjectId == prjId).FirstOrDefault();
            budget.fSalaryHour = target.budget.fSalaryHour;
            budget.fManagementFeePct = target.budget.fManagementFeePct;
            budget.fApplydate = todayDate;
            budget.fagree = "待審核";

            CVM_BudgetLevel bl = new CVM_BudgetLevel();
            bl.levels = db.tLevels.Where(m => m.fProjectId == prjId).ToList();            //多個時段
            bl.budget = db.tBudgets.Where(m => m.fProjectId == prjId).FirstOrDefault();   //一個預算書

            //--------編輯階段表(多個)-----//
            var level = db.tLevels.Where(m => m.fProjectId == prjId).ToList();
            for (int i = 0; i < level.Count(); i++)
            {
                level[i].fEstimateTime = Convert.ToInt32(target.levels[i].fEstimateTime);
            }

            int totaltime = 0;
            for (int i = 0; i < level.Count; i++)
            {
                totaltime += Convert.ToInt32(level[i].fEstimateTime);
            }

            int personnelCost = Convert.ToInt32(totaltime * budget.fSalaryHour);
            int managementFee = Convert.ToInt32(bl.budget.fManagementFeePct * bl.budget.tProject.fPrice);
            int totalBuget = personnelCost + managementFee;

            budget.fBudget = totalBuget;

            db.SaveChanges();
            return "success";
            //return View(bl);
        }

        [HttpPost]
        public string AgreeBudget(int? prjId)
        {
            //---------編輯階預算書--------//
            var budget = db.tBudgets.Where(p => p.fProjectId == prjId).FirstOrDefault();


            budget.fagree = "審核通過";
            budget.fPassdate = todayDate;

            db.SaveChanges();

            return "success";
        }

        //======================專案階段表==========================//
        [HttpPost]
        public string CreateLevel(string prjId, string levelName)
        {
            //TempData["prjId"] = Convert.ToInt32(prjId);

            tLevel level = new tLevel();
            level.fProjectId = Convert.ToInt32(prjId);
            level.fLevelName = levelName;
            level.fEstimateTime = 0;
            level.fSpendCost = 0;

            db.tLevels.Add(level);
            db.SaveChanges();

            return "success";
            //return RedirectToAction("EditBudget");
        }

        [HttpPost]
        public ActionResult GetEditLevel(int fId)
        {
            var level = from p in db.tLevels
                        where p.fLevelId == fId
                        select new
                        {
                            p.fLevelName,
                            p.fLevelId,
                        };

            return Json(new { data = level.ToList() }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public string EditLevel(tLevel target)
        {
            int Id = target.fLevelId;
            int prjId = target.fProjectId;

            //TempData["prjId"] = prjId;

            var level = db.tLevels.Where(p => p.fLevelId == Id).FirstOrDefault();
            level.fLevelName = target.fLevelName;
            db.SaveChanges();

            //return RedirectToAction("EditBudget");
            return "success";
        }

        public string DeleteLevel(int? fId)
        {
            //計下目前案號傳給 list
            //TempData["prjId"] = Convert.ToInt32(prjId);

            //判斷已有此階段的任務
            var task = db.tProjectDetails.Where(p => p.fLevelId == fId).Count();
            if (task > 0)
            {
                return "error";
            }

            var level = db.tLevels.Where(p => p.fLevelId == fId).FirstOrDefault();
            db.tLevels.Remove(level);
            db.SaveChanges();

            return "success";
            //return RedirectToAction("EditBudget");
        }

        //======================專案成本==========================//

        public ActionResult projectCost(int prjId)
        {
            CVM_BudgetLevel bl = new CVM_BudgetLevel();
            bl.budget = db.tBudgets.Where(p => p.fProjectId == prjId).FirstOrDefault();
            bl.levels = db.tLevels.Where(m => m.fProjectId == prjId).ToList();

            double budget = Convert.ToDouble(bl.budget.fBudget);
            var creatdDate = db.tProjects.Where(p => p.fProjectId == prjId).FirstOrDefault().fCreatdDate;
            var dateline = db.tProjects.Where(p => p.fProjectId == prjId).FirstOrDefault().fDateline;

            ViewBag.LevelName = getNameCost(bl, prjId).levelname;
            ViewBag.LevelCost = getNameCost(bl, prjId).levelcost;
            ViewBag.TaskName = getNameCost(bl, prjId).taskname;
            ViewBag.TaskCost = getNameCost(bl, prjId).tasklcost;
            ViewBag.Month = accumulateCost(bl, prjId).months;
            ViewBag.AccumulateCost = accumulateCost(bl, prjId).cost;
            ViewBag.Allcost = accumulateCost(bl, prjId).allCost;
            ViewBag.CreatdDate = creatdDate.Year + "/" + creatdDate.Month;
            ViewBag.Dateline = dateline.Value.Year + "/" + dateline.Value.Month;
            ViewBag.Progress = Convert.ToInt32((db.tProjects.Where(m => m.fProjectId == prjId).FirstOrDefault().fProgress) * 100);


            double budgetRate = accumulateCost(bl, prjId).allCost / budget;
            ViewBag.BudgetRate = Math.Round(budgetRate, 2) * 100;

            return View(bl);
        }

        public (string[] taskname, int[] tasklcost, string[] levelname, int[] levelcost) getNameCost(CVM_BudgetLevel bl, int prjId)
        {
            //找出此案的所有任務
            var tasks = db.tProjectDetails.Where(p => p.fProjectId == prjId).ToList();
            int taskcount = tasks.Count();

            //任務名稱
            string[] taskName = new string[taskcount];
            //任務時數
            int[] tasklCost = new int[taskcount];

            //找出此案的所有階段
            var levels = db.tLevels.Where(p => p.fProjectId == prjId).ToList();
            int levelcount = levels.Count();
            //階段名稱
            string[] levelName = new string[levelcount];
            //階段id
            int[] levelId = new int[levelcount];
            //階段時數
            int[] levelCost = new int[levelcount];

            for (int i = 0; i < taskcount; i++)
            {
                //任務名稱
                taskName[i] = tasks[i].fTaskName;

                int SalaryHour = 200;
                if (tasks[i].tEmployee != null)
                    SalaryHour = Convert.ToInt32(tasks[i].tEmployee.fSalaryHour);

                //各任務費用
                tasklCost[i] = Convert.ToInt32(tasks[i].fTimes) * SalaryHour;
            }


            for (int i = 0; i < levelcount; i++)
            {
                levelName[i] = levels[i].fLevelName;
                levelId[i] = levels[i].fLevelId;

                //各階段費用
                levelCost[i] = Convert.ToInt32(levels[i].fSpendCost);
            }

            return (taskName, tasklCost, levelName, levelCost);
        }

        public (string[] months, int[] cost, double allCost) accumulateCost(CVM_BudgetLevel bl, int prjId)
        {
            //所有費用合計
            double allCost = 0;

            //簽約費
            int price = Convert.ToInt32(db.tProjects.Where(p => p.fProjectId == prjId).FirstOrDefault().fPrice);
            //管銷費用
            int managementCost = Convert.ToInt32(bl.budget.fManagementFeePct * price);

            //經歷的所有月份
            var monthList = (from p in db.tTimeRecords
                             where p.fProjectId == prjId
                             select p.fDate.Month).Distinct().ToList();
            var monthsNums = monthList.Count();

            string[] months = new string[monthsNums];
            for (int i = 0; i < monthsNums; i++)
            {
                months[i] = monthList[i] + "月";
            }

            //各月的花費
            int[] cost = new int[monthsNums];
            List<tTimeRecord> recordList = new List<tTimeRecord>();

            for (int i = 0; i < monthsNums; i++)
            {
                //月分
                var nowmonth = monthList[i];
                var monthRecord = db.tTimeRecords.Where(p => p.fDate.Month == nowmonth).Where(p => p.tProjectDetail.fProjectId == prjId).ToList();

                foreach (var item in monthRecord)
                {
                    //算出各月分花費
                    cost[i] = Convert.ToInt32(item.fTime * item.tEmployee.fSalaryHour);
                }
            }

            //各月份累加費用

            if (monthsNums > 0)
            {
                cost[0] = managementCost;

                for (int i = 1; i < monthsNums; i++)
                {
                    cost[i] += cost[i - 1];
                }
                //所有費用合計
                allCost = cost[monthsNums - 1];
            }

            return (months, cost, allCost);
        }


        public ActionResult prjCostData(int prjId)
        {
            CVM_BudgetLevel bl = new CVM_BudgetLevel();
            bl.budget = db.tBudgets.Where(p => p.fProjectId == prjId).FirstOrDefault();
            bl.levels = db.tLevels.Where(m => m.fProjectId == prjId).ToList();

            var list = from l in db.tLevels
                       join b in db.tBudgets
                       on l.fProjectId equals b.fProjectId
                       where l.fProjectId == prjId
                       select new
                       {
                           //階段名稱
                           Levelname = l.fLevelName,
                           //預估費用
                           EstimateCost = (b.fSalaryHour* l.fEstimateTime).ToString(),
                           //EstimateCost = convertMoney(b.fSalaryHour * l.fEstimateTime),
                           //已花費用
                           cost = l.fSpendCost.ToString()
                       };          

            return Json(list.ToList(), JsonRequestBehavior.AllowGet);
        }

        

    }
}
