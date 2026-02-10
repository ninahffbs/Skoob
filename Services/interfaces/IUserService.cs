using Skoob.DTOs;

namespace Skoob.Interfaces;

public interface IUserService
{
    public List<UserResponseDTO> GetUsers(int page);
    public void UpdateUserName(Guid id, string newName);
    public UserResponseDTO? GetUserById(Guid id);
    public UserResponseDTO? GetByUserName(string userName);
    public UserResponseDTO CreateUser(CreateUserDTO createDto);
    public void UpdatePassword(Guid id, UpdatePasswordDTO dto);
    public bool DeleteUser(Guid id);
}