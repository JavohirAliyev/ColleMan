using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace ColleMan.Interfaces
{
    public interface ICloud
    {
        Task<string> UploadImageAsync(IFormFile image, string imageName);
        Task DeleteImageAsync(string imageName);
    }
}
