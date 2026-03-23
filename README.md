# AgInventory — Inventory & Parts Tracker

A full-stack inventory management system built for an agricultural equipment shop. Built to demonstrate proficiency in the Heartland Ag Tech technology stack.

## Tech Stack

- **Backend:** ASP.NET Core Web API (.NET 10), C#
- **Database:** SQL Server 2025 Express, T-SQL, Stored Procedures
- **Frontend:** HTML5, CSS3, JavaScript, jQuery, Bootstrap
- **Reporting:** Microsoft SSRS (Report Builder)
- **Tools:** Visual Studio 2026, SSMS 22, Git

## Features

- **Dashboard** — KPI cards showing total parts, low stock count, total stock value and recent audit activity
- **Parts Catalog** — searchable and filterable parts table with real-time stock status
- **Stock Management** — check out parts against work orders, automatic stock level updates
- **Purchase Orders** — create POs with line items, receive deliveries with one click
- **Audit Log** — immutable record of every stock change with user, timestamp and reason
- **SSRS Report** — formatted inventory stock level report grouped by category

## Database Schema

10 tables: `Parts`, `Categories`, `Suppliers`, `Locations`, `Stock`, `Users`, `AuditLog`, `Checkouts`, `PurchaseOrders`, `PurchaseOrderItems`

3 stored procedures:
- `usp_GetLowStockParts` — returns all parts below reorder threshold
- `usp_AdjustStock` — updates stock and writes audit log atomically
- `usp_ReceivePO` — loops through PO line items and restocks each part

## Project Structure
```
AgInventory/
├── Database/
│   └── database.sql          # Full schema, seed data, stored procedures
├── AgInventory.API/          # ASP.NET Core Web API
│   └── Controllers/          # 6 API controllers
├── Frontend/                 # HTML/CSS/JS web pages
│   ├── dashboard.html
│   ├── parts.html
│   ├── checkout.html
│   ├── purchaseorders.html
│   └── auditlog.html
└── Reports/
    └── InventoryStockReport.rdl  # SSRS stock level report
```

## Setup Instructions

1. Run `Database/database.sql` against a SQL Server Express instance named `SQLEXPRESS`
2. Open `AgInventory.API` in Visual Studio and press F5
3. Open `Frontend/dashboard.html` in a browser