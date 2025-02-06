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
    //[SerializeField] private GameObject gameObject;

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
            float x = Random.Range(-180, 180);
            float y = Random.Range(-180, 180);
            float z = Random.Range(-180, 180);
            float xForce = Random.Range(10, 30);
            float yForce = Random.Range(10, 30);
            float zForce = Random.Range(10, 30);
            Vector3 randomTorque = new Vector3(x, y, z);
            Debug.Log(x + " " + y + " " + z);
            rb.AddTorque(randomTorque.x * xForce, randomTorque.y * yForce, randomTorque.z * zForce, ForceMode.Impulse);

            //StartCoroutine(MoveDiceUpAndDown(diceRoll, 0.3f, 0.4f));
            StartCoroutine(WaitAndCheckFace(diceRoll));
        }
        return diceRoll.result;
    }

    /*private IEnumerator MoveDiceUpAndDown(DiceRoll diceRoll, float height, float duration)
    {
        Transform diceTransform = diceRoll.transform;
        Vector3 startPosition = diceTransform.position;
        Vector3 targetPosition = startPosition + Vector3.up * height;

        // Поднимаем кубик вверх
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            diceTransform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        diceTransform.position = targetPosition;

        // Ждём перед падением
        yield return new WaitForSeconds(0.5f);

        // Опускаем кубик вниз
        elapsedTime = 0f;
        while (elapsedTime < duration - 0.2f)
        {
            diceTransform.position = Vector3.Lerp(targetPosition, startPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        diceTransform.position = startPosition;

        // После завершения движения можно проверить верхнюю грань
        diceRoll.SnapToClosestFace(diceRoll, Camera.main.transform.rotation.eulerAngles.x, gameObject.transform.position);
    }*/

    private IEnumerator WaitAndCheckFace(DiceRoll diceRoll)
    {
        yield return new WaitForSeconds(1.0f);

        while (diceRoll.GetComponent<Rigidbody>().angularVelocity.magnitude > 3.0f)
        {
            yield return null; // Ждем, пока кубик не замедлится
        }

        diceRoll.SnapToClosestFace(diceRoll, camera.transform.rotation.eulerAngles.x); // После остановки проверяем верхнюю грань
    }
}
