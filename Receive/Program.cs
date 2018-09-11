using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;
using System.Xml.Linq;

namespace Receive
{
    class Program
    {
        static void Main(string[] args)
        {
            // 初始化方法
            ProgramTools.SetXmlRoot();

            // 变量
            string ip;
            int port;

            //string currenPath = System.IO.Directory.GetCurrentDirectory();
            //XDocument document = XDocument.Load(currenPath + "\\Application.xml");
            //XElement root = document.Root;
            //XElement ele = root.Element("Setting");

            ip = ProgramTools.GetXmlNoteValue("ip");
            port = int.Parse(ProgramTools.GetXmlNoteValue("port"));//int.Parse(ele.Attribute("port").Value);

            Console.WriteLine("服务器启动中,IP:{0},Port:{1}",ip,port);
            Serv serv = new Serv();
            serv.Start(ip, port);
            
            while (true)
            {
                string str = Console.ReadLine();
                switch (str)
                {
                    case "quit":
                        return;
                    case "updata":
                        ProgramTools.GetFileNameList(ProgramTools.GetXmlNoteValue("modPath"));
                        break;
                }
            }


            //IPAddress ipAdr = IPAddress.Parse("172.17.0.5");
            //IPEndPoint hosts = new IPEndPoint(ipAdr, 2556);
            //TcpListener tcpLisyener = new TcpListener(hosts);
            //tcpLisyener.Start();
            //Console.WriteLine("开始监听...");
            //while (true) 
            //{
            //    string str = Console.ReadLine();
            //    switch(str)
            //    {
            //        case "quit":
            //            return;
            //        case "send":
            //            Thread thread = new Thread(SendFileFunc);
            //            thread.Start(tcpLisyener);
            //            thread.IsBackground = true;
            //            break;
            //    }
            //}
        }

        /// <summary>
        /// 发送文件
        /// </summary>
        /// <param name="obj"></param>
        private static void SendFileFunc(object obj) 
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

                        FileStream fileStream = new FileStream("C:\\test.jar", FileMode.Open);

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
                        Console.WriteLine("发送成功!");
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine("发送失败" + e.Message);
                }
                
            }
        }
    }
}
