using System;

namespace Web.Helpers
{
    public static class ApiKeyHelper
    {
        public static string Generate()
        {
            return Guid.NewGuid().ToString().Replace("-", string.Empty);
        }
    }
}