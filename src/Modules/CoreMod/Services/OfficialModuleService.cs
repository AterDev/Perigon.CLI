using System.Net.Http.Headers;

namespace CoreMod.Services;

/// <summary>
/// Provides access to official Perigon modules hosted on GitHub.
/// </summary>
public class OfficialModuleService
{
    private const string OfficialPackagePrefix = "Perigon.";
    private const string GitHubApiUrl = "https://api.github.com";
    private static readonly TimeSpan OfficialModulesCacheDuration = TimeSpan.FromMinutes(1);
    private const string OfficialModulesMetadataUrlFormat =
        "https://raw.githubusercontent.com/AterDev/Perigon.Modules/{0}/modules.json";
    private const string OfficialModulesPackageUrlFormat =
        "https://raw.githubusercontent.com/AterDev/Perigon.Modules/{0}/package_modules/{1}.zip";

    private static readonly HttpClient SharedHttpClient = CreateDefaultHttpClient();

    private readonly HttpClient _httpClient;
    private readonly Localizer _localizer;
    private readonly ILogger<OfficialModuleService> _logger;
    private readonly SemaphoreSlim _modulesCacheLock = new(1, 1);
    private readonly Dictionary<string, (IReadOnlyList<PackageMetadata> Modules, DateTimeOffset ExpiresAt)> _modulesCache = [];

    public OfficialModuleService(Localizer localizer, ILogger<OfficialModuleService> logger)
        : this(SharedHttpClient, localizer, logger) { }

    public OfficialModuleService(
        HttpClient httpClient,
        Localizer localizer,
        ILogger<OfficialModuleService> logger
    )
    {
        _httpClient = httpClient;
        _localizer = localizer;
        _logger = logger;
    }

    public static bool IsOfficialPackageName(string packageName)
    {
        return !string.IsNullOrWhiteSpace(packageName)
            && packageName.StartsWith(OfficialPackagePrefix, StringComparison.OrdinalIgnoreCase);
    }

    public static string NormalizeOfficialModuleName(string packageName)
    {
        var moduleName = packageName.Trim();
        if (moduleName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        {
            moduleName = Path.GetFileNameWithoutExtension(moduleName);
        }

        if (moduleName.StartsWith(OfficialPackagePrefix, StringComparison.OrdinalIgnoreCase))
        {
            moduleName = moduleName[OfficialPackagePrefix.Length..];
        }

        return moduleName;
    }

    public async Task<bool> CanConnectToGitHubAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, GitHubApiUrl);
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to connect to GitHub");
            return false;
        }
    }

    public Task<IReadOnlyList<PackageMetadata>> GetOfficialModulesAsync(
        CancellationToken cancellationToken = default
    )
    {
        return GetOfficialModulesAsync("main", cancellationToken);
    }

    private async Task<IReadOnlyList<PackageMetadata>> GetOfficialModulesAsync(
        string branch,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await _modulesCacheLock.WaitAsync(cancellationToken);
            try
            {
                if (_modulesCache.TryGetValue(branch, out var cached)
                    && cached.ExpiresAt > DateTimeOffset.UtcNow)
                {
                    return cached.Modules;
                }

                var metadataUrl = string.Format(OfficialModulesMetadataUrlFormat, branch);
                using var request = new HttpRequestMessage(HttpMethod.Get, metadataUrl);
                using var response = await _httpClient.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();

                await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                var modules = await JsonSerializer.DeserializeAsync(
                    stream,
                    PerigonJsonSerializerContext.ModuleMetadata.ListPackageMetadata,
                    cancellationToken
                );

                IReadOnlyList<PackageMetadata> officialModules = modules?
                    .OrderBy(m => m.ModuleName, StringComparer.OrdinalIgnoreCase)
                    .ToList()
                    ?? [];
                _modulesCache[branch] = (officialModules, DateTimeOffset.UtcNow.Add(OfficialModulesCacheDuration));

                return officialModules;
            }
            finally
            {
                _modulesCacheLock.Release();
            }
        }
        catch (Exception ex) when (ContainsHttpRequestException(ex))
        {
            // Network failures are handled by the CLI so solution creation can continue
            // without official modules. Do not log the exception here because the
            // transport details are not useful to the end user.
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch official module metadata");
            throw new InvalidOperationException(
                _localizer.Get(Localizer.OfficialModulesFetchFailed),
                ex
            );
        }
    }

    public Task<string> DownloadOfficialModulePackageAsync(
        string packageName,
        string targetDirectory,
        CancellationToken cancellationToken = default
    )
    {
        return DownloadOfficialModulePackageAsync(packageName, targetDirectory, false, cancellationToken);
    }

    public async Task<string> DownloadOfficialModulePackageAsync(
        string packageName,
        string targetDirectory,
        bool isAot,
        CancellationToken cancellationToken
    )
    {
        var branch = isAot ? "aot" : "main";
        var moduleName = NormalizeOfficialModuleName(packageName);
        var modules = await GetOfficialModulesAsync(branch, cancellationToken);
        var module = modules.FirstOrDefault(m =>
            string.Equals(m.ModuleName, moduleName, StringComparison.OrdinalIgnoreCase)
        );

        if (module == null)
        {
            throw new InvalidOperationException(
                _localizer.Get(Localizer.OfficialModuleNotFound, packageName)
            );
        }

        Directory.CreateDirectory(targetDirectory);
        var packagePath = Path.Combine(targetDirectory, $"{module.ModuleName}.zip");
        var downloadUrl = string.Format(OfficialModulesPackageUrlFormat, branch, module.ModuleName);

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, downloadUrl);
            using var response = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken
            );
            response.EnsureSuccessStatusCode();

            await using var packageStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using var fileStream = File.Create(packagePath);
            await packageStream.CopyToAsync(fileStream, cancellationToken);

            return packagePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download official module package {ModuleName}", moduleName);
            throw new InvalidOperationException(
                _localizer.Get(Localizer.OfficialModuleDownloadFailed, moduleName),
                ex
            );
        }
    }

    private static bool ContainsHttpRequestException(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is HttpRequestException)
            {
                return true;
            }
        }

        return false;
    }

    private static HttpClient CreateDefaultHttpClient()
    {
        // HttpClient.DefaultProxy is initialized from HTTP_PROXY, HTTPS_PROXY,
        // ALL_PROXY and NO_PROXY by the runtime.
        var client = new HttpClient(
            new HttpClientHandler
            {
                UseProxy = true,
                Proxy = HttpClient.DefaultProxy
            }
        );
        client.DefaultRequestHeaders.UserAgent.Add(
            new ProductInfoHeaderValue("Perigon.CLI", AssemblyHelper.GetCurrentToolVersion())
        );
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json")
        );
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/octet-stream")
        );
        return client;
    }
}
