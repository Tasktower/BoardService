using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tasktower.Lib.Aspnetcore.DataAccess.Entities;
using Tasktower.ProjectService.Tools.Constants;

namespace Tasktower.ProjectService.DataAccess.Entities
{
    public class ProjectRoleEntity : AuditableEntity
    {
        public Guid Id { get; set; }
        public ProjectRoleValue Role { get; set; }
        public bool PendingInvite { get; set; }
        public string UserId { get; set; }
        public virtual UserEntity UserEntity { get; set; }
        public Guid ProjectId { get; set; }
        public virtual ProjectEntity ProjectEntity { get; set; }
        public static void BuildEntity(EntityTypeBuilder<ProjectRoleEntity > entityTypeBuilder)
        {
            entityTypeBuilder = entityTypeBuilder.ToTable("project_roles");
            BuildAuditableEntity(entityTypeBuilder);
            
            entityTypeBuilder.Property(e => e.Id)
                .HasColumnName("id")
                .IsRequired()
                .HasDefaultValueSql("NEWSEQUENTIALID()");
            entityTypeBuilder.HasKey(e => e.Id);

            entityTypeBuilder.Property(e => e.UserId)
                .HasColumnName("user_id")
                .HasMaxLength(100)
                .IsRequired();

            entityTypeBuilder.Property(e => e.Role)
                .HasConversion(
                    r => r.ToString(),
                    s => Enum.Parse<ProjectRoleValue>(s))
                .HasColumnName("role")
                .HasMaxLength(20)
                .IsRequired();

            entityTypeBuilder.Property(e => e.PendingInvite)
                .HasColumnName("pending_invite")
                .IsRequired();
                // .HasDefaultValue(true);

            entityTypeBuilder.Property(e => e.ProjectId)
                .HasColumnName("project_id")
                .IsRequired();

            entityTypeBuilder
                .HasOne(e => e.ProjectEntity)
                .WithMany(t => t.ProjectRoles)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entityTypeBuilder
                .HasOne(e => e.UserEntity)
                .WithMany(t => t.ProjectRoles)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entityTypeBuilder
                .HasIndex(e => new {e.ProjectId, e.UserId})
                .IsUnique();
        }

    }
}
