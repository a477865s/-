using EIP_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EIP_System.ViewModels
{
    public class VMPunchtime
    {
        public int id { get; set; }
        public string title{ get; set; }
        public string description { get; set; }
        public string start { get; set; }
        public int displayOrder { get; set; }
        public string color { get; set; }

        public VMPunchtime convert(tPunchtime p)
        {
            VMPunchtime vmp = new VMPunchtime();
            vmp.id = p.fId;
            vmp.title = p.fstatus;
            vmp.description = p.fstatus + " 打卡時間為:" + p.fDatetime.ToString("yyyy-MM-dd HH:mm:ss");
            vmp.start = p.fDatetime.ToString("yyyy-MM-dd");
            vmp.displayOrder = p.fId;

            //bg color
            if (p.fstatus == "遲到") vmp.color = "#ff9800";
            else if (p.fstatus == "未打卡") vmp.color = "#d92534";
            else if (p.fstatus == "已補打卡") vmp.color = "#00bcd4";
            else vmp.color = "#2196f3"; //上班、下班

            return vmp;
        }
        public List<VMPunchtime> getlist(List<tPunchtime> list)
        {
            List<VMPunchtime> vmlist = new List<VMPunchtime>();
            
            foreach (tPunchtime item in list)
            {
                vmlist.Add((new VMPunchtime()).convert(item));
            }

            return vmlist;
        }
    }
}