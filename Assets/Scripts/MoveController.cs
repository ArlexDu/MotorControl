using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveController : MonoBehaviour {

    private List<moveSphere> spheres;
    private double PI = 3.141592657;
    private int mode=3;
	// Use this for initialization
	void Start () {
        spheres = new List<moveSphere>();
        //Debug.Log(transform.name);
        //Transform[] gather = GetComponentsInChildren<Transform>();
        int i =1;
        foreach(Transform child in transform){
            moveSphere ms = new moveSphere();
            ms.sphere = child;
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

            if (mode != 1) {
                if (sphere.speed > 0)//表示target>location
                {
                    if (sphere.location >= sphere.target) {
                        sphere.speed = 0;
                    }
                }
                else
                {
                    if (sphere.location <= sphere.target)
                    {
                        sphere.speed = 0;
                    }
                }
            }
            sphere.sphere.transform.position = new Vector3(sphere.sphere.transform.position.x, sphere.sphere.transform.position.y + sphere.speed * Time.deltaTime, sphere.sphere.transform.position.z);
        }
	}

    //设置每个球的速度
    public void setSpeed(int num, int rpm)
    {
        float speed = (float)(rpm * 2 * PI * 0.04) / 60;
        spheres[num - 1].speed = speed;
        Debug.Log("speed is "+speed);
    }

    //设置每个球的初始位置
    public void setLocation(int num, int pluse)
    {
        float location = (float)(pluse / 400 * 2 * PI * 0.04);
        spheres[num - 1].location = location;
    }

    //设置每个球的运动目标位置
    public void setTarget(int num, int pluse)
    {
        float location = (float)(pluse/400 * 2 * PI * 0.04);
        spheres[num - 1].target = location;
        //更新速度
        if (spheres[num - 1].target > spheres[num - 1].location)
        {
            spheres[num - 1].speed = spheres[num - 1].speed > 0 ? spheres[num - 1].speed : -spheres[num - 1].speed;
        }
        else if (spheres[num - 1].target < spheres[num - 1].location)
        {
            spheres[num - 1].speed = spheres[num - 1].speed < 0 ? spheres[num - 1].speed : -spheres[num - 1].speed;
        }
        else {
            spheres[num - 1].speed = 0;
        }
    }

    //设置每个球加速时间（ms）
    public void setSpeedPlusTime(int num, int time)
    {
        spheres[num - 1].speedPlusTime = time;
    }

    //设置每个球减速时间（ms）
    public void setSpeedMinusTime(int num, int time)
    {
        spheres[num - 1].speedMinusTime = time;
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
    public void setMode(int m) {
        mode = m;
    }

    //设置每个球周期性位置的周期
    public void setCoilStatus(int num, bool run)
    {
        spheres[num - 1].run = run;
    }
}
