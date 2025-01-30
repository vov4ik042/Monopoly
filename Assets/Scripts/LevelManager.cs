using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum Scenes
{
    Menu,
    GameBoard
};

public class LevelManager : MonoBehaviour
{
    [SerializeField] private GameObject settingsWindow;
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject startGameOptions;
    [SerializeField] private Slider slider;

    private static float _fadeSpeed = 0.02f;
    private static Color _fadeTransperancy = new Color(0, 0, 0, 0.1f);
    private static AsyncOperation _asyncOperation;

    public static LevelManager Instance;
    public GameObject _faderObj;
    public Image _faderImg;
    void Start()
    {
        //DontDestroyOnLoad(this);
        Instance = this;
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
        //PlayScene(Scenes.Menu);
    }

    public static void PlayScene(Scenes sceneEnum)
    {
        Instance.LoadScene(sceneEnum.ToString());
    }

    private void LoadScene(string sceneName)
    {
        Instance.StartCoroutine(Load(sceneName));
        Instance.StartCoroutine(FadeOut(Instance._faderObj, Instance._faderImg));
    }

    private static IEnumerator FadeOut(GameObject faderObject, Image faderImg)
    {
        faderObject.SetActive(true);
        while (faderImg.color.a < 1)
        {
            faderImg.color += _fadeTransperancy;
            yield return new WaitForSeconds(_fadeSpeed);
        }

        ActivateScene();
    }

    private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        Instance.StartCoroutine(FadeIn(Instance._faderObj, Instance._faderImg));
    }

    private static IEnumerator FadeIn(GameObject faderObject, Image faderImg)
    {
        faderObject.SetActive(true);
        while (faderImg.color.a > 0)
        {
            faderImg.color -= _fadeTransperancy;
            yield return new WaitForSeconds(_fadeSpeed);
        }
        faderObject.SetActive(false);
    }

    private static IEnumerator Load(string sceneName)
    {
        /*asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        _asyncOperation.allowSceneActivation = false;
        yield return _asyncOperation;*/

        _asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        _asyncOperation.allowSceneActivation = false;

        Instance.loadingScreen.SetActive(true);
        /*while (operation.isDone == false)
        {
            float progress = operation.progress;
            Instance.slider.value = progress;
            yield return null;
        }*/

        yield return _asyncOperation;
    }

    private static void ActivateScene()
    {
        _asyncOperation.allowSceneActivation = true;
    }
}
    /*public void Loadlevel(int sceneIndex)
    {
        StartCoroutine(loadAscyncronosly(sceneIndex));
    }

    private IEnumerator loadAscyncronosly(int sceneIndex)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
        loadingScreen.SetActive(true);
        while (operation.isDone == false)
        {
            float progress = operation.progress;
            slider.value = progress;
            yield return null;
        }
    }*/

