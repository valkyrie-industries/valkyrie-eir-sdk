using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valkyrie.EIR;
using Valkyrie.EIR.Interaction;

[RequireComponent(typeof(AudioSource))]
public class Drum : MonoBehaviour
{
    [SerializeField]
    Valkyrie.EIR.Interaction.Interactables.GrabInteractable drumStick;

    AudioSource audioSource;
    Vector3 initScale;


    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        initScale = this.transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    public void MakeSound(float value)
    {
        audioSource.volume = value;
        AudioClip clip = audioSource.clip;
        audioSource.PlayOneShot(clip);
        
        transform.localScale = new Vector3(initScale.x * 1.1f, initScale.y, initScale.z * 1.1f);
        StartCoroutine(ReturnTheScale());

    }

    IEnumerator ReturnTheScale()
    {
        for (int i = 0; i < 10; i++)
        {
            float factor = 1.1f - 0.01f * i;
            transform.localScale = new Vector3(initScale.x * factor, initScale.y, initScale.z * factor);
            yield return new WaitForEndOfFrame();
        }
    }

}
