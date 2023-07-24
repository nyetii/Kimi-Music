using System.Net.NetworkInformation;
using Discord;

namespace Kimi.Services.Core
{
    public class Settings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public General General { get; set; } = new();
        public Profile Profile { get; set; } = new();
    }

    public class General
    {
        public string[] Prefix { get; set; } = { "!", "m!" };
        public ulong[]? DebugGuildId { get; set; } = null;
    }

    public class Profile
    {
        public string? Status { get; set; } = null;
        public string? Link { get; set; } = null;
        public ActivityType ActivityType { get; set; } = 0;
        public UserStatus UserStatus { get; set; } = (UserStatus)1;
    }
}
