﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using mylittle_project.Domain.Entities;
using System.Text.Json;

namespace mylittle_project.infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<AdminUser> AdminUsers { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<Branding> Brandings { get; set; }
        public DbSet<BrandingText> BrandingTexts { get; set; }
        public DbSet<BrandingMedia> BrandingMedia { get; set; }
        public DbSet<ContentSettings> ContentSettings { get; set; }
        public DbSet<DomainSettings> DomainSettings { get; set; }
        public DbSet<ActivityLogBuyer> ActivityLogs { get; set; }
        public DbSet<ColorPreset> ColorPresets { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductSection> ProductSections { get; set; }
        public DbSet<ProductField> ProductFields { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Buyer> Buyers { get; set; }
        public DbSet<Dealer> Dealers { get; set; }
        public DbSet<UserDealer> UserDealers { get; set; }
        public DbSet<PortalAssignment> PortalAssignments { get; set; }
        public DbSet<VirtualNumberAssignment> VirtualNumberAssignments { get; set; }
        public DbSet<KycDocumentRequest> KycDocumentRequests { get; set; }
        public DbSet<KycDocumentUpload> KycDocumentUploads { get; set; }
        public DbSet<TenantPortalLink> TenentPortalLinks { get; set; }
        public DbSet<FeatureModule> FeatureModules { get; set; }
        public DbSet<Feature> Features { get; set; }
        public DbSet<TenantFeatureModule> TenantFeatureModules { get; set; }
        public DbSet<TenantFeature> TenantFeatures { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<GlobalSubscription> GlobalSubscriptions { get; set; }
        public DbSet<TenantSubscription> TenantSubscriptions { get; set; }
        public DbSet<DealerPlanAssignment> TenantPlanAssignments { get; set; }
        public DbSet<DealerSubscriptionApplication> DealerSubscriptionApplications { get; set; }
        public DbSet<Filter> Filters { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<ProductReview> ProductReviews { get; set; }
        public DbSet<ProductTag> ProductTags { get; set; }
        public DbSet<ProductAttribute> ProductAttributes { get; set; }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is AuditableEntity &&
                           (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted));

            foreach (var entry in entries)
            {
                var entity = (AuditableEntity)entry.Entity;
                var now = DateTime.UtcNow;

                switch (entry.State)
                {
                    case EntityState.Added:
                        entity.CreatedAt = now;
                        entity.UpdatedAt = now;
                        break;

                    case EntityState.Modified:
                        entity.UpdatedAt = now;
                        break;

                    case EntityState.Deleted:
                        entry.State = EntityState.Modified; // Soft delete
                        entity.IsDeleted = true;
                        entity.DeletedAt = now;
                        entity.UpdatedAt = now;
                        break;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Convert List<string> to JSON string in Filter.Values
            var listToStringConverter = new ValueConverter<List<string>, string>(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null) ?? new List<string>()
            );

            modelBuilder.Entity<Filter>()
                .Property(f => f.Values)
                .HasConversion(listToStringConverter);

            modelBuilder.Entity<Category>()
                .HasMany(c => c.Filters)
                .WithMany(f => f.Categories);

            modelBuilder.Entity<Buyer>().HasQueryFilter(b => !b.IsDeleted);
            modelBuilder.Entity<Brand>().HasQueryFilter(b => !b.IsDeleted);

            modelBuilder.Entity<ActivityLogBuyer>()
                .HasOne(a => a.Buyer)
                .WithMany(b => b.ActivityLogs)
                .HasForeignKey(a => a.BuyerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ActivityLogBuyer>()
                .HasOne<Tenant>()
                .WithMany()
                .HasForeignKey(a => a.TenantId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Buyer)
                .WithMany(b => b.Orders)
                .HasForeignKey(o => o.BuyerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VirtualNumberAssignment>()
                .HasIndex(v => v.VirtualNumber).IsUnique();

            modelBuilder.Entity<VirtualNumberAssignment>()
                .HasIndex(v => v.DealerId).IsUnique();

            modelBuilder.Entity<VirtualNumberAssignment>()
                .HasOne(v => v.Dealer)
                .WithOne(b => b.VirtualNumberAssignment)
                .HasForeignKey<VirtualNumberAssignment>(v => v.DealerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserDealer>()
                .HasOne(u => u.Dealer)
                .WithMany()  // No collection in Dealer
                .HasForeignKey(u => u.DealerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PortalAssignment>()
                .HasOne(p => p.DealerUser)
                .WithMany(u => u.PortalAssignments)
                .HasForeignKey(p => p.DealerUserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PortalAssignment>()
                .HasOne(p => p.AssignedPortal)
                .WithMany()
                .HasForeignKey(p => p.AssignedPortalTenantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TenantPortalLink>()
                .HasOne(l => l.SourceTenant)
                .WithMany()
                .HasForeignKey(l => l.SourceTenantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TenantPortalLink>()
                .HasOne(l => l.TargetTenant)
                .WithMany()
                .HasForeignKey(l => l.TargetTenantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Branding>()
                .HasMany(b => b.ColorPresets)
                .WithOne()
                .HasForeignKey(cp => cp.BrandingId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FeatureModule>().HasIndex(m => m.Key).IsUnique();
            modelBuilder.Entity<Feature>().HasIndex(f => f.Key).IsUnique();

            modelBuilder.Entity<TenantFeatureModule>()
                .HasKey(tm => new { tm.TenantId, tm.ModuleId });

            modelBuilder.Entity<TenantFeatureModule>()
                .HasOne(tm => tm.Tenant)
                .WithMany(t => t.FeatureModules)
                .HasForeignKey(tm => tm.TenantId);

            modelBuilder.Entity<TenantFeatureModule>()
                .HasOne(tm => tm.Module)
                .WithMany()
                .HasForeignKey(tm => tm.ModuleId);

            modelBuilder.Entity<TenantFeature>()
                .HasKey(tf => new { tf.TenantId, tf.FeatureId });

            modelBuilder.Entity<TenantFeature>()
                .HasOne(tf => tf.Tenant)
                .WithMany(t => t.Features)
                .HasForeignKey(tf => tf.TenantId);

            modelBuilder.Entity<TenantFeature>()
                .HasOne(tf => tf.Feature)
                .WithMany()
                .HasForeignKey(tf => tf.FeatureId);

            modelBuilder.Entity<TenantSubscription>()
                .HasOne(ts => ts.GlobalPlan)
                .WithMany()
                .HasForeignKey(ts => ts.GlobalPlanId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DealerPlanAssignment>()
                .HasOne(e => e.Tenant)
                .WithMany()
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DealerPlanAssignment>()
                .HasOne(e => e.Category)
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DealerPlanAssignment>()
                .HasOne(e => e.Dealer)
                .WithMany()
                .HasForeignKey(e => e.DealerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DealerSubscriptionApplication>()
                .HasOne(ds => ds.Tenant)
                .WithMany()
                .HasForeignKey(ds => ds.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DealerSubscriptionApplication>()
                .HasOne(ds => ds.Dealer)
                .WithMany()
                .HasForeignKey(ds => ds.DealerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DealerSubscriptionApplication>()
                .HasOne(ds => ds.Category)
                .WithMany()
                .HasForeignKey(ds => ds.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductReview>()
                .HasOne(r => r.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductTag>()
                .HasOne(t => t.Product)
                .WithMany(p => p.Tags)
                .HasForeignKey(t => t.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
