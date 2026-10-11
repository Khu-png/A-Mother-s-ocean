using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class LoadingManager : Singleton<LoadingManager>
{
    [SerializeField] private GameObject loadingCanvas;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private TMP_Text progressText;
    [SerializeField, Min(0.01f)] private float progressSpeed = 0.8f;
    [SerializeField, Min(1f)] private float videoTimeout = 15f;
    [SerializeField, Min(0f)] private float videoWarmupTime = 0.25f;
    private Coroutine loadingRoutine;
    private VideoPlayer menuVideo;
    private bool firstFrameReady;
    private bool videoFailed;

    public bool IsLoading { get; private set; }
    public float Progress { get; private set; }

    private void Awake()
    {
        RegisterSingleton(this);
        if (loadingCanvas != null) loadingCanvas.SetActive(true);
        SetProgress(0f);
    }

    public void LoadMainMenu()
    {
        if (!BeginLoading()) return;
        loadingRoutine = StartCoroutine(LoadMenu());
    }

    public void LoadGameplay(int levelIndex)
    {
        if (!BeginLoading()) return;
        loadingRoutine = StartCoroutine(LoadLevel(levelIndex));
    }

    private bool BeginLoading()
    {
        if (IsLoading) return false;
        if (loadingCanvas == null || progressSlider == null)
        {
            Debug.LogError("LoadingManager requires a loading canvas and progress slider.", this);
            return false;
        }
        IsLoading = true;
        loadingCanvas.SetActive(true);
        progressSlider.interactable = false;
        SetProgress(0f);
        return true;
    }

    private IEnumerator LoadLevel(int levelIndex)
    {
        yield return null;
        yield return null;
        yield return MoveProgress(0.2f);
        if (!PrepareGameplay(levelIndex, false)) yield break;
        yield return MoveProgress(0.6f);
        if (!PrepareGameplay(levelIndex, true)) yield break;
        yield return MoveProgress(0.9f);
        // Allow Start/LateUpdate and canvas layout to finish behind the cover.
        yield return null;
        yield return null;
        yield return FinishLoading();
    }

    private bool PrepareGameplay(int levelIndex, bool openUI)
    {
        try
        {
            if (openUI) LevelManager.Ins.OnInit();
            else
            {
                LevelManager.Ins.OnDespawn();
                LevelManager.Ins.OnLoadLevel(levelIndex);
            }
            return true;
        }
        catch (System.Exception exception)
        {
            Debug.LogException(exception, this);
            loadingRoutine = null;
            IsLoading = false;
            LoadMainMenu();
            return false;
        }
    }

    private IEnumerator LoadMenu()
    {
        // Let the loading cover render before instantiating the menu.
        yield return null;
        yield return null;
        GameManager.ChangeState(GameState.MainMenu);
        yield return MoveProgress(0.2f);

        UIManager.Ins.CloseAll();
        Mainmenu menu = UIManager.Ins.OpenUI<Mainmenu>();
        if (menu == null)
        {
            Debug.LogError("Mainmenu prefab is missing from UIManager.", this);
            IsLoading = false;
            loadingRoutine = null;
            yield break;
        }
        yield return MoveProgress(0.45f);
        yield return PrepareVideo(menu);
        yield return FinishLoading();
    }

    private IEnumerator FinishLoading()
    {
        yield return MoveProgress(1f);
        yield return new WaitForSecondsRealtime(0.15f);
        loadingCanvas.SetActive(false);
        IsLoading = false;
        loadingRoutine = null;
    }

    private IEnumerator PrepareVideo(Mainmenu menu)
    {
        menuVideo = menu.BackgroundVideo;
        if (menuVideo == null)
        {
            yield return MoveProgress(0.95f);
            yield break;
        }
        firstFrameReady = false;
        videoFailed = false;
        menuVideo.errorReceived += OnVideoError;
        menuVideo.frameReady += OnFrameReady;
        menuVideo.sendFrameReadyEvents = true;
        menuVideo.Stop();
        menuVideo.Prepare();

        float deadline = Time.realtimeSinceStartup + videoTimeout;
        while (!menuVideo.isPrepared && !videoFailed && Time.realtimeSinceStartup < deadline)
        {
            AdvanceProgress(0.78f);
            yield return null;
        }
        if (!menuVideo.isPrepared) videoFailed = true;
        if (!videoFailed)
        {
            yield return MoveProgress(0.8f);
            menuVideo.Play();
            deadline = Time.realtimeSinceStartup + videoTimeout;
            while (!firstFrameReady && !videoFailed && Time.realtimeSinceStartup < deadline)
            {
                AdvanceProgress(0.93f);
                yield return null;
            }
            if (!firstFrameReady) videoFailed = true;
        }
        ReleaseVideoEvents();
        if (videoFailed)
        {
            menuVideo.Stop();
            menu.ShowBackgroundFallback();
            Debug.LogWarning("Menu video could not start; showing the static background.", menu);
        }
        else
        {
            menu.ShowVideoBackground();
            yield return new WaitForSecondsRealtime(videoWarmupTime);
        }
        menuVideo = null;
        yield return MoveProgress(0.95f);
    }

    private IEnumerator MoveProgress(float target)
    {
        while (Progress < target)
        {
            AdvanceProgress(target);
            yield return null;
        }
    }

    private void AdvanceProgress(float target)
    {
        SetProgress(Mathf.MoveTowards(Progress, target, progressSpeed * Time.unscaledDeltaTime));
    }

    private void SetProgress(float value)
    {
        Progress = Mathf.Clamp01(value);
        if (progressSlider != null) progressSlider.SetValueWithoutNotify(Progress);
        if (progressText != null) progressText.text = $"{Mathf.FloorToInt(Progress * 100f)}%";
    }

    private void OnFrameReady(VideoPlayer source, long frame) => firstFrameReady = true;

    private void OnVideoError(VideoPlayer source, string message)
    {
        videoFailed = true;
        Debug.LogWarning($"Menu video: {message}", source);
    }

    private void ReleaseVideoEvents()
    {
        if (menuVideo == null) return;
        menuVideo.errorReceived -= OnVideoError;
        menuVideo.frameReady -= OnFrameReady;
        menuVideo.sendFrameReadyEvents = false;
    }

    private void OnDisable()
    {
        if (loadingRoutine != null) StopCoroutine(loadingRoutine);
        ReleaseVideoEvents();
        menuVideo = null;
        loadingRoutine = null;
        IsLoading = false;
    }
}
