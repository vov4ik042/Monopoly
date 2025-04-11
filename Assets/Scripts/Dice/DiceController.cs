using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class DiceController : NetworkBehaviour
{
    public static DiceController Instance;

    [SerializeField] private GameObject cubePrefab;
    [SerializeField] private GameObject dicePlate1;
    [SerializeField] private GameObject dicePlate2;
    [SerializeField] private GameObject diceParent;
    [SerializeField] private Camera cam;

    private GameObject cube1;
    private GameObject cube2;

    private Vector3 baseScale; // Исходный размер кубов
    private int lastScreenWidth;
    private int lastScreenHeight;

    public int _sumResult;
    private int sumResult 
    {
        get
        {
            return _sumResult;
        }
        set
        {
            _sumResult = value;
        }
    }

    private void RememberCubePosition()
    {
        baseScale = cube1.transform.localScale; // Запоминаем базовый размер
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
    }

    private void Update()
    {
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
            UpdateCubes();
        }
    }

    private void UpdateCubes()
    {
        if (cam == null || cube1 == null || cube2 == null) return;

        // Получаем верхний левый угол в мировых координатах
        Vector3 topLeft = cam.ScreenToWorldPoint(new Vector3(0, Screen.height, cam.nearClipPlane + 5));

        // Устанавливаем кубы относительно верхнего левого угла с фиксированным отступом
        float offsetX = 1.0f; // Отступ от левого края
        float offsetY = -1.0f; // Отступ от верхнего края
        cube1.transform.position = topLeft + new Vector3(offsetX, offsetY, 0);
        cube2.transform.position = topLeft + new Vector3(offsetX + 1.0f, offsetY - 1.0f, 0); // Чуть ниже второго

        // Масштабирование кубов относительно ширины экрана
        float scaleFactor = Mathf.Clamp(Screen.width / 1920f, 0.5f, 1f);
        cube1.transform.localScale = baseScale * scaleFactor;
        cube2.transform.localScale = baseScale * scaleFactor;
    }

    private void Awake()
    {
        Instance = this;
    }

    public void CreateCubesUI()
    {
        Vector3 position1 = new Vector3(-9.10f, 19.69f, -15.71f);
        Vector3 position2 = new Vector3(-7.90f, 19.69f, -15.71f);
        Quaternion rotation = Quaternion.Euler(-75, 0, 0);

        cube1 = Instantiate(cubePrefab, position1, rotation);
        cube1.GetComponent<DiceRoll>().InitializeDicePlate(dicePlate1);
        NetworkObject cube1NetworkObject = cube1.GetComponent<NetworkObject>();
        cube1NetworkObject.Spawn();

        cube2 = Instantiate(cubePrefab, position2, rotation);
        cube2.GetComponent<DiceRoll>().InitializeDicePlate(dicePlate2);
        NetworkObject cube2NetworkObject = cube2.GetComponent<NetworkObject>();
        cube2NetworkObject.Spawn();

        RememberCubePosition();
    }

    public void WriteResultCubes()
    {
        GameController.Instance.SetStepsValueServerRpc(cube1.GetComponent<DiceRoll>().GetResult() + cube2.GetComponent<DiceRoll>().GetResult());
    }

    public void MakeResultCubesDefault()
    {
        cube1.GetComponent<DiceRoll>().SetResult(0);
        cube2.GetComponent<DiceRoll>().SetResult(0);
    }
    public IEnumerator DropDiceCoroutine()
    {
        MakeResultCubesDefault();

        yield return new WaitForSeconds(0.1f);

        DiceThrow(new NetworkObjectReference(cube1));
        DiceThrow(new NetworkObjectReference(cube2));

        yield return new WaitUntil(() => cube1.GetComponent<DiceRoll>().GetResult() != 0 && cube2.GetComponent<DiceRoll>().GetResult() != 0);

        WriteResultCubes();
    }

    public void DiceThrow(NetworkObjectReference diceRef)
    {
        if (diceRef.TryGet(out NetworkObject diceNetworkObject))
        {
            DiceRoll diceRoll = diceNetworkObject.GetComponent<DiceRoll>();
            Rigidbody rb = diceRoll.GetComponent<Rigidbody>();
            if (rb != null)
            {
                float xForce = Random.Range(40, 70);
                float yForce = Random.Range(40, 70);
                float zForce = Random.Range(40, 70);

                Vector3 randomTorque = new Vector3(xForce, yForce, zForce);

                rb.AddTorque(randomTorque);
                StartCoroutine(WaitAndCheckFace(diceRoll));//
            }
        }
        else
        {
            Debug.LogError("Failed to get dice NetworkObject");
        }
    }

    private IEnumerator WaitAndCheckFace(DiceRoll diceRoll)
    {
        yield return new WaitForSeconds(1.0f);

        while (diceRoll.GetComponent<Rigidbody>().angularVelocity.magnitude > 2.8f)
        {
            yield return null; // Ждем, пока кубик не замедлится
        }

        diceRoll.SnapToClosestFace(cam.transform.rotation.eulerAngles.x);
    }
}
