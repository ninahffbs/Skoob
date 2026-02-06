using Skoob.DTOs;

namespace Skoob.Interfaces;

public interface IUserService
{
    public List<UserResponseDTO> GetUsers();
    public UserResponseDTO CreateUser(CreateUserDTO createDto);
}