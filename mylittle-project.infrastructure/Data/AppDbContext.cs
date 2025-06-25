using Microsoft.EntityFrameworkCore;
using mylittle_project.Domain.Entities;

namespace mylittle_project.infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // ──────────────────────────────── Core ────────────────────────────────
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<AdminUser> AdminUsers { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<Branding> Brandings { get; set; }
        public DbSet<BrandingText> BrandingTexts { get; set; }
        public DbSet<BrandingMedia> BrandingMedia { get; set; }
        public DbSet<ContentSettings> ContentSettings { get; set; }
        public DbSet<DomainSettings> DomainSettings { get; set; }
        public DbSet<ActivityLogBuyer> ActivityLogs { get; set; }

        // ─────────────────────── Product & Listings ───────────────────────────
        public DbSet<Product> Products { get; set; }

        // ───────────────────────── Orders & Buyers ────────────────────────────
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Buyer> Buyers { get; set; }

        // ──────────────────────── Dealer-specific ─────────────────────────────
        public DbSet<BusinessInfo> BusinessInfos { get; set; }
        public DbSet<SubscriptionDealer> DealerSubscriptions { get; set; }
        public DbSet<AssignedCategory> AssignedCategories { get; set; }
        public DbSet<UserDealer> UserDealers { get; set; }
        public DbSet<PortalAssignment> PortalAssignments { get; set; }
        public DbSet<VirtualNumberAssignment> VirtualNumberAssignments { get; set; }

        // ───────────────────────────── Misc ───────────────────────────────────
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<TenentPortalLink> TenentPortalLinks { get; set; }
        public DbSet<KycDocumentRequest> KycDocumentRequests { get; set; }
        public DbSet<KycDocumentUpload> KycDocumentUploads { get; set; }

        // ─────────────── NEW dynamic-feature tables (master / child) ──────────
        public DbSet<FeatureModule> FeatureModules { get; set; }
        public DbSet<Feature> Features { get; set; }
        public DbSet<TenantFeatureModule> TenantFeatureModules { get; set; }
        public DbSet<TenantFeature> TenantFeatures { get; set; }

        // ─────────────────────────── ModelBuilder ─────────────────────────────
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ------------- Global soft-delete filter -------------
            modelBuilder.Entity<Buyer>()
                        .HasQueryFilter(b => !b.IsDeleted);

            // ------------- ActivityLog ↔ Buyer (many-to-one) -----
            modelBuilder.Entity<ActivityLogBuyer>()
                        .HasKey(a => a.Id);

            modelBuilder.Entity<ActivityLogBuyer>()
                        .HasOne(a => a.Buyer)
                        .WithMany(b => b.ActivityLogs)
                        .HasForeignKey(a => a.BuyerId);

            // ------------- Order ↔ Buyer (many-to-one) -----------
            modelBuilder.Entity<Order>()
                        .HasOne(o => o.Buyer)
                        .WithMany(b => b.Orders)
                        .HasForeignKey(o => o.BuyerId)
                        .OnDelete(DeleteBehavior.Cascade);

            // ------------- VirtualNumberAssignment ---------------
            modelBuilder.Entity<VirtualNumberAssignment>()
                        .HasIndex(v => v.VirtualNumber)
                        .IsUnique();

            modelBuilder.Entity<VirtualNumberAssignment>()
                        .HasIndex(v => v.BusinessId)
                        .IsUnique();

            modelBuilder.Entity<VirtualNumberAssignment>()
                        .HasOne(v => v.BusinessInfo)
                        .WithOne(b => b.VirtualNumberAssignment)
                        .HasForeignKey<VirtualNumberAssignment>(v => v.BusinessId)
                        .OnDelete(DeleteBehavior.Cascade);

            // ------------- UserDealer ↔ BusinessInfo -------------
            modelBuilder.Entity<BusinessInfo>()
                        .HasOne(b => b.UserDealer)
                        .WithMany(u => u.BusinessInfos)
                        .HasForeignKey(b => b.UserDealerId)
                        .OnDelete(DeleteBehavior.Restrict);

            // ------------- PortalAssignment ↔ UserDealer ---------
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

            // ------------- SubscriptionDealer (owned cats) -------
            modelBuilder.Entity<SubscriptionDealer>()
                        .OwnsMany(s => s.Categories);

            modelBuilder.Entity<SubscriptionDealer>()
                        .HasOne(s => s.BusinessInfo)
                        .WithMany()
                        .HasForeignKey(s => s.BusinessId)
                        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SubscriptionDealer>()
                        .HasIndex(s => new { s.BusinessId, s.TenantId })
                        .IsUnique();

            // ------------- TenentPortalLink ----------------------
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

            // ------------- Branding → ColorPresets ---------------
            modelBuilder.Entity<Branding>()
                        .HasMany(b => b.ColorPresets)
                        .WithOne()
                        .HasForeignKey(cp => cp.BrandingId)
                        .OnDelete(DeleteBehavior.Cascade);

            // ------------- Product precision ---------------------
            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(e => e.Price).HasPrecision(18, 2);
                entity.Property(e => e.TenantId).HasColumnType("uniqueidentifier");
            });

            // ───────────────────── NEW dynamic-feature config ──────────────────

            // unique keys for easy look-ups
            modelBuilder.Entity<FeatureModule>()
                        .HasIndex(m => m.Key)
                        .IsUnique();

            modelBuilder.Entity<Feature>()
                        .HasIndex(f => f.Key)
                        .IsUnique();

            // composite PKs (tenant ↔ feature toggles)
            modelBuilder.Entity<TenantFeatureModule>()
                        .HasKey(tm => new { tm.TenantId, tm.ModuleId });

            modelBuilder.Entity<TenantFeature>()
                        .HasKey(tf => new { tf.TenantId, tf.FeatureId });

            // TenantFeatureModule relationships
            modelBuilder.Entity<TenantFeatureModule>()
                        .HasOne(tm => tm.Tenant)
                        .WithMany(t => t.FeatureModules)
                        .HasForeignKey(tm => tm.TenantId);

            modelBuilder.Entity<TenantFeatureModule>()
                        .HasOne(tm => tm.Module)
                        .WithMany()                     // no reverse nav on FeatureModule
                        .HasForeignKey(tm => tm.ModuleId);

            // TenantFeature relationships
            modelBuilder.Entity<TenantFeature>()
                        .HasOne(tf => tf.Tenant)
                        .WithMany(t => t.Features)
                        .HasForeignKey(tf => tf.TenantId);

            modelBuilder.Entity<TenantFeature>()
                        .HasOne(tf => tf.Feature)
                        .WithMany()                     // no reverse nav on Feature
                        .HasForeignKey(tf => tf.FeatureId);
        }
    }
}
