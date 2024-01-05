using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerMove : MonoBehaviour
{

    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask whatsGround;
    [SerializeField] private float offsetX = 10;
    [SerializeField] private float acc = 10;
    [SerializeField] private float jumpForce = 10;
    [SerializeField] private float groundDis = 0.1f;
    
    // Counters
    [SerializeField] private float coyoteTime = 0.2f;
    private float _coyoteTimeCounter;
    [SerializeField] private float jumpBufferTime = 0.2f;
    private float _jumpBufferCounter;
    
    private Rigidbody2D _rb;
    private BoxCollider2D _collider;
    private bool _grounded;

    // Start is called before the first frame update
    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<BoxCollider2D>();
    }

    private void OnDrawGizmosSelected()
    {
        if (_collider == null)
            _collider = GetComponent<BoxCollider2D>();
        
        Gizmos.color = IsGrounded() ? Color.green : Color.magenta;
        var bounds = _collider.bounds;
        Gizmos.DrawWireCube(bounds.center + Vector3.down * groundDis, new Vector3(bounds.size.x, bounds.size.y + groundDis, 1));
    }

    private bool IsGrounded()
    {
        var bounds = _collider.bounds;
        var hit = Physics2D.BoxCast(bounds.center, bounds.size, 0, Vector2.down, groundDis, whatsGround);
        if (hit.transform == null) return false;
        
        return true;
    }


    private void Update()
    {
        cam.transform.position = new Vector3(transform.position.x + offsetX, 0, -10);
        var bounds = _collider.bounds;
        var hit = Physics2D.BoxCast(bounds.center, bounds.size, 0, Vector2.down, groundDis, whatsGround);

        if (hit.collider != null)
        {
            _grounded = true;
            _coyoteTimeCounter = coyoteTime;
        }

        if (!_grounded)
        {
            _coyoteTimeCounter -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        _rb.AddForce(Vector2.right * (acc * Time.deltaTime));
        
        Jump();
    }

    private bool _jump;
    private void Jump()
    {
        if (Input.GetButtonDown("Jump"))
        {
            _jumpBufferCounter = jumpBufferTime;
            _jump = true;
        }
        else
        {
            _jumpBufferCounter -= Time.fixedDeltaTime;
        }
        
        if (_coyoteTimeCounter > 0f  && _jumpBufferCounter > 0f)
        {
            _grounded = false;
            _rb.velocity = new Vector2(_rb.velocity.x, jumpForce);
            _jumpBufferCounter = 0f;
        }

        if (_jump && Input.GetButtonUp("Jump") && _rb.velocity.y > 0f)
        {
            var velocity = _rb.velocity;
            velocity = new Vector2(velocity.x, velocity.y * 0.5f );
            _rb.velocity = velocity;

            _coyoteTimeCounter = 0f;
        }
    }
}
