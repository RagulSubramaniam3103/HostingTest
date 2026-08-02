using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EWOMS_ClassLibrary.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgreSQL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EWO_AdminAuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AdminId = table.Column<string>(type: "text", nullable: false),
                    AdminName = table.Column<string>(type: "text", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    TargetId = table.Column<string>(type: "text", nullable: true),
                    TargetName = table.Column<string>(type: "text", nullable: true),
                    Details = table.Column<string>(type: "text", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IpAddress = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EWO_AdminAuditLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EWO_ChatGroup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    ProfileImage = table.Column<string>(type: "text", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EWO_ChatGroup", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EWO_ConstructionExpense",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ItemName = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    SpentOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EWO_ConstructionExpense", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EWO_Conversation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EWO_Conversation", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EWO_DeleteUserPost",
                columns: table => new
                {
                    SNo = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ProfileImage = table.Column<byte[]>(type: "bytea", nullable: true),
                    Caption = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedBy = table.Column<string>(type: "text", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EWO_DeleteUserPost", x => x.SNo);
                });

            migrationBuilder.CreateTable(
                name: "EWO_ExpenseCategory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EWO_ExpenseCategory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EWO_MasterNotification",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EWO_MasterNotification", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EWO_MasterRole",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EWO_MasterRole", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EWO_MasterUser",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: true),
                    CreatedUser = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ProfileImage = table.Column<byte[]>(type: "bytea", nullable: true),
                    IsPrivate = table.Column<bool>(type: "boolean", nullable: false),
                    ComicAccess = table.Column<bool>(type: "boolean", nullable: false),
                    PrivateImageAccess = table.Column<bool>(type: "boolean", nullable: false),
                    PrivateVideoAccess = table.Column<bool>(type: "boolean", nullable: false),
                    PrivateMenuPassword = table.Column<string>(type: "text", nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EWO_MasterUser", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EWO_MasterUserBackup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: true),
                    UserRole = table.Column<string>(type: "text", nullable: true),
                    CreatedUser = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProfileImage = table.Column<byte[]>(type: "bytea", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EWO_MasterUserBackup", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EWO_MenuAccess",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    MenuName = table.Column<string>(type: "text", nullable: false),
                    HasAccess = table.Column<bool>(type: "boolean", nullable: false),
                    IsPrivate = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EWO_MenuAccess", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EWO_PartnerConnection",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SenderId = table.Column<string>(type: "text", nullable: false),
                    ReceiverId = table.Column<string>(type: "text", nullable: false),
                    IsAccepted = table.Column<bool>(type: "boolean", nullable: false),
                    RequestDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConnectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EWO_PartnerConnection", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EWO_PartnerSharedPhoto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConnectionId = table.Column<int>(type: "integer", nullable: false),
                    UploaderId = table.Column<string>(type: "text", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    FilePath = table.Column<string>(type: "text", nullable: false),
                    FileType = table.Column<string>(type: "text", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    SharedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EWO_PartnerSharedPhoto", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EWO_PostComment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PostId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EWO_PostComment", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EWO_PostLike",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PostId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LikedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EWO_PostLike", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EWO_SavedPost",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PostId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    SavedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EWO_SavedPost", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EWO_UserNote",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Content = table.Column<string>(type: "text", nullable: true),
                    Color = table.Column<string>(type: "text", nullable: false),
                    IsFlagged = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EWO_UserNote", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EWO_UserPost",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    profileimage = table.Column<byte[]>(type: "bytea", nullable: true),
                    Caption = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsBlurred = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    ModeratedBy = table.Column<string>(type: "text", nullable: true),
                    ModeratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VideoUrl = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EWO_UserPost", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EWO_UserStory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    StoryImage = table.Column<byte[]>(type: "bytea", nullable: false),
                    Caption = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsBlurred = table.Column<bool>(type: "boolean", nullable: false),
                    ViewCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EWO_UserStory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EWOMS_FriendRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SenderId = table.Column<string>(type: "text", nullable: false),
                    ReceiverId = table.Column<string>(type: "text", nullable: false),
                    RequestDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EWOMS_FriendRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Master_UserPasswordLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Master_UserPasswordLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EWO_ChatMessage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SenderId = table.Column<string>(type: "text", nullable: false),
                    ReceiverId = table.Column<string>(type: "text", nullable: true),
                    GroupId = table.Column<int>(type: "integer", nullable: true),
                    Message = table.Column<string>(type: "text", nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDelivered = table.Column<bool>(type: "boolean", nullable: false),
                    DeliveredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Image = table.Column<string>(type: "text", nullable: true),
                    Video = table.Column<string>(type: "text", nullable: true),
                    Document = table.Column<string>(type: "text", nullable: true),
                    FileName = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    ConversationId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EWO_ChatMessage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EWO_ChatMessage_EWO_Conversation_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "EWO_Conversation",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EWO_RoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EWO_RoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EWO_RoleClaims_EWO_MasterRole_RoleId",
                        column: x => x.RoleId,
                        principalTable: "EWO_MasterRole",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EWO_ChatGroupMember",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GroupId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsAdmin = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EWO_ChatGroupMember", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EWO_ChatGroupMember_EWO_ChatGroup_GroupId",
                        column: x => x.GroupId,
                        principalTable: "EWO_ChatGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EWO_ChatGroupMember_EWO_MasterUser_UserId",
                        column: x => x.UserId,
                        principalTable: "EWO_MasterUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EWO_ConversationMember",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConversationId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EWO_ConversationMember", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EWO_ConversationMember_EWO_Conversation_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "EWO_Conversation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EWO_ConversationMember_EWO_MasterUser_UserId",
                        column: x => x.UserId,
                        principalTable: "EWO_MasterUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EWO_Followers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FollowerId = table.Column<string>(type: "text", nullable: false),
                    FollowingId = table.Column<string>(type: "text", nullable: false),
                    FollowedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EWO_Followers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EWO_Followers_EWO_MasterUser_FollowerId",
                        column: x => x.FollowerId,
                        principalTable: "EWO_MasterUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EWO_Followers_EWO_MasterUser_FollowingId",
                        column: x => x.FollowingId,
                        principalTable: "EWO_MasterUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EWO_MasterAdmin",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    UserName = table.Column<string>(type: "text", nullable: true),
                    FullName = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Password = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    Address1 = table.Column<string>(type: "text", nullable: true),
                    Address2 = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "text", nullable: true),
                    State = table.Column<string>(type: "text", nullable: true),
                    PostalCode = table.Column<string>(type: "text", nullable: true),
                    Country = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EWO_MasterAdmin", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EWO_MasterAdmin_EWO_MasterUser_UserId",
                        column: x => x.UserId,
                        principalTable: "EWO_MasterUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EWO_MasterManager",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    UserName = table.Column<string>(type: "text", nullable: true),
                    FullName = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Password = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    Address1 = table.Column<string>(type: "text", nullable: true),
                    Address2 = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "text", nullable: true),
                    State = table.Column<string>(type: "text", nullable: true),
                    PostalCode = table.Column<string>(type: "text", nullable: true),
                    Country = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EWO_MasterManager", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EWO_MasterManager_EWO_MasterUser_UserId",
                        column: x => x.UserId,
                        principalTable: "EWO_MasterUser",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EWO_MasterUserDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    UserName = table.Column<string>(type: "text", nullable: true),
                    FullName = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Password = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    Address1 = table.Column<string>(type: "text", nullable: true),
                    Address2 = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "text", nullable: true),
                    State = table.Column<string>(type: "text", nullable: true),
                    PostalCode = table.Column<string>(type: "text", nullable: true),
                    Country = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EWO_MasterUserDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EWO_MasterUserDetails_EWO_MasterUser_UserId",
                        column: x => x.UserId,
                        principalTable: "EWO_MasterUser",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EWO_UserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EWO_UserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EWO_UserClaims_EWO_MasterUser_UserId",
                        column: x => x.UserId,
                        principalTable: "EWO_MasterUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EWO_UserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EWO_UserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_EWO_UserLogins_EWO_MasterUser_UserId",
                        column: x => x.UserId,
                        principalTable: "EWO_MasterUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EWO_UserPersonalFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    FilePath = table.Column<string>(type: "text", nullable: false),
                    FileType = table.Column<string>(type: "text", nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    UploadDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EWO_UserPersonalFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EWO_UserPersonalFiles_EWO_MasterUser_UserId",
                        column: x => x.UserId,
                        principalTable: "EWO_MasterUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EWO_UserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EWO_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_EWO_UserRoles_EWO_MasterRole_RoleId",
                        column: x => x.RoleId,
                        principalTable: "EWO_MasterRole",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EWO_UserRoles_EWO_MasterUser_UserId",
                        column: x => x.UserId,
                        principalTable: "EWO_MasterUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EWO_UserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EWO_UserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_EWO_UserTokens_EWO_MasterUser_UserId",
                        column: x => x.UserId,
                        principalTable: "EWO_MasterUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EWO_UserStoryView",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StoryId = table.Column<int>(type: "integer", nullable: false),
                    ViewerId = table.Column<string>(type: "text", nullable: false),
                    ViewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EWO_UserStoryView", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EWO_UserStoryView_EWO_UserStory_StoryId",
                        column: x => x.StoryId,
                        principalTable: "EWO_UserStory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EWO_PersonalFileLike",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FileId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LikedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EWO_PersonalFileLike", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EWO_PersonalFileLike_EWO_MasterUser_UserId",
                        column: x => x.UserId,
                        principalTable: "EWO_MasterUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EWO_PersonalFileLike_EWO_UserPersonalFiles_FileId",
                        column: x => x.FileId,
                        principalTable: "EWO_UserPersonalFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EWO_ChatGroupMember_GroupId",
                table: "EWO_ChatGroupMember",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_EWO_ChatGroupMember_UserId",
                table: "EWO_ChatGroupMember",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EWO_ChatMessage_ConversationId",
                table: "EWO_ChatMessage",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_EWO_ConversationMember_ConversationId",
                table: "EWO_ConversationMember",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_EWO_ConversationMember_UserId",
                table: "EWO_ConversationMember",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EWO_Followers_FollowerId",
                table: "EWO_Followers",
                column: "FollowerId");

            migrationBuilder.CreateIndex(
                name: "IX_EWO_Followers_FollowingId",
                table: "EWO_Followers",
                column: "FollowingId");

            migrationBuilder.CreateIndex(
                name: "IX_EWO_MasterAdmin_UserId",
                table: "EWO_MasterAdmin",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EWO_MasterManager_UserId",
                table: "EWO_MasterManager",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "EWO_MasterRole",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "EWO_MasterUser",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "EWO_MasterUser",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EWO_MasterUserDetails_UserId",
                table: "EWO_MasterUserDetails",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EWO_PersonalFileLike_FileId",
                table: "EWO_PersonalFileLike",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_EWO_PersonalFileLike_UserId",
                table: "EWO_PersonalFileLike",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EWO_RoleClaims_RoleId",
                table: "EWO_RoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_EWO_UserClaims_UserId",
                table: "EWO_UserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EWO_UserLogins_UserId",
                table: "EWO_UserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EWO_UserPersonalFiles_UserId",
                table: "EWO_UserPersonalFiles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EWO_UserRoles_RoleId",
                table: "EWO_UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_EWO_UserStoryView_StoryId",
                table: "EWO_UserStoryView",
                column: "StoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EWO_AdminAuditLog");

            migrationBuilder.DropTable(
                name: "EWO_ChatGroupMember");

            migrationBuilder.DropTable(
                name: "EWO_ChatMessage");

            migrationBuilder.DropTable(
                name: "EWO_ConstructionExpense");

            migrationBuilder.DropTable(
                name: "EWO_ConversationMember");

            migrationBuilder.DropTable(
                name: "EWO_DeleteUserPost");

            migrationBuilder.DropTable(
                name: "EWO_ExpenseCategory");

            migrationBuilder.DropTable(
                name: "EWO_Followers");

            migrationBuilder.DropTable(
                name: "EWO_MasterAdmin");

            migrationBuilder.DropTable(
                name: "EWO_MasterManager");

            migrationBuilder.DropTable(
                name: "EWO_MasterNotification");

            migrationBuilder.DropTable(
                name: "EWO_MasterUserBackup");

            migrationBuilder.DropTable(
                name: "EWO_MasterUserDetails");

            migrationBuilder.DropTable(
                name: "EWO_MenuAccess");

            migrationBuilder.DropTable(
                name: "EWO_PartnerConnection");

            migrationBuilder.DropTable(
                name: "EWO_PartnerSharedPhoto");

            migrationBuilder.DropTable(
                name: "EWO_PersonalFileLike");

            migrationBuilder.DropTable(
                name: "EWO_PostComment");

            migrationBuilder.DropTable(
                name: "EWO_PostLike");

            migrationBuilder.DropTable(
                name: "EWO_RoleClaims");

            migrationBuilder.DropTable(
                name: "EWO_SavedPost");

            migrationBuilder.DropTable(
                name: "EWO_UserClaims");

            migrationBuilder.DropTable(
                name: "EWO_UserLogins");

            migrationBuilder.DropTable(
                name: "EWO_UserNote");

            migrationBuilder.DropTable(
                name: "EWO_UserPost");

            migrationBuilder.DropTable(
                name: "EWO_UserRoles");

            migrationBuilder.DropTable(
                name: "EWO_UserStoryView");

            migrationBuilder.DropTable(
                name: "EWO_UserTokens");

            migrationBuilder.DropTable(
                name: "EWOMS_FriendRequests");

            migrationBuilder.DropTable(
                name: "Master_UserPasswordLogs");

            migrationBuilder.DropTable(
                name: "EWO_ChatGroup");

            migrationBuilder.DropTable(
                name: "EWO_Conversation");

            migrationBuilder.DropTable(
                name: "EWO_UserPersonalFiles");

            migrationBuilder.DropTable(
                name: "EWO_MasterRole");

            migrationBuilder.DropTable(
                name: "EWO_UserStory");

            migrationBuilder.DropTable(
                name: "EWO_MasterUser");
        }
    }
}
