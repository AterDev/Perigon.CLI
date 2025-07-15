namespace StudioMod.Models.ApiDocInfoDtos;

public class RequestClientDto
{
    [Required]
    public string? OpenApiEndpoint { get; set; }

    [Required]
    public RequestClientType ClientType { get; set; }

    [Required]
    public string? OutputPath { get; set; }
}
