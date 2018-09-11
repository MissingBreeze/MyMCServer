using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace Receive
{
    class Conn
    {

        public const int BUFFER_SIZE = 1024;

        public Socket socket;

        public bool isUse = false;

        public byte[] readBuff = new byte[BUFFER_SIZE];

        public int buffCount = 0;

        public Conn() 
        {
            readBuff = new byte[BUFFER_SIZE];
        }

        public void Init(Socket socket) 
        {
            this.socket = socket;
            isUse = true;
            buffCount = 0;
        }

        /// <summary>
        /// 缓冲区剩余的字节数
        /// </summary>
        /// <returns></returns>
        public int BuffRemain() 
        {
            return BUFFER_SIZE - buffCount;
        }

        /// <summary>
        /// 获取客户端地址
        /// </summary>
        /// <returns></returns>
        public string GetAddress() 
        {
            if (!isUse)
                return "无法获取地址";
            return socket.RemoteEndPoint.ToString();
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public void Close() 
        {
            if (!isUse) 
                return;
            Console.WriteLine("关闭连接" + GetAddress());
            socket.Close();
            isUse = false;
        }

    }
}
