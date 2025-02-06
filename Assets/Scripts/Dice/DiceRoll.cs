using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class DiceRoll : MonoBehaviour
{
    [SerializeField] private Transform[] edges;
    [SerializeField] private GameObject dicePlate;
    public int result { get; private set; }
    private Rigidbody rb;

    /*private void OnDrawGizmos()
    {
        if (edges[0] == null || dicePlate == null) return;

        // Получаем угол между ними
        float angle = Vector3.Angle(edges[0].up, Vector3.up);
        float angle1 = Vector3.Angle(edges[1].up, Vector3.up);
        float angle2 = Vector3.Angle(edges[2].up, Vector3.up);
        float angle3 = Vector3.Angle(edges[3].up, Vector3.up);
        float angle4 = Vector3.Angle(edges[4].up, Vector3.up);
        float angle5 = Vector3.Angle(edges[5].up, Vector3.up);
        // Вектор направления для каждой трансформации
        Vector3 edgeDirection = edges[0].forward;
        Vector3 diceDirection = Vector3.up;

        // Рисуем линии в сцене
        Gizmos.color = Color.red;
        Gizmos.DrawRay(edges[0].position, edgeDirection * 2);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(Vector3.up, diceDirection * 2);

        Debug.Log("angle 1 " + angle);
        Debug.Log("angle 2 " + angle1);
        Debug.Log("angle 3 " + angle2);
        Debug.Log("angle 4 " + angle3);
        Debug.Log("angle 5 " + angle4);
        Debug.Log("angle 6 " + angle5);
    }*/

    public void SnapToClosestFace(DiceRoll diceRoll, float camera)
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // Отключаем физику перед докручиванием

        /* Vector3 currentRotation = transform.rotation.eulerAngles;
         float targetX = Mathf.Round(currentRotation.x / 90) * 90;
         float targetY = Mathf.Round(currentRotation.y / 90) * 90;
         float targetZ = Mathf.Round(currentRotation.z / 90) * 90;
         //Debug.Log("diceRoll " + diceRoll + "   " + targetX + " " + targetY + " " + targetZ);

         if (targetY == 0 || targetY == 360)
         {
             //Debug.Log("Y = 0 X = " + targetX);
             targetX -= 30;
         }
         if (targetY == 90)
         {
             //Debug.Log("Y = 90 Z = " + targetZ);
             targetZ += 60;
         }
         if (targetY == 270)
         {
             //Debug.Log("Y = 270 Z = " + targetZ);
             targetZ -= 60;
         }
         if (targetY == 180)
         {
             //Debug.Log("Y = 180 X = " + targetX);
             targetX -= 60;
         }

         Debug.Log(" targetX " + targetX + " targetY " + targetY + " targetZ " + targetZ);*/
 /*       int randomNumber = Random.Range(0, 24);
        diceRoll.result = (randomNumber / 4) + 1;
        Debug.Log("diceRoll.result " + diceRoll.result);
        Vector3 target = RandomEdgeCubik(camera, randomNumber);*/

        Vector3 target = RandomEdgeCubik(camera); 
        Quaternion targetRotation = Quaternion.Euler(target.x, target.y, target.z);
        StartCoroutine(RotateSmooth(targetRotation));
    }

    private IEnumerator RotateSmooth(Quaternion targetRotation)
    {
        Quaternion startRotation = transform.rotation;
        float duration = 0.8f; // Теперь плавнее
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            t = Mathf.SmoothStep(0, 1, t); // Добавляем сглаживание

            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRotation; // Финальное исправление
        rb.isKinematic = false; // Включаем физику обратно
    }

    private Vector3 RandomEdgeCubik(float cameraAngl)
    {
        int cameraAngle = (int)cameraAngl;
        float res1 = Mathf.Infinity, res2 = Mathf.Infinity;
        int res1Index = 0, res2Index = 0;
        string result = "";
        float[] mas = new float[6];

        for (int i = 0; i < edges.Length; i++)
        {
            mas[i] = Vector3.Distance(edges[i].position, dicePlate.transform.position);
            //mas[i] = Vector3.Angle(edges[i].up, Vector3.up);
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
            //Debug.Log(mas[i]);
        }

        res1Index++; res2Index++;
        result += res1Index + "" + res2Index;
        int resultInt = int.Parse(result);
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

        /*Vector3[] edgeAngles = new Vector3[]
         {
            new Vector3(-165 + cameraAngle, 0, 0),   // 1
            new Vector3(-165 + cameraAngle, 0, 90),  // 2
            new Vector3(-165 + cameraAngle, 0, 180), // 3
            new Vector3(-165 + cameraAngle, 0, 270), // 4
            new Vector3(-75 + cameraAngle, 0, 0),    // 5
            new Vector3(0, 90, -75 + cameraAngle),   // 6
            new Vector3(75 - cameraAngle, 180, 0),   // 7
            new Vector3(0, 270, 75 - cameraAngle),   // 8
            new Vector3(0, 270, -15 - cameraAngle),  // 9
            new Vector3(-75 + cameraAngle, 360, -90), // 10
            new Vector3(0, 90, -165 + cameraAngle),  // 11
            new Vector3(75 - cameraAngle, 180, -90), // 12
            new Vector3(-75 + cameraAngle, 360, -270), // 13
            new Vector3(0, 90, -345 + cameraAngle),  // 14
            new Vector3(75 - cameraAngle, 180, -270), // 15
            new Vector3(0, 270, -195 - cameraAngle), // 16
            new Vector3(0, 270, -105 - cameraAngle), // 17
            new Vector3(-75 + cameraAngle, 0, -180), // 18
            new Vector3(0, 90, -255 + cameraAngle),  // 19
            new Vector3(75 - cameraAngle, 180, -180), // 20
            new Vector3(165 - cameraAngle, 180, -270), // 21
            new Vector3(165 - cameraAngle, 180, -360), // 22
            new Vector3(165 - cameraAngle, 180, -90),  // 23
            new Vector3(165 - cameraAngle, 180, -180)  // 24
         };*/
        return edgeAnglesDict[resultInt];
    }
}

