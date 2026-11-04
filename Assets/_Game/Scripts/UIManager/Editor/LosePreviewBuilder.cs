using System;
using System.IO;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

public static class LosePreviewBuilder
{
    private const string PREFAB = "Assets/_Game/Prefab/UI/Lose.prefab";
    private const string LOSE_ART =
        "Assets/_Game/Resources/Art/Generated/lose-sad-shark.png";

    [MenuItem("Tools/A Mother's Ocean/Build Lose Screen")]
    public static void Build()
    {
        RectTransform root = WinPreviewElements.Rect("Lose", null,
            Vector2.one * .5f, Vector2.zero);
        root.anchorMin = Vector2.zero;
        root.anchorMax = Vector2.one;
        Lose lose = root.gameObject.AddComponent<Lose>();
        try
        {
            BuildContents(root, lose);
            PrefabUtility.SaveAsPrefabAsset(root.gameObject, PREFAB);
            PreserveReference();
            AssetDatabase.ImportAsset(PREFAB, ImportAssetOptions.ForceUpdate);
            AssetDatabase.SaveAssets();
        }
        finally { UnityEngine.Object.DestroyImmediate(root.gameObject); }
    }

    private static void BuildContents(RectTransform root, Lose lose)
    {
        Sprite rounded = WinPreviewElements.CreateRoundedSprite();
        Image shade = WinPreviewElements.Image("Shade", root, Vector2.one * .5f,
            Vector2.zero, new Color(0f, .08f, .12f, .58f));
        Stretch(shade.rectTransform);
        shade.raycastTarget = true;

        RectTransform panel = WinPreviewElements.Rect("Panel", root,
            new Vector2(.5f, .5f), new Vector2(760, 1020));
        TMP_Text title = WinPreviewElements.Text("Title", panel,
            new Vector2(.5f, .79f), new Vector2(700, 180), "OUT OF TIME", 104);

        Image shark = WinPreviewElements.Image("Sad Shark", panel,
            new Vector2(.5f, .55f), new Vector2(580, 400), Color.white);
        shark.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(LOSE_ART);
        shark.preserveAspect = true;

        TMP_Text replayLabel;
        Button replay = Button("Replay Button", panel,
            new Vector2(.5f, .24f), new Vector2(620, 170),
            new Color32(54, 169, 229, 255), "REPLAY  -1", lose.OnClickReplay,
            rounded, Color.clear, out replayLabel);

        SerializedObject data = new SerializedObject(lose);
        Assign(data, "m_RectTransform", root);
        Assign(data, "replayButton", replay);
        Assign(data, "titleText", title);
        Assign(data, "replayLabel", replayLabel);
        data.ApplyModifiedPropertiesWithoutUndo();
    }

    private static Image Panel(string name, Transform owner, Vector2 center,
        Vector2 size, Color color, Sprite rounded)
    {
        Image image = WinPreviewElements.Image(name, owner, center, size, color);
        image.sprite = rounded;
        image.type = Image.Type.Sliced;
        image.raycastTarget = true;
        return image;
    }

    private static Button Button(string name, Transform owner, Vector2 center,
        Vector2 size, Color color, string text, UnityEngine.Events.UnityAction action,
        Sprite rounded, Color frameColor, out TMP_Text label)
    {
        Image frame = Panel(name, owner, center, size, frameColor, rounded);
        Image fill = Panel("Fill", frame.rectTransform, Vector2.one * .5f,
            size - new Vector2(18, 18), color, rounded);
        Button button = frame.gameObject.AddComponent<Button>();
        button.targetGraphic = fill;
        UnityEventTools.AddPersistentListener(button.onClick, action);
        label = WinPreviewElements.Text("Label", frame.rectTransform,
            Vector2.one * .5f, size - new Vector2(30, 20), text, 56);
        return button;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
    }

    private static void Assign(SerializedObject data, string property,
        UnityEngine.Object value)
    {
        data.FindProperty(property).objectReferenceValue = value;
    }

    private static void PreserveReference()
    {
        string yaml = File.ReadAllText(PREFAB);
        Match component = Regex.Match(yaml,
            @"--- !u!114 &(\d+)\n(?:(?!--- !u!).)*guid: 7225b3effe510e34eb96b72609dd477c",
            RegexOptions.Singleline);
        if (!component.Success) throw new InvalidOperationException("Saved prefab has no Lose component.");
        string identifier = component.Groups[1].Value;
        yaml = yaml.Replace("&" + identifier + "\n", "&6968357842755941048\n")
            .Replace("fileID: " + identifier + "}", "fileID: 6968357842755941048}");
        File.WriteAllText(PREFAB, yaml);
    }
}
