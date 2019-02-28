using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moveSphere{
    public moveSphere(){
        targetLocation = 0;
        prepareLocation = 0;
        changeDirection = true;
    }
    public string name;
    public Transform sphere;
    //是否开机
    public bool run;
    //电机编号
    public int address;
    //计划目标位置
    public float targetLocation = 0;
    //目标位置(脉冲数)
    public int prepareLocation = 0;
    //零速加速到指定速度的时间 (ms)
    public float speedPlusTime;
    //指定速度减速到零速的时间 (ms)
    public float speedMinusTime;
    //电机指令速度 (m/s)
    public float speed;
    //周期性位置的周期
    public int locationPeriod;
    //电机原始位置
    public float originLocation;
    //电机原始速度
    public float originSpeed;
    //位置属性
    public int locationProperty;
    //运动循环命令次数
    public int cycleIndex;
    //运动循环等待时间
    public int waitingTime;
    //等待时间单位 ms/s
    public int waitingTimeUnit;
    //运动模式 1：速度 2：位置 3：点到点
    public int mode;
    //电机当前速度 (m/s)
    public float currentSpeed=0;
    //停止加速时间 (毫秒时间戳)
    public double stopAccelerateTime;
    //开始减速位置（ms）
    public float startSlowdownPosition;
    //目标速度
    public float targetSpeed;
    //开始加速时间(毫秒时间戳)
    public double startTime;
    //判断是否变向
    public bool changeDirection;
}
