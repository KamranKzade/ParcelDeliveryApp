using AuthServer.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthServer.API.Configurations;

public class UserAppConfiguration : IEntityTypeConfiguration<UserApp>
{
	public void Configure(EntityTypeBuilder<UserApp> builder)
	{
		builder.Property(x => x.Address).HasMaxLength(50);
	}
}
