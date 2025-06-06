using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StuldastolProduction
{
    public static class CurrentUser
    {
        public static int UserId { get; private set; }
        public static string Login { get; private set; }
        public static string Email { get; private set; }

        public static void SetUser(int userId, string login, string email)
        {
            UserId = userId;
            Login = login;
            Email = email;
        }

        public static void ClearUser()
        {
            UserId = 0;
            Login = null;
            Email = null;
        }
    }
}
