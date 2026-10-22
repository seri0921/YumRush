using UnityEngine;

public enum ElementType
{
    Ground,  //通常床
    Enemy, //敵
    Updraft, //桐生
    other //何でも
}

[CreateAssetMenu(fileName = "DElements", menuName = "ScriptableObject/DElements")]
public class DElements : ScriptableObject
{
    public string elementName = "New Element";
    public GameObject prefab;
    public ElementType elementType = ElementType.Ground;

    [Header("基本距離")]
    public float minReachX = 2.0f;
    public float maxReachX = 5.0f;
    public float minReachY = 2.0f;
    public float maxReachY = 5.0f;

    [Header("上昇気流などのブースト値チェック")]
    public float extraUpwardBoost = 0f;

    [Header("※試験的採用、このprefabからダイブして次に向かうルート")]
    public bool allowGlide = false;
    public float diveMinY = -3.0f; //どこまで下げるのを許可するか
    public float diveMaxX = 5.0f; //引き起こしでどこまで行けるか

    [Header("生成ウェイト")]
    [Range(1, 100)] public int spawnWeight = 10;

    [Header("フリーアンカーとして使う？")]
    public bool isFreeAnchor = false;

}