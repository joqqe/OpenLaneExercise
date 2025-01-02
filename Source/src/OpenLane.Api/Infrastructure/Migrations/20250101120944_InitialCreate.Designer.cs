﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OpenLane.Api.Infrastructure;

#nullable disable

namespace OpenLane.Api.Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20250101120944_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("OpenLane.Api.Domain.Bid", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<Guid>("ObjectId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("OfferId")
                        .HasColumnType("int");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(18, 6))");

                    b.Property<DateTimeOffset>("ReceivedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<Guid>("User")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("ObjectId")
                        .IsUnique();

                    b.HasIndex("OfferId");

                    b.ToTable("Bids");
                });

            modelBuilder.Entity("OpenLane.Api.Domain.Offer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTimeOffset>("ClosesAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<Guid>("ObjectId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset>("OpensAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<int>("ProductId")
                        .HasColumnType("int");

                    b.Property<decimal>("StartingPrice")
                        .HasColumnType("decimal(18, 6))");

                    b.HasKey("Id");

                    b.HasIndex("ObjectId")
                        .IsUnique();

                    b.HasIndex("ProductId");

                    b.ToTable("Offers");
                });

            modelBuilder.Entity("OpenLane.Api.Domain.Product", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("ObjectId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("ObjectId")
                        .IsUnique();

                    b.ToTable("Products");
                });

            modelBuilder.Entity("OpenLane.Api.Domain.Bid", b =>
                {
                    b.HasOne("OpenLane.Api.Domain.Offer", "Offer")
                        .WithMany("Bids")
                        .HasForeignKey("OfferId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Offer");
                });

            modelBuilder.Entity("OpenLane.Api.Domain.Offer", b =>
                {
                    b.HasOne("OpenLane.Api.Domain.Product", "Product")
                        .WithMany("Offers")
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Product");
                });

            modelBuilder.Entity("OpenLane.Api.Domain.Offer", b =>
                {
                    b.Navigation("Bids");
                });

            modelBuilder.Entity("OpenLane.Api.Domain.Product", b =>
                {
                    b.Navigation("Offers");
                });
#pragma warning restore 612, 618
        }
    }
}