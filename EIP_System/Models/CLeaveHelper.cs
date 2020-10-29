using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EIP_System.Models
{
    public enum LeaveSortName
    {
        特休假 = 1,
        事假 = 2,
        普通傷病假 = 3,
        婚假 = 5,
        喪假 = 6,
        公傷病假 = 8
    }
    public class CLeaveHelper
    {
        //public int hour_特休假 = 0;
        public int hour_事假 = 14 * 8;
        public int hour_普通傷病假 = 30 * 8;
        public int hour_婚假 = 8 * 8;
        public int hour_喪假 = 8 * 8;

        EIP_DBEntities db = new EIP_DBEntities();

        //判斷請假時數是否正常
        public (bool isPass, string err_msg) checkLeavehour(int empId, string sortName, double usingtime)
        {
            //找這名員工假別紀錄
            tLeavecount lc_record = db.tLeavecounts
                .Where(m => m.fEmployeeId == empId && m.tleavesort.fLeavename == sortName)
                .FirstOrDefault();

            //判斷時數是否符合條件
            bool isPass = true; string err_msg = "";
            double usedtime = (lc_record == null) ? 0 : lc_record.fUesdtime;
            double remaintime = (lc_record == null) ? 0 : lc_record.fRemaintime;

            switch (sortName)
            {
                case "特休假":
                    if ((usingtime + usedtime) >= remaintime) { isPass = false; err_msg = "超過特休假上限"; }
                    break;
                case "事假":
                    if ((usingtime + usedtime) >= hour_事假) { isPass = false; err_msg = "超過事假上限"; }
                    break;
                case "普通傷病假":
                    if ((usingtime + usedtime) >= hour_普通傷病假) { isPass = false; err_msg = "超過普通傷病假上限"; }
                    break;
                case "婚假":
                    if ((usingtime + usedtime) >= hour_婚假) {isPass = false; err_msg = "超過婚假上限"; }
                    break;
                case "喪假":
                    if ((usingtime + usedtime) >= hour_喪假) { isPass = false; }
                    else err_msg = "超過喪假上限";
                    break;
                case "公傷病假":
                    isPass = false;
                    break;
            }

            return (isPass, err_msg);
        }

        //取得特休假
        private int getLeave_特休假hours(int empId)
        {
            int hour_特休假 = 0;

            //開始日期 = 入職當天
            DateTime startdate = db.tEmployees
                .Where(m => m.fEmployeeId == empId)
                .FirstOrDefault().fHireDate;

            //特休計算 一年365天
            // 6個月以上1年未滿者，3日。 
            // 1年以上2年未滿者，7日。 
            // 2年以上3年未滿者，10日。 
            // 3年以上5年未滿，每年14日。 
            // 5年以上10年未滿者，每年15日。 
            // 10年以上者，每1年加給1日，加至30日為止。 

            TimeSpan dateTime = DateTime.Now.Date - startdate;
            int days = dateTime.Days;

            if (6 * 30 <= days && days < 365) hour_特休假 = 3;
            else if (365 <= days && days < 365 * 2) hour_特休假 = 7;
            else if (365 * 2 <= days && days < 365 * 3) hour_特休假 = 10;
            else if (365 * 3 <= days && days < 365 * 5) hour_特休假 = 14;
            else if (365 * 5 <= days && days < 365 * 10) hour_特休假 = 15;
            else if (365 * 10 <= days)
            {
                int adddays = (days - 365 * 10) / 365;
                hour_特休假 = 15 + adddays;
            }
            return hour_特休假 * 8;
        }

        //假別時數累加計算
        public void Leavecount(int empId, string sortName, double usetime)
        {
            //開始日期 = 入職當天
            DateTime startdate = db.tEmployees
                .Where(m => m.fEmployeeId == empId)
                .FirstOrDefault().fHireDate;

            int sortId = 0; int alltime = 0; DateTime enddate = new DateTime();

            switch (sortName)
            {
                case "特休假":
                    sortId = (int)LeaveSortName.特休假;
                    alltime = getLeave_特休假hours(empId);
                    enddate = DateTime.Now;
                    break;
                case "事假":
                    sortId = (int)LeaveSortName.事假;
                    alltime = hour_事假;
                    enddate = startdate.AddYears(1);
                    break;
                case "普通傷病假":
                    sortId = (int)LeaveSortName.普通傷病假;
                    alltime = hour_普通傷病假;
                    enddate = startdate.AddYears(1);
                    break;
                case "婚假":
                    sortId = (int)LeaveSortName.婚假;
                    alltime = hour_婚假;
                    break;
                case "喪假":
                    sortId = (int)LeaveSortName.喪假;
                    alltime = hour_喪假;
                    break;
                case "公傷病假":
                    sortId = (int)LeaveSortName.公傷病假;
                    alltime = 0;
                    break;
            }

            tLeavecount lc = db.tLeavecounts.Where(m => m.fSortId == sortId && m.fEmployeeId == empId).FirstOrDefault();
            if (lc == null)
            {
                //建立員工假別新紀錄
                tLeavecount leavecount = new tLeavecount()
                {
                    fEmployeeId = empId,
                    fSortId = sortId,
                    fAlltime = alltime,
                    fUesdtime = usetime,
                    fRemaintime = alltime - usetime,
                    fStartdate = startdate,
                    fEnddate = enddate
                };
                db.tLeavecounts.Add(leavecount);
                db.SaveChanges();
            }
            else
            {
                //累加上去
                double tmp_remain = lc.fRemaintime;
                double tmp_use = lc.fUesdtime;
                lc.fUesdtime = tmp_use + usetime;
                lc.fRemaintime = tmp_remain - usetime;
                db.SaveChanges();
            }

        }

        //取得假別Json
        public string getLeavecountJSON(int empId, string sortName)
        {

            int sortId = 0;
            switch (sortName)
            {
                case "特休假":
                    sortId = (int)LeaveSortName.特休假;
                    break;
                case "事假":
                    sortId = (int)LeaveSortName.事假;
                    break;
                case "普通傷病假":
                    sortId = (int)LeaveSortName.普通傷病假;
                    break;
                case "婚假":
                    sortId = (int)LeaveSortName.婚假;
                    break;
                case "喪假":
                    sortId = (int)LeaveSortName.喪假;
                    break;
                case "公傷病假":
                    sortId = (int)LeaveSortName.公傷病假;
                    break;
            }

            var Leavecount = from lc in db.tLeavecounts
                             where lc.fEmployeeId == empId && lc.fSortId == sortId
                             select new
                             {
                                 alltime = lc.fAlltime,
                                 usedtime = lc.fUesdtime,
                                 remaintime = lc.fRemaintime,
                                 startdate = lc.fStartdate,
                                 enddate = lc.fEnddate
                             };

            return Newtonsoft.Json.JsonConvert.SerializeObject(Leavecount);
        }
    }
}