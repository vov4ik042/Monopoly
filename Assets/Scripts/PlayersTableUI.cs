using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayersTableUI : MonoBehaviour
{
    [SerializeField] private Button btnBunkrupt;
    [SerializeField] private Button btnVoteKick;
    [SerializeField] private Button btnLeaveGame;

    private static PlayersTableUI Instance;

    private void Awake()
    {
        Instance = this;
        btnBunkrupt.onClick.AddListener(() =>
        {
            Bunkrupt.Instance.Show();
        });
    }
}
