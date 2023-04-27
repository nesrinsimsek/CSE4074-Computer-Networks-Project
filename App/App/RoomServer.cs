using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace App
{
    internal class RoomServer
    {

        private bool running = false;
        private TcpListener listener = null;
        private int portNumber;
        public RoomServer(int port)
        {
            portNumber = port;

            if(portNumber == 8081)
                listener = new TcpListener(IPAddress.Any, port);
        }

        public void Start()
        {
            if (portNumber == 8081)
            {
                Thread serverThread = new Thread(new ThreadStart(Run));
                serverThread.Start();
            }
        }

        private void Run()
        {
            running = true;
            listener.Start();

            while (running)
            {
                
                TcpClient client = listener.AcceptTcpClient();
                HandleClient(client);
                client.Close();
            }

            running = false;
            listener.Stop();
        }

        private void HandleClient(TcpClient client)
        {
            StreamReader sr = new StreamReader(client.GetStream());
            string msg = "";

            while(sr.Peek() != -1)
            {
                msg += sr.ReadLine() + "\n";
            }
            Console.WriteLine("Request: \n" + msg);

            RoomRequest req = RoomRequest.GetRequest(msg);
            RoomResponse resp = null;
            if (req != null) resp = RoomResponse.From(req);
                
            if (resp != null) resp.Post(client.GetStream());
        }
    }
}
