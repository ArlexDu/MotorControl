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
    SerialPort port;
    //  接受线程，处理线程
    Thread portRev, portDeal;
    Queue<string> dataQueue;
    string outStr = string.Empty;

    // Use this for initialization
    void Start()
    {
        Debug.Log("Start");
        dataQueue = new Queue<string>();
        port = new SerialPort(portName, baudrate, parite, dataBits, stopbits);
        port.ReadTimeout = 400;
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
                    resStr += buf[i].ToString("X2");
                    dataQueue.Enqueue(resStr);
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    //处理线程函数
    void DealData()
    {
        while (dataQueue.Count != 0)
        {
            for (int i = 0; i < dataQueue.Count; i++)
            {
                outStr += dataQueue.Dequeue();
                if (outStr.Length == 2)
                {
                    /*if (outStr == "AB")
                    {
                        byte[] ba = new byte[10]; 
                        for (int j = 0; j < 10; j++)
                        {
                            ba[j] = (byte)j;
                        }
                        SendData(ba);
                    }*/
                    Debug.Log(outStr);
                    outStr = string.Empty;
                }
            }
        }
    }

    //发送数据
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
        if (!portRev.IsAlive)
        {
            portRev = new Thread(PortReceivedThread); portRev.IsBackground = true;
            portRev.Start();
        }
        if (!portDeal.IsAlive)
        {
            portDeal = new Thread(DealData);
            portDeal.Start();
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
