using Skoob.Models;
namespace Skoob.Interfaces;

public interface IUserRepository
{
    public List<Mainuser> SelectUsers();
    public Mainuser? GetUserById(Guid id);
    public void UpdateUserName(Mainuser mainuser);
    public Mainuser CreateUser(Mainuser user);
    public bool UsernameExists(string username);
    public bool EmailExists(string email);
    public void UpdatePassword(Mainuser user);
    public  bool DeleteUser(Guid id);
}