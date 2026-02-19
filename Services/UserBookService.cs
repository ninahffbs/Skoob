using System.Collections.Concurrent;
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

        DateTime? startDate = dto.StartDate;
        if (dto.Status == StatusBook.QueroLer)
        {
            startDate = null;
        }
        else
        {
            startDate = startDate ?? DateTime.UtcNow;
        }

        // regra 1 usuario existe?
        if (!_userRepository.Exists(userId)) throw new ArgumentException($"Usuário com ID {userId} Não encontrado");

        // regra 2 livro existe?
        if (!_userbookRepository.BookExists(dto.BookId)) throw new ArgumentException($"Livro com id {dto.BookId} nNão encontrado");

        // regra 3 usuario ja tem o livro?
        if (_userbookRepository.UserHasBook(userId, dto.BookId)) throw new ArgumentException("Você já tem esse livro adicionado."); ;

        var book = _bookRepository.GetBookById(dto.BookId);

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
        if (!_userRepository.Exists(userId)) throw new ArgumentException($"Usuário com {userId} não encontrado");

        var userbooks = _userbookRepository.GetUserbooksByUserId(userId);

        return userbooks.Select(ub => MapToDTO(ub)).ToList();
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

    private string GetStatusName(StatusBook status)
    {
        return status switch
        {
            StatusBook.Lendo => "Lendo",
            StatusBook.Lido => "Lido",
            StatusBook.QueroLer => "Quero Ler",
            _ => "Desconhecido"
        };
    }

    public void RemoveUserBook(Guid userId, Guid bookId)
    {
        var deleted = _userbookRepository.DeleteUserBook(userId, bookId);

        if (!deleted) throw new ArgumentException("Este livro não existe na biblioteca deste usuário");
    }

    public void UpdateReadPages(Guid userId, Guid bookId, int newPages)
    {
        if (newPages < 0)
        {
            throw new ArgumentException("O número de páginas não pode ser negativo");
        }
        var userBook = _userbookRepository.GetUserbook(userId, bookId);
        var book = _bookRepository.GetBookById(bookId);

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
        _userbookRepository.UpdateReadPages(userBook);
    }

    public void AddRating(Guid userId, Guid bookId, int rating)
    {
        if (rating < 0)
        {
            throw new ArgumentException("A avaliação deve estar entre 1 e 5.");
        }

        var userBook = _userbookRepository.GetUserbook(userId, bookId);

        if (userBook.Status != StatusBook.Lido)
        {
            throw new ArgumentException("Você não pode avaliar um livro que ainda não finalizou");
        }

        userBook.Rating = (short)rating;
        _userbookRepository.AddRating(userBook);
    }

    //FilterByTitle
    public List<UserbookResponseDTO> FilterUserBookByTitle(Guid userId, string searchedTitle)
    {
        if (string.IsNullOrWhiteSpace(searchedTitle) || searchedTitle.Length < 3)
        {
            throw new ArgumentException("O título deve ter pelo menos 3 caracteres.");
        }
        var user = _userRepository.GetById(userId);
        if (user == null)
        {
            throw new ArgumentException("Usuário com esse id não foi encontrado.");
        }
        var userBooks = _userbookRepository.GetUserbooksByUserId(userId);

        var filteredBooks = userBooks
            .Where(ub => ub.Book.Title
                .Contains(searchedTitle, StringComparison.OrdinalIgnoreCase))
            .Select(ub => MapToDTO(ub)).ToList();

        return filteredBooks;
    }

    public List<UserbookResponseDTO> FilterUserBookByGenre(Guid userId, string searchedGenre)
    {
        if (string.IsNullOrWhiteSpace(searchedGenre) || searchedGenre.Length < 3)
        {
            throw new ArgumentException("O gênero buscado deve ter pelo menos 3 caracteres.");
        }

        var user = _userRepository.GetById(userId);
        if (user == null)
        {
            throw new ArgumentException("Usuário com esse id não foi encontrado.");
        }

        var userBooks = _userbookRepository.GetUserbooksByUserId(userId);

        var filteredBooks = userBooks
            .Where(ub => ub.Book.Genres
                .Any(g => g.Name
                    .Contains(searchedGenre, StringComparison.OrdinalIgnoreCase)))
            .Select(ub => MapToDTO(ub))
            .ToList();

        return filteredBooks;
    }

    public AnnualReadingReportDTO GenerateAnnualReport(Guid userId, int year)
    {
        if (!_userRepository.Exists(userId))
            throw new ArgumentException($"Usuário com ID {userId} não encontrado");

        var userBooks = _userbookRepository.GetUserbooksByUserId(userId);

        //livros finalizados no ano
        var finishedThisYear = userBooks
            .Where(ub =>
                ub.Status == StatusBook.Lido &&
                ub.FinishDate.HasValue &&
                ub.FinishDate.Value.Year == year)
            .ToList();

        //livros atualmente lendo
        var currentlyReading = userBooks
            .Where(ub => ub.Status == StatusBook.Lendo)
            .ToList();

        //livros atualmente quero ler
        var wantToRead = userBooks
            .Where(ub => ub.Status == StatusBook.QueroLer)
            .ToList();

        //Total de paginas lidas: se finalizado, usa total do livro
        //se lendo, usa PagesRead
        int pagesFromFinished = finishedThisYear
            .Sum(ub => ub.Book.PagesNumber);

        int pagesFromReading = currentlyReading
            .Sum(ub => ub.PagesRead ?? 0);

        int totalPages = pagesFromFinished + pagesFromReading;

        //estimativa -> (1 página = 1 minuto)
        double totalHours = totalPages / 60.0;

        //média das avaliações de livros finalizados no ano
        var ratedBooks = finishedThisYear
            .Where(ub => ub.Rating.HasValue)
            .ToList();

        double averageRating = ratedBooks.Any()
            ? ratedBooks.Average(ub => ub.Rating!.Value)
            : 0;

        //gênero favorito do ano
        var favoriteGenre = finishedThisYear
            .SelectMany(ub => ub.Book.Genres)
            .GroupBy(g => g.Name)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault();

        return new AnnualReadingReportDTO
        {
            Year = year,
            TotalRead = finishedThisYear.Count,
            TotalReading = currentlyReading.Count,
            TotalWantToRead = wantToRead.Count,
            TotalPagesRead = totalPages,
            EstimatedReadingHours = Math.Round(totalHours, 2),
            AverageRating = Math.Round(averageRating, 2),
            FavoriteGenre = favoriteGenre
        };
    }
}