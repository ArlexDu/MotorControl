using System;
//先要引入这个命名空间
using System.IO.Ports;

//这个是连接上的串口设备的定义好的参数，发送这个参数就能控制串口设备
public enum PortsType
{
    //01、全开：PC发送'I'； 
    //02、全关：PC发送'i'; 
    I, i,
    //03、第一路开：PC发送'A'；
    //04、第二路开：PC发送'B'；
    //05、第三路开：PC发送'C'；
    //06、第四路开：PC发送'D'；
    //07、第五路开：PC发送'E'；
    //08、第六路开：PC发送'F'；
    //09、第七路开：PC发送'G'；
    //10、第八路开：PC发送'H'；
    A, B, C, D, E, F, G, H,
    //11、第一路关：PC发送'a'；
    //12、第二路关：PC发送'b'；
    //13、第三路关：PC发送'c'；
    //14、第四路关：PC发送'd'；
    //15、第五路关：PC发送'e'；
    //16、第六路关：PC发送'f'；
    //17、第七路关：PC发送'g'；
    //18、第八路关：PC发送'h'；
    a, b, c, d, e, f, g, h
}
public class PortsControl
{
    //第一个参数是端口的名字，一会说怎么看端口，第二个参数是波特率，这个是设备自身的参数
    SerialPort sp = new SerialPort("COM3", 9600);//声明一个串口类

    //这个是完整的参数，名别是 端口名、波特率、奇偶效验、数据位、流控制参数
    //SerialPort sp1 = new SerialPort("COM3", 9600, Parity.None, 8, StopBits.None);

    //这个就是一个发送的API，其他程序就调用这个API
    public void Send_Click(PortsType data)
    {
        sp.Open();
        switch (data)
        {
            case PortsType.I:
                //给端口发送数据
                sp.WriteLine("I");
                break;
            case PortsType.i:
                sp.WriteLine("i");
                break;
            case PortsType.A:
                sp.WriteLine("A");
                break;
            case PortsType.B:
                sp.WriteLine("B");
                break;
            case PortsType.C:
                sp.WriteLine("C");
                break;
            case PortsType.D:
                sp.WriteLine("D");
                break;
            case PortsType.E:
                sp.WriteLine("E");
                break;
            case PortsType.F:
                sp.WriteLine("F");
                break;
            case PortsType.G:
                sp.WriteLine("G");
                break;
            case PortsType.H:
                sp.WriteLine("H");
                break;
            case PortsType.a:
                sp.WriteLine("a");
                break;
            case PortsType.b:
                sp.WriteLine("b");
                break;
            case PortsType.c:
                sp.WriteLine("c");
                break;
            case PortsType.d:
                sp.WriteLine("d");
                break;
            case PortsType.e:
                sp.WriteLine("e");
                break;
            case PortsType.f:
                sp.WriteLine("f");
                break;
            case PortsType.g:
                sp.WriteLine("g");
                break;
            case PortsType.h:
                sp.WriteLine("h");
                break;
            default:
                break;
        }
        sp.Close();
    }
}

