using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class DDDD : MonoBehaviour
{
    [Header("チャンク設定")]
    public float chunkWidth = 12.0f;
    public float chunkHeight = 25.0f;
    public List<DElements> elementList;

    [Header("左右保証")]
    public float sideMargin = 12.5f;

    [Header("分岐確立")]
    [Range(0f, 1f)] public float branchChance = 0.4f;

    [Header("ランダムフリーアンカー設定")]
    public int freeAnchorCount = 3;
    public float freeAnchorMinY = 3.0f;
    public float freeAnchorXThreshold = 2.5f;
    public float freeAnchorMinYDiff = 3.0f;
    public int maxAdjustAttempts = 10;

    [Header("保存設定")]
    public string savePath = "Assets/_Project/Features/Stage/ShunkData/";

    private Transform currentChunkContainer;
    private const float AIR_DASH_DISTANCE = 3.24f;
    public void GenerateChunk()
    {
        ClearCurrentChunk();

        GameObject container = new GameObject($"VirtualChunk_{System.DateTime.Now:HHmmss}");
        container.transform.SetParent(this.transform);
        container.transform.localPosition = Vector3.zero;
        currentChunkContainer = container.transform;

        if (elementList == null || elementList.Count == 0) return;

        Vector2 currentPos = new Vector2(0f, 1.5f);
        DElements lastElem = SpawnElement(currentPos);
        if (lastElem == null) return;

        float currentY = currentPos.y;
        int safetyLoop = 0;

        float leftMinX = -chunkWidth / 2f + sideMargin;
        float leftMaxX = -0.5f;
        float rightMinX = 0.5f;
        float rightMaxX = chunkWidth / 2f - sideMargin;

        while (currentY < chunkHeight && safetyLoop < 50)
        {
            safetyLoop++;
            bool shouldBranch = (Random.value < branchChance) && (currentY < chunkHeight - 8.0f);

            if (shouldBranch)
            {
                Vector2 leftPos = currentPos;
                Vector2 rightPos = currentPos;
                DElements leftElem = lastElem;
                DElements rightElem = lastElem;
                int branchSteps = Random.Range(2, 4);
                for (int b = 0; b < branchSteps; b++)
                {
                    leftPos = CalculateNextPos(leftPos, leftElem, leftMinX, leftMaxX, -1f, out leftElem);
                    rightPos = CalculateNextPos(rightPos, rightElem, rightMinX, rightMaxX, 1f, out rightElem);
                }

                float dyMerge = Mathf.Max(
                    leftElem != null ? leftElem.maxReachY : 3.0f,
                    rightElem != null ? rightElem.maxReachY : 3.0f
                );
                currentY = Mathf.Max(leftPos.y, rightPos.y) + dyMerge;
                currentPos = new Vector2(Random.Range(-1.5f, 1.5f), currentY);
                lastElem = SpawnElement(currentPos);

            }
            else
            {
                float dir = (currentPos.x > 0) ? -1f : 1f;
                currentPos = CalculateNextPos(currentPos, lastElem, -chunkWidth / 2f + sideMargin, chunkWidth / 2f - sideMargin, dir, out lastElem);
                currentY = currentPos.y;
            }

            if (lastElem == null) break;
        }

        SpawnFreeAnchors();

    }

    private Vector2 CalculateNextPos(Vector2 currentPos, DElements lastElem, float minXBound, float maxXBound, float preferredDir, out DElements nextElem)
    {
        nextElem = GetRandomElementFromList(elementList);
        if (nextElem == null || lastElem == null) return currentPos;

        //ダッシュ回復地形かどうかで分岐させる。
        bool hasDash = (lastElem.elementType == ElementType.Ground ||
                        lastElem.elementType == ElementType.Enemy ||
                        lastElem.elementType == ElementType.Updraft);

        float dashBonus = hasDash ? AIR_DASH_DISTANCE : 0f;

        float minX = nextElem.minReachX;
        float maxX = nextElem.maxReachX + dashBonus;
        float minY = nextElem.minReachY;
        float maxY = nextElem.maxReachY + lastElem.extraUpwardBoost;

        //ダイブ滑空でのみ行ける地形も考慮に入れる
        if (lastElem.allowGlide && Random.value < 0.45f)
        {
            minY = lastElem.diveMinY;  //下方向へ落とし
            maxX = lastElem.diveMaxX + dashBonus; //急加速滑空
        }

        float dx = Random.Range(minX, maxX);
        float dy = Random.Range(minY, maxY);

        float center = (minXBound + maxXBound) / 2f;
        float dir = (currentPos.x > center) ? -1f : 1f;
        if (preferredDir != 0f && Random.value < 0.7f) dir = preferredDir;

        float nextX = Mathf.Clamp(currentPos.x + (dx * dir), minXBound, maxXBound);
        float nextY = currentPos.y + dy;

        Vector2 nextPos = new Vector2(nextX, nextY);
        nextElem = SpawnElement(nextPos, nextElem);

        return nextPos;
    }

    private void SpawnFreeAnchors()
    {
        if (elementList == null || freeAnchorCount <= 0) return;

        List<DElements> freeCandidates = elementList
            .Where(e => e != null && e.isFreeAnchor && e.prefab != null)
            .ToList();

        if (freeCandidates.Count == 0) return;

        for (int i = 0; i < freeAnchorCount; i++)
        {
            DElements chosenAnchor = GetRandomElementFromList(freeCandidates);
            if (chosenAnchor == null) continue;

            float x = Random.Range(-chunkWidth / 2f + sideMargin, chunkWidth / 2f - sideMargin);
            float y = Random.Range(freeAnchorMinY, chunkHeight - 2.0f);
            Vector2 candidatePos = new Vector2(x, y);

            for (int attempt = 0; attempt < maxAdjustAttempts; attempt++)
            {
                bool hasOverlap = false;

                foreach (Transform child in currentChunkContainer)
                {
                    float xDiff = Mathf.Abs(child.localPosition.x - candidatePos.x);
                    float yDiff = Mathf.Abs(child.localPosition.y - candidatePos.y);

                    if (xDiff < freeAnchorXThreshold && yDiff < freeAnchorMinYDiff)
                    {
                        hasOverlap = true;

                        float xDir = (candidatePos.x >= child.localPosition.x) ? 1.0f : -1.0f;
                        candidatePos.x = Mathf.Clamp(child.localPosition.x + (freeAnchorXThreshold * xDir), -chunkWidth / 2f + sideMargin, chunkWidth / 2f - sideMargin);

                        float yDir = (candidatePos.y >= child.localPosition.y) ? 1.0f : -1.0f;
                        candidatePos.y = Mathf.Clamp(child.localPosition.y + (freeAnchorMinYDiff * yDir), freeAnchorMinY, chunkHeight - 2.0f);

                        break;
                    }
                }

                if (!hasOverlap) break;
            }

            SpawnElement(candidatePos, chosenAnchor);
        }
    }

    private DElements SpawnElement(Vector2 pos, DElements targetElem = null)
    {
        DElements chosenElement = targetElem ?? GetRandomElementFromList(elementList);
#if UNITY_EDITOR
        GameObject spawnedObj = PrefabUtility.InstantiatePrefab(chosenElement.prefab, currentChunkContainer) as GameObject;
        if (spawnedObj != null)
        {
            spawnedObj.transform.localPosition = new Vector3(pos.x, pos.y, 0f);
        }
#else
        Instantiate(chosenElement.prefab, new Vector3(pos.x, pos.y, 0f), Quaternion.identity, currentChunkContainer);
#endif
        return chosenElement;
    }
    public void ClearCurrentChunk()
    {
        if (currentChunkContainer != null)
        {
            DestroyImmediate(currentChunkContainer.gameObject);
        }

        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }

    public void SaveCurrentChunkAsPrefab()
    {
#if UNITY_EDITOR

        Transform targetContainer = GetOrFindChunkContainer();

        if (currentChunkContainer == null || currentChunkContainer.childCount == 0)
        {
            Debug.LogError("ヌルエラー");
            return;
        }

        if (!System.IO.Directory.Exists(savePath))
        {
            System.IO.Directory.CreateDirectory(savePath);
        }

        string fileName = $"{savePath}Chunk_{System.DateTime.Now:yyyyMMdd_HHmmss}.prefab";
        PrefabUtility.SaveAsPrefabAsset(currentChunkContainer.gameObject, fileName);

        Debug.Log($"チャンクをprefab保存したゾイ: {fileName}");
        AssetDatabase.Refresh();
#endif
    }

    private Transform GetOrFindChunkContainer()
    {
        if (currentChunkContainer != null) return currentChunkContainer;

        // 子要素の中から "VirtualChunk" で始まるオブジェクトを検索して再割り当て
        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("VirtualChunk"))
            {
                currentChunkContainer = child;
                return currentChunkContainer;
            }
        }
        return null;
    }

    private DElements GetRandomElementFromList(List<DElements> list)
    {
        if (list == null || list.Count == 0) return null;

        int totalWeight = 0;
        foreach (var elem in list)
        {
            if (elem != null) totalWeight += elem.spawnWeight;
        }

        if (totalWeight <= 0) return list[0];

        int randomValue = Random.Range(0, totalWeight);
        int currentWeight = 0;

        foreach (var elem in list)
        {
            if (elem == null) continue;
            currentWeight += elem.spawnWeight;
            if (randomValue < currentWeight) return elem;
        }

        return list[0];
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = transform.position + new Vector3(0, chunkHeight / 2f, 0);
        Gizmos.DrawWireCube(center, new Vector3(chunkWidth, chunkHeight, 0));
 
    }

#if UNITY_EDITOR
[CustomEditor(typeof(DDDD))]
public class DDDDEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DDDD tool = (DDDD)target;

        EditorGUILayout.Space(15);
        EditorGUILayout.LabelField("チャンク生成操作", EditorStyles.boldLabel);

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 13,
            fontStyle = FontStyle.Bold,
            fixedHeight = 35
        };

        GUI.backgroundColor = new Color(0.4f, 0.8f, 1.0f);
        if (GUILayout.Button("地形リロード", buttonStyle))
        {
            tool.GenerateChunk();
        }

        GUI.backgroundColor = new Color(0.4f, 0.9f, 0.4f);
        if (GUILayout.Button("チャンク保存", buttonStyle))
        {
            tool.SaveCurrentChunkAsPrefab();
        }

        GUI.backgroundColor = new Color(1.0f, 0.4f, 0.4f);
        if (GUILayout.Button("リセット", buttonStyle))
        {
            tool.ClearCurrentChunk();
        }

        GUI.backgroundColor = Color.white;
    }
}
#endif
}