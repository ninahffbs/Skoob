using Skoob.Models;
namespace Skoob.Interfaces;

public interface IUserRepository
{
    public List<Mainuser> SelectUsers();
    public Mainuser CreateUser(Mainuser user);

    public bool UsernameExists(string username);

    public bool EmailExists(string email);

}