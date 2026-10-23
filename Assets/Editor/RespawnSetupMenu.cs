using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>Explicit, Undoable migration of a selected scene Chunk.</summary>
public static class RespawnSetupMenu
{
    [MenuItem("Tools/Respawn/Configure Selected Chunk")]
    private static void ConfigureSelectedChunk()
    {
        GameObject root = Selection.activeGameObject;
        if (Application.isPlaying || root == null || !root.scene.IsValid())
            throw new InvalidOperationException("Select a scene Chunk in Edit Mode.");
        if (root.GetComponentInParent<ChunkRespawnController>(true) != null ||
            root.GetComponentInChildren<ChunkRespawnController>(true) != null)
            throw new InvalidOperationException("This hierarchy already contains a respawn Chunk.");

        var targets = new List<GameObject>();
        foreach (ScoreItem item in root.GetComponentsInChildren<ScoreItem>(true))
            targets.Add(item.gameObject);
        foreach (EnemyBase enemy in root.GetComponentsInChildren<EnemyBase>(true))
            targets.Add(enemy.gameObject);
        if (targets.Count == 0)
            throw new InvalidOperationException("The selected Chunk contains no supported items or enemies.");

        // Validate everything before changing the scene. Overrides would be lost on respawn.
        foreach (GameObject target in targets)
        {
            if (target == root || !PrefabUtility.IsAnyPrefabInstanceRoot(target) ||
                PrefabUtility.GetCorrespondingObjectFromSource(target) == null)
                throw new InvalidOperationException($"{target.name}: use a prefab instance root below the Chunk.");
            foreach (GameObject other in targets)
                if (target != other && target.transform.IsChildOf(other.transform))
                    throw new InvalidOperationException("Nested respawn targets are not supported.");
            if (PrefabUtility.GetAddedComponents(target).Count > 0 ||
                PrefabUtility.GetRemovedComponents(target).Count > 0 ||
                PrefabUtility.GetAddedGameObjects(target).Count > 0 ||
                PrefabUtility.GetRemovedGameObjects(target).Count > 0)
                throw new InvalidOperationException($"{target.name}: save structural overrides to a prefab variant first.");
            var modifications = PrefabUtility.GetPropertyModifications(target);
            if (modifications == null)
                continue; // No overrides is a valid prefab instance.
            foreach (var modification in modifications)
            {
                // Unity returns outermost-instance modifications, including unrelated siblings.
                GameObject modifiedObject = modification.target as GameObject;
                if (modification.target is Component modifiedComponent)
                    modifiedObject = modifiedComponent.gameObject;
                Transform sourceRoot = PrefabUtility.GetCorrespondingObjectFromSource(target.transform);
                Transform originalRoot = PrefabUtility.GetCorrespondingObjectFromOriginalSource(target.transform);
                if (modifiedObject == null ||
                    (!modifiedObject.transform.IsChildOf(sourceRoot) && !modifiedObject.transform.IsChildOf(originalRoot)))
                    continue;
                bool rootTransform = modification.target == PrefabUtility.GetCorrespondingObjectFromSource(target.transform) ||
                    modification.target == PrefabUtility.GetCorrespondingObjectFromOriginalSource(target.transform);
                bool rootObject = modification.target == PrefabUtility.GetCorrespondingObjectFromSource(target) ||
                    modification.target == PrefabUtility.GetCorrespondingObjectFromOriginalSource(target);
                bool placement = rootTransform &&
                    (modification.propertyPath.StartsWith("m_LocalPosition.", StringComparison.Ordinal) ||
                     modification.propertyPath.StartsWith("m_LocalRotation.", StringComparison.Ordinal) ||
                     modification.propertyPath.StartsWith("m_LocalScale.", StringComparison.Ordinal) ||
                     modification.propertyPath.StartsWith("m_LocalEulerAnglesHint.", StringComparison.Ordinal) ||
                     modification.propertyPath == "m_RootOrder");
                bool identity = rootObject &&
                    (modification.propertyPath == "m_Name" || modification.propertyPath == "m_IsActive");
                if (!placement && !identity)
                    throw new InvalidOperationException($"{target.name}: save property overrides to a prefab variant first ({modification.propertyPath}).");
            }
        }

        const string folder = "Assets/_Project/Features/Respawn/Settings";
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets/_Project/Features/Respawn", "Settings");
        RespawnConfig individual = LoadOrCreate<RespawnConfig>(folder + "/DefaultIndividual.asset");
        ChunkRespawnConfig chunkConfig = LoadOrCreate<ChunkRespawnConfig>(folder + "/DefaultChunk.asset");

        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Configure respawn Chunk");
        try
        {
            RespawnManager manager = UnityEngine.Object.FindFirstObjectByType<RespawnManager>();
            if (manager == null)
            {
                var managerObject = new GameObject("RespawnManager");
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(managerObject, root.scene);
                Undo.RegisterCreatedObjectUndo(managerObject, "Create RespawnManager");
                manager = Undo.AddComponent<RespawnManager>(managerObject);
            }
            foreach (GameObject target in targets)
            {
                var slot = new GameObject(target.name + " SpawnPoint");
                Undo.RegisterCreatedObjectUndo(slot, "Create SpawnPoint");
                slot.transform.SetParent(target.transform.parent, false);
                slot.transform.localPosition = target.transform.localPosition;
                slot.transform.localRotation = target.transform.localRotation;
                slot.transform.localScale = target.transform.localScale;
                slot.transform.SetSiblingIndex(target.transform.GetSiblingIndex());
                slot.SetActive(target.activeSelf);
                var point = Undo.AddComponent<SpawnPoint>(slot);
                point.Configure(PrefabUtility.GetCorrespondingObjectFromSource(target), individual);
                EditorUtility.SetDirty(point);
                Undo.DestroyObjectImmediate(target);
            }
            var chunk = Undo.AddComponent<ChunkRespawnController>(root);
            chunk.Configure(manager, chunkConfig);
            EditorUtility.SetDirty(chunk);
            Undo.CollapseUndoOperations(group);
            Selection.activeGameObject = root;
        }
        catch
        {
            Undo.RevertAllDownToGroup(group);
            throw;
        }
        AssetDatabase.SaveAssets();
    }

    private static T LoadOrCreate<T>(string path) where T : ScriptableObject
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset != null)
            return asset;
        asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }
}
