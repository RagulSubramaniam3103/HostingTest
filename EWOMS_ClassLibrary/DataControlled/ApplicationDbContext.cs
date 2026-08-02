using EWOMS_ClassLibrary.DataIntegration;
using EWOMS_ClassLibrary.DataIntegration.ChatMessenger;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EWOMS_ClassLibrary.DataControlled
{
    public class ApplicationDbContext : IdentityDbContext<MasterUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // User table
            builder.Entity<MasterUser>().ToTable("EWO_MasterUser");

            // Role table
            builder.Entity<IdentityRole>().ToTable("EWO_MasterRole");

            // UserRoles (mapping)
            builder.Entity<IdentityUserRole<string>>().ToTable("EWO_UserRoles");

            // User Claims
            builder.Entity<IdentityUserClaim<string>>().ToTable("EWO_UserClaims");

            // Role Claims
            builder.Entity<IdentityRoleClaim<string>>().ToTable("EWO_RoleClaims");

            // User Logins (external login)
            builder.Entity<IdentityUserLogin<string>>().ToTable("EWO_UserLogins");

            // User Tokens
            builder.Entity<IdentityUserToken<string>>().ToTable("EWO_UserTokens");

            builder.Entity<MasterAdmin>().ToTable("EWO_MasterAdmin");

            builder.Entity<MasterManager>().ToTable("EWO_MasterManager");

            builder.Entity<MasterUserDetails>().ToTable("EWO_MasterUserDetails");

            builder.Entity<MasterNotification>().ToTable("EWO_MasterNotification");

            // Chat tables
            builder.Entity<Conversation>().ToTable("EWO_Conversation");
            builder.Entity<ConversationMember>().ToTable("EWO_ConversationMember");
            builder.Entity<ChatMessage>().ToTable("EWO_ChatMessage");

            builder.Entity<UserPost>().ToTable("EWO_UserPost");
            builder.Entity<UserStory>().ToTable("EWO_UserStory");
            builder.Entity<UserStoryView>().ToTable("EWO_UserStoryView");
            builder.Entity<MasterUserBackup>().ToTable("EWO_MasterUserBackup");
            builder.Entity<AdminAuditLog>().ToTable("EWO_AdminAuditLog");
            builder.Entity<DeleteUserPost>().ToTable("EWO_DeleteUserPost");
            builder.Entity<PostLike>().ToTable("EWO_PostLike");
            builder.Entity<SavedPost>().ToTable("EWO_SavedPost");
            builder.Entity<PostComment>().ToTable("EWO_PostComment");

            // Group Chat tables
            builder.Entity<ChatGroup>().ToTable("EWO_ChatGroup");
            builder.Entity<ChatGroupMember>().ToTable("EWO_ChatGroupMember");

            builder.Entity<ChatGroupMember>()
                .HasOne(m => m.Group)
                .WithMany(g => g.Members)
                .HasForeignKey(m => m.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ChatGroupMember>()
                .HasOne(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Restrict);



            //Mapping Userid from MasterUser to MasterAdmin

            builder.Entity<MasterAdmin>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Chat relationships
            builder.Entity<ConversationMember>()
                .HasOne(cm => cm.Conversation)
                .WithMany(c => c.Members)
                .HasForeignKey(cm => cm.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ConversationMember>()
                .HasOne(cm => cm.User)
                .WithMany()
                .HasForeignKey(cm => cm.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // FOLLOWER RELATIONSHIP
            builder.Entity<UserFollower>()
                .HasOne(f => f.Follower)
                .WithMany()
                .HasForeignKey(f => f.FollowerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<UserFollower>()
                .HasOne(f => f.Following)
                .WithMany()
                .HasForeignKey(f => f.FollowingId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<UserPersonalFile>().ToTable("EWO_UserPersonalFiles");
            builder.Entity<PersonalFileLike>().ToTable("EWO_PersonalFileLike");
            builder.Entity<PartnerConnection>().ToTable("EWO_PartnerConnection");
            builder.Entity<PartnerSharedPhoto>().ToTable("EWO_PartnerSharedPhoto");
            builder.Entity<UserNote>().ToTable("EWO_UserNote");
            builder.Entity<ConstructionExpense>().ToTable("EWO_ConstructionExpense");
            builder.Entity<ExpenseCategory>().ToTable("EWO_ExpenseCategory");

            // Global DateTime UTC conversion for PostgreSQL compatibility
            var dateTimeConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
                v => v.ToUniversalTime(),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            var nullableDateTimeConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime?, DateTime?>(
                v => v.HasValue ? v.Value.ToUniversalTime() : (DateTime?)null,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTime?)null);

            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                if (entityType.FindPrimaryKey() == null)
                {
                    continue;
                }

                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime))
                    {
                        property.SetValueConverter(dateTimeConverter);
                    }
                    else if (property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(nullableDateTimeConverter);
                    }
                }
            }
        }

        public DbSet<FriendRequests> EWOMS_FriendRequests { get; set; }
        public DbSet<UserFollower> EWOMS_Followers { get; set; }
        public DbSet<ChatMessage> EWOMS_ChatMessages { get; set; }

        //Extra Master Table 
        public DbSet<MasterAdmin> Master_Admins { get; set; }

        public DbSet<Master_UserPasswordLog> Master_UserPasswordLogs { get; set; }
        public DbSet<MasterManager> Master_MasterManager { get; set; }
        public DbSet<MasterUserDetails> Master_MasterUserDetails { get; set; }
        public DbSet<MasterNotification> masterNotifications { get; set; }

        // Chat DbSets
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<ConversationMember> ConversationMembers { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

        public DbSet<ChatGroup> ChatGroups { get; set; }
        public DbSet<ChatGroupMember> ChatGroupMembers { get; set; }

        public DbSet<UserPost> UserPost {  get; set; }
        public DbSet<UserStory> UserStories { get; set; }
        public DbSet<UserStoryView> UserStoryViews { get; set; }
        public DbSet<MasterUserBackup> MasterUserBackups { get; set; }
        public DbSet<AdminAuditLog> AdminAuditLogs { get; set; }
        public DbSet<DeleteUserPost> DeleteUserPost { get; set; }
        public DbSet<PostLike> PostLikes { get; set; }
        public DbSet<SavedPost> SavedPosts { get; set; }
        public DbSet<PostComment> PostComments { get; set; }
        public DbSet<UserPersonalFile> UserPersonalFiles { get; set; }
        public DbSet<PersonalFileLike> PersonalFileLikes { get; set; }
        public DbSet<PartnerConnection> PartnerConnections { get; set; }
        public DbSet<PartnerSharedPhoto> PartnerSharedPhotos { get; set; }
        public DbSet<UserNote> UserNotes { get; set; }
        public DbSet<ConstructionExpense> ConstructionExpenses { get; set; }
        public DbSet<ExpenseCategory> ExpenseCategories { get; set; }
        public DbSet<MenuAccess> MenuAccesses { get; set; }

    }
}
