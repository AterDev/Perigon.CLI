using System.Text.Json.Serialization;

namespace Share.Models.Auth;
/// <summary>
/// 刷新 token
/// </summary>
public class TokenRefreshDto
{
    [JsonPropertyName("refresh_token")]
    public required string RefreshToken { get; set; }
}
