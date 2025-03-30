using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public enum Scenes
{
    Menu,
    GameBoard,
    Lobby,
    CharacterSelect
};

public class SceneManager : MonoBehaviour
{
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Slider slider;

    private static float _fadeSpeed = 0.02f;
    private static Color _fadeTransperancy = new Color(0, 0, 0, 0.1f);
    private static AsyncOperation _asyncOperation;

    public static SceneManager Instance;
    public GameObject _faderObj;
    public Image _faderImg;
    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        //UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnLevelFinishedLoading;///////////////////////
    }

    public static void PlayScene(Scenes sceneEnum)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneEnum.ToString());
        //Instance.LoadScene(sceneEnum.ToString());////////////////////
    }
    public static void PlaySceneNetwork(Scenes sceneEnum)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(sceneEnum.ToString(), LoadSceneMode.Single);
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

        _asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
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

