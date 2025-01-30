using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayersChoiceobjects : MonoBehaviour
{
    /*public GameObject[] availableFigures; // Префабы фигур
    private int selectedFigureIndex = 0;

    public void SelectFigure(int index)
    {
        selectedFigureIndex = index;
    }

    public void ConfirmSelection()
    {
        GameObject figurePrefab = availableFigures[selectedFigureIndex];

        GameObject playerObject = Instantiate(figurePrefab, new Vector3(0, 0, 0), Quaternion.identity);
        Player playerComponent = playerObject.AddComponent<Player>();

        playerComponent.playerID = ControllerPlayer.Instance.players.Count + 1;
        playerComponent.playerName = "Игрок " + playerComponent.playerID;
        Debug.Log(playerComponent.playerName);

        ControllerPlayer.Instance.AddPlayer(playerComponent);
    }*/
}
