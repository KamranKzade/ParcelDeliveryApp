using System.Reflection;
using AuthServer.API.Models;
using SharedLibrary.Extentions;
using SharedLibrary.Configuration;
using FluentValidation.AspNetCore;
using AuthServer.API.Localizations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Services.Abstract;
using AuthServer.API.CustomValidations;
using AuthServer.API.Services.Abstract;
using AuthServer.API.Services.Concrete;
using SharedLibrary.UnitOfWork.Abstract;
using AuthServer.API.Repository.Concrete;
using SharedLibrary.Repositories.Abstract;

namespace AuthServer.API.Extentions;


public static class StartUpExtention
{
	public static void AddIdentityWithExtention(this IServiceCollection services)
	{
		// Token-a omur vermek
		services.Configure<DataProtectionTokenProviderOptions>(options =>
		{
			options.TokenLifespan = TimeSpan.FromHours(2);
		});

		// security stampa omur vermek
		services.Configure<SecurityStampValidatorOptions>(opt =>
		{
			opt.ValidationInterval = TimeSpan.FromMinutes(30);
		});

		services.AddIdentity<UserApp, IdentityRole>(opt =>
		{
			opt.User.RequireUniqueEmail = true;
			opt.User.AllowedUserNameCharacters = "abcdefghijklmnoprstuvwxyz1234567890_";

			opt.Password.RequiredLength = 6; // uzunluq
			opt.Password.RequireNonAlphanumeric = false; // Simvollar olmasada olar
			opt.Password.RequireLowercase = true; // Kicik herfe mutlerolmalidir
			opt.Password.RequireUppercase = false; // botuk olmasada olar
			opt.Password.RequireDigit = false; // reqem olmasada olar

			opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
			opt.Lockout.MaxFailedAccessAttempts = 5;
		}).AddUserValidator<UserValidator>()
		.AddErrorDescriber<LocalizationIdentityErrorDescriber>()
		.AddPasswordValidator<PasswordValidator>()
		.AddDefaultTokenProviders() // Deafult token aliriq
		.AddEntityFrameworkStores<AppDbContext>();

	}

	public static void AddScopeWithExtention(this IServiceCollection services)
	{
		services.AddScoped<IAuthenticationService, AuthenticationService>();
		services.AddScoped<IUserService, UserService>();
		services.AddScoped<ITokenService, TokenService>();
		services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
		services.AddScoped(typeof(IServiceGeneric<,>), typeof(ServiceGeneric<,>));
		services.AddScoped<IUnitOfWork, UnitOfWork.Concrete.UnitOfWork>();
	}


	public static void AddDbContextExtention(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddDbContext<AppDbContext>(options =>
		{
			options.UseSqlServer(configuration.GetConnectionString("SqlServer"));
		});
	}

	public static void AddCustomTokenAuthExtention(this IServiceCollection services, IConfiguration configuration)
	{
		// TokenOption elave edirik Configure-a 
		// Option pattern --> DI uzerinden appsetting-deki datalari elde etmeye deyilir.
		services.Configure<CustomTokenOption>(configuration.GetSection("TokenOptions"));
		var tokenOptions = configuration.GetSection("TokenOptions").Get<CustomTokenOption>();
		services.AddCustomTokenAuth(tokenOptions);
	}

	public static void AddControllersExtention(this IServiceCollection services)
	{
		services.AddControllers()
				.AddFluentValidation(opts => // FluentValidationlari sisteme tanidiriq
				{
					opts.RegisterValidatorsFromAssembly(Assembly.GetExecutingAssembly());
				});
	}
}
