using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Configuration;
using log4net;

namespace TcpServer
{
    public partial class TcpServer : Form
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(TcpServer));
        /// <summary>
        /// 생성자
        /// </summary>
        public TcpServer()
        {
            InitializeComponent();
            Init();
        }

        /// <summary>
        /// Init Multi Thread
        /// </summary>
        private void Init()
        {
            TcpListener server = null;
            IPEndPoint localAddress = null;

            try
            {
                localAddress = new IPEndPoint(GetIPAddress(), Convert.ToInt32(ConfigurationManager.AppSettings["ServerPort"]));
                server = new TcpListener(localAddress);
                server.Start();
                log.Info("===================== Server Start =====================");

                while (true)
                {
                    TcpClient client = server.AcceptTcpClient();

                    if (client.Connected)
                    {
                        log.Info("Add Client Connection: " + ((IPEndPoint)client.Client.RemoteEndPoint).ToString());

                        Thread tcpHandlerThread = new Thread(new ParameterizedThreadStart(TcpHandler));
                        tcpHandlerThread.Start(client);
                    }
                    else
                        log.Info("Unable to Connect: " + ((IPEndPoint)client.Client.RemoteEndPoint).ToString());
                }
            }
            catch (SocketException e)
            {
                log.Error(e);
            }
            catch (ArgumentNullException ne)
            {
                log.Error(ne);
            }
            finally
            {
                if (server != null)
                {
                    server.Stop();
                    server = null;
                }

                if (localAddress != null)
                    localAddress = null;
            }

            log.Info("===================== Server Shutdown =====================");
            this.Close();
        }

        /// <summary>
        /// TCP Handler
        /// </summary>
        /// <param name="client"></param>
        private void TcpHandler(object client)
        {
            TcpClient tcpClient = null;

            try
            {
                tcpClient = (TcpClient)client;
                using (NetworkStream stream = tcpClient.GetStream())
                {
                    byte[] bytes = new byte[10];
                    int byteLength = stream.Read(bytes, 0, bytes.Length);   //Flag Receive
                    string flag = Encoding.Default.GetString(bytes, 0, byteLength);
                    string clientInfo = tcpClient.Client.RemoteEndPoint.ToString();
                    log.Info(flag + " Start: " + clientInfo);

                    if (flag.Equals("Upload"))
                        Upload(tcpClient);
                    else if (flag.Equals("Download"))
                        Download(tcpClient);
                    else if (flag.Equals("AutoUpdate"))
                        AutoUpdate(tcpClient);

                    log.Info(flag + " End: " + clientInfo);
                    log.Info("Terminate Client: " + clientInfo);
                }
            }
            catch (Exception e)
            {
                log.Error(e);
            }

            if (tcpClient != null)
                tcpClient.Close();
        }

        #region Upload
        /// <summary>
        /// Upload
        /// </summary>
        /// <param name="stream"></param>
        private void Upload(TcpClient client)
        {
            using (NetworkStream stream = client.GetStream())
            {
                int byteSize = 8192;
                byte[] bytes = new byte[10];
                bytes = Encoding.Default.GetBytes(byteSize.ToString());
                stream.Write(bytes, 0, bytes.Length);   //ByteSize Send

                bytes = new byte[10];
                int byteLength = stream.Read(bytes, 0, bytes.Length);   //File Count Receive
                int fileCount = Convert.ToInt32(Encoding.Default.GetString(bytes, 0, byteLength));

                for (int i = 0; i < fileCount; i++)
                {
                    FileWrite(client, byteSize);
                }
            }
        }

        /// <summary>
        /// Stream Read and File Write
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="byteSize"></param>
        private void FileWrite(TcpClient client, int byteSize)
        {
            NetworkStream stream = client.GetStream();
            byte[] bytes = new byte[10];
            bytes = Encoding.Default.GetBytes("Ready");
            stream.Write(bytes, 0, bytes.Length);   //Ready Send

            bytes = new byte[256];
            int byteLength = stream.Read(bytes, 0, bytes.Length);   //File Length Receivce / NoFiles Receive
            string file = Encoding.Default.GetString(bytes, 0, byteLength);

            if (!file.Equals("NoFiles"))
            {
                string[] serverFile = file.Split('*');

                bool exist = FileCompare(serverFile[2], serverFile[1]);
                bytes = new byte[4];
                bytes = Encoding.Default.GetBytes(exist ? "OK" : "NO");
                stream.Write(bytes, 0, bytes.Length);   // Send

                if (exist)
                {
                    log.Info(serverFile[2] + " Receive: " + client.Client.RemoteEndPoint.ToString());
                    using (FileStream fs = new FileStream(serverFile[2], FileMode.Create, FileAccess.Write))
                    {
                        int count = 0;
                        int indexCount = Convert.ToInt32(serverFile[1]) / byteSize + 1;
                        byte[] fileBytes = new byte[byteSize];

                        for (int j = 0; j < indexCount; j++)
                        {
                            count = stream.Read(fileBytes, 0, fileBytes.Length);
                            fs.Write(fileBytes, 0, count);  //File Receive
                        }
                    }
                    log.Info(serverFile[2] + " Receive Complete: " + client.Client.RemoteEndPoint.ToString());
                }
                else
                {
                    bytes = new byte[10];
                    byteLength = stream.Read(bytes, 0, bytes.Length);   //Next File Request Send Receive
                }                
            }
        }

        #region FileCompare
        /// <summary>
        /// File Compare
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="lastAccessTime"></param>
        /// <returns></returns>
        private bool FileCompare(string fileName, string length)
        {
            if (File.Exists(fileName))
            {
                FileInfo file = new FileInfo(fileName);
                if (length.Equals(file.Length.ToString()))
                    return false;
            }
            return true;
        }
        #endregion

        #endregion

        #region Download
        /// <summary>
        /// Download
        /// </summary>
        /// <param name="stream"></param>
        private void Download(TcpClient client)
        {
            using (NetworkStream stream = client.GetStream())
            {
                byte[] bytes = new byte[10];
                bytes = Encoding.Default.GetBytes("FileCount");
                stream.Write(bytes, 0, bytes.Length);   //FileCount Request Send

                bytes = new byte[10];
                int byteLength = stream.Read(bytes, 0, bytes.Length);   //File Count Receive
                int fileCount = Convert.ToInt32(Encoding.Default.GetString(bytes, 0, byteLength));

                bytes = new byte[10];
                int byteSize = 8192;
                bytes = Encoding.Default.GetBytes(byteSize.ToString());
                stream.Write(bytes, 0, bytes.Length);   //ByteSize Send

                for (int i = 0; i < fileCount; i++)
                {
                    FileRead(client, byteSize);
                }
            }
        }

        /// <summary>
        /// File Read and Stream Write
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="byteSize"></param>
        private void FileRead(TcpClient client, int byteSize)
        {
            NetworkStream stream = client.GetStream();
            
            byte[] bytes = new byte[256];
            int byteLength = stream.Read(bytes, 0, bytes.Length);   //File Path Receive
            string path = Encoding.Default.GetString(bytes, 0, byteLength);

            FileInfo file = new FileInfo(path);
            bytes = new byte[64];

            if (file.Exists)
            {
                bytes = Encoding.Default.GetBytes(file.LastAccessTime.ToString() + "*" + file.Length.ToString());
                stream.Write(bytes, 0, bytes.Length);   //File Length Send

                bytes = new byte[4];
                byteLength = stream.Read(bytes, 0, bytes.Length);   // Receive
                string exist = Encoding.Default.GetString(bytes, 0, byteLength);

                if (exist.Equals("OK")) //Require Update
                {
                    log.Info(path + " Send: " + client.Client.RemoteEndPoint.ToString());
                    using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                    {
                        int count = 0;
                        byte[] fileBytes = new byte[byteSize];

                        while ((count = fs.Read(fileBytes, 0, fileBytes.Length)) > 0)
                        {
                            stream.Write(fileBytes, 0, count);      //File Send
                        }
                    }
                    log.Info(path + " Send Complete: " + client.Client.RemoteEndPoint.ToString());
                }
                else
                {
                    bytes = new byte[10];
                    bytes = Encoding.Default.GetBytes("NextFile");
                    stream.Write(bytes, 0, bytes.Length);   //Next File Request Send
                }
            }
            else
            {
                bytes = Encoding.Default.GetBytes("NoFiles");
                stream.Write(bytes, 0, bytes.Length);   //NoFiles Send
            }
            file = null;
        }
        #endregion

        #region AutoUpdate
        /// <summary>
        /// AutoUpdate
        /// </summary>
        /// <param name="stream"></param>
        private void AutoUpdate(TcpClient client)
        {
            using (NetworkStream stream = client.GetStream())
            {
                string dir = ConfigurationManager.AppSettings["AutoUpdateDirectory"];
                List<string> files = new List<string>(Directory.EnumerateFiles(dir));
                string fileInfo = "";
                int byteSize = 8192;

                foreach (string fileName in files)
                {
                    FileInfo file = new FileInfo(fileName);
                    fileInfo += file.Name + "*" + file.LastAccessTime.ToString() + "*" + file.Length.ToString() + "/";
                }

                byte[] bytes = new byte[1024];
                bytes = Encoding.Default.GetBytes(fileInfo == "" ? "NoFiles" : fileInfo + byteSize);
                stream.Write(bytes, 0, bytes.Length);   //File Info Send

                while (!fileInfo.Equals(""))
                {
                    bytes = new byte[256];
                    int byteLength = stream.Read(bytes, 0, bytes.Length);   //File Request Receive
                    string requestFile = Encoding.Default.GetString(bytes, 0, byteLength);

                    if (requestFile == "") break;

                    byte[] fileBytes = new byte[byteSize];
                    using (FileStream fs = new FileStream(dir + requestFile, FileMode.Open, FileAccess.Read))
                    {
                        int count = 0;
                        log.Info(dir + requestFile + " Send: " + client.Client.RemoteEndPoint.ToString());
                        while ((count = fs.Read(fileBytes, 0, fileBytes.Length)) > 0)
                        {
                            stream.Write(fileBytes, 0, count);      //File Send
                        }
                        log.Info(dir + requestFile + " Send Complete: " + client.Client.RemoteEndPoint.ToString());
                    }
                }
            }
        }
        #endregion


        /// <summary>
        /// Client의 IP 주소를 가져옵니다.
        /// </summary>
        /// <returns></returns>
        private IPAddress GetIPAddress()
        {
            IPHostEntry host = null;

            try
            {
                host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (IPAddress ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                        return ip;
                }
            }
            catch (Exception e)
            {
                log.Info(e);
            }
            finally
            {
                host = null;
            }

            return null;
        }
    }
}
