using EIP_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EIP_System.ViewModels
{
    public class CVM_BudgetLevel
    {
        public tBudget budget { get; set; }
        public List<tLevel> levels { get; set; }
    }
}