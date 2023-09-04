using AuthServer.API.Models;
using Microsoft.AspNetCore.Identity;

namespace AuthServer.API.CustomValidations;

public class UserValidator : IUserValidator<UserApp>
{
	public Task<IdentityResult> ValidateAsync(UserManager<UserApp> manager, UserApp user)
	{
		var errors = new List<IdentityError>();

		// username - in  reqem ile basladigini yoxlayiriq
		var isDigit = int.TryParse(user.UserName[0].ToString(), out _);


		if (isDigit)
			errors.Add(new IdentityError() { Code = "UserNameFirstLetterDigit", Description = "Kullanici adinin ilk xarakteri reqem ola bilmez" });

		if (errors.Any())
			return Task.FromResult(IdentityResult.Failed(errors.ToArray()));

		return Task.FromResult(IdentityResult.Success);
	}
}
