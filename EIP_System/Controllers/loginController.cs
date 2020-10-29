using EIP_System.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace 期末專題.Controllers
{
    public class loginController : Controller
    {
        public class recaptcha
        {
            public string success { get; set; }
        }
        // GET: login
        public ActionResult Login()
        {
            return View();
        }
        
        [HttpPost]
        public ActionResult Login(FormCollection fc)
        {
            EIP_DBEntities db = new EIP_DBEntities();
            bool valid = false;
            if (fc["isDemo"].ToString() == "t") { valid = true; }
            else if (Request.Form["g-recaptcha-response"] == "")
            {
                ViewBag.info = "請確認是否為機器人";
                return View();
            }
            else
            {
                var req = (HttpWebRequest)HttpWebRequest.Create("https://www.google.com/recaptcha/api/siteverify");
                string posStr = "secret=6Lcay9YZAAAAAEl_1BQukIB0AJiZtrXJyONKenmu&response=" + Request.Form["g-recaptcha-response"] + "&remoteip=" + Request.Url.Host;
                byte[] byteStr = Encoding.UTF8.GetBytes(posStr);
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded";
                using (Stream streamArr = req.GetRequestStream())
                {
                    streamArr.Write(byteStr, 0, byteStr.Length);
                }
                using (var res = (HttpWebResponse)req.GetResponse())
                {
                    using (StreamReader getJson = new StreamReader(res.GetResponseStream()))
                    {
                        string json = getJson.ReadToEnd();
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        recaptcha data = js.Deserialize<recaptcha>(json);
                        valid = Convert.ToBoolean(data.success);
                    }
                }
            }
            if (valid)
            {
                try
                {
                    string acc = fc["acc"];
                    var em = (from e in db.tEmployees
                              where e.fIdent == acc
                              select new
                              {
                                  e.fEmployeeId,
                                  e.fIdent,
                                  e.fAuth,
                                  e.fTitle,
                                  e.fName,
                                  e.fDepartment,
                                  e.fPassword
                              }).FirstOrDefault();
                    if (fc["acc"].ToString() == em.fIdent && fc["pw"].ToString() == em.fPassword)
                    {
                        Session["name" + em.fEmployeeId] = em.fName;
                        Session["auth" + em.fEmployeeId] = em.fAuth;
                        Session["department" + em.fEmployeeId] = em.fDepartment;
                        Session["title" + em.fEmployeeId] = em.fTitle;
                        HttpCookie cookie = new HttpCookie("id");
                        cookie.Value = em.fEmployeeId.ToString();
                        Response.Cookies.Add(cookie);

                        return RedirectToAction("Index", "Index");
                    }
                }
                catch
                {
                    ViewBag.info = "帳號密碼錯誤";
                    return View();
                }
                ViewBag.info = "帳號密碼錯誤";
                return View();
            }
            else
            {
                ViewBag.info = "機器人驗證失敗";
                return View();
            }
           
        }
    }
}