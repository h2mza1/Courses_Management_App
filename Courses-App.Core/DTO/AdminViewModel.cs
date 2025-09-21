using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Courses_App.Core.DTO
{
    public class AdminViewModel
    {
        public IEnumerable<HistoryDTO>? Courses { get; set; }
        public IEnumerable<HistoryDTO>? Instructors { get; set; }

    }
}
