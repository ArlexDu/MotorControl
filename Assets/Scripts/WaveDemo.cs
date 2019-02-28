using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
public class WaveDemo : MonoBehaviour {

    private Button btnWave;
    public Controller controller;
    public MoveController moveController;
    public InputField inputLocation;
    public Dropdown address;
    private bool isRunWave;
    public List<moveSphere> spheres;
    public GameObject demoImage;
    // Use this for initialization
    void Start () {
        isRunWave = false;
        btnWave = GameObject.Find("demo").GetComponent<Button>();
        btnWave.onClick.AddListener(wave);
        starts();
        demoImage.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {
        if (isRunWave)
        {
            spheres = moveController.getSpheres();
            foreach (moveSphere sphere in spheres)
            {
                    if (!sphere.run)
                    {
                        continue;
                    }
                if (sphere.name == "Light_01")
                {
                    //Debug.Log("ratio is " + ratio);
                    //Debug.Log("distance is " + Mathf.Abs(sphere.sphere.transform.position.y - sphere.targetLocation));
                }
                //Debug.Log("distance is "+Mathf.Abs(sphere.sphere.transform.position.y - sphere.targetLocation));
                long now = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
                if (sphere.stopAccelerateTime > now) {
                    sphere.changeDirection = true;
                }
                if (sphere.changeDirection && now > sphere.stopAccelerateTime && Mathf.Abs(sphere.sphere.transform.position.y - sphere.targetLocation) < 0.001f)
                {
                    if (sphere.targetSpeed > 0)
                    {
                        //Debug.Log("address is "+sphere.address);
                        controller.updateLocation((sphere.address-1), 0);
                        /*if (sphere.name == "Light_01")
                        {
                            //Debug.Log("ratio is " + ratio);
                            Debug.Log("change direction to 0");
                        }*/
                    }
                    else
                    {
                        //Debug.Log("address is " + sphere.address);
                        controller.updateLocation((sphere.address - 1), 1500);
                        /*if (sphere.name == "Light_01")
                        {
                            //Debug.Log("ratio is " + ratio);
                            Debug.Log("change direction to 1500");
                        }*/
                    }
                    sphere.changeDirection = false;
                }
            }
        }
    }

    //开启波浪运动demo
    private void wave()
    {
        spheres = moveController.getSpheres();
        if (isRunWave)//停止波浪运动demo
        {
            demoImage.SetActive(false);
            isRunWave = false;
        }
        else
        {//开启波浪运动demo
            demoImage.SetActive(true);
            int addr = 0;
            foreach (moveSphere sphere in spheres)
            {
                int time = 2 * addr;
                bool direction = false;//1500到0
                if (sphere.targetLocation == 0) {//0到1500
                    direction = true;
                }
                StartCoroutine(startWave(addr, time,direction));
                addr++;
            }
        }

    }

    private IEnumerator startWave(int addr,int time, bool direction) {
        yield return new WaitForSeconds(time);
        //address.value = addr;
        //address.onValueChanged.Invoke(addr);
        //Debug.Log(address.options[address.value].text);
        controller.startEngine();
        if (direction)
        {
            controller.updateLocation(addr,1500);
        }
        else {
            controller.updateLocation(addr, 1500);
        }
        if (addr == (spheres.Count-1)) {
            isRunWave = true;
        }
    }

    private void starts()
    {
        spheres = moveController.getSpheres();
        int addr = 0;
        foreach (moveSphere sphere in spheres)
        {
            int time = 2 * addr;
            StartCoroutine(startEngines(addr, time));
            addr++;
        }
    }

    private IEnumerator startEngines(int addr, int time)
    {
        yield return new WaitForSeconds(time);
        address.value = addr;
        address.onValueChanged.Invoke(addr);
    }
}
