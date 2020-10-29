using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EIP_System.Models;
using EIP_System.ViewModels;

namespace EIP_System.Controllers
{
    public class SalaryCalculateController : Controller
    {

        public static int Month = DateTime.Now.Month;

        EIP_DBEntities db = new EIP_DBEntities();

        // GET: SalaryCalculate
        public ActionResult Index()
        {
            return View();

        }

        //public ActionResult InputMonth(int month)
        //{
        //    Month = month;
        //    return RedirectToAction("SalaryCalculateList");
        //}
        public ActionResult SalaryCalculateList(int? _month)
        {
            ViewBag.Month = Month;
            HttpCookie cookie = Request.Cookies["id"];
            int u = Convert.ToInt32(cookie.Value);
            int fakeid = u;

            if (_month != null)
            {
                Month = (int)_month;
                ViewBag.Month = Month;
            }

            List<VMSalaryCalculate> list = new List<VMSalaryCalculate>();
            
            //東哥開示東哥開示東哥開示東哥開示東哥開示東哥開示東哥開示東哥開示東哥開示東哥開示東哥開示東哥開示
            //集群 台北市
            var dong = (from a in db.tOvertimes
                        join a1 in db.tSignoffs
                        on a.fId equals a1.fOvertimeId
                        join a2 in db.tLeaves
                        on a1.fLeaveId equals a2.fId

                        //有養狗的家庭
                        //where a.fActiveDate.Month == Month
                        //   || a2.fActiveDate.Month == Month
                        //   && a.fEmployeeId == fakeid
                        //   || a2.fEmployeeId == fakeid
                        //&& a1.fIsAgreed == 1

                        //電話、狗的品種
                        select new
                        {   //取資料表
                            a,
                            a1,
                            a2,
                            //同名稱的處理
                            a.fId,
                            a1Id = a1.fId,
                            //跨資料表
                            a.fReason,
                            a1.fIsAgreed,
                            a2.fStatus
                        }).ToList();
            ////東哥開示東哥開示東哥開示東哥開示東哥開示東哥開示東哥開示東哥開示東哥開示東哥開示東哥開示東哥開示



            var LeaveTimeList = from a in db.tLeaves
                                join ts in db.tSignoffs
                                on a.fId equals ts.fLeaveId
                                where
                                a.fActiveDate.Month == Month
                                && a.fEmployeeId == fakeid
                                && ts.fIsAgreed == 1
                                select a;
            var OverTimeList = from a in db.tOvertimes
                               join ts in db.tSignoffs
                               on a.fId equals ts.fOvertimeId
                               where
                               a.fActiveDate.Month == Month
                               && a.fEmployeeId == fakeid
                               && ts.fIsAgreed == 1
                               select a;
            //請假foreach
            foreach (tLeave LeaveTimeItems in LeaveTimeList)
            {
                VMSalaryCalculate leaveTime = new VMSalaryCalculate();
                tEmployee temp = db.tEmployees.Where(m => m.fEmployeeId == LeaveTimeItems.fEmployeeId).FirstOrDefault();
                //月薪
                ViewBag.fSalary = temp.fSalaryMonth;
                leaveTime.fSalary = temp.fSalaryMonth;
                //日期
                leaveTime.LeaveTimeActiveDate = LeaveTimeItems.fActiveDate;
                //全扣
                if (LeaveTimeItems.fSort.Equals("事假"))
                {
                    leaveTime.fAttendMoney = LeaveTimeItems.fTimeCount * temp.fSalaryHour;
                    leaveTime.LeaveTimeActiveDate = LeaveTimeItems.fActiveDate;
                    leaveTime.fSort = LeaveTimeItems.fSort;
                }
                //扣一半
                else if (LeaveTimeItems.fSort.Equals("普通傷病假"))
                {
                    leaveTime.fAttendMoney = LeaveTimeItems.fTimeCount * temp.fSalaryHour * 0.5;
                    leaveTime.LeaveTimeActiveDate = LeaveTimeItems.fActiveDate;
                    leaveTime.fSort = LeaveTimeItems.fSort;

                }
                //不扣錢
                else if (LeaveTimeItems.fSort.Equals("特休假"))
                {
                    leaveTime.fAttendMoney = LeaveTimeItems.fTimeCount * temp.fSalaryHour * 0;
                    leaveTime.LeaveTimeActiveDate = LeaveTimeItems.fActiveDate;
                    leaveTime.fSort = LeaveTimeItems.fSort;

                }
                else if (LeaveTimeItems.fSort.Equals("婚假"))
                {
                    leaveTime.fAttendMoney = LeaveTimeItems.fTimeCount * temp.fSalaryHour * 0;
                    leaveTime.LeaveTimeActiveDate = LeaveTimeItems.fActiveDate;
                    leaveTime.fSort = LeaveTimeItems.fSort;

                }
                else if (LeaveTimeItems.fSort.Equals("喪假"))
                {
                    leaveTime.fAttendMoney = LeaveTimeItems.fTimeCount * temp.fSalaryHour * 0;
                    leaveTime.LeaveTimeActiveDate = LeaveTimeItems.fActiveDate;
                    leaveTime.fSort = LeaveTimeItems.fSort;

                }
                else if (LeaveTimeItems.fSort.Equals("公傷病假"))
                {
                    leaveTime.fAttendMoney = LeaveTimeItems.fTimeCount * temp.fSalaryHour * 0;
                    leaveTime.LeaveTimeActiveDate = LeaveTimeItems.fActiveDate;
                    leaveTime.fSort = LeaveTimeItems.fSort;

                }
                else
                {
                }
                //勞保費用規則
                leaveTime.fLBI = (int)temp.fLBI;
                leaveTime.fHI = (int)temp.fHI;
                ViewBag.fLBI = (int)temp.fLBI;
                ViewBag.fHI = (int)temp.fHI;
                leaveTime.total = Convert.ToDouble(temp.fSalaryMonth + leaveTime.fOverTimeMoney + leaveTime.fAttendMoney - leaveTime.fLBI - leaveTime.fHI);
                ViewBag.total = temp.fSalaryMonth + leaveTime.fOverTimeMoney + leaveTime.fAttendMoney - leaveTime.fLBI - leaveTime.fHI;
                list.Add(leaveTime);
            }
            //加班foreach
            foreach (tOvertime OverTimeItems in OverTimeList)
            {
                double OneTwo = 0;
                double ThreeFour = 0;
                double ThreeEight = 0;
                double NineTwelve = 0;
                double NineTen = 0;
                double ElevenTwelve = 0;
                tEmployee temp = db.tEmployees.Where(m => m.fEmployeeId == OverTimeItems.fEmployeeId).FirstOrDefault();

                VMSalaryCalculate overtime = new VMSalaryCalculate();
                //月薪
                ViewBag.fSalary = temp.fSalaryMonth;
                overtime.fSalary = temp.fSalaryMonth;
                //加班月份日期
                //overtime.fActiveDate = item.fActiveDate.GetDateTimeFormats('D')[0].ToString();
                overtime.OverTimeActiveDate = OverTimeItems.fActiveDate;

                //平日加班
                if (OverTimeItems.fSort.Equals("平日加班"))
                {
                    if (OverTimeItems.fTimeCount <= 2)
                    {
                        OneTwo = OverTimeItems.fTimeCount;
                        ThreeFour = 0;
                        overtime.fOverTimeMoney = Convert.ToInt32(OneTwo * 1.34 * temp.fSalaryHour);
                        overtime.fSort = OverTimeItems.fSort;
                    }
                    else
                    {
                        OneTwo = 2;
                        ThreeFour = OverTimeItems.fTimeCount - 2;
                        overtime.fOverTimeMoney = Convert.ToInt32((OneTwo * 1.34 + ThreeFour * 1.67) * temp.fSalaryHour);
                        overtime.fSort = OverTimeItems.fSort;

                    }
                }
                //休息日加班
                else if (OverTimeItems.fSort.Equals("休息日加班"))
                {
                    if (OverTimeItems.fTimeCount <= 2)
                    {
                        OneTwo = OverTimeItems.fTimeCount;
                        ThreeFour = 0;
                        overtime.fOverTimeMoney = Convert.ToInt32(OneTwo * 1.34 * temp.fSalaryHour);
                        overtime.fSort = OverTimeItems.fSort;

                    }
                    else if (OverTimeItems.fTimeCount <= 8)
                    {
                        OneTwo = 2;
                        ThreeEight = OverTimeItems.fTimeCount - 2;
                        overtime.fOverTimeMoney = Convert.ToInt32((OneTwo * 1.34 + ThreeEight * 1.67) * temp.fSalaryHour);
                        overtime.fSort = OverTimeItems.fSort;
                    }
                    else if (OverTimeItems.fTimeCount <= 12)
                    {
                        OneTwo = 2;
                        ThreeEight = 6;
                        NineTwelve = OverTimeItems.fTimeCount - 8;
                        overtime.fOverTimeMoney = Convert.ToInt32((OneTwo * 1.34 + ThreeEight * 1.67 + NineTwelve * 2.67) * temp.fSalaryHour);
                        overtime.fSort = OverTimeItems.fSort;

                    }
                }
                //休假日加班
                else if (OverTimeItems.fSort.Equals("休假日加班"))
                {
                    if (OverTimeItems.fTimeCount <= 8)
                    {
                        overtime.fOverTimeMoney = Convert.ToInt32((OverTimeItems.fTimeCount + 8) * temp.fSalaryHour);
                        overtime.fSort = OverTimeItems.fSort;

                    }
                    else if (OverTimeItems.fTimeCount <= 10)
                    {
                        NineTen = OverTimeItems.fTimeCount - 8;
                        overtime.fOverTimeMoney = Convert.ToInt32((16 + NineTen * 2.34) * temp.fSalaryHour);
                        overtime.fSort = OverTimeItems.fSort;


                    }
                    else if (OverTimeItems.fTimeCount <= 12)
                    {
                        NineTen = 2;
                        ElevenTwelve = OverTimeItems.fTimeCount - 10;
                        overtime.fOverTimeMoney = Convert.ToInt32((16 + NineTen * 2.34 + ElevenTwelve * 2.67) * temp.fSalaryHour);
                        overtime.fSort = OverTimeItems.fSort;

                    }
                }
                //例假日加班
                else
                {
                    overtime.fOverTimeMoney = Convert.ToInt32((OverTimeItems.fTimeCount * 2) * temp.fSalaryHour);
                    overtime.fSort = OverTimeItems.fSort;

                }
                //勞保費用規則
                overtime.fLBI = (int)temp.fLBI;
                overtime.fHI = (int)temp.fHI;
                ViewBag.fLBI = (int)temp.fLBI;
                ViewBag.fHI = (int)temp.fHI;
                overtime.total = Convert.ToDouble(temp.fSalaryMonth + overtime.fOverTimeMoney + overtime.fAttendMoney - overtime.fLBI - overtime.fHI);
                ViewBag.total = temp.fSalaryMonth + overtime.fOverTimeMoney + overtime.fAttendMoney - overtime.fLBI - overtime.fHI;
                list.Add(overtime);
            }
            return View(list);
        }
    
    }
}