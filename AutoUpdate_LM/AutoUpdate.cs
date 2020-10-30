using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoUpdate_LM
{
    public partial class AutoUpdate : Form
    {
        string flag = "";
        string filePath = "";
        string filePathCopy = "";

        public AutoUpdate(string[] args)
        {
            InitializeComponent();

            //List<string> files = new List<string>(Directory.EnumerateFiles(@"C:\Users\simso\OneDrive\바탕 화면\AutoUpdateTest"));
            //FileInfo file = new FileInfo(files[0]);
            //file.LastWriteTime
            if (args.Length > 0)
            {
                //args[0] Upload, Download
                //args[1] File Path
                //args[2] Copy File Path
            }
            else
            {
                flag = "Auto";  //Auto Update
            }
        }

        private void AutoUpdate_Shown(object sender, EventArgs e)
        {
            //string bindIP = "14.32.21.123";
            string bindIP = "127.0.0.1"; // GetIPAddress();
            int bindPort = 5605;
            ////string serverIp = "14.32.21.123";
            string serverIp = "127.0.0.1"; // GetIPAddress();
            const int serverPort = 5425;

            try
            {
                IPEndPoint clientAddress = new IPEndPoint(IPAddress.Parse(bindIP), bindPort);
                IPEndPoint serverAddress = new IPEndPoint(IPAddress.Parse(serverIp), serverPort);
                TcpClient client = new TcpClient(clientAddress);
                client.Connect(serverAddress);

                NetworkStream stream = client.GetStream();
                byte[] bytes = new byte[10];
                bytes = Encoding.Default.GetBytes(flag);
                stream.Write(bytes, 0, bytes.Length);   //flag Send

                if (flag.Equals("Upload"))
                {

                }
                else if (flag.Equals("Download"))
                {

                }
                else
                {
                    InitControls();
                    InitAutoUpdate(client);
                }
            }
            catch (SocketException se)
            {
                Console.WriteLine(se);
            }

            Console.WriteLine("클라이언트를 종료합니다.");
        }

        private string GetIPAddress()
        {
            IPHostEntry host;
            string localIP = "";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                }
            }
            return localIP;
        }

        private void InitControls()
        {
            pbrTotal.Style = ProgressBarStyle.Continuous;
            pbrTotal.Minimum = 0;
            pbrTotal.Maximum = 100;
            pbrTotal.Value = 0;
            pbrTotal.Enabled = true;
        }
        
        private void InitAutoUpdate(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            string dir = Directory.GetCurrentDirectory() + @"\";

            byte[] bytes = new byte[1024];
            int byteLength = stream.Read(bytes, 0, bytes.Length);   //File Info Receive
            string fileInfo = Encoding.Default.GetString(bytes, 0, byteLength);

            string[] files = fileInfo.Split('/');
            Stack<string> fileStack = new Stack<string>();
            Dictionary<string, string> dicFile = new Dictionary<string, string>();
            int totalSize = 0;

            for (int i = 0; i < files.Length; i++)
            {
                string[] info = files[i].Split('*');

                if (FileCompare(dir + info[0], info[1]))
                {
                    dicFile.Add(info[0], info[2]);
                    fileStack.Push(info[0]);
                    totalSize += Convert.ToInt32(info[2]);
                }
            }

            //bytes = new byte[8];
            //bytes = Encoding.Default.GetBytes(dicFile.Count.ToString());
            //stream.Write(bytes, 0, bytes.Length);   //File Count Send
            
            if (dicFile.Count > 0)
            {
                foreach (KeyValuePair<string, string> item in dicFile)
                {
                    bytes = new byte[256];
                    bytes = Encoding.Default.GetBytes(item.Key);
                    //stream.Write(bytes, 0, bytes.Length);   //File Request Send

                    //string[] serverFile = null;
                    //bytes = new byte[256];
                    //byteLength = stream.Read(bytes, 0, bytes.Length);   //Fileinfo Receive
                    //serverFile = Encoding.Default.GetString(bytes, 0, byteLength).Split('\\');
                    //string fileName = dir + @"\" + serverFile[0];

                    //bytes = Encoding.Default.GetBytes(FileCompare(fileName, serverFile[1]));
                    //stream.Write(bytes, 0, 1);   //Request Send

                    //if (Encoding.Default.GetString(bytes) == "O")
                    //{
                    int byteCount = 8192;
                    int indexCount = (Convert.ToInt32(item.Value) / byteCount) + 1;
                    byte[] fileBytes = new byte[byteCount];  //8192
                    FileStream fs = new FileStream(item.Key, FileMode.Create, FileAccess.Write);
                    int count = 0;

                    for (int j = 0; j < indexCount; j++)
                    {
                        count = stream.Read(fileBytes, 0, fileBytes.Length);
                        fs.Write(fileBytes, 0, count);  //File Receive
                    }

                    fs.Close();
                    stream.Write(bytes, 0, 1);   //Request Send
                    //}
                    //lblSpeed.Text = "Updating... (" + (i + 1) + " / " + fileCount + ")";
                    //pbrTotal.PerformStep();
                    //Application.DoEvents();
                }
            }


            //byte[] bytes = new byte[3];
            //int byteLength = stream.Read(bytes, 0, bytes.Length);   //FileCount Receive
            //int fileCount = Convert.ToInt32(Encoding.Default.GetString(bytes, 0, byteLength));

            //pbrTotal.Step = 100 / fileCount;
            //lblSpeed.Text = "Updating... (0 / " + fileCount + ")";

            //bytes = Encoding.Default.GetBytes("O");
            //stream.Write(bytes, 0, 1);   //Request Send

            for (int i = 0; i < fileCount; i++)
            {
                string[] serverFile = null;
                bytes = new byte[256];
                byteLength = stream.Read(bytes, 0, bytes.Length);   //Fileinfo Receive
                serverFile = Encoding.Default.GetString(bytes, 0, byteLength).Split('\\');
                string fileName = dir + @"\" + serverFile[0];

                bytes = Encoding.Default.GetBytes(FileCompare(fileName, serverFile[1]));
                stream.Write(bytes, 0, 1);   //Request Send

                if (Encoding.Default.GetString(bytes) == "O")
                {
                    int byteCount = 8192;
                    int indexCount = (Convert.ToInt32(serverFile[1]) / byteCount) + 1;
                    byte[] fileBytes = new byte[byteCount];  //8192
                    FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);
                    int count = 0;

                    for (int j = 0; j < indexCount; j++)
                    {
                        count = stream.Read(fileBytes, 0, fileBytes.Length);
                        fs.Write(fileBytes, 0, count);  //File Receive
                    }

                    fs.Close();
                    stream.Write(bytes, 0, 1);   //Request Send
                }
                lblSpeed.Text = "Updating... (" + (i + 1) + " / " + fileCount + ")";
                pbrTotal.PerformStep();
                Application.DoEvents();
                //lblUpdate.Update();
            }

            pbrTotal.Value = 100;
            pbrTotal.Enabled = false;
            stream.Close();
            client.Close();
            this.Close();
        }

        private bool FileCompare(string fileName, string lastAccessTime)
        {
            if (File.Exists(fileName))
            {
                FileInfo file = new FileInfo(fileName);
                if (lastAccessTime.Equals(file.LastAccessTime.ToString()))
                    return false;
            }
            return true;
        }
    }
}
