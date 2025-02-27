using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class DiceController : NetworkBehaviour
{
    public static DiceController Instance;

    [SerializeField] private GameObject cubePrefab;
    [SerializeField] private GameObject dicePlate1;
    [SerializeField] private GameObject dicePlate2;
    [SerializeField] private Camera camera;

    private GameObject cube1;
    private GameObject cube2;

    private int cubeCount = 1;// Для понимания какой кубик

    private int _sumResult;
    private int sumResult 
    {
        get
        {
            return _sumResult;
        }
        set
        {
            _sumResult = value;
            GameController.Instance.steps = _sumResult;
        }
    }
    private void Awake()
    {
        Instance = this;
    }

    public void SpawnCubes(Transform objCanvas)
    {
        Vector3 position1 = new Vector3(-9.16f, 19.57f, -15.71f);//-871.0f, 399.0f, 54.0f -9.16f, 19.57f, -15.71f
        Vector3 position2 = new Vector3(-8.04f, 19.57f, -15.71f);//-759.0f, 399.0f, 54.0f -8.04f, 19.57f, -15.71f
        Quaternion rotation = Quaternion.Euler(-75, 0, 0);

        cube1 = Instantiate(cubePrefab, position1, rotation);

        /*        cube1 = Instantiate(cubePrefab, objCanvas);

                RectTransform rectTransform1 = cube1.GetComponent<RectTransform>();
                rectTransform1.anchoredPosition = position1;
                rectTransform1.rotation = rotation;*/

        cube1.GetComponent<DiceRoll>().InitializeDicePlate(dicePlate1);
        cube1.GetComponent<NetworkObject>().Spawn();

        Debug.Log("Cube1 spawned");

        cube2 = Instantiate(cubePrefab, position2, rotation);
        /*cube2 = Instantiate(cubePrefab, objCanvas);

        RectTransform rectTransform2 = cube2.GetComponent<RectTransform>();
        rectTransform2.anchoredPosition = position2;
        rectTransform2.rotation = rotation;*/

        cube2.GetComponent<DiceRoll>().InitializeDicePlate(dicePlate2);
        cube2.GetComponent<NetworkObject>().Spawn();

        Debug.Log("Cube2 spawned");
    }

    public void WriteResultCube()// Запись результатов бросков
    {
        if (cubeCount == 2)
        {
            sumResult = cube1.GetComponent<DiceRoll>().result + cube2.GetComponent<DiceRoll>().result;
        }

        cubeCount++;

        if (cubeCount == 3) cubeCount = 1;
    }
    public void DropDice()
    {
        if (!IsHost) return; // Только хост кидает кубик

        diceThrow(cube1.GetComponent<DiceRoll>());
        diceThrow(cube2.GetComponent<DiceRoll>());
    }

    private void diceThrow(DiceRoll diceRoll)
    {
        Rigidbody rb = diceRoll.GetComponent<Rigidbody>();
        if (rb != null)
        {
            float x = Random.Range(-180, 180);
            float y = Random.Range(-180, 180);
            float z = Random.Range(-180, 180);

            float xForce = Random.Range(40, 70);
            float yForce = Random.Range(40, 70);
            float zForce = Random.Range(40, 70);

            Debug.Log("xForce " + xForce + " yForce" + yForce + " zForce" + zForce);

            Vector3 randomTorque = new Vector3(xForce, yForce, zForce);
            //Vector3 randomTorque = new Vector3(x * xForce, y * yForce, z * zForce);

            rb.AddTorque(randomTorque);

            StartCoroutine(WaitAndCheckFace(diceRoll));
        }
    }

    private IEnumerator WaitAndCheckFace(DiceRoll diceRoll)
    {
        yield return new WaitForSeconds(1.0f);

        while (diceRoll.GetComponent<Rigidbody>().angularVelocity.magnitude > 2.8f)
        {
            yield return null; // Ждем, пока кубик не замедлится
        }

        diceRoll.SnapToClosestFace(diceRoll, camera.transform.rotation.eulerAngles.x); // После остановки проверяем грани
    }
}
