using UnityEngine;

[CreateAssetMenu(menuName = "Enemies/Enemy Score Config")]
public sealed class EnemyScoreConfig : ScriptableObject
{
    [SerializeField, Min(0)] private int _points = 100;
    public int Points => _points;
}
