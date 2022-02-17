using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Owner
{
    Player,
    Enemy

}


public class BulletComponent : MonoBehaviour
{

    private Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void FireAt(Vector3 position, Vector3 direction, float speed, float lifeTime)
    {
        transform.position = position;
        GetComponent<Rigidbody>().velocity = direction.normalized * speed;
        transform.rotation = Quaternion.LookRotation(direction);
        transform.rotation *= Quaternion.Euler(90f, 0f, 0f);


        if (lifeTime > 0f)
            Destroy(gameObject, lifeTime);
        else
            Destroy(gameObject, 30f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
