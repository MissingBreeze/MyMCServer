using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;

namespace Receive
{
    class Serv
    {
        /// <summary>
        /// 监听套接字
        /// </summary>
        public Socket listenfd;

        public Conn[] conns;

        /// <summary>
        /// 最大连接数
        /// </summary>
        public int maxConn = 50;

        /// <summary>
        /// 获取连接池索引，返回负数表示获取失败
        /// </summary>
        /// <returns></returns>
        public int NewIndex() 
        {
            if (conns == null) 
            {
                return -1;
            }
            for (int i = 0; i < conns.Length; i++)
            {
                if (conns[i] == null) 
                {
                    conns[i] = new Conn();
                    return i;
                }
                else if (conns[i].isUse == false) 
                {
                    return i;
                }
            }
            return -1;
        }

        public void Start(string hosts, int port) 
        {
            
            conns = new Conn[maxConn];
            for (int i = 0; i < maxConn; i++)
            {
                conns[i] = new Conn();
            }
            listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipAdr = IPAddress.Parse(hosts);
            IPEndPoint ipEp = new IPEndPoint(ipAdr, port);
            try
            {
                listenfd.Bind(ipEp);
            }
            catch (Exception e) 
            {
                Console.WriteLine("服务器端口异常："+ e.Message);
                Console.ReadLine();
            }
            // 监听，maxConn 监听的最大数量
            listenfd.Listen(maxConn);
            listenfd.BeginAccept(AcceptCb, null);
            Console.WriteLine("服务器启动成功");
            //Console.WriteLine(listenfd.LocalEndPoint.ToString());
        }

        // 监听回调
        public void AcceptCb(IAsyncResult ar) 
        {
            try
            {
                Socket socket = listenfd.EndAccept(ar);
                int index = NewIndex();
                if (index < 0)
                {
                    socket.Close();
                    Console.Write("连接已满");
                }
                else 
                {
                    Conn conn = conns[index];
                    conn.Init(socket);
                    string adr = conn.GetAddress();
                    Console.WriteLine("客户端已连接，ip:" + adr + "池ID" + index);
                    conn.socket.BeginReceive(conn.readBuff, conn.buffCount, conn.BuffRemain(), SocketFlags.None, ReceiveCb, conn);
                }
                listenfd.BeginAccept(AcceptCb,null);
            }
            catch (Exception e) 
            {
                Console.WriteLine("AcceptCb失败:" + e.Message);
            }
        }
        

        private void ReceiveCb(IAsyncResult ar) 
        {
            Conn conn = (Conn)ar.AsyncState;
            try 
            {
                int count = conn.socket.EndReceive(ar);
                if (count <= 0) 
                {
                    Console.WriteLine("收到" + conn.GetAddress() + "断开连接");
                    conn.Close();
                    return;
                }
                string str = System.Text.Encoding.UTF8.GetString(conn.readBuff,0,count);
                Console.WriteLine("收到数据：" + str);
                string[] strArry = str.Split(' ');
                Console.WriteLine("收到数据0：" + strArry[0]);
                // get：获取mod文件列表；download：下载文件
                switch (strArry[0]) 
                {
                    case "get":
                        SendModListFile();
                        break;
                    case "download":
                        string modName = strArry[1];
                        SendMod(modName);
                        break;
                }

                //Console.WriteLine("收到" + conn.GetAddress() + "的数据：" + str);
                //str = conn.GetAddress() + ":" + str;
                //byte[] bytes = System.Text.Encoding.UTF8.GetBytes(str);
                //for (int i = 0; i < conns.Length; i++)
                //{
                //    if (conns[i] == null) 
                //    {
                //        continue;
                //    }
                //    if (!conns[i].isUse) 
                //    {
                //        continue;
                //    }
                //    Console.WriteLine("将消息传送给" + conns[i].GetAddress());
                //    conns[i].socket.Send(bytes);
                //}
                conn.socket.BeginReceive(conn.readBuff, conn.buffCount, conn.BuffRemain(), SocketFlags.None, ReceiveCb, conn);

            }
            catch(Exception e)
            {
                Console.WriteLine("收到" + conn.GetAddress() + "断开连接");
                conn.Close();
            }
        }

        /// <summary>
        /// 发送mod列表文件
        /// </summary>
        public void SendModListFile() 
        {
            string ip = ProgramTools.GetXmlNoteValue("ip");
            int port = int.Parse(ProgramTools.GetXmlNoteValue("sendPort"));
            Console.WriteLine("ip : port" + ip + " : " + port);
            IPAddress ipAdr = IPAddress.Parse(ip);
            IPEndPoint hosts = new IPEndPoint(ipAdr, port);
            TcpListener tcpLisyener = new TcpListener(hosts);
            tcpLisyener.Start();
            Thread thread = new Thread(SendModTextFileFunc);
            thread.Start(tcpLisyener);
            thread.IsBackground = true;
        }

        /// <summary>
        /// 发送mod列表文件
        /// </summary>
        /// <param name="obj"></param>
        private void SendModTextFileFunc(object obj)
        {
            TcpListener tcpListener = obj as TcpListener;
            while (true)
            {
                try
                {
                    // 接收连接请求
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();
                    if (tcpClient.Connected)
                    {
                        // 要传输的流
                        NetworkStream stream = tcpClient.GetStream();

                        string modPath = System.IO.Directory.GetCurrentDirectory() + "\\Mod.txt";

                        //if (!Directory.Exists(modPath)) 
                        //{
                        //    ProgramTools.GetFileNameList(ProgramTools.GetXmlNoteValue("modPath"));
                        //    //SendModTextFileFunc(obj);
                        //    //return;
                        //}

                        FileStream fileStream = new FileStream(modPath, FileMode.Open);

                        int fileReadSize = 0;
                        long fileLength = 0;

                        while (fileLength < fileStream.Length)
                        {
                            byte[] buffer = new byte[2048];
                            fileReadSize = fileStream.Read(buffer, 0, buffer.Length);
                            stream.Write(buffer, 0, fileReadSize);
                            fileLength += fileReadSize;
                        }
                        fileStream.Flush();
                        stream.Flush();
                        fileStream.Close();
                        stream.Close();
                        tcpClient.Close();
                        tcpListener.Stop();
                        Console.WriteLine("发送成功!");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("发送失败" + e.Message);
                    Thread.CurrentThread.Abort();
                }

            }
        }

        private string modName;

        /// <summary>
        /// 发送mod文件
        /// </summary>
        public void SendMod(string modName) 
        {
            this.modName = modName;
            string ip = ProgramTools.GetXmlNoteValue("ip");
            int port = int.Parse(ProgramTools.GetXmlNoteValue("sendModPort"));
            IPAddress ipAdr = IPAddress.Parse(ip);
            IPEndPoint hosts = new IPEndPoint(ipAdr, port);
            TcpListener tcpLisyener = new TcpListener(hosts);
            tcpLisyener.Start();
            Thread thread = new Thread(SendModFile);
            thread.Start(tcpLisyener);
            thread.IsBackground = true;
        }

        /// <summary>
        /// 发送mod列表文件
        /// </summary>
        /// <param name="obj"></param>
        private void SendModFile(object obj)
        {
            TcpListener tcpListener = obj as TcpListener;
            while (true)
            {
                try
                {
                    // 接收请求
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();
                    if (tcpClient.Connected)
                    {
                        // 要传输的流
                        NetworkStream stream = tcpClient.GetStream();

                        string modPath = ProgramTools.GetXmlNoteValue("modPath") + @"/" + modName;

                        FileStream fileStream = new FileStream(modPath, FileMode.Open);

                        int fileReadSize = 0;
                        long fileLength = 0;

                        while (fileLength < fileStream.Length)
                        {
                            byte[] buffer = new byte[2048];
                            fileReadSize = fileStream.Read(buffer, 0, buffer.Length);
                            stream.Write(buffer, 0, fileReadSize);
                            fileLength += fileReadSize;
                        }
                        fileStream.Flush();
                        stream.Flush();
                        fileStream.Close();
                        stream.Close();
                        tcpListener.Stop();
                        Console.WriteLine("发送成功!");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("发送失败" + e.Message);
                    Thread.CurrentThread.Abort();
                }

            }
        }

    }
}
