using Microsoft.AspNetCore.SignalR;

namespace SignalRApi;

public sealed class QueryStringUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        var userId = connection.GetHttpContext()?.Request.Query["userId"].ToString();
        return string.IsNullOrWhiteSpace(userId) ? "Alice" : userId;
    }
}
