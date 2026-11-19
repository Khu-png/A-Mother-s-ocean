using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class MapLevelRegistration
{
    public static void Validate(LevelManager manager, int levelNumber, string path)
    {
        if (File.Exists(path) || AssetDatabase.LoadMainAssetAtPath(path) != null)
            throw new UnityException("Level đã tồn tại");
        ValidateSlot(manager, levelNumber);
    }

    // The manager must be assigned and its slot for this level number must be empty.
    public static void ValidateSlot(LevelManager manager, int levelNumber)
    {
        if (manager == null)
            throw new UnityException("Hãy gán Level Manager trước khi tạo level");

        SerializedObject serializedManager = new SerializedObject(manager);
        SerializedProperty levels = serializedManager.FindProperty("levels");
        int index = levelNumber - 1;
        if (index < levels.arraySize && levels.GetArrayElementAtIndex(index).objectReferenceValue != null)
            throw new UnityException("Level đã tồn tại");
    }

    public static void Assign(LevelManager manager, int levelNumber, LevelData level)
    {
        SerializedObject serializedManager = new SerializedObject(manager);
        SerializedProperty levels = serializedManager.FindProperty("levels");
        int previousSize = levels.arraySize;
        if (previousSize < levelNumber)
        {
            levels.arraySize = levelNumber;
            for (int i = previousSize; i < levelNumber; i++)
                levels.GetArrayElementAtIndex(i).objectReferenceValue = null;
        }

        levels.GetArrayElementAtIndex(levelNumber - 1).objectReferenceValue = level;
        serializedManager.ApplyModifiedProperties();
        EditorUtility.SetDirty(manager);
        if (PrefabUtility.IsPartOfPrefabInstance(manager))
            PrefabUtility.RecordPrefabInstancePropertyModifications(manager);
        if (manager.gameObject.scene.IsValid())
            EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);
    }

    public static string Create(LevelManager manager, int requestedLevelNumber,
        string[] rows, int timeLimitSeconds,
        out LevelData asset)
    {
        int levelNumber = Mathf.Max(1, requestedLevelNumber);
        string path = $"Assets/_Game/ScriptableObject/Map/Level_{levelNumber}.asset";
        Validate(manager, levelNumber, path);
        EnsureLevelDataFolder();

        asset = ScriptableObject.CreateInstance<LevelData>();
        asset.SetMapRows(rows);
        asset.SetTimeLimit(timeLimitSeconds);
        AssetDatabase.CreateAsset(asset, path);
        Assign(manager, levelNumber, asset);
        RegisterDifficulty(manager, levelNumber);
        AssetDatabase.SaveAssets();
        return $"Đã tạo Level {levelNumber}, gán vào Level Manager và thêm vào Level Difficulty Config. Hãy lưu scene/prefab.";
    }

    // Registers a level asset that already exists: manager slot plus difficulty row.
    public static string Register(LevelManager manager, int levelNumber, LevelData level)
    {
        ValidateSlot(manager, levelNumber);
        Assign(manager, levelNumber, level);
        RegisterDifficulty(manager, levelNumber);
        AssetDatabase.SaveAssets();
        return $"Đã lưu {AssetDatabase.GetAssetPath(level)}, gán làm Level {levelNumber} và thêm vào Level Difficulty Config. Hãy lưu scene/prefab.";
    }

    // Adds a row for the new level to the difficulty table, set to what the table
    // already resolves for it (rules or default), so it can be edited there right away.
    private static void RegisterDifficulty(LevelManager manager, int levelNumber)
    {
        SerializedObject serializedManager = new SerializedObject(manager);
        LevelDifficultyConfig config = (LevelDifficultyConfig)serializedManager
            .FindProperty("difficultyConfig").objectReferenceValue;
        SerializedObject serializedConfig = new SerializedObject(config);
        SerializedProperty rows = serializedConfig.FindProperty("levels");
        for (int i = 0; i < rows.arraySize; i++)
        {
            if (rows.GetArrayElementAtIndex(i).FindPropertyRelative("level").intValue == levelNumber) return;
        }
        rows.arraySize++;
        SerializedProperty row = rows.GetArrayElementAtIndex(rows.arraySize - 1);
        row.FindPropertyRelative("level").intValue = levelNumber;
        row.FindPropertyRelative("difficulty").enumValueIndex = (int)config.GetDifficulty(levelNumber);
        serializedConfig.ApplyModifiedProperties();
        EditorUtility.SetDirty(config);
    }

    private static void EnsureLevelDataFolder()
    {
        const string root = "Assets/_Game/ScriptableObject";
        const string map = root + "/Map";
        if (!AssetDatabase.IsValidFolder(root))
            AssetDatabase.CreateFolder("Assets/_Game", "ScriptableObject");
        if (!AssetDatabase.IsValidFolder(map))
            AssetDatabase.CreateFolder(root, "Map");
    }
}
