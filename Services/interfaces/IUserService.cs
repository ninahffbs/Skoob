using Skoob.DTOs;

namespace Skoob.Interfaces;

public interface IUserService
{
    public List<UserResponseDTO> GetUsers();
    public void UpdateUserName(Guid id, string newName);
    public UserResponseDTO CreateUser(CreateUserDTO createDto);
}