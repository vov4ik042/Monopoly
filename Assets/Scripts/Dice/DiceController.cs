using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class DiceController : NetworkBehaviour
{
    public static DiceController Instance;

    [SerializeField] private GameObject cubePrefab;
    [SerializeField] private GameObject dicePlate1;
    [SerializeField] private GameObject dicePlate2;
    [SerializeField] private GameObject diceParent;
    [SerializeField] private Camera mainCamera;

    private GameObject cube1;
    private GameObject cube2;

    public float xOff = 0.063f;
    public float xOff1 = 0.122f;
    public float yOff = 0.84f;
    public float distanceFromCamera = 10f; // глубина в мире
    private void Awake()
    {
        Instance = this;
    }
    private Vector2 lastScreenSize;

    private void Start()
    {
        lastScreenSize = new Vector2(Screen.width, Screen.height);
        UpdatePos();
    }

    private void Update()
    {
        if (Screen.width != lastScreenSize.x || Screen.height != lastScreenSize.y)
        {
            lastScreenSize = new Vector2(Screen.width, Screen.height);
            UpdatePos();
        }
    }

    private void UpdatePos()
    {
        Vector3 pos1 = mainCamera.ViewportToWorldPoint(new Vector3(xOff, yOff, distanceFromCamera));
        Vector3 pos2 = mainCamera.ViewportToWorldPoint(new Vector3(xOff1, yOff, distanceFromCamera));

        pos1.z = cube1.transform.position.z;
        pos2.z = cube2.transform.position.z;

        cube1.transform.position = pos1;
        cube2.transform.position = pos2;

        Debug.Log("cameraRightMiddle1:" + pos1);
        Debug.Log("cameraRightMiddle2:" + pos2);

        /*Vector3 viewportPos = mainCamera.WorldToViewportPoint(cube2.transform.position);

        float distanceToLeft = viewportPos.x;           // от 0 до 1
        float distanceToTop = 1f - viewportPos.y;       // от 0 до 1

        Debug.Log("До левого края: " + distanceToLeft);
        Debug.Log("До верхнего края: " + distanceToTop);*/
    }

    public void CreateCubesUI()
    {
        Vector3 position1 = new Vector3(-8.9f, 19.45f, -15.62f);
        //Vector3 position1 = new Vector3(-9.24f, 19.58f, -15.62f);
        Vector3 position2 = new Vector3(-7.8f, 19.45f, -15.62f);
        //Vector3 position2 = new Vector3(-7.956f, 19.58f, -15.62f);
        Quaternion rotation = Quaternion.Euler(-75, 0, 0);

        cube1 = Instantiate(cubePrefab, position1, rotation);
        cube1.GetComponent<DiceRoll>().InitializeDicePlate(dicePlate1);
        NetworkObject cube1NetworkObject = cube1.GetComponent<NetworkObject>();
        cube1NetworkObject.Spawn();

        cube2 = Instantiate(cubePrefab, position2, rotation);
        cube2.GetComponent<DiceRoll>().InitializeDicePlate(dicePlate2);
        NetworkObject cube2NetworkObject = cube2.GetComponent<NetworkObject>();
        cube2NetworkObject.Spawn();

        WriteCubesClientRpc(new NetworkObjectReference(cube1), new NetworkObjectReference(cube2));
    }
    [ClientRpc]
    private void WriteCubesClientRpc(NetworkObjectReference networkObjectReference1, NetworkObjectReference networkObjectReference2)
    {
        if (networkObjectReference1.TryGet(out NetworkObject Netcube1))
        {
            cube1 = Netcube1.gameObject;
        }
        if (networkObjectReference2.TryGet(out NetworkObject Netcube2))
        {
            cube2 = Netcube2.gameObject;
        }
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

        diceRoll.SnapToClosestFace(mainCamera.transform.rotation.eulerAngles.x);
    }
}
