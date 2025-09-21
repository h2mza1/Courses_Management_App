﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Courses_App.Core.Models
{
    public class Categeory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Icon {  get; set; }
        public bool IsDeleted { get; set; } = false;
        public virtual ICollection<Course> Courses { get; set; }


    }
}
