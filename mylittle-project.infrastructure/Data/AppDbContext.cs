using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using mylittle_project.Application.DTOs;
using mylittle_project.Domain.Entities;
using System.Collections.Generic;
using System.Text.Json;

namespace mylittle_project.infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSets
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

        // Products & Listings
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Buyer> Buyers { get; set; }
        public DbSet<BusinessInfo> BusinessInfos { get; set; }
        public DbSet<SubscriptionDealer> DealerSubscriptions { get; set; }
        public DbSet<AssignedCategory> AssignedCategories { get; set; }
        public DbSet<UserDealer> UserDealers { get; set; }
        public DbSet<PortalAssignment> PortalAssignments { get; set; }
        public DbSet<VirtualNumberAssignment> VirtualNumberAssignments { get; set; }
        public DbSet<KycDocumentRequest> KycDocumentRequests { get; set; }
        public DbSet<KycDocumentUpload> KycDocumentUploads { get; set; }

        public DbSet<TenentPortalLink> TenentPortalLinks { get; set; }

        public DbSet<FeatureModule> FeatureModules { get; set; }
        public DbSet<Feature> Features { get; set; }
        public DbSet<TenantFeatureModule> TenantFeatureModules { get; set; }
        public DbSet<TenantFeature> TenantFeatures { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<GlobalSubscription> GlobalSubscriptions { get; set; }
        public DbSet<TenantSubscription> TenantSubscriptions { get; set; }
        public DbSet<TenantPlanAssignment> TenantPlanAssignments { get; set; }
        public DbSet<Filter> Filters { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // JSON conversion for List<string>
            // JSON conversion for List<string> (Filter.Values)
            var listToStringConverter = new ValueConverter<List<string>, string>(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null) ?? new List<string>()
            );

            // TenantPlanAssignment Config
            modelBuilder.Entity<TenantPlanAssignment>(entity =>
            {
                entity.ToTable("TenantPlanAssignments");

                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Tenant)
                      .WithMany()
                      .HasForeignKey(e => e.TenantId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Category)
                      .WithMany()
                      .HasForeignKey(e => e.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Dealer)
                      .WithMany()
                      .HasForeignKey(e => e.DealerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(e => e.PlanType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            });

            // Filter.Value JSON conversion
            modelBuilder.Entity<Filter>()
                .Property(f => f.Values)
                .HasConversion(listToStringConverter);

            // Configure one-to-many relationship: Category → Products
            modelBuilder.Entity<Category>()
                .HasMany(c => c.Products)
                .WithOne(p => p.Category)
                .HasForeignKey(p => p.CategoryId);

            // Configure many-to-many relationship: Category ↔ Filters
            modelBuilder.Entity<Category>()
                .HasMany(c => c.Filters)
                .WithMany(f => f.Categories);

            base.OnModelCreating(modelBuilder);

            // 👇 Keep all other config same...
            modelBuilder.Entity<Buyer>().HasQueryFilter(b => !b.IsDeleted);

            modelBuilder.Entity<ActivityLogBuyer>().HasKey(a => a.Id);

            modelBuilder.Entity<ActivityLogBuyer>()
                .HasOne(a => a.Buyer)
                .WithMany(b => b.ActivityLogs)
                .HasForeignKey(a => a.BuyerId);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Buyer)
                .WithMany(b => b.Orders)
                .HasForeignKey(o => o.BuyerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VirtualNumberAssignment>()
                .HasIndex(v => v.VirtualNumber).IsUnique();

            modelBuilder.Entity<VirtualNumberAssignment>()
                .HasIndex(v => v.BusinessId).IsUnique();

            modelBuilder.Entity<VirtualNumberAssignment>()
                .HasOne(v => v.BusinessInfo)
                .WithOne(b => b.VirtualNumberAssignment)
                .HasForeignKey<VirtualNumberAssignment>(v => v.BusinessId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BusinessInfo>()
                .HasOne(b => b.UserDealer)
                .WithMany(u => u.BusinessInfos)
                .HasForeignKey(b => b.UserDealerId)
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

            modelBuilder.Entity<SubscriptionDealer>()
                .OwnsMany(s => s.Categories);

            modelBuilder.Entity<SubscriptionDealer>()
                .HasOne(s => s.BusinessInfo)
                .WithMany()
                .HasForeignKey(s => s.BusinessId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SubscriptionDealer>()
                .HasIndex(s => new { s.BusinessId, s.TenantId }).IsUnique();

            modelBuilder.Entity<TenentPortalLink>(entity =>
            {
                entity.HasOne(l => l.SourceTenant)
                      .WithMany()
                      .HasForeignKey(l => l.SourceTenantId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(l => l.TargetTenant)
                      .WithMany()
                      .HasForeignKey(l => l.TargetTenantId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Branding>()
                .HasMany(b => b.ColorPresets)
                .WithOne()
                .HasForeignKey(cp => cp.BrandingId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(e => e.Price).HasPrecision(18, 2);
                entity.Property(e => e.TenantId).HasColumnType("uniqueidentifier");
            });

            modelBuilder.Entity<FeatureModule>()
                .HasIndex(m => m.Key)
                .IsUnique();

            modelBuilder.Entity<Feature>()
                .HasIndex(f => f.Key)
                .IsUnique();

            modelBuilder.Entity<TenantFeatureModule>()
                .HasKey(tm => new { tm.TenantId, tm.ModuleId });

            modelBuilder.Entity<TenantFeature>()
                .HasKey(tf => new { tf.TenantId, tf.FeatureId });

            modelBuilder.Entity<TenantFeatureModule>()
                .HasOne(tm => tm.Tenant)
                .WithMany(t => t.FeatureModules)
                .HasForeignKey(tm => tm.TenantId);

            modelBuilder.Entity<TenantFeatureModule>()
                .HasOne(tm => tm.Module)
                .WithMany()
                .HasForeignKey(tm => tm.ModuleId);

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
        }
    }
}
