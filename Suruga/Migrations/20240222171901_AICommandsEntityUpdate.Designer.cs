﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Suruga.Contexts;

#nullable disable

namespace Suruga.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20240222171901_AICommandsEntityUpdate")]
    partial class AICommandsEntityUpdate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.0-preview.1.24081.2");

            modelBuilder.Entity("Suruga.Contexts.Entities.AISessionEntity", b =>
                {
                    b.Property<ulong>("UserId")
                        .HasColumnType("INTEGER");

                    b.Property<byte[]>("Json")
                        .IsRequired()
                        .HasColumnType("BLOB");

                    b.HasKey("UserId");

                    b.ToTable("Sessions");
                });
#pragma warning restore 612, 618
        }
    }
}
