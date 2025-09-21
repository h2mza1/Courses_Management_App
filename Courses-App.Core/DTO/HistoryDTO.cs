using System;

namespace Courses_App.Core.DTO
{
    public class HistoryDTO
    {
        public int Id { get; set; }                  // Id للعنصر
        public string Name { get; set; } = "";      // اسم العنصر أو العنوان
        public string Description { get; set; } = "";// وصف العنصر (اختياري)
        public string EntityType { get; set; } = ""; // نوع الكيان (Course, User, News, ...)
        public DateTime? DeletedAt { get; set; }    // تاريخ الحذف (اختياري)
        public string ExtraInfo { get; set; } = "";
        public string? UserId { get; set; } = "";
    }
}
