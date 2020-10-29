using EIP_System.Models;
using EIP_System.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EIP_System.Controllers
{
    public class AttendController : Controller
    {
        private EIP_DBEntities db = new EIP_DBEntities();
        public VMEmployee getLoginEmpData()
        {
            //取目前登入使用者
            HttpCookie cookie = Request.Cookies["id"];

            int empId = Convert.ToInt32(cookie.Value);

            VMEmployee vmemp = new VMEmployee().convert(db.tEmployees.Where(m => m.fEmployeeId == empId).FirstOrDefault());

            return vmemp;
        }

        // GET: Attend
        public ActionResult AttendIndex()
        {
            VMEmployee Emp = getLoginEmpData();

            var Todayrecord = from p in db.tPunchtimes.AsEnumerable()
                              where p.fDatetime.Date == DateTime.Now.Date && p.fEmployeeId == Emp.id
                              select new
                              {
                                  status = p.fstatus,
                                  datetime = p.fDatetime.ToLongTimeString()
                              };

            //ViewBag 傳前端
            ViewBag.Todayrecord = Newtonsoft.Json.JsonConvert.SerializeObject(Todayrecord);

            return View();
        }
        //取得該員工考勤紀錄
        [HttpPost]
        public ActionResult getLeaverecord()
        {
            VMEmployee Emp = getLoginEmpData();

            List<VMsignoff> list = (new VMsignoff())
                                    .getList(db.tSignoffs.ToList())
                                    .Where(m => m.emp_id == Emp.id)
                                    .Take(10)
                                    .ToList();

            return Json(new { data = list }, JsonRequestBehavior.AllowGet);
        }

        //取得假別統計
        public ActionResult getLeaveTotal()
        {
            VMEmployee Emp = getLoginEmpData();

            var leavetotal = from lt in db.tLeaves
                             where lt.fEmployeeId == Emp.id
                             group lt by lt.fSort into g
                             select new
                             {
                                 label = g.Key,
                                 hour = g.Sum(p => p.fTimeCount)
                             };

            return Json(leavetotal.ToList(), JsonRequestBehavior.AllowGet);
        }

    }
}