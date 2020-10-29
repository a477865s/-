using EIP_System.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace EIP_System.ViewModels
{
    public class VMEmployee
    {
        [DisplayName("員工編號")]
        public int id { get; set; }

        [DisplayName("部門")]
        public string department { get; set; }

        [DisplayName("職稱")]
        public string title { get; set; }

        [DisplayName("姓名")]
        public string name { get; set; }

        public int auth { get; set; }

        public VMEmployee convert(tEmployee emp)
        {
            VMEmployee vmemp = new VMEmployee();
            vmemp.id = emp.fEmployeeId;
            vmemp.department = emp.fDepartment;
            vmemp.title = emp.fTitle;
            vmemp.name = emp.fName;
            vmemp.auth = emp.fAuth;

            return vmemp;
        }
        public List<VMEmployee> getlist(List<tEmployee> emplist)
        {
            List<VMEmployee> vmlist = new List<VMEmployee>();
            foreach (var emp in emplist)
            {
                vmlist.Add(new VMEmployee().convert(emp));
            }

            return vmlist;
        }
    }
}