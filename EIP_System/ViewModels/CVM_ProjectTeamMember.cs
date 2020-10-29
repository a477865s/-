using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EIP_System.Models;

namespace EIP_System.ViewModels
{ 
    public class CVM_ProjectTeamMember
    {

        public tProject project { get; set; }
        public List<tTeamMember> members { get; set; }
    }
}