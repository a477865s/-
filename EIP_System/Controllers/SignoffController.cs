using EIP_System.Models;
using EIP_System.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EIP_System.Controllers
{
    public class SignoffController : Controller
    {
        EIP_DBEntities db = new EIP_DBEntities();

        private static List<VMsignoff> list;

        // GET: Signoff
        public ActionResult SignoffIndex()
        {
            list = (new VMsignoff()).getList(db.tSignoffs.OrderByDescending(m => m.fId).ToList());

            return View();
        }
        public ActionResult getAllData()
        {

            return Json(new { data = list.Where(m => m.isagreed == null) }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getSignoffData()
        {

            return Json(new { data = list.Where(m => m.isagreed != null) }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getRow(int? id)
        {
            VMsignoff row = list.Where(m => m.id == id).FirstOrDefault();

            return Json(row, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Editpass(int id, int agree)
        {
            //資料庫更新
            tSignoff signoff = db.tSignoffs.Where(m => m.fId == id).FirstOrDefault();
            signoff.fIsAgreed = agree;  //通過 不通過
            signoff.fPassdate = DateTime.Now;   //通過日期

            //list更新
            var target = list.Where(m => m.id == id).FirstOrDefault();
            target.isagreed = agree;
            target.passdate = DateTime.Now.ToString("yyyy-MM-dd hh:mm");

            //通過審核
            if (agree == 1)
            {
                if (target.catelog == "請假申請")
                {
                    //儲存假別紀錄
                    int empId = signoff.tLeave.fEmployeeId;
                    string sortName = signoff.tLeave.fSort;
                    double useTime = signoff.tLeave.fTimeCount;
                    (new CLeaveHelper()).Leavecount(empId, sortName, useTime);

                    createNotifies(empId, "審核通過", "申請" + sortName + "已通過審核");
                }
                if (target.catelog == "補打卡申請")
                {
                    //儲存假別紀錄
                    int empId = signoff.tApplypunch.fEmployeeId;
                    //修改打卡紀錄
                    int punchtimeId = signoff.tApplypunch.fPunchTimeId;
                    string status = "已補打卡";

                    tPunchtime punchtime = db.tPunchtimes
                        .Where(m => m.fId == punchtimeId)
                        .FirstOrDefault();

                    punchtime.fstatus = status;

                    createNotifies(empId, "審核通過", "申請補打卡" + punchtime.fDatetime + "已通過審核");
                }
                if (target.catelog == "加班申請")
                {
                    //儲存假別紀錄
                    int empId = signoff.tOvertime.fEmployeeId;
                    string sortName = signoff.tOvertime.fSort;

                    createNotifies(empId, "審核通過", "申請" + sortName + "已通過審核");
                }
            }

            db.SaveChanges();

            return Json("success", JsonRequestBehavior.AllowGet);

        }
        public void createNotifies(int empId, string title, string content)
        {
            tNotify notify = new tNotify();
            notify.fEmployeeId = empId;
            notify.fTitle = title;
            notify.fContent = content;
            notify.fType = 0;
            notify.fTime = DateTime.Now;
            db.tNotifies.Add(notify);
            db.SaveChanges();

        }
    }
}