# HarvestHub AI Instructions

## Architecture overview
- Solution is a microservices stack under src with a YARP API gateway, multiple .NET services, and an Angular SPA under web.
- Web SPA: Angular standalone components + route-level lazy loading; routes are defined in [web/src/app/app.routes.ts](web/src/app/app.routes.ts) and load feature route files under web/src/app/features.
- Key web components live under feature folders: layout shell in [web/src/app/features/layout/layout.component.ts](web/src/app/features/layout/layout.component.ts), catalog list/detail in [web/src/app/features/catalog/catalog-list.component.ts](web/src/app/features/catalog/catalog-list.component.ts) and [web/src/app/features/catalog/product-detail.component.ts](web/src/app/features/catalog/product-detail.component.ts), cart in [web/src/app/features/cart/cart.component.ts](web/src/app/features/cart/cart.component.ts), checkout in [web/src/app/features/checkout/checkout.component.ts](web/src/app/features/checkout/checkout.component.ts), orders in [web/src/app/features/orders/orders.component.ts](web/src/app/features/orders/orders.component.ts), and auth dialog in [web/src/app/features/auth/auth-dialog.component.ts](web/src/app/features/auth/auth-dialog.component.ts).
- API gateway: YARP reverse proxy routes */basket-service*, */catalog-service*, */ordering-service* to internal services; update [src/ApiGateways/YarpApiGateway/appsettings.json](src/ApiGateways/YarpApiGateway/appsettings.json) when adding routes or clusters.
- Auth flows: Identity service issues JWT and sets cookie hh_access_token; all APIs read JWT from cookie in their auth handlers (see [src/Identity/Identity.Api/Program.cs](src/Identity/Identity.Api/Program.cs) and [src/Services/Basket/Basket.Api/Program.cs](src/Services/Basket/Basket.Api/Program.cs)).
- Service boundaries:
  - Catalog.Api and Basket.Api use Carter + MediatR + FluentValidation + Marten/Postgres (see [src/Services/Catalog/Catalog.Api/Program.cs](src/Services/Catalog/Catalog.Api/Program.cs) and [src/Services/Basket/Basket.Api/Program.cs](src/Services/Basket/Basket.Api/Program.cs)).
  - Discount.Grpc is a gRPC service with SQLite EF Core (see [src/Services/Discount/Discount.Grpc/Program.cs](src/Services/Discount/Discount.Grpc/Program.cs)).
  - OrderStream uses layered Application/Infrastructure/Api with EF Core SQL Server, domain events interceptors, and FeatureManagement (see [src/Services/Order/OrderStream.Application/DependencyInjection.cs](src/Services/Order/OrderStream.Application/DependencyInjection.cs) and [src/Services/Order/OrderStream.Infrastructure/DependencyInjection.cs](src/Services/Order/OrderStream.Infrastructure/DependencyInjection.cs)).
- Messaging: MassTransit + RabbitMQ via shared extension in [src/BuildingBlocks/BuildingBlocks.MessageBroker/MassTransit/Extension.cs](src/BuildingBlocks/BuildingBlocks.MessageBroker/MassTransit/Extension.cs).
- Cross-cutting pipeline: MediatR logging + validation behaviors in BuildingBlocks (example: [src/BuildingBlocks/BuildingBlocks/Behaviors/LoggingBehavior.cs](src/BuildingBlocks/BuildingBlocks/Behaviors/LoggingBehavior.cs)).

## Local dev workflows
- .NET build/watch: use the solution at src/HarvestHub.sln (tasks: build/publish/watch are preconfigured).
- Containers: docker-compose spins up infra (Postgres, Redis, RabbitMQ, SQL Server) and all APIs (see [src/docker-compose.yml](src/docker-compose.yml) and [src/docker-compose.override.yml](src/docker-compose.override.yml)); env vars there define connection strings and service ports.
- Web SPA: Angular CLI dev server and commands are documented in [web/README.md](web/README.md) (ng serve/build/test).

## Project-specific conventions
- Auth scopes are enforced per service policy names Read/Write with scope claims (catalog_* / basket_* / order_*). Follow the same policy naming when adding endpoints (see [src/Services/Catalog/Catalog.Api/Program.cs](src/Services/Catalog/Catalog.Api/Program.cs)).
- Carter is used for HTTP endpoints in services (MapCarter in Program.cs or AddApiServices in OrderStream); prefer Carter modules over controllers.
- Basket service decorates IBasketRepository with cache (registration in [src/Services/Basket/Basket.Api/Program.cs](src/Services/Basket/Basket.Api/Program.cs)); keep caching behavior in the decorator rather than the base repository.
- When adding consumers or message contracts, register them via AddMessageBroker with the calling assembly (see [src/Services/Order/OrderStream.Application/DependencyInjection.cs](src/Services/Order/OrderStream.Application/DependencyInjection.cs)).
- Web app API access goes through `ApiService` with gateway-relative paths (ex: /catalog-service/products); see [web/src/app/core/api/api.service.ts](web/src/app/core/api/api.service.ts) and service wrappers in [web/src/app/core/services](web/src/app/core/services).
- Base URLs are configurable via localStorage keys hh_api_base_url and hh_auth_base_url (defaults http://localhost:6004 and http://localhost:6005) in [web/src/app/app.config.ts](web/src/app/app.config.ts).
- Auth state uses Angular signals in `AuthService` and is kept in sync with /api/auth/me (see [web/src/app/core/auth/auth.service.ts](web/src/app/core/auth/auth.service.ts)); the `authGuard` refreshes session when needed.
- HTTP requests to API/auth base URLs must include credentials via `authInterceptor` (cookie-based JWT); keep new API calls going through HttpClient with that interceptor in [web/src/app/core/auth/auth.interceptor.ts](web/src/app/core/auth/auth.interceptor.ts).

## Integration points
- Internal service URLs in YARP point to container DNS names (basket.api, catalog.api, orderstream.api); keep these aligned with docker-compose service names.
- Basket calls Discount over gRPC via GrpcSettings:DiscountUrl (see [src/Services/Basket/Basket.Api/Program.cs](src/Services/Basket/Basket.Api/Program.cs)).
- Health checks are exposed at /health for APIs; keep new checks wired in the same pattern (see [src/Services/Order/OrderStream.Api/DependencyInjection.cs](src/Services/Order/OrderStream.Api/DependencyInjection.cs)).
