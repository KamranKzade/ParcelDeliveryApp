using Microsoft.AspNetCore.Identity;

namespace AuthServer.API.Localizations;


public class LocalizationIdentityErrorDescriber : IdentityErrorDescriber
{
	public override IdentityError DuplicateEmail(string email)
	{
		return new IdentityError()
		{
			Code = "DuplicateEmail",
			Description = $"{email} artiq basqa bir user tərəfindən istifadə olunub"
		};
	}

	public override IdentityError DuplicateUserName(string userName)
	{
		return new IdentityError()
		{
			Code = "DuplicateUsername",
			Description = $"{userName} artiq basqa bir user tərəfindən istifadə olunub"
		};
	}

	public override IdentityError PasswordTooShort(int length)
	{
		return new IdentityError()
		{
			Code = "PasswordTooShort",
			Description = $"Parol ən az {length} xarakterdən ibarət olmalıdır"
		};
	}
}
