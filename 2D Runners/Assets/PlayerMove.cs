using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerMove : MonoBehaviour
{

    private Rigidbody2D _rb;
    [SerializeField] private Camera cam;
    [SerializeField] private float offsetX = 10;
    [SerializeField] private float acc = 10;

    // Start is called before the first frame update
    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    private void Update()
    {
        cam.transform.position = new Vector3(transform.position.x + offsetX, 0, -10);
        _rb.AddForce(Vector2.right * acc);
    }
}
