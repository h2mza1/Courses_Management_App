using Courses_App.Core.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;


namespace Corses_App.Data.Repostory
{
    public interface ICategeoryRepostory
    {
        Task<CategeoryReadDto> AddCategeory(CategeoryCreateDto categeory);
        Task<List<CategeoryReadDto>?> GetCategeories();
        Task<CategeoryReadDto?> AddCategeoryAsync(CategeoryCreateDto categeory);
        Task<CategeoryReadDto?> UpdateCategory(CategeoryCreateDto categeory , int id); 
        Task<bool?> Delete(int id);
        Task<CategeoryReadDto?> GetCategeoryById(int id);
        Task<int> GetCategoriesCountAsync();


    }
}
