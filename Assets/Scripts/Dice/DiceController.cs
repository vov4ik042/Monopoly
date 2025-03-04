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

    [ServerRpc(RequireOwnership = false)]
    public void SpawnCubesServeServerRpc()
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

        SendCubesToClientsClientRpc(cube1NetworkObject, cube2NetworkObject, dicePlate1, dicePlate2);
    }

    [ClientRpc]
    private void SendCubesToClientsClientRpc(NetworkObjectReference cube1Ref, NetworkObjectReference cube2Ref,
        NetworkObjectReference dicePlate1Ref, NetworkObjectReference dicePlate2Ref)
    {
        if (cube1Ref.TryGet(out NetworkObject cube1NetworkObject) && cube2Ref.TryGet(out NetworkObject cube2NetworkObject))
        {
            cube1 = cube1NetworkObject.gameObject;
            cube2 = cube2NetworkObject.gameObject;

            cube1.GetComponent<DiceRoll>().InitializeDicePlate(dicePlate1Ref);
            cube2.GetComponent<DiceRoll>().InitializeDicePlate(dicePlate2Ref);
        }
        else
        {
            Debug.LogError("Failed to receive cubes or dice plates on client");
        }
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
        ulong localClientId = NetworkManager.Singleton.LocalClientId;

        DiceThrowServerRpc(new NetworkObjectReference(cube1), localClientId);
        DiceThrowServerRpc(new NetworkObjectReference(cube2), localClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void DiceThrowServerRpc(NetworkObjectReference diceRef, ulong clientId)
    {
        if (diceRef.TryGet(out NetworkObject diceNetworkObject))
        {
            DiceRoll diceRoll = diceNetworkObject.GetComponent<DiceRoll>();
            Rigidbody rb = diceRoll.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Apply torque on the server
                float x = Random.Range(-180, 180);
                float y = Random.Range(-180, 180);
                float z = Random.Range(-180, 180);

                float xForce = Random.Range(40, 70);
                float yForce = Random.Range(40, 70);
                float zForce = Random.Range(40, 70);

                Vector3 randomTorque = new Vector3(xForce, yForce, zForce);
                rb.AddTorque(randomTorque);

                // Notify the client to start the calculation
                StartCalculationClientRpc(diceRef, clientId);
            }
        }
        else
        {
            Debug.LogError("Failed to get dice NetworkObject");
        }
    }

    [ClientRpc]
    private void StartCalculationClientRpc(NetworkObjectReference diceRef, ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId && diceRef.TryGet(out NetworkObject diceNetworkObject))
        {
            DiceRoll diceRoll = diceNetworkObject.GetComponent<DiceRoll>();
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

        diceRoll.SnapToClosestFace(diceRoll, camera.transform.rotation.eulerAngles.x);

        SendResultToServerServerRpc(diceRoll.NetworkObject, diceRoll.result);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendResultToServerServerRpc(NetworkObjectReference diceRef, int result)
    {
        if (diceRef.TryGet(out NetworkObject diceNetworkObject))
        {
            DiceRoll diceRoll = diceNetworkObject.GetComponent<DiceRoll>();
            diceRoll.result = result;

            UpdateResultClientRpc(diceRef, result);
        }
    }

    [ClientRpc]
    private void UpdateResultClientRpc(NetworkObjectReference diceRef, int result)
    {
        if (diceRef.TryGet(out NetworkObject diceNetworkObject))
        {
            DiceRoll diceRoll = diceNetworkObject.GetComponent<DiceRoll>();
            diceRoll.result = result;

            Debug.Log($"Dice result updated to {result} on client");
        }
    }
}
