# 🥬 HarvestHub

HarvestHub is a **grocery delivery platform** built as a modern microservices stack with an Angular SPA frontend. It’s designed for local development with containers and a clean service boundary architecture.

## ✨ Highlights
- 🧩 **Microservices** with .NET 8, MediatR, Carter, FluentValidation
- 🌐 **API Gateway** using YARP (single entrypoint for the SPA)
- 🔐 **Cookie-based JWT auth** via Identity service
- 🛒 Core flows: **catalog → cart → checkout → orders**
- ⚡ **Angular** standalone components + lazy loaded feature routes
- 🐇 **RabbitMQ** with MassTransit for messaging

## 🧱 Architecture Overview
- **Frontend**: Angular SPA under [web](web) with standalone components and lazy-loaded routes
- **Gateway**: YARP API Gateway under [src/ApiGateways/YarpApiGateway](src/ApiGateways/YarpApiGateway)
- **Services** (under [src/Services](src/Services)):
	- **Catalog**: product listing and details (Carter + Marten/Postgres)
	- **Basket**: cart management + caching decorator (Carter + Marten/Postgres)
	- **Discount**: gRPC + SQLite EF Core
	- **OrderStream**: orders + EF Core SQL Server + domain events
- **Identity**: JWT issuance + cookie auth in [src/Identity/Identity.Api](src/Identity/Identity.Api)

## 🧭 Frontend Map
Key feature areas live in [web/src/app/features](web/src/app/features):
- 🏠 Layout shell: [web/src/app/features/layout](web/src/app/features/layout)
- 🧺 Catalog list & product detail: [web/src/app/features/catalog](web/src/app/features/catalog)
- 🛒 Cart: [web/src/app/features/cart](web/src/app/features/cart)
- 💳 Checkout: [web/src/app/features/checkout](web/src/app/features/checkout)
- 📦 Orders: [web/src/app/features/orders](web/src/app/features/orders)
- 🔐 Auth dialog: [web/src/app/features/auth](web/src/app/features/auth)

Routes are defined in [web/src/app/app.routes.ts](web/src/app/app.routes.ts) with lazy-loaded feature route files.

## 🔌 Backend Map
The gateway forwards paths to services (configured in [src/ApiGateways/YarpApiGateway/appsettings.json](src/ApiGateways/YarpApiGateway/appsettings.json)):
- `/catalog-service/*` → Catalog API
- `/basket-service/*` → Basket API
- `/ordering-service/*` → OrderStream API

All APIs read the JWT from the **`hh_access_token` cookie** and enforce scope-based policies like `catalog_read`, `basket_write`, etc.

## 🚀 Local Development

### 1) Start infrastructure + services (containers)
Use Docker Compose from [src/docker-compose.yml](src/docker-compose.yml):

1. Start containers (Postgres, Redis, RabbitMQ, SQL Server, APIs)
2. Services will be reachable via the **YARP Gateway**

### 2) Run the Angular SPA
The SPA is located in [web](web). Use Angular CLI to run the dev server.

### 3) Build the .NET solution
Use the solution at [src/HarvestHub.sln](src/HarvestHub.sln) (build/publish/watch tasks are preconfigured in VS Code).

## 🧪 Helpful Notes
- API base URLs are configurable via localStorage keys:
	- `hh_api_base_url` (default: `http://localhost:6004`)
	- `hh_auth_base_url` (default: `http://localhost:6005`)
- HTTP calls from the SPA use an auth interceptor so cookies are included.
- Basket service decorates its repository with a caching layer.

## 🧑‍💻 Contributing Tips
- Prefer **Carter modules** for HTTP endpoints.
- Keep gateway routes aligned with docker-compose service names.
- Reuse existing message broker setup via BuildingBlocks.

---

If you need anything else added (API endpoints list, diagrams, or dev scripts), say the word 🚜