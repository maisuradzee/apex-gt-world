using System.Collections.Generic;
using UnityEngine;

namespace ApexGTWorld.Offline
{
    /// <summary>Builds a compact four-gate practice circuit and local AI grid.</summary>
    public sealed class OfflineRaceCourse : MonoBehaviour
    {
        [SerializeField] private int aiOpponentCount = 3;
        [SerializeField] private OfflineRaceDefinition defaultEvent = new OfflineRaceDefinition();

        private GameObject playerRoot;
        private OfflineRaceCheckpoint[] checkpoints;
        private readonly List<GameObject> aiRoots = new List<GameObject>();
        private readonly List<OfflineAiDriver> aiDrivers = new List<OfflineAiDriver>();
        private readonly List<OfflineRaceParticipant> participants = new List<OfflineRaceParticipant>();
        private OfflineRaceSession currentSession;

        public OfflineRaceSession CurrentSession => currentSession;
        public bool PlayerFinished => currentSession != null && currentSession.IsPlayerFinished;
        public float PlayerFinishTime => currentSession == null ? 0f : currentSession.PlayerFinishTimeSeconds;
        public OfflineRaceDefinition DefaultEvent => defaultEvent;

        public void Configure(GameObject player)
        {
            playerRoot = player;
            if (playerRoot == null) return;
            BuildCheckpoints();
            BuildAiGrid();
        }

        public bool BeginRace(OfflineRaceDefinition definition)
        {
            if (playerRoot == null || checkpoints == null || checkpoints.Length == 0) return false;
            if (definition == null) definition = defaultEvent;
            if (definition.checkpointCount != checkpoints.Length) return false;

            StopRace();
            playerRoot.transform.SetPositionAndRotation(new Vector3(0f, 0.6f, -5f), Quaternion.identity);
            Rigidbody playerBody = playerRoot.GetComponent<Rigidbody>();
            if (playerBody != null)
            {
                playerBody.velocity = Vector3.zero;
                playerBody.angularVelocity = Vector3.zero;
            }

            participants.Clear();
            OfflineRaceParticipant playerParticipant = playerRoot.GetComponent<OfflineRaceParticipant>();
            if (playerParticipant == null) playerParticipant = playerRoot.AddComponent<OfflineRaceParticipant>();
            playerParticipant.Configure(null, OfflineGameManager.Instance != null
                ? OfflineGameManager.Instance.Profile.displayName : "Player", true);
            participants.Add(playerParticipant);

            for (int i = 0; i < aiRoots.Count; i++)
            {
                GameObject root = aiRoots[i];
                OfflineRaceParticipant participant = root.GetComponent<OfflineRaceParticipant>();
                if (participant == null) participant = root.AddComponent<OfflineRaceParticipant>();
                participant.Configure(null, "AI " + (i + 1), false);
                participants.Add(participant);
            }

            currentSession = new OfflineRaceSession(definition, participants);
            for (int i = 0; i < aiDrivers.Count; i++)
            {
                aiRoots[i].transform.SetPositionAndRotation(
                    new Vector3((i - 1) * 2.2f, 0.7f, -8f - i * 3f), Quaternion.identity);
                aiRoots[i].SetActive(true);
                Rigidbody aiBody = aiRoots[i].GetComponent<Rigidbody>();
                if (aiBody != null)
                {
                    aiBody.velocity = Vector3.zero;
                    aiBody.angularVelocity = Vector3.zero;
                }
                aiDrivers[i].BeginDriving();
            }
            return true;
        }

        public void StopRace()
        {
            for (int i = 0; i < participants.Count; i++)
                if (participants[i] != null) participants[i].DetachSession();
            participants.Clear();
            currentSession = null;
            for (int i = 0; i < aiDrivers.Count; i++)
            {
                if (aiDrivers[i] != null) aiDrivers[i].StopDriving();
                if (i < aiRoots.Count && aiRoots[i] != null) aiRoots[i].SetActive(false);
            }
        }

        private void BuildCheckpoints()
        {
            if (checkpoints != null) return;
            Vector3[] positions =
            {
                new Vector3(0f, 2f, 24f),
                new Vector3(24f, 2f, 0f),
                new Vector3(0f, 2f, -24f),
                new Vector3(-24f, 2f, 0f)
            };
            float[] rotations = { 0f, 90f, 180f, 270f };
            checkpoints = new OfflineRaceCheckpoint[positions.Length];

            for (int i = 0; i < positions.Length; i++)
            {
                GameObject gate = GameObject.CreatePrimitive(PrimitiveType.Cube);
                gate.name = "Offline Checkpoint " + (i + 1);
                gate.transform.SetParent(transform, true);
                gate.transform.position = positions[i];
                gate.transform.rotation = Quaternion.Euler(0f, rotations[i], 0f);
                gate.transform.localScale = new Vector3(10f, 4f, 0.25f);

                BoxCollider collider = gate.GetComponent<BoxCollider>();
                collider.isTrigger = true;
                OfflineRaceCheckpoint checkpoint = gate.AddComponent<OfflineRaceCheckpoint>();
                checkpoint.Configure(i);
                Renderer renderer = gate.GetComponent<Renderer>();
                renderer.material.color = i == 0
                    ? new Color(0.1f, 0.8f, 0.35f, 0.85f)
                    : new Color(0.1f, 0.55f, 0.9f, 0.75f);
                checkpoints[i] = checkpoint;
            }
        }

        private void BuildAiGrid()
        {
            if (aiRoots.Count > 0) return;
            aiOpponentCount = Mathf.Clamp(aiOpponentCount, 1, 5);
            Transform[] route = new Transform[checkpoints.Length];
            for (int i = 0; i < checkpoints.Length; i++) route[i] = checkpoints[i].transform;

            for (int i = 0; i < aiOpponentCount; i++)
            {
                GameObject ai = new GameObject("Offline AI Driver " + (i + 1));
                ai.transform.SetParent(transform, true);
                ai.transform.position = new Vector3((i - 1) * 2.2f, 0.7f, -5f - i * 3f);

                GameObject bodyVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bodyVisual.name = "AI Vehicle Body";
                bodyVisual.transform.SetParent(ai.transform, false);
                bodyVisual.transform.localScale = new Vector3(1.8f, 0.55f, 3.6f);
                bodyVisual.transform.localPosition = new Vector3(0f, 0.35f, 0f);
                Destroy(bodyVisual.GetComponent<Collider>());

                BoxCollider hitbox = ai.AddComponent<BoxCollider>();
                hitbox.size = new Vector3(1.8f, 1.1f, 3.6f);
                Rigidbody rb = ai.AddComponent<Rigidbody>();
                rb.mass = 1200f;
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                OfflineAiDriver driver = ai.AddComponent<OfflineAiDriver>();
                driver.Configure(route, 13f + i * 1.4f, (i - 1) * 1.2f);
                ai.SetActive(false);

                aiRoots.Add(ai);
                aiDrivers.Add(driver);
            }
        }
    }
}
