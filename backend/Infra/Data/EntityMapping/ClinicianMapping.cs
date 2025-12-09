using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infra.Data.EntityMapping;

public class ClinicianMapping : IEntityTypeConfiguration<Clinician>
{
    public void Configure(EntityTypeBuilder<Clinician> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.LicenseNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Specialization)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Bio)
            .HasMaxLength(1000);

        // One-to-One relationship with User
        builder.HasOne(x => x.User)
            .WithOne()
            .HasForeignKey<Clinician>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
