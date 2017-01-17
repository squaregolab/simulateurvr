using UnityEngine;
using System.Collections;

public class Hover : MonoBehaviour {


    public float speed = 90f;
    public float turnSpeed = 5f;
    public float hoverForce = 65f;
    public float hoverHeight = 3.5f;
    private float A;
    private float powerInput;
    private float turnInput;
    private Rigidbody carRigidbody;
    private GameObject[] cheminParcours;
    private bool saut = false;


    void Awake()
    {
        carRigidbody = GetComponent<Rigidbody>();
        cheminParcours = GameObject.FindGameObjectsWithTag("Respawn");
       // saut = false;
    }

    void Update()
    {
        powerInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");
       // A = Input.GetAxis("A");
        bool Reset = Mathf.Abs(Input.GetAxisRaw("A")) > 0.2F || Mathf.Abs(Input.GetAxisRaw("A")) > 0.2F;
        if (Reset)
        {
            Debug.Log("test");
            //Quaternion test = (0,0,0,0);
            transform.position = RespawnProche(cheminParcours).position;
            transform.rotation = Quaternion.identity;
            carRigidbody.velocity = Vector3.zero;
            carRigidbody.angularVelocity = Vector3.zero;
        }

    }

    void FixedUpdate()
    {
        Camera cam = Camera.main;
        Ray ray = new Ray(transform.position, -transform.up);
        RaycastHit hit;
      
        if (saut)
        {
            if (transform.rotation.x > -35)
            {
                float x = transform.rotation.x;
                transform.Rotate(Vector3.right);
            }
            //else saut = false;
        }
        if (Physics.Raycast(ray, out hit, hoverHeight))
        {
            float proportionalHeight = (hoverHeight - hit.distance) / hoverHeight;
            Vector3 appliedHoverForce = Vector3.up * proportionalHeight * hoverForce;
            carRigidbody.AddForce(appliedHoverForce, ForceMode.Acceleration);
        }
        
        Vector3 forwardSpeed = (carRigidbody.transform.forward * speed * powerInput);
        Vector3 force = (forwardSpeed.normalized * carRigidbody.mass * speed * powerInput);
        force = (carRigidbody.drag * force) / (1 - 0.1f * carRigidbody.drag);
        carRigidbody.AddForce(force);

        Quaternion origine = transform.rotation;
        cam.transform.parent.transform.rotation = transform.rotation = origine * Quaternion.AngleAxis(turnInput*turnSpeed, Vector3.up);
        //carRigidbody.AddRelativeForce(0f, 0f, powerInput * speed);
        //carRigidbody.AddRelativeTorque(0f, turnInput * turnSpeed, 0f);


    }
    Transform RespawnProche(GameObject[] respawn)
    {
        Transform tmin = null;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = transform.position;
        foreach (GameObject t in respawn)
        {
            float dist = Vector3.Distance(t.transform.position, currentPos);
            if (dist < minDist)
            {
                tmin = t.transform;
                minDist = dist;
            }
        }
        return tmin;
    }
    private void OnCollisionEnter(Collision collision)
    {
        //transform.rotation = Quaternion.identity;
        carRigidbody.velocity = Vector3.zero;
        carRigidbody.angularVelocity = Vector3.zero;
       
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "saut")
        {
            saut = true;
           // transform.rotation = Quaternion.Euler(-35,-73, 0);
            //transform.position = new Vector3(transform.position.x,transform.position.y,transform.position.z);
            //carRigidbody.constraints = RigidbodyConstraints.None;
            //carRigidbody.constraints = RigidbodyConstraints.FreezeRotationX;
           // Debug.Log("saut");
        }
        else if (other.tag == "FinDeco")
            transform.rotation = Quaternion.Euler(0, 0, 0);
    }
}