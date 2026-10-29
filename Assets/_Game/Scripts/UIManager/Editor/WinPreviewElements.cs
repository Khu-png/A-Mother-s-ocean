using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class WinPreviewElements
{
    private const string FONT_PATH = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";

    public static void CreateTextMaterial()
    {
        foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(FONT_PATH))
        {
            if (!(asset is Material source)) continue;
            Material material = new Material(source);
            material.SetColor("_OutlineColor", new Color32(7, 75, 118, 255));
            material.SetFloat("_OutlineWidth", 0.2f);
            material.SetFloat("_FaceDilate", 0.25f);
            material.EnableKeyword("OUTLINE_ON");
            string path = "Assets/_Game/Resources/Art/WinFlow/WinText.mat";
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(material, path);
            return;
        }
        throw new System.InvalidOperationException("Missing font material.");
    }

    public static Sprite CreateRoundedSprite()
    {
        const int SIZE = 128;
        const float RADIUS = 24f;
        Texture2D texture = new Texture2D(SIZE, SIZE, TextureFormat.RGBA32, false);
        for (int y = 0; y < SIZE; y++)
        {
            for (int x = 0; x < SIZE; x++)
            {
                float dx = Mathf.Max(Mathf.Abs(x + .5f - SIZE / 2f) - (SIZE / 2f - RADIUS), 0);
                float dy = Mathf.Max(Mathf.Abs(y + .5f - SIZE / 2f) - (SIZE / 2f - RADIUS), 0);
                float alpha = Mathf.Clamp01(RADIUS - Mathf.Sqrt(dx * dx + dy * dy));
                texture.SetPixel(x, y, new Color(1, 1, 1, alpha));
            }
        }
        texture.Apply();
        string path = "Assets/_Game/Resources/Art/WinFlow/RoundedButton.png";
        System.IO.File.WriteAllBytes(path, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(path);
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spriteBorder = Vector4.one * RADIUS;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    public static RectTransform Rect(string name, Transform owner, Vector2 center, Vector2 size)
    {
        RectTransform rect = new GameObject(name, typeof(RectTransform)).transform as RectTransform;
        rect.gameObject.layer = 5;
        rect.SetParent(owner, false);
        rect.anchorMin = rect.anchorMax = center;
        rect.sizeDelta = size;
        return rect;
    }

    public static Image Image(string name, Transform owner, Vector2 center, Vector2 size, Color color)
    {
        RectTransform rect = Rect(name, owner, center, size);
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    public static TMP_Text Text(string name, Transform owner, Vector2 center, Vector2 size,
        string value, float fontSize)
    {
        RectTransform rect = Rect(name, owner, center, size);
        TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_PATH);
        text.text = value;
        text.fontSize = fontSize;
        text.fontStyle = FontStyles.Bold;
        text.fontWeight = FontWeight.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        text.fontSharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/_Game/Resources/Art/WinFlow/WinText.mat");
        return text;
    }

    public static Image Coin(string name, Transform owner, Vector2 center, float diameter)
    {
        Image coin = Image(name, owner, center, Vector2.one * diameter, new Color32(255, 197, 46, 255));
        coin.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        Text("Symbol", coin.rectTransform, Vector2.one * 0.5f, Vector2.one * diameter, "$", diameter * 0.65f);
        return coin;
    }

    public static CanvasGroup Group(string name, Transform owner)
    {
        RectTransform rect = Rect(name, owner, Vector2.one * 0.5f, Vector2.zero);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        return rect.gameObject.AddComponent<CanvasGroup>();
    }

    public static Sprite HugSprite()
    {
        foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath("Assets/_Game/Resources/Art/Generated/victory-screen.png"))
        {
            if (asset is Sprite sprite && sprite.name == "victory-hug") return sprite;
        }
        throw new System.InvalidOperationException("Victory hug sprite has not imported.");
    }
}
