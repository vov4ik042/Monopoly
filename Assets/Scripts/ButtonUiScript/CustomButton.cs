using UnityEngine;
using UnityEngine.UI;

public class CustomButton : MonoBehaviour
{
    [SerializeField] private int _alpha = 1;

    private void Start()
    {
        GetComponent<Image>().alphaHitTestMinimumThreshold = _alpha;
    }
}
