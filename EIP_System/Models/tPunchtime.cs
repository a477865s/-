//------------------------------------------------------------------------------
// <auto-generated>
//     這個程式碼是由範本產生。
//
//     對這個檔案進行手動變更可能導致您的應用程式產生未預期的行為。
//     如果重新產生程式碼，將會覆寫對這個檔案的手動變更。
// </auto-generated>
//------------------------------------------------------------------------------

namespace EIP_System.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class tPunchtime
    {
        public int fId { get; set; }
        public System.DateTime fDatetime { get; set; }
        public int fEmployeeId { get; set; }
        public string fstatus { get; set; }
        public string fPunchclass { get; set; }
        public Nullable<decimal> fLatitude { get; set; }
        public Nullable<decimal> fLongitude { get; set; }
    
        public virtual tEmployee tEmployee { get; set; }
    }
}
