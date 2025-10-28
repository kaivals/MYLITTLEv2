# 🏪 MultiTenant Trader Portal – Dealer & Buyer Management System

### 📅 Duration: May 2025 – Jul 2025  
A **SaaS-based platform** designed to manage multiple equipment **dealers and buyers** under a unified system, following a **multi-tenant architecture** with clean and scalable backend design.

---

## 🚀 Purpose

The **MultiTenant Trader Portal** provides a centralized solution for managing multiple dealers, buyers, and their stores.  
Each dealer operates in an **isolated tenant environment**, while a **Super Admin** manages the overall platform, including subscriptions, products, and orders.

---

## 🧩 Key Features

### 👤 Multi-Tenant Structure
- Each **dealer (tenant)** has **isolated data and schema**.
- Every tenant manages their own store, products, and orders.
- **Super Admin** has complete control across all tenants.

### 🔐 Role-Based Access Control
- **Super Admin:** Manage dealers, subscriptions, and system-wide settings.  
- **Store Manager:** Manage products, inventory, and orders within their store.

### 🛒 Core Functionalities
- Product management with **categories, tags, and inventory tracking**.  
- Order lifecycle tracking (Created → Shipped → Delivered → Completed).  
- **Category-aware dynamic filters** (e.g., Brand, Size) for smart product search.  
- Subscription plans with **auto-renewal** and **resource limits** (e.g., product count).  
- **Dealer onboarding** with KYC verification and approval workflow.  
- **Seasonal & promotional discounts** for flexible pricing control.

### 🛡️ Security & Compliance
- **JWT-based authentication & authorization.**  
- **Role-based checks** for secure endpoint access.  
- **Audit logging** to track all major actions.  
- **Data isolation per tenant** (schema-per-tenant model).

---

## 🏗️ Architecture Overview

| Layer | Description |
|-------|-------------|
| **Presentation Layer** | ASP.NET Core Web API controllers for RESTful communication. |
| **Application Layer** | Business logic, DTOs, and validation rules. |
| **Infrastructure Layer** | SQL Server, Repository Pattern, Unit of Work implementation. |
| **Domain Layer** | Entity models and core domain logic. |

### 🧱 Tech Stack

| Category | Technology |
|-----------|-------------|
| **Backend** | ASP.NET Core Web API (.NET 9) |
| **Database** | SQL Server (Schema-per-tenant design) |
| **Architecture** | Clean Architecture (Repository + Unit of Work) |
| **Authentication** | JWT Tokens |
| **ORM** | Entity Framework Core |
| **Logging** | Serilog / Audit Logs |
| **Version Control** | Git & GitHub |

---

## 📦 Project Modules

1. **Super Admin Dashboard**
   - Manage dealers, subscriptions, and reports.
2. **Dealer Management**
   - Create, update, and suspend dealer accounts.
3. **Product & Inventory Management**
   - CRUD operations, stock management, tagging, and categories.
4. **Order Management**
   - Track order status with full lifecycle transitions.
5. **Subscription Module**
   - Plan creation, renewal logic, and resource restriction.
6. **Authentication & Authorization**
   - JWT + Role-based access control.
7. **Reports & Analytics**
   - Insights on sales, dealer performance, and active subscriptions.

---

## 🧪 Sample Folder Structure

