
using UnityEngine;

public class NoInternetPanel : MonoBehaviour
{
    [SerializeField] private Canvas canvas;

    private void Start()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            canvas.enabled = true;
        }
    }
    public void Retry()
    {
        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            canvas.enabled = false;
        }
    }
}
