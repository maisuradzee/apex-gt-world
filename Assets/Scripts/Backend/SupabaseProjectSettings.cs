using UnityEngine;

namespace ApexGTWorld.Backend
{
    /// <summary>
    /// Public client configuration for the APEX GT WORLD Supabase project.
    /// A publishable key is designed for client use when RLS is correctly configured.
    /// Never add a service_role or sb_secret key here.
    /// </summary>
    public static class SupabaseProjectSettings
    {
        public const string ProjectUrl = "https://hzawgrvonsqyqjyzzreg.supabase.co";
        public const string ProjectRef = "hzawgrvonsqyqjyzzreg";
        public const string PublishableKey = "sb_publishable_lv67xFJr4GRLOmqrJAQG9A_onqEvDCN";

        public static bool IsConfigured =>
            ProjectUrl.StartsWith("https://") &&
            ProjectRef.Length > 0 &&
            PublishableKey.StartsWith("sb_publishable_");
    }
}
