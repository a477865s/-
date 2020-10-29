using EIP_System.Models;
using EIP_System.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EIP_System.Controllers
{
    public class ProjectDashboardController : Controller
    {
        EIP_DBEntities db = new EIP_DBEntities();

        //當天日期
        DateTime todayDate = DateTime.Now.Date;

        public ActionResult Dashboard(int? prjId)
        {
            int _prjId = Convert.ToInt32(prjId);

            CVM_BudgetLevel bl = new CVM_BudgetLevel();
            bl.budget = db.tBudgets.Where(p => p.fProjectId == _prjId).FirstOrDefault();
            bl.levels = db.tLevels.Where(m => m.fProjectId == _prjId).ToList();

            double budget = Convert.ToDouble(bl.budget.fBudget);
            var creatdDate = db.tProjects.Where(p => p.fProjectId == _prjId).FirstOrDefault().fCreatdDate;
            var dateline = db.tProjects.Where(p => p.fProjectId == _prjId).FirstOrDefault().fDateline;

            ViewBag.Todo = db.tProjectDetails.Where(p => p.fProjectId == prjId).Where(p => p.fStatus == "未開始").Count();
            ViewBag.Doing = db.tProjectDetails.Where(p => p.fProjectId == prjId).Where(p => p.fStatus == "進行中").Count();
            ViewBag.Acceptance = db.tProjectDetails.Where(p => p.fProjectId == prjId).Where(p => p.fStatus == "待驗收").Count();
            ViewBag.Finish = db.tProjectDetails.Where(p => p.fProjectId == prjId).Where(p => p.fStatus == "驗收完成").Count();
            ViewBag.LevelName = getNameCost(bl, _prjId).levelname;
            ViewBag.LevelCost = getNameCost(bl, _prjId).levelcost;
            ViewBag.TaskName = getNameCost(bl, _prjId).taskname;
            ViewBag.TaskCost = getNameCost(bl, _prjId).tasklcost;
            ViewBag.Month = accumulateCost(bl, _prjId).months;
            ViewBag.AccumulateCost = accumulateCost(bl, _prjId).cost;
            ViewBag.Allcost = accumulateCost(bl, _prjId).allCost;
            ViewBag.CreatdDate = creatdDate.Year + "/" + creatdDate.Month;
            ViewBag.Dateline = dateline.Value.Year + "/" + dateline.Value.Month;
            ViewBag.Progress = Convert.ToInt32((db.tProjects.Where(m => m.fProjectId == prjId).FirstOrDefault().fProgress) * 100);

            double budgetRate = 0;
            if (budget == 0)
            {
                ViewBag.BudgetRate = 0;
            }
            else
            {
                budgetRate = accumulateCost(bl, _prjId).allCost / budget;
                ViewBag.BudgetRate = Math.Round(budgetRate, 2) * 100;
            }
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


    }
}