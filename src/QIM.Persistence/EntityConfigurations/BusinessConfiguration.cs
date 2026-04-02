using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QIM.Domain.Entities;

namespace QIM.Persistence.EntityConfigurations;

public class BusinessConfiguration : IEntityTypeConfiguration<Business>
{
    public void Configure(EntityTypeBuilder<Business> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.NameAr).HasMaxLength(300).IsRequired();
        builder.Property(x => x.NameEn).HasMaxLength(300).IsRequired();
        builder.Property(x => x.DescriptionAr).HasMaxLength(2000);
        builder.Property(x => x.DescriptionEn).HasMaxLength(2000);
        builder.Property(x => x.OwnerId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.LogoUrl).HasMaxLength(500);
        builder.Property(x => x.Email).HasMaxLength(200);
        builder.Property(x => x.Website).HasMaxLength(500);
        builder.Property(x => x.Facebook).HasMaxLength(500);
        builder.Property(x => x.Instagram).HasMaxLength(500);
        builder.Property(x => x.WhatsApp).HasMaxLength(50);
        builder.Property(x => x.Phones).HasMaxLength(500);
        builder.Property(x => x.AccountCode).HasMaxLength(50);
        builder.Property(x => x.Rating).HasDefaultValue(0);
        builder.Property(x => x.ReviewCount).HasDefaultValue(0);

        builder.HasOne(x => x.Owner).WithMany(u => u.Businesses).HasForeignKey(x => x.OwnerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Activity).WithMany(c => c.Businesses).HasForeignKey(x => x.ActivityId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Speciality).WithMany(s => s.Businesses).HasForeignKey(x => x.SpecialityId).OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.ActivityId);
        builder.HasIndex(x => x.OwnerId);
    }
}
