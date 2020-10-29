using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace EIP_System.ViewModels
{
    public class VMPunchtimeApply
    {
        public int applyId { get; set; }

        [DisplayName("打卡型別")]
        public string sort { get; set; }

        [DisplayName("事由")]
        public string reason { get; set; }

        [DisplayName("審核主管")]
        public int supervisorId { get; set; }
    }
}