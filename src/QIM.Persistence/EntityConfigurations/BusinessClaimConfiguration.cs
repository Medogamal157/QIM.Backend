using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QIM.Domain.Entities;

namespace QIM.Persistence.EntityConfigurations;

public class BusinessClaimConfiguration : IEntityTypeConfiguration<BusinessClaim>
{
    public void Configure(EntityTypeBuilder<BusinessClaim> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UserId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.Phone).HasMaxLength(20);
        builder.Property(x => x.Email).HasMaxLength(200);
        builder.Property(x => x.Message).HasMaxLength(2000);
        builder.Property(x => x.DocumentUrls).HasMaxLength(2000);
        builder.HasOne(x => x.Business).WithMany(b => b.Claims).HasForeignKey(x => x.BusinessId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.User).WithMany(u => u.BusinessClaims).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}
