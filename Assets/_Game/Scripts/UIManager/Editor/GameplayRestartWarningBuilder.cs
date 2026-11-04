using System;
using System.IO;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

public static class GameplayRestartWarningBuilder
{
    private const string REQUEST = "Temp/BuildGameplayRestartWarning.request";
    private const string REPORT = "Temp/BuildGameplayRestartWarning.result";
    private const string PREFAB = "Assets/_Game/Prefab/UI/Gameplay.prefab";

    [InitializeOnLoadMethod]
    private static void QueueRequestedBuild()
    {
        if (File.Exists(REQUEST)) EditorApplication.delayCall += BuildRequested;
    }

    private static void BuildRequested()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.delayCall += BuildRequested;
            return;
        }
        File.Delete(REQUEST);
        try
        {
            Build();
            File.WriteAllText(REPORT,
                "PASS: gameplay restart warning and serialized references created.");
        }
        catch (Exception exception)
        {
            File.WriteAllText(REPORT, exception.ToString());
            Debug.LogException(exception);
        }
    }

    [MenuItem("Tools/A Mother's Ocean/Build Gameplay Restart Warning")]
    public static void Build()
    {
        Gameplay source = AssetDatabase.LoadAssetAtPath<Gameplay>(PREFAB);
        Gameplay gameplay = UnityEngine.Object.Instantiate(source);
        try
        {
            SerializedObject data = new SerializedObject(gameplay);
            SerializedProperty warningProperty = data.FindProperty("restartWarning");
            if (warningProperty.objectReferenceValue == null)
                CreateWarning(gameplay, data);
            data.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(gameplay.gameObject, PREFAB);
            PreserveGameplayReference();
            AssetDatabase.ImportAsset(PREFAB, ImportAssetOptions.ForceUpdate);
            AssetDatabase.SaveAssets();
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(gameplay.gameObject);
        }
    }

    private static void CreateWarning(Gameplay gameplay, SerializedObject data)
    {
        Sprite rounded = WinPreviewElements.CreateRoundedSprite();
        Image blocker = WinPreviewElements.Image("Restart Warning", gameplay.transform,
            Vector2.one * .5f, Vector2.zero, new Color(0f, .04f, .08f, .72f));
        Stretch(blocker.rectTransform);
        blocker.raycastTarget = true;
        RectTransform dialog = CreateDialog(blocker.rectTransform, rounded);
        TMP_Text message = WinPreviewElements.Text("Message", dialog,
            new Vector2(.5f, .54f), new Vector2(700, 120),
            "THIS WILL COST 1 HEART", 54);
        CreateButtons(dialog, gameplay, rounded, out TMP_Text confirmLabel);
        Assign(data, "restartWarning", blocker.gameObject);
        Assign(data, "restartWarningMessage", message);
        Assign(data, "restartConfirmLabel", confirmLabel);
        blocker.gameObject.SetActive(false);
    }

    private static RectTransform CreateDialog(RectTransform parent, Sprite rounded)
    {
        Image dialog = Panel("Dialog", parent, Vector2.one * .5f,
            new Vector2(780, 600), new Color32(17, 61, 87, 255), rounded);
        WinPreviewElements.Text("Warning Title", dialog.rectTransform,
            new Vector2(.5f, .79f), new Vector2(700, 120), "RESTART LEVEL?", 78);
        return dialog.rectTransform;
    }

    private static void CreateButtons(RectTransform dialog, Gameplay gameplay,
        Sprite rounded, out TMP_Text confirmLabel)
    {
        Button("Cancel Button", dialog, new Vector2(.28f, .18f),
            new Vector2(310, 125), new Color32(238, 92, 92, 255), "CANCEL",
            gameplay.OnClickCancelRestart, rounded, out _);
        Button("Confirm Button", dialog, new Vector2(.72f, .18f),
            new Vector2(310, 125), new Color32(99, 204, 78, 255), "RESTART",
            gameplay.OnClickConfirmRestart, rounded, out confirmLabel);
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
        Sprite rounded, out TMP_Text label)
    {
        Image frame = Panel(name, owner, center, size,
            new Color32(7, 42, 63, 255), rounded);
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

    private static void PreserveGameplayReference()
    {
        string yaml = File.ReadAllText(PREFAB);
        Match component = Regex.Match(yaml,
            @"--- !u!114 &(\d+)\n(?:(?!--- !u!).)*guid: 8bece27c7d3f40388b50416025ef81d1",
            RegexOptions.Singleline);
        if (!component.Success)
            throw new InvalidOperationException("Saved prefab has no Gameplay component.");
        string identifier = component.Groups[1].Value;
        yaml = yaml.Replace("&" + identifier + "\n", "&1326024517158735020\n")
            .Replace("fileID: " + identifier + "}", "fileID: 1326024517158735020}");
        File.WriteAllText(PREFAB, yaml);
    }
}
