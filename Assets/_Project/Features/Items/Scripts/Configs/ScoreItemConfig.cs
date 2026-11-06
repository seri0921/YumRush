using UnityEngine;

[CreateAssetMenu(menuName = "Items/Score Item Config")]
public sealed class ScoreItemConfig : ScriptableObject
{
    [SerializeField, Min(0)] private int points = 100;
    public int Points => points;
    [SerializeField] private ScoreGainKind _gainKind;
    public ScoreGainKind GainKind => _gainKind;
}
