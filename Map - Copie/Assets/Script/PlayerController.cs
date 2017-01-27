using System.Collections;
using UnityEngine.UI; //Pour le texte
using System.Collections.Generic;
using UnityEngine;
//using UnityStandardAssets.Utility;
//using UnityEngine;

public class PlayerController : MonoBehaviour
{

    private Rigidbody rb;
    private float vertical, horizontal;
    
    //Valeur des sticks
    private float moveRotation;
    private float moveVertical;
	private float manetteRot;
    private float gachetteAr;
    private float Boost;
    private float moveBas;
    private float RotBas;
    private Vector3 Zero;
    //Transform target;
    private float RotDroite;
    private float stick2dg;
    private float stick2hb;
    
	/////////////////
    //Parcours
    private bool start;
	private int malusTemps = 0;
	public int NombreParcours;
	private float temps;
	private GameObject[] cheminParcours;
	public GameObject Depart;
	public GameObject finish;
	private GameObject[] respawn;
	/////////////////
    //Multiplicateur de vitesse
    public float speed;
    public float hoverHeight, hoverForce;
    public float TurnSpeed;
    public GameObject PluieMete, PluieMete2;
	/////////////////
	///Texte Vaisseau
	public TextMesh text;
    public TextMesh Vitesse;
	/////////////////

	//Accelerateur
	private UnitySerialPort unitySerialPort;
	public static PlayerController Instance;
	private float accelerateur;

	/////////////////
    /* public GameObject Boussole;*/

    //public Text WinText;

    void Awake()
    {
		
        //Recupere le rb de l'objet
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        start = false;
		Instance = this;
        cheminParcours = GameObject.FindGameObjectsWithTag("Parcours");
		respawn = GameObject.FindGameObjectsWithTag("Respawn");
        //score = 0;
        /*countText.text = "Score : " + countText.ToString();
        WinText.text = "";
        SetCountText();*/

    }
	void Start()
	{
		unitySerialPort = UnitySerialPort.Instance;
	}
    void Update()
    {
		/*if (Input.GetKeyDown ("f"))
			Application.Quit ();*/
		if (Input.GetKeyDown ("f"))
			Application.Quit ();
		if (unitySerialPort.RawData != "Ready")
		accelerateur = float.Parse(unitySerialPort.RawData);
	Vitesse.text = System.Math.Round(Mathf.Abs(rb.velocity.magnitude),0).ToString();
        /*  for(int i = 0; i<cheminParcours.Length;i++)
              if (cheminParcours[i].activeSelf)
              {
                  Boussole.transform.rotation = Quaternion.Euler(cheminParcours[i].transform.position.y, 0, 0);
              }*/
        //Recupere valeur// 
        moveRotation = Input.GetAxisRaw("Horizontal");
        moveVertical = Input.GetAxisRaw("Vertical");
        gachetteAr = Input.GetAxisRaw("A");
        Boost = Input.GetAxisRaw("B");
        moveBas = Input.GetAxisRaw("Y");
        RotBas = Input.GetAxisRaw("X");
		manetteRot = Input.GetAxisRaw ("LTRT");
        //Transform target;
        //  RotDroite = Input.GetAxisRaw("LTRT");
        stick2dg = Input.GetAxisRaw("Stick2");
        stick2hb = Input.GetAxisRaw("Stick2hb");
        
           if(start)
           {
               temps += Time.deltaTime;
               text.text = temps.ToString().Substring(0,5);
           }
    }


    void LateUpdate()
    {

    }
    // Update physics
    void FixedUpdate()
    {
        deplacement();
        Ray rayBas = new Ray(transform.position, -transform.up);
        RaycastHit hit;
        if (Physics.Raycast(rayBas, out hit, hoverHeight))
        {
            float proportionalHeight = (hoverHeight - hit.distance) / hoverHeight;
            Vector3 appliedHoverForce = Vector3.up * proportionalHeight * hoverForce;
            rb.AddForce(appliedHoverForce, ForceMode.Acceleration);
        }
		bool gachette = Mathf.Abs(Input.GetAxis("Y")) > 0.2F || Mathf.Abs(Input.GetAxis("Y")) > 0.2F;
		if (gachette) {
			foreach (GameObject i in cheminParcours) {
				i.SetActive(true);
			}

			Depart.SetActive (true);
			finish.SetActive (true);
		}
    }
    void deplacement()
    {
        Camera cam = Camera.main;
        //////////////////////////////////
        //Mouvement//

        //Si les stick de déplacemebnt sont utilisé le bool est vrai
        bool stickMove = Mathf.Abs(Input.GetAxis("Vertical")) > 0.2F || Mathf.Abs(Input.GetAxis("Vertical")) > 0.2F;
        bool stickVue = Mathf.Abs(Input.GetAxis("Stick2")) > 0.2F || Mathf.Abs(Input.GetAxis("Stick2")) > 0.2F;
		bool boostPressed = Mathf.Abs(Input.GetAxis("B")) > 0.2F || Mathf.Abs(Input.GetAxis("B")) > 0.2F;
		bool b_gachetteAr = Mathf.Abs(Input.GetAxis("A")) > 0.2F || Mathf.Abs(Input.GetAxis("A")) > 0.2F;
	//Quaternion origine;
        //rotation Droite Gauche
		if (b_gachetteAr) {
			rb.velocity = Vector3.zero;;
			rb.angularVelocity = Vector3.zero;
			Transform tf = RespawnProche (respawn);
			transform.position = tf.position;
			transform.rotation = tf.rotation;
		}
		
       Quaternion origine = transform.rotation;
		cam.transform.parent.transform.rotation = transform.rotation = origine * Quaternion.AngleAxis(stick2hb/2+manetteRot*TurnSpeed, Vector3.up);

   /*     //HautBas
        rb.AddForce(0f, moveRotation * speed, 0f);
        rb.AddForce(0f, (-1) * moveRotation * speed, 0f);
		*/
        //Rotation Bas
        /*origine = transform.rotation;
        transform.rotation = origine * Quaternion.AngleAxis(RotBas, Vector3.right);
        
        //Rotation Haut
        origine = transform.rotation;
        transform.rotation = origine * Quaternion.AngleAxis(-1*RotHaut, Vector3.right);
       */
        //Rotation Droite
        /*   origine = transform.rotation;
           cam.transform.parent.transform.rotation = transform.rotation = origine * Quaternion.AngleAxis(RotDroite, Vector3.forward);
        */
        //Penche Droite/Gauche manette

		//Fonction
		origine = transform.rotation;
		cam.transform.parent.transform.rotation = transform.rotation = origine * Quaternion.AngleAxis(-1 * moveVertical*TurnSpeed, Vector3.right);
        //transform.Rotate(stick2hb * TurnSpeed,stick2dg*TurnSpeed,0);
        //Rotation Haut/Bas manette
        origine = transform.rotation;
		cam.transform.parent.transform.rotation = transform.rotation = origine * Quaternion.AngleAxis(moveRotation*TurnSpeed, Vector3.forward);
        //////////////////////////////////////stick2dg
        //Avance 
        //double mv = System.Math.Round(moveVertical, 1);
	
		float mv = accelerateur/100;
        
        mv = mv * 10;

            Vector3 forwardSpeed = (rb.transform.forward * speed * mv);
            Vector3 force = (forwardSpeed.normalized * rb.mass * speed * mv);
            force = (rb.drag * force) / (1 - 0.0001f * rb.drag);
		if (boostPressed) rb.AddForce (force * 2);
		else rb.AddForce(force);

       // Debug.Log(rb.velocity.magnitude);
        /*Vector3 force = rb.transform.forward*speed*mv;
        //force = (rb.drag * force.normalized) / (1 - 0.0001f * rb.drag)*100;
        Debug.Log(force);
        rb.AddForce(force); */

    }
    //Si on percute un objet, other et l'objet en question
    private void OnTriggerEnter(Collider other)
    {
        //Si l'objet est bien un pickUp l'efface mais le detruit pas
       
        if (other.gameObject.CompareTag("Start") && !start)
        {
			temps = 0;
            other.gameObject.SetActive(false);
            // temps = Time.realtimeSinceStartup;
            start = true;
            //  score++;
            //   SetCountText();
        }
        if (other.gameObject.CompareTag("Parcours") && start)
        {
			malusTemps++;
            other.gameObject.SetActive(false);
            // temps = Time.realtimeSinceStartup;
            //  score++;
            //   SetCountText();
        }
        else if (other.gameObject.CompareTag("Finish") && start)
        {
			malusTemps = NombreParcours - malusTemps;
			temps = 5 * malusTemps + temps;
			text.text = temps.ToString().Substring(0,5);
			malusTemps = 0;
			//Debug.Log ("Malus : " +malusTemps + " Temps " + temps);
            other.gameObject.SetActive(false);
            //  temps = temps - Time.realtimeSinceStartup;
            start = false;
        }
    }
    private void OnCollisionExit(Collision collider)
    {
        //Debug.Log(collider.relativeVelocity.magnitude);
        if(collider.relativeVelocity.magnitude > 2)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;      
        }
        
    }
	private void OnTriggerExit(Collider other)
	{
		if (other.tag == "Map") {
			Transform tf = RespawnProche (respawn);
			rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
			transform.position = tf.position;
			transform.rotation = tf.rotation;
		}
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
}