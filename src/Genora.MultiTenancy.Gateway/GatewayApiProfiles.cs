namespace Genora.MultiTenancy.Gateway;

// Explicit public API inventory. Admin and SignalR are outside these HTTP profiles.
public static class GatewayApiProfiles
{
    public static IReadOnlyList<GatewayApiRoute> GetEndpoints(GatewayTenantOptions tenant)
    {
        var result = new List<GatewayApiRoute>();
        foreach (var profile in tenant.ApiProfiles)
        {
            if (profile.Equals("Hl25", StringComparison.OrdinalIgnoreCase))
                result.AddRange([
                    new() { Path = "/api/mini-app/hl25/decode-phone", Methods = ["POST"] },
                    new() { Path = "/api/mini-app/hl25/config", Methods = ["GET"] },
                    new() { Path = "/api/mini-app/hl25/participants/register", Methods = ["POST"] },
                    new() { Path = "/api/mini-app/hl25/participants/me", Methods = ["GET", "PUT"] },
                    new() { Path = "/api/mini-app/hl25/frames", Methods = ["POST"] },
                    new() { Path = "/api/mini-app/hl25/frames/share", Methods = ["POST"] },
                    new() { Path = "/api/mini-app/hl25/wheel", Methods = ["GET"] },
                    new() { Path = "/api/mini-app/hl25/wheel/spin", Methods = ["POST"] },
                    new() { Path = "/api/mini-app/hl25/me/gifts", Methods = ["GET"] },
                    new() { Path = "/api/mini-app/hl25/frames/campaigns", Methods = ["GET"] },
                    new() { Path = "/api/mini-app/hl25/frames/templates", Methods = ["GET"] },
                    new() { Path = "/api/mini-app/hl25/me/frames", Methods = ["GET"] },
                    new() { Path = "/api/mini-app/hl25/gifts", Methods = ["GET"] },
                    new() { Path = "/api/mini-app/hl25/me/spin-turns", Methods = ["GET"] },
                    new() { Path = "/api/mini-app/hl25/me/spins", Methods = ["GET"] },
                    new() { Path = "/api/mini-app/hl25/upload-image", Methods = ["POST"] },
                ]);
            else if (profile.Equals("Hlg", StringComparison.OrdinalIgnoreCase))
                result.AddRange([
                    new() { Path = "/api/mini-app/hlg/decode-phone", Methods = ["POST"] },
                    new() { Path = "/api/mini-app/hlg/customer/upsert", Methods = ["POST"] },
                    new() { Path = "/api/mini-app/hlg/customer/by-phone", Methods = ["GET"] },
                    new() { Path = "/api/mini-app/hlg/profile", Methods = ["PUT"] },
                    new() { Path = "/api/mini-app/hlg/profile/stats", Methods = ["GET"] },
                    new() { Path = "/api/mini-app/hlg/profile/learning-history", Methods = ["GET"] },
                    new() { Path = "/api/mini-app/hlg/profile/point-history", Methods = ["GET"] },
                    new() { Path = "/api/mini-app/hlg/profile/reward-history", Methods = ["GET"] },
                    new() { Path = "/api/mini-app/hlg/knowledge/categories", Methods = ["GET"] },
                    new() { Path = "/api/mini-app/hlg/knowledge/categories/{id}", Methods = ["GET"] },
                    new() { Path = "/api/mini-app/hlg/knowledge/categories/{id}/products", Methods = ["GET"] },
                    new() { Path = "/api/mini-app/hlg/knowledge/products/{id}", Methods = ["GET"] },
                    new() { Path = "/api/mini-app/hlg/knowledge/products/{id}/complete", Methods = ["POST"] },
                    new() { Path = "/api/mini-app/hlg/games", Methods = ["GET"] },
                    new() { Path = "/api/mini-app/hlg/games/{id}", Methods = ["GET"] },
                    new() { Path = "/api/mini-app/hlg/games/{id}/start", Methods = ["POST"] },
                    new() { Path = "/api/mini-app/hlg/games/answer", Methods = ["POST"] },
                    new() { Path = "/api/mini-app/hlg/games/sessions/{sessionId}/finish", Methods = ["POST"] },
                    new() { Path = "/api/mini-app/hlg/games/{id}/live-feed", Methods = ["GET"] },
                    new() { Path = "/api/mini-app/hlg/games/sessions/{sessionId}/shipping-address", Methods = ["POST"] },
                    new() { Path = "/api/mini-app/hlg/rewards", Methods = ["GET"] },
                    new() { Path = "/api/mini-app/hlg/rewards/{id}/redeem", Methods = ["POST"] },
                    new() { Path = "/api/mini-app/hlg/ranking/event", Methods = ["GET"] },
                    new() { Path = "/api/mini-app/hlg/ranking/entries", Methods = ["GET"] },
                    // CMS content + phân cấp Ngành hàng→Nhãn hàng→Sản phẩm + ranking events (khớp HoaLinhGamificationContentController và các endpoint mới của HoaLinhGamificationController)
                    new() { Path = "/api/mini-app/hlg/content", Methods = ["GET"] },
                    new() { Path = "/api/mini-app/hlg/knowledge/brands", Methods = ["GET"] },
                    new() { Path = "/api/mini-app/hlg/knowledge/brands/{id}", Methods = ["GET"] },
                    new() { Path = "/api/mini-app/hlg/knowledge/products", Methods = ["GET"] },
                    new() { Path = "/api/mini-app/hlg/knowledge/products/{id}/progress", Methods = ["POST"] },
                    new() { Path = "/api/mini-app/hlg/profile/game-history", Methods = ["GET"] },
                    new() { Path = "/api/mini-app/hlg/games/sessions/{sessionId}/rewards/{rewardId}/redeem", Methods = ["POST"] },
                    new() { Path = "/api/mini-app/hlg/ranking/events", Methods = ["GET"] },
                    new() { Path = "/api/mini-app/hlg/ranking/events/{id}/prizes", Methods = ["GET"] },
                    new() { Path = "/api/mini-app/hlg/ranking/events/{id}/winners", Methods = ["GET"] },
                    new() { Path = "/api/mini-app/hlg/ranking/events/{id}/entries", Methods = ["GET"] },
                    new() { Path = "/api/mini-app/hlg/ranking/share-image", Methods = ["POST"] },
                ]);
            else throw new InvalidOperationException("Unknown TenantGateway API profile; use Hl25, Hlg or explicit AdditionalRoutes.");
        }
        result.AddRange(tenant.AdditionalRoutes);
        return result;
    }
}
