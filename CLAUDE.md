# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

AgInventory is a parts/inventory tracker for an agricultural equipment shop: an ASP.NET Core 10 Web API over SQL Server (Dapper, no EF), a static jQuery/Bootstrap frontend, and an SSRS report. There is no test project, no package manager for the frontend, and no build step for `Frontend/` — it is plain files served as-is.

## Commands

```bash
# API (from AgInventory.API/AgInventory.API/)
dotnet build
dotnet run                      # https://localhost:7015 + http://localhost:5073, Swagger UI at /swagger
dotnet run --launch-profile http # http only
```

Database: run `Database/database.sql` once against a SQL Server instance. It is a full drop-and-create script (schema + seed data + stored procs), so re-running it resets all data. The default connection string in `appsettings.json` targets `localhost\SQLEXPRESS`.

Frontend: open `Frontend/dashboard.html` directly, or serve the folder (`python3 -m http.server`). No build, no bundler.

## Architecture

**Data access is SQL-first.** Controllers hold a connection string injected from `IConfiguration` and call Dapper directly — there is no repository, service, or DbContext layer. Multi-step writes live in stored procedures rather than C#:

- `usp_AdjustStock` — the single write path for stock. It updates `Stock.QtyOnHand` *and* inserts the `AuditLog` row together; never `UPDATE Stock` directly, or the audit trail silently breaks.
- `usp_ReceivePO` — cursors over `PurchaseOrderItems` and calls `usp_AdjustStock` per line (hardcodes `LocationId = 1`), then flips the PO to `RECEIVED`.
- `usp_GetLowStockParts` — the `QtyOnHand <= ReorderPoint` definition of "low stock". Note `PartsController.GetAllParts` re-expresses the same rule as an inline `CASE` producing `StockStatus`; if the threshold rule changes, both need updating.

Model classes live in the project root (`Part.cs`, `StockItem.cs`, `AdjustStockRequest.cs`) under namespace `AgInventory.API.Models`, but `PurchaseOrdersController.cs` and the audit/category/supplier controllers declare their own DTOs inline in the controller file. Dapper maps by column alias, so the `SELECT ... AS` aliases must match property names exactly.

`StockController` and `PartsController` overlap deliberately: `/api/stock/lowstock` and `/api/parts/lowstock` both call `usp_GetLowStockParts`, and `/api/stock/adjust` and `/api/parts/adjuststock` both call `usp_AdjustStock`. The frontend uses the `/stock` variants.

**Frontend.** Every page is a standalone HTML file that includes `js/api.js`, which defines `API_BASE` plus the `API.get`/`API.post` jQuery wrappers and shared helpers (`showAlert`, `formatDate`, `stockBadge`). Page logic is inline `<script>` in each HTML file. There is no auth — `userId: 1` is hardcoded at the call sites for checkout and PO receipt.

**Deployment.** `.github/workflows/azure-static-web-apps-*.yml` deploys `/Frontend` to Azure Static Web Apps on push to `main`; `index.html` is a meta-refresh to `dashboard.html`. The API is deployed separately to Azure App Service, and `API_BASE` in `Frontend/js/api.js` is hardcoded to that URL — switching to a local API means editing that constant (the localhost URL is kept commented above it). CORS is wide open (`AllowAll`).
