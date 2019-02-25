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
    private Thread portRev, portDeal;
    private Queue<byte> dataQueue;
    private string outStr = string.Empty;
    private int resultNum = 8;

    // Use this for initialization
    void Start()
    {
        Debug.Log("Start");
        dataQueue = new Queue<byte>();
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
            portRev = new Thread(PortReceivedThread);
            portRev.IsBackground = true;
            portRev.Start();
            portDeal = new Thread(DealData);
            portDeal.Start();
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
        showInfo(results);
        //分析电机的状态
        if (num == 15) {
            //转化需要byte低位在前高位在后
            byte[] status = new byte[2];
            status[0] = results[4];
            status[1] = results[3];
            int finalStatus = System.BitConverter.ToInt16(status, 0);
            //Debug.Log(finalStatus);

            byte[] speed = new byte[2];
            speed[0] = results[6];
            speed[1] = results[5];
            int finalSpeed = System.BitConverter.ToInt16(speed, 0);
            //Debug.Log(finalSpeed);

            byte[] location = new byte[4];
            location[0] = results[8];
            location[1] = results[7];
            location[2] = results[10];
            location[3] = results[9];
            int finalLocation = System.BitConverter.ToInt32(location, 0);
            //Debug.Log(finalLocation);

            byte[] mode = new byte[2];
            mode[0] = results[12];
            mode[1] = results[11];
            int finalMode = System.BitConverter.ToInt16(mode, 0);
            //Debug.Log(finalMode);

            controller.updateStatusValue(finalStatus, finalSpeed, finalLocation, finalMode);
        }
    }

    //发送数据 同时设置返回的指令数量 https://blog.csdn.net/yangbingzhou/article/details/39504015
    public void sendMessage(byte[] data)
    {
        if (port.IsOpen)
        {
            port.Write(data, 0, data.Length);
            showInfo(data);
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
    }

    void OnApplicationQuit()
    {
        Debug.Log("退出！");
        if (portRev.IsAlive)
        {
            portRev.Abort();
        }
        if (portDeal.IsAlive)
        {
            portDeal.Abort();
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

}
