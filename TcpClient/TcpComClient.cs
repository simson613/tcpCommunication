using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace TcpComClientDll
{
    public class TcpComClient    //string flag, string downloadPath, string uploadPath, out Exception exception
    {
        private string serverIP;
        private int serverPort;

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="serverIP"></param>
        /// <param name="serverPort"></param>
        public TcpComClient(string serverIP, int serverPort)
        {
            this.serverIP = serverIP;
            this.serverPort = serverPort;
        }

        #region Init
        /// <summary>
        /// Init Client / Set File Transfer
        /// </summary>
        /// <param name="flag"></param>
        /// <returns></returns>
        private TcpClient Init(string flag)
        {
            int bindPort = 56351;
            while (true)
            {
                if (portAvailable(bindPort))
                    break;
                else
                    bindPort++;
            }

            try
            {
                IPEndPoint clientAddress = new IPEndPoint(GetIPAddress(), bindPort);
                IPEndPoint serverAddress = new IPEndPoint(IPAddress.Parse(serverIP), serverPort);
                TcpClient client = new TcpClient(clientAddress);
                client.Connect(serverAddress);

                NetworkStream stream = client.GetStream();

                byte[] bytes = new byte[10];
                bytes = Encoding.Default.GetBytes(flag);
                stream.Write(bytes, 0, bytes.Length);   //flag Send

                return client;
            }
            catch (SocketException se)
            {
                Console.WriteLine(se);
            }
            catch (ArgumentNullException ne)
            {
                Console.WriteLine(ne.Message);
            }
            return null;
        }

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
                Console.WriteLine(e);
            }
            finally
            {
                host = null;
            }

            return null;
        }

        /// <summary>
        /// Port를 사용할 수 있는지 확인합니다.
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        private bool portAvailable(int port)
        {
            IPGlobalProperties iPGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnInfoArray = iPGlobalProperties.GetActiveTcpConnections();
            foreach (TcpConnectionInformation tcpi in tcpConnInfoArray)
            {
                if (tcpi.LocalEndPoint.Port == port)
                    return false;
            }
            return true;
        }
        #endregion

        #region Upload
        /// <summary>
        /// Upload Init
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="fileCount"></param>
        /// <returns></returns>
        private string UploadInit(NetworkStream stream, int fileCount)
        {
            try
            {
                byte[] bytes = new byte[10];
                int byteLength = stream.Read(bytes, 0, bytes.Length);   //ByteSize Receivce
                string byteSize = Encoding.Default.GetString(bytes, 0, byteLength);

                bytes = new byte[10];
                bytes = Encoding.Default.GetBytes(fileCount.ToString());
                stream.Write(bytes, 0, bytes.Length);   //File Count Send

                return byteSize;
            }
            catch (Exception ce)
            {
                Console.WriteLine(ce);
                return null;
            }
        }

        /// <summary>
        /// File Upload
        /// </summary>
        /// <param name="clientPath"></param>
        /// <param name="serverPath"></param>
        public void Upload(string clientPath, string serverPath)
        {
            try
            {
                TcpClient client = Init("Upload");
                using (NetworkStream stream = client.GetStream())
                {
                    if (File.Exists(clientPath))
                        UploadLogic(stream, clientPath, serverPath, Convert.ToInt32(UploadInit(stream, 1)));
                    else
                        Console.WriteLine(clientPath + " File Not Exist");
                }

                if (client != null)
                {
                    client.Close();
                    client = null;
                }
            }
            catch (Exception ce)
            {
                Console.WriteLine(ce);
            }
        }

        /// <summary>
        /// File List Upload
        /// </summary>
        /// <param name="clientPath"></param>
        /// <param name="serverPath"></param>
        public void UploadList(List<string> clientPath, List<string> serverPath)
        {
            try
            {
                TcpClient client = Init("Upload");
                using (NetworkStream stream = client.GetStream())
                {
                    int byteSize = Convert.ToInt32(UploadInit(stream, clientPath.Count));

                    for (int i = 0; i < clientPath.Count; i++)
                    {
                        if (File.Exists(clientPath[i]))
                            UploadLogic(stream, clientPath[i], serverPath[i], byteSize);
                        else
                            Console.WriteLine(clientPath[i] + " File Not Exist");
                    }
                }

                if (client != null)
                {
                    client.Close();
                    client = null;
                }
            }
            catch (Exception ce)
            {
                Console.WriteLine(ce);
            }
        }

        /// <summary>
        /// File Upload
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="clientPath"></param>
        /// <param name="serverPath"></param>
        /// <param name="byteSize"></param>
        private void UploadLogic(NetworkStream stream, string clientPath, string serverPath, int byteSize)
        {
            byte[] bytes = new byte[10];
            int byteLength = stream.Read(bytes, 0, bytes.Length);   // Receive

            FileInfo file = new FileInfo(clientPath);
            bytes = new byte[256];

            if (file.Exists)
            {
                bytes = Encoding.Default.GetBytes(
                    file.LastAccessTime.ToString() + "*" + file.Length.ToString() + "*" + serverPath);
                stream.Write(bytes, 0, bytes.Length);   //File LastAccessTime, Length Send

                bytes = new byte[4];
                byteLength = stream.Read(bytes, 0, bytes.Length);   // Receive
                string exist = Encoding.Default.GetString(bytes, 0, byteLength);

                if (exist.Equals("OK")) //최신파일이 아닐떄
                {
                    int count = 0;
                    byte[] fileBytes = new byte[byteSize];

                    using (FileStream fs = new FileStream(clientPath, FileMode.Open, FileAccess.Read))
                    {
                        while ((count = fs.Read(fileBytes, 0, fileBytes.Length)) > 0)
                        {
                            stream.Write(fileBytes, 0, count);      //File Send
                        }
                    }
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
                stream.Write(bytes, 0, bytes.Length);   // NoFiles Send
                Console.WriteLine("NoFiles");
            }
        }
        #endregion

        #region Download
        /// <summary>
        /// Download Init
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="fileCount"></param>
        /// <returns></returns>
        private string DownloadInit(NetworkStream stream, int fileCount)
        {
            try
            {
                byte[] bytes = new byte[10];
                int byteLength = stream.Read(bytes, 0, bytes.Length);   //FileCount Request Receivce

                bytes = new byte[10];
                bytes = Encoding.Default.GetBytes(fileCount.ToString());
                stream.Write(bytes, 0, bytes.Length);   //File Count Send

                bytes = new byte[10];
                byteLength = stream.Read(bytes, 0, bytes.Length);   //ByteSize Receivce
                return Encoding.Default.GetString(bytes, 0, byteLength);
            }
            catch (Exception ce)
            {
                Console.WriteLine(ce);
                return null;
            }
        }

        /// <summary>
        /// File Download
        /// </summary>
        /// <param name="clientPath">Path + FileName</param>
        /// <param name="serverPath">Path + FileName</param>
        public void Download(string clientPath, string serverPath)
        {
            try
            {
                TcpClient client = Init("Download");
                using (NetworkStream stream = client.GetStream())
                {
                    DownloadLogic(stream, clientPath, serverPath, Convert.ToInt32(DownloadInit(stream, 1)));
                }

                if (client != null)
                {
                    client.Close();
                    client = null;
                }
            }
            catch (Exception ce)
            {
                Console.WriteLine(ce);
            }
        }

        /// <summary>
        /// File List Download
        /// </summary>
        /// <param name="clientPath">List (Path + Filename)</param>
        /// <param name="serverPath">List (Path + Filename)</param>
        public void DownloadList(List<string> clientPath, List<string> serverPath)
        {
            try
            {
                TcpClient client = Init("Download");
                using (NetworkStream stream = client.GetStream())
                {
                    int byteSize = Convert.ToInt32(DownloadInit(stream, clientPath.Count));

                    for (int i = 0; i < clientPath.Count; i++)
                    {
                        DownloadLogic(stream, clientPath[i], serverPath[i], byteSize);
                    }
                }

                if (client != null)
                {
                    client.Close();
                    client = null;
                }
            }
            catch (Exception ce)
            {
                Console.WriteLine(ce);
            }
        }

        /// <summary>
        /// File Download
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="clientPath"></param>
        /// <param name="serverPath"></param>
        /// <param name="byteSize"></param>
        private void DownloadLogic(NetworkStream stream, string clientPath, string serverPath, int byteSize)
        {
            byte[] bytes = new byte[256];
            bytes = Encoding.Default.GetBytes(serverPath);
            stream.Write(bytes, 0, bytes.Length);   //File Path Send

            bytes = new byte[64];
            int byteLength = stream.Read(bytes, 0, bytes.Length);   //File Length Receivce / NoFiles Receive
            string file = Encoding.Default.GetString(bytes, 0, byteLength);

            if (!file.Equals("NoFiles"))
            {
                string[] serverFile = file.Split('*');

                bool exist = FileCompare(clientPath, serverFile[1]);
                bytes = new byte[4];
                bytes = Encoding.Default.GetBytes(exist ? "OK" : "NO");
                stream.Write(bytes, 0, bytes.Length);   // Send

                if (exist)
                {
                    byte[] fileBytes = new byte[byteSize];
                    int count = 0;
                    int indexCount = Convert.ToInt32(serverFile[1]) / byteSize + 1;

                    using (FileStream fs = new FileStream(clientPath, FileMode.Create, FileAccess.Write))
                    {
                        for (int j = 0; j < indexCount; j++)
                        {
                            count = stream.Read(fileBytes, 0, fileBytes.Length);
                            fs.Write(fileBytes, 0, count);  //File Receive
                        }
                    }
                }
                else
                {
                    bytes = new byte[10];
                    byteLength = stream.Read(bytes, 0, bytes.Length);   //Next File Request Send Receive
                }
            }
            else
            {
                Console.WriteLine("NoFiles");
            }
        }
        #endregion

        #region AutoUpdate
        /// <summary>
        /// Auto Update
        /// </summary>
        /// <returns></returns>
        public TcpClient ClientAutoUpdate()
        {
            return Init("AutoUpdate");
        }
        #endregion

        #region FileCompare
        /// <summary>
        /// File Compare
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="lastAccessTime"></param>
        /// <returns></returns>
        public bool FileCompare(string fileName, string length)
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
    }
}
