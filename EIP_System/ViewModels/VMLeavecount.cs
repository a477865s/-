using EIP_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EIP_System.ViewModels
{
    public class VMLeavecount
    {
        public string name { get; set; }
        public double alltime { get; set; }
        public double remaintime { get; set; }
        public double usedtime { get; set; }
        public string startdate { get; set; }
        public string enddate { get; set; }

        public VMLeavecount convert(tLeavecount lc)
        {
            VMLeavecount vmlc = new VMLeavecount();
            vmlc.name = lc.tleavesort.fLeavename;
            vmlc.alltime = lc.fAlltime;
            vmlc.remaintime = lc.fRemaintime;
            vmlc.usedtime = lc.fUesdtime;
            vmlc.startdate = (lc.fStartdate == null)? "": ((DateTime)lc.fStartdate).ToString("yyyy-MM-dd");
            vmlc.enddate = (lc.fEnddate == null) ? "" : ((DateTime)lc.fEnddate).ToString("yyyy-MM-dd");

            return vmlc;
        }

        public List<VMLeavecount> getlist(List<tLeavecount> list)
        {
            List<VMLeavecount> vmlist = new List<VMLeavecount>();
            foreach (var item in list)
            {
                vmlist.Add(new VMLeavecount().convert(item));
            }

            return vmlist;
        }
    }
}