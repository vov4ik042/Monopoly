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
    [SerializeField] private Camera camera;

    private GameObject cube1;
    private GameObject cube2;

    private int cubeCount = 1;// Для понимания какой кубик

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
    private void Awake()
    {
        Instance = this;
    }

    public void SpawnCubes()
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
    public IEnumerator DropDiceCoroutine(ulong localClientId)
    {
        MakeResultCubesDefault();

        yield return new WaitForSeconds(0.1f);

        DiceThrow(new NetworkObjectReference(cube1), localClientId);
        DiceThrow(new NetworkObjectReference(cube2), localClientId);

        yield return new WaitUntil(() => cube1.GetComponent<DiceRoll>().GetResult() != 0 && cube2.GetComponent<DiceRoll>().GetResult() != 0);

        WriteResultCubes();
    }

    public void DiceThrow(NetworkObjectReference diceRef, ulong clientId)
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

        diceRoll.SnapToClosestFace(camera.transform.rotation.eulerAngles.x);
    }
}
