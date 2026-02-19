using Skoob.DTOs;
using Skoob.Enums;

namespace Skoob.Interfaces;

public interface IUserServiceBook
{
    public UserbookResponseDTO AddBookUser(Guid userId, AddBooksUserDTO dto);
    public List<UserbookResponseDTO> GetUserBooks(Guid userId);
    public void RemoveUserBook(Guid userId, Guid bookId);
    public void UpdateReadPages(Guid userId, Guid bookId, int newPages);
    public void AddRating(Guid userId, Guid bookId, int rating);
    public void UpdateStatus(Guid userId, Guid bookId, StatusBook newStatus);
    public void UpdateReview(Guid userId, Guid bookId, string? reviewText);
    public List<UserbookResponseDTO> FilterUserBookByTitle(Guid userId, string searchedTitle);
    public List<UserbookResponseDTO> FilterUserBookByGenre(Guid userId, string searchedGenre);
    public AnnualReadingReportDTO GenerateAnnualReport(Guid userId, int year);
    public List<UserbookResponseDTO> FilterUserBookByAuthor(Guid userId, string searchedAuthor);
} 