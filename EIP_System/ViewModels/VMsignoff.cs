using EIP_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EIP_System.ViewModels
{
    public class VMsignoff
    {
        public int id { get; set; }
        public int emp_id { get; set; }
        public string emp_name { get; set; }
        public string catelog { get; set; }
        public string applyclass { get; set; }
        public string reason { get; set; }
        public string applydate { get; set; }
        public string activedate { get; set; }
        public string enddate { get; set; }
        public string expireddate { get; set; }
        public string passdate { get; set; }
        public string supervisor { get; set; }
        public int? isagreed { get; set; }
        public int? revoke { get; set; }


        public VMsignoff convert(tSignoff tSignoff)
        {
            VMsignoff vmsignoff = new VMsignoff();
            //大項目判斷
            string catelogName = "";
            int id = 0;
            string name = "";
            string reason = "";
            string applydate = "";
            string expireddate = "";
            if (tSignoff.fLeaveId != null)
            {
                catelogName = "請假申請";
                id = tSignoff.tLeave.tEmployee.fEmployeeId;
                name = tSignoff.tLeave.tEmployee.fName;
                reason = tSignoff.tLeave.fReason;
                applydate = tSignoff.tLeave.fApplyDate.AddHours(8).ToString("yyyy-MM-dd HH:mm");
                expireddate = tSignoff.tLeave.fActiveDate.ToString("yyyy-MM-dd HH:mm");
            }
            else if (tSignoff.fOvertimeId != null)
            {
                catelogName = "加班申請";
                id = tSignoff.tOvertime.tEmployee.fEmployeeId;
                name = tSignoff.tOvertime.tEmployee.fName;
                reason = tSignoff.tOvertime.fReason;
                applydate = tSignoff.tOvertime.fSubmitDate.AddHours(8).ToString("yyyy-MM-dd HH:mm");
                expireddate = tSignoff.tOvertime.fActiveDate.ToString("yyyy-MM-dd HH:mm");
            }
            else
            {
                catelogName = "補打卡申請";
                id = tSignoff.tApplypunch.tEmployee.fEmployeeId;
                name = tSignoff.tApplypunch.tEmployee.fName;
                reason = tSignoff.tApplypunch.fReason;
                applydate = tSignoff.tApplypunch.fApplyDate.AddHours(8).ToString("yyyy-MM-dd HH:mm");
                expireddate = tSignoff.fEnddate.ToString("yyyy-MM-dd HH:mm");
            }

            vmsignoff.id = tSignoff.fId;
            vmsignoff.emp_id = id;
            vmsignoff.emp_name = name;
            vmsignoff.catelog = catelogName;
            vmsignoff.applyclass = tSignoff.fApplyClass;
            vmsignoff.reason = reason;
            vmsignoff.applydate = applydate;
            vmsignoff.activedate = tSignoff.fStartdate.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
            vmsignoff.enddate = tSignoff.fEnddate.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
            vmsignoff.expireddate = expireddate;
            vmsignoff.passdate = (tSignoff.fPassdate != null) ? ((DateTime)tSignoff.fPassdate).ToLocalTime().ToString("yyyy-MM-dd HH:mm") : "null";
            vmsignoff.supervisor = tSignoff.tEmployee.fName;
            vmsignoff.isagreed = tSignoff.fIsAgreed;

            return vmsignoff;
        }

        public List<VMsignoff> getList(List<tSignoff> signofflist)
        {
            List<VMsignoff> list = new List<VMsignoff>();
            
            foreach (var item in signofflist)
            {
                list.Add(new VMsignoff().convert(item));
            }

            return list;
        }
    }
}