using System;
using UnityEngine;
using UnityEngine.UI;

public class GameplayOceanUI : MonoBehaviour
{
    [SerializeField] private GridMap gridMap;
    [SerializeField] private string levelLabel = "Level 01";
    [SerializeField] private Font font;

    private RectTransform title;
    private Sprite panelSprite;

    private struct UiButton
    {
        public RectTransform Rect;
        public Text Label;
    }

    private void Awake()
    {
        panelSprite = CreateRoundedSprite();
        BuildTopHud();
        BuildGameInfo();
        BuildBottomHud();
    }

    private void Update()
    {
        float wave = Time.unscaledTime * 2.2f;
        title.anchoredPosition = new Vector2(0f, -126f + Mathf.Sin(wave) * 5f);
    }

    private void BuildTopHud()
    {
        AddText("Version", transform, "v0.1 ocean", 24, TextAnchor.UpperRight, new Color(0.83f, 0.96f, 1f, 0.82f),
            Anchor(1f, 1f), Anchor(1f, 1f), new Vector2(-28f, -14f), new Vector2(260f, 42f));

        title = AddText("Level Title", transform, levelLabel, 58, TextAnchor.MiddleCenter, new Color(0.02f, 0.2f, 0.28f, 1f),
            Anchor(0.5f, 1f), Anchor(0.5f, 1f), new Vector2(0f, -126f), new Vector2(420f, 92f)).rectTransform;

        UiButton pause = AddButton("Pause Button", transform, "II", new Vector2(92f, 92f), new Color(0.13f, 0.48f, 0.78f, 0.95f), null);
        Place(pause.Rect, Anchor(0f, 1f), Anchor(0f, 1f), new Vector2(88f, -138f), new Vector2(92f, 92f));

        UiButton reset = AddButton("Reset Button", transform, "RESET", new Vector2(150f, 74f), new Color(0.09f, 0.62f, 0.82f, 0.95f), ResetLevel);
        Place(reset.Rect, Anchor(1f, 1f), Anchor(1f, 1f), new Vector2(-100f, -138f), new Vector2(150f, 74f));
    }

    private void BuildGameInfo()
    {
        AddPanel("Target Counter Back", transform, new Color(0.02f, 0.23f, 0.38f, 0.32f),
            Anchor(0f, 1f), Anchor(0f, 1f), new Vector2(176f, -236f), new Vector2(260f, 56f));
        AddText("Target Counter", transform, "BABY SHARKS 00/04", 24, TextAnchor.MiddleCenter, Color.white,
            Anchor(0f, 1f), Anchor(0f, 1f), new Vector2(176f, -236f), new Vector2(240f, 44f));
    }

    private void BuildBottomHud()
    {
        AddPanel("Bottom Tray", transform, new Color(0.08f, 0.35f, 0.62f, 0.72f),
            Anchor(0.5f, 0f), Anchor(0.5f, 0f), new Vector2(0f, 96f), new Vector2(430f, 118f));

        for (int i = 0; i < 3; i++)
        {
            float x = -128f + i * 128f;
            AddPanel("Item Slot " + i, transform, new Color(0.93f, 0.99f, 1f, 0.92f),
                Anchor(0.5f, 0f), Anchor(0.5f, 0f), new Vector2(x, 100f), new Vector2(82f, 72f));
        }
    }

    private void ResetLevel()
    {
        Time.timeScale = 1f;
        if (gridMap != null) gridMap.ResetPlayerToStart();
    }

    private UiButton AddButton(string name, Transform parent, string text, Vector2 size, Color color, Action action)
    {
        Image image = AddPanel(name, parent, color, Anchor(0.5f, 0.5f), Anchor(0.5f, 0.5f), Vector2.zero, size);
        Button button = image.gameObject.AddComponent<Button>();
        image.raycastTarget = true;
        button.targetGraphic = image;
        if (action != null) button.onClick.AddListener(() => action());

        Text label = AddText("Label", image.rectTransform, text, 34, TextAnchor.MiddleCenter, Color.white,
            Anchor(0f, 0f), Anchor(1f, 1f), Vector2.zero, Vector2.zero);
        return new UiButton { Rect = image.rectTransform, Label = label };
    }

    private Image AddPanel(string name, Transform parent, Color color, Vector2 min, Vector2 max, Vector2 position, Vector2 size)
    {
        GameObject view = new GameObject(name);
        view.layer = gameObject.layer;
        RectTransform rect = view.AddComponent<RectTransform>();
        Image image = view.AddComponent<Image>();
        view.transform.SetParent(parent, false);
        image.sprite = panelSprite;
        image.type = Image.Type.Sliced;
        image.color = color;
        image.raycastTarget = false;
        Place(rect, min, max, position, size);
        return image;
    }

    private Text AddText(string name, Transform parent, string text, int size, TextAnchor align, Color color,
        Vector2 min, Vector2 max, Vector2 position, Vector2 rectSize)
    {
        GameObject view = new GameObject(name);
        view.layer = gameObject.layer;
        RectTransform rect = view.AddComponent<RectTransform>();
        Text label = view.AddComponent<Text>();
        view.transform.SetParent(parent, false);
        label.font = UiFont();
        label.text = text;
        label.fontSize = size;
        label.fontStyle = FontStyle.Bold;
        label.alignment = align;
        label.color = color;
        label.raycastTarget = false;
        Place(rect, min, max, position, rectSize);
        return label;
    }

    private void Place(RectTransform rect, Vector2 min, Vector2 max, Vector2 position, Vector2 size)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private Vector2 Anchor(float x, float y) { return new Vector2(x, y); }

    private Font UiFont()
    {
        if (font != null) return font;
        Font legacyFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return legacyFont != null ? legacyFont : Resources.GetBuiltinResource<Font>("Arial.ttf");
    }

    private Sprite CreateRoundedSprite()
    {
        Texture2D texture = new Texture2D(64, 64, TextureFormat.ARGB32, false);
        Color clear = new Color(1f, 1f, 1f, 0f);
        for (int y = 0; y < 64; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                float dx = Mathf.Max(10f - x, x - 53f, 0f);
                float dy = Mathf.Max(10f - y, y - 53f, 0f);
                texture.SetPixel(x, y, dx * dx + dy * dy <= 100f ? Color.white : clear);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, 64f, 64f), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(16f, 16f, 16f, 16f));
    }
}
