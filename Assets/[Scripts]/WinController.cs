using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinController : MonoBehaviour
{
    // Start is called before the first frame update
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            MenuController.Instance.OnAction((int)Action.Victory);
        }
    }
}
