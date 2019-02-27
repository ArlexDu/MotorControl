using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System.Threading;
using UnityEngine.UI;

public class Controller : MonoBehaviour {

    private Button btnSpeedPlusTime, btnSpeedMinusTime, btnSpeed, btnLocationPeriod, btnLocation, btnLocationProperty, btnCycleIndex, btnWaitingTime,
        btnWaitingTimeUnit, btnStart, btnEnd, btnSave;

    private InputField inputSpeedPlusTime, inputSpeedMinusTime, inputSpeed, inputLocationPeriod, inputLocation, inputLocationProperty, inputCycleIndex,
        inputWaitingTime, inputWaitingTimeUnit;

    private Text showStatus, showSpeed, showLocation, showMode;

    public MessageManagement messageManagement;
    public MoveController moveController;

    private Dropdown addressList;

    private Toggle speedMode, locationMode, pointMode;

    //地址字典
    private Dictionary<string, byte> addrs = new Dictionary<string, byte>();

    //状态字典
    private Dictionary<int, string> status = new Dictionary<int, string>();

    //模式字典
    private Dictionary<int, string> modes = new Dictionary<int, string>();

    private float interval = 0;

    public GameObject startImage, endImage;

    private int currentAddress=1;

    private int currentMode = 1;

    // Use this for initialization
    void Start () {
        //初始化button
        btnSpeedPlusTime = GameObject.Find("speedPlusTimeApply").GetComponent<Button>();
        btnSpeedMinusTime = GameObject.Find("speedMinusTimeApply").GetComponent<Button>();
        btnSpeed = GameObject.Find("speedApply").GetComponent<Button>();
        btnLocationPeriod = GameObject.Find("locationPeriodApply").GetComponent<Button>();
        btnLocation = GameObject.Find("locationApply").GetComponent<Button>();
        btnLocationProperty = GameObject.Find("locationPropertyApply").GetComponent<Button>();
        btnCycleIndex = GameObject.Find("cycleIndexApply").GetComponent<Button>();
        btnWaitingTime = GameObject.Find("waitingTimeApply").GetComponent<Button>();
        btnWaitingTimeUnit = GameObject.Find("waitingTimeUnitApply").GetComponent<Button>();

        //初始化inputfiled
        inputSpeedPlusTime = GameObject.Find("speedPlusTimeValue").GetComponent<InputField>();
        inputSpeedMinusTime = GameObject.Find("speedMinusTimeValue" ).GetComponent<InputField>();
        inputSpeed = GameObject.Find("speedValue").GetComponent<InputField>();
        inputLocationPeriod = GameObject.Find("locationPeriodValue").GetComponent<InputField>();
        inputLocation = GameObject.Find("locationValue").GetComponent<InputField>();
        inputLocationProperty = GameObject.Find("locationPropertyValue").GetComponent<InputField>();
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

        updateButtonStatus(false);

        addressList = GameObject.Find("address").GetComponent<Dropdown>();
        updateDropDownItem();
        bindEvent();
        changeEngine(0);
    }

    private void updateButtonStatus(bool status) {
        btnSpeedPlusTime.interactable = status;
        btnSpeedMinusTime.interactable = status;
        btnSpeed.interactable = status;
        btnLocationPeriod.interactable = status;
        btnLocation.interactable = status;
        btnLocationProperty.interactable = status;
        btnCycleIndex.interactable = status;
        btnWaitingTime.interactable = status;
        btnWaitingTimeUnit.interactable = status;
    }

    //初始化参数
    private void getCommandStatus()
    {
        //Debug.Log("get init");
        byte addr = addrs[addressList.options[addressList.value].text];
        byte[] msg = { addr, 0x03, 0x00, 0x00, 0x00, 0x0f };
        handleMsg(msg);
    }

    //获得电机状态,当前速度,当前位置,当前模式
    private void getEngineerStatus()
    {
        //Debug.Log("get Status");
        byte addr = addrs[addressList.options[addressList.value].text];
        byte[] msg = { addr, 0x03, 0x00, 0xc8, 0x00, 0x05 };
        handleMsg(msg);
    }

    //获得线圈及寄存器状态 点击是否开启
    private void geCoilStatus()
    {
        //Debug.Log("get Coil");
        byte addr = addrs[addressList.options[addressList.value].text];
        byte[] msg = { addr, 0x01, 0x00, 0x01, 0x00, 0x01 };
        handleMsg(msg);
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
        btnLocationProperty.onClick.AddListener(setLocationProperty);
        btnCycleIndex.onClick.AddListener(setCycleIndex);
        btnWaitingTime.onClick.AddListener(setWaitingTime);
        btnWaitingTimeUnit.onClick.AddListener(setWaitingTimeUnit);


        speedMode.onValueChanged.AddListener((bool value) => setSpeedMode(value));
        locationMode.onValueChanged.AddListener((bool value) => setLocationMode(value));
        pointMode.onValueChanged.AddListener((bool value) => setPointMode(value));

        addressList.onValueChanged.AddListener((int value) => changeEngine(value));
    }

    // Update is called once per frame
    void Update () {
        //更新电机的状态
        if (interval > 0.3)
        {
            if (messageManagement.getCompleteStatus()) {
                getEngineerStatus();
                interval = 0;
            }
        }
        interval += Time.deltaTime;

    }

    void OnApplicationQuit()
    {
        endEngine();
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

    //更改电机控制页面
    private void changeEngine(int value) {

        moveController.changeSphereColor(currentAddress, false);
        currentAddress = int.Parse(addressList.options[value].text);
        moveController.changeSphereColor(currentAddress, true);

        //初始化参数
        getCommandStatus();
        //获取状态寄存器数据
        getEngineerStatus();
        //获取线圈寄存器状态
        geCoilStatus();

        startImage.SetActive(false);
        endImage.SetActive(true);


    }

    //启动电机
    private void startEngine()
    {
        byte addr = addrs[addressList.options[addressList.value].text];
        byte[] msg = { addr, 0x05, 0x00, 0x01, 0xff, 0x00 };
        handleMsg(msg);
        moveController.setCoilStatus(currentAddress, true);
        startImage.SetActive(true);
        endImage.SetActive(false);
        updateButtonStatus(true);
    }

    //停止电机
    private void endEngine()
    {
        byte addr = addrs[addressList.options[addressList.value].text];
        byte[] msg = { addr, 0x05, 0x00, 0x01, 0x00, 0x00 };
        handleMsg(msg);
        moveController.setCoilStatus(currentAddress, false);
        startImage.SetActive(false);
        endImage.SetActive(true);
        updateButtonStatus(false);
    }

    //设置加速到指定位置的时间 加减速时间在开始运动 IO 线圈寄存器=OFF 或者外部 IO（启动信号）光耦不导通时生效
    private void setSpeedPlusTime()
    {
        byte addr = addrs[addressList.options[addressList.value].text];
        byte[] msg = { addr, 0x06, 0x00, 0x06 };
        byte[] sp = getParameterValue(inputSpeedPlusTime);
        byte[] raw = messageManagement.combineArray(msg, sp);
        handleMsg(raw);
        moveController.setSpeedPlusTime(currentAddress, int.Parse(inputSpeedPlusTime.text));
    }

    //设置减速到指定速度的时间 加减速时间在开始运动 IO 线圈寄存器=OFF 或者外部 IO（启动信号）光耦不导通时生效
    private void setSpeedMinusTime()
    {
        byte addr = addrs[addressList.options[addressList.value].text];
        byte[] msg = { addr, 0x06, 0x00, 0x07 };
        byte[] sp = getParameterValue(inputSpeedMinusTime);
        byte[] raw = messageManagement.combineArray(msg, sp);
        handleMsg(raw);
        moveController.setSpeedMinusTime(currentAddress, int.Parse(inputSpeedMinusTime.text));
    }

    //设置转速 速度模式：速度值随时生效点到点位置模式：开始运动 IO 线圈寄存器 = OFF 或者外部 IO（启动信号）光耦不导通时生效
    private void setSpeed()
    {
        byte addr = addrs[addressList.options[addressList.value].text];
        byte[] msg = { addr, 0x06, 0x00, 0x03 };
        byte[] sp = getParameterValue(inputSpeed);
        byte[] raw = messageManagement.combineArray(msg, sp);
        handleMsg(raw);
        moveController.setSpeed(currentAddress, int.Parse(inputSpeed.text));
    }

    //设置周期性位置的周期  默认=1，重新上电有效
    private void setLocationPeriod() {
        byte addr = addrs[addressList.options[addressList.value].text];
        byte[] msg = { addr, 0x06, 0x00, 0x0e };
        byte[] sp = getParameterValue(inputLocationPeriod);
        byte[] raw = messageManagement.combineArray(msg, sp);
        handleMsg(raw);
        moveController.setLocationPeriod(currentAddress, int.Parse(inputLocationPeriod.text));
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
        moveController.setTarget(currentAddress, int.Parse(inputLocation.text));
    }


    //设置周期性位置的周期 1-30000ms 默认=1，重新上电有效
    private void setLocationProperty()
    {
        byte addr = addrs[addressList.options[addressList.value].text];
        byte[] msg = { addr, 0x06, 0x00, 0x0d };
        byte[] sp = getParameterValue(inputLocationProperty);
        byte[] raw = messageManagement.combineArray(msg, sp);
        handleMsg(raw);
        moveController.setLocationProperty(currentAddress, int.Parse(inputLocationProperty.text));
    }

    //运动循环命令次数 0-30000 随时生效
    private void setCycleIndex()
    {
        byte addr = addrs[addressList.options[addressList.value].text];
        byte[] msg = { addr, 0x06, 0x00, 0x08 };
        byte[] sp = getParameterValue(inputCycleIndex);
        byte[] raw = messageManagement.combineArray(msg, sp);
        handleMsg(raw);
        moveController.setCycleIndex(currentAddress, int.Parse(inputCycleIndex.text));
    }

    //运动循环等待时间 0-30000（单位根据寄存器12 确定） 随时生效
    private void setWaitingTime()
    {
        byte addr = addrs[addressList.options[addressList.value].text];
        byte[] msg = { addr, 0x06, 0x00, 0x09 };
        byte[] sp = getParameterValue(inputWaitingTime);
        byte[] raw = messageManagement.combineArray(msg, sp);
        handleMsg(raw);
        moveController.setWaitingTime(currentAddress, int.Parse(inputWaitingTime.text));
    }

    //等待时间单位 0：ms 1：s 重新上电有效
    private void setWaitingTimeUnit()
    {
        byte addr = addrs[addressList.options[addressList.value].text];
        byte[] msg = { addr, 0x06, 0x00, 0x0c };
        byte[] sp = getParameterValue(inputWaitingTimeUnit);
        byte[] raw = messageManagement.combineArray(msg, sp);
        handleMsg(raw);
        moveController.setWaitingTimeUnit(currentAddress, int.Parse(inputWaitingTimeUnit.text));
    }

    //设置为速度模式
    public void setSpeedMode(bool value) {
        if (value) {
            Debug.Log("set speed mode");
            byte addr = addrs[addressList.options[addressList.value].text];
            byte[] msg = { addr, 0x06, 0x00, 0x00, 0x00, 0x01 };
            handleMsg(msg);
            moveController.setMode(currentAddress,1);
            currentMode = 1;
        }
    }
    
    //设置为位置模式
    public void setLocationMode(bool value)
    {
        if (value) {
            Debug.Log("set location mode");
            byte addr = addrs[addressList.options[addressList.value].text];
            byte[] msg = { addr, 0x06, 0x00, 0x00, 0x00, 0x02 };
            handleMsg(msg);
            moveController.setMode(currentAddress, 2);
            currentMode = 2;
        }
    }

    //设定为点到点位置模式
    public void setPointMode(bool value)
    {
        if (value) {
            Debug.Log("set point mode");
            byte addr = addrs[addressList.options[addressList.value].text];
            byte[] msg = { addr, 0x06, 0x00, 0x00, 0x00, 0x03 };
            handleMsg(msg);
            moveController.setMode(currentAddress, 3);
            currentMode = 3;
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

    //处理指令加入CRC校验，设置返回指令位数
    private void handleMsg(byte[] msg) {
        byte[] crc = messageManagement.CRCCalc(msg);
        byte[] data = messageManagement.combineArray(msg, crc);
        messageManagement.addToMessageQueue(data);
    }

    //得到电机的初始化状态
    public void updateCommandStatus(moveSphere sphere)
    {
        //初始化页面参数
        inputSpeedPlusTime.text  = sphere.speedPlusTime.ToString();
        inputSpeedMinusTime.text = sphere.speedMinusTime.ToString();
        inputSpeed.text = sphere.speed.ToString();
        inputLocationPeriod.text = sphere.locationPeriod.ToString();
        inputLocation.text = sphere.location.ToString();
        inputLocationProperty.text = sphere.locationProperty.ToString();
        inputCycleIndex.text = sphere.cycleIndex.ToString();
        inputWaitingTime.text = sphere.waitingTime.ToString();
        inputWaitingTimeUnit.text = sphere.waitingTimeUnit.ToString();
        showMode.text = modes[sphere.mode];
        //更新动画小球参数
        moveController.setSpeedPlusTime(sphere.address, sphere.speedPlusTime);
        moveController.setSpeedMinusTime(sphere.address, sphere.speedMinusTime);
        moveController.setSpeed(sphere.address, (int)sphere.speed);
        moveController.setLocationPeriod(sphere.address, sphere.locationPeriod);
        moveController.setLocationProperty(sphere.address, sphere.locationProperty);
        moveController.setLocation(sphere.address, (int)sphere.location);
        moveController.setCycleIndex(sphere.address, sphere.cycleIndex);
        moveController.setWaitingTime(sphere.address, sphere.waitingTime);
        moveController.setWaitingTimeUnit(sphere.address, sphere.waitingTimeUnit);
        moveController.setMode(currentAddress, sphere.mode);
        switch (sphere.mode)
        {
            case 1:
                speedMode.isOn = true;
                locationMode.isOn = false;
                pointMode.isOn = false;
                break;
            case 2:
                speedMode.isOn = false;
                locationMode.isOn = true;
                pointMode.isOn = false;
                break;
            case 3:
                speedMode.isOn = false;
                locationMode.isOn = false;
                pointMode.isOn = true;
                break;
        }
    }

    //更新电机状态
    public void updateStatusValue(int type, int speed, int location, int mode)
    {
        showStatus.text = status[type];
        showSpeed.text = Convert.ToString(speed);
        showLocation.text = Convert.ToString(location);
        showMode.text = modes[mode];
    }

    //更新电机状态
    public void updateCoilStatus(int addr, bool run)
    {
        moveController.setCoilStatus(addr, run);

        //更新页面显示
        if (currentAddress == addr)
        {
            if (run) {
                startImage.SetActive(true);
                endImage.SetActive(false);
            }
            else
            {
                startImage.SetActive(false);
                endImage.SetActive(true);
            }
        }

        updateButtonStatus(run);
    }
}
