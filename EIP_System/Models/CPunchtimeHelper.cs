using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web;

namespace EIP_System.Models
{
    public class CPunchtimeHelper
    {
        EIP_DBEntities db = new EIP_DBEntities();

        DateTime Today;
        //設定打卡上下班時段
        TimeSpan timespan_上班0600 = new TimeSpan(6, 0, 0);
        TimeSpan timespan_上班0900 = new TimeSpan(9, 0, 0);

        TimeSpan timespan_下班1700 = new TimeSpan(17, 0, 0);
        TimeSpan timespan_下班2200 = new TimeSpan(22, 0, 0); //下班 17:00 單日加班不超過4h


        public (string msg_type, string msg) punchtime(int empId, DateTime now, bool demo)
        {
            //上班時間09:00 下班時間17:00
            //取今天日期 or Demo 日期
            Today = demo ? now.Date : DateTime.Today;

            //組合
            DateTime Today_上班上 = Today + timespan_上班0600;
            DateTime Today_上班下 = Today + timespan_上班0900;

            DateTime Today_下班上 = Today + timespan_下班1700;
            DateTime Today_下班下 = Today + timespan_下班2200;

            //判斷打卡上下班時段，重複打卡不紀錄至SQL
            string msg_type = ""; string msg = "";

            //今天該員工的最後一筆打卡紀錄
            var record = db.tPunchtimes.AsEnumerable()
                            .Where(m => m.fDatetime.Date == Today.Date && m.fEmployeeId == empId).LastOrDefault();

            if (Today_上班上 <= now && now <= Today_上班下)
            {
                if (record != null)
                {
                    msg_type = "info";
                    msg = "您已打過上班卡了，紀錄為: " + record.fDatetime.ToString("yyyy-MM-dd HH:mm:ss") + " 上班簽到";
                }
                else
                {

                    msg_type = "success";
                    msg = "上班打卡成功，紀錄為: " + now.ToString("yyyy-MM-dd HH:mm:ss") + " 上班簽到";
                    savePunchtime(empId, "上班", now);
                }
            }
            else if (Today_下班上 <= now && now <= Today_下班下)
            {
                if (record != null && record.fstatus == "下班")
                {
                    msg_type = "info";
                    msg = "您已打過下班卡了，紀錄為: " + record.fDatetime.ToString("yyyy-MM-dd HH:mm:ss") + " 下班簽到";
                }
                else
                {
                    msg_type = "success";
                    msg = "下班打卡成功，紀錄為: " + now.ToString("yyyy-MM-dd HH:mm:ss") + " 下班簽到";
                    savePunchtime(empId, "下班", now);
                }
            }
            else if (Today_上班下 <= now && now <= Today_下班上)
            {
                if (record != null)
                {
                    msg_type = "info";
                    msg = "您已打過卡了，紀錄為: " + record.fDatetime.ToString("yyyy-MM-dd HH:mm:ss") + " 上班遲到";
                }
                else
                {
                    msg_type = "success";
                    msg = "打卡成功，紀錄為: " + now.ToString("yyyy-MM-dd HH:mm:ss") + " 遲到";
                    savePunchtime(empId, "遲到", now);
                }
            }
            else 
            {
                msg_type = "error";
                msg = "在非打卡時段，不能打卡!!!";
            }

            return (msg_type, msg);

        }
        //儲存打卡
        private void savePunchtime(int empId, string status, DateTime now)
        {
            tPunchtime p = new tPunchtime();

            p.fEmployeeId = empId;
            p.fstatus = status;
            p.fDatetime = now;
            p.fPunchclass = "一般打卡";

            db.tPunchtimes.Add(p);
            db.SaveChanges();
        }

        //載入打卡紀錄(謹慎使用)
        public void InsertPunchtime(int empId, DateTime startdate , DateTime enddate)
        {
            //開始日期到今天
            while (!(startdate.Date == enddate.Date))
            {
                if (startdate.DayOfWeek != DayOfWeek.Saturday && startdate.DayOfWeek != DayOfWeek.Sunday)
                {
                    //上班
                    savePunchtime(empId, "上班", startdate + timespan_上班0900);
                    //下班
                    savePunchtime(empId, "下班", startdate + timespan_下班1700);

                }
                startdate = startdate.AddDays(1);
            }
            db.SaveChanges();
        }

        //載入空的打卡紀錄(謹慎使用)
        public void InsertNullPunchtime(int empId)
        {
            //取該員工三天前卡紀錄 
            DateTime startdate = Today.Subtract(new TimeSpan(72,0,0));

            //補上未打卡紀錄到列表，直接存入資料庫
            while (!(startdate > Today))
            {
                //過濾假日
                if (startdate.DayOfWeek != DayOfWeek.Saturday && startdate.DayOfWeek != DayOfWeek.Sunday)
                {
                    //一天需上下班兩筆記錄
                    var day_records = db.tPunchtimes.AsEnumerable()
                        .Where(m => m.fEmployeeId == empId
                        && m.fDatetime.Date == startdate.Date)
                        .ToList();

                    if (day_records.Count == 1)
                    {
                        //當天第一次打卡不需補，但上班未打卡需要補
                        if (day_records[0].fDatetime.Date == startdate.Date)
                        {
                            //上班未打卡
                            if (day_records.Exists(m => m.fstatus == "下班"))
                                savePunchtime(empId, "未打卡", startdate + timespan_上班0900);
                        }
                        else
                        {
                            //找是缺哪一種
                            if (day_records.Exists(m => m.fstatus == "上班"))
                            {
                                //下班未打卡
                                savePunchtime(empId, "未打卡", startdate + timespan_下班1700);
                            }
                            else if (day_records.Exists(m => m.fstatus == "遲到"))
                            {
                                //下班未打卡
                                savePunchtime(empId, "未打卡", startdate + timespan_下班1700);
                            }
                            else
                            {
                                //上班未打卡
                                savePunchtime(empId, "未打卡", startdate + timespan_上班0900);
                            }
                        }
                    }
                    else if (day_records.Count == 0)//補兩筆
                    {
                        //上班未打卡
                        savePunchtime(empId, "未打卡", startdate + timespan_上班0900);

                        //下班未打卡
                        savePunchtime(empId, "未打卡", startdate + timespan_下班1700);
                    }
                }

                startdate = startdate.AddDays(1);
            }

        }


    }
}