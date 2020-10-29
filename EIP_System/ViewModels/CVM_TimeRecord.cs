using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EIP_System.Models;

namespace EIP_System.ViewModels
{
    public class CVM_TimeRecord
    {
        public int timeRecordId { get; set; }
        public string date { get; set; }
        public int employeeId { get; set; }
        public string employeeName { get; set; }
        public int projectId { get; set; }
        public string projectName { get; set; }
        public string levelName { get; set; }
        //public int projectDetailId { get; set; }
        public string taskName { get; set; }
        public int time { get; set; }
        public string approve { get; set; }


        public List<CVM_TimeRecord> getList(List<tTimeRecord> recordList)
        {
            List<CVM_TimeRecord> list = new List<CVM_TimeRecord>();

            foreach (var item in recordList)
            {
                CVM_TimeRecord record = new CVM_TimeRecord
                {
                    timeRecordId = item.fTimeRecordId,
                    date = item.fDate.ToString("yyyy-MM-dd"),
                    employeeId = item.fEmployeeId,
                    employeeName = item.tEmployee.fName,
                    projectId = item.fProjectId,
                    projectName = item.tProject.fProjectName,
                    levelName = item.tProjectDetail.tLevel.fLevelName,
                    taskName = item.tProjectDetail.fTaskName,
                    time = item.fTime,
                    approve = item.fApprove,
                };
                list.Add(record);
            }
            return list;
        }


        //public List<CVM_TimeRecord> getList(List<tTimeRecord> recordList)
        //{
        //    List<CVM_TimeRecord> list = new List<CVM_TimeRecord>();

        //    foreach (var item in recordList)
        //    {
        //        list.Add(new CVM_TimeRecord()
        //        {
        //            timeRecordId = item.fTimeRecordId,
        //            date = item.fDate.ToString("yyyy-MM-dd"),
        //            employeeId = item.fEmployeeId,
        //            employeeName = item.tEmployee_c.fName,
        //            projectId = item.fProjectId,
        //            projectName = item.tProject.fProjectName,
        //            levelName = item.tProjectDetail.tLevel.fLevelName,
        //            taskName = item.tProjectDetail.fTaskName,
        //            time = item.fTime,
        //            approve = item.fApprove,
        //        });
        //    }
        //    return list;
        //}
    }
}