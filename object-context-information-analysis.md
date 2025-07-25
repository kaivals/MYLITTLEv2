# Assignment 1 – Object, Context & Information Analysis

## 📌 Project Title: SaaS-Based Multi-Tenant Portal

This project is a SaaS (Software as a Service) based **multi-tenant portal** designed to serve multiple client businesses (tenants) under a single codebase. Each tenant can manage its own users, dealers, buyers, products and subscriptions, with dynamic field configuration and feature modules for customization.

---

## 🧱 OBJECTS

| Object               | Description                                                                 |
|----------------------|-----------------------------------------------------------------------------|
| `Tenant`             | Represents a customer organization using the platform                       |
| `Admin User`         | Manages tenant configuration, users and subscriptions                       |
| `Dealer`             | Business partner under a tenant who manages product listings and orders     |
| `Buyer`              | End-user who browses and purchases products                                 |
| `Subscription`       | Tracks plan, billing, validity and feature limits per tenant                |
| `Portal`             | Tenant-specific frontend domain (e.g., `tenant.example.com`)                |
| `Product`            | Configurable item offered by a tenant                                       |
| `Product Section`    | Logical grouping of product fields (e.g., “Product Info”, “Shipping”)       |
| `Product Field`      | Custom field under a section (e.g., SKU, Price, Weight)                     |
| `Product Field Value`| Actual data filled in each product field, stored as JSON                    |
| `Feature Module`     | Toggleable functionality (e.g., Catalog, Orders, Reports)                   |
| `Order`              | Record of a purchase made by a buyer                                        |
| `KYC Document`       | Verification documents for dealer onboarding                                |
| `Notification`       | Alerts sent to users (e.g., new order, product update)                      |
| `Role`               | Defines permission levels (SuperAdmin, TenantAdmin, Dealer, Buyer)          |
| `Activity Log`       | Audit trail of system actions (e.g., “Product Created”, “Login Attempt”)    |

---

## 🌐 CONTEXTS

| Context                  | Objects Involved                                     | Description                                                                      |
|--------------------------|------------------------------------------------------|----------------------------------------------------------------------------------|
| Tenant Management        | Tenant, Admin User, Subscription                     | Onboard new tenants, assign plans, manage admin users and feature access         |
| Product Configuration    | Product, Product Section, Product Field              | Define dynamic product schema: sections & fields, visibility and validation      |
| Dealer Management        | Dealer, KYC Document, Portal                         | Onboard dealers, collect KYC, assign portal access                               |
| Buyer Interaction        | Buyer, Product, Portal, Order                        | Buyers browse catalog, add to cart, place orders                                 |
| Feature Management       | Feature Module, Subscription, Tenant                 | Enable/disable modules per tenant based on subscription                          |
| Field Visibility Logic   | Product Field, Dealer                                | Show/hide fields in dealer UI; enforce `IsRequired` where applicable             |
| Subscription Control     | Subscription, Tenant                                 | Enforce feature limits, plan renewal, grace periods                              |
| Order Processing         | Order, Buyer, Dealer                                 | Manage order lifecycle: Placed → Processing → Shipped → Delivered                |
| Notification System      | Notification, Admin User, Dealer                     | Generate and deliver alerts (email/in-app) for key events                        |
| Role & Permission        | Role, Admin User, Dealer, Buyer                      | RBAC: assign roles and permissions per module                                    |
| KYC Verification         | KYC Document, Dealer                                 | Upload, review, approve or reject dealer verification documents                  |
| Logging & Monitoring     | Activity Log, Admin User, Dealer                     | Record user actions and system events for audit and debugging                    |
| Portal Branding          | Portal, Tenant                                       | Customize portal look (logo, color theme, custom domain, welcome message)        |
| Autosync Field Logic     | Product Field, Product Field Value, Feature Module   | Auto-sync field values to filters and search indices when enabled                |
| Analytics & Reporting    | Order, Product, Dealer, Buyer, Activity Log          | Generate KPI reports: sales trends, top products, user engagement                |

---

## 🧠 INFORMATION PER CONTEXT

### 1. Tenant Management
- **Tenant**: ID, Name, Contact Info, Status (Active/Suspended)  
- **Subscription**: Plan Name, StartDate, EndDate, Limits (max Dealers, Products, Orders)  
- **Admin User**: Username, Email, Role Permissions  

### 2. Product Configuration
- **Section Metadata**: Title, DisplayOrder, Visibility  
- **Field Metadata**: Name, Type (text, number, dropdown, date),  
  `IsVisible`, `IsRequired`, `AutoSyncToFilters`  
- **Field Values**: Stored as JSON per product, e.g.:  
  ```json
  {
    "Product Name": "Resin Clock",
    "Price": 750,
    "Weight": "1.2kg",
    "Material": "Wood"
  }
3. Dealer Management
Dealer Profile: Name, Email, Phone, Status, Assigned Portal URL

KYC Records: DocumentType, UploadDate, ApprovalStatus

Access Rights: Visible Fields list, Permitted Operations (Create/Edit/Delete)

4. Buyer Interaction
Buyer Profile: Name, Email, Registration Date

Browsing Data: Viewed Products, Search Filters Used

Order History: Order IDs, Dates, Status, Total Amount

Wishlist/Cart: Saved items, Quantities, Timestamps

5. Feature Management
Feature Module: ModuleName, Description, DefaultState

FeatureAccess: TenantID, ModuleID, IsEnabled

6. Subscription Control
Plan Details: PlanID, Name, Price, DurationDays

Usage Metrics: DealersCount, ProductsCount, OrdersCount

RenewalRules: AutoRenew (true/false), GracePeriodDays

7. Order Processing
Order Entity: OrderID, TenantID, DealerID, BuyerID

OrderItems: ProductID, Quantity, UnitPrice

PaymentInfo: PaymentMethod, TransactionID, Status

8. Notification System
Notification: NotificationID, Type, RecipientID, Message, IsRead

Channels: Email, SMS, In-App

9. Role & Permission
Role: RoleID, Name, Description

Permission: ModuleName, CRUDFlags (Create/Read/Update/Delete)

UserRoleMapping: UserID, RoleID

10. KYC Verification
Document: DocID, DealerID, Type, FileURL, Status, ReviewedBy, ReviewedAt

11. Logging & Monitoring
ActivityLog: LogID, UserID, Action, Entity, EntityID, Timestamp, Details

12. Portal Branding
BrandSettings: LogoURL, ThemeColor, FaviconURL, CustomDomain, WelcomeText

13. Autosync Field Logic
SyncConfig: FieldID, IsAutoSyncEnabled, LastSyncedAt

FilterIndex: FieldValue → ProductID lookup table

14. Analytics & Reporting
ReportConfig: ReportID, Name, QueryDefinition, Schedule

