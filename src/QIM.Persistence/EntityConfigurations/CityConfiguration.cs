using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QIM.Domain.Entities;

namespace QIM.Persistence.EntityConfigurations;

public class CityConfiguration : IEntityTypeConfiguration<City>
{
    public void Configure(EntityTypeBuilder<City> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.NameAr).HasMaxLength(100).IsRequired();
        builder.Property(x => x.NameEn).HasMaxLength(100).IsRequired();
        builder.HasOne(x => x.Country).WithMany(c => c.Cities).HasForeignKey(x => x.CountryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.CountryId, x.NameEn }).IsUnique();
    }
}
