namespace friendout_backend.Providers
{
    /// <summary>
    /// Represents the collection of OAuth providers configured for the application.
    /// </summary>
    public class OAuthProviders
    {
        /// <summary>
        /// A dictionary containing configuration options for each OAuth provider.
        /// The key is the provider name (e.g., "Discord") and the value is an instance of <see cref="OAuthProviderOptions"/>.
        /// </summary>
        public Dictionary<string, OAuthProviderOptions> Providers { get; set; } = new();
    }

    /// <summary>
    /// Represents the configuration options for an OAuth provider.
    /// </summary>
    public class OAuthProviderOptions
    {
        /// <summary>
        /// Gets or sets the OAuth client identifier provided by the provider (ClientId).
        /// </summary>
        public string ClientId { get; set; } = null!;

        /// <summary>
        /// Gets or sets the OAuth client secret provided by the provider (ClientSecret).
        /// </summary>
        public string ClientSecret { get; set; } = null!;

        /// <summary>
        /// Gets or sets the OAuth callback path that the provider will redirect to after authentication.
        /// Example: "/auth/discord/callback".
        /// </summary>
        public string CallBack { get; set; } = null!;
    }
}