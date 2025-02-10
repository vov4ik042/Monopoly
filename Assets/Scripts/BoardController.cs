using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Board : MonoBehaviour
{
    static public Board Instance;
    Transform[] childObjects;
    [SerializeField] private List<Transform> childList = new List<Transform>();
    private void Awake()
    {
        Instance = this;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        FillNodes();
        for (int i = 0; i < childList.Count; i++)
        {
            Vector3 currentPos = childList[i].position;
            if(i > 0)
            {
                Vector3 previousPos = childList[i - 1].position;
                Gizmos.DrawLine(previousPos, currentPos);
            }
        }
    }

    public Vector3 NextPosChild(int current)
    {
        if(current + 1 < childList.Count)
        {
            return childList[current + 1].position;
        }
        return childList[0].position;
    }

    private void FillNodes()
    {
        childList.Clear();

        for (int i = 0;i < transform.childCount;i++)
        {
            childList.Add(transform.GetChild(i));
        }
    }

    public void DefineFildForPlayer(int number)
    {

    }
}
