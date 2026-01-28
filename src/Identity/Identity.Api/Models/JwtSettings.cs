namespace Identity.Api.Models
{
    public record JwtSettings(string Issuer, string Audience, string SigningKey, int TokenLifetimeMinutes);
}
