using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class move : MonoBehaviour {

    public float speed;
    public float distance;
    private List<moveSphere> spheres;
	// Use this for initialization
	void Start () {
        spheres = new List<moveSphere>();
        Debug.Log(transform.name);
        //Transform[] gather = GetComponentsInChildren<Transform>();
        int i =1;
        foreach(Transform child in transform){
            moveSphere ms = new moveSphere();
            ms.sphere = child;
            ms.speed = speed;
            ms.origin = child.position.y;
            //if(i<=4){
            //    ms.startTime = 0;
            //}else if(i<=8){
            //    ms.startTime = 1;
            //}else if(i<=12){
            //    ms.startTime = 2;
            //}else if(i<=16){
            //    ms.startTime = 3;
            //}
            ms.startTime = i;
            spheres.Add(ms);
            i++;
            Debug.Log(child.name);
        }
	}
	
	// Update is called once per frame
    void Update (){
        foreach(moveSphere sphere in spheres){
            if(sphere.startTime>Time.time){
                continue;
            }
            if(sphere.sphere.transform.position.y>sphere.origin+distance||sphere.sphere.transform.position.y<0){
                sphere.speed *= -1; 
            }
            sphere.sphere.position += new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + sphere.speed * Time.deltaTime, gameObject.transform.position.z);    
        }
	}
}
