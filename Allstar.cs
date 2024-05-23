using Microsoft.Extensions.Hosting;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Diagnostics;

namespace AllmonNet
{
    public static class Allstar
    {
        private static Socket socket;

        public static void Connect()
        {
            if (socket == null || !socket.Connected)
            {
                /// TODO: Make this into a loop that does the same for each local node.
                string host = AppConfig.Get("Allmon:hostedNodes:0:host");
                int port = int.Parse(AppConfig.Get("Allmon:hostedNodes:0:port"));

                IPHostEntry ipHost = Dns.GetHostEntry(host);
                IPAddress ipAddr = ipHost.AddressList[0];
                IPEndPoint localEndPoint = new IPEndPoint(ipAddr, port);
                socket = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                socket.Connect(localEndPoint);

                Debug.WriteLine($"Socket connected to {socket.RemoteEndPoint.ToString()}");

                Send($"ACTION: LOGIN\r\nUSERNAME: {AppConfig.Get("Allmon:hostedNodes:0:user")}\r\nSECRET: {AppConfig.Get("Allmon:hostedNodes:0:password")}\r\nEVENTS: 0\r\nActionID: Login\r\n\r\n");
            }
        }

        public static void Disconnect()
        {
            if (socket != null && socket.Connected)
            {
                Send("ACTION: Logoff\r\n\r\n");
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
        }

        public static string Send(string message)
        {
            string response = "";

            if (socket == null || !socket.Connected)
            {
                Connect();
            }

            int byteSent = socket.Send(Encoding.ASCII.GetBytes(message));

            // Data buffer
            byte[] messageReceived = new byte[1024];

            // We receive the message using the method Receive(). This method returns number of bytes received, that we'll use to convert them to string.
            int byteRecv = socket.Receive(messageReceived);
            response = Encoding.ASCII.GetString(messageReceived, 0, byteRecv);
            Debug.WriteLine("    " + response.Replace(Environment.NewLine, Environment.NewLine + "    "));

            return response;
        }

        public static AsteriskResponse RequestStatus(string localNode)
        {
            Debug.WriteLine("__RequestStatus_____");
            AsteriskResponse output = new AsteriskResponse(Send($"ACTION: RptStatus\r\nCOMMAND: XStat\r\nNODE: {localNode}\r\nActionID: XStat\r\n\r\n"));
            return output;
        }

        public static AsteriskResponse RequestConn(string localNode)
        {
            Debug.WriteLine("__RequestConn_____");
            AsteriskResponse output = new AsteriskResponse(Send($"ACTION: RptStatus\r\nCOMMAND: SawStat\r\nNODE: {localNode}\r\nActionID: SawStat\r\n\r\n"));
            return output;
        }

        public static string SendCommand(string command)
        {
            Debug.WriteLine("__SendCommand " + command + "_____");
            return Send($"ACTION: COMMAND\r\nCOMMAND: {command}\r\nActionID: {command}\r\n\r\n");
        }
    }

    public class AsteriskResponse {
        public string Response = "";
        public string ActionID = "";
        public string Node = "";
        public string Message = "";
        public string LinkedNodes = "";
        public string RPT_TXKEYED = "";
        public string RPT_NUMLINKS = "";
        public string RPT_LINKS = "";
        public string RPT_NUMALINKS = "";
        public string RPT_ALINKS = "";
        public string RPT_RXKEYED = "";
        public string RPT_AUTOPATCHUP = "";
        public string RPT_ETXKEYED = "";
        public string TRANSFERCAPABILITY = "";
        public string parrot_ena = "";
        public string sys_ena = "";
        public string tot_ena = "";
        public string link_ena = "";
        public string patch_ena = "";
        public string patch_state = "";
        public string sch_ena = "";
        public string user_funs = "";
        public string tail_type = "";
        public string iconns = "";
        public string tot_state = "";
        public string ider_state = "";
        public string tel_mode = "";
        public List<AsteriskConnection> Connections = new List<AsteriskConnection>();

        public AsteriskResponse(string input) {
            string[] lines = input.Split(new char[] { '\n' });

            foreach (string line in lines)
            {
                string[] parts = line.Split(": ", 2);
                switch (parts[0])
                {
                    case "ActionID":
                        ActionID = parts[1];
                        break;
                    case "Response":
                        Response = parts[1];
                        break;
                    case "Node":
                        Node = parts[1];
                        break;
                    case "Conn":
                        Connections.Add(new AsteriskConnection(parts[1]));
                        break;
                    case "LinkedNodes":
                        LinkedNodes = parts[1];
                        break;
                    case "parrot_ena":
                        parrot_ena = parts[1];
                        break;
                    case "sys_ena":
                        sys_ena = parts[1];
                        break;
                    case "tot_ena":
                        tot_ena = parts[1];
                        break;
                    case "link_ena":
                        link_ena = parts[1];
                        break;
                    case "patch_ena":
                        patch_ena = parts[1];
                        break;
                    case "patch_state":
                        patch_state = parts[1];
                        break;
                    case "sch_ena":
                        sch_ena = parts[1];
                        break;
                    case "user_funs":
                        user_funs = parts[1];
                        break;
                    case "tail_type":
                        tail_type = parts[1];
                        break;
                    case "iconns":
                        iconns = parts[1];
                        break;
                    case "tot_state":
                        tot_state = parts[1];
                        break;
                    case "ider_state":
                        ider_state = parts[1];
                        break;
                    case "tel_mode":
                        tel_mode = parts[1];
                        break;
                    default:
                        break;
                }
            }

        }
    }

    public class AsteriskConnection
    {
        public string Node;
        public string IpAddress;
        public string IsKeyed;
        public string Direction;
        public string LinkTime;
        public string LinkStatus;
        public TimeSpan KeyedTime;
        public TimeSpan UnkeyedTime;
        
        public AsteriskConnection(string input)
        {
            string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            int intNode = 0;
            if (int.TryParse(parts[0], out intNode))
            {
                if (intNode > 0 && intNode < 3000000)
                {
                    if (parts.Length == 6)
                    {
                        // Xstat response
                        Node = parts[0];
                        IpAddress = parts[1];
                        IsKeyed = parts[2];
                        Direction = parts[3];
                        LinkTime = parts[4];
                        LinkStatus = parts[5];
                    }
                    else
                    {
                        // SawStat response
                        Node = parts[0];
                        IsKeyed = parts[1];
                        KeyedTime = TimeSpan.FromSeconds(double.Parse(parts[2]));
                        UnkeyedTime = TimeSpan.FromSeconds(double.Parse(parts[3]));
                    }
                }
                else
                {
                    // Echolink connection
                    Node = parts[0];
                    IsKeyed = parts[1];
                    Direction = parts[2];
                    LinkTime = parts[3];
                    LinkStatus = parts[4];
                }
            }
            else
            {
                Debug.WriteLine("Unable to parse connection data: " + input);
            }

        }
    }
}
