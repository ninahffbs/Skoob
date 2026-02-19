using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Skoob.DTOs;
using Skoob.Enums;

namespace Skoob.Tests.DTOs
{
    [TestFixture]
    public class UserbookDTOTests
    {
        private IList<ValidationResult> ValidateModel(object model)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(model);

            Validator.TryValidateObject(model, context, results, true);

            return results;
        }

        // ===============================
        // AddBooksUserDTO
        // ===============================

        [Test]
        public void Validate_AddBooksUserDTO_ValidData_NoErrors()
        {
            var dto = new AddBooksUserDTO
            {
                BookId = Guid.NewGuid(),
                Status = StatusBook.Lendo,
                PagesRead = 10,
                StartDate = DateTime.Now
            };

            var results = ValidateModel(dto);

            Assert.That(results.Count, Is.EqualTo(0));
        }

        [Test]
        public void Validate_AddBooksUserDTO_NegativePagesRead_ReturnsError()
        {
            var dto = new AddBooksUserDTO
            {
                Status = StatusBook.Lendo,
                PagesRead = -1
            };

            var results = ValidateModel(dto);

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].ErrorMessage,
                Is.EqualTo("Páginas lidas deve ser maior ou igual a 0"));
        }

        // ===============================
        // AddRatingDTO
        // ===============================

        [Test]
        public void Validate_AddRatingDTO_ValidRating_NoErrors()
        {
            var dto = new AddRatingDTO
            {
                Rating = 4
            };

            var results = ValidateModel(dto);

            Assert.That(results.Count, Is.EqualTo(0));
        }

        [Test]
        public void Validate_AddRatingDTO_RatingNull_ReturnsError()
        {
            var dto = new AddRatingDTO
            {
                Rating = null
            };

            var results = ValidateModel(dto);

            Assert.That(results.Count, Is.EqualTo(1));
        }

        [Test]
        public void Validate_AddRatingDTO_RatingOutOfRange_ReturnsError()
        {
            var dto = new AddRatingDTO
            {
                Rating = 6
            };

            var results = ValidateModel(dto);

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].ErrorMessage,
                Is.EqualTo("Rating deve ser entre 1 e 5"));
        }

        // ===============================
        // UpdateReadPagesDTO
        // ===============================

        [Test]
        public void Validate_UpdateReadPagesDTO_ValidData_NoErrors()
        {
            var dto = new UpdateReadPagesDTO
            {
                PagesRead = 50
            };

            var results = ValidateModel(dto);

            Assert.That(results.Count, Is.EqualTo(0));
        }

        [Test]
        public void Validate_UpdateReadPagesDTO_NegativePages_ReturnsError()
        {
            var dto = new UpdateReadPagesDTO
            {
                PagesRead = -5
            };

            var results = ValidateModel(dto);

            Assert.That(results.Count, Is.EqualTo(1));
        }

        // ===============================
        // UpdateUserbooksDTO
        // ===============================

        [Test]
        public void Validate_UpdateUserbooksDTO_ValidData_NoErrors()
        {
            var dto = new UpdateUserbooksDTO
            {
                Status = StatusBook.Lido,
                PagesRead = 100,
                Rating = 5,
                Review = "Excelente livro!"
            };

            var results = ValidateModel(dto);

            Assert.That(results.Count, Is.EqualTo(0));
        }

        [Test]
        public void Validate_UpdateUserbooksDTO_InvalidStatusRange_ReturnsError()
        {
            var dto = new UpdateUserbooksDTO
            {
                Status = (StatusBook)5
            };

            var results = ValidateModel(dto);

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].ErrorMessage,
                Is.EqualTo("O status deve estar entre 0 (Lendo), 1 (Lido) ou 2 (Quero Ler)."));
        }

        [Test]
        public void Validate_UpdateUserbooksDTO_NegativePagesRead_ReturnsError()
        {
            var dto = new UpdateUserbooksDTO
            {
                Status = StatusBook.Lendo,
                PagesRead = -10
            };

            var results = ValidateModel(dto);

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].ErrorMessage,
                Is.EqualTo("Páginas lidas deve ser >= 0"));
        }

        [Test]
        public void Validate_UpdateUserbooksDTO_ReviewGreaterThan500Characters_ReturnsError()
        {
            var dto = new UpdateUserbooksDTO
            {
                Status = StatusBook.Lido,
                Review = new string('a', 501)
            };

            var results = ValidateModel(dto);

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].ErrorMessage,
                Is.EqualTo("Review não pode ter mais de 500 caracteres"));
        }

        [Test]
        public void Validate_UpdateUserbooksDTO_RatingOutOfRange_ReturnsError()
        {
            var dto = new UpdateUserbooksDTO
            {
                Status = StatusBook.Lido,
                Rating = 10
            };

            var results = ValidateModel(dto);

            Assert.That(results.Count, Is.EqualTo(1));
        }

        // ===============================
        // UserbookResponseDTO
        // ===============================

        [Test]
        public void Properties_UserbookResponseDTO_SetAndGetValues_CorrectlyAssigned()
        {
            var id = Guid.NewGuid();

            var dto = new UserbookResponseDTO
            {
                BookId = id,
                BookTitle = "Livro Teste",
                BookPages = 300,
                AuthorName = "Autor",
                PagesRead = 150,
                PercentComplete = 50,
                Status = StatusBook.Lendo,
                StatusName = "Lendo",
                StartDate = DateTime.Today,
                Rating = 5,
                Review = "Muito bom!"
            };

            Assert.That(dto.BookId, Is.EqualTo(id));
            Assert.That(dto.BookTitle, Is.EqualTo("Livro Teste"));
            Assert.That(dto.PercentComplete, Is.EqualTo(50));
            Assert.That(dto.StatusName, Is.EqualTo("Lendo"));
            Assert.That(dto.Rating, Is.EqualTo(5));
        }
        [Test]
        public void Validate_UpdateUserbooksDTO_RatingBelowMinimum_ReturnsError()
        {
            var dto = new UpdateUserbooksDTO
            {
                Status = StatusBook.Lido,
                Rating = 0
            };

            var results = ValidateModel(dto);

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].ErrorMessage,
                Is.EqualTo("Rating deve ser entre 1 e 5"));
        }

        [Test]
        public void Validate_UpdateUserbooksDTO_RatingAtMinimum_IsValid()
        {
            var dto = new UpdateUserbooksDTO
            {
                Status = StatusBook.Lido,
                Rating = 1
            };

            var results = ValidateModel(dto);

            Assert.That(results.Count, Is.EqualTo(0));
        }

        [Test]
        public void Validate_UpdateUserbooksDTO_RatingAtMaximum_IsValid()
        {
            var dto = new UpdateUserbooksDTO
            {
                Status = StatusBook.Lido,
                Rating = 5
            };

            var results = ValidateModel(dto);

            Assert.That(results.Count, Is.EqualTo(0));
        }

        [Test]
        public void Validate_UpdateUserbooksDTO_PagesReadZero_IsValid()
        {
            var dto = new UpdateUserbooksDTO
            {
                Status = StatusBook.Lendo,
                PagesRead = 0
            };

            var results = ValidateModel(dto);

            Assert.That(results.Count, Is.EqualTo(0));
        }

        [Test]
        public void Validate_UpdateUserbooksDTO_ReviewExactly500Characters_IsValid()
        {
            var dto = new UpdateUserbooksDTO
            {
                Status = StatusBook.Lido,
                Review = new string('a', 500)
            };

            var results = ValidateModel(dto);

            Assert.That(results.Count, Is.EqualTo(0));
        }

        [Test]
        public void Validate_UpdateUserbooksDTO_OnlyStatusDefault_IsValid()
        {
            var dto = new UpdateUserbooksDTO();

            var results = ValidateModel(dto);

            Assert.That(results.Count, Is.EqualTo(0));
        }
        [Test]
        public void Validate_UpdateUserbooksDTO_MultipleErrors_ReturnsAllErrors()
        {
            var dto = new UpdateUserbooksDTO
            {
                Status = (StatusBook)10,
                PagesRead = -5,
                Rating = 10,
                Review = new string('a', 600)
            };

            var results = ValidateModel(dto);

            Assert.That(results.Count, Is.EqualTo(4));
        }

        [Test]
        public void Validate_UpdateUserbooksDTO_RatingNull_NoError()
        {
            var dto = new UpdateUserbooksDTO
            {
                Status = StatusBook.Lido,
                Rating = null
            };

            var results = ValidateModel(dto);

            Assert.That(results.Count, Is.EqualTo(0));
        }
        [Test]
        public void Validate_UpdateUserbooksDTO_PagesReadNull_NoError()
        {
            var dto = new UpdateUserbooksDTO
            {
                Status = StatusBook.Lendo,
                PagesRead = null
            };

            var results = ValidateModel(dto);

            Assert.That(results.Count, Is.EqualTo(0));
        }

        [Test]
        public void Validate_UpdateUserbooksDTO_ReviewAtLimit_NoError()
        {
            var dto = new UpdateUserbooksDTO
            {
                Status = StatusBook.Lido,
                Review = new string('a', 500)
            };

            var results = ValidateModel(dto);

            Assert.That(results.Count, Is.EqualTo(0));
        }
        [Test]
        public void Validate_UpdateUserbooksDTO_StartAndFinishDate_Accessed_ShouldBeCovered()
        {
            var start = DateTime.Now;
            var finish = DateTime.Now.AddDays(10);

            var dto = new UpdateUserbooksDTO
            {
                Status = StatusBook.Lendo,
                StartDate = start,
                FinishDate = finish
            };

            // Força execução do getter
            Assert.That(dto.StartDate, Is.EqualTo(start));
            Assert.That(dto.FinishDate, Is.EqualTo(finish));
        }
    }  
}
