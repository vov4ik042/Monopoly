using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class DiceRoll : MonoBehaviour
{
    [SerializeField] private Transform[] edges;
    public int result { get; private set; }
    private Rigidbody rb;

    public void SnapToClosestFace(DiceRoll diceRoll, float camera)
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // Отключаем физику перед докручиванием

        /*Vector3 currentRotation = transform.rotation.eulerAngles;

                float targetX = Mathf.Round(currentRotation.x / 54) * 54;
                float targetY = Mathf.Round(currentRotation.y / 150) * 150;
                float targetZ = Mathf.Round(currentRotation.y / 150) * 150;

        float targetX = Mathf.Round(currentRotation.x / 90) * 90;
        float targetY = Mathf.Round(currentRotation.y / 90) * 90;
        float targetZ = Mathf.Round(currentRotation.z / 90) * 90;
        Debug.Log("diceRoll " + diceRoll + "   " + targetX + " " + targetY + " " + targetZ);

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

        int randomNumber = Random.Range(0, 24);
        diceRoll.result = (randomNumber / 4) + 1;
        Debug.Log("diceRoll.result " + diceRoll.result);
        Vector3 target = RandomEdgeCubik(camera, randomNumber); 
        Debug.Log(" targetX " + target.x + " targetY " + target.y + " targetZ " + target.z);
        Quaternion targetRotation = Quaternion.Euler(target.x, target.y, target.z);

        StartCoroutine(RotateSmoothly(targetRotation));
    }

    private IEnumerator RotateSmoothly(Quaternion targetRotation)
    {
        Quaternion startRotation = transform.rotation;
        float duration = 0.4f; // Теперь плавнее
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

    private Vector3 RandomEdgeCubik(float cameraAngl, int number)
    {
        int cameraAngle = (int)cameraAngl;
        Vector3[] edgeAngles = new Vector3[]
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
         };
        return edgeAngles[number];
    }
}

