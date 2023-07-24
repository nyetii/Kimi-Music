using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Kimi.Services.Core;

namespace Kimi.Services.Helpers
{
    public static class AttachmentHelper
    {
        private static HttpClient? _httpClient;
        public static async Task DownloadAttachment(this IAttachment value, string? name = null)
        {
            var socketsHandler = new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromSeconds(30)
            };

            _httpClient = new HttpClient(socketsHandler);

            var attachment = await _httpClient.GetAsync(value.Url);

            attachment.EnsureSuccessStatusCode();

            name ??= value.Filename.Split('.').GetValue(0)?.ToString();
            var type = value.Filename.Split('.').GetValue(1)?.ToString();

            var path = $@"{Info.AppDataPath}\{name}.{type}";

            await using var filestream = new FileStream(path, FileMode.CreateNew);
            await attachment.Content.CopyToAsync(filestream);
        }
        
        public static bool IsValid(this IAttachment value, int limit = 1_000_000)
        {
            return value.Size <= limit;
        }
    }
}
