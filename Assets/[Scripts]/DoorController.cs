using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum Axis
{
    X, Y, Z
}


public class DoorController : MonoBehaviour, ILevelCompleteInterface
{
    public Axis scaleAxis;
    public float a = 0;
    public float b = 6f;
    public float openTime = 2f;

    public void OnCloseDoors()
    {
        StartCoroutine(CloseDoor());
    }
    public void OnLevelCompleted(int level)
    {
        StartCoroutine(OpenDoor());
    }

    private IEnumerator OpenDoor()
    {
        float t = 0;
        while (t < openTime)
        {

            transform.localScale = scaleAxis switch
            {
                Axis.X => new Vector3(Mathf.Lerp(b, a, t / openTime), 1f, 1f),
                Axis.Y => new Vector3(1f, Mathf.Lerp(b, a, t / openTime), 1f),
                Axis.Z => new Vector3(1f, 1f, Mathf.Lerp(b, a, t / openTime))
            };
            t += Time.deltaTime;
            yield return null;
        }
    }
    private IEnumerator CloseDoor()
    {
        float t = 0;
        while (t < openTime)
        {

            transform.localScale = scaleAxis switch
            {
                Axis.X => new Vector3(Mathf.Lerp(a, b, t / openTime), 1f, 1f),
                Axis.Y => new Vector3(1f, Mathf.Lerp(a, b, t / openTime), 1f),
                Axis.Z => new Vector3(1f, 1f, Mathf.Lerp(a, b, t / openTime))
            };
            t += Time.deltaTime;
            yield return null;
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
