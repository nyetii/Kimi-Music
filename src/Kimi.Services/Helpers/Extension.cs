using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kimi.Services.Helpers
{
    public static class Extension
    {
        public static Guid ToGuid(this string value) => Guid.Parse(value);

        public static string Fallback(this string? str, string placeholder) => string.IsNullOrWhiteSpace(str) ? placeholder : str;
    }
}
