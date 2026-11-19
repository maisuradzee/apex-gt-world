using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace ApexGTWorld.Backend
{
    /// <summary>
    /// Lightweight UnityWebRequest adapter for Supabase Auth and PostgREST.
    /// It stores the access token in memory only. Production token persistence must
    /// use Android Keystore / iOS Keychain through a vetted secure-storage plugin.
    /// </summary>
    public sealed class SupabaseRestClient : MonoBehaviour
    {
        public SupabaseAuthSession CurrentSession { get; private set; }
        public bool IsSignedIn => CurrentSession != null && !string.IsNullOrEmpty(CurrentSession.access_token);

        public IEnumerator SignInWithPassword(string email, string password, Action<SupabaseAuthSession, string> completed)
        {
            if (IsSignedIn)
            {
                completed?.Invoke(null, "Sign out before switching accounts.");
                yield break;
            }

            var body = JsonUtility.ToJson(new SupabaseAuthCredentials { email = email, password = password });
            yield return Send(
                "/auth/v1/token?grant_type=password",
                UnityWebRequest.kHttpVerbPOST, body, false,
                (json, error) =>
                {
                    if (!string.IsNullOrEmpty(error)) { completed?.Invoke(null, error); return; }
                    try
                    {
                        CurrentSession = JsonUtility.FromJson<SupabaseAuthSession>(json);
                        if (CurrentSession == null || string.IsNullOrEmpty(CurrentSession.access_token))
                            throw new Exception("Supabase Auth returned no access token.");
                        completed?.Invoke(CurrentSession, null);
                    }
                    catch (Exception ex) { completed?.Invoke(null, ex.Message); }
                });
        }

        public IEnumerator SignUpWithPassword(string email, string password, string displayName, Action<SupabaseAuthSession, string> completed)
        {
            if (IsSignedIn)
            {
                completed?.Invoke(null, "Sign out before creating a new account.");
                yield break;
            }

            if (string.IsNullOrWhiteSpace(displayName) || displayName.Trim().Length < 3 || displayName.Trim().Length > 24)
            {
                completed?.Invoke(null, "Display name must contain 3–24 characters.");
                yield break;
            }

            var body = JsonUtility.ToJson(new SupabaseSignUpRequest
            {
                email = email,
                password = password,
                data = new SupabaseSignUpMetadata { display_name = displayName.Trim() }
            });
            yield return Send("/auth/v1/signup", UnityWebRequest.kHttpVerbPOST, body, false, (json, error) =>
            {
                if (!string.IsNullOrEmpty(error)) { completed?.Invoke(null, error); return; }
                try
                {
                    var session = JsonUtility.FromJson<SupabaseAuthSession>(json);
                    if (session != null && !string.IsNullOrEmpty(session.access_token)) CurrentSession = session;
                    completed?.Invoke(session, null);
                }
                catch (Exception ex) { completed?.Invoke(null, ex.Message); }
            });
        }

        public IEnumerator RefreshSession(Action<SupabaseAuthSession, string> completed)
        {
            if (CurrentSession == null || string.IsNullOrEmpty(CurrentSession.refresh_token))
            {
                completed?.Invoke(null, "No refresh token is available in memory.");
                yield break;
            }

            var body = JsonUtility.ToJson(new SupabaseRefreshRequest { refresh_token = CurrentSession.refresh_token });
            yield return Send("/auth/v1/token?grant_type=refresh_token", UnityWebRequest.kHttpVerbPOST, body, false, (json, error) =>
            {
                if (!string.IsNullOrEmpty(error)) { completed?.Invoke(null, error); return; }
                try
                {
                    var refreshed = JsonUtility.FromJson<SupabaseAuthSession>(json);
                    if (refreshed == null || string.IsNullOrEmpty(refreshed.access_token))
                    {
                        CurrentSession = null;
                        completed?.Invoke(null, "Supabase Auth returned an invalid refreshed session.");
                        return;
                    }
                    if (refreshed.user == null && CurrentSession != null)
                        refreshed.user = CurrentSession.user;
                    CurrentSession = refreshed;
                    completed?.Invoke(CurrentSession, null);
                }
                catch (Exception ex) { completed?.Invoke(null, ex.Message); }
            });
        }

        public IEnumerator SignOut(Action<bool, string> completed)
        {
            if (!IsSignedIn) { completed?.Invoke(true, null); yield break; }
            yield return Send("/auth/v1/logout", UnityWebRequest.kHttpVerbPOST, "{}", true, (json, error) =>
            {
                if (string.IsNullOrEmpty(error)) CurrentSession = null;
                completed?.Invoke(string.IsNullOrEmpty(error), error);
            });
        }

        public IEnumerator FetchMyProfile(Action<ApexProfile, string> completed)
        {
            if (!RequireSession(completed)) yield break;
            string userId = UnityWebRequest.EscapeURL(CurrentSession.user.id);
            string path = "/rest/v1/apex_profiles?select=user_id,display_name,reputation,home_region&user_id=eq." + userId;
            yield return Send(path, UnityWebRequest.kHttpVerbGET, null, true, (json, error) =>
            {
                if (!string.IsNullOrEmpty(error)) { completed?.Invoke(null, error); return; }
                try
                {
                    var result = JsonUtility.FromJson<ApexProfileArray>("{\"items\":" + json + "}");
                    completed?.Invoke(result != null && result.items != null && result.items.Length > 0 ? result.items[0] : null, null);
                }
                catch (Exception ex) { completed?.Invoke(null, "Profile parsing failed: " + ex.Message); }
            });
        }

        public IEnumerator SaveMyProfile(string displayName, Action<ApexProfile, string> completed)
        {
            if (!IsSignedIn || CurrentSession.user == null)
            {
                completed?.Invoke(null, "Sign in before updating your profile.");
                yield break;
            }

            if (string.IsNullOrWhiteSpace(displayName) || displayName.Trim().Length < 3 || displayName.Trim().Length > 24)
            {
                completed?.Invoke(null, "Display name must contain 3–24 characters.");
                yield break;
            }

            string userId = UnityWebRequest.EscapeURL(CurrentSession.user.id);
            var body = JsonUtility.ToJson(new ApexProfileUpdate { display_name = displayName.Trim() });

            yield return Send("/rest/v1/apex_profiles?user_id=eq." + userId, "PATCH", body, true, (json, error) =>
            {
                if (!string.IsNullOrEmpty(error)) { completed?.Invoke(null, error); return; }
                try
                {
                    var result = JsonUtility.FromJson<ApexProfileArray>("{\"items\":" + json + "}");
                    if (result == null || result.items == null || result.items.Length == 0)
                    {
                        completed?.Invoke(null, "Profile row was not found for the signed-in user.");
                        return;
                    }
                    completed?.Invoke(result.items[0], null);
                }
                catch (Exception ex) { completed?.Invoke(null, "Profile parsing failed: " + ex.Message); }
            }, "return=representation");
        }

        public IEnumerator FetchRegions(Action<ApexRegion[], string> completed)
        {
            if (!IsSignedIn) { completed?.Invoke(null, "Sign in before requesting world regions."); yield break; }
            yield return Send("/rest/v1/apex_regions?select=region_id,display_name,enabled&order=region_id", UnityWebRequest.kHttpVerbGET, null, true, (json, error) =>
            {
                if (!string.IsNullOrEmpty(error)) { completed?.Invoke(null, error); return; }
                try
                {
                    var result = JsonUtility.FromJson<ApexRegionArray>("{\"items\":" + json + "}");
                    completed?.Invoke(result != null ? result.items : null, null);
                }
                catch (Exception ex) { completed?.Invoke(null, "Region parsing failed: " + ex.Message); }
            });
        }

        private bool RequireSession(Action<ApexProfile, string> completed)
        {
            if (IsSignedIn && CurrentSession.user != null) return true;
            completed?.Invoke(null, "Sign in before requesting player data.");
            return false;
        }

        private IEnumerator Send(string path, string method, string body, bool useUserToken, Action<string, string> completed, string prefer = null)
        {
            if (!SupabaseProjectSettings.IsConfigured)
            {
                completed?.Invoke(null, "Supabase project settings are not configured.");
                yield break;
            }

            using (var request = new UnityWebRequest(SupabaseProjectSettings.ProjectUrl + path, method))
            {
                request.downloadHandler = new DownloadHandlerBuffer();
                if (!string.IsNullOrEmpty(body))
                {
                    request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
                    request.SetRequestHeader("Content-Type", "application/json");
                }
                request.SetRequestHeader("apikey", SupabaseProjectSettings.PublishableKey);
                if (useUserToken && IsSignedIn)
                    request.SetRequestHeader("Authorization", "Bearer " + CurrentSession.access_token);
                if (!string.IsNullOrEmpty(prefer)) request.SetRequestHeader("Prefer", prefer);
                request.SetRequestHeader("Accept", "application/json");

                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                    completed?.Invoke(request.downloadHandler.text, null);
                else
                    completed?.Invoke(null, "Supabase request failed (" + request.responseCode + "): " + request.error + " | " + request.downloadHandler.text);
            }
        }
    }
}
