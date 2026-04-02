using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QIM.Domain.Entities;

namespace QIM.Persistence.EntityConfigurations;

public class SpecialityConfiguration : IEntityTypeConfiguration<Speciality>
{
    public void Configure(EntityTypeBuilder<Speciality> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.NameAr).HasMaxLength(200).IsRequired();
        builder.Property(x => x.NameEn).HasMaxLength(200).IsRequired();
        builder.HasOne(x => x.Activity).WithMany(c => c.Specialities).HasForeignKey(x => x.ActivityId).OnDelete(DeleteBehavior.Restrict);
    }
}
