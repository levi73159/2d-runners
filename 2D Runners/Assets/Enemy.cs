using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

#if DEBUG

using UnityEditor;

#endif


public class Enemy : MonoBehaviour
{
    public bool explodingImpact = true;
    public float waitTimeTillReset = 0.3f;
    public float explodeForce = 0.1f;

    public bool enableWait = false;
    private Rigidbody2D _rb;
    private Rigidbody2D _playerRb;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    public void OnValidate()
    {
        if (!explodingImpact) return;
        if (_rb != null) return;
        
        _rb = GetComponent<Rigidbody2D>();
        if (_rb != null) return;
        Debug.LogError("The Object must have a rigid body for explode impact!");
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        
        StartCoroutine(Death(other));
    }


    private IEnumerator Death(Collision2D player)
    {
        if (_playerRb == null)
        {
            _playerRb = player.transform.GetComponent<Rigidbody2D>();
            if (_playerRb == null)
            {
                Debug.LogException(new NoNullAllowedException("Player must have a rigid body"), player.gameObject);
                yield break;
            }
        }
    

        // Disable gravity and apply explosion force to both objects
        _playerRb.GetComponent<PlayerMove>().enabled = false;
        _rb.gravityScale = 0.0f;
        _playerRb.gravityScale = 0.0f;

        var oldXVelocity = _playerRb.velocity.x;
        _playerRb.velocity = Vector2.zero;
        _playerRb.constraints = RigidbodyConstraints2D.None;

        const float explosionAngle = 45f; // Set the desired explosion angle in degrees

// Convert the angle to radians for Mathf.Sin and Mathf.Cos
        var angleInRadians = explosionAngle * Mathf.Deg2Rad;

// Calculate the components of the explodeDirection vector based on the angle
        var explodeDirectionX = Mathf.Cos(angleInRadians);
        var explodeDirectionY = Mathf.Sin(angleInRadians);

// Set the components of the explodeDirection vector
        var explodeDirection = new Vector2(explodeDirectionX, explodeDirectionY).normalized;

// Apply the explosive force to the enemy (upward direction)
        _rb.AddForce(explodeDirection * explodeForce, ForceMode2D.Impulse);

// Invert x-component to make the player go in the opposite direction
        explodeDirection.x *= -1;
        explodeDirection.x += oldXVelocity / 12;
        _playerRb.AddForce(explodeDirection * explodeForce, ForceMode2D.Impulse);
        
        // Add rotation torque based on explosion force
        const float rotationTorque = 10f; // Adjust the rotation torque as needed
        _rb.AddTorque(rotationTorque, ForceMode2D.Impulse);
        _playerRb.AddTorque(-rotationTorque, ForceMode2D.Impulse);
        
// Slow down both objects gradually while in the air
        StartCoroutine(SlowDownOverTime());

        if (enableWait)
            yield return new WaitForSeconds(waitTimeTillReset);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    private IEnumerator SlowDownOverTime()
    {
        const float dampingFactor = 0.95f; // Adjust the damping factor as needed
    
        while (_rb.velocity != Vector2.zero || _playerRb.velocity != Vector2.zero)
        {
            _rb.velocity *= dampingFactor;
            _playerRb.velocity *= dampingFactor;
            
            _rb.angularVelocity *= dampingFactor;
            _playerRb.angularVelocity *= dampingFactor;

            yield return null;
        }

        // Re-enable gravity and restore player movement
        _rb.gravityScale = 1f;
        _playerRb.gravityScale = 1f;
        _playerRb.GetComponent<PlayerMove>().enabled = true;
    }

}

#if DEBUG

[CustomEditor(typeof(Enemy))]
public class EnemyEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var t = (Enemy) target;
        t.explodingImpact = EditorGUILayout.Toggle("Enable ExplodingImpact", t.explodingImpact);
        if (t.explodingImpact)
        {
            t.explodeForce = EditorGUILayout.FloatField("ExplodingImpact Force", t.explodeForce);
            t.enableWait = true;
        }

        t.enableWait = EditorGUILayout.Toggle("Enable Wait Time", t.enableWait);
        if (t.enableWait)
        {
            t.waitTimeTillReset = EditorGUILayout.FloatField("Wait Time Till Reset", t.waitTimeTillReset);
        }
    }
}
    
#endif