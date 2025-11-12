using Ater.AspNetCore.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Ater.AspNetCore.Toolkit;

/// <summary>
/// 服务扩展
/// </summary>
public static class ServiceExtentisons
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder AddToolKitServices()
        {
            var components =
                builder.Configuration.GetSection(ComponentOption.ConfigPath).Get<ComponentOption>()
                ?? throw new Exception($"can't get {ComponentOption.ConfigPath} config");

            builder.AddToolkitOptions(components);
            return builder;
        }

        public IHostApplicationBuilder AddToolkitOptions(
            ComponentOption components
        )
        {
            var config = builder.Configuration;
            if (components.UseSmtp)
            {
                builder.Services.Configure<SmtpOption>(config.GetSection(SmtpOption.ConfigPath));
            }
            if (components.UseSMS)
            {
                builder.Services.Configure<SMSOption>(config.GetSection(SMSOption.ConfigPath));
            }
            if (components.UseAWSS3)
            {
                builder.Services.Configure<AWSS3Option>(config.GetSection(AWSS3Option.ConfigPath));
            }

            return builder;
        }
    }
}
