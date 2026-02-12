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
            NewPassword = "NewStrong@Password123",
            ConfirmNewPassword = "NewStrongPassword123" 
        };

        var errors = ValidateModel(dto);
        
        Assert.That(errors, Is.Empty);
    }
}