using System;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class WinPreviewAnimations
{
    private const string DIRECTORY = "Assets/_Game/Resources/Art/WinFlow";

    public static RuntimeAnimatorController Build()
    {
        Directory.CreateDirectory(DIRECTORY);
        AssetDatabase.Refresh();
        AnimationClip intro = CreateIntro();
        AnimationClip claim = CreateClaim();
        string controllerPath = DIRECTORY + "/WinFlow.controller";
        AssetDatabase.DeleteAsset(controllerPath);
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        AnimatorState opening = machine.AddState("Intro");
        opening.motion = intro;
        AnimatorState claiming = machine.AddState("Claim");
        claiming.motion = claim;
        machine.defaultState = opening;
        return controller;
    }

    static AnimationClip CreateIntro()
    {
        AnimationClip clip = new AnimationClip { name = "Intro", frameRate = 60 };
        Scale(clip, "Hug", new float[] { 0, .001f, .32f, 1.08f, .5f, 1, 1.3f, 1, 1.75f, .877193f });
        Curve(clip, "Hug", typeof(RectTransform), "m_AnchoredPosition.y", 0, 0, .7f, 12, 1.1f, -8, 1.3f, 0, 1.75f, 60);
        Curve(clip, "Hug", typeof(RectTransform), "localEulerAnglesRaw.z", 0, 0, .5f, 2, .9f, -2, 1.3f, 0, 1.75f, 0);
        Curve(clip, "Reward", typeof(CanvasGroup), "m_Alpha", 0, 0, 1.3f, 0, 1.75f, 1);
        Curve(clip, "Fade", typeof(CanvasGroup), "m_Alpha", 0, 0, 1.75f, 0);
        Scale(clip, "Reward/ClaimButton", new float[] { 0, 1, 1.75f, 1 });
        for (int i = 0; i < 7; i++)
            Curve(clip, "FlyingCoin" + i, typeof(CanvasGroup), "m_Alpha", 0, 0, 1.75f, 0);
        return Save(clip);
    }

    static AnimationClip CreateClaim()
    {
        AnimationClip clip = new AnimationClip { name = "Claim", frameRate = 60 };
        Scale(clip, "Hug", new float[] { 0, .877193f, 1.9f, .877193f });
        Curve(clip, "Hug", typeof(RectTransform), "m_AnchoredPosition.y", 0, 60, 1.9f, 60);
        Curve(clip, "Hug", typeof(RectTransform), "localEulerAnglesRaw.z", 0, 0, 1.9f, 0);
        Curve(clip, "Reward", typeof(CanvasGroup), "m_Alpha", 0, 1, 1.9f, 1);
        Scale(clip, "Reward/ClaimButton", new float[] { 0, 1, .12f, .94f, .25f, 1, 1.9f, 1 });
        Curve(clip, "Fade", typeof(CanvasGroup), "m_Alpha", 0, 0, 1.55f, 0, 1.9f, 1);
        for (int i = 0; i < 7; i++) AddCoinFlight(clip, i);
        return Save(clip);
    }

    static void AddCoinFlight(AnimationClip clip, int index)
    {
        string path = "FlyingCoin" + index;
        float start = .25f + index * .07f;
        float end = start + .7f;
        Curve(clip, path, typeof(CanvasGroup), "m_Alpha", 0, 0, start, 0, start + .02f, 1, end - .02f, 1, end, 0, 1.9f, 0);
        foreach (string anchor in new[] { "m_AnchorMin", "m_AnchorMax" })
        {
            Curve(clip, path, typeof(RectTransform), anchor + ".x", 0, .5f, start, .5f, end, .755f, 1.9f, .755f);
            Curve(clip, path, typeof(RectTransform), anchor + ".y", 0, .284f, start, .284f, end, .953f, 1.9f, .953f);
        }
        Curve(clip, path, typeof(RectTransform), "m_AnchoredPosition.x", 0, 0, start, 0, start + .35f, 90, end, 0, 1.9f, 0);
    }

    static void Scale(AnimationClip clip, string path, float[] keys)
    {
        Curve(clip, path, typeof(RectTransform), "m_LocalScale.x", keys);
        Curve(clip, path, typeof(RectTransform), "m_LocalScale.y", keys);
    }

    static void Curve(AnimationClip clip, string path, Type type, string property, params float[] points)
    {
        Keyframe[] keys = new Keyframe[points.Length / 2];
        for (int i = 0; i < keys.Length; i++) keys[i] = new Keyframe(points[i * 2], points[i * 2 + 1]);
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, type, property), new AnimationCurve(keys));
    }

    static AnimationClip Save(AnimationClip clip)
    {
        string path = DIRECTORY + "/" + clip.name + ".anim";
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(clip, path);
        return clip;
    }
}
