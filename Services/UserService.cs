using Skoob.DTOs;
using Skoob.Interfaces;
using Skoob.Models;

namespace Skoob.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    public List<UserResponseDTO> GetUsers()
    {
        List<Mainuser> mainUsers = _userRepository.SelectUsers();
        List<UserResponseDTO> usersDTO = new();

        foreach (var mainuser in mainUsers)
        {
            usersDTO.Add(new UserResponseDTO
            {
                Id = mainuser.Id,
                UserName = mainuser.UserName,
                Email = mainuser.Email,
                CreatedAt = mainuser.CreatedAt
            });
        }

        return usersDTO;
    }
}