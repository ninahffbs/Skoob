using Skoob.DTOs;
using Skoob.Interfaces;
using Skoob.Models;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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

    public UserResponseDTO? GetUserById(Guid id)
    {
        var user = _userRepository.GetUserById(id);
        
        if (user == null)
            return null;

        return new UserResponseDTO
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
        };
    }

    public void UpdateUserName(Guid id, string newName)
    {
        var user = _userRepository.GetUserById(id);
        if (user == null)
        {
            throw new KeyNotFoundException($"Usuário com ID '{id}' não encontrado");
        }
        if (_userRepository.UsernameExists(newName))
        {
            throw new ArgumentException($"Nome de usuário '{newName}' já está em uso");
        }
        user.UserName = newName.Trim();
        try
        {
            _userRepository.UpdateUserName(user);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
        {
            throw new ArgumentException($"Nome de usuário '{newName}' já está em uso");
        }
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

        string passwordHash = BCrypt.Net.BCrypt.HashPassword(createDto.Password);

        var user = new Mainuser
        {
            UserName = createDto.UserName.Trim(),
            Email = createDto.Email.Trim().ToLowerInvariant(),
            UserPassword = passwordHash
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
    public void UpdatePassword(Guid id, UpdatePasswordDTO dto)
    {
        var user = _userRepository.GetUserById(id);
        if (user == null)
        {
            throw new KeyNotFoundException($"Usuário com ID '{id}' não encontrado");
        }
        var passwordMatches = BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.UserPassword);

        if (!passwordMatches)
        {
            throw new ArgumentException("Senha atual digitada incorreta!");
        }
        user.UserPassword = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        _userRepository.UpdatePassword(user);
    }
    
    public bool DeleteUser(Guid id)
    {
        var user = _userRepository.GetUserById(id);


        return _userRepository.DeleteUser(id);
    }
}
