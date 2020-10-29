using EIP_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EIP_System.Controllers
{
    public class Metting_roomController : Controller
    {
        EIP_DBEntities db = new EIP_DBEntities();
        // GET: monthtest
        public ActionResult mettingIndex(string message,int? fid)
        {

            var dbs = db.tMetting_date.ToList();
            HttpCookie cookie = Request.Cookies["id"];
            int u = Convert.ToInt32(cookie.Value);
            ViewBag.id = fid;
            ViewBag.user = db.tEmployees.Where(p => p.fEmployeeId == u).Select(p => p.fName).First();
            ViewBag.userid = u;
            var values = new Dictionary<string, object>();
            if (message != null) values["message"] = message;
            return View(values);
        }       

        [HttpPost]
        public ActionResult Edit(tMetting_date data)
        {
            var new_date = db.tMetting_date.FirstOrDefault(p => p.fId == data.fId);
            var old_time = db.tMetting_date.Where(p => p.fDate == data.fDate&& p.fRoom==data.fRoom&& p.fId != data.fId).Select(p => new { p.fStarttime, p.fEndtime });
            if (data == null)
                return RedirectToAction("Index");
            is_空白驗證(data);
            if (old_time.Count() != 0)
            {
                foreach (var old in old_time)
                {
                    if (data.fStarttime < old.fEndtime && data.fEndtime > old.fStarttime)
                    {
                        this.ModelState.AddModelError("fEndtime", "跟其他時間選擇衝突到，請重新選擇請選擇時間");
                        break;
                    }
                }
            }
            var b = Convert.ToDateTime(data.fDate).DayOfWeek.ToString();
            if ( b== "Sunday" || b== "Saturday" || DateTime.Now.Date>Convert.ToDateTime(data.fDate))
            {
                this.ModelState.AddModelError("fDate", "請勿選六日時間");
            }
            
            if (this.ModelState.IsValid)
            {
                new_date.fDate = data.fDate;
                new_date.fStarttime = data.fStarttime;
                new_date.fEndtime = data.fEndtime;
                new_date.fReason = data.fReason;
                db.SaveChanges();
                return RedirectToAction("mettingIndex",new { message = "編輯成功",fid=data.fId });
            }
            return RedirectToAction("mettingIndex",new { message = "除前端還有後端驗證!!!!" });
        }

        private void is_空白驗證(tMetting_date data)
        {
            if (String.IsNullOrEmpty(data.fReason))
            {
                this.ModelState.AddModelError("fReason", "不可空白");
            }

            if (data.fStarttime != null && data.fEndtime != null)
            {
                if (data.fEndtime.Value < data.fStarttime.Value)
                {
                    this.ModelState.AddModelError("fEndtime", "請選擇大於開始時間");
                }
            }
            if (data.fStarttime == null)
            {
                this.ModelState.AddModelError("fStarttime", "請選擇時間");
            }
            if (data.fEndtime == null)
            {
                this.ModelState.AddModelError("fEndtime", "請選擇時間");
            }
        }
               
        [HttpPost]
        public ActionResult Create(tMetting_date data)
        {
            var old_time = db.tMetting_date.Where(p => p.fDate == data.fDate&&p.fRoom==data.fRoom).Select(p => new { p.fStarttime, p.fEndtime });
            is_空白驗證(data);
            if (old_time.Count() != 0)
            {
                foreach (var old in old_time)
                {
                    if (data.fStarttime < old.fEndtime && data.fEndtime > old.fStarttime)
                    {
                        this.ModelState.AddModelError("fEndtime", "跟其他時間選擇衝突到，請重新選擇請選擇時間");
                        break;
                    }
                }                
            }
            var b = Convert.ToDateTime(data.fDate).DayOfWeek.ToString();
            if (b == "Sunday" || b == "Saturday" || DateTime.Now.Date > Convert.ToDateTime(data.fDate))
            {
                this.ModelState.AddModelError("fDate", "請勿選六日時間");
            }
            if (this.ModelState.IsValid)
            {
                tMetting_date d = data;
                db.tMetting_date.Add(d);
                db.SaveChanges();
                var fid = db.tMetting_date.OrderByDescending(p => p.fId).First().fId ;
                return RedirectToAction("mettingIndex", new { message = "創建成功" ,fid=fid});
            }
            return RedirectToAction("mettingIndex", new { message = "除前端還有後端驗證!!!!" });
        }
        public ActionResult Delete(int? fId)
        {

            if (fId == null)
                return View();
            var del_data = db.tMetting_date.FirstOrDefault(p => p.fId == (int)fId);
            db.tMetting_date.Remove(del_data);
            db.SaveChanges();
            return RedirectToAction("mettingIndex", new { message = "刪除成功" });
        }
        [HttpPost]
        public string seal(int[] empid,string reson,string content)
        {
            var b = empid.Distinct();
            foreach(var a in b)
            {
                tNotify notify = new tNotify();
                notify.fEmployeeId = a;
                notify.fTitle = reson;
                notify.fContent = content+"請準時出席";
                notify.fType = 0;
                notify.fTime = DateTime.Now;
                db.tNotifies.Add(notify);
                db.SaveChanges();
            }
            return "source";            

        }

    }
}
