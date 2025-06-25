using Microsoft.EntityFrameworkCore;
using mylittle_project.Application.DTOs;
using mylittle_project.Application.Interfaces;
using mylittle_project.Domain.Entities;
using mylittle_project.infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mylittle_project.infrastructure.Services
{
    public class TenantService : ITenantService
    {
        private readonly AppDbContext _context;

        public TenantService(AppDbContext context)
        {
            _context = context;
        }

        // ──────────────────────────────────────────────────────────────
        // 1)  GET ENTIRE FEATURE TREE  (for the “Features” UI tab)
        // ──────────────────────────────────────────────────────────────
        public async Task<List<FeatureModuleDto>> GetFeatureTreeAsync(Guid tenantId)
        {
            var modules = await _context.FeatureModules
                                        .Include(m => m.Features)
                                        .AsNoTracking()
                                        .ToListAsync();

            var tenantModules = await _context.TenantFeatureModules
                                               .Where(tm => tm.TenantId == tenantId)
                                               .ToDictionaryAsync(tm => tm.ModuleId);

            var tenantFeatures = await _context.TenantFeatures
                                               .Where(tf => tf.TenantId == tenantId)
                                               .ToDictionaryAsync(tf => tf.FeatureId);

            return modules.Select(m => new FeatureModuleDto
            {
                ModuleId = m.Id,
                Name = m.Name,
                IsEnabled = tenantModules.TryGetValue(m.Id, out var tm) && tm.IsEnabled,
                Features = m.Features.Select(f => new FeatureToggleDto
                {
                    FeatureId = f.Id,
                    Name = f.Name,
                    IsEnabled = tenantFeatures.TryGetValue(f.Id, out var tf) && tf.IsEnabled
                }).ToList()
            }).ToList();
        }

        // ──────────────────────────────────────────────────────────────
        // 2)  TOGGLE AN ENTIRE MODULE (cascades to children)
        // ──────────────────────────────────────────────────────────────
        public async Task<bool> UpdateModuleToggleAsync(Guid tenantId, Guid moduleId, bool isEnabled)
        {
            var tModule = await _context.TenantFeatureModules
                                        .Include(tm => tm.TenantFeatures)
                                        .FirstOrDefaultAsync(tm =>
                                            tm.TenantId == tenantId && tm.ModuleId == moduleId);

            if (tModule == null) return false;

            tModule.IsEnabled = isEnabled;

            // cascade: children can only be ON if parent is ON
            foreach (var child in tModule.TenantFeatures)
                child.IsEnabled = isEnabled && child.IsEnabled;

            await _context.SaveChangesAsync();
            return true;
        }

        // ──────────────────────────────────────────────────────────────
        // 3)  TOGGLE A SINGLE CHILD FEATURE
        // ──────────────────────────────────────────────────────────────
        public async Task<bool> UpdateFeatureToggleAsync(Guid tenantId, Guid featureId, bool isEnabled)
        {
            var tFeature = await _context.TenantFeatures
                                         .FirstOrDefaultAsync(tf =>
                                             tf.TenantId == tenantId && tf.FeatureId == featureId);

            if (tFeature == null) return false;

            var parent = await _context.TenantFeatureModules
                                       .FirstAsync(tm =>
                                           tm.TenantId == tenantId && tm.ModuleId == tFeature.ModuleId);

            if (!parent.IsEnabled && isEnabled)
                return false;                        // cannot turn ON while parent OFF

            tFeature.IsEnabled = isEnabled;
            await _context.SaveChangesAsync();
            return true;
        }

        // ──────────────────────────────────────────────────────────────
        // 4)  GLOBAL TENANT LIST  (unchanged except for removed Include)
        // ──────────────────────────────────────────────────────────────
        public async Task<IEnumerable<Tenant>> GetAllAsync()
        {
            return await _context.Tenants
                .Include(t => t.AdminUser)
                .Include(t => t.Subscription)
                .Include(t => t.Store)
                .Include(t => t.Branding)
                .Include(t => t.ContentSettings)
                .Include(t => t.DomainSettings)
                .Include(t => t.FeatureModules) // eager-load toggles if needed
                .Include(t => t.Features)
                .ToListAsync();
        }

        // ──────────────────────────────────────────────────────────────
        // 5)  CREATE TENANT  (FeatureSettings removed → seed toggles)
        // ──────────────────────────────────────────────────────────────
        public async Task<Tenant> CreateAsync(TenantDto dto)
        {
            var tenantId = Guid.NewGuid();

            var tenant = new Tenant
            {
                Id = tenantId,
                Name = dto.TenantName,
                TenantName = dto.TenantName,
                TenantNickname = dto.TenantNickname,
                Subdomain = dto.Subdomain,
                IndustryType = dto.IndustryType,
                Status = dto.Status,
                Description = dto.Description,
                IsActive = dto.IsActive,
                LastAccessed = DateTime.UtcNow,

                // ------------ AdminUser -----------------
                AdminUser = new AdminUser
                {
                    FullName = dto.AdminUser.FullName,
                    Email = dto.AdminUser.Email,
                    Password = dto.AdminUser.Password,
                    Role = dto.AdminUser.Role,
                    PhoneNumber = dto.AdminUser.PhoneNumber,
                    CountryCode = dto.AdminUser.CountryCode,
                    DateOfBirth = dto.AdminUser.DateOfBirth,
                    Gender = dto.AdminUser.Gender,
                    StreetAddress = dto.AdminUser.StreetAddress,
                    City = dto.AdminUser.City,
                    StateProvince = dto.AdminUser.StateProvince,
                    ZipPostalCode = dto.AdminUser.ZipPostalCode,
                    Country = dto.AdminUser.Country
                },

                // ------------ Subscription ---------------
                Subscription = new Subscription
                {
                    PlanName = dto.Subscription.PlanName,
                    StartDate = dto.Subscription.StartDate,
                    EndDate = dto.Subscription.EndDate,
                    IsTrial = dto.Subscription.IsTrial,
                    IsActive = dto.Subscription.IsActive,
                    TenantId = tenantId
                },

                // ------------ Store -----------------------
                Store = new Store
                {
                    Currency = dto.Store.Currency,
                    Language = dto.Store.Language,
                    Timezone = dto.Store.Timezone,
                    UnitSystem = dto.Store.UnitSystem,
                    TextDirection = dto.Store.TextDirection,
                    NumberFormat = dto.Store.NumberFormat,
                    DateFormat = dto.Store.DateFormat,
                    EnableTaxCalculations = dto.Store.EnableTaxCalculations,
                    EnableShipping = dto.Store.EnableShipping,
                    EnableFilters = dto.Store.EnableFilters,
                    TenantId = tenantId
                },

                // ------------ Branding --------------------
                Branding = new Branding
                {
                    PrimaryColor = dto.Branding.PrimaryColor,
                    AccentColor = dto.Branding.AccentColor,
                    BackgroundColor = dto.Branding.BackgroundColor,
                    SecondaryColor = dto.Branding.SecondaryColor,
                    TextColor = dto.Branding.TextColor,
                    Text = new BrandingText
                    {
                        FontName = dto.Branding.TextSettings.FontName,
                        FontSize = dto.Branding.TextSettings.FontSize,
                        FontWeight = dto.Branding.TextSettings.FontWeight
                    },
                    Media = new BrandingMedia
                    {
                        LogoUrl = dto.Branding.MediaSettings.LogoUrl,
                        FaviconUrl = dto.Branding.MediaSettings.FaviconUrl,
                        BackgroundImageUrl = dto.Branding.MediaSettings.BackgroundImageUrl
                    },
                    ColorPresets = dto.Branding.ColorPresets?.Select(p => new ColorPreset
                    {
                        Name = p.Name,
                        PrimaryColor = p.PrimaryColor,
                        SecondaryColor = p.SecondaryColor,
                        AccentColor = p.AccentColor
                    }).ToList()
                },

                // ------------ Content ---------------------
                ContentSettings = new ContentSettings
                {
                    WelcomeMessage = dto.ContentSettings.WelcomeMessage,
                    CallToAction = dto.ContentSettings.CallToAction,
                    HomePageContent = dto.ContentSettings.HomePageContent,
                    AboutUs = dto.ContentSettings.AboutUs,
                    ContactUs = dto.ContentSettings.ContactUs,
                    TermsAndPrivacyPolicy = dto.ContentSettings.TermsAndPrivacyPolicy
                },

                // ------------ Domain ----------------------
                DomainSettings = new DomainSettings
                {
                    Subdomain = dto.DomainSettings.Subdomain,
                    MainDomain = dto.DomainSettings.MainDomain,
                    CustomDomain = dto.DomainSettings.CustomDomain,
                    EnableApiAccess = dto.DomainSettings.EnableApiAccess
                }
            };

            _context.Tenants.Add(tenant);

            // seed OFF toggles for every module/feature
            await SeedTenantFeaturesAsync(tenantId);

            await _context.SaveChangesAsync();
            return tenant;
        }

        // helper used only by CreateAsync
        private async Task SeedTenantFeaturesAsync(Guid tenantId)
        {
            var modules = await _context.FeatureModules
                                        .Include(m => m.Features)
                                        .ToListAsync();

            foreach (var module in modules)
            {
                _context.TenantFeatureModules.Add(new TenantFeatureModule
                {
                    TenantId = tenantId,
                    ModuleId = module.Id,
                    IsEnabled = false
                });

                foreach (var feature in module.Features)
                {
                    _context.TenantFeatures.Add(new TenantFeature
                    {
                        TenantId = tenantId,
                        FeatureId = feature.Id,
                        ModuleId = module.Id,
                        IsEnabled = false
                    });
                }
            }
        }

        // ──────────────────────────────────────────────────────────────
        // 6)  LOAD TENANT WITH FULL TOGGLES (handy for admin pages)
        // ──────────────────────────────────────────────────────────────
        public async Task<Tenant?> GetTenantWithFeaturesAsync(Guid tenantId)
        {
            return await _context.Tenants
                                 .Include(t => t.FeatureModules)
                                 .ThenInclude(tm => tm.Module)
                                 .Include(t => t.Features)
                                 .ThenInclude(tf => tf.Feature)
                                 .FirstOrDefaultAsync(t => t.Id == tenantId);
        }

        // ──────────────────────────────────────────────────────────────
        // 7)  UPDATE STORE SETTINGS  (unchanged)
        // ──────────────────────────────────────────────────────────────
        public async Task<bool> UpdateStoreAsync(Guid tenantId, StoreDto dto)
        {
            var tenant = await _context.Tenants
                                       .Include(t => t.Store)
                                       .FirstOrDefaultAsync(t => t.Id == tenantId);

            if (tenant?.Store == null)
            {
                tenant!.Store = new Store { TenantId = tenantId };
                _context.Stores.Add(tenant.Store);
            }

            tenant.Store.Country = dto.Country;
            tenant.Store.Currency = dto.Currency;
            tenant.Store.Language = dto.Language;
            tenant.Store.Timezone = dto.Timezone;
            tenant.Store.UnitSystem = dto.UnitSystem;
            tenant.Store.TextDirection = dto.TextDirection;
            tenant.Store.NumberFormat = dto.NumberFormat;
            tenant.Store.DateFormat = dto.DateFormat;
            tenant.Store.EnableTaxCalculations = dto.EnableTaxCalculations;
            tenant.Store.EnableShipping = dto.EnableShipping;
            tenant.Store.EnableFilters = dto.EnableFilters;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
