using APIFull.Models;

namespace APIFull.Repositories
{
    public static class UserRepository
    {
        public static User Get(string userName, string passWord)
        {
            var users = new List<User>{
                new User {Id = 1, UserName = "batman", PassWord = "batman", Role = "manager"},
                new User {Id = 2, UserName = "robin", PassWord = "robin", Role = "employee"}
            };

            // return users.Where(x => x.UserName.ToLower() == userName.ToLower() && x.PassWord == passWord).FirstOrDefault();
            return users.FirstOrDefault( x =>
                x.UserName.ToLower() == userName.ToLower() &&
                x.PassWord == passWord
            );
        }
    }
}