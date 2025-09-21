using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Courses_App.Core.Models
{
    public class Course
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int InstructorId { get; set; }
        [Range(1, 5, ErrorMessage = "Rate must be between 1 and 5")]
        public float Rate { get; set; } = 1f;
        public string? Image {  get; set; } = string.Empty;
        public Instructor? Instructor { get; set; }
        public decimal Price { get; set; } = decimal.Zero;
        public bool IsActive { get; set; } = true;
        public DateTime StartDate { get; set; } = DateTime.MinValue;
        public DateTime EndDate { get; set; } = DateTime.MinValue;
        private DateTime SafeAddMonths(DateTime date, int months)
        {
            int newMonth = date.Month + months;
            int newYear = date.Year + (newMonth - 1) / 12;
            newMonth = ((newMonth - 1) % 12) + 1;

            int daysInNewMonth = DateTime.DaysInMonth(newYear, newMonth);
            int newDay = Math.Min(date.Day, daysInNewMonth);

            return new DateTime(newYear, newMonth, newDay);
        }
        public string DurationDays
        {
            get
            {
                // فرق الأيام
                int totalDays = (EndDate - StartDate).Days;

                if (totalDays <= 0) return "0 day";

                // نحسب الأشهر
                int totalMonths = ((EndDate.Year - StartDate.Year) * 12) + EndDate.Month - StartDate.Month;
                if (EndDate.Day < StartDate.Day)
                    totalMonths--;

                // تاريخ نهاية الأشهر المحسوبة (آمن)
                DateTime approxMonthEnd = SafeAddMonths(StartDate, totalMonths);

                // باقي الأيام
                int remainingDays = (EndDate - approxMonthEnd).Days;

                // نحسب الأسابيع
                int weeks = remainingDays / 7;
                int days = remainingDays % 7;

                // صياغة النتيجة
                string result = $"{totalMonths} month";
                if (weeks > 0) result += $" and {weeks} week";
                if (days > 0) result += $" and {days} day";

                return result;
            }
            
        }
        public bool IsDeleted { get; set; }= false;
        public virtual ICollection<Enrollment>? Enrollments { get; set; }
        public virtual ICollection<CourseVideos> Videos { get; set; } = new List<CourseVideos>();
        [ForeignKey(nameof(Categeory))]
        public int? CategeoryId { get; set; }
        public Categeory Categeory { get; set; }
        public Course()
        {
            IsActive = StartDate <= DateTime.Now;

        }
    }
}
