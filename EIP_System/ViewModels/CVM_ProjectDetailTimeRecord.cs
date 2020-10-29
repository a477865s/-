using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EIP_System.Models;

namespace EIP_System.ViewModels
{
    public class CVM_ProjectDetailTimeRecord
    {
        public tProjectDetail prjDetail { get; set; }
        public tTimeRecord timeRecord { get; set; }
    }
}