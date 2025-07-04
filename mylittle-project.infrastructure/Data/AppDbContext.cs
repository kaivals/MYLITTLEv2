using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using mylittle_project.Domain.Entities;
using System.Collections.Generic;
using System.Text.Json;

namespace mylittle_project.infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Core Tables
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<AdminUser> AdminUsers { get; set; }
        public DbSet<Store> Stores { get; set; }

        public DbSet<Branding> Brandings { get; set; }
        public DbSet<BrandingText> BrandingTexts { get; set; }
        public DbSet<BrandingMedia> BrandingMedia { get; set; }
        public DbSet<ContentSettings> ContentSettings { get; set; }
        public DbSet<DomainSettings> DomainSettings { get; set; }

        // Logging
        public DbSet<ActivityLogBuyer> ActivityLogs { get; set; }

        // Colors & Design
        public DbSet<ColorPreset> ColorPresets { get; set; }

        // Products
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductSection> ProductSections { get; set; }
        public DbSet<ProductField> ProductFields { get; set; }

        // Orders
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        // Buyers / Dealers
        public DbSet<Buyer> Buyers { get; set; }
        public DbSet<Dealer> Dealers { get; set; }
        public DbSet<UserDealer> UserDealers { get; set; }

        // Dealer Assignments
        public DbSet<PortalAssignment> PortalAssignments { get; set; }
        public DbSet<VirtualNumberAssignment> VirtualNumberAssignments { get; set; }

        // KYC
        public DbSet<KycDocumentRequest> KycDocumentRequests { get; set; }
        public DbSet<KycDocumentUpload> KycDocumentUploads { get; set; }

        // Tenant Linking
        public DbSet<TenentPortalLink> TenentPortalLinks { get; set; }

        // Feature Modules
        public DbSet<FeatureModule> FeatureModules { get; set; }
        public DbSet<Feature> Features { get; set; }
        public DbSet<TenantFeatureModule> TenantFeatureModules { get; set; }
        public DbSet<TenantFeature> TenantFeatures { get; set; }

        // Categories / Subscriptions
        public DbSet<Category> Categories { get; set; }
        public DbSet<GlobalSubscription> GlobalSubscriptions { get; set; }
        public DbSet<TenantSubscription> TenantSubscriptions { get; set; }
        public DbSet<TenantPlanAssignment> TenantPlanAssignments { get; set; }

        public DbSet<DealerSubscription> DealerSubscriptions { get; set; }

        // Filters
        public DbSet<Filter> Filters { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var listToStringConverter = new ValueConverter<List<string>, string>(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null) ?? new List<string>()
            );

            // Filters JSON Conversion
            modelBuilder.Entity<Filter>()
                .Property(f => f.Values)
                .HasConversion(listToStringConverter);


            // Category - Filter many-to-many
            modelBuilder.Entity<Category>()
                .HasMany(c => c.Filters)
                .WithMany(f => f.Categories);

            modelBuilder.Entity<Buyer>().HasQueryFilter(b => !b.IsDeleted);
            modelBuilder.Entity<ActivityLogBuyer>()
                .HasOne(a => a.Buyer)
                .WithMany(b => b.ActivityLogs)
                .HasForeignKey(a => a.BuyerId)
                .OnDelete(DeleteBehavior.Cascade); // this can stay

            modelBuilder.Entity<ActivityLogBuyer>()
                .HasOne<Tenant>() // or .WithMany() if you have nav prop
                .WithMany()
                .HasForeignKey(a => a.TenantId)
                .OnDelete(DeleteBehavior.NoAction); // ❌ Prevent multiple cascade paths


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

            modelBuilder.Entity<Dealer>()
                .HasOne(b => b.UserDealer)
                .WithMany(u => u.Dealers)
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

   
            modelBuilder.Entity<FeatureModule>().HasIndex(m => m.Key).IsUnique();
            modelBuilder.Entity<Feature>().HasIndex(f => f.Key).IsUnique();

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
            });

            modelBuilder.Entity<DealerSubscription>()
                .HasOne(ds => ds.Tenant)
                .WithMany()
                .HasForeignKey(ds => ds.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<DealerSubscription>()
                .HasOne(ds => ds.Dealer)
                .WithMany()
                .HasForeignKey(ds => ds.DealerId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<DealerSubscription>()
                .HasOne(ds => ds.Category)
                .WithMany()
                .HasForeignKey(ds => ds.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
