using UnityEngine;
using System.Collections;

public class AudioVaisseau : MonoBehaviour {

    public AudioSource jetSound;
    private float jetPitch;
    private const float LowPitch = .1f;
    private const float HighPitch = 1000f;
    private const float SpeedToRevs = .01f;
    Vector3 myVelocity;
    Rigidbody carRigidbody;

    void Awake()
    {
        carRigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        /*myVelocity = carRigidbody.velocity;
        float forwardSpeed = transform.InverseTransformDirection(carRigidbody.velocity).z;
        float engineRevs = Mathf.Abs(forwardSpeed) * SpeedToRevs;
        jetSound.pitch = Mathf.Clamp(engineRevs, LowPitch, HighPitch);*/

            myVelocity = carRigidbody.velocity;
            float forwardSpeed = transform.InverseTransformDirection(carRigidbody.velocity).z;
            float engineRevs = Mathf.Abs(forwardSpeed) * SpeedToRevs;
            jetSound.pitch = Mathf.Clamp(engineRevs, LowPitch, HighPitch);

    }
   /* public AudioSource waterSound;
    bool enter = true;

    private void OnCollisionEnter(Collider collision)
    {
        if (enter)
        {
            waterSound.Play();
            enter = false;
        }
    }
    private void OnCollisionExit(Collider collision)
    {
        if (collision.gameObject.CompareTag("water"))
        {
            enter = true;
        }
    }
*/
}