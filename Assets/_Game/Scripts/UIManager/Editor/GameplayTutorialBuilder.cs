using System;
using System.IO;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class GameplayTutorialBuilder
{
    private const string REQUEST = "Temp/BuildGameplayTutorial.request";
    private const string REPORT = "Temp/BuildGameplayTutorial.result";
    private const string PREFAB = "Assets/_Game/Prefab/UI/Gameplay.prefab";
    private const string PANEL =
        "Assets/_Game/Resources/Art/SettingsUI/settings_panel_body.png";
    private const string MOTHER =
        "Assets/_Game/Resources/Art/Generated/mother_shark_sprite.png";
    private const string HAND =
        "Assets/_Game/Resources/BlockedIn/Internship_BT4_MoveStopMove-20221110T015023Z-001/Internship_BT4_MoveStopMove/GUI/GamePlay/Icon_Hand.png";

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
            File.WriteAllText(REPORT, "PASS: Level 1 tutorial UI created.");
        }
        catch (Exception exception)
        {
            File.WriteAllText(REPORT, exception.ToString());
            Debug.LogException(exception);
        }
    }

    [MenuItem("Tools/A Mother's Ocean/Build Level 1 Tutorial")]
    public static void Build()
    {
        Gameplay source = AssetDatabase.LoadAssetAtPath<Gameplay>(PREFAB);
        Gameplay gameplay = UnityEngine.Object.Instantiate(source);
        try
        {
            SerializedObject gameplayData = new SerializedObject(gameplay);
            RemovePreviousTutorial(gameplayData);
            GameplayTutorial tutorial = CreateTutorial(gameplay.transform,
                out GameObject tutorialRoot);
            Assign(gameplayData, "tutorialRoot", tutorialRoot);
            Assign(gameplayData, "tutorialController", tutorial);
            gameplayData.ApplyModifiedPropertiesWithoutUndo();
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

    private static void RemovePreviousTutorial(SerializedObject gameplayData)
    {
        SerializedProperty root = gameplayData.FindProperty("tutorialRoot");
        if (root.objectReferenceValue != null)
            UnityEngine.Object.DestroyImmediate(root.objectReferenceValue);
        SerializedProperty controller = gameplayData.FindProperty("tutorialController");
        if (controller.objectReferenceValue != null)
            UnityEngine.Object.DestroyImmediate(controller.objectReferenceValue);
    }

    private static GameplayTutorial CreateTutorial(Transform owner,
        out GameObject tutorialRoot)
    {
        RectTransform root = WinPreviewElements.Rect("Tutorial", owner,
            Vector2.one * .5f, Vector2.zero);
        tutorialRoot = root.gameObject;
        Stretch(root);
        CreateDim(root);
        Image glow = CreateFocus("Target Glow", root,
            new Color(0.15f, 0.95f, 1f, 0.4f), 170f);
        Image focus = CreateFocus("Target Focus", root, Color.white, 145f);
        CreateDialogue(root, out TMP_Text dialogue);
        CreateNarrator(root);
        Image hand = CreateHand(root);
        GameplayTutorial tutorial = owner.gameObject.AddComponent<GameplayTutorial>();
        SerializedObject data = new SerializedObject(tutorial);
        Assign(data, "tutorialRoot", root.gameObject);
        Assign(data, "dialogueText", dialogue);
        Assign(data, "handIcon", hand);
        Assign(data, "focusGlow", glow);
        Assign(data, "focusIcon", focus);
        data.ApplyModifiedPropertiesWithoutUndo();
        return tutorial;
    }

    private static void CreateDim(Transform root)
    {
        Image dim = WinPreviewElements.Image("Dim", root, Vector2.one * .5f,
            Vector2.zero, new Color(0.01f, 0.07f, 0.13f, 0.62f));
        Stretch(dim.rectTransform);
    }

    private static Image CreateFocus(string name, Transform root,
        Color color, float size)
    {
        Image image = WinPreviewElements.Image(name, root, Vector2.one * .5f,
            Vector2.one * size, color);
        image.preserveAspect = true;
        return image;
    }

    private static void CreateDialogue(Transform root, out TMP_Text dialogue)
    {
        Image bubble = WinPreviewElements.Image("Dialogue Bubble", root,
            new Vector2(.38f, .34f), new Vector2(700, 190),
            new Color(1f, .97f, .86f, 1f));
        bubble.sprite = LoadSprite(PANEL);
        bubble.type = Image.Type.Sliced;
        RectTransform tail = WinPreviewElements.Rect("Dialogue Tail", root,
            new Vector2(.52f, .285f), new Vector2(64, 64));
        tail.localRotation = Quaternion.Euler(0f, 0f, 45f);
        Image tailImage = tail.gameObject.AddComponent<Image>();
        tailImage.color = new Color(1f, .97f, .86f, 1f);
        tailImage.raycastTarget = false;
        dialogue = WinPreviewElements.Text("Dialogue", root,
            new Vector2(.38f, .34f), new Vector2(630, 140),
            "Swipe right to move", 64);
    }

    private static void CreateNarrator(Transform root)
    {
        Image narrator = WinPreviewElements.Image("Default Shark Narrator", root,
            new Vector2(.73f, .18f), new Vector2(480, 360), Color.white);
        narrator.sprite = LoadSprite(MOTHER);
        narrator.preserveAspect = true;
    }

    private static Image CreateHand(Transform root)
    {
        Image hand = WinPreviewElements.Image("Tutorial Hand", root,
            Vector2.one * .5f, new Vector2(155, 165), Color.white);
        hand.sprite = LoadSprite(HAND);
        hand.preserveAspect = true;
        return hand;
    }

    private static Sprite LoadSprite(string path)
    {
        foreach (UnityEngine.Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
        {
            if (asset is Sprite sprite) return sprite;
        }
        throw new InvalidOperationException("Không tìm thấy sprite: " + path);
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.anchoredPosition = Vector2.zero;
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
            throw new InvalidOperationException("Gameplay component không tồn tại.");
        string identifier = component.Groups[1].Value;
        yaml = yaml.Replace("&" + identifier + "\n", "&1326024517158735020\n")
            .Replace("fileID: " + identifier + "}",
                "fileID: 1326024517158735020}");
        File.WriteAllText(PREFAB, yaml);
    }
}
