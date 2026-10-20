using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class HBRuleSmokeCheck
{
    private const string REPORT_PATH = "Temp/HBRuleSmokeCheck.txt";
    [MenuItem("Tools/A Mother's Ocean/Validate Coding Refactor")]
    public static void Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        try
        {
            CheckPathReuse();
            CheckMapOwnership();
            CheckMaterialReference();
            CheckMapBuilderLimits();
            File.WriteAllText(REPORT_PATH,
                "PASS: Path reuse, map ownership, material, shader and map size limits.");
            GameDiagnostics.Log("Coding refactor checks passed.");
        }
        catch (Exception exception)
        {
            File.WriteAllText(REPORT_PATH, "FAIL: " + exception);
            GameDiagnostics.LogException(exception);
        }
    }

    static void CheckPathReuse()
    {
        GameObject owner = new GameObject("PathPoolCheck");
        try
        {
            PathTrail trail = new PathTrail(owner.transform, 1f, Color.white, 2);
            trail.Mark(Vector2Int.zero, Vector3.zero);
            trail.Mark(Vector2Int.right, Vector3.right);
            Require(trail.HasMark(Vector2Int.zero) && trail.HasMark(Vector2Int.right), "Initial marks missing.");
            trail.Remove(Vector2Int.zero);
            trail.Mark(Vector2Int.up, Vector3.up);
            Require(!trail.HasMark(Vector2Int.zero) && trail.HasMark(Vector2Int.up), "Removed mark was not reused.");
            trail.Clear();
            trail.Mark(Vector2Int.zero, Vector3.zero);
            trail.Mark(Vector2Int.right, Vector3.right);
            Require(trail.HasMark(Vector2Int.zero) && trail.HasMark(Vector2Int.right), "Clear failed to return marks.");
        }
        finally { UnityEngine.Object.DestroyImmediate(owner); }
    }

    static void CheckMapOwnership()
    {
        GameObject owner = new GameObject("MapOwnershipCheck");
        owner.SetActive(false);
        try
        {
            GridMap map = owner.AddComponent<GridMap>();
            GameObject unrelated = new GameObject("Unrelated");
            unrelated.transform.SetParent(owner.transform, false);
            GameObject generated = new GameObject("Generated");
            generated.transform.SetParent(owner.transform, false);
            FieldInfo rootField = typeof(GridMap).GetField("runtimeRoot", BindingFlags.Instance | BindingFlags.NonPublic);
            rootField.SetValue(map, generated.transform);
            map.ClearRuntimeMap();
            Require(generated == null && unrelated != null, "Clear destroyed the wrong objects.");
            map.ClearRuntimeMap();
            Require(unrelated != null, "Repeated clear destroyed unrelated objects.");
        }
        finally { UnityEngine.Object.DestroyImmediate(owner); }
    }

    static void CheckMaterialReference()
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Prefab/Map/GridLines.mat");
        GridMap prefab = AssetDatabase.LoadAssetAtPath<GridMap>("Assets/_Game/Prefab/Map/MapTemplate.prefab");
        Require(material != null && material.shader != null && material.shader.name == "Sprites/Default",
            "Grid shader is invalid: " + (material != null && material.shader != null ? material.shader.name : "null"));
        SerializedObject serializedPrefab = new SerializedObject(prefab);
        Require(serializedPrefab.FindProperty("gridMaterial").objectReferenceValue == material, "Grid material is not wired.");
    }

    static void CheckMapBuilderLimits()
    {
        MapBuilderGridState grid = new MapBuilderGridState(100, 100);
        Require(grid.Width == MapBuilderLimits.MAX_COLUMNS, "Map Builder column limit failed.");
        Require(grid.Height == MapBuilderLimits.MAX_ROWS, "Map Builder row limit failed.");
    }

    static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
