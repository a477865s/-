using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EIP_System.ViewModels
{
    public class VMCOvertime
    {
        public int fId { get; set; }
        [DisplayName("員工編號")]
        public int fEmployeeId { get; set; }

        [DisplayName("加班類型")]
        public string fSort { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
        [DisplayName("申請日期")]
        public System.DateTime fSubmitDate { get; set; }

        [DisplayName("生效日期")]
        public System.DateTime fActiveDate { get; set; }

        [DisplayName("時數")]
        public double fTimeCount { get; set; }

        [DisplayName("原因")]
        public string fReason { get; set; }

        public int? isAgree { get; set; }

    }
}