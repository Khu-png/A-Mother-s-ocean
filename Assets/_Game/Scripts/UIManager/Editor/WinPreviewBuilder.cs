using System;
using System.IO;
using System.Text.RegularExpressions;
using AssetKits.ParticleImage;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using ParticlePlayMode = AssetKits.ParticleImage.Enumerations.PlayMode;

public static class WinPreviewBuilder
{
    private const string REQUEST = "Temp/BuildWinPreview.request";
    private const string REPORT = "Temp/BuildWinPreview.result";
    private const string PREFAB = "Assets/_Game/Prefab/UI/Win.prefab";
    private const string CONFETTI_PREFAB = "Assets/AssetKits/ParticleImage/Demo/Prefabs/ConfettiBurst.prefab";

    [InitializeOnLoadMethod]
    static void QueueRequestedBuild()
    {
        if (File.Exists(REQUEST)) EditorApplication.delayCall += BuildRequested;
    }

    static void BuildRequested()
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
            File.WriteAllText(REPORT, "PASS: Win prefab, Animator and clips created with all references assigned.");
        }
        catch (Exception exception)
        {
            File.WriteAllText(REPORT, exception.ToString());
            Debug.LogException(exception);
        }
    }

    [MenuItem("Tools/A Mother's Ocean/Build Approved Win Preview")]
    public static void Build()
    {
        RectTransform root = WinPreviewElements.Rect("Win", null, Vector2.one * 0.5f, Vector2.zero);
        root.anchorMin = Vector2.zero;
        root.anchorMax = Vector2.one;
        Win win = root.gameObject.AddComponent<Win>();
        try
        {
            BuildContents(root, win);
            PrefabUtility.SaveAsPrefabAsset(root.gameObject, PREFAB);
            PreserveWinReference();
            AssetDatabase.ImportAsset(PREFAB, ImportAssetOptions.ForceUpdate);
            AssetDatabase.SaveAssets();
        }
        finally { UnityEngine.Object.DestroyImmediate(root.gameObject); }
    }

    static void BuildContents(RectTransform root, Win win)
    {
        WinPreviewElements.CreateTextMaterials();
        Image shade = WinPreviewElements.Image("Shade", root, Vector2.one * 0.5f, Vector2.zero,
            new Color(0f, 0.098f, 0.188f, 0.45f));
        shade.rectTransform.anchorMin = Vector2.zero;
        shade.rectTransform.anchorMax = Vector2.one;
        shade.raycastTarget = true;
        RectTransform content = WinPreviewElements.Rect("Content", root, Vector2.one * 0.5f, Vector2.zero);
        content.anchorMin = Vector2.zero;
        content.anchorMax = Vector2.one;
        content.gameObject.SetActive(false);
        Animator animator = content.gameObject.AddComponent<Animator>();
        animator.runtimeAnimatorController = WinPreviewAnimations.Build();
        animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        RectTransform hug = WinPreviewElements.Rect("Hug", content, new Vector2(.5f, .5f),
            new Vector2(855, 815));
        WinPreviewElements.Glow("MotherGlow", hug, new Vector2(.45f, .5f),
            new Vector2(1260, 1160), new Color32(38, 196, 255, 255));
        WinPreviewElements.Glow("BabyGlow", hug, new Vector2(.71f, .31f),
            new Vector2(960, 900), new Color32(73, 226, 255, 255));
        Image hugArtwork = WinPreviewElements.Image("SharkArtwork", hug, Vector2.one * .5f,
            new Vector2(855, 815), Color.white);
        hugArtwork.sprite = WinPreviewElements.HugSprite();
        hugArtwork.preserveAspect = true;
        ParticleImage[] confettiEffects =
        {
            CreateConfettiEffect(content, "ConfettiBurstLeft", .06f),
            CreateConfettiEffect(content, "ConfettiBurstRight", .94f)
        };
        CanvasGroup reward = WinPreviewElements.Group("Reward", content);
        WinPreviewElements.Title("Title", reward.transform, new Vector2(.5f, .797f),
            new Vector2(1000, 170), "YOU WIN", 129);
        RectTransform rewardAmount = WinPreviewElements.Rect("RewardAmount", reward.transform,
            new Vector2(.5f, .284f), new Vector2(560, 240));
        WinPreviewElements.Coin("RewardCoin", rewardAmount, new Vector2(.28f, .5f), 180);
        var rewardText = WinPreviewElements.Text("Amount", rewardAmount, new Vector2(.65f, .5f),
            new Vector2(320, 150), "+100", 86);
        Image buttonFrame = WinPreviewElements.Image("ClaimButton", reward.transform, new Vector2(.5f, .153f),
            new Vector2(810, 192), new Color32(255, 244, 194, 255));
        Sprite rounded = WinPreviewElements.CreateRoundedSprite();
        buttonFrame.sprite = rounded;
        buttonFrame.type = Image.Type.Sliced;
        buttonFrame.pixelsPerUnitMultiplier = 24f / 54f;
        buttonFrame.raycastTarget = true;
        Image fill = WinPreviewElements.Image("Fill", buttonFrame.rectTransform, Vector2.one * .5f,
            new Vector2(786, 168), new Color32(121, 216, 44, 255));
        fill.sprite = rounded;
        fill.type = Image.Type.Sliced;
        fill.pixelsPerUnitMultiplier = 24f / 42f;
        Button button = buttonFrame.gameObject.AddComponent<Button>();
        button.targetGraphic = fill;
        UnityEventTools.AddPersistentListener(button.onClick, win.OnClickNext);
        var label = WinPreviewElements.Text("Label", buttonFrame.rectTransform, Vector2.one * .5f,
            new Vector2(750, 150), "CLAIM 100", 81);
        var wallet = WinPreviewElements.CoinBar("WalletCoinBar", content,
            new Vector2(.84f, .953f), new Vector2(300, 128), "250", 54);
        for (int i = 0; i < 7; i++)
        {
            Image coin = WinPreviewElements.Coin("FlyingCoin" + i, content, new Vector2(.5f, .284f), 54);
            coin.gameObject.AddComponent<CanvasGroup>().alpha = 0;
        }
        Image fade = WinPreviewElements.Image("Fade", content, Vector2.one * .5f, Vector2.zero,
            new Color32(3, 35, 65, 255));
        fade.rectTransform.anchorMin = Vector2.zero;
        fade.rectTransform.anchorMax = Vector2.one;
        fade.gameObject.AddComponent<CanvasGroup>().alpha = 0;
        SerializedObject data = new SerializedObject(win);
        Assign(data, "m_RectTransform", root);
        Assign(data, "sequenceAnimator", animator);
        Assign(data, "claimButton", button);
        Assign(data, "rewardText", rewardText);
        Assign(data, "claimText", label);
        Assign(data, "walletText", wallet);
        SerializedProperty confettiData = data.FindProperty("confettiEffects");
        confettiData.arraySize = confettiEffects.Length;
        for (int i = 0; i < confettiEffects.Length; i++)
            confettiData.GetArrayElementAtIndex(i).objectReferenceValue = confettiEffects[i];
        data.ApplyModifiedPropertiesWithoutUndo();
        content.gameObject.SetActive(true);
    }

    static ParticleImage CreateConfettiEffect(Transform owner, string name, float horizontalAnchor)
    {
        foreach (UnityEngine.Object asset in AssetDatabase.LoadAllAssetsAtPath(CONFETTI_PREFAB))
        {
            if (!(asset is ParticleImage effect)) continue;
            SerializedObject data = new SerializedObject(effect);
            if (data.FindProperty("_main").objectReferenceValue != effect) continue;
            ParticleImage instance = UnityEngine.Object.Instantiate(effect, owner);
            instance.gameObject.name = name;
            instance.rectTransform.anchorMin = new Vector2(horizontalAnchor, .5f);
            instance.rectTransform.anchorMax = new Vector2(horizontalAnchor, .5f);
            instance.rectTransform.anchoredPosition = Vector2.zero;
            instance.rectTransform.sizeDelta = Vector2.one * 100f;
            instance.raycastTarget = false;
            instance.loop = false;
            instance.PlayMode = ParticlePlayMode.None;
            foreach (ParticleImage child in instance.children)
            {
                child.raycastTarget = false;
                child.loop = false;
            }
            return instance;
        }
        throw new InvalidOperationException("ConfettiBurst has no main ParticleImage.");
    }

    static void Assign(SerializedObject data, string property, UnityEngine.Object value)
    {
        data.FindProperty(property).objectReferenceValue = value;
    }

    static void PreserveWinReference()
    {
        string yaml = File.ReadAllText(PREFAB);
        Match component = Regex.Match(yaml,
            @"--- !u!114 &(\d+)\n(?:(?!--- !u!).)*guid: e3401376547e9734c9d60e25b6eb0337",
            RegexOptions.Singleline);
        if (!component.Success) throw new InvalidOperationException("Saved prefab has no Win component.");
        string identifier = component.Groups[1].Value;
        yaml = yaml.Replace("&" + identifier + "\n", "&7799615184282313327\n")
            .Replace("fileID: " + identifier + "}", "fileID: 7799615184282313327}");
        File.WriteAllText(PREFAB, yaml);
    }
}
