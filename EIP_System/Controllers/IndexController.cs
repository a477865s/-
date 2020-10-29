using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using EIP_System.Models;

namespace 期末專題.Controllers
{
    public class IndexController : Controller
    {
        // GET: Index
        public  ActionResult Index()
        {
            HttpCookie cookie = Request.Cookies["id"];
            if(cookie == null)
                return RedirectToAction("Login", "login");

            try { string name = Session["name" + cookie.Value].ToString(); }
            catch { return RedirectToAction("Login", "login");}
                

            int auth =Convert.ToInt32(Session["auth"+cookie.Value]);
            if (auth>1)
                ViewBag.a = "true";
            else
                ViewBag.a = "false";
            ViewBag.acc = Session["name"+ cookie.Value].ToString();
            return View();
        }
        [HttpPost]
        public string ShowBoard()
        {
            EIP_DBEntities db = new EIP_DBEntities();
            var list = from b in db.tBillboards
                       select new 
                       {
                           b.fId,
                           b.fTitle,
                           b.fContent,
                           b.fPostTime,
                           b.fType,
                           b.fEmployeeId
                       };
            string json = JsonConvert.SerializeObject(list);
            return json;
        }
        [HttpPost]
        public string GetEmployeeName(string id) 
        {
            int fid = Convert.ToInt32(id);
            EIP_DBEntities db = new EIP_DBEntities();
            var list = (from e in db.tEmployees
                        where e.fEmployeeId == fid
                        select e).FirstOrDefault();
            return list.fName;
        }
        [HttpPost]
        public string ShowBell()
        {
            HttpCookie cookie = Request.Cookies["id"];
            loginController lc = new loginController();
            int fid = Convert.ToInt32(cookie.Value);
            EIP_DBEntities db = new EIP_DBEntities();
            var list = from b in db.tNotifies
                       where b.fEmployeeId == fid 
                       select new
                       {
                        b.fId,
                        b.fType,
                        b.fTitle,
                        b.fContent,
                        b.fTime,
                        b.fEmployeeId
                       };
            string json = JsonConvert.SerializeObject(list);
            return json;
        }
        [HttpPost]
        public string ShowCalendar(string id)
        {
            HttpCookie cookie = Request.Cookies["id"];
            int fid = Convert.ToInt32(id);
            string json = "";
            EIP_DBEntities db = new EIP_DBEntities();
            IQueryable list;
            if (string.IsNullOrEmpty(id))
            {
                int empid = Convert.ToInt32(cookie.Value);
                list = from b in db.tCalendars
                       where b.fEmployeeId == empid
                       select new { 
                       b.fId,
                       b.fContent,
                       b.fStart,
                       b.fEnd,
                       b.fTitle,
                       b.fSort,
                       } ;
                json = JsonConvert.SerializeObject(list);
                return json;
            }
            list = from b in db.tCalendars
                   where b.fId == fid
                   select new
                   {
                       b.fId,
                       b.fContent,
                       b.fStart,
                       b.fEnd,
                       b.fTitle,
                   };
            json = JsonConvert.SerializeObject(list);
            return json;
        }
        [HttpPost]
        public string InsertBoard(string id,string title,string content,string type) 
        {
            string json = "";
            HttpCookie cookie = Request.Cookies["id"];
            tBillboard tb = new tBillboard();
            IQueryable list;
            EIP_DBEntities db = new EIP_DBEntities();
            if (string.IsNullOrEmpty(id)) 
            {
                tb.fContent = content;
                tb.fTitle = title;
                tb.fType = type;
                tb.fPostTime = DateTime.Now.ToString();
                tb.fEmployeeId = Convert.ToInt32(cookie.Value);
                db.tBillboards.Add(tb);
                db.SaveChanges();
                list = from b in db.tBillboards
                           select new
                           {
                               b.fId,
                               b.fTitle,
                               b.fContent,
                               b.fPostTime,
                               b.fEmployeeId,
                               b.fType
                           };
                json = JsonConvert.SerializeObject(list);
                return json;
            }
            int fid = Convert.ToInt32(id);
            var listu = (db.tBillboards.Where(x => x.fId == fid)).FirstOrDefault();
            listu.fContent = content;
            listu.fTitle = title;
            listu.fType = type;
            listu.fPostTime = DateTime.Now.ToString();
            db.SaveChanges();
            list = from b in db.tBillboards
                   select new
                   {
                       b.fId,
                       b.fTitle,
                       b.fContent,
                       b.fPostTime,
                       b.fEmployeeId,
                       b.fType
                   };
            json = JsonConvert.SerializeObject(list);
            return json;
        }
        [HttpPost]
        public string InsertBell(string title, string content,string id)
        {
            int fid = Convert.ToInt32(id);
            tNotify tn = new tNotify();
            tn.fContent = content;
            tn.fTitle = title;
            tn.fType = 0;
            tn.fTime = DateTime.Now;
            tn.fEmployeeId = fid;
            EIP_DBEntities db = new EIP_DBEntities();
            db.tNotifies.Add(tn);
            db.SaveChanges();
            var list = from n in db.tNotifies
                       where n.fEmployeeId == fid
                       select new 
                       {
                           n.fTime,
                           n.fTitle,
                           n.fContent,
                           n.fType
                       };
            string json = JsonConvert.SerializeObject(list);
            return json;
        }
        [HttpPost]
        public string UpdateBell(string id)
        {
            HttpCookie cookie = Request.Cookies["id"];
            int fEmployeeId = Convert.ToInt32(cookie.Value);
            int fid = Convert.ToInt32(id);
            EIP_DBEntities db = new EIP_DBEntities();
            var listt = (from n in db.tNotifies
                        where n.fId == fid
                        select n).FirstOrDefault();
            listt.fType = 1;
            db.SaveChanges();
            var list = from n in db.tNotifies
                       where n.fEmployeeId == fEmployeeId
                       select new 
                       {
                           n.fId,
                           n.fType,
                           n.fTitle,
                           n.fContent,
                           n.fTime
                       };
            string json = JsonConvert.SerializeObject(list);
            return json;
        }
        [HttpPost]
        public string InsertCalendar(string id, string start, string end,string title,string content)
        {
            HttpCookie cookie = Request.Cookies["id"];
            int empid = Convert.ToInt32(cookie.Value);
            EIP_DBEntities db = new EIP_DBEntities();
            string json = "";
            IQueryable list;
            if (string.IsNullOrEmpty(id)) 
            {
                tCalendar tc = new tCalendar();
                tc.fTitle = title;
                tc.fContent = content;
                tc.fStart = start;
                tc.fEnd = end;
                tc.fEmployeeId = empid;
                tc.fSort = "0";
                db.tCalendars.Add(tc);
                db.SaveChanges();
                list = from c in db.tCalendars
                       where c.fEmployeeId == tc.fEmployeeId
                       select new {
                           c.fId,
                           c.fTitle,
                           c.fStart,
                           c.fEnd,
                           c.fSort
                           };
                json = JsonConvert.SerializeObject(list);
                return json;
            }
            //現有行事曆
            int fid = Convert.ToInt32(id);
            var listu = (db.tCalendars.Where(x => x.fId == fid)).FirstOrDefault() ;
            listu.fStart = start;
            listu.fEnd = end;
            listu.fTitle = title;
            listu.fContent = content;
            db.SaveChanges();
            list = from c in db.tCalendars
                   where c.fEmployeeId == empid
                       select new 
                       {
                           c.fId,
                           c.fContent,
                           c.fTitle,
                           c.fStart,
                           c.fEnd,
                           c.fSort

                       };
            json = JsonConvert.SerializeObject(list);
            return json;
        }
        public string RemoveCalendar(string id) 
        {
            HttpCookie cookie = Request.Cookies["id"];
            int empid = Convert.ToInt32(cookie.Value);
            int fid = Convert.ToInt32(id);
            string json = "";
            EIP_DBEntities db = new EIP_DBEntities();
            var del = (from c in db.tCalendars
                       where c.fId == fid
                       select c).FirstOrDefault();
            db.tCalendars.Remove(del);
            db.SaveChanges();
            var list = from b in db.tCalendars
                   where b.fEmployeeId == empid
                   select new
                   {
                       b.fId,
                       b.fContent,
                       b.fStart,
                       b.fEnd,
                       b.fTitle,
                       b.fSort
                   };
            json = JsonConvert.SerializeObject(list);
            return json;
        }
        public string RemoveBoard(string id)
        {
            HttpCookie cookie = Request.Cookies["id"];
            int fid = Convert.ToInt32(id);
            string json = "";
            EIP_DBEntities db = new EIP_DBEntities();
            var del = (from c in db.tBillboards
                       where c.fId == fid
                       select c).FirstOrDefault();
            db.tBillboards.Remove(del);
            db.SaveChanges();
            var list = from b in db.tBillboards
                       select new
                       {
                           b.fId,
                           b.fContent,
                           b.fTitle,
                           b.fType,
                           b.fEmployeeId,
                           b.fPostTime
                       };
            json = JsonConvert.SerializeObject(list);
            return json;
        }
        public ActionResult Logout() 
        {
            if (Request.Cookies["id"] != null)
            {
                Response.Cookies["id"].Expires = DateTime.Now.AddDays(-1);
            }
            Session.Abandon();
            return RedirectToAction("Login","login");
        }
        public string getpunchtime()
        {
            HttpCookie cookie = Request.Cookies["id"];
            int empId = Convert.ToInt32(cookie.Value);
            DateTime Today = DateTime.Now;
            EIP_DBEntities db = new EIP_DBEntities();
            var record = db.tPunchtimes.AsEnumerable()
                           .Where(m => m.fDatetime.Date == Today.Date && m.fEmployeeId == empId).LastOrDefault();
            if (record != null)
                return record.fId.ToString();
            else
                return null;
        }
        public ActionResult timeoutpage() 
        {

            return View();
        }
    }
}