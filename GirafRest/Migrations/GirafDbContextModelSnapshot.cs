﻿using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using GirafRest.Data;
using GirafRest.Models;

namespace GirafRest.Migrations
{
    [DbContext(typeof(GirafDbContext))]
    partial class GirafDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.1");

            modelBuilder.Entity("GirafRest.Models.Department", b =>
                {
                    b.Property<long>("Key")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.HasKey("Key");

                    b.ToTable("Departments");
                });

            modelBuilder.Entity("GirafRest.Models.Frame", b =>
                {
                    b.Property<long>("Key")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<string>("Discriminator")
                        .IsRequired();

                    b.Property<DateTime>("lastEdit");

                    b.HasKey("Key");

                    b.ToTable("Frames");

                    b.HasDiscriminator<string>("Discriminator").HasValue("Frame");
                });

            modelBuilder.Entity("GirafRest.Models.GirafUser", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AccessFailedCount");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<long?>("DepartmentKey");

                    b.Property<string>("Email")
                        .HasMaxLength(256);

                    b.Property<bool>("EmailConfirmed");

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256);

                    b.Property<string>("PasswordHash");

                    b.Property<string>("PhoneNumber");

                    b.Property<bool>("PhoneNumberConfirmed");

                    b.Property<string>("SecurityStamp");

                    b.Property<bool>("TwoFactorEnabled");

                    b.Property<string>("UserName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("DepartmentKey");

                    b.HasIndex("NormalizedEmail")
                        .HasName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasName("UserNameIndex");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Name")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasName("RoleNameIndex");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("RoleId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider");

                    b.Property<string>("ProviderKey");

                    b.Property<string>("ProviderDisplayName");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("RoleId");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("LoginProvider");

                    b.Property<string>("Name");

                    b.Property<string>("Value");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("GirafRest.Models.Choice", b =>
                {
                    b.HasBaseType("GirafRest.Models.Frame");


                    b.ToTable("Choices");

                    b.HasDiscriminator().HasValue("Choice");
                });

            modelBuilder.Entity("GirafRest.Models.PictoFrame", b =>
                {
                    b.HasBaseType("GirafRest.Models.Frame");

                    b.Property<int>("AccessLevel");

                    b.Property<long?>("DepartmentKey");

                    b.Property<string>("GirafUserId");

                    b.Property<string>("Title")
                        .IsRequired();

                    b.HasIndex("DepartmentKey");

                    b.HasIndex("GirafUserId");

                    b.ToTable("PictoFrames");

                    b.HasDiscriminator().HasValue("PictoFrame");
                });

            modelBuilder.Entity("GirafRest.Models.Pictogram", b =>
                {
                    b.HasBaseType("GirafRest.Models.PictoFrame");


                    b.ToTable("Pictograms");

                    b.HasDiscriminator().HasValue("Pictogram");
                });

            modelBuilder.Entity("GirafRest.Models.Sequence", b =>
                {
                    b.HasBaseType("GirafRest.Models.PictoFrame");

                    b.Property<long>("ThumbnailKey");

                    b.HasIndex("ThumbnailKey");

                    b.ToTable("Sequences");

                    b.HasDiscriminator().HasValue("Sequence");
                });

            modelBuilder.Entity("GirafRest.Models.GirafUser", b =>
                {
                    b.HasOne("GirafRest.Models.Department")
                        .WithMany("Members")
                        .HasForeignKey("DepartmentKey");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole")
                        .WithMany("Claims")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("GirafRest.Models.GirafUser")
                        .WithMany("Claims")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("GirafRest.Models.GirafUser")
                        .WithMany("Logins")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole")
                        .WithMany("Users")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("GirafRest.Models.GirafUser")
                        .WithMany("Roles")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("GirafRest.Models.PictoFrame", b =>
                {
                    b.HasOne("GirafRest.Models.Department")
                        .WithMany("Resources")
                        .HasForeignKey("DepartmentKey");

                    b.HasOne("GirafRest.Models.GirafUser")
                        .WithMany("Resources")
                        .HasForeignKey("GirafUserId");
                });

            modelBuilder.Entity("GirafRest.Models.Sequence", b =>
                {
                    b.HasOne("GirafRest.Models.Pictogram", "Thumbnail")
                        .WithMany()
                        .HasForeignKey("ThumbnailKey")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
