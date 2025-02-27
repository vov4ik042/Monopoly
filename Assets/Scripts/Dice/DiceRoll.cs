using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class DiceRoll : NetworkBehaviour
{
    [SerializeField] private Transform[] edges;
    private Rigidbody rb;
    private GameObject dicePlate;
    private int _result;
    public int result
    {
        get
        {
            return _result;
        }
        private set
        {
            _result = value;
            DiceController.Instance.WriteResultCube();
        }
    }

    public void InitializeDicePlate(GameObject gameObject)
    {
        dicePlate = gameObject;
    }

    public void SnapToClosestFace(DiceRoll diceRoll, float camera)
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // Отключаем физику перед докручиванием

        Vector3 target = RandomEdgeCubik(camera, diceRoll); 
        Quaternion targetRotation = Quaternion.Euler(target.x, target.y, target.z);
        StartCoroutine(RotateSmooth(targetRotation));
    }

    private IEnumerator RotateSmooth(Quaternion targetRotation)
    {
        Quaternion startRotation = transform.rotation;
        float duration = 0.8f; // Плавность
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            t = Mathf.SmoothStep(0, 1, t); // Сглаживание

            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRotation; // Финальное исправление
        rb.isKinematic = false;
    }

    private Vector3 RandomEdgeCubik(float cameraAngl, DiceRoll diceRoll)
    {
        int cameraAngle = (int)cameraAngl;
        float res1 = Mathf.Infinity, res2 = Mathf.Infinity;
        int res1Index = 0, res2Index = 0;
        string result = "";
        float[] mas = new float[6];

        for (int i = 0; i < edges.Length; i++)
        {
            mas[i] = Vector3.Distance(edges[i].position, dicePlate.transform.position);
        }
        for (int i = 0; i < mas.Length; i++)
        {
            float a = mas[i];

            if (a < res1)
            {
                res2 = res1;
                res2Index = res1Index;
                res1 = a;
                res1Index = i;
            }
            // Если текущее значение меньше второго минимального, но больше первого
            else if (a < res2)
            {
                res2 = a;
                res2Index = i;
            }
        }

        res1Index++; res2Index++;
        result += res1Index + "" + res2Index;
        int resultInt = int.Parse(result);
        diceRoll.result = res1Index;//this?
        //Debug.Log($"Выбраны грани: {res1Index}, {res2Index} -> {resultInt}");

        Dictionary<int, Vector3> edgeAnglesDict = new Dictionary<int, Vector3>()
        {
            {12, new Vector3(-165 + cameraAngle, 0, 0)},//1 2 4 //
            {14, new Vector3(-165 + cameraAngle, 0, 90)},//1 4 5
            {15, new Vector3(-165 + cameraAngle, 0, 180)},//1 5 3
            {13, new Vector3(-165 + cameraAngle, 0, 270)},//1 3 2
            {26, new Vector3(-75 + cameraAngle, 0, 0)},//2 6 4 //
            {24, new Vector3(0, 90, -75 + cameraAngle)},//2 4 1
            {21, new Vector3(75 - cameraAngle, 180, 0)},//2 1 3
            {23, new Vector3(0, 270, 75 - cameraAngle)},//2 3 6
            {35, new Vector3(0, 270, -15 - cameraAngle)},//3 5 6 //
            {36, new Vector3(-75 + cameraAngle, 360, -90)},//3 6 2
            {32, new Vector3(0, 90, -165 + cameraAngle)},//3 2 1
            {31, new Vector3(75 - cameraAngle, 180, -90)},//3 1 5
            {46, new Vector3(-75 + cameraAngle, 360, -270)},//4 6 5 //
            {45, new Vector3(0, 90, -345 + cameraAngle)},//4 5 1
            {41, new Vector3(75 - cameraAngle, 180, -270)},//4 1 2
            {42, new Vector3(0, 270, -195 - cameraAngle)},//4 2 6
            {54, new Vector3(0, 270, -105 - cameraAngle)},//5 4 6 //
            {56, new Vector3(-75 + cameraAngle, 0, -180)},//5 6 3
            {53, new Vector3(0, 90, -255 + cameraAngle)},//5 3 1
            {51, new Vector3(75 - cameraAngle, 180, -180)},//5 1 4
            {64, new Vector3(165 - cameraAngle, 180, -270)},//6 4 2 //
            {62, new Vector3(165 - cameraAngle, 180, -360)},//6 2 3
            {63, new Vector3(165 - cameraAngle, 180, -90)},//6 3 5
            {65, new Vector3(165 - cameraAngle, 180, -180)}//6 5 4
        };

        return edgeAnglesDict[resultInt];
    }
}

