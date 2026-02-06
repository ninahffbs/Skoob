using Skoob.DTOs;

namespace Skoob.Interfaces;

public interface IUserService
{
    public List<UserResponseDTO> GetUsers();
    public string UpdateUserName(Guid id, string newName);
}