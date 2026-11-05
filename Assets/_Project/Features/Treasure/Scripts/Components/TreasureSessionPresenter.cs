using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Treasure
{
    [DisallowMultipleComponent]
    public sealed class TreasureSessionPresenter : MonoBehaviour
    {
        [SerializeField, Min(4)] private int _requiredKeys = 4;
        [SerializeField] private TreasureKeyPresenter[] _biomeKeyPrefabs;
        [SerializeField] private TreasureChestPresenter _chestPrefab;
        [SerializeField] private ScoreItem _rewardPrefab;
        [SerializeField] private TreasureSpawnPoint[] _spawnPoints;
        private TreasureProgressLogic _progress;
        private TreasureChestPresenter _chest;
        private readonly List<TreasureKeyPresenter> _keys = new List<TreasureKeyPresenter>();
        public IReadOnlyList<TreasureKeyPresenter> Keys => _keys;
        public TreasureChestPresenter Chest => _chest;
        public bool CanOpen => _progress != null && _progress.CanOpen;
        public bool IsOpened => _progress != null && _progress.IsOpened;
        public int CollectedKeys => _progress == null ? 0 : _progress.CollectedKeys;

        private void Start()
        {
            if (_biomeKeyPrefabs == null || _biomeKeyPrefabs.Length != 4 || _chestPrefab == null || _rewardPrefab == null || _spawnPoints == null)
                throw new InvalidOperationException("Treasure requires four biome key prefabs, a chest, a reward and spawn points.");
            foreach (var prefab in _biomeKeyPrefabs)
                if (prefab == null) throw new InvalidOperationException("A biome key prefab is missing.");
            var points = new List<TreasureSpawnPoint>();
            var positions = new HashSet<Vector3>();
            foreach (var point in _spawnPoints)
            {
                if (point == null || !point.gameObject.activeInHierarchy || !positions.Add(point.transform.position))
                    throw new InvalidOperationException("Treasure points must be active, assigned and at distinct positions.");
                points.Add(point);
            }
            var random = new System.Random();
            var biomes = new int[points.Count];
            var weights = new double[points.Count];
            var chestPoints = new bool[points.Count];
            for (int index = 0; index < points.Count; index++)
            {
                biomes[index] = points[index].BiomeIndex;
                weights[index] = points[index].KeyWeight;
                chestPoints[index] = points[index].CanSpawnChest;
            }
            var placement = new TreasurePlacementLogic();
            int chestIndex = placement.SelectChest(biomes, weights, chestPoints, 4, _requiredKeys, random);
            var chestPoint = points[chestIndex];
            // Exclude the chest from key selection without changing the scene's candidate list.
            weights[chestIndex] = 0;
            int[] selected = placement.SelectKeys(biomes, weights, 4, _requiredKeys, random);
            _progress = new TreasureProgressLogic(_requiredKeys);
            _chest = Instantiate(_chestPrefab, chestPoint.transform.position, Quaternion.identity, transform);
            _chest.Initialize(this);
            for (int keyId = 0; keyId < selected.Length; keyId++)
            {
                var point = points[selected[keyId]];
                var key = Instantiate(_biomeKeyPrefabs[point.BiomeIndex], point.transform.position, Quaternion.identity, transform);
                key.Initialize(this, keyId);
                _keys.Add(key);
            }
        }

        public bool CollectKey(int keyId)
        {
            if (!_progress.TryCollect(keyId)) return false;
            _chest.Refresh();
            return true;
        }

        public void OpenChest()
        {
            if (!_progress.TryOpen()) return;
            _chest.Refresh();
            Instantiate(_rewardPrefab, _chest.transform.position + Vector3.up * 1.5f, Quaternion.identity, transform);
        }
    }
}
