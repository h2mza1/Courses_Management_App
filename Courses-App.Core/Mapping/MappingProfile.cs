using AutoMapper;
using Courses_App.Core.DTO;
using Courses_App.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Courses_App.Core.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserReadDTO>();

            // من Course إلى CourseReadDTO
            CreateMap<Course, CourseReadDTO>();

            // من Instructor إلى InstructorReadDto
            CreateMap<Instructor, InstructorReadDto>();

            // من Category إلى CategoryReadDto
            CreateMap<Categeory, CategeoryReadDto>();
        }
    }
}
