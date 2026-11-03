using UnityEngine;
using System.Collections.Generic;
public class TreasureManager : MonoBehaviour
{
    [SerializeField] private ScoreItem _treasurePrefab;
    [SerializeField] private Transform[] _treasureSpawnPoints;

    [Header("åÆê›íË")]
    [SerializeField] private TreasureKey[] _keyPrefabs;
    [SerializeField] private Transform[] _keySpawnPoints;    

    private readonly List<Transform> _candidatePool = new List<Transform>();
    private readonly List<Transform> _selectedKeyPoints = new List<Transform>();
    private readonly List<TreasureKey> _shuffledKeyPrefabs = new List<TreasureKey>();

    private void Start()
    {
        SpawnTreasureAndKeys();
    }

    private void SpawnTreasureAndKeys()
    {
        if (_treasurePrefab == null || _keyPrefabs == null || _keyPrefabs.Length == 0) return;
        if (_treasureSpawnPoints == null || _treasureSpawnPoints.Length == 0) return;

        Transform barrierPoint = _treasureSpawnPoints[Random.Range(0, _treasureSpawnPoints.Length)];
        ScoreItem spawnedItem = Instantiate(_treasurePrefab, barrierPoint.position, barrierPoint.rotation);

        if (!spawnedItem.TryGetComponent<TreasureItem>(out var barrier))
        {
            barrier = spawnedItem.GetComponentInChildren<TreasureItem>();
        }

        int keyCount = _keyPrefabs.Length;
        GetRandomSpawnPoints(_keySpawnPoints, keyCount, _selectedKeyPoints);

        _shuffledKeyPrefabs.Clear();
        _shuffledKeyPrefabs.AddRange(_keyPrefabs);
        ShuffleList(_shuffledKeyPrefabs);

        int spawnCount = Mathf.Min(_selectedKeyPoints.Count, _shuffledKeyPrefabs.Count);
        for (int i = 0; i < spawnCount; i++)
        {
            Transform point = _selectedKeyPoints[i];
            TreasureKey keyPrefab = _shuffledKeyPrefabs[i];

            TreasureKey spawnedKey = Instantiate(keyPrefab, point.position, point.rotation);
            spawnedKey.Setup(barrier);
        }
    }

    private void GetRandomSpawnPoints(Transform[] pool, int count, List<Transform> resultList)
    {
        resultList.Clear();
        if (pool == null || pool.Length == 0) return;

        _candidatePool.Clear();
        _candidatePool.AddRange(pool);

        int targetCount = Mathf.Min(count, _candidatePool.Count);
        for (int i = 0; i < targetCount; i++)
        {
            int randomIndex = Random.Range(0, _candidatePool.Count);
            resultList.Add(_candidatePool[randomIndex]);
            _candidatePool.RemoveAt(randomIndex);
        }
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

}
