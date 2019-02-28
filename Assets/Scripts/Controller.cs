using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System.Threading;
using UnityEngine.UI;

public class Controller : MonoBehaviour {

    private Button btnSpeedChangeTime, btnSpeed, btnLocation, btnLocationProperty, btnStart, btnEnd;

    private InputField inputSpeedChangeTime, inputSpeed, inputLocation, inputLocationProperty;

    private Text showStatus, showSpeed, showLocation, showMode;

    public MessageManagement messageManagement;
    public MoveController moveController;

    private Dropdown addressList;

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
        btnSpeedChangeTime = GameObject.Find("speedChangeTimeApply").GetComponent<Button>();
        btnSpeed = GameObject.Find("speedApply").GetComponent<Button>();
        btnLocation = GameObject.Find("locationApply").GetComponent<Button>();
        btnLocationProperty = GameObject.Find("locationPropertyApply").GetComponent<Button>();

        //初始化inputfiled
        inputSpeedChangeTime = GameObject.Find("speedChangeTimeValue").GetComponent<InputField>();
        inputSpeed = GameObject.Find("speedValue").GetComponent<InputField>();
        inputLocation = GameObject.Find("locationValue").GetComponent<InputField>();
        inputLocationProperty = GameObject.Find("locationPropertyValue").GetComponent<InputField>();

        //初始化状态
        showStatus = GameObject.Find("engineStatusValue").GetComponent<Text>();
        showSpeed = GameObject.Find("currentSpeedValue").GetComponent<Text>();
        showLocation = GameObject.Find("currentLocationValue").GetComponent<Text>();
        showMode = GameObject.Find("modeValue").GetComponent<Text>();

        //初始化功能按钮
        btnStart = GameObject.Find("start").GetComponent<Button>();
        btnEnd = GameObject.Find("end").GetComponent<Button>();

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
        btnSpeedChangeTime.interactable = status;
        btnSpeed.interactable = status;
        btnLocation.interactable = status;
        btnLocationProperty.interactable = status;
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
        btnSpeedChangeTime.onClick.AddListener(setSpeedChangeTime);
        btnSpeed.onClick.AddListener(setSpeed);
        btnLocation.onClick.AddListener(setLocation);
        btnLocationProperty.onClick.AddListener(setLocationProperty);
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

        //设置运动模式为点到点的模式
        setPointMode(true);
        //获取参数状态
        getCommandStatus();
        //获取状态寄存器数据
        getEngineerStatus();
        //获取线圈寄存器状态
        geCoilStatus();

        startImage.SetActive(false);
        endImage.SetActive(true);

    }

    //启动电机
    public void startEngine()
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
    public void endEngine()
    {
        byte addr = addrs[addressList.options[addressList.value].text];
        byte[] msg = { addr, 0x05, 0x00, 0x01, 0x00, 0x00 };
        handleMsg(msg);
        moveController.setCoilStatus(currentAddress, false);
        startImage.SetActive(false);
        endImage.SetActive(true);
        updateButtonStatus(false);
    }

    //设置加减速的时间
    private void setSpeedChangeTime()
    {
        setSpeedPlusTime();
        setSpeedMinusTime();
    }

    //设置加速到指定位置的时间 加减速时间在开始运动 IO 线圈寄存器=OFF 或者外部 IO（启动信号）光耦不导通时生效
    private void setSpeedPlusTime()
    {
        byte addr = addrs[addressList.options[addressList.value].text];
        byte[] msg = { addr, 0x06, 0x00, 0x06 };
        byte[] sp = getParameterValue(inputSpeedChangeTime);
        byte[] raw = messageManagement.combineArray(msg, sp);
        handleMsg(raw);
    }

    //设置减速到指定速度的时间 加减速时间在开始运动 IO 线圈寄存器=OFF 或者外部 IO（启动信号）光耦不导通时生效
    private void setSpeedMinusTime()
    {
        byte addr = addrs[addressList.options[addressList.value].text];
        byte[] msg = { addr, 0x06, 0x00, 0x07 };
        byte[] sp = getParameterValue(inputSpeedChangeTime);
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

    //4 5 电机指令脉冲 增量式/绝对式脉冲数）开始运动 IO 线圈寄存器=OFF 或者外部IO（启动信号）光耦不导通时生效
    public void setLocation() {
        byte addr = addrs[addressList.options[addressList.value].text];
        byte[] msg = { addr, 0x10, 0x00, 0x04, 0x00, 0x02, 0x04 };
        //Debug.Log(inputLocation.name + " : " + inputLocation.text);
        //基本上四位，因为int对应4byte
        int value = int.Parse(inputLocation.text);
        byte[] s = System.BitConverter.GetBytes(value);
        Array.Reverse(s);
        byte[] sp = new byte[4];
        sp[0] = s[2];
        sp[1] = s[3];
        sp[2] = s[0];
        sp[3] = s[1];
        byte[] raw = messageManagement.combineArray(msg, sp);
        handleMsg(raw);
        moveController.setPrepareTarget(currentAddress, int.Parse(inputLocation.text));
    }


    //设置周期性位置的周期 1-30000ms 默认=1，重新上电有效
    private void setLocationProperty()
    {
        byte addr = addrs[addressList.options[addressList.value].text];
        byte[] msg = { addr, 0x06, 0x00, 0x0d };
        byte[] sp = getParameterValue(inputLocationProperty);
        byte[] raw = messageManagement.combineArray(msg, sp);
        handleMsg(raw);
        //moveController.setLocationProperty(currentAddress, int.Parse(inputLocationProperty.text));
    }

    //设置为速度模式
    public void setSpeedMode(bool value) {
        if (value) {
            //Debug.Log("set speed mode");
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
            //Debug.Log("set location mode");
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
            //Debug.Log("set point mode");
            byte addr = addrs[addressList.options[addressList.value].text];
            byte[] msg = { addr, 0x06, 0x00, 0x00, 0x00, 0x03 };
            handleMsg(msg);
            moveController.setMode(currentAddress, 3);
            currentMode = 3;
        }
    }

    //获取InputFiled输入的值转化为byte数组
    private byte[] getParameterValue(InputField inputFiled) {
        //Debug.Log(inputFiled.name+" : "+ inputFiled.text);
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
        inputSpeedChangeTime.text  = sphere.speedPlusTime.ToString();
        inputSpeed.text = sphere.speed.ToString();
        //int prepareLocation = moveController.getLocation(sphere.address);
        inputLocation.text = sphere.originLocation.ToString();
        moveController.setLocation(sphere.address, (int)sphere.originLocation);
        inputLocationProperty.text = sphere.locationProperty.ToString();
        showMode.text = modes[sphere.mode];
        //更新动画小球参数
        moveController.setSpeedPlusTime(sphere.address, (int)sphere.speedPlusTime);
        moveController.setSpeedMinusTime(sphere.address, (int)sphere.speedMinusTime);
        moveController.setSpeed(sphere.address, (int)sphere.speed);
        moveController.setLocationPeriod(sphere.address, sphere.locationPeriod);
        moveController.setLocationProperty(sphere.address, sphere.locationProperty);
        //moveController.setLocation(sphere.address, (int)sphere.originLocation);
        //moveController.setTarget(sphere.address, (int)sphere.originLocation);
        moveController.setCycleIndex(sphere.address, sphere.cycleIndex);
        moveController.setWaitingTime(sphere.address, sphere.waitingTime);
        moveController.setWaitingTimeUnit(sphere.address, sphere.waitingTimeUnit);
        moveController.setMode(currentAddress, sphere.mode);
        moveController.setAddress(currentAddress, currentAddress);
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

    //读取反馈成功指令更新参数
    public void updateParameterValue(int addr,int type, int value)
    {
        switch (type) {
            case 3://更新速度
                moveController.setSpeed(addr, value);
                inputSpeed.text = value.ToString();
                break;
            case 4://更新位置
                moveController.setTarget(addr);
                inputLocation.text = moveController.getLocation(addr).ToString();
                break;
            case 6://更新加速时间
                moveController.setSpeedPlusTime(addr, value);
                inputSpeedChangeTime.text = value.ToString();
                break;
            case 7://更新减速时间
                moveController.setSpeedMinusTime(addr, value);
                break;
            case 13://更新位置属性
                moveController.setLocationProperty(addr, value);
                inputLocationProperty.text = value.ToString();
                break;        
        }
    }

    //4 5 电机指令脉冲 增量式/绝对式脉冲数）开始运动 IO 线圈寄存器=OFF 或者外部IO（启动信号）光耦不导通时生效
    public void updateLocation(int address,int value)
    {
        byte addr = addrs[addressList.options[address].text];
        byte[] msg = { addr, 0x10, 0x00, 0x04, 0x00, 0x02, 0x04 };
        //Debug.Log(inputLocation.name + " : " + inputLocation.text);
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
        moveController.setPrepareTarget((address+1), value);
    }
}
