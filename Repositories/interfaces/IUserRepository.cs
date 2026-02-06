using Skoob.Models;
namespace Skoob.Interfaces;

public interface IUserRepository
{
    public List<Mainuser> SelectUsers();
    public Mainuser? GetUserById(Guid id);
    public string UpdateUserName(Guid id, string newName);
}