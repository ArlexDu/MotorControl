using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class MoveController : MonoBehaviour {

    private List<moveSphere> spheres;
    private double PI = 3.141592657;
	// Use this for initialization
	void Start () {
        spheres = new List<moveSphere>();
        //Debug.Log(transform.name);
        //Transform[] gather = GetComponentsInChildren<Transform>();
        int i =1;
        foreach(Transform child in transform){
            moveSphere ms = new moveSphere();
            ms.sphere = child;
            ms.name = child.name;
            spheres.Add(ms);
            i++;
            //Debug.Log(child.name);
        }
	}
	
	// Update is called once per frame
    void Update (){
        foreach (moveSphere sphere in spheres)
        {
            if (!sphere.run) {
                continue;
            }
            if (sphere.mode != 1) {
                long now = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
                
                if (now < sphere.stopAccelerateTime)//加速度
                {
                    double ratio = (now - sphere.startTime)/(sphere.stopAccelerateTime - sphere.startTime);
                    sphere.currentSpeed = Mathf.Lerp(sphere.originSpeed, sphere.targetSpeed, float.Parse(ratio.ToString()));
                }
                else//减速度
                {
                    float ratio = Mathf.Abs(sphere.sphere.transform.position.y - sphere.targetLocation) / Mathf.Abs(sphere.startSlowdownPosition - sphere.targetLocation);
                    sphere.currentSpeed = Mathf.Lerp(sphere.originSpeed, sphere.targetSpeed,ratio);
                    /*if (sphere.name == "Light_02")
                    {
                        //Debug.Log("ratio is " + ratio);
                        Debug.Log("ratio is " + ratio);
                    }*/
                }
                /*if (sphere.name == "Light_01")
                {
                    Debug.Log("sphere.targetLocation is " + sphere.targetLocation);
                }*/
                //Debug.Log("sphere.targetLocation is " + sphere.targetLocation);
                if (sphere.speed > 0)//表示target>location
                {
                    if (sphere.sphere.transform.position.y >= sphere.targetLocation) {
                        //Debug.Log("reset Position1");
                        sphere.currentSpeed = 0;
                        sphere.sphere.transform.position = new Vector3(sphere.sphere.transform.position.x, sphere.targetLocation, sphere.sphere.transform.position.z);
                    }
                }
                else if (sphere.speed <= 0)
                {
                    if (sphere.sphere.transform.position.y <= sphere.targetLocation)
                    {
                        //Debug.Log("reset Position2");
                        sphere.currentSpeed = 0;
                        sphere.sphere.transform.position = new Vector3(sphere.sphere.transform.position.x, sphere.targetLocation, sphere.sphere.transform.position.z);
                    }
                }

                if (sphere.stopAccelerateTime!=0 && now > sphere.stopAccelerateTime && Mathf.Abs(sphere.currentSpeed) <= 0.0001)
                {
                    //Debug.Log("reset Position3");
                    sphere.currentSpeed = 0;
                    sphere.sphere.transform.position = new Vector3(sphere.sphere.transform.position.x, sphere.targetLocation, sphere.sphere.transform.position.z);
                }
                //Debug.Log("address is "+sphere.address);

            }
            sphere.sphere.transform.position = new Vector3(sphere.sphere.transform.position.x, sphere.sphere.transform.position.y + sphere.currentSpeed * Time.deltaTime, sphere.sphere.transform.position.z);
        }
	}

    //设置每个球的速度
    public void setSpeed(int num, int rpm)
    {
        float speed = (float)(rpm * PI * 0.04) / 60*5;
        spheres[num - 1].speed = speed;
    }

    //设置每个球的初始位置
    public void setLocation(int num, int pluse)
    {
        //Debug.Log("set Location");
        float location = (float)(pluse / 200 * PI * 0.04)*5;
        spheres[num - 1].originLocation = location;
        spheres[num - 1].targetLocation = location;
        spheres[num - 1].sphere.transform.position = new Vector3(spheres[num - 1].sphere.transform.position.x, location, spheres[num - 1].sphere.transform.position.z);
    }

    //设置计划目标位置，等获取串口反馈数据后更新为正式目标位置
    public void setPrepareTarget(int num, int pluse) {
        /*if (spheres[num - 1].name == "Light_01")
        {
            Debug.Log("set prepareTarget " + pluse);
        }*/
        spheres[num - 1].prepareLocation = pluse;
    }

    //设置每个球的运动目标位置
    public void setTarget(int num)
    {
        int pluse = spheres[num - 1].prepareLocation;
        float location = (float)(pluse / 200 * PI * 0.04) * 5;
        /*if (spheres[num - 1].name == "Light_01")
        {
            Debug.Log("set Target "+location);
        }*/
        spheres[num - 1].targetLocation = location;
        //更新球改变运动状态前的原始状态
        spheres[num - 1].originLocation = spheres[num - 1].sphere.transform.position.y;
        spheres[num - 1].originSpeed = spheres[num - 1].currentSpeed;
        //Debug.Log("set target num is " + num);
        //Debug.Log("origin Speed is " + spheres[num - 1].originSpeed);
        //Debug.Log("origin Location is " + spheres[num - 1].originLocation);
        //更新速度
        if (spheres[num - 1].targetLocation > spheres[num - 1].originLocation)
        {
            spheres[num - 1].speed = spheres[num - 1].speed > 0 ? spheres[num - 1].speed : (-spheres[num - 1].speed);
        }
        else if (spheres[num - 1].targetLocation < spheres[num - 1].originLocation)
        {
            spheres[num - 1].speed = spheres[num - 1].speed < 0 ? spheres[num - 1].speed : (-spheres[num - 1].speed);
        }
        //Debug.Log("speedPlusTime is " + spheres[num - 1].speedPlusTime);
        float accelerate = (spheres[num - 1].speed - spheres[num - 1].originSpeed) / spheres[num - 1].speedPlusTime;
        //Debug.Log("accelerate is " + accelerate);
        //计算加速时间
        float acceleratDistance = spheres[num - 1].originSpeed * spheres[num - 1].speedPlusTime + 0.5f * accelerate * spheres[num - 1].speedPlusTime * spheres[num - 1].speedPlusTime;
        //Debug.Log("accelerate distance is " + acceleratDistance);
        //设置停止加速时间和开始减速时间
        if (Mathf.Abs(acceleratDistance) > Mathf.Abs(spheres[num - 1].targetLocation - spheres[num - 1].originLocation) / 2.0f)
        {
            float time = Mathf.Sqrt((spheres[num - 1].targetLocation - spheres[num - 1].originLocation) / accelerate);
            //Debug.Log("accelerate time is "+time);
            spheres[num - 1].targetSpeed = spheres[num - 1].originSpeed + accelerate * time;
            spheres[num - 1].startTime = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
            spheres[num - 1].stopAccelerateTime = spheres[num - 1].startTime + time*1000;
            spheres[num - 1].startSlowdownPosition = spheres[num - 1].originSpeed * time + 0.5f * accelerate * time * time;
        }
        else {
            spheres[num - 1].targetSpeed = spheres[num - 1].originSpeed + accelerate * spheres[num - 1].speedPlusTime;
            spheres[num - 1].startTime = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
            spheres[num - 1].stopAccelerateTime = spheres[num - 1].startTime + spheres[num - 1].speedPlusTime*1000;
            spheres[num - 1].startSlowdownPosition = spheres[num - 1].targetLocation - acceleratDistance;
        }
        //Debug.Log("start Time is " + spheres[num - 1].startTime);
        //Debug.Log("stopAccelerateTime is " + spheres[num - 1].stopAccelerateTime);
        //Debug.Log("startSlowdownPosition is " + spheres[num - 1].startSlowdownPosition);
        //Debug.Log("target speed is " + spheres[num - 1].targetSpeed);
        //Debug.Log("target position is " + spheres[num - 1].targetLocation);
    }

    //设置每个球加速时间（ms）
    public void setSpeedPlusTime(int num, int time)
    {
        spheres[num - 1].speedPlusTime = float.Parse(time.ToString())/1000;
        //Debug.Log("speed change time is " + spheres[num - 1].speedPlusTime);
    }

    //设置每个球减速时间（ms）
    public void setSpeedMinusTime(int num, int time)
    {
        spheres[num - 1].speedMinusTime = float.Parse(time.ToString()) / 1000;
    }

    //设置每个球循环次数
    public void setCycleIndex(int num, int cycleIndex)
    {
        spheres[num - 1].cycleIndex = cycleIndex;
    }

    //设置每个球循环等待时间
    public void setWaitingTime(int num, int waitingTime)
    {
        spheres[num - 1].waitingTime = waitingTime;
    }

    //设置每个球循环等待时间单位
    public void setWaitingTimeUnit(int num, int waitingTimeUnit)
    {
        spheres[num - 1].waitingTimeUnit = waitingTimeUnit;
    }

    //设置每个球位置属性
    public void setLocationProperty(int num, int locationProperty)
    {
        spheres[num - 1].locationProperty = locationProperty;
    }

    //设置每个球周期性位置的周期
    public void setLocationPeriod(int num, int locationPeriod)
    {
        spheres[num - 1].locationPeriod = locationPeriod;
    }

    //设置当前的运动模式
    public void setMode(int num, int mode) {
        spheres[num - 1].mode = mode;
    }

    //设置当前的地址
    public void setAddress(int num, int addr)
    {
        spheres[num - 1].address = addr;
    }

    //设置每个球周期性位置的周期
    public void setCoilStatus(int num, bool run)
    {
        spheres[num - 1].run = run;
    }

    //改变当前电机球的颜色
    public void changeSphereColor(int num, bool select) {
        if (select)
        {
            spheres[num - 1].sphere.gameObject.GetComponent<Renderer>().material.color = Color.red;
        }
        else {
            spheres[num - 1].sphere.gameObject.GetComponent<Renderer>().material.color = Color.white;
        }
    }
    //获取灯对象
    public int getLocation(int num)
    {
        return spheres[num-1].prepareLocation;
    }

    //获取灯对象
    public List<moveSphere> getSpheres()
    {
        return spheres;
    }
}
