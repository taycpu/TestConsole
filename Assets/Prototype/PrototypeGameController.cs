using TMPro;
using UnityEngine;

namespace Prototype
{
    public class PrototypeGameController : MonoBehaviour
    {
        public static PrototypeGameController Instance { get; private set; }

        [SerializeField] private PrototypePlayer player;
        [SerializeField] private PrototypeCollectible collectiblePrefab;
        [SerializeField] private PrototypeHazard hazardPrefab;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private Vector2 arenaSize = new Vector2(14f, 8f);
        [SerializeField] private int startingHazards = 3;

        private readonly System.Collections.Generic.List<PrototypeHazard> _hazards = new System.Collections.Generic.List<PrototypeHazard>();
        private PrototypeCollectible _collectible;
        private float _nextSpawnTime;
        private int _score;
        private int _hazardHits;

        public Vector2 ArenaExtents => arenaSize * 0.5f;
        public int Score => _score;
        public int HazardHits => _hazardHits;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            ResetPrototype();
        }

        private void Update()
        {
            if (_collectible == null && Time.time >= _nextSpawnTime)
                SpawnCollectible();

            UpdateStatus();
        }

        public void ResetPrototype()
        {
            _score = 0;
            _hazardHits = 0;
            _nextSpawnTime = Time.time;

            if (player != null)
                player.ResetPlayer(Vector3.zero);

            if (_collectible != null)
                Destroy(_collectible.gameObject);
            _collectible = null;

            foreach (var hazard in _hazards)
                if (hazard != null) Destroy(hazard.gameObject);
            _hazards.Clear();

            for (int i = 0; i < startingHazards; i++)
                SpawnHazard(i);

            Debug.Log("[Prototype] Reset collect-and-avoid playground.");
        }

        public void Collect(PrototypeCollectible collectible)
        {
            if (collectible != _collectible)
                return;

            _score++;
            Debug.Log($"[Prototype] Collected dot #{_score}.");
            Destroy(_collectible.gameObject);
            _collectible = null;
            _nextSpawnTime = Time.time + PrototypeDebugOptions.Current.PrototypeSpawnInterval;
        }

        public void HitHazard()
        {
            if (PrototypeDebugOptions.Current.PrototypeInvulnerable)
            {
                Debug.Log("[Prototype] Hazard ignored because invulnerability is enabled.");
                return;
            }

            _hazardHits++;
            Debug.LogWarning($"[Prototype] Hazard hit #{_hazardHits}. Score remains {_score}.");
        }

        public void EmitLogBurst()
        {
            for (int i = 1; i <= 10; i++)
                Debug.Log($"[Prototype] Burst log {i}/10 at score {_score}.");
        }

        private void SpawnCollectible()
        {
            if (collectiblePrefab == null)
                return;

            _collectible = Instantiate(collectiblePrefab, RandomPointInsideArena(), Quaternion.identity, transform);
            _collectible.gameObject.SetActive(true);
            Debug.Log("[Prototype] Spawned collectible dot.");
        }

        private void SpawnHazard(int index)
        {
            if (hazardPrefab == null)
                return;

            var hazard = Instantiate(hazardPrefab, RandomPointInsideArena(), Quaternion.identity, transform);
            hazard.name = $"Hazard {index + 1}";
            hazard.gameObject.SetActive(true);
            hazard.SetDirection(Random.insideUnitCircle.normalized);
            _hazards.Add(hazard);
        }

        private Vector3 RandomPointInsideArena()
        {
            Vector2 extents = ArenaExtents - new Vector2(0.75f, 0.75f);
            return new Vector3(Random.Range(-extents.x, extents.x), Random.Range(-extents.y, extents.y), 0f);
        }

        private void UpdateStatus()
        {
            if (statusText == null)
                return;

            statusText.text =
                $"Score: {_score}\n" +
                $"Hazard Hits: {_hazardHits}\n" +
                "Move: WASD / Arrows\n" +
                "Triple-tap corner button for Debug Panel";
        }
    }
}
