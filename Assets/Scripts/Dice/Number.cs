using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Number : MonoBehaviour
{
    [SerializeField] private GameObject cube;
    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "Table")
        {
            Result(gameObject.name);
        }
    }

    private void Result(string name)
    {
        /*switch (name)
        {
            case "1": cube.GetComponent<DiceRoll>().result = 6; break;
            case "2": cube.GetComponent<DiceRoll>().result = 5; break;
            case "3": cube.GetComponent<DiceRoll>().result = 4; break;
            case "4": cube.GetComponent<DiceRoll>().result = 3; break;
            case "5": cube.GetComponent<DiceRoll>().result = 2; break;
            case "6": cube.GetComponent<DiceRoll>().result = 1; break;
        }*/
    }
}
