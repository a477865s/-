using EIP_System.Models;
using EIP_System.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EIP_System.Controllers
{
    public class Attend_LeaveController : Controller
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

        // GET: Attend_Leave
        public ActionResult LeaveIndex()
        {

            //取目前登入使用者
            VMEmployee Emp = getLoginEmpData();

            //需要隨時更新
            //若此員工沒特休紀錄，載入一個
            var Leave_特休假 = db.tLeavecounts.Where(m => m.fEmployeeId == Emp.id && m.fSortId == 1).FirstOrDefault();

            if (Leave_特休假 == null)
            {
                (new CLeaveHelper()).Leavecount(Emp.id, "特休假", 0);
            }


            //取得該名員工資料
            Emp = (new VMEmployee())
                .convert(db.tEmployees
                .Where(m => m.fEmployeeId == Emp.id)
                .FirstOrDefault());

            //ViewBag 傳前端
            ViewBag.emp = Newtonsoft.Json.JsonConvert.SerializeObject(Emp);

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
            ViewBag.supervisors = sup_items;

            //假別資料
            var leavelist = from ls in db.tleavesorts
                            select new
                            {
                                id = ls.fSortId,
                                name = ls.fLeavename,
                                content = ls.fLeavecontent,
                            };

            ViewBag.leavelist = Newtonsoft.Json.JsonConvert.SerializeObject(leavelist);

            //轉SelectListItem
            List<SelectListItem> leave_items = new List<SelectListItem>();
            foreach (var item in leavelist)
            {
                leave_items.Add(new SelectListItem()
                {
                    Text = item.name,
                    Value = item.name
                });
            }
            //ViewBag 傳前端
            ViewBag.leaves = leave_items;

            return View();
        }
        [HttpPost]
        public ActionResult CreateLeave(VMLeave leave)
        {
            VMEmployee Emp = getLoginEmpData();

            //檢查時數是否超過
            var checkLeave = (new CLeaveHelper()).checkLeavehour(Emp.id, leave.leavesort, leave.timecount);

            if (checkLeave.isPass)
            {
                //請假儲存
                tLeave tLeave = new tLeave();
                tLeave.fEmployeeId = Emp.id;
                tLeave.fSort = leave.leavesort;
                tLeave.fApplyDate = DateTime.Now;
                tLeave.fActiveDate = leave.start;
                tLeave.fEndDate = leave.end;
                tLeave.fTimeCount = leave.timecount;
                tLeave.fReason = leave.reason;

                db.tLeaves.Add(tLeave);
                db.SaveChanges();

                //簽核表
                tSignoff tSignoff = new tSignoff();
                tSignoff.fLeaveId = int.Parse(db.tLeaves
                    .OrderByDescending(p => p.fId)
                    .Select(r => r.fId)
                    .First().ToString());
                tSignoff.fSupervisorId = leave.supervisorId;
                tSignoff.fApplyClass = leave.leavesort;
                tSignoff.fStartdate = DateTime.Now;
                tSignoff.fEnddate = leave.start;

                db.tSignoffs.Add(tSignoff);
                db.SaveChanges();

                //通知
                //tNotify notify = new tNotify();
                //notify.fEmployeeId = Convert.ToInt32(leave.supervisorId);
                //notify.fTitle = "員工請假申請通知";
                //notify.fContent = "員工 " + Emp.name + " 申請" + leave.leavesort;
                //notify.fType = 0;
                //notify.fTime = DateTime.Now;
                //notify.fSort = leave.leavesort;

                //db.tNotifies.Add(notify);
                //db.SaveChanges();

                TempData["Attend_msg"] = leave.leavesort + " 申請成功";

                return RedirectToAction("AttendIndex", "Attend");
            }
            else
            {
                TempData["Attend_err_msg"] = checkLeave.err_msg;

                return RedirectToAction("LeaveIndex", "Attend_Leave");
            }


        }

    }
}