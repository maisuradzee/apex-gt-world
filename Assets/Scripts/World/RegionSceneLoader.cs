using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ApexGTWorld.World
{
    /// <summary>
    /// Local additive scene loader only. It does not perform online handoff;
    /// online transfer must be orchestrated by the authoritative world directory.
    /// </summary>
    public sealed class RegionSceneLoader : MonoBehaviour
    {
        public bool IsLoading { get; private set; }

        public void LoadRegion(string sceneName, Action<bool> completed = null)
        {
            if (string.IsNullOrWhiteSpace(sceneName) || IsLoading)
            {
                completed?.Invoke(false);
                return;
            }
            StartCoroutine(LoadRoutine(sceneName, completed));
        }

        private System.Collections.IEnumerator LoadRoutine(string sceneName, Action<bool> completed)
        {
            IsLoading = true;
            AsyncOperation operation;
            try { operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive); }
            catch (Exception ex)
            {
                Debug.LogException(ex, this);
                IsLoading = false;
                completed?.Invoke(false);
                yield break;
            }
            if (operation == null)
            {
                IsLoading = false;
                completed?.Invoke(false);
                yield break;
            }
            while (!operation.isDone) yield return null;
            IsLoading = false;
            completed?.Invoke(true);
        }
    }
}
