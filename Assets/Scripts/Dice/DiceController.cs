using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class DiceController : MonoBehaviour
{
    public static DiceController Instance;
    //[SerializeField] private GameObject prefab;
    [SerializeField] private DiceRoll cube1;
    [SerializeField] private DiceRoll cube2;
    [SerializeField] private Camera camera;

    //private float forceMagnitude = 1f;
    public int sumResult { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    /*public void CreateCubes()
    {
        cube1 = Instantiate(prefab, new Vector3(0, 0, -2), Quaternion.identity);
        cube2 = Instantiate(prefab, new Vector3(0, 0, 2), Quaternion.identity);
    }*/
    public int startThrow()
    {
        if (cube1 == null)
        {
            //CreateCubes();
        }

        //cube1.gameObject.GetComponent<DiceRoll>().result = 0;
        //cube2.gameObject.GetComponent<DiceRoll>().result = 0;
        int a = diceThrow(cube1);
        int b = diceThrow(cube2);
        return a + b;
    }
    private int diceThrow(DiceRoll diceRoll)
    {
        Rigidbody rb = diceRoll.GetComponent<Rigidbody>();
        if (rb != null)
        {
            float x = Random.Range(60, 120);
            float y = Random.Range(60, 120);
            float z = Random.Range(60, 120);
            float xForce = Random.Range(1, 11);
            float yForce = Random.Range(1, 11);
            float zForce = Random.Range(1, 11);
            Vector3 randomTorque = new Vector3(x, y, z);

            rb.AddTorque(randomTorque.x * xForce, randomTorque.y * yForce, randomTorque.z * zForce, ForceMode.Impulse);

            StartCoroutine(WaitAndCheckFace(diceRoll));
        }
        return diceRoll.result;
    }
    private IEnumerator WaitAndCheckFace(DiceRoll diceRoll)
    {
        yield return new WaitForSeconds(1.0f);

        while (diceRoll.GetComponent<Rigidbody>().angularVelocity.magnitude > 10.0f)
        {
            yield return null; // Ждем, пока кубик не замедлится
        }

        diceRoll.SnapToClosestFace(diceRoll, camera.transform.rotation.eulerAngles.x); // После остановки проверяем верхнюю грань
    }
}
