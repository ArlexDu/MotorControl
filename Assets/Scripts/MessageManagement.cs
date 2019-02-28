using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System.Threading;
using UnityEngine.UI;

/**
 * 本文件用于串口数据收发
 **/

public class MessageManagement : MonoBehaviour
{
    public string portName = "COM5";
    public int baudrate = 9600;
    public Parity parite = Parity.None;
    public int dataBits = 8;
    public StopBits stopbits = StopBits.One;
    public Controller controller;
    private SerialPort port;
    //  接受线程，处理线程
    private Thread portRev, portDeal,portSend;
    private Queue<byte> dataQueue;
    //指令队列
    private Queue<byte[]> msgQueue;
    private string outStr = string.Empty;
    private int resultNum = 8;
    private bool completed;

    // Use this for initialization
    void Start()
    {
        Debug.Log("Start");
        dataQueue = new Queue<byte>();
        msgQueue = new Queue<byte[]>();
        port = new SerialPort(portName, baudrate, parite, dataBits, stopbits);
        //设定等待时间为4ms，若超过4ms则认为是下一帧数据
        port.ReadTimeout = 4;
        try
        {
            if (!port.IsOpen)
            {
                Debug.Log("串口打开成功！");
                port.Open();
            }
            else
            {
                Debug.Log("串口已经打开");
            }
            //前台线程和后台线程。这两者的区别就是：
            //应用程序必须运行完所有的前台线程才可以退出；
            //而对于后台线程，应用程序则可以不考虑其是否已经运行完毕而直接退出，所有的后台线程在应用程序退出时都会自动结束。
            portRev = new Thread(PortReceivedThread);
            portRev.IsBackground = true;
            portRev.Start();

            portDeal = new Thread(DealData);
            portDeal.IsBackground = true;

            portSend = new Thread(sendMessage);
            portSend.IsBackground = true;
            portSend.Start();
            Loom.Initialize();
            completed = true;
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    //接受线程函数
    void PortReceivedThread()
    {
        try
        {
            byte[] buf = new byte[1];
            string resStr = string.Empty;
            if (port.IsOpen)
            {
                port.Read(buf, 0, 1);
            }
            if (buf.Length == 0)
            {
                return;
            }
            if (buf != null)
            {
                for (int i = 0; i < buf.Length; i++)
                {
                    //resStr += buf[i].ToString("X2");
                    dataQueue.Enqueue(buf[i]);
                }
            }
        }
        catch (System.Exception ex)
        {
            if (dataQueue.Count > 0) {
                if (!portDeal.IsAlive)
                {
                    portDeal = new Thread(DealData);
                    portDeal.IsBackground = true;
                    portDeal.Start();
                }
            }
        }
    }

    //处理线程函数
    void DealData()
    {
        int num = dataQueue.Count;
        byte[] results = new byte[num];
        for (int i = 0; i < num; i++)
        {
            byte result = dataQueue.Dequeue();
            results[i] = result;
        }
        //Debug.Log("Return:");
        //showInfo(results);
        completed = true;//串口是一个消息收->发，所以发一个消息必须等到这个消息的反馈才能继续发下一个消息
        int address = getSingleValue(results[0]);
        switch (num){
            case 6://获取线圈寄存器的状态
                byte runbyte = results[3];
                bool run = runbyte == 0x01 ? true : false; 
                Loom.QueueOnMainThread((param) =>
                {
                    controller.updateCoilStatus(address,run);
                }, null);
                break;
            case 8://单个寄存器写入
                int type = getFinalValue(results, new int[] { 3, 2 });
                int value = getFinalValue(results, new int[] { 5, 4 });
                Loom.QueueOnMainThread((param) =>
                {
                    controller.updateParameterValue(address, type, value);
                }, null);
                break;
            case 15://实时分析电机的状态        
                int currStatus = getFinalValue(results,new int[] { 4, 3 });
                int currSpeed = getFinalValue(results, new int[] { 6, 5 });
                int currLocation = getFinalValue(results, new int[] { 8, 7, 10, 9 });
                int currMode = getFinalValue(results, new int[] { 12, 11 });
                Loom.QueueOnMainThread((param) =>
                {
                    controller.updateStatusValue(currStatus, currSpeed, currLocation, currMode);
                }, null);
                break;
            case 35://获取命令寄存器的状态
                moveSphere sphere = new moveSphere();
                sphere.address = address;
                sphere.mode = getFinalValue(results, new int[] { 4, 3 });
                sphere.speed = getFinalValue(results, new int[] { 10, 9 });
                sphere.originLocation = getFinalValue(results, new int[] { 12, 11, 14, 13 });
                sphere.speedPlusTime = getFinalValue(results, new int[] { 16, 15 });
                sphere.speedMinusTime = getFinalValue(results, new int[] { 18, 17 });
                sphere.cycleIndex = getFinalValue(results, new int[] { 20, 19 });
                sphere.waitingTime = getFinalValue(results, new int[] { 22, 21 });
                sphere.waitingTimeUnit = getFinalValue(results, new int[] { 28, 27 });
                sphere.locationProperty = getFinalValue(results, new int[] { 30, 29 });
                sphere.locationPeriod = getFinalValue(results, new int[] { 32, 31 });
                Loom.QueueOnMainThread((param) =>
                {
                    controller.updateCommandStatus(sphere);
                }, null);
                
                break;

        }
        
    }

    //解析串口返回数据
    private int getFinalValue(byte[] raw, int[] index) {
        int num = index.Length;
        byte[] b = new byte[num];
        int finalValue = 0;
        for (int i = 0; i < num; i++) {
            b[i] = raw[index[i]];
        }
        switch (num)
        {//转化需要byte低位在前高位在后
            case 2:
                finalValue = System.BitConverter.ToInt16(b, 0);
                break;
            case 4:
                finalValue = System.BitConverter.ToInt32(b, 0);
                break;
        }
        //Debug.Log(finalValue);
        return finalValue;
    }

    //解析单个参数
    private int getSingleValue(byte raw)
    {
        byte[] b = new byte[2];
        b[0] = raw;
        b[1] = 0x00;
        int value = System.BitConverter.ToInt16(b, 0);
        //Debug.Log(finalValue);
        return value;
    }

    //将消息加入到消息队列
    public void addToMessageQueue(byte[] msg) {
        msgQueue.Enqueue(msg);
    }

    //发送数据 同时设置返回的指令数量 https://blog.csdn.net/yangbingzhou/article/details/39504015
    void sendMessage()
    {
        while(msgQueue.Count > 0) {
            if (completed) {
                byte[] data = msgQueue.Dequeue();
                completed = false;
                if (port.IsOpen)
                {
                    port.Write(data, 0, data.Length);
                    //Debug.Log("Send:");
                    //showInfo(data);
                }
            }
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (!portRev.IsAlive) {
            portRev = new Thread(PortReceivedThread);
            portRev.IsBackground = true;
            portRev.Start();
        }
        if (!portSend.IsAlive)
        {
            portSend = new Thread(sendMessage);
            portSend.IsBackground = true;
            portSend.Start();
        }
    }

    void OnApplicationQuit()
    {
        //清空消息队列
        msgQueue.Clear();
        Debug.Log("退出！");
        if (portRev.IsAlive)
        {
            portRev.Abort();
        }
        if (portDeal.IsAlive)
        {
            portDeal.Abort();
        }
        if (portSend.IsAlive)
        {
            portSend.Abort();
        }
        port.Close();
    }

    //展示byte数据
    private void showInfo(byte[] bytes)
    {
        string info = "";
        foreach (byte b in bytes)
        {
            info = info + b.ToString("X2") + " ";
        }
        Debug.Log(info);    
    }

    //合并数组
    public byte[] combineArray(byte[] arr1, byte[] arr2)
    {
        byte[] data = new byte[arr1.Length + arr2.Length];
        arr1.CopyTo(data, 0);
        arr2.CopyTo(data, arr1.Length);
        return data;
    }

    //CRC校验
    public byte[] CRCCalc(byte[] crcbuf)
    {
        //计算并填写CRC校验码
        int crc = 0xffff;
        int len = crcbuf.Length;
        for (int n = 0; n < len; n++)
        {
            byte i;
            crc = crc ^ crcbuf[n];
            for (i = 0; i < 8; i++)
            {
                int TT;
                TT = crc & 1;
                crc = crc >> 1;
                crc = crc & 0x7fff;
                if (TT == 1)
                {
                    crc = crc ^ 0xa001;
                }
                crc = crc & 0xffff;
            }

        }
        byte[] crcFinal = new byte[2];
        crcFinal[1] = (byte)((crc >> 8) & 0xff);
        crcFinal[0] = (byte)(crc & 0xff);
        return crcFinal;
    }

    //获取当前指令状态
    public bool getCompleteStatus() {
        return completed;
    }
}
