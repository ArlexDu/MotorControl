using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System.Threading;
using UnityEngine.UI;

public class Controller : MonoBehaviour {

    private Button btnSpeedPlusTime, btnSpeedMinusTime, btnSpeed, btnLocationPeriod, btnLocation, btnProperty, btnCycleIndex, btnWaitingTime,
        btnWaitingTimeUnit, btnStart, btnEnd, btnSave;
    private InputField inputSpeedPlusTime, inputSpeedMinusTime, inputSpeed, inputLocationPeriod, inputLocation, inputProperty, inputCycleIndex,
        inputWaitingTime, inputWaitingTimeUnit;
    public MessageManagement messageManagement;

    private Dropdown addressList;

    private Toggle speedMode, locationMode, pointMode;

    //地址字典
    private Dictionary<string, byte> addrs = new Dictionary<string, byte>();

	// Use this for initialization
	void Start () {
        //初始化button
        btnSpeedPlusTime = GameObject.Find("speedPlusTimeApply").GetComponent<Button>();
        btnSpeedMinusTime = GameObject.Find("speedMinusTimeApply").GetComponent<Button>();
        btnSpeed = GameObject.Find("speedApply").GetComponent<Button>();
        btnLocationPeriod = GameObject.Find("locationPeriodApply").GetComponent<Button>();
        btnLocation = GameObject.Find("locationApply").GetComponent<Button>();
        btnProperty = GameObject.Find("propertyApply").GetComponent<Button>();
        btnCycleIndex = GameObject.Find("cycleIndexApply").GetComponent<Button>();
        btnWaitingTime = GameObject.Find("waitingTimeApply").GetComponent<Button>();
        btnWaitingTimeUnit = GameObject.Find("waitingTimeUnitApply").GetComponent<Button>();

        //初始化inputfiled
        inputSpeedPlusTime = GameObject.Find("speedPlusTimeValue").GetComponent<InputField>();
        inputSpeedMinusTime = GameObject.Find("speedMinusTimeValue").GetComponent<InputField>();
        inputSpeed = GameObject.Find("speedValue").GetComponent<InputField>();
        inputLocationPeriod = GameObject.Find("locationPeriodValue").GetComponent<InputField>();
        inputLocation = GameObject.Find("locationValue").GetComponent<InputField>();
        inputProperty = GameObject.Find("propertyValue").GetComponent<InputField>();
        inputCycleIndex = GameObject.Find("cycleIndexValue").GetComponent<InputField>();
        inputWaitingTime = GameObject.Find("waitingTimeValue").GetComponent<InputField>();
        inputWaitingTimeUnit = GameObject.Find("waitingTimeUnitValue").GetComponent<InputField>();

        //初始化功能按钮
        btnStart = GameObject.Find("start").GetComponent<Button>();
        btnEnd = GameObject.Find("end").GetComponent<Button>();
        btnSave = GameObject.Find("save").GetComponent<Button>();

        //初始化模式选择框
        speedMode = GameObject.Find("speedMode").GetComponent<Toggle>();
        locationMode = GameObject.Find("locationMode").GetComponent<Toggle>();
        pointMode = GameObject.Find("pointMode").GetComponent<Toggle>();

        //初始化地址字典
        addrs.Add("01", 0x01);
        addrs.Add("02", 0x02);
        addrs.Add("03", 0x03);
        addrs.Add("04", 0x04);

        addressList = GameObject.Find("address").GetComponent<Dropdown>();
        updateDropDownItem();
        bindEvent();

    }

    //绑定点击事件
    private void bindEvent()
    {
        btnStart.onClick.AddListener(startEngine);
        btnEnd.onClick.AddListener(endEngine);
        btnSpeedPlusTime.onClick.AddListener(setSpeedPlusTime);
        btnSpeedPlusTime.onClick.AddListener(setSpeedMinusTime);
        btnSpeed.onClick.AddListener(setSpeed);
        btnLocationPeriod.onClick.AddListener(setLocationPeriod);

        btnProperty.onClick.AddListener(setLocationProperty);
        btnCycleIndex.onClick.AddListener(setCycleIndex);
        btnWaitingTime.onClick.AddListener(setWaitingTime);
        btnWaitingTimeUnit.onClick.AddListener(setWaitingTimeUnit);


        speedMode.onValueChanged.AddListener((bool value) => setSpeedMode(value));
        locationMode.onValueChanged.AddListener((bool value) => setLocationMode(value));
        pointMode.onValueChanged.AddListener((bool value) => setPointMode(value));
    }

    // Update is called once per frame
    void Update () {

	}

    void OnApplicationQuit()
    {

    }

    //初始化电机选择地址列表
    private void updateDropDownItem()
    {
        addressList.options.Clear();
        Dropdown.OptionData temoData;
        for (int i = 1; i <= 4; i++)
        {
            //给每一个option选项赋值
            temoData = new Dropdown.OptionData();
            temoData.text = string.Format("{0:D2}", i);
            addressList.options.Add(temoData);
        }
        //初始选项的显示
        addressList.captionText.text = "01";

    }

    //启动电机
    private void startEngine()
    {
        byte addr = addrs[addressList.options[addressList.value].text];
        byte[] msg = { addr, 0x05, 0x00, 0x01, 0xff, 0x00 };
        handleMsg(msg);
    }

    //停止电机
    private void endEngine()
    {
        byte addr = addrs[addressList.options[addressList.value].text];
        byte[] msg = { addr, 0x05, 0x00, 0x01, 0x00, 0x00 };
        handleMsg(msg);
    }

    //设置加速到指定位置的时间 加减速时间在开始运动 IO 线圈寄存器=OFF 或者外部 IO（启动信号）光耦不导通时生效
    private void setSpeedPlusTime()
    {
        byte addr = addrs[addressList.options[addressList.value].text];
        byte[] msg = { addr, 0x06, 0x00, 0x06 };
        byte[] sp = getParameterValue(inputSpeedPlusTime);
        byte[] raw = messageManagement.combineArray(msg, sp);
        handleMsg(raw);
    }

    //设置减速到指定速度的时间 加减速时间在开始运动 IO 线圈寄存器=OFF 或者外部 IO（启动信号）光耦不导通时生效
    private void setSpeedMinusTime()
    {
        byte addr = addrs[addressList.options[addressList.value].text];
        byte[] msg = { addr, 0x06, 0x00, 0x07 };
        byte[] sp = getParameterValue(inputSpeedMinusTime);
        byte[] raw = messageManagement.combineArray(msg, sp);
        handleMsg(raw);
    }

    //设置转速 速度模式：速度值随时生效点到点位置模式：开始运动 IO 线圈寄存器 = OFF 或者外部 IO（启动信号）光耦不导通时生效
    private void setSpeed()
    {
        byte addr = addrs[addressList.options[addressList.value].text];
        byte[] msg = { addr, 0x06, 0x00, 0x03 };
        byte[] sp = getParameterValue(inputSpeed);
        byte[] raw = messageManagement.combineArray(msg, sp);
        handleMsg(raw);
    }

    //设置周期性位置的周期  默认=1，重新上电有效
    private void setLocationPeriod() {
        byte addr = addrs[addressList.options[addressList.value].text];
        byte[] msg = { addr, 0x06, 0x00, 0x0e };
        byte[] sp = getParameterValue(inputLocationPeriod);
        byte[] raw = messageManagement.combineArray(msg, sp);
        handleMsg(raw);
    }


    //设置周期性位置的周期 1-30000ms 默认=1，重新上电有效
    private void setLocationProperty()
    {
        byte addr = addrs[addressList.options[addressList.value].text];
        byte[] msg = { addr, 0x06, 0x00, 0x0d };
        byte[] sp = getParameterValue(inputProperty);
        byte[] raw = messageManagement.combineArray(msg, sp);
        handleMsg(raw);
    }

    //运动循环命令次数 0-30000 随时生效
    private void setCycleIndex()
    {
        byte addr = addrs[addressList.options[addressList.value].text];
        byte[] msg = { addr, 0x06, 0x00, 0x08 };
        byte[] sp = getParameterValue(inputCycleIndex);
        byte[] raw = messageManagement.combineArray(msg, sp);
        handleMsg(raw);
    }

    //运动循环等待时间 0-30000（单位根据寄存器12 确定） 随时生效
    private void setWaitingTime()
    {
        byte addr = addrs[addressList.options[addressList.value].text];
        byte[] msg = { addr, 0x06, 0x00, 0x09 };
        byte[] sp = getParameterValue(inputWaitingTime);
        byte[] raw = messageManagement.combineArray(msg, sp);
        handleMsg(raw);
    }

    //等待时间单位 0：ms 1：s 重新上电有效
    private void setWaitingTimeUnit()
    {
        byte addr = addrs[addressList.options[addressList.value].text];
        byte[] msg = { addr, 0x06, 0x00, 0x0c };
        byte[] sp = getParameterValue(inputWaitingTimeUnit);
        byte[] raw = messageManagement.combineArray(msg, sp);
        handleMsg(raw);
    }

    //设置为速度模式
    public void setSpeedMode(bool value) {
        if (value) {
            byte addr = addrs[addressList.options[addressList.value].text];
            byte[] msg = { addr, 0x06, 0x00, 0x00, 0x00, 0x01 };
            handleMsg(msg);
        }
    }
    
    //设置为位置模式
    public void setLocationMode(bool value)
    {
        if (value) {
            byte addr = addrs[addressList.options[addressList.value].text];
            byte[] msg = { addr, 0x06, 0x00, 0x00, 0x00, 0x02 };
            handleMsg(msg);
        }
    }

    //设定为点到点位置模式
    public void setPointMode(bool value)
    {
        if (value) {
            byte addr = addrs[addressList.options[addressList.value].text];
            byte[] msg = { addr, 0x06, 0x00, 0x00, 0x00, 0x03 };
            handleMsg(msg);
        }
    }

    //获取InputFiled输入的值转化为byte数组
    private byte[] getParameterValue(InputField inputFiled) {
        Debug.Log(inputFiled.name+" : "+ inputFiled.text);
        int value = int.Parse(inputFiled.text);
        //基本上四位，因为int对应4byte
        byte[] s = System.BitConverter.GetBytes(value);
        Array.Reverse(s);
        byte[] sp = new byte[2];
        sp[0] = s[2];
        sp[1] = s[3];
        return sp;
    }

    private void handleMsg(byte[] msg) {
        byte[] crc = messageManagement.CRCCalc(msg);
        byte[] data = messageManagement.combineArray(msg, crc);
        messageManagement.sendMessage(data);
    }

}
