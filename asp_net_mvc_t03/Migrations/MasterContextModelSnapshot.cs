﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using asp_net_mvc_t03.Models;

#nullable disable

namespace asp_net_mvc_t03.Migrations
{
    [DbContext(typeof(MasterContext))]
    partial class MasterContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("asp_net_mvc_t03.Models.Message", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<int>("AuthorId")
                        .HasColumnType("int")
                        .HasColumnName("authorId");

                    b.Property<string>("Body")
                        .IsRequired()
                        .HasMaxLength(2550)
                        .IsUnicode(false)
                        .HasColumnType("varchar(2550)")
                        .HasColumnName("body")
                        .UseCollation("Cyrillic_General_CI_AS");

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("datetime2")
                        .HasColumnName("createDate");

                    b.Property<string>("Head")
                        .IsRequired()
                        .HasMaxLength(255)
                        .IsUnicode(false)
                        .HasColumnType("varchar(255)")
                        .HasColumnName("head")
                        .UseCollation("Cyrillic_General_CI_AS");

                    b.Property<bool>("New")
                        .HasColumnType("bit")
                        .HasColumnName("new");

                    b.Property<int?>("ReplyId")
                        .HasColumnType("int")
                        .HasColumnName("replyId");

                    b.Property<int>("ToUserId")
                        .HasColumnType("int")
                        .HasColumnName("toUserId");

                    b.Property<string>("Uid")
                        .IsRequired()
                        .HasMaxLength(100)
                        .IsUnicode(false)
                        .HasColumnType("varchar(100)")
                        .HasColumnName("uid");

                    b.HasKey("Id");

                    b.HasIndex("AuthorId");

                    b.HasIndex("ToUserId");

                    b.ToTable("messages", "task3");
                });

            modelBuilder.Entity("asp_net_mvc_t03.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(255)
                        .IsUnicode(false)
                        .HasColumnType("varchar(255)")
                        .HasColumnName("email");

                    b.Property<DateTime>("LastLoginDate")
                        .HasColumnType("datetime2")
                        .HasColumnName("lastLoginDate");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .IsUnicode(false)
                        .HasColumnType("varchar(255)")
                        .HasColumnName("name");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasMaxLength(255)
                        .IsUnicode(false)
                        .HasColumnType("varchar(255)")
                        .HasColumnName("password");

                    b.Property<DateTime>("RegistrationDate")
                        .HasColumnType("datetime2")
                        .HasColumnName("registrationDate");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(255)
                        .IsUnicode(false)
                        .HasColumnType("varchar(255)")
                        .HasColumnName("status");

                    b.HasKey("Id");

                    b.HasIndex(new[] { "Email" }, "users_mail_uindex")
                        .IsUnique();

                    b.ToTable("users", "task3");
                });

            modelBuilder.Entity("asp_net_mvc_t03.Models.Message", b =>
                {
                    b.HasOne("asp_net_mvc_t03.Models.User", "Author")
                        .WithMany("MessageAuthors")
                        .HasForeignKey("AuthorId")
                        .IsRequired()
                        .HasConstraintName("messages_users_id_fk");

                    b.HasOne("asp_net_mvc_t03.Models.User", "ToUser")
                        .WithMany("MessageToUsers")
                        .HasForeignKey("ToUserId")
                        .IsRequired()
                        .HasConstraintName("messages_users_id_fk_2");

                    b.Navigation("Author");

                    b.Navigation("ToUser");
                });

            modelBuilder.Entity("asp_net_mvc_t03.Models.User", b =>
                {
                    b.Navigation("MessageAuthors");

                    b.Navigation("MessageToUsers");
                });
#pragma warning restore 612, 618
        }
    }
}
