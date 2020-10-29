using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EIP_System.ViewModels
{
    public class VMLeave
    {
        //請假資訊
        [Required]
        [DisplayName("假別")]
        public string leavesort { get; set; }

        [Required]
        [DisplayName("起始時間")]
        [DisplayFormat(ApplyFormatInEditMode = true)]
        public DateTime start { get; set; }

        [Required]
        [DisplayName("結束時間")]
        [DisplayFormat(ApplyFormatInEditMode = true)]
        public DateTime end { get; set; }

        [DisplayName("時數計算(單位/小時)")]
        public double timecount { get; set; }

        [DisplayName("事由")]
        public string reason { get; set; }

        [Required]
        [DisplayName("審核主管")]
        public int supervisorId { get; set; }

    }
}