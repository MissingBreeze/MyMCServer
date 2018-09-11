using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;
using System.Net;
using System.IO;
using System.Xml.Linq;

namespace Receive
{
    static public class ProgramTools
    {
        /// <summary>
        ///  mod文件列表
        /// </summary>
        static public List<string> modFile = new List<string>();




        /// <summary>
        /// 判断端口是否被使用
        /// </summary>
        /// <param name="port">要验证的端口</param>
        /// <returns>是否被使用</returns>
        static public bool PortInUse(int port) 
        {
            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] ipEndPoints = ipProperties.GetActiveTcpListeners();
            foreach (IPEndPoint item in ipEndPoints)
            {
                Console.WriteLine(item.Port);
                if (item.Port == port) 
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取本机ip地址
        /// </summary>
        /// <returns></returns>
        static public string GetIP() 
        {
            IPHostEntry localhost = Dns.GetHostEntry(Dns.GetHostName());
            return localhost.AddressList[0].ToString();
        }

        /// <summary>
        /// 更新文件名列表并生成文件
        /// </summary>
        static public void GetFileNameList(string path) 
        {
            DirectoryInfo folder = new DirectoryInfo(path);
            modFile = new List<string>();
            foreach (var item in folder.GetFiles())
            {
                modFile.Add(item.Name);
            }
            CreateText();
        }

        /// <summary>
        /// 生成mod列表的Text文件
        /// </summary>
        static public void CreateText() 
        {
            string modTextPath = System.IO.Directory.GetCurrentDirectory() + "\\Mod.txt";
            if (Directory.Exists(modTextPath)) 
            {
                File.Delete(modTextPath);
            }

            
            StreamWriter sw = File.CreateText(modTextPath);
            foreach (var item in modFile)
            {
                sw.WriteLine(item);
            }
            sw.Close();
            //FileStream fs = new FileStream(modTextPath, FileMode.Create,FileAccess.Write);
            //StreamWriter sw = new StreamWriter(fs);
            //foreach (var item in modFile)
            //{
            //    sw.WriteLine(item);
            //}
            //sw.Flush();
            //sw.Close();
            //fs.Close();
        }

        /// <summary>
        /// xml节点setting
        /// </summary>
        static private XElement _element;

        /// <summary>
        /// 设置配置文件xml根节点
        /// </summary>
        static public void SetXmlRoot() 
        {
            string currenPath = System.IO.Directory.GetCurrentDirectory();
            XDocument document = XDocument.Load(currenPath + "\\Application.xml");
            XElement root = document.Root;
            _element = root.Element("Setting");
        }

        /// <summary>
        /// 获取xml节点值
        /// </summary>
        /// <param name="note"></param>
        /// <returns></returns>
        static public string GetXmlNoteValue(string note) 
        {
            if (_element.Attribute(note) != null && !string.IsNullOrEmpty(_element.Attribute(note).Value)) 
            {
                return _element.Attribute(note).Value;
            }
            return null;
        }
    }
}
