using Courses_App.Core.DTO;
using Courses_App.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Corses_App.Data.Repostory
{
    public interface IVideoReostory 
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="courseId"></param>
        /// <returns>return all vedios on course</returns>
        Task <List<CourseVideoDto>?> GetVideos (int courseId);
        Task <CourseVideos?>AddVideo (CourseVideos courseVideos);
        Task<bool> DeleteVideo (int videoId);
        Task <CourseVideos?> UpdateVideoInfo(VideoDto courseVideos);
        Task <CourseVideos?> GetVideoById (int videoId);

    }
}
