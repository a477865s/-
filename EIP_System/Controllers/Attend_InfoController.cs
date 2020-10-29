using EIP_System.Models;
using EIP_System.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EIP_System.Controllers
{
    public class Attend_InfoController : Controller
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

        private static List<VMsignoff> Emp_list;

        // GET: Attend_Info
        public ActionResult InfoIndex()
        {
            //取目前登入使用者
            VMEmployee Emp = getLoginEmpData();

            //取得員工申請的紀錄
            List<VMsignoff> list = (new VMsignoff())
                                    .getList(db.tSignoffs
                                    .OrderByDescending(m => m.fId).ToList());

            Emp_list = list.Where(m => m.emp_id == Emp.id).ToList();

            //需要隨時更新
            //若此員工沒特休紀錄，載入一個
            var Leave_特休假 = db.tLeavecounts.Where(m => m.fEmployeeId == Emp.id && m.fSortId == 1).FirstOrDefault();

            if (Leave_特休假 == null)
            {
                (new CLeaveHelper()).Leavecount(Emp.id, "特休假", 0);
            }

            return View();
        }

        [HttpPost]
        public ActionResult getAllData()
        {
            return Json(new { data = Emp_list }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getrow(int id)
        {
            return Json(Emp_list.Where(m => m.id == id).ToList(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult submitRevoke(int id)
        {
            //移除資料
            tSignoff tSignoff = db.tSignoffs.Where(m => m.fId == id).FirstOrDefault();

            int attend_id = 0;
            if (tSignoff.fLeaveId != null)
            {
                //請假
                attend_id = (int)tSignoff.fLeaveId;
                db.tLeaves.Remove(db.tLeaves.Where(m => m.fId == attend_id).FirstOrDefault());
            }
            else if (tSignoff.fOvertimeId != null)
            {
                //加班
                attend_id = (int)tSignoff.fOvertimeId;
                db.tOvertimes.Remove(db.tOvertimes.Where(m => m.fId == attend_id).FirstOrDefault());
            }
            else
            {
                //補打卡
                attend_id = (int)tSignoff.fAlpplypunchId;
                db.tApplypunches.Remove(db.tApplypunches.Where(m => m.fId == attend_id).FirstOrDefault());
            }

            db.tSignoffs.Remove(tSignoff);
            db.SaveChanges();

            //Emp_list 移除資料
            var target = Emp_list.Where(m => m.id == id).FirstOrDefault();
            Emp_list.Remove(target);

            return Json("success", JsonRequestBehavior.AllowGet);
        }

        //請假統計
        [HttpPost]
        public ActionResult getLeaveChartData()
        {
            VMEmployee Emp = getLoginEmpData();

            List<VMLeavecount> list = 
                (new VMLeavecount()).getlist(db.tLeavecounts.Where(m => m.fEmployeeId == Emp.id).ToList());

            return Json(list, JsonRequestBehavior.AllowGet);
        }
    }
}