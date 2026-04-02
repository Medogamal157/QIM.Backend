using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QIM.Domain.Entities;

namespace QIM.Persistence.EntityConfigurations;

public class BusinessAddressConfiguration : IEntityTypeConfiguration<BusinessAddress>
{
    public void Configure(EntityTypeBuilder<BusinessAddress> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.StreetName).HasMaxLength(300);
        builder.Property(x => x.BuildingNumber).HasMaxLength(50);
        builder.HasOne(x => x.Business).WithMany(b => b.Addresses).HasForeignKey(x => x.BusinessId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Country).WithMany().HasForeignKey(x => x.CountryId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.City).WithMany().HasForeignKey(x => x.CityId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.District).WithMany().HasForeignKey(x => x.DistrictId).OnDelete(DeleteBehavior.SetNull);
    }
}
