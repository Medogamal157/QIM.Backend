using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QIM.Domain.Entities;

namespace QIM.Persistence.EntityConfigurations;

public class DistrictConfiguration : IEntityTypeConfiguration<District>
{
    public void Configure(EntityTypeBuilder<District> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.NameAr).HasMaxLength(100).IsRequired();
        builder.Property(x => x.NameEn).HasMaxLength(100).IsRequired();
        builder.HasOne(x => x.City).WithMany(c => c.Districts).HasForeignKey(x => x.CityId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.CityId, x.NameEn }).IsUnique();
    }
}
