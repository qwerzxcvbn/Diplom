using System;
using System.Linq;
using System.Text.RegularExpressions;
using StuldastolClient.Services;

namespace StuldastolClient.Services
{
    public class UserService : IUserService
    {
        public bool AuthenticateUser(string login, string password)
        {
            using (var context = new StuldastolEntities())
            {
                var user = context.Users.FirstOrDefault(u => u.Login == login && u.PasswordHash == password);
                if (user != null)
                {
                    CurrentUser.SetUser(user.Id, user.Login, user.Email);
                    return true;
                }
                return false;
            }
        }

        public void RegisterUser(string fullName, string phone, string email, string login, string password)
        {
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(phone) ||
                string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(login) ||
                string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Все поля должны быть заполнены.");
            }

            if (!Regex.IsMatch(phone, @"^[\d\s+()-]+$"))
            {
                throw new ArgumentException("Некорректный формат номера телефона.");
            }

            using (var context = new StuldastolEntities())
            {
                var user = new Users
                {
                    Login = login,
                    Email = email,
                    PasswordHash = password,
                    FirstName = fullName.Split(' ')[0],
                    LastName = fullName.Contains(' ') ? fullName.Split(' ')[1] : "",
                    Phone = phone,
                    RoleId = 1, // Customer
                    CreatedAt = DateTime.Now
                };
                context.Users.Add(user);
                context.SaveChanges();
            }
        }
        public bool IsEmailTaken(string email)
        {
            using (var context = new StuldastolEntities())
            {
                return context.Users.Any(u => u.Email == email);
            }
        }

        public int GetUserRole(int userId)
        {
            using (var context = new StuldastolEntities())
            {
                return context.Users.FirstOrDefault(u => u.Id == userId)?.RoleId ?? 0;
            }
        }
    }
}