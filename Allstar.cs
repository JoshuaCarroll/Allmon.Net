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

                Debug.WriteLine("Socket connected to -> {0} ", socket.RemoteEndPoint.ToString());

                Send($"ACTION: LOGIN\r\nUSERNAME: {AppConfig.Get("Allmon:hostedNodes:0:user")}\r\nSECRET: {AppConfig.Get("Allmon:hostedNodes:0:password")}\r\nEVENTS: 0\r\nActionID: {"1"}\r\n\r\n");
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

            Connect();
            int byteSent = socket.Send(Encoding.ASCII.GetBytes(message));

            // Data buffer
            byte[] messageReceived = new byte[1024];

            // We receive the message using the method Receive(). This method returns number of bytes received, that we'll use to convert them to string
            int byteRecv = socket.Receive(messageReceived);
            Debug.WriteLine(Encoding.ASCII.GetString(messageReceived, 0, byteRecv));

            return response;
        }

        public static string RequestStatus(string localNode)
        {
            return Send($"ACTION: RptStatus\r\nCOMMAND: XStat\r\nNODE: {localNode}\r\nActionID: 2\r\n\r\n");
        }

        public static string RequestConn(string localNode)
        {
            return Send($"ACTION: RptStatus\r\nCOMMAND: SawStat\r\nNODE: {localNode}\r\nActionID: 2\r\n\r\n");
        }

        public static string SendCommand(string command)
        {
            return Send($"ACTION: COMMAND\r\nCOMMAND: {command}\r\nActionID: $actionID\r\n\r\n");
        }
    }


}
