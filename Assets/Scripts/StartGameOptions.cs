using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGameOptions : MonoBehaviour
{
    public void ButtonClickStartGame()
    {
        LevelManager.PlayScene(Scenes.GameBoard);
    }
}
