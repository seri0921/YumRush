using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ProjectTemplateGenerator
{
    // ゲームごとに変更可能
    private const string Root = "Assets/_Project";

    private static readonly string[] Folders =
    {
        // Art
        "Art/Sprites",
        "Art/Materials",
        "Art/Animations",
        "Art/Fonts",

        // Audio
        "Audio/BGM",
        "Audio/SFX",

        // Features
        "Features/Player",
        "Features/Enemy",
        "Features/Combat",
        "Features/Stage",
        "Features/UI",

        // Core
        "Core/Events",
        "Core/Utilities",
        "Core/Extensions",

        // Systems
        "Systems/Save",
        "Systems/Audio",
        "Systems/SceneManagement",

        // Others
        "Scenes",
        "Settings",
        "Tests",
        "Sandbox"
    };

    private static readonly string[] Scenes =
    {
        "Title",
        "Game",
        "Result"
    };

    [MenuItem(
        "Tools/Project Template/Create Folder Structure")]
    private static void Generate()
    {
        try
        {
            // 1. ゲーム用フォルダ
            CreateFolderRecursive(Root);

            foreach (string folder in Folders)
            {
                CreateFolderRecursive($"{Root}/{folder}");
            }

            // 2. 外部アセット用フォルダ
            CreateFolderRecursive("Assets/ThirdParty");

            // 3. シーン作成
            CreateScenes();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                $"Project template generated: {Root}");
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    private static void CreateFolderRecursive(string path)
    {
        string[] parts = path.Split('/');
        string current = parts[0];

        for (int i = 1; i < parts.Length; i++)
        {
            string next = $"{current}/{parts[i]}";

            if (!AssetDatabase.IsValidFolder(next))
            {
                string guid = AssetDatabase.CreateFolder(
                    current,
                    parts[i]
                );

                if (string.IsNullOrEmpty(guid))
                {
                    throw new InvalidOperationException(
                        $"Folder creation failed: {next}");
                }
            }

            current = next;
        }
    }

    private static void CreateScenes()
    {
        foreach (string sceneName in Scenes)
        {
            string path =
                $"{Root}/Scenes/{sceneName}.unity";

            // 既存シーンは変更しない
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path)
                != null)
            {
                continue;
            }

            // 既存シーンを維持して一時的に作成
            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Additive
            );

            try
            {
                bool saved = EditorSceneManager.SaveScene(
                    scene,
                    path
                );

                if (!saved)
                {
                    throw new InvalidOperationException(
                        $"Scene save failed: {path}");
                }
            }
            finally
            {
                if (scene.IsValid() && scene.isLoaded)
                {
                    EditorSceneManager.CloseScene(
                        scene,
                        true
                    );
                }
            }
        }
    }
}
