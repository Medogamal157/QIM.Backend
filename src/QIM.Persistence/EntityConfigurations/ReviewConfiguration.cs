using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QIM.Domain.Entities;

namespace QIM.Persistence.EntityConfigurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UserId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.Comment).HasMaxLength(1000);
        builder.Property(x => x.FlagReason).HasMaxLength(500);
        builder.Property(x => x.FlaggedByUserId).HasMaxLength(450);

        builder.HasOne(x => x.Business).WithMany(b => b.Reviews).HasForeignKey(x => x.BusinessId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.User).WithMany(u => u.Reviews).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.FlaggedByUser).WithMany().HasForeignKey(x => x.FlaggedByUserId).OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.BusinessId);
        builder.HasIndex(x => x.Status);
    }
}
