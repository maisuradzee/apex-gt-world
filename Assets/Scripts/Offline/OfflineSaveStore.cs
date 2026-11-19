using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace ApexGTWorld.Offline
{
    /// <summary>
    /// Versioned JSON save store under Application.persistentDataPath.
    /// A previous valid file is kept as a recovery backup.
    /// </summary>
    public static class OfflineSaveStore
    {
        private const string SaveFileName = "apex_offline_profile.json";
        private const string BackupFileName = "apex_offline_profile.bak";
        private const string TemporaryFileName = "apex_offline_profile.tmp";

        public static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

        public static bool TryLoad(out OfflinePlayerProfile profile, out string message)
        {
            profile = null;
            message = null;

            try
            {
                string directory = Application.persistentDataPath;
                Directory.CreateDirectory(directory);
                string primary = Path.Combine(directory, SaveFileName);
                string backup = Path.Combine(directory, BackupFileName);

                if (File.Exists(primary) && TryRead(primary, out profile, out string primaryError))
                    return true;

                if (File.Exists(backup) && TryRead(backup, out profile, out string backupError))
                {
                    message = "Recovered the previous local save.";
                    TrySave(profile, out _);
                    return true;
                }

                if (File.Exists(primary) || File.Exists(backup))
                {
                    message = "No valid local save was found. A new offline profile was created.";
                }
                else
                {
                    message = "New offline profile created.";
                }

                profile = new OfflinePlayerProfile();
                profile.Normalize();
                if (!TrySave(profile, out string saveError))
                {
                    message = message + " Initial save failed: " + saveError;
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                message = "Local save could not be loaded: " + ex.Message;
                profile = null;
                return false;
            }
        }

        public static bool TrySave(OfflinePlayerProfile profile, out string error)
        {
            error = null;
            if (profile == null)
            {
                error = "Cannot save an empty offline profile.";
                return false;
            }

            string temporary = null;
            try
            {
                profile.Normalize();
                string directory = Application.persistentDataPath;
                Directory.CreateDirectory(directory);

                string primary = Path.Combine(directory, SaveFileName);
                string backup = Path.Combine(directory, BackupFileName);
                temporary = Path.Combine(directory, TemporaryFileName);

                profile.lastSavedUtc = DateTime.UtcNow.ToString("O");
                string json = JsonUtility.ToJson(profile, true);
                byte[] bytes = new UTF8Encoding(false).GetBytes(json);

                using (var stream = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush(true);
                }

                if (File.Exists(primary))
                {
                    File.Copy(primary, backup, true);
                    File.Delete(primary);
                }

                File.Move(temporary, primary);
                return true;
            }
            catch (Exception ex)
            {
                error = "Local save failed: " + ex.Message;
                try
                {
                    string primary = Path.Combine(Application.persistentDataPath, SaveFileName);
                    string backup = Path.Combine(Application.persistentDataPath, BackupFileName);
                    if (!File.Exists(primary) && File.Exists(backup))
                        File.Copy(backup, primary, true);
                }
                catch (Exception recoveryError)
                {
                    Debug.LogError("Offline save recovery failed: " + recoveryError.Message);
                }
                return false;
            }
            finally
            {
                if (!string.IsNullOrEmpty(temporary) && File.Exists(temporary))
                {
                    try { File.Delete(temporary); }
                    catch (Exception cleanupError) { Debug.LogWarning("Could not remove temporary save: " + cleanupError.Message); }
                }
            }
        }

        private static bool TryRead(string path, out OfflinePlayerProfile profile, out string error)
        {
            profile = null;
            error = null;
            try
            {
                string json = File.ReadAllText(path, Encoding.UTF8);
                profile = JsonUtility.FromJson<OfflinePlayerProfile>(json);
                if (profile == null || profile.schemaVersion != 1)
                    throw new InvalidDataException("Unsupported or invalid local save schema.");
                profile.Normalize();
                return true;
            }
            catch (Exception ex)
            {
                profile = null;
                error = ex.Message;
                return false;
            }
        }
    }
}
