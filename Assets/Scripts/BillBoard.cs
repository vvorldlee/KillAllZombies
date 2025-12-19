using UnityEngine;

public class BillBoard : MonoBehaviour
{
    public Transform target;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (target == null)
        {
            target = Camera.main.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (target != null)
        {
            transform.forward = target.forward;
        }
    }
}