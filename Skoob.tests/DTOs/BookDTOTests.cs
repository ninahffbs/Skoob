using NUnit.Framework;
using Skoob.DTOs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Skoob.Tests.DTOs
{
    [TestFixture]
    public class BookDTOTests
    {
        private IList<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(model, null, null);

            Validator.TryValidateObject(
                model,
                context,
                validationResults,
                true
            );

            return validationResults;
        }

        [Test]
        public void Validate_ValidDTO_NoValidationErrors()
        {
            // Arrange
            var dto = new BookDTO
            {
                Id = Guid.NewGuid(),
                Title = "Harry Potter",
                Pages = 300,
                Synopsis = new string('a', 500),
                PublishedDate = 1997,
                AuthorName = "J.K. Rowling",
                Genres = new List<string> { "Fantasy" }
            };

            // Act
            var results = ValidateModel(dto);

            // Assert
            Assert.That(results.Count, Is.EqualTo(0));
        }

        [Test]
        public void Validate_SynopsisGreaterThan500Characters_ReturnsValidationError()
        {
            // Arrange
            var dto = new BookDTO
            {
                Synopsis = new string('a', 501)
            };

            // Act
            var results = ValidateModel(dto);

            // Assert
            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].ErrorMessage,
                Is.EqualTo("Sinopse não pode ter mais de 500 caracteres"));
        }

        [Test]
        public void Validate_SynopsisExactly500Characters_NoValidationError()
        {
            // Arrange
            var dto = new BookDTO
            {
                Synopsis = new string('a', 500)
            };

            // Act
            var results = ValidateModel(dto);

            // Assert
            Assert.That(results.Count, Is.EqualTo(0));
        }

        [Test]
        public void Validate_NullSynopsis_NoValidationError()
        {
            // Arrange
            var dto = new BookDTO
            {
                Synopsis = null
            };

            // Act
            var results = ValidateModel(dto);

            // Assert
            Assert.That(results.Count, Is.EqualTo(0));
        }

        [Test]
        public void Constructor_GenresList_ShouldBeInitialized()
        {
            // Arrange & Act
            var dto = new BookDTO();

            // Assert
            Assert.That(dto.Genres, Is.Not.Null);
            Assert.That(dto.Genres, Is.Empty);
        }

        [Test]
        public void Properties_SetAndGetValues_ShouldReturnCorrectValues()
        {
            // Arrange
            var id = Guid.NewGuid();
            var genres = new List<string> { "Fantasy", "Adventure" };

            var dto = new BookDTO
            {
                Id = id,
                Title = "Test Book",
                Pages = 200,
                Synopsis = "Some synopsis",
                PublishedDate = 2020,
                AuthorName = "Author Test",
                Genres = genres
            };

            // Act & Assert
            Assert.That(dto.Id, Is.EqualTo(id));
            Assert.That(dto.Title, Is.EqualTo("Test Book"));
            Assert.That(dto.Pages, Is.EqualTo(200));
            Assert.That(dto.Synopsis, Is.EqualTo("Some synopsis"));
            Assert.That(dto.PublishedDate, Is.EqualTo(2020));
            Assert.That(dto.AuthorName, Is.EqualTo("Author Test"));
            Assert.That(dto.Genres, Is.EqualTo(genres));
        }
    }
}
