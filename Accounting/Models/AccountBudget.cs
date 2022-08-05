using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Accounting.Models
{
    public class AccountBudget
    {
        public string Budget { get; set; }
        public string Category { get; set; }
        public int AmountOfMoney { get; set; }
        public DateTime Date { get; set; }
        public string Comments { get; set; }
        public string SearchBy { get; set; }
        public string Text { get; set; }
    }
}
