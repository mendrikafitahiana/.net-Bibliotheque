namespace Bibtheque.Services
{
    public interface IUserService
    {
        bool IsUserLoggedIn(HttpContext context);
    }
}
