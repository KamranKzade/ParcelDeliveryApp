using AuthServer.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthServer.API.Configurations;

public class UserRefleshTokenConfiguration : IEntityTypeConfiguration<UserRefleshToken>
{
	public void Configure(EntityTypeBuilder<UserRefleshToken> builder)
	{
		builder.HasKey(x => x.UserId);
		builder.Property(x => x.RefleshToken).IsRequired().HasMaxLength(200);
	}
}
