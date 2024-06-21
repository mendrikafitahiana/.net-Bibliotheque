
namespace Bibtheque.Services
{
    public class UserService : IUserService
    {
        public bool IsUserLoggedIn(HttpContext context)
        {
            return context.User.Identity.IsAuthenticated;
        }
    }
}
