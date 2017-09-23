using System.Threading.Tasks;
using api.Models;
using api.Utils;

namespace api.Services
{
    public class UserService : IUserService
    {
        private readonly IDataStore _store;

        public UserService(IDataStore store)
        {
            _store = store;
        }

        public Task<Option<User>> GetById(int userId)
            => _store.GetById<User>(userId);
    }
}