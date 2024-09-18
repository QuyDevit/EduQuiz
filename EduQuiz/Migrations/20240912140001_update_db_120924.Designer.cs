﻿// <auto-generated />
using System;
using EduQuiz.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace EduQuiz.Migrations
{
    [DbContext(typeof(EduQuizDBContext))]
    [Migration("20240912140001_update_db_120924")]
    partial class update_db_120924
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("EduQuiz.Models.EF.Choice", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Answer")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("DisplayOrder")
                        .HasColumnType("int");

                    b.Property<bool>("IsCorrect")
                        .HasColumnType("bit");

                    b.Property<int?>("QuestionId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("QuestionId");

                    b.ToTable("Choice");
                });

            modelBuilder.Entity("EduQuiz.Models.EF.EduQuiz", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ImageCover")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("MusicId")
                        .HasColumnType("int");

                    b.Property<string>("OrderQuestion")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Status")
                        .HasColumnType("bit");

                    b.Property<int?>("ThemeId")
                        .HasColumnType("int");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("Type")
                        .HasColumnType("int");

                    b.Property<DateTime?>("UpdateAt")
                        .HasColumnType("datetime2");

                    b.Property<int?>("UserId")
                        .HasColumnType("int");

                    b.Property<Guid>("Uuid")
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("Visibility")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.HasIndex("MusicId");

                    b.HasIndex("ThemeId");

                    b.HasIndex("UserId");

                    b.ToTable("EduQuiz");
                });

            modelBuilder.Entity("EduQuiz.Models.EF.Folder", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("ParentFolderId")
                        .HasColumnType("int");

                    b.Property<bool>("Status")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("UpdateAt")
                        .HasColumnType("datetime2");

                    b.Property<int?>("UserId")
                        .HasColumnType("int");

                    b.Property<Guid>("Uuid")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("ParentFolderId");

                    b.HasIndex("UserId");

                    b.ToTable("Folders");
                });

            modelBuilder.Entity("EduQuiz.Models.EF.Interest", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Status")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("Interest");
                });

            modelBuilder.Entity("EduQuiz.Models.EF.Music", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Source")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Status")
                        .HasColumnType("bit");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("UpdateAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("Music");
                });

            modelBuilder.Entity("EduQuiz.Models.EF.Question", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<int?>("EduQuizId")
                        .HasColumnType("int");

                    b.Property<string>("Image")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ImageEffect")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("PointsMultiplier")
                        .HasColumnType("int");

                    b.Property<string>("QuestionText")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Status")
                        .HasColumnType("bit");

                    b.Property<int?>("Time")
                        .HasColumnType("int");

                    b.Property<int?>("TypeAnswer")
                        .HasColumnType("int");

                    b.Property<string>("TypeQuestion")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("UpdateAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("EduQuizId");

                    b.ToTable("Question");
                });

            modelBuilder.Entity("EduQuiz.Models.EF.QuizFolder", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("EduQuizId")
                        .HasColumnType("int");

                    b.Property<int>("FolderId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("EduQuizId");

                    b.HasIndex("FolderId");

                    b.ToTable("QuizFolders");
                });

            modelBuilder.Entity("EduQuiz.Models.EF.Role", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Notes")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Status")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("UpdateAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("Role");
                });

            modelBuilder.Entity("EduQuiz.Models.EF.Theme", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Source")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Status")
                        .HasColumnType("bit");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("UpdateAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("Theme");
                });

            modelBuilder.Entity("EduQuiz.Models.EF.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DateOfBirth")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("EmailVerified")
                        .HasColumnType("bit");

                    b.Property<string>("Favorite")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LanguagePreference")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("LastLoginAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LinkToken")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Organization")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PrivacySettings")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ProfilePicture")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RefeshToken")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("RoleId")
                        .HasColumnType("int");

                    b.Property<bool>("Status")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("SubscriptionEndDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("SubscriptionStartDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("SubscriptionType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("VerificationCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ViolationCount")
                        .HasColumnType("int");

                    b.Property<int?>("WorkplaceTypeId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.HasIndex("WorkplaceTypeId");

                    b.ToTable("User");
                });

            modelBuilder.Entity("EduQuiz.Models.EF.WorkplaceType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Notes")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Status")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("UpdateAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("WorkplaceType");
                });

            modelBuilder.Entity("EduQuiz.Models.EF.Choice", b =>
                {
                    b.HasOne("EduQuiz.Models.EF.Question", "Question")
                        .WithMany("Choices")
                        .HasForeignKey("QuestionId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Question");
                });

            modelBuilder.Entity("EduQuiz.Models.EF.EduQuiz", b =>
                {
                    b.HasOne("EduQuiz.Models.EF.Music", "Music")
                        .WithMany()
                        .HasForeignKey("MusicId");

                    b.HasOne("EduQuiz.Models.EF.Theme", "Theme")
                        .WithMany()
                        .HasForeignKey("ThemeId");

                    b.HasOne("EduQuiz.Models.EF.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId");

                    b.Navigation("Music");

                    b.Navigation("Theme");

                    b.Navigation("User");
                });

            modelBuilder.Entity("EduQuiz.Models.EF.Folder", b =>
                {
                    b.HasOne("EduQuiz.Models.EF.Folder", "ParentFolder")
                        .WithMany("ChildFolders")
                        .HasForeignKey("ParentFolderId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("EduQuiz.Models.EF.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId");

                    b.Navigation("ParentFolder");

                    b.Navigation("User");
                });

            modelBuilder.Entity("EduQuiz.Models.EF.Question", b =>
                {
                    b.HasOne("EduQuiz.Models.EF.EduQuiz", "EduQuiz")
                        .WithMany("Questions")
                        .HasForeignKey("EduQuizId");

                    b.Navigation("EduQuiz");
                });

            modelBuilder.Entity("EduQuiz.Models.EF.QuizFolder", b =>
                {
                    b.HasOne("EduQuiz.Models.EF.EduQuiz", "EduQuiz")
                        .WithMany("QuizFolders")
                        .HasForeignKey("EduQuizId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("EduQuiz.Models.EF.Folder", "Folder")
                        .WithMany("QuizFolders")
                        .HasForeignKey("FolderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("EduQuiz");

                    b.Navigation("Folder");
                });

            modelBuilder.Entity("EduQuiz.Models.EF.User", b =>
                {
                    b.HasOne("EduQuiz.Models.EF.Role", "Role")
                        .WithMany()
                        .HasForeignKey("RoleId");

                    b.HasOne("EduQuiz.Models.EF.WorkplaceType", "WorkplaceType")
                        .WithMany()
                        .HasForeignKey("WorkplaceTypeId");

                    b.Navigation("Role");

                    b.Navigation("WorkplaceType");
                });

            modelBuilder.Entity("EduQuiz.Models.EF.EduQuiz", b =>
                {
                    b.Navigation("Questions");

                    b.Navigation("QuizFolders");
                });

            modelBuilder.Entity("EduQuiz.Models.EF.Folder", b =>
                {
                    b.Navigation("ChildFolders");

                    b.Navigation("QuizFolders");
                });

            modelBuilder.Entity("EduQuiz.Models.EF.Question", b =>
                {
                    b.Navigation("Choices");
                });
#pragma warning restore 612, 618
        }
    }
}
