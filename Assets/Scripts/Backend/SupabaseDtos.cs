using System;

namespace ApexGTWorld.Backend
{
    [Serializable]
    public sealed class SupabaseAuthCredentials
    {
        public string email;
        public string password;
    }

    [Serializable]
    public sealed class SupabaseSignUpRequest
    {
        public string email;
        public string password;
        public SupabaseSignUpMetadata data;
    }

    [Serializable]
    public sealed class SupabaseSignUpMetadata
    {
        public string display_name;
    }

    [Serializable]
    public sealed class SupabaseRefreshRequest
    {
        public string refresh_token;
    }

    [Serializable]
    public sealed class SupabaseAuthSession
    {
        public string access_token;
        public string token_type;
        public int expires_in;
        public string refresh_token;
        public SupabaseAuthUser user;
    }

    [Serializable]
    public sealed class SupabaseAuthUser
    {
        public string id;
        public string email;
    }

    [Serializable]
    public sealed class ApexProfile
    {
        public string user_id;
        public string display_name;
        public long reputation;
        public string home_region;
    }

    [Serializable]
    internal sealed class ApexProfileUpdate
    {
        public string display_name;
    }

    [Serializable]
    public sealed class ApexWallet
    {
        public string user_id;
        public long credits;
        public long premium_tokens;
        public string updated_at;
    }

    [Serializable]
    public sealed class ApexPlayerProgress
    {
        public string user_id;
        public int career_level;
        public long career_xp;
        public string[] unlocked_content;
        public string updated_at;
    }

    [Serializable]
    internal sealed class ApexRaceRewardClaimRequest
    {
        public string race_result_id;
    }

    [Serializable]
    public sealed class ApexRaceRewardSettlement
    {
        public bool ok;
        public bool applied;
        public long reward_credits;
        public long credits_balance;
        public long premium_tokens_balance;
        public string race_result_id;
    }

    [Serializable]
    public sealed class ApexRegion
    {
        public string region_id;
        public string display_name;
        public bool enabled;
    }

    [Serializable]
    internal sealed class ApexProfileArray { public ApexProfile[] items; }

    [Serializable]
    internal sealed class ApexWalletArray { public ApexWallet[] items; }

    [Serializable]
    internal sealed class ApexPlayerProgressArray { public ApexPlayerProgress[] items; }

    [Serializable]
    internal sealed class ApexRegionArray { public ApexRegion[] items; }

    [Serializable]
    internal sealed class SupabaseErrorBody
    {
        public string message;
        public string error;
        public string error_description;
        public string msg;
        public string code;
    }
}
