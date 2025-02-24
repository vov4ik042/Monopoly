using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class DiceController : MonoBehaviour
{
    public static DiceController Instance;
    [SerializeField] private DiceRoll cube1;
    [SerializeField] private DiceRoll cube2;
    [SerializeField] private Camera camera;

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

    public void WriteResultCube()// Запись результатов бросков
    {
        if (cubeCount == 2)
        {
            sumResult = cube1.result + cube2.result;
        }

        cubeCount++;

        if (cubeCount == 3) cubeCount = 1;
    }
    public void DropDice()
    {
        diceThrow(cube1);
        diceThrow(cube2);
    }

    private void diceThrow(DiceRoll diceRoll)
    {
        Rigidbody rb = diceRoll.GetComponent<Rigidbody>();
        if (rb != null)
        {
            float x = Random.Range(-180, 180);
            float y = Random.Range(-180, 180);
            float z = Random.Range(-180, 180);

            float xForce = Random.Range(10, 30);
            float yForce = Random.Range(10, 30);
            float zForce = Random.Range(10, 30);

            Vector3 randomTorque = new Vector3(x, y, z);
            rb.AddTorque(randomTorque.x * xForce, randomTorque.y * yForce, randomTorque.z * zForce, ForceMode.Impulse);

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
