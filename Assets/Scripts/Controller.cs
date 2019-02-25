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

    private Text showStatus, showSpeed, showLocation, showMode;

    public MessageManagement messageManagement;

    private Dropdown addressList;

    private Toggle speedMode, locationMode, pointMode;

    //地址字典
    private Dictionary<string, byte> addrs = new Dictionary<string, byte>();

    //状态字典
    private Dictionary<int, string> status = new Dictionary<int, string>();

    //模式字典
    private Dictionary<int, string> modes = new Dictionary<int, string>();

    private float interval = 0;

    private bool sendingMessage;

    private int currentStatus=8, currentSpeed=0, currentLocation=0, currentMode=1;

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

        //初始化状态
        showStatus = GameObject.Find("engineStatusValue").GetComponent<Text>();
        showSpeed = GameObject.Find("currentSpeedValue").GetComponent<Text>();
        showLocation = GameObject.Find("currentLocationValue").GetComponent<Text>();
        showMode = GameObject.Find("modeValue").GetComponent<Text>();

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

        status.Add(6,"电机使能");
        status.Add(7, "中点报错");
        status.Add(8, "未接电机线");
        status.Add(9, "欠压");
        status.Add(10, "过压");
        status.Add(11, "E2PROM 错误");
        status.Add(14, "脱机，未使能");
        status.Add(15, "过流");

        modes.Add(1, "内部速度模式 ");
        modes.Add(2, "周期位置模式");
        modes.Add(3, "点到点位置模式");


        addressList = GameObject.Find("address").GetComponent<Dropdown>();
        updateDropDownItem();
        bindEvent();

        sendingMessage = false;

    }

    //绑定点击事件
    private void bindEvent()
    {
        btnStart.onClick.AddListener(startEngine);
        btnEnd.onClick.AddListener(endEngine);
        btnSpeedPlusTime.onClick.AddListener(setSpeedPlusTime);
        btnSpeedMinusTime.onClick.AddListener(setSpeedMinusTime);
        btnSpeed.onClick.AddListener(setSpeed);
        btnLocationPeriod.onClick.AddListener(setLocationPeriod);
        btnLocation.onClick.AddListener(setLocation);
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
        //更新电机的状态
        if (interval > 1) {
            getEngineerStatus();
            interval = 0;
        }
        updateStatus();
        interval += Time.deltaTime;

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

    //4 5 电机指令脉冲 增量式/绝对式脉冲数）开始运动 IO 线圈寄存器=OFF 或者外部IO（启动信号）光耦不导通时生效
    private void setLocation()
    {
        byte addr = addrs[addressList.options[addressList.value].text];
        byte[] msg = { addr, 0x10, 0x00, 0x04, 0x00, 0x02, 0x04 };
        Debug.Log(inputLocation.name + " : " + inputLocation.text);
        int value = int.Parse(inputLocation.text);
        //基本上四位，因为int对应4byte
        byte[] s = System.BitConverter.GetBytes(value);
        Array.Reverse(s);
        byte[] sp = new byte[4];
        sp[0] = s[2];
        sp[1] = s[3];
        sp[2] = s[0];
        sp[3] = s[1];
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

    //获得电机状态,当前速度,当前位置,当前模式
    private void getEngineerStatus()
    {
        byte addr = addrs[addressList.options[addressList.value].text];
        byte[] msg = { addr, 0x03, 0x00, 0xc8, 0x00, 0x05 };
        handleMsg(msg);
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

    //处理指令加入CRC校验，设置返回指令位数
    private void handleMsg(byte[] msg) {
        byte[] crc = messageManagement.CRCCalc(msg);
        byte[] data = messageManagement.combineArray(msg, crc);
        messageManagement.sendMessage(data);
        sendingMessage = true;
    }
    //更新电机状态
    public void updateStatusValue(int status, int speed, int location, int mode)
    {
        currentStatus = status;
        currentSpeed = speed;
        currentLocation = location;
        currentMode = mode;
    }
    //更新电机状态
    public void updateStatus() {
        showStatus.text = status[currentStatus];
        showSpeed.text = Convert.ToString(currentSpeed);
        showLocation.text = Convert.ToString(currentLocation);
        showMode.text = modes[currentMode];
    }
}
