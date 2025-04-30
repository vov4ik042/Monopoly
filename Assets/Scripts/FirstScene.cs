using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstScene : MonoBehaviour
{
    void Start()
    {
        SceneManager.PlayScene(Scenes.Menu);
    }
}
