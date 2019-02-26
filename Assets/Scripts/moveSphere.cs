using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moveSphere{
    public moveSphere(){}
    public string name;
    public Transform sphere;
    //是否开机
    public bool run;
    //电机编号
    public int address;
    //目标位置
    public float target;
    //零速加速到指定速度的时间 (ms)
    public int speedPlusTime;
    //指定速度减速到零速的时间 (ms)
    public int speedMinusTime;
    //电机指令速度 (m/s)
    public float speed;
    //周期性位置的周期
    public int locationPeriod;
    //电机指定位置
    public float location;
    //位置属性
    public int locationProperty;
    //运动循环命令次数
    public int cycleIndex;
    //运动循环等待时间
    public int waitingTime;
    //等待时间单位 ms/s
    public int waitingTimeUnit;

}
