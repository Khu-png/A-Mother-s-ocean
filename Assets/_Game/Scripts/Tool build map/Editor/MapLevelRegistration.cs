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
}
