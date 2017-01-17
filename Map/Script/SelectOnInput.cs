using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectOnInput : MonoBehaviour {

    public EventSystem eventSystem;
    public GameObject selectedObject;
    public AudioClip Sound;
    private bool buttonSelected;
    private AudioSource source;
    // Use this for initialization
	void Awake () {
        Cursor.visible = false;
        source = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        bool stickMove = Mathf.Abs(Input.GetAxis("Vertical")) > 0.2F;
        if ( stickMove && buttonSelected == false)
        {
            eventSystem.SetSelectedGameObject(selectedObject);
            source.PlayOneShot(Sound,1F);
            buttonSelected = true;
            

        }
       
	}
   
    private void OnDisable()
    {
        buttonSelected = false;
       
    }
}
