using EIP_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EIP_System.ViewModels
{
    public class CVM_TaskTeamMember
    {
        public tProjectDetail projectDetail { get; set; }

        public int[] empId { get; set; }
        //public List<tTeamMember> members { get; set; }
    }
}