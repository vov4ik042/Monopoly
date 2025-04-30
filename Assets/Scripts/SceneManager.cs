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
    public static SceneManager Instance;
    private void Start()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static void PlayScene(Scenes sceneEnum)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneEnum.ToString());
    }
    public static void PlaySceneNetwork(Scenes sceneEnum)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(sceneEnum.ToString(), LoadSceneMode.Single);
    }
}