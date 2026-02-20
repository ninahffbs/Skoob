using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Skoob.DTOs;
using Skoob.Enums;
using Skoob.Interfaces;
using Skoob.Models;

namespace Skoob.Services;

public class UserbookService : IUserServiceBook
{
    private readonly IUserbookRepository _userbookRepository;
    private readonly IUserRepository _userRepository;
    private readonly IBookRepository _bookRepository;
    private readonly int _pageSize;

    public UserbookService(
        IUserbookRepository userbookRepository,
        IUserRepository userRepository,
        IBookRepository bookRepository,
        IConfiguration configuration)
    {
        _userbookRepository = userbookRepository;
        _userRepository = userRepository;
        _bookRepository = bookRepository;
        _pageSize = configuration.GetValue<int>("Pagination:UsersPageSize");
    }

    public UserbookResponseDTO AddBookUser(Guid userId, AddBooksUserDTO dto)
    {
        if (!Enum.IsDefined(dto.Status))
        {
            throw new ArgumentException($"O valor {dto.Status} não é um status válido para um livro.");
        }

        DateTime? startDate = dto.Status == StatusBook.QueroLer ? null : (dto.StartDate ?? DateTime.UtcNow);

        if (!_userRepository.Exists(userId)) 
            throw new ArgumentException($"Usuário com ID {userId} não encontrado");

        if (!_userbookRepository.BookExists(dto.BookId)) 
            throw new ArgumentException($"Livro com id {dto.BookId} não encontrado");

        if (_userbookRepository.UserHasBook(userId, dto.BookId)) 
            throw new ArgumentException("Você já tem esse livro adicionado.");

        var book = _bookRepository.GetBookById(dto.BookId);

        if (dto.Status == StatusBook.Lido)
        {
            dto.PagesRead = book.PagesNumber;
        }

        if (dto.PagesRead > book.PagesNumber)
        {
            throw new ArgumentException($"Número de páginas lidas para {book.Title} só pode ser até {book.PagesNumber}");
        }

        if (dto.PagesRead == book.PagesNumber)
        {
            dto.Status = StatusBook.Lido;
        }

        var userbook = new Userbook
        {
            UserId = userId,
            BookId = dto.BookId,
            Status = dto.Status,
            PagesRead = dto.PagesRead,
            StartDate = startDate,
            FinishDate = dto.Status == StatusBook.Lido ? DateTime.UtcNow : null
        };

        _userbookRepository.AddBookToUser(userbook);

        var addedBook = _userbookRepository.GetUserbook(userId, dto.BookId) ?? throw new Exception("Não encontrado.");

        return MapToDTO(addedBook);
    }

    public List<UserbookResponseDTO> GetUserBooks(Guid userId)
    {
        if (!_userRepository.Exists(userId)) 
            throw new ArgumentException($"Usuário com {userId} não encontrado");

        var userbooks = _userbookRepository.GetUserbooksByUserId(userId);

        return userbooks.Select(MapToDTO).ToList();
    }

    public void RemoveUserBook(Guid userId, Guid bookId)
    {
        var deleted = _userbookRepository.DeleteUserBook(userId, bookId);

        if (!deleted) 
            throw new ArgumentException("Este livro não existe na biblioteca deste usuário");
    }

    public void UpdateReadPages(Guid userId, Guid bookId, int newPages)
    {
        if (newPages < 0)
        {
            throw new ArgumentException("O número de páginas não pode ser negativo");
        }

        var userBook = _userbookRepository.GetUserbook(userId, bookId);
        var book = _bookRepository.GetBookById(bookId);

        if (userBook == null)
        {
            throw new ArgumentException("Livro não encontrado na biblioteca deste usuário.");
        }

        if (newPages > book.PagesNumber)
        {
            throw new ArgumentException($"Este livro possui apenas {book.PagesNumber} páginas");
        }
        
        userBook.PagesRead = newPages;

        if (newPages == book.PagesNumber)
        {
            userBook.Status = StatusBook.Lido;
            userBook.FinishDate = DateTime.UtcNow;
        }
        else if (newPages > 0)
        {
            userBook.Status = StatusBook.Lendo;
        }

        _userbookRepository.Update(userBook);
    }

    public void AddRating(Guid userId, Guid bookId, int rating)
    {
        if (rating < 0 || rating > 5)
        {
            throw new ArgumentException("A avaliação deve estar entre 0 e 5.");
        }

        var userBook = _userbookRepository.GetUserbook(userId, bookId);

        if (userBook?.Status != StatusBook.Lido)
        {
            throw new ArgumentException("Você não pode avaliar um livro que ainda não finalizou");
        }

        userBook.Rating = (short)rating;
        _userbookRepository.Update(userBook);
    }

    public List<UserbookResponseDTO> FilterUserBookByTitle(Guid userId, string searchedTitle)
    {
        if (string.IsNullOrWhiteSpace(searchedTitle) || searchedTitle.Length < 3)
        {
            throw new ArgumentException("O título deve ter pelo menos 3 caracteres.");
        }

        if (!_userRepository.Exists(userId))
            throw new ArgumentException("Usuário com esse id não foi encontrado.");

        var filteredBooks = _userbookRepository.GetUserBooksByTitle(userId, searchedTitle);

        return filteredBooks.Select(MapToDTO).ToList();
    }

    public List<UserbookResponseDTO> FilterUserBookByGenre(Guid userId, string searchedGenre)
    {
        if (string.IsNullOrWhiteSpace(searchedGenre) || searchedGenre.Length < 3)
        {
            throw new ArgumentException("O gênero buscado deve ter pelo menos 3 caracteres.");
        }

        if (!_userRepository.Exists(userId))
            throw new ArgumentException("Usuário com esse id não foi encontrado.");

        var filteredBooks = _userbookRepository.GetUserBooksByGenre(userId, searchedGenre);

        return filteredBooks.Select(MapToDTO).ToList();
    }

    public List<UserbookResponseDTO> FilterUserBookByAuthor(Guid userId, string searchedAuthor)
    {
        if (string.IsNullOrWhiteSpace(searchedAuthor) || searchedAuthor.Length < 3)
        {
            throw new ArgumentException("O autor buscado deve ter pelo menos 3 caracteres.");
        }

        if (!_userRepository.Exists(userId))
            throw new ArgumentException("Usuário com este id não foi encontrado.");

        var filteredBooks = _userbookRepository.GetUserBooksByAuthor(userId, searchedAuthor);

        return filteredBooks.Select(MapToDTO).ToList();
    }

    public AnnualReadingReportDTO GenerateAnnualReport(Guid userId, int year)
    {
        var user = _userRepository.GetById(userId) 
            ?? throw new ArgumentException($"Usuário com ID {userId} não encontrado");

        string memberSince = user.CreatedAt?.ToString("dd/MM/yyyy") ?? "Data não disponível";
        string timeOnPlatform = "0 minutos";

        if (user.CreatedAt.HasValue)
        {
            var diff = DateTime.UtcNow - user.CreatedAt.Value.ToUniversalTime();
            timeOnPlatform = $"{(long)diff.TotalMinutes:N0} minutos";
        }

        var userBooks = _userbookRepository.GetUserbooksByUserId(userId);

        var finishedThisYear = userBooks
            .Where(ub => ub.Status == StatusBook.Lido && ub.FinishDate?.Year == year)
            .ToList();

        var currentlyReading = userBooks.Where(ub => ub.Status == StatusBook.Lendo).ToList();
        var wantToRead = userBooks.Where(ub => ub.Status == StatusBook.QueroLer).ToList();

        int pagesFromFinished = finishedThisYear.Sum(ub => ub.Book.PagesNumber);
        int pagesFromReading = currentlyReading.Sum(ub => ub.PagesRead ?? 0);
        int totalPages = pagesFromFinished + pagesFromReading;

        var ratedBooks = finishedThisYear.Where(ub => ub.Rating.HasValue).ToList();
        double averageRating = ratedBooks.Any() ? ratedBooks.Average(ub => ub.Rating!.Value) : 0;

        var favoriteGenre = finishedThisYear
            .SelectMany(ub => ub.Book.Genres)
            .GroupBy(g => g.Name)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault();

        return new AnnualReadingReportDTO
        {
            UserName = user.UserName,
            Year = year,
            TotalRead = finishedThisYear.Count,
            TotalReading = currentlyReading.Count,
            TotalWantToRead = wantToRead.Count,
            TotalPagesRead = totalPages,
            EstimatedReadingHours = Math.Round(totalPages / 60.0, 2),
            AverageRating = Math.Round(averageRating, 2),
            FavoriteGenre = favoriteGenre,
            MemberSince = memberSince,
            TimeOnPlatform = timeOnPlatform
        };
    }

    public void UpdateStatus(Guid userId, Guid bookId, StatusBook newStatus)
    {
        var userBook = _userbookRepository.GetUserbook(userId, bookId) 
            ?? throw new ArgumentException("Livro não encontrado na biblioteca deste usuário.");

        if (!Enum.IsDefined(newStatus))
            throw new ArgumentException($"O valor {newStatus} não é um status válido.");

        var book = _bookRepository.GetBookById(bookId);
        
        switch (newStatus)
        {
            case StatusBook.Lido:
                userBook.FinishDate = DateTime.UtcNow;
                userBook.StartDate ??= DateTime.UtcNow;
                userBook.PagesRead = book.PagesNumber;
                break;

            case StatusBook.Lendo:
                userBook.FinishDate = null; 
                userBook.StartDate ??= DateTime.UtcNow;
                break;

            case StatusBook.QueroLer:
                userBook.StartDate = null;
                userBook.FinishDate = null;
                userBook.PagesRead = 0;
                break;
        }

        userBook.Status = newStatus;
        _userbookRepository.Update(userBook);
    }

    public void UpdateReview(Guid userId, Guid bookId, string? reviewText)
    {
        var userBook = _userbookRepository.GetUserbook(userId, bookId) 
            ?? throw new ArgumentException("Livro não encontrado na biblioteca deste usuário.");

        if (userBook.Status == StatusBook.QueroLer) 
        {
            throw new InvalidOperationException("Você não pode avaliar um livro que ainda não começou a ler.");
        }

        userBook.Review = reviewText?.Trim();
        _userbookRepository.Update(userBook);
    }

    private UserbookResponseDTO MapToDTO(Userbook ub)
    {
        return new UserbookResponseDTO
        {
            BookId = ub.BookId,
            BookTitle = ub.Book.Title,
            BookPages = ub.Book.PagesNumber,
            AuthorName = ub.Book.Author?.Name,
            PagesRead = ub.PagesRead ?? 0,
            PercentComplete = ub.Book.PagesNumber > 0
                ? (int)((ub.PagesRead ?? 0) * 100.0 / ub.Book.PagesNumber)
                : 0,
            Status = ub.Status,
            StatusName = GetStatusName(ub.Status),
            StartDate = ub.StartDate,
            FinishDate = ub.FinishDate,
            Rating = ub.Rating,
            Review = ub.Review
        };
    }

    private string GetStatusName(StatusBook status) => status switch
    {
        StatusBook.Lendo => "Lendo",
        StatusBook.Lido => "Lido",
        StatusBook.QueroLer => "Quero Ler",
        _ => "Desconhecido"
    };
}