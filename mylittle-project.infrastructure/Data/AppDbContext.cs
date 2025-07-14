using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using mylittle_project.Application.Interfaces;
using mylittle_project.Domain.Entities;
using System.Text.Json;
namespace mylittle_project.infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        private readonly ICurrentUserService _currentUser;

        public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService currentUser)
        : base(options)
        {
            _currentUser = currentUser;
        }


        public DbSet<UserCredential> UserCredentials { get; set; }


        //Tenant related entities
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<AdminUser> AdminUsers { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<BrandingText> BrandingTexts { get; set; }
        public DbSet<BrandingMedia> BrandingMedia { get; set; }
        public DbSet<ContentSettings> ContentSettings { get; set; }
        public DbSet<DomainSettings> DomainSettings { get; set; }
        public DbSet<ActivityLogBuyer> ActivityLogs { get; set; }
        public DbSet<ColorPreset> ColorPresets { get; set; }

        //Order related entities
        
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Buyer> Buyers { get; set; }

        // Dealer related entities
        public DbSet<Dealer> Dealers { get; set; }
        public DbSet<UserDealer> UserDealers { get; set; }
        public DbSet<PortalAssignment> PortalAssignments { get; set; }
        public DbSet<VirtualNumberAssignment> VirtualNumberAssignments { get; set; }
        public DbSet<KycDocumentRequest> KycDocumentRequests { get; set; }
        public DbSet<KycDocumentUpload> KycDocumentUploads { get; set; }
        public DbSet<TenantPortalLink> TenentPortalLinks { get; set; }

        // Feature related entities
        public DbSet<FeatureModule> FeatureModules { get; set; }
        public DbSet<Feature> Features { get; set; }
        public DbSet<TenantFeatureModule> TenantFeatureModules { get; set; }
        public DbSet<TenantFeature> TenantFeatures { get; set; }

        //Subscription related entities
        public DbSet<GlobalSubscription> GlobalSubscriptions { get; set; }
        public DbSet<TenantSubscription> TenantSubscriptions { get; set; }
        public DbSet<DealerPlanAssignment> TenantPlanAssignments { get; set; }
        public DbSet<DealerSubscriptionApplication> DealerSubscriptionApplications { get; set; }
        public DbSet<Filter> Filters { get; set; }


        //Product related entities
        public DbSet<Category> Categories { get; set; }
        public DbSet<BrandProduct> Brands { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductSection> ProductSections { get; set; }
        public DbSet<ProductField> ProductFields { get; set; }
        public DbSet<Branding> ProductBrandings { get; set; } 
        public DbSet<ProductReview> ProductReviews { get; set; }
        public DbSet<ProductTag> ProductTags { get; set; }
        public DbSet<ProductAttribute> ProductAttributes { get; set; }
        public DbSet<ProductFieldValue> ProductFieldValues { get; set; }


        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var userId = _currentUser.UserId ?? Guid.Empty;

            foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = now;
                        entry.Entity.CreatedBy = userId;
                        entry.Entity.IsDeleted = false;
                        break;

                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = now;
                        entry.Entity.UpdatedBy = userId;
                        break;

                    case EntityState.Deleted:
                        // Soft delete instead of hard delete
                        entry.State = EntityState.Modified;
                        entry.Entity.IsDeleted = true;
                        entry.Entity.DeletedAt = now;
                        entry.Entity.UpdatedBy = userId;
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

            // Soft Delete Filters
            modelBuilder.Entity<Buyer>().HasQueryFilter(b => !b.IsDeleted);
            modelBuilder.Entity<BrandProduct>().HasQueryFilter(b => !b.IsDeleted);
            modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<Category>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<Order>().HasQueryFilter(o => !o.IsDeleted);
            modelBuilder.Entity<ActivityLogBuyer>().HasQueryFilter(a => !a.IsDeleted);
            modelBuilder.Entity<ProductReview>().HasQueryFilter(r => !r.IsDeleted);
            modelBuilder.Entity<Filter>().HasQueryFilter(f => !f.IsDeleted);
            modelBuilder.Entity<PortalAssignment>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<UserDealer>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<VirtualNumberAssignment>().HasQueryFilter(v => !v.IsDeleted);
            modelBuilder.Entity<DealerPlanAssignment>().HasQueryFilter(d => !d.IsDeleted);
            modelBuilder.Entity<DealerSubscriptionApplication>().HasQueryFilter(d => !d.IsDeleted);
            modelBuilder.Entity<TenantPortalLink>().HasQueryFilter(t => !t.IsDeleted);
            modelBuilder.Entity<TenantFeatureModule>().HasQueryFilter(t => !t.IsDeleted);
            modelBuilder.Entity<TenantFeature>().HasQueryFilter(t => !t.IsDeleted);
            modelBuilder.Entity<TenantSubscription>().HasQueryFilter(t => !t.IsDeleted);
            modelBuilder.Entity<Feature>().HasQueryFilter(f => !f.IsDeleted);
            modelBuilder.Entity<FeatureModule>().HasQueryFilter(m => !m.IsDeleted);
            modelBuilder.Entity<Branding>().HasQueryFilter(b => !b.IsDeleted);
            modelBuilder.Entity<ProductFieldValue>().HasQueryFilter(f => !f.IsDeleted);
            modelBuilder.Entity<ProductField>().HasQueryFilter(f => !f.IsDeleted);


            modelBuilder.Entity<UserCredential>()
                .HasOne(uc => uc.User)
                .WithMany() // You can create a navigation collection in ApplicationUser if needed
                .HasForeignKey(uc => uc.UserId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<Filter>()
                .Property(f => f.Values)
                .HasConversion(listToStringConverter);

            modelBuilder.Entity<Category>()
                .HasMany(c => c.Filters)
                .WithMany(f => f.Categories);

            modelBuilder.Entity<ActivityLogBuyer>()
                .HasOne(a => a.Buyer)
                .WithMany(b => b.ActivityLogs)
                .HasForeignKey(a => a.BuyerId)
                .OnDelete(DeleteBehavior.Cascade);
            // In DbContext OnModelCreating
            modelBuilder.Entity<ProductFieldValue>()
                .HasIndex(p => new { p.ProductId, p.FieldId })
                .IsUnique();

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

            // Product - Category Many-to-Many
            modelBuilder.Entity<Product>()
                .HasMany(p => p.Categories)
                .WithMany(c => c.Products)
                .UsingEntity(j => j.ToTable("ProductCategories"));

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Brand)
                .WithMany()
                .HasForeignKey(p => p.BrandId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductFieldValue>()
                .HasIndex(p => new { p.ProductId, p.FieldId, p.IsDeleted })
                .IsUnique();


            modelBuilder.Entity<ProductFieldValue>()
                .HasOne(pfv => pfv.Field)
                .WithMany()
                .HasForeignKey(pfv => pfv.FieldId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>()
                .HasMany(p => p.Tags)
                .WithMany(t => t.Products)
                .UsingEntity(j => j.ToTable("ProductProductTags"));

            base.OnModelCreating(modelBuilder);
        }
    }
}
