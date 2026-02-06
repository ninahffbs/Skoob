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

     public UserResponseDTO CreateUser(CreateUserDTO createDto)
    {
        if (_userRepository.UsernameExists(createDto.UserName))
        {
            throw new ArgumentException($"Nome de usuário '{createDto.UserName}' já está em uso");
        }

         if (_userRepository.EmailExists(createDto.Email))
        {
            throw new ArgumentException($"Email '{createDto.Email}' já está cadastrado");
        }

        var user = new Mainuser
        {
            UserName = createDto.UserName.Trim(),
            Email = createDto.Email.Trim().ToLowerInvariant(),
            UserPassword = createDto.Password
        };

        var createdUser = _userRepository.CreateUser(user);
        return new UserResponseDTO
        {
            Id = createdUser.Id,
            UserName = createdUser.UserName,
            Email = createdUser.Email,
            CreatedAt = createdUser.CreatedAt,
        };
    }
}
