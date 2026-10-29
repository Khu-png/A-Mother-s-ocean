using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class WinFlowCheck
{
    public static void Run()
    {
        WinPreviewBuilder.Build();
        ValidateReferences();
        ValidateRewardClaims();
        RenderPreview();
        File.WriteAllText("Temp/WinFlowCheck.txt", "PASS: references, animations, reward claims and preview render.");
    }

    static void ValidateReferences()
    {
        Win win = AssetDatabase.LoadAssetAtPath<Win>("Assets/_Game/Prefab/UI/Win.prefab");
        SerializedObject data = new SerializedObject(win);
        foreach (string field in new[] { "sequenceAnimator", "claimButton", "rewardText", "claimText", "walletText" })
            Require(data.FindProperty(field).objectReferenceValue != null, "Missing " + field);
        AssetDatabase.TryGetGUIDAndLocalFileIdentifier(win, out string guid, out long id);
        Require(id == 7799615184282313327, "Win prefab reference changed.");
        foreach (string name in new[] { "Intro", "Claim" })
        {
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(
                "Assets/_Game/Resources/Art/WinFlow/" + name + ".anim");
            Require(clip.length > 1f, "Missing animation duration.");
            Require(AnimationUtility.GetCurveBindings(clip).Length > 10, "Missing animation curves.");
        }
    }

    static void ValidateRewardClaims()
    {
        bool hadCoins = PlayerPrefs.HasKey(MenuPlayerProgress.CoinsKey);
        int coins = PlayerPrefs.GetInt(MenuPlayerProgress.CoinsKey);
        try
        {
            PlayerPrefs.SetInt(MenuPlayerProgress.CoinsKey, 250);
            MenuPlayerProgress.GrantCompletionCoins(100);
            Require(MenuPlayerProgress.Coins == 350, "Incorrect balance.");
            MenuPlayerProgress.GrantCompletionCoins(100);
            Require(MenuPlayerProgress.Coins == 450, "Repeated reward was not granted.");
        }
        finally
        {
            if (hadCoins) PlayerPrefs.SetInt(MenuPlayerProgress.CoinsKey, coins);
            else PlayerPrefs.DeleteKey(MenuPlayerProgress.CoinsKey);
            PlayerPrefs.Save();
        }
    }

    static void RenderPreview()
    {
        RectTransform canvasRect = WinPreviewElements.Rect("PreviewCanvas", null, Vector2.one * .5f,
            new Vector2(1080, 1920));
        Canvas canvas = canvasRect.gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvasRect.localScale = Vector3.one * .01f;
        Camera camera = new GameObject("PreviewCamera").AddComponent<Camera>();
        RenderTexture target = new RenderTexture(360, 640, 24);
        try
        {
            camera.transform.position = new Vector3(0, 0, -10);
            camera.orthographic = true;
            camera.orthographicSize = 9.6f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.02f, .35f, .55f);
            camera.cullingMask = 1 << 5;
            camera.targetTexture = target;
            RectTransform background = WinPreviewElements.Rect("Ocean", canvasRect, Vector2.one * .5f,
                new Vector2(1080, 1920));
            RawImage image = background.gameObject.AddComponent<RawImage>();
            image.texture = AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/_Game/Resources/Art/Generated/ocean_background_9x16_rounded.png");
            Win template = AssetDatabase.LoadAssetAtPath<Win>("Assets/_Game/Prefab/UI/Win.prefab");
            Win win = UnityEngine.Object.Instantiate(template, canvasRect, false);
            win.enabled = false;
            SerializedObject data = new SerializedObject(win);
            Animator animator = (Animator)data.FindProperty("sequenceAnimator").objectReferenceValue;
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(
                "Assets/_Game/Resources/Art/WinFlow/Intro.anim");
            clip.SampleAnimation(animator.gameObject, 1.75f);
            Canvas.ForceUpdateCanvases();
            camera.Render();
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = target;
            Texture2D pixels = new Texture2D(360, 640, TextureFormat.RGB24, false);
            pixels.ReadPixels(new Rect(0, 0, 360, 640), 0, 0);
            pixels.Apply();
            File.WriteAllBytes("Temp/WinFlowUnityPreview.png", pixels.EncodeToPNG());
            RenderTexture.active = previous;
            UnityEngine.Object.DestroyImmediate(pixels);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(canvasRect.gameObject);
            UnityEngine.Object.DestroyImmediate(camera.gameObject);
            target.Release();
            UnityEngine.Object.DestroyImmediate(target);
        }
    }

    static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
