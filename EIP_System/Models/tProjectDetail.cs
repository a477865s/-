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
    
    public partial class tProjectDetail
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tProjectDetail()
        {
            this.tTimeRecords = new HashSet<tTimeRecord>();
        }
    
        public int fProjectDetailId { get; set; }
        public int fProjectId { get; set; }
        public Nullable<int> fLevelId { get; set; }
        public string fTaskName { get; set; }
        public Nullable<int> fEmployeeId { get; set; }
        public string fStatus { get; set; }
        public Nullable<System.DateTime> fStartTime { get; set; }
        public Nullable<System.DateTime> fDeadline { get; set; }
        public Nullable<int> fTimes { get; set; }
        public string fRemarks { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tTimeRecord> tTimeRecords { get; set; }
        public virtual tEmployee tEmployee { get; set; }
        public virtual tProject tProject { get; set; }
        public virtual tLevel tLevel { get; set; }
    }
}
