using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartObject : MonoBehaviour {
	GameObject player;
	GameObject cat;
	AudioSource catSound;
	AudioClip catOneShotClip;
	AudioSource catOneShotSource;
	// Use this for initialization
	void Start () {
		player=new GameObject("player");
		player.AddComponent<AudioListener>();
		cat=new GameObject("cat");
		catSound=cat.AddComponent<AudioSource>();
		catOneShotSource=cat.AddComponent<AudioSource>();
		catSound.clip=Resources.Load("cat_background") as AudioClip;
		catOneShotClip=Resources.Load("cat_oneshot") as AudioClip;
		catSound.loop=true;
		catSound.Play();
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.Space)){
			catOneShotSource.PlayOneShot(catOneShotClip);
			Debug.Log("test");
		}
	}
}
