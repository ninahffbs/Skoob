using NUnit.Framework;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Skoob.DTOs;
namespace Skoob.tests;

[TestFixture]
public class UpdatePasswordDTOTests
{
    private IList<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model);
        
        Validator.TryValidateObject(model, validationContext, validationResults, true);
        
        return validationResults;
    }

    [Test]
    public void Should_Fail_When_Confirmation_Does_Not_Match_NewPassword()
    {
        var dto = new UpdatePasswordDTO
        {
            OldPassword = "OldPassword123",
            NewPassword = "NewStrongPassword123",
            ConfirmNewPassword = "WRONG_PASSWORD" 
        };

        var errors = ValidateModel(dto);

        
        Assert.That(errors, Is.Not.Empty, "Deveria ter erros de validação");
        
        var hasMismatchError = errors.Any(e => e.ErrorMessage.Contains("As senhas não conferem"));
        Assert.That(hasMismatchError, Is.True, "O erro deveria ser sobre senhas não conferirem");
    }

    [Test]
    public void Should_Pass_When_Passwords_Match()
    {
     
        var dto = new UpdatePasswordDTO
        {
            OldPassword = "OldPassword123",
            NewPassword = "NewStrong@Password123",
            ConfirmNewPassword = "NewStrong@Password123" 
        };

 
        var errors = ValidateModel(dto);

    
        Assert.That(errors, Is.Empty, "Não deveria ter nenhum erro de validação");
    }

    
}   
