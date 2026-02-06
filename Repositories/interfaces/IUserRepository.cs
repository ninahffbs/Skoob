using Skoob.Models;
namespace Skoob.Interfaces;

public interface IUserRepository
{
    public List<Mainuser> SelectUsers();
}