using EIP_System.Models;
using EIP_System.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EIP_System.Controllers
{
    public class Attend_PunchtimeController : Controller
    {
        EIP_DBEntities db = new EIP_DBEntities();
        public VMEmployee getLoginEmpData()
        {
            //取目前登入使用者
            HttpCookie cookie = Request.Cookies["id"];

            int empId = Convert.ToInt32(cookie.Value);

            VMEmployee vmemp = new VMEmployee().convert(db.tEmployees.Where(m => m.fEmployeeId == empId).FirstOrDefault());

            return vmemp;
        }

        public int EmpApplyPunchtimeCount(int EmpId)
        {
            DateTime Today = DateTime.Now;
            DateTime mouth_月初 = new DateTime(Today.Year, Today.Month, 1);
            DateTime mouth_月底 = new DateTime(Today.Year, Today.Month, DateTime.DaysInMonth(Today.Year, Today.Month));

            int count = db.tApplypunches
                            .Where(m => m.fEmployeeId == EmpId &&
                                        mouth_月初 < m.fApplyDate && m.fApplyDate < mouth_月底).Count();
            //最多申請兩次
            int EmpApplycount = ((2 - count) < 0) ? 0 : (2 - count);

            return EmpApplycount;
        }

        // GET: Attend_Punchtime
        public ActionResult PunchtimeIndex()
        {
            VMEmployee Emp = getLoginEmpData();

            //取得client IP
            //string REMOTE_ADDR = Request.ServerVariables["REMOTE_ADDR"];
            string REMOTE_ADDR = "118.160.81.252";
            //設定允許的IP位址
            string[] IPList =
                {
                //我家
                "61.231.60.147",
                //學校
                "118.160.81.252"
            };
            bool isPass = false;
            foreach (var item in IPList)
                if (item == REMOTE_ADDR) { isPass = true; break; }

            ViewBag.REMOTE_ADDR = REMOTE_ADDR;
            ViewBag.isPass = isPass;

            //謹慎使用
            //(new CPunchtimeHelper()).InsertPunchtime(Emp.id, new DateTime(2020, 9, 1), new DateTime(2020, 10, 15));

            //申請人的同部門&上級
            int Auth = ((Emp.auth + 1) > 3) ? 3 : (Emp.auth + 1);
            var supervisors = from s in db.tEmployees
                              where s.fDepartment == Emp.department && s.fAuth == Auth
                              select new
                              {
                                  s.fEmployeeId,
                                  s.fName
                              };
            //轉SelectListItem
            List<SelectListItem> sup_items = new List<SelectListItem>();
            foreach (var item in supervisors)
            {
                sup_items.Add(new SelectListItem()
                {
                    Text = item.fName,
                    Value = item.fEmployeeId.ToString()
                });
            }

            //ViewBag 傳前端
            ViewBag.Empname = Emp.name;
            ViewBag.supervisors = sup_items;
            ViewBag.applycount = EmpApplyPunchtimeCount(Emp.id);

            return View();
        }
        //取得員工的打卡紀錄
        public ActionResult getEmpPunchtimes()
        {
            VMEmployee Emp = getLoginEmpData();

            List<VMPunchtime> Emp_punchtimelist = (new VMPunchtime()).getlist(db.tPunchtimes.Where(m => m.fEmployeeId == Emp.id).ToList());

            return Json(Emp_punchtimelist, JsonRequestBehavior.AllowGet);
        }
        //打卡
        public ActionResult punchTime(DateTime now, bool isDemo)
        {
            VMEmployee Emp = getLoginEmpData();

            var punchtime = (new CPunchtimeHelper()).punchtime(Emp.id, now, isDemo);

            return Json(new { msg_type = punchtime.msg_type, msg = punchtime.msg }, JsonRequestBehavior.AllowGet);
        }
        //補打卡申請
        [HttpPost]
        public ActionResult punchTimeApply(VMPunchtimeApply applyPunchtime)
        {
            VMEmployee Emp = getLoginEmpData();

            //用完申請紀錄
            if (EmpApplyPunchtimeCount(Emp.id) == 0)
                return Json("error", JsonRequestBehavior.AllowGet);
            //重複申請
            if (db.tApplypunches.Where(m => m.fPunchTimeId == applyPunchtime.applyId).FirstOrDefault() != null)
                return Json("same", JsonRequestBehavior.AllowGet);

            //申請補打卡
            tApplypunch Applypunch = new tApplypunch();
            Applypunch.fEmployeeId = Emp.id;
            Applypunch.fPunchTimeId = applyPunchtime.applyId;
            Applypunch.fApplyDate = DateTime.Now;
            Applypunch.fSort = applyPunchtime.sort;
            Applypunch.fReason = applyPunchtime.reason;

            db.tApplypunches.Add(Applypunch);
            db.SaveChanges();

            //簽核表 3天內審核完成
            tSignoff tSignoff = new tSignoff();
            tSignoff.fAlpplypunchId = int.Parse(db.tApplypunches
                .OrderByDescending(p => p.fId)
                .Select(r => r.fId)
                .First().ToString());
            tSignoff.fSupervisorId = applyPunchtime.supervisorId;
            tSignoff.fApplyClass = applyPunchtime.sort;
            tSignoff.fStartdate = DateTime.Now;
            tSignoff.fEnddate = DateTime.Now.AddDays(3);

            db.tSignoffs.Add(tSignoff);
            db.SaveChanges();

            return Json("success", JsonRequestBehavior.AllowGet);
        }


    }
}