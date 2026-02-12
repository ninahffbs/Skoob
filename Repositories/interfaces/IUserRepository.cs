using Skoob.Models;

namespace Skoob.Interfaces;

public interface IUserRepository
{
    public List<Mainuser> SelectUsers(int page, int pageSize);
    public Mainuser? GetById(Guid id);
    public Mainuser? GetByName(string username);
    public void UpdateUserName(Mainuser mainuser);
    public Mainuser Register(Mainuser user);
    public bool UsernameExists(string username);
    public bool EmailExists(string email);
    public void UpdatePassword(Mainuser user);
    public  bool DeleteUser(Guid id);
}