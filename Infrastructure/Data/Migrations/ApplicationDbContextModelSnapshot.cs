using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            modelBuilder.Entity("Domain.Entities.Role", b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
                b.Property<string>("Code").IsRequired().HasMaxLength(256).HasColumnType("nvarchar(256)");
                b.HasKey("Id");
                b.HasIndex("Code").IsUnique();
                b.ToTable("Roles");
            });

            modelBuilder.Entity("Domain.Entities.Flight", b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
                b.Property<string>("Origin").IsRequired().HasMaxLength(256).HasColumnType("nvarchar(256)");
                b.Property<string>("Destination").IsRequired().HasMaxLength(256).HasColumnType("nvarchar(256)");
                b.Property<DateTimeOffset>("Departure").HasColumnType("datetimeoffset");
                b.Property<DateTimeOffset>("Arrival").HasColumnType("datetimeoffset");
                b.Property<int>("Status").HasColumnType("int");
                b.HasKey("Id");
                b.ToTable("Flights");
            });

            modelBuilder.Entity("Domain.Entities.User", b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
                b.Property<string>("Username").IsRequired().HasMaxLength(256).HasColumnType("nvarchar(256)");
                b.Property<string>("Password").IsRequired().HasMaxLength(256).HasColumnType("nvarchar(256)");
                b.Property<int>("RoleId").HasColumnType("int");
                b.HasKey("Id");
                b.HasIndex("RoleId");
                b.HasIndex("Username").IsUnique();
                b.ToTable("Users");
            });

            modelBuilder.Entity("Domain.Entities.User", b =>
            {
                b.HasOne("Domain.Entities.Role", "Role")
                    .WithMany("Users")
                    .HasForeignKey("RoleId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();
            });
        }
    }
}
