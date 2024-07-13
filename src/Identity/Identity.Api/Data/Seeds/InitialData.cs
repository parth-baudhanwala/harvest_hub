using IdentityServer4.Models;

namespace Identity.Api.Data.Seeds;

public static class InitialData
{
    public static IEnumerable<IdentityResource> IdentityResources =>
    [
        new IdentityResources.OpenId(),
        new IdentityResources.Profile(),
        new IdentityResource
        {
            Name = "role",
            UserClaims = ["role"]
        }
    ];

    public static IEnumerable<ApiScope> ApiScopes =>
    [
        new ApiScope("basket_read"),
        new ApiScope("basket_write"),
        new ApiScope("catalog_read"),
        new ApiScope("catalog_write"),
        new ApiScope("order_read"),
        new ApiScope("order_write"),
    ];

    public static IEnumerable<ApiResource> ApiResources =>
    [
        new ApiResource("Basket")
        {
            ApiSecrets = [new Secret("BasketApiSecret".Sha256())],
            Scopes = ["basket_read", "basket_write"],
            UserClaims = ["role"]
        },
        new ApiResource("Catalog")
        {
            ApiSecrets = [new Secret("CatalogApiSecret".Sha256())],
            Scopes = ["catalog_read", "catalog_write"],
            UserClaims = ["role"]
        },
        new ApiResource("Order")
        {
            ApiSecrets = [new Secret("OrderApiSecret".Sha256())],
            Scopes = ["order_read", "order_write"],
            UserClaims = ["role"]
        }
    ];

    public static IEnumerable<Client> Clients =>
    [
        new Client
        {
            ClientId = "Basket.Api",
            ClientName = "Basket API",
            AllowedGrantTypes = GrantTypes.ClientCredentials,
            ClientSecrets = { new Secret("BasketApiSecret".Sha256()) },
            AllowedScopes = { "basket_read", "basket_write" }
        },
        new Client
        {
            ClientId = "Catalog.Api",
            ClientName = "Catalog API",
            AllowedGrantTypes = GrantTypes.ClientCredentials,
            ClientSecrets = { new Secret("CatalogApiSecret".Sha256()) },
            AllowedScopes = { "catalog_read", "catalog_write" }
        },
        new Client
        {
            ClientId = "Order.Api",
            ClientName = "Order Stream API",
            AllowedGrantTypes = GrantTypes.ClientCredentials,
            ClientSecrets = { new Secret("OrderApiSecret".Sha256()) },
            AllowedScopes = { "order_read", "order_write" }
        },

        // interactive client using code flow + pkce
        new Client
        {
            ClientId = "HarvestHub.Web.Client",
            ClientSecrets = { new Secret("HarvestHubClientSecret".Sha256()) },
            AllowedGrantTypes = GrantTypes.Code,
            RedirectUris = { "https://localhost:7022/signin-oidc" },
            FrontChannelLogoutUri = "https://localhost:7022/signout-oidc",
            PostLogoutRedirectUris = { "https://localhost:7022/signout-callback-oidc" },
            AllowOfflineAccess = true,
            AllowedScopes = { "openid", "profile", "basket_read", "basket_write", "catalog_read", "catalog_write", "order_read", "order_write" },
            RequirePkce = true,
            RequireConsent = true,
            AllowPlainTextPkce = false
        },
    ];
}
