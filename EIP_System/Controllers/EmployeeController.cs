using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using EIP_System.Models;
//人員管理權限只有人資部的人才可以看到
//如果其他部門的人會導到其他頁面

namespace EIP_System.Controllers
{
    public class EmployeeController : Controller
    {
        EIP_DBEntities db = new EIP_DBEntities();
        // GET: Employee
        public ActionResult Index()
        {
            HttpCookie cookie = Request.Cookies["id"];
            int u = Convert.ToInt32(cookie.Value);
            int fakeid = u;
            //查詢部門別及權限用來判斷可以看甚麼
            var Check = (from b in db.tEmployees
                         where b.fEmployeeId == fakeid
                         select new { b.fDepartment, b.fAuth }).FirstOrDefault();
            //先把物件取出來，登入者的資訊物件

            //這個是登入者的部門
            string department = Check.fDepartment.ToString();
            if (department == "資訊部")
            {
                return View();
            }            
            //你不是人資部，所以你不能看到員工的秘密
            else
            {
                return RedirectToAction("UserIsNotHR", "Employee");                
            }
        }
        public int LBI(int LB)
        {
            if (LB <= 23800)
            {
                return 524;
            }
            else if(LB<=24000)
            {
                return 528;
            }
            else if (LB <= 25200)
            {
                return 554;
            }
            else if (LB <= 26400)
            {
                return 581;
            }
            else if (LB <= 27600)
            {
                return 607;
            }
            else if (LB <= 28800)
            {
                return 634;
            }
            else if (LB <= 30300)
            {
                return 667;
            }
            else if (LB <= 31800)
            {
                return 700;
            }
            else if (LB <= 33000)
            {
                return 733;
            }
            else if (LB <= 34800)
            {
                return 766;
            }
            else if (LB <= 36300)
            {
                return 799;
            }
            else if (LB <= 38200)
            {
                return 840;
            }
            else if (LB <= 40100)
            {
                return 882;
            }
            else if (LB <= 42000)
            {
                return 924;
            }
            else if (LB <= 43900)
            {
                return 966;
            }
            else 
            {
                return 1008;
            }
        }
        public int HI(int HI)
        {
            if (HI <= 23800)
            {
                return 900;
            }
            else if (HI <= 28800)
            {
                return 1200;
            }
            else if (HI <= 36300)
            {
                return 1500;
            }
            else if (HI <= 45800)
            {
                return 1900;
            }
            else if (HI <= 57800)
            {
                return 2400;
            }
            else if (HI <= 72800)
            {
                return 3000;
            }
            else if (HI <= 87600)
            {
                return 3700;
            }
            else if (HI <= 110100)
            {
                return 4500;
            }
            else if (HI <= 150000)
            {
                return 5400;
            }
            else
            {
                return 6400;
            }

        }
        public ActionResult UserIsNotHR()
        {
            return View();
        }
        public ActionResult Getdata()
        {
             var hello = from p in db.tEmployees
                            select new
                            {
                                fEmployeeId = p.fEmployeeId,
                                fName = p.fName,
                                fIdent = p.fIdent,
                                fPassword = p.fPassword.Substring(0, 1) + "****" + p.fPassword.Substring(p.fPassword.Length - 1, 1),
                                fDepartment = p.fDepartment,
                                fTitle = p.fTitle,
                                fEmail = p.fEmail,
                                fBirth = p.fBirth,
                                fPhonePersonal = p.fPhonePersonal,
                                fState = p.fState,
                                fAuth = p.fAuth,
                            };
                return Json(new { data = hello.ToList() }, JsonRequestBehavior.AllowGet);
        }        

        public ActionResult Create(tEmployee target)
        {            
            if (target.fEmployeeId > 0)
            {
                //修改
                DateTime Now = DateTime.Now;
                tEmployee updateEmp = db.tEmployees.Where(emp => emp.fEmployeeId == target.fEmployeeId).FirstOrDefault();
                //updateEmp.fEmployeeId = target.fEmployeeId;
                updateEmp.fName = target.fName;
                updateEmp.fIdent = target.fIdent;
                updateEmp.fPassword = target.fPassword;
                updateEmp.fDepartment = target.fDepartment;
                updateEmp.fTitle = target.fTitle;
                updateEmp.fEmail = target.fEmail;
                updateEmp.fBirth = target.fBirth;
                updateEmp.fPhonePersonal = target.fPhonePersonal;
                updateEmp.fHireDate = target.fHireDate;
                updateEmp.fState = target.fState;
                updateEmp.fAuth = Convert.ToInt32(target.fAuth);
                updateEmp.fSalaryMonth = target.fSalaryMonth;
                updateEmp.fSalaryHour = target.fSalaryMonth / 30 / 8;
                updateEmp.fPhoneHouse = target.fPhoneHouse;
                updateEmp.fCountry = target.fCountry;
                updateEmp.fBirthPlace = target.fBirthPlace;
                updateEmp.fEducation = target.fEducation;
                updateEmp.fAddressNow = target.fAddressNow;
                updateEmp.fAddressOrigin = target.fAddressOrigin;
                updateEmp.fEngyName = target.fEngyName;
                updateEmp.fEngyPhone = target.fEngyPhone;
                TimeSpan fOld = Now.Subtract(updateEmp.fHireDate);
                updateEmp.fOld = Math.Round(fOld.TotalDays, 1);
                updateEmp.fFireDate = target.fFireDate;
                updateEmp.fBackDate = target.fBackDate;
                updateEmp.fLBI = LBI(target.fSalaryMonth);
                updateEmp.fHI = HI(target.fSalaryMonth);
                db.SaveChanges();
            
            }
            else
            {
                //身分證驗證(資料倒入)
                
                if (target.fName == null)
                {
                    return Json("empty", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    string idcheck = target.fIdent;
                    bool regId = Regex.IsMatch(idcheck, @"^[A-Za-z]{1}[1-2]{1}[0-9]{8}$");
                    //密碼驗證(資料倒入)///////
                    string pwdcheck = target.fPassword;
                    bool regPwd = Regex.IsMatch(pwdcheck, @"^.*(?=.{6,})(?=.*\d)(?=.*[a-zA-Z]).*$");
                    if(regId && regPwd)
                    {
                        //新增
                        DateTime Now = DateTime.Now;
                        tEmployee emp = new tEmployee();
                        //emp.fEmployeeId = target.fEmployeeId;
                        emp.fName = target.fName;
                        emp.fName = target.fName;
                        emp.fIdent = target.fIdent;
                        emp.fPassword = target.fPassword;
                        emp.fDepartment = target.fDepartment;
                        emp.fTitle = target.fTitle;
                        emp.fEmail = target.fEmail;
                        emp.fBirth = target.fBirth;
                        emp.fPhonePersonal = target.fPhonePersonal;
                        emp.fHireDate = target.fHireDate;
                        emp.fState = target.fState;
                        emp.fAuth = Convert.ToInt32(target.fAuth);
                        emp.fSalaryMonth = target.fSalaryMonth;
                        emp.fSalaryHour = target.fSalaryMonth / 30 / 8;
                        emp.fPhoneHouse = target.fPhoneHouse;
                        emp.fCountry = target.fCountry;
                        emp.fBirthPlace = target.fBirthPlace;
                        emp.fEducation = target.fEducation;
                        emp.fAddressNow = target.fAddressNow;
                        emp.fAddressOrigin = target.fAddressOrigin;
                        emp.fEngyName = target.fEngyName;
                        emp.fEngyPhone = target.fEngyPhone;
                        TimeSpan fOld = Now.Subtract(emp.fHireDate);
                        emp.fOld = Math.Round(fOld.TotalDays, 1);
                        emp.fFireDate = target.fFireDate;
                        emp.fBackDate = target.fBackDate;
                        emp.fLBI = LBI(target.fSalaryMonth);
                        emp.fHI =  HI(target.fSalaryMonth);
                        db.tEmployees.Add(emp);
                        db.SaveChanges();
                        
                    }
                    else
                    {
                    return Json("error", JsonRequestBehavior.AllowGet);                    
                    }                
                }
            }

            return Json("success", JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult GetEdit(int id)
        {
            var Monday = from p in db.tEmployees.Where(m => m.fEmployeeId == id)
                         select new
                         {
                             p.fEmployeeId,
                             p.fName,
                             p.fIdent,
                             p.fPassword,
                             p.fDepartment,
                             p.fTitle,
                             p.fEmail,
                             p.fBirth,
                             p.fPhonePersonal,
                             p.fHireDate,
                             p.fState,
                             p.fAuth,
                             p.fCountry,
                             p.fBirthPlace,
                             p.fSalaryMonth,
                             p.fEducation,
                             p.fPhoneHouse,
                             p.fAddressNow,
                             p.fAddressOrigin,
                             p.fEngyName,
                             p.fEngyPhone,
                         };
            //var emp = db.tEmployees.Where(m => m.fEmployeeId == id).FirstOrDefault();


            return Json(new { data = Monday.ToList() }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Delete(int id)
        {
            var del = db.tEmployees.Where(m => m.fEmployeeId == id).FirstOrDefault();
            db.tEmployees.Remove(del);
            db.SaveChanges();

            return Json("success", JsonRequestBehavior.AllowGet);
        }

    }
}