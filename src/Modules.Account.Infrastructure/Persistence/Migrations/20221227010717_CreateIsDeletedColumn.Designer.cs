﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Modules.Account.Infrastructure.Persistence;

#nullable disable

namespace Modules.Account.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(AccountDbContext))]
    [Migration("20221227010717_CreateIsDeletedColumn")]
    partial class CreateIsDeletedColumn
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("Account")
                .HasAnnotation("ProductVersion", "6.0.12")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("Modules.Account.Core.Models.Data.Account", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<string>("NickName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("Id", "IsDeleted");

                    b.ToTable("Accounts", "Account");
                });

            modelBuilder.Entity("Modules.Account.Core.Models.Data.Credential", b =>
                {
                    b.Property<int>("AuthenticationProvider")
                        .HasColumnType("int");

                    b.Property<string>("ProviderId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("AccountId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Key")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("AuthenticationProvider", "ProviderId");

                    b.HasIndex("AccountId");

                    b.ToTable("Credentials", "Account");
                });

            modelBuilder.Entity("Modules.Account.Core.Models.Data.Credential", b =>
                {
                    b.HasOne("Modules.Account.Core.Models.Data.Account", "Account")
                        .WithMany("Credentials")
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");
                });

            modelBuilder.Entity("Modules.Account.Core.Models.Data.Account", b =>
                {
                    b.Navigation("Credentials");
                });
#pragma warning restore 612, 618
        }
    }
}
