using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class SocketServer
{
    private Thread thread;
    private UdpClient client;
    private string address;
    private int port = 5065;
    private Action<string> listenerFunc;

    public SocketServer(string address, int port)
    {
        this.address = address;
        this.port = port;
        this.InitThread();
    }

    private void InitThread()
    {
        thread = new Thread(new ThreadStart(InitServer));
        thread.IsBackground = true;
        thread.Start();
    }

    private void InitServer()
    {
        client = new UdpClient(port);
        IPEndPoint listenIP = new IPEndPoint(IPAddress.Any, 0);
        //if (null == address)
        //{
        //    listenIP = new IPEndPoint(IPAddress.Any, 0);
        //}
        //else
        //{
        //    listenIP = new IPEndPoint(IPAddress.Parse(address), 0);
        //}

        while (true)
        {
            try
            {
                byte[] data = client.Receive(ref listenIP);
                string text = Encoding.UTF8.GetString(data);
              
                if (null != listenerFunc)
                {
                    listenerFunc(text);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }

    public void Listen(Action<string> func)
    {
        this.listenerFunc = func;
    }

    public void Close()
    {
        if (thread!=null) thread.Abort();
        if (client!=null) client.Close();
    }
}