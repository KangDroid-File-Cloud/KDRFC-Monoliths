﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Modules.Account.Infrastructure.Persistence;

#nullable disable

namespace Modules.Account.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(AccountDbContext))]
    partial class AccountDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("Account")
                .HasAnnotation("ProductVersion", "7.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Modules.Account.Core.Models.Data.Account", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NickName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Accounts", "Account");
                });

            modelBuilder.Entity("Modules.Account.Core.Models.Data.Credential", b =>
                {
                    b.Property<int>("AuthenticationProvider")
                        .HasColumnType("int");

                    b.Property<string>("ProviderId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("AccountId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Key")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("AuthenticationProvider", "ProviderId");

                    b.HasIndex("AccountId");

                    b.ToTable("Credentials", "Account");
                });

            modelBuilder.Entity("Modules.Account.Core.Models.Data.Credential", b =>
                {
                    b.HasOne("Modules.Account.Core.Models.Data.Account", null)
                        .WithMany("Credentials")
                        .HasForeignKey("AccountId");
                });

            modelBuilder.Entity("Modules.Account.Core.Models.Data.Account", b =>
                {
                    b.Navigation("Credentials");
                });
#pragma warning restore 612, 618
        }
    }
}
