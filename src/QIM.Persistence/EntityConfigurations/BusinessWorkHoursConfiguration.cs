using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QIM.Domain.Entities;

namespace QIM.Persistence.EntityConfigurations;

public class BusinessWorkHoursConfiguration : IEntityTypeConfiguration<BusinessWorkHours>
{
    public void Configure(EntityTypeBuilder<BusinessWorkHours> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.Business).WithMany(b => b.WorkHours).HasForeignKey(x => x.BusinessId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.BusinessId, x.DayOfWeek }).IsUnique();
    }
}
