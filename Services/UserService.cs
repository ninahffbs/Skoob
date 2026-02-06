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
        List<UserResponseDTO> usersDTO = [];

        foreach (var user in mainUsers)
        {
            usersDTO.Add(new UserResponseDTO
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                CreatedAt = user.CreatedAt
            });
        }

        return usersDTO;
    }
}