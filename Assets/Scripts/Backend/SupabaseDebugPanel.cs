using System.Collections;
using UnityEngine;
using ApexGTWorld.Backend;

namespace ApexGTWorld.UI
{
    /// <summary>
    /// Development-only panel for validating Supabase Auth and basic PostgREST access.
    /// Replace this OnGUI panel with the production UI before release.
    /// </summary>
    public sealed class SupabaseDebugPanel : MonoBehaviour
    {
        [SerializeField] private SupabaseRestClient client;
        private string email = string.Empty;
        private string password = string.Empty;
        private string displayName = "Driver";
        private string status = "Supabase: not signed in";
        private string walletStatus = "Wallet: not loaded";
        private string progressStatus = "Career: not loaded";
        private bool visible = true;

        private void Awake()
        {
            if (client == null) client = GetComponent<SupabaseRestClient>();
        }

        private void OnGUI()
        {
            if (!visible)
            {
                if (GUI.Button(new Rect(12, 12, 150, 42), "SUPABASE TEST")) visible = true;
                return;
            }

            float width = Mathf.Min(Screen.width - 24f, 460f);
            float height = Mathf.Min(Screen.height - 24f, 440f);
            GUI.Box(new Rect(12, 12, width, height), "APEX GT WORLD — SUPABASE TEST");
            GUILayout.BeginArea(new Rect(24, 42, width - 24, height - 54));
            GUILayout.Label("Email");
            email = GUILayout.TextField(email, 120);
            GUILayout.Label("Password");
            password = GUILayout.PasswordField(password, '*', 120);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("SIGN IN", GUILayout.Height(38)))
                StartCoroutine(client.SignInWithPassword(email, password, (session, error) =>
                    status = error ?? ("Signed in: " + (session.user != null ? session.user.email : "user"))));
            if (GUILayout.Button("SIGN UP", GUILayout.Height(38)))
                StartCoroutine(client.SignUpWithPassword(email, password, displayName, (session, error) =>
                    status = error ?? (session != null && session.access_token != null ? "Account created and signed in" : "Account created; check email confirmation")));
            GUILayout.EndHorizontal();
            GUILayout.Space(6);
            displayName = GUILayout.TextField(displayName, 24);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("SAVE PROFILE", GUILayout.Height(34)))
                StartCoroutine(client.SaveMyProfile(displayName, (profile, error) => status = error ?? "Profile saved"));
            if (GUILayout.Button("LOAD PROFILE", GUILayout.Height(34)))
                StartCoroutine(client.FetchMyProfile((profile, error) => status = error ?? (profile == null ? "Profile not found" : "Profile: " + profile.display_name)));
            GUILayout.EndHorizontal();
            if (GUILayout.Button("LOAD WORLD REGIONS", GUILayout.Height(34)))
                StartCoroutine(client.FetchRegions((regions, error) => status = error ?? ("Regions loaded: " + (regions == null ? 0 : regions.Length))));
            if (GUILayout.Button("SIGN OUT", GUILayout.Height(30)))
                StartCoroutine(client.SignOut((ok, error) => status = error ?? "Signed out"));
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("LOAD WALLET", GUILayout.Height(34)))
                StartCoroutine(client.FetchMyWallet((wallet, error) =>
                    walletStatus = error ?? (wallet == null ? "Wallet not found" :
                        "Credits: " + wallet.credits + " | Premium: " + wallet.premium_tokens)));
            if (GUILayout.Button("LOAD CAREER", GUILayout.Height(34)))
                StartCoroutine(client.FetchMyProgress((progress, error) =>
                    progressStatus = error ?? (progress == null ? "Career not found" :
                        "Level: " + progress.career_level + " | XP: " + progress.career_xp)));
            GUILayout.EndHorizontal();
            GUILayout.Label(walletStatus);
            GUILayout.Label(progressStatus);
            GUILayout.Label("Status: " + status);
            GUILayout.Label("This is a development console, not the final game UI.");
            if (GUILayout.Button("HIDE", GUILayout.Height(26))) visible = false;
            GUILayout.EndArea();
        }
    }
}
