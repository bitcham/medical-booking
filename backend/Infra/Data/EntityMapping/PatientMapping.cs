using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infra.Data.EntityMapping;

public class PatientMapping : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.PhoneNumber)
            .HasMaxLength(20)
            .IsRequired();

        builder.OwnsOne(x => x.Address, a =>
        {
            a.Property(p => p.Street).HasMaxLength(100).IsRequired();
            a.Property(p => p.City).HasMaxLength(50).IsRequired();
            a.Property(p => p.ZipCode).HasMaxLength(20).IsRequired();
            a.Property(p => p.Country).HasMaxLength(50).IsRequired();
        });
        
        // One-to-One relationship with User
        builder.HasOne(x => x.User)
            .WithOne()
            .HasForeignKey<Patient>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
