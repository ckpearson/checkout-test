using System.Threading.Tasks;
using api.Models;
using api.Utils;

namespace api.Services
{
    public interface IUserService
    {
        Task<Option<User>> GetById(int userId);
    }
}