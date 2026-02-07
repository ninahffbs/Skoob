using Skoob.DTOs;

namespace Skoob.Interfaces;

public interface IUserService
{
    public List<UserResponseDTO> GetUsers();
    public UserResponseDTO? GetUserById(Guid id);
    public string UpdateUserName(Guid id, string newName);
    public UserResponseDTO CreateUser(CreateUserDTO createDto);
    public bool DeleteUser(Guid id);
}