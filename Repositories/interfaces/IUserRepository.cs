using Skoob.Models;
namespace Skoob.Interfaces;

public interface IUserRepository
{
    public List<Mainuser> SelectUsers();
    public Mainuser? GetUserById(Guid id);
    public string UpdateUserName(Guid id, string newName);
    public Mainuser CreateUser(Mainuser user);
    public bool UsernameExists(string username);
    public bool EmailExists(string email);
}