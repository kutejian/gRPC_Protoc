using WebApplication1.Protos;

namespace WebApplication1.Entity
{
    public class UserData
    {
        public static List<User> UserAll { get; set; } = new List<User>()
        {
            new User
            {
                Id = 1,
                Age = 9,
                Name = "yaokang",
                LastName = "yao",
                Salary =2200
            },
            new User
            {
                Id = 2,
                Age = 10,
                Name = "congyu",
                LastName = "cong",
                Salary =3300
            },
            new User
            {
                Id = 3,
                Age = 16,
                Name = "kute",
                LastName = "ku",
                Salary =4400
            }
        };

    }
}
