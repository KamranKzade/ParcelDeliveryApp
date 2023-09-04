using AuthServer.API.Models;
using Microsoft.AspNetCore.Identity;

namespace AuthServer.API.CustomValidations;

public class PasswordValidator : IPasswordValidator<UserApp>
{
	public Task<IdentityResult> ValidateAsync(UserManager<UserApp> manager, UserApp user, string password)
	{
		var errors = new List<IdentityError>();

		// Password da username olub olmadigini yoxlayiriq
		if (password.ToLower().Contains(user.UserName.ToLower()))
			errors.Add(new IdentityError() { Code = "PasswrodContainUserNmae", Description = "Parolda username ola bilmez" });

		// Password un ne ile basladigini gosteririk ve neye icaze olmaz onu deyirik
		if (password.ToLower().StartsWith("1234"))
			errors.Add(new() { Code = "PasswordContaint1234", Description = "Parol 1234 ile baslaya bilmez" });


		// Error varsa geri idendity result olaraq Failed veririk ve icerisinde errorlari gosteririk
		if (errors.Any())
			return Task.FromResult(IdentityResult.Failed(errors.ToArray()));

		return Task.FromResult(IdentityResult.Success);
	}
}
