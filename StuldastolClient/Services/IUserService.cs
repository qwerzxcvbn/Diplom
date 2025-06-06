using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StuldastolClient.Services
{
    public interface IUserService
    {
        bool AuthenticateUser(string login, string password);
        void RegisterUser(string fullName, string phone, string email, string login, string password);
        int GetUserRole(int userId);
        bool IsEmailTaken(string email);
    }
}
