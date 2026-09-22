namespace RoyalVillaWeb
{
    public static class SD
    {
        public enum ApiType
        {
            GET,
            POST,
            PUT,
            DELETE,
        }

        public const string SessionAccessToken = "JWTToken";
        public const string SessionRefreshToken = "RefreshToken";

        public enum Roles
        {
            Admin,
            Customer
        }

        public const string CurrentApiVersion = "v2";
        public static string ApiBaseUrl { get; set; }

        public static string GetImageUrl(string? imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                return "/images/placeholder-villa.jpg";
            }

            if (imageUrl.StartsWith("http"))
            {
                return imageUrl;
            }

            return $"{ApiBaseUrl}{imageUrl}";
        }
    }
}
