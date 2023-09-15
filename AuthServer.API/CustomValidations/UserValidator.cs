using AuthServer.API.Models;
using Microsoft.AspNetCore.Identity;

namespace AuthServer.API.CustomValidations;

public class UserValidator : IUserValidator<UserApp>
{
	private readonly ILogger<UserValidator> _logger;

	public UserValidator(ILogger<UserValidator> logger)
	{
		_logger = logger;
	}

	public Task<IdentityResult> ValidateAsync(UserManager<UserApp> manager, UserApp user)
	{
		try
		{
			var errors = new List<IdentityError>();

			// username - in  reqem ile basladigini yoxlayiriq
			var isDigit = int.TryParse(user.UserName[0].ToString(), out _);


			if (isDigit)
				errors.Add(new IdentityError() { Code = "UserNameFirstLetterDigit", Description = "Kullanici adinin ilk xarakteri reqem ola bilmez" });

			if (errors.Any())
			{
				_logger.LogError($"User validation failed for user {user.Id}: {string.Join(", ", errors.Select(e => e.Description))}");
				return Task.FromResult(IdentityResult.Failed(errors.ToArray()));
			}

			_logger.LogInformation($"User validation successful for user {user.Id}");
			return Task.FromResult(IdentityResult.Success);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, $"An error occurred during user validation for user {user.Id}");
			throw;
		}
	}
}
