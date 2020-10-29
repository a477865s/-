using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using EIP_System.Models;
using EIP_System.ViewModels;
//加班邏輯
//正常時間每日8小時，每周40小時
//加班單日不能超過4小時，當月總計不能超過46小時，每三個月總計不能超過138小時

//加班申請失敗demo1
//單日加班超過4小時

//加班申請失敗demo2
//單月總計超過46小時

//加班申請失敗demo3
//三個月總計加班超過138小時

//加班費試算規則(適用月薪制)

//平日加班
//1~2小時為時薪*1.34
//3~4小時為時薪*1.67

//休息日的加班(禮拜六)
//1~2小時為時薪*1.34
//3~8小時為時薪*1.67
//9~12小時為時薪*2.67

//休假日的加班(禮拜日)
//1~8小時為日薪
//9~10小時為時薪*2.34
//11~12小時為時薪*2.67

namespace EIP_System.Controllers
{
    public class OverTimeController : Controller
    {
        EIP_DBEntities db = new EIP_DBEntities();
        private static VMEmployee Emp;

        // GET: OverTime
        public ActionResult Index()
        {
            return View();
        }
        //取得加班紀錄
        [HttpPost]
        public ActionResult getOverTimeRecord()
        {
            List<VMCOvertime> list = new List<VMCOvertime>();
            foreach (tOvertime item in db.tOvertimes.ToList())
            {
                tSignoff temp = db.tSignoffs.Where(m => m.fOvertimeId == item.fId).FirstOrDefault();


                VMCOvertime overtime = new VMCOvertime();
                overtime.fId = item.fId;
                overtime.fEmployeeId = item.fEmployeeId;
                overtime.fSort = item.fSort;
                overtime.fSubmitDate = item.fSubmitDate;
                overtime.fActiveDate = item.fActiveDate;
                overtime.fTimeCount = item.fTimeCount;
                overtime.fReason = item.fReason;
                overtime.isAgree = temp.fIsAgreed;
                list.Add(overtime);

            }
            return Json(new { data = list }, JsonRequestBehavior.AllowGet);            
        }
        public ActionResult CreateOverTime()
        {
            HttpCookie cookie = Request.Cookies["id"];
            int u = Convert.ToInt32(cookie.Value);
            int fakeid = u;
            //取得該名員工資料
            Emp = (new VMEmployee())
                .convert(db.tEmployees
                .Where(m => m.fEmployeeId == fakeid)
                .FirstOrDefault());

            //ViewBag 傳前端
            ViewBag.emp = Newtonsoft.Json.JsonConvert.SerializeObject(Emp);
            return View();

        }
        [HttpPost]
        public ActionResult CreateOverTime(tOvertime o, tSignoff s, tEmployee t)
        {
            HttpCookie cookie = Request.Cookies["id"];
            int u = Convert.ToInt32(cookie.Value);
            int fakeid = u;
            Emp = (new VMEmployee())
                    .convert(db.tEmployees
                    .Where(m => m.fEmployeeId == fakeid)
                    .FirstOrDefault());

            //ViewBag 傳前端
            ViewBag.emp = Newtonsoft.Json.JsonConvert.SerializeObject(Emp);
            //撈資料庫當月
            double TimeCountForMonth = (from a in db.tOvertimes.AsEnumerable()
                                        join ts4 in db.tSignoffs.AsEnumerable()
                                        on a.fId equals ts4.fOvertimeId
                                        where a.fActiveDate.Year == DateTime.Now.Year
                                        && a.fActiveDate.Month == DateTime.Now.Month
                                        && a.fEmployeeId == fakeid
                                        && ts4.fIsAgreed==1
                                        select a.fTimeCount).DefaultIfEmpty(0).Sum();
            ////撈資料庫上個月
            //double TimeCountLastMonth = (from a in db.tOvertimes.AsEnumerable()
            //                              where a.fActiveDate.Year == DateTime.Now.Year
            //                              && a.fActiveDate.Month == DateTime.Now.AddMonths(-1).Month
            //                              && a.fEmployeeId == fakeid
            //                              select a.fTimeCount).Sum();
            ////撈資料庫上上個月
            //double TimeCountBeforeLastMonth = (from a in db.tOvertimes.AsEnumerable()
            //                                   where a.fActiveDate.Year == DateTime.Now.Year
            //                                   && a.fActiveDate.Month == DateTime.Now.AddMonths(-2).Month
            //                                   && a.fEmployeeId == fakeid
            //                                   select a.fTimeCount).Sum();
            if (string.IsNullOrEmpty(o.fSort)||string.IsNullOrEmpty(o.fReason))
            {
                TempData["Attend_msg"] = "請輸入加班類別或原因";
                return View();
            }
            //本日加班查詢
            int NowDateMonth = o.fActiveDate.Month;
            int NowDate = o.fActiveDate.Day;
            double TimeCountToday = (from a in db.tOvertimes.AsEnumerable()
                                     join ts5 in db.tSignoffs.AsEnumerable()
                                     on a.fId equals ts5.fOvertimeId
                                     where a.fActiveDate.Month == NowDateMonth
                                     &&a.fActiveDate.Day== NowDate
                                     && a.fEmployeeId == fakeid
                                     && ts5.fIsAgreed == 1
                                     select a.fTimeCount).DefaultIfEmpty().Sum()+o.fTimeCount;
            //三個月加班合計
            //本月
            double TimeCountOne = (from b in db.tOvertimes.AsEnumerable()
                                   join ts6 in db.tSignoffs.AsEnumerable()
                                   on b.fId equals ts6.fOvertimeId
                                   where b.fActiveDate.Year == DateTime.Now.Year 
                                   && b.fActiveDate.Month == DateTime.Now.Month 
                                   && b.fEmployeeId == fakeid
                                   && ts6.fIsAgreed == 1
                                   select b.fTimeCount).Sum();
            //上月
            double TimeCountTwo = (from c in db.tOvertimes.AsEnumerable()
                                   join ts7 in db.tSignoffs.AsEnumerable()
                                   on c.fId equals ts7.fOvertimeId
                                   where c.fActiveDate.Year == DateTime.Now.Year 
                                   && c.fActiveDate.Month == DateTime.Now.AddMonths(-1).Month 
                                   && c.fEmployeeId == fakeid
                                   && ts7.fIsAgreed == 1
                                   select c.fTimeCount).Sum();
            //上上月
            double TimeCountThree = (from d in db.tOvertimes.AsEnumerable()
                                     join ts8 in db.tSignoffs.AsEnumerable()
                                     on d.fId equals ts8.fOvertimeId
                                     where d.fActiveDate.Year == DateTime.Now.Year 
                                     && d.fActiveDate.Month == DateTime.Now.AddMonths(-2).Month 
                                     && d.fEmployeeId == fakeid
                                     && ts8.fIsAgreed == 1
                                     select d.fTimeCount).Sum();

            double TimeCountThreeMonth = TimeCountOne + TimeCountTwo + TimeCountThree;
            //第一層判斷他目前是不是已經超過加班上限了
            //if (TimeCountForMonth > 46)
            //{
            //    TempData["message"] = "你太累瞜~~當月加班時數超過上限";
            //    return View();
            //}
            if (TimeCountThreeMonth > 138)
            {
                TempData["Attend_msg"] = "你太累瞜~~三個月內累計加班時數超過上限";
                return View();
            }

            if (o.fTimeCount > 4 || TimeCountToday > 4)
            {               
                if (o.fSort=="平日加班")
                {
                    TempData["Attend_msg"] = "你太累瞜~~平日加班時數超過上限";
                    return View();
                }
                else if (TimeCountToday > 12)
                {
                    TempData["Attend_msg"] = "你太累瞜~~當日加班總時數超過上限";
                    return View();
                }
            }
            //假設系統撈出來的資料目前都沒有超過上限
            //新增時共有兩個Table，一個是加班申請，一個是簽核表插入
            s.fOvertimeId = o.fId;//簽核表編號=加班申請fid
            s.fApplyClass = o.fSort;//簽核表種類=加班申請
            o.fSubmitDate = DateTime.Now/*.ToLocalTime()*/;//申請日期等於Now
            o.fActiveDate = o.fActiveDate/*.ToLocalTime()*/;
            s.fStartdate = o.fActiveDate;//簽核表申請日期=現在加班申請日期

            //s.tEmployee.fEmployeeId = fakeid;//簽核表寫入員工編號
            o.fEmployeeId = fakeid;//加班表寫入員工編號
           
            //查詢部門別及權限用來判斷核決主管是誰
            var Check = (from b in db.tEmployees
                               where b.fEmployeeId == fakeid
                               select new { b.fDepartment,b.fAuth }).FirstOrDefault();
            //先把物件取出來，登入者的資訊物件

            //這個是登入者的部門
            string department = Check.fDepartment.ToString();
            //這個是登入者的權限
            int auth = Check.fAuth;

            //帶入主管編號
            var SupervisorIdCheckJunior = (from a in db.tEmployees
                                where a.fAuth == 2 && a.fDepartment == department
                                select new { a.fEmployeeId}).FirstOrDefault();
            var SupervisorIdCheckSenior = (from a in db.tEmployees
                                     where a.fAuth == 3 && a.fDepartment == department
                                     select new { a.fEmployeeId }).FirstOrDefault();
            //這個是一般主管的代號
            int SupervisorJunior = SupervisorIdCheckJunior.fEmployeeId;
            //這個是最高權限的代號
            int SupervisorIdSenior = SupervisorIdCheckSenior.fEmployeeId;

            //寫入相對應的主管編號
            if (auth == 1)
            {
                s.fSupervisorId = SupervisorJunior;//主管權限
            }
            else
            {
                s.fSupervisorId = SupervisorIdSenior;//最高權限編號
            }
            
            s.fIsAgreed = null;//寫入是否同意，預設為null(待審核)
            s.fEnddate = o.fActiveDate;
            o.fTimeCount = Convert.ToDouble(o.fTimeCount);//選單輸入的文字轉成加班的數字並存回加班表
            //很遺憾的，可能有超時狀況，進入細項判斷式
            if (o.fTimeCount + TimeCountForMonth > 46 || o.fTimeCount + TimeCountThreeMonth > 138)
            {
                //這個是申請當月的月份
                int NowMonth = s.fStartdate.Month;
                int LastMonth = s.fStartdate.Month - 1;
                int BeforeLastMonth = s.fStartdate.Month - 2;

                //申請加班開始日期的月份已經請的時數
                double now = (from to in db.tOvertimes.AsEnumerable()
                              join ts1 in db.tSignoffs.AsEnumerable()
                              on to.fId equals ts1.fOvertimeId
                              where to.fActiveDate.Year == DateTime.Now.Year
                              && to.fActiveDate.Month == NowMonth
                              && to.fEmployeeId == fakeid
                              && ts1.fIsAgreed == 1
                              select to.fTimeCount).DefaultIfEmpty(0).Sum();


                //申請加班開始日期的上個月已經請的時數
                double Last = (from a in db.tOvertimes.AsEnumerable()
                               join ts2 in db.tSignoffs.AsEnumerable()
                               on a.fId equals ts2.fOvertimeId
                               where a.fActiveDate.Year == DateTime.Now.Year
                               && a.fActiveDate.Month == LastMonth
                               && a.fEmployeeId == fakeid
                               && ts2.fIsAgreed == 1
                               select a.fTimeCount).DefaultIfEmpty(0).Sum();

                //申請加班開始日期的上上個月已經請的時數
                double BeforeLast = (from a in db.tOvertimes.AsEnumerable()
                                     join ts3 in db.tSignoffs.AsEnumerable()
                                     on a.fId equals ts3.fOvertimeId
                                     where a.fActiveDate.Year == DateTime.Now.Year
                                     && a.fActiveDate.Month == BeforeLastMonth
                                     && a.fEmployeeId == fakeid
                                     && ts3.fIsAgreed == 1
                                     select a.fTimeCount).DefaultIfEmpty(0).Sum();

                //申請加班當月的時數已經超過上限
                if (now + o.fTimeCount > 46)
                {
                    TempData["Attend_msg"] = "你太累瞜~~當月加班時數超過上限";
                    return View();
                }
                //申請加班的前三個月總時數已經超過上限
                else if (Last + BeforeLast + o.fTimeCount > 138)
                {
                    TempData["Attend_msg"] = "你太累瞜~~累計三個月內加班時數超過上限";
                    return View();
                }
                //沒事，他有可能是申請上個月的加班，但是因為這個月已經滿了，才會進來這個鬼地方
                else
                {
                    //他可以出去判斷式了
                    db.tOvertimes.Add(o);//加入t物件(加班表)
                    db.tSignoffs.Add(s);//加入s物件(簽核表)
                    db.SaveChanges();//存檔
                    TempData["Attend_msg"] = "送出成功";
                    //return View();
                    return RedirectToAction("AttendIndex", "Attend");
                }
            }
            //他當月既沒有超過上限，三個月內也沒有超過上限
            db.tOvertimes.Add(o);//加入t物件(加班表)
            db.tSignoffs.Add(s);//加入s物件(簽核表)
            db.SaveChanges();//存檔
            TempData["Attend_msg"] = "送出成功";
            //return View();
            return RedirectToAction("AttendIndex", "Attend");
        }

    }
}