using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EIP_System.ViewModels
{
    public class VMSalaryCalculate
    {
        [DisplayName("加班日期")]
        [DisplayFormat(DataFormatString ="{0:yyyy 年 MM 月 dd 日}")]
        public DateTime OverTimeActiveDate { get; set; }


        [DisplayName("請假日期")]
        [DisplayFormat(DataFormatString = "{0:yyyy 年 MM 月 dd 日}")]
        public DateTime LeaveTimeActiveDate { get; set; }


        [DisplayName("種類")]
        public string fSort { get; set; }


        [DisplayName("本薪")]
        public int fSalary { get; set; }


        [DisplayName("加班加班費")]
        public double fOverTimeMoney { get; set; }


        [DisplayName("請假扣薪")]
        public double? fAttendMoney { get; set; }


        [DisplayName("勞保")]
        public int fLBI { get; set; }


        [DisplayName("健保")]
        public int fHI { get; set; }


        [DisplayName("合計")]
        public double total { get; set; }



    }
}