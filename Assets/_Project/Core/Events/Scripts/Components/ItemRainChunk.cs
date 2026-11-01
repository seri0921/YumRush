using UnityEngine;
using System.Collections.Generic;

public class ItemRainChunk : MonoBehaviour
{
    [Header("生成アイテム")]
    [SerializeField] private GameObject itemPrefab;

    [Header("タイマー設定")]
    [SerializeField, Tooltip("最小クールタイム（秒）")] private float minInterval = 0.4f;
    [SerializeField, Tooltip("最大クールタイム（秒）")] private float maxInterval = 1.2f;
    [SerializeField, Tooltip("スポーン位置の左右ランダムブレ幅")] private float randomXRange = 1.5f;

    [Header("スポーン位置（Transformをセット）")]
    [SerializeField] private Transform[] spawnPoints;

    private float[] cooldowns;

    private void OnEnable()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            List<Transform> points = new List<Transform>();
            foreach (Transform child in transform)
            {
                points.Add(child);
            }
            spawnPoints = points.ToArray();
        }
        if (spawnPoints == null || spawnPoints.Length == 0) return;
        cooldowns = new float[spawnPoints.Length];
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            cooldowns[i] = Random.Range(0f, maxInterval);
        }
    }

    private void Update()
    {
        if (spawnPoints == null || cooldowns == null) return;

        float deltaTime = Time.deltaTime;
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] == null) continue;

            cooldowns[i] -= deltaTime;
            if (cooldowns[i] <= 0f)
            {
                SpawnItemAt(spawnPoints[i]);
                cooldowns[i] = Random.Range(minInterval, maxInterval);
            }
        }
    }

    private void SpawnItemAt(Transform point)
    {
        if (itemPrefab == null) return;
        float offsetX = Random.Range(-randomXRange, randomXRange);
        Vector3 spawnPosition = point.position + new Vector3(offsetX, 0f, 0f);

        Instantiate(itemPrefab, spawnPosition, point.rotation, transform);
    }
}
