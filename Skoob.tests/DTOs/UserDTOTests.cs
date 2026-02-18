using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Skoob.DTOs;
using System.Linq;

namespace Skoob.Tests.DTOs
{
    [TestFixture]
    public class UserDTOTests
    {
        private IList<ValidationResult> ValidateModel(object model)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(model);

            Validator.TryValidateObject(model, context, results, true);

            return results;
        }

        // ==========================================================
        // CreateUserDTO
        // ==========================================================

        [Test]
        public void Validate_CreateUserDTO_ValidData_NoErrors()
        {
            var dto = new CreateUserDTO
            {
                UserName = "user_123",
                Email = "user@email.com",
                Password = "Password@1",
                ConfirmPassword = "Password@1"
            };

            var results = ValidateModel(dto);

            Assert.That(results.Count, Is.EqualTo(0));
        }

        [Test]
        public void Validate_CreateUserDTO_UserNameTooShort_ReturnsError()
        {
            var dto = new CreateUserDTO
            {
                UserName = "abc",
                Email = "user@email.com",
                Password = "Password@1",
                ConfirmPassword = "Password@1"
            };

            var results = ValidateModel(dto);

            Assert.That(results.Count, Is.EqualTo(1));
        }

        [Test]
        public void Validate_CreateUserDTO_UserNameWithInvalidCharacters_ReturnsError()
        {
            var dto = new CreateUserDTO
            {
                UserName = "user name",
                Email = "user@email.com",
                Password = "Password@1",
                ConfirmPassword = "Password@1"
            };

            var results = ValidateModel(dto);

            Assert.That(results.Any(x => x.ErrorMessage!.Contains("underscore")));
        }

        [Test]
        public void Validate_CreateUserDTO_InvalidEmail_ReturnsError()
        {
            var dto = new CreateUserDTO
            {
                UserName = "user123",
                Email = "invalid email",
                Password = "Password@1",
                ConfirmPassword = "Password@1"
            };

            var results = ValidateModel(dto);

            Assert.That(results.Any(x => x.ErrorMessage!.Contains("Email inválido")));
        }

        [Test]
        public void Validate_CreateUserDTO_EmailWithSpaces_ReturnsError()
        {
            var dto = new CreateUserDTO
            {
                UserName = "user123",
                Email = "user @email.com",
                Password = "Password@1",
                ConfirmPassword = "Password@1"
            };

            var results = ValidateModel(dto);

            Assert.That(results.Any(x => x.ErrorMessage!.Contains("não pode conter espaços")));
        }

        [Test]
        public void Validate_CreateUserDTO_WeakPassword_ReturnsError()
        {
            var dto = new CreateUserDTO
            {
                UserName = "user123",
                Email = "user@email.com",
                Password = "password",
                ConfirmPassword = "password"
            };

            var results = ValidateModel(dto);

            Assert.That(results.Any(x => x.ErrorMessage!.Contains("Senha deve conter")));
        }

        [Test]
        public void Validate_CreateUserDTO_PasswordMismatch_ReturnsError()
        {
            var dto = new CreateUserDTO
            {
                UserName = "user123",
                Email = "user@email.com",
                Password = "Password@1",
                ConfirmPassword = "Different@1"
            };

            var results = ValidateModel(dto);

            Assert.That(results.Any(x => x.ErrorMessage!.Contains("não coincidem")));
        }

        // ==========================================================
        // UpdatePasswordDTO
        // ==========================================================

        [Test]
        public void Validate_UpdatePasswordDTO_ValidData_NoErrors()
        {
            var dto = new UpdatePasswordDTO
            {
                OldPassword = "OldPassword@1",
                NewPassword = "NewPassword@1",
                ConfirmNewPassword = "NewPassword@1"
            };

            var results = ValidateModel(dto);

            Assert.That(results.Count, Is.EqualTo(0));
        }

        [Test]
        public void Validate_UpdatePasswordDTO_NewPasswordWeak_ReturnsError()
        {
            var dto = new UpdatePasswordDTO
            {
                OldPassword = "OldPassword@1",
                NewPassword = "weak",
                ConfirmNewPassword = "weak"
            };

            var results = ValidateModel(dto);

            Assert.That(results.Any(x => x.ErrorMessage!.Contains("Senha deve conter")));
        }

        [Test]
        public void Validate_UpdatePasswordDTO_PasswordMismatch_ReturnsError()
        {
            var dto = new UpdatePasswordDTO
            {
                OldPassword = "OldPassword@1",
                NewPassword = "NewPassword@1",
                ConfirmNewPassword = "Another@1"
            };

            var results = ValidateModel(dto);

            Assert.That(results.Any(x => x.ErrorMessage!.Contains("não coincidem")));
        }

        // ==========================================================
        // UserResponseDTO
        // ==========================================================

        [Test]
        public void Constructor_UserResponseDTO_BooksListShouldBeInitialized()
        {
            var dto = new UserResponseDTO();

            Assert.That(dto.Books, Is.Not.Null);
            Assert.That(dto.Books, Is.Empty);
        }

        [Test]
        public void Properties_UserResponseDTO_SetAndGetValues_CorrectlyAssigned()
        {
            var id = Guid.NewGuid();

            var dto = new UserResponseDTO
            {
                Id = id,
                UserName = "user123",
                Email = "user@email.com",
                CreatedAt = DateTime.Now,
                TotalBooks = 10,
                BooksRead = 5
            };

            Assert.That(dto.Id, Is.EqualTo(id));
            Assert.That(dto.UserName, Is.EqualTo("user123"));
            Assert.That(dto.TotalBooks, Is.EqualTo(10));
        }

        // ==========================================================
        // UpdateUserNameRequest (sem validação)
        // ==========================================================

        [Test]
        public void Properties_UpdateUserNameRequest_SetAndGetValues_CorrectlyAssigned()
        {
            var dto = new UpdateUserNameRequest
            {
                UserName = "newUser"
            };

            Assert.That(dto.UserName, Is.EqualTo("newUser"));
        }

        // ==========================================================
        // UserBookSimpleDTO
        // ==========================================================

        [Test]
        public void Properties_UserBookSimpleDTO_SetAndGetValues_CorrectlyAssigned()
        {
            var dto = new UserBookSimpleDTO
            {
                BookTitle = "Livro Teste",
                PagesRead = 100,
                PercentComplete = 50,
                Status = "Lendo",
                StartedAt = DateTime.Today
            };

            Assert.That(dto.BookTitle, Is.EqualTo("Livro Teste"));
            Assert.That(dto.PercentComplete, Is.EqualTo(50));
            Assert.That(dto.Status, Is.EqualTo("Lendo"));
        }
    }
}
