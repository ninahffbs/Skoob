using Skoob.DTOs;
using Skoob.Interfaces;
using Skoob.Models;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Skoob.Enums;

namespace Skoob.Services;
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly int _pageSize;

    public UserService(
        IUserRepository userRepository,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _pageSize = configuration.GetValue<int>("Pagination:UsersPageSize");
    }

    public List<UserResponseDTO> GetUsers(int page)
    {
        if (page <= 0)
            page = 1;

        var mainUsers = _userRepository.SelectUsers(page, _pageSize);

        return [.. mainUsers.Select(user => new UserResponseDTO
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            TotalBooks = user.Userbooks?.Count ?? 0,
            BooksRead = user.Userbooks?.Count(ub => ub.FinishDate != null) ?? 0,
            Books = user.Userbooks?.Select(ub => ub.Book.Title).ToList() ?? []
        })];
    }

    public UserResponseDTO? GetUserById(Guid id)
    {
        var user = _userRepository.GetById(id);
        
        if (user == null)
            return null;

        return new UserResponseDTO
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            TotalBooks = user.Userbooks?.Count ?? 0,
            BooksRead = user.Userbooks?.Count(ub => ub.FinishDate != null) ?? 0,
            Books = user.Userbooks?.Select(ub => ub.Book.Title).ToList() ?? []
        };
    }

    public UserProfileDTO? GetByUserName(string userName)
    {
        var user = _userRepository.GetByName(userName);

        if (user == null) return null;

        return new UserProfileDTO
        {
            UserName = user.UserName,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            TotalBooks = user.Userbooks.Count,
            BooksRead = user.Userbooks.Count(ub => ub.FinishDate != null),
            

            Books = [.. user.Userbooks.Select(ub => new UserBookSimpleDTO
            {
                BookTitle = ub.Book.Title, 
                PagesRead = ub.PagesRead ?? 0,
                PercentComplete = ub.Book.PagesNumber > 0 ? (int)((ub.PagesRead ?? 0) * 100.0 / ub.Book.PagesNumber) : 0,
                Status = ub.Status.ToString(), 
                StartedAt = ub.StartDate 
            })]

        };
    }

    public void UpdateUserName(Guid id, string newName)
    {
        var user = _userRepository.GetById(id);
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

        var createdUser = _userRepository.Register(user);
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
        var user = _userRepository.GetById(id);
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
        var user = _userRepository.GetById(id) ?? throw new ArgumentException($"Usuário com ID {id} não encontrado");
        
        return _userRepository.DeleteUser(id);
    }
}
