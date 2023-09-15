using AuthServer.API.Models;
using Microsoft.AspNetCore.Identity;

namespace AuthServer.API.CustomValidations;

public class PasswordValidator : IPasswordValidator<UserApp>
{
	private readonly ILogger<PasswordValidator> _logger;

	public PasswordValidator(ILogger<PasswordValidator> logger)
	{
		_logger = logger;
	}

	public Task<IdentityResult> ValidateAsync(UserManager<UserApp> manager, UserApp user, string password)
	{
		try
		{
			var errors = new List<IdentityError>();

			// Password da username olub olmadigini yoxlayiriq
			if (password.ToLower().Contains(user.UserName.ToLower()))
			{
				errors.Add(new IdentityError() { Code = "PasswrodContainUserNmae", Description = "Parolda username ola bilmez" });
			}

			// Password un ne ile basladigini gosteririk ve neye icaze olmaz onu deyirik
			if (password.ToLower().StartsWith("1234"))
			{
				errors.Add(new() { Code = "PasswordContaint1234", Description = "Parol 1234 ile baslaya bilmez" });
			}


			// Error varsa geri idendity result olaraq Failed veririk ve icerisinde errorlari gosteririk
			if (errors.Any())
			{
				_logger.LogError($"Password validation failed for user {user.Id}: {string.Join(", ", errors.Select(e => e.Description))}");
				return Task.FromResult(IdentityResult.Failed(errors.ToArray()));
			}

			_logger.LogInformation("Password validation successful for user {UserId}", user.Id);
			return Task.FromResult(IdentityResult.Success);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, $"An error occurred during password validation for user {user.Id}");
			throw;
		}
	}
}
