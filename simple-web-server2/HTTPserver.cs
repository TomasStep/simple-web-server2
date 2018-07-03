using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace simple_web_server2
{
    public class HTTPserver
    {
        //Server status
        private bool isOn = false;
        //Directory of main index.html file
        public const String webDir = "/web/";
        //Directory of error html file
        public const String errDir = "/error/";
        //Socket mechanism
        private TcpListener listener;
        //constructor
        public HTTPserver(int port)
        {
            //Initialize listener, assigning any available IP with assigned port
            listener = new TcpListener(IPAddress.Any, port);
        }
        public void Start()
        {
            //creating a server thread and starting the service
            Thread serverThread = new Thread(new ThreadStart(Run));
            serverThread.Start();
        }
        private void Run()
        {
            Console.WriteLine("Server is On! \nwaiting for clients...");
            isOn = true;
            listener.Start();
            while (isOn)
            {
                TcpClient client = listener.AcceptTcpClient();
                Console.WriteLine("Client connected successfuly!");
                infoClient(client);
                client.Close();

            }          
            Stop();
        }
        private void infoClient(TcpClient client)
        {
            StreamReader reader = new StreamReader(client.GetStream());
            String msg = "";
            while (reader.Peek() != -1)
            {
                msg += reader.ReadLine() + "\n";
            }
            Debug.WriteLine("Request: \n" + msg);

            Request req = Request.GetRequest(msg);
            Response resp = Response.From(req);
            resp.Post(client.GetStream());
        }
        public void Stop()
        {
            isOn = false;
            listener.Stop();
        }
    }
    class Request
    {
        public String Type { get; set; }
        public String Url { get; set; }
        public String Host { get; set; }
        private Request(String type, String url, String host)
        {
            Type = type;
            Url = url;
            Host = host;
        }
        public static Request GetRequest(String request)
        {
            if (string.IsNullOrEmpty(request))
            {
                Console.WriteLine("null or empty");
                return null;
            }
            string[] tokens = request.Split(' ');
            string type = tokens[0];
            string url = tokens[1];
            string host = tokens[4];
            return new Request(type, url, host);

        }
    }
    class Response
    {
        private Byte[] data = null;
        private String status;
        private String mime;
        private Response(String status, String mime, Byte[] data)
        {
            this.status = status;
            this.mime = mime;
            this.data = data;
        }
        public static Response From(Request request)
        {
            if (request == null || request.Type != "GET")
                badRequest();
            else
            {
                String file = Environment.CurrentDirectory + HTTPserver.webDir + request.Url;
                FileInfo f = new FileInfo(file);
                if(f.Exists && f.Extension.Contains("."))
                {
                    return loadSite(f);
                }
                else
                {
                    DirectoryInfo di = new DirectoryInfo(f + "/");
                    if (!di.Exists)
                        return badRequest();
                    FileInfo[] files = di.GetFiles();
                    foreach(FileInfo ff in files)
                    {
                        String n = ff.Name;
                        if (n.Contains("index.html"))
                            return loadSite(ff);
                    }
                    
                }
            }
            return badRequest();
        }
        private static Response loadSite(FileInfo f)
        {
            //Console.WriteLine("GOOD request!!!");
            FileStream fs = f.OpenRead();
            BinaryReader reader = new BinaryReader(fs);
            Byte[] d = new byte[fs.Length];
            reader.Read(d, 0, d.Length);
            fs.Close();
            return new Response("200 OK", "text/html", d);
        }
        private static Response badRequest()
        {
            //Console.WriteLine("Bad request...");
            String file = Environment.CurrentDirectory + HTTPserver.errDir + "error.html";
            FileInfo fi = new FileInfo(file);
            FileStream fs = fi.OpenRead();
            BinaryReader reader = new BinaryReader(fs);
            Byte[] d = new byte[fs.Length];
            reader.Read(d, 0, d.Length);
            fs.Close();
            return new Response("Bad request", "text/html", new Byte[0]);
        }
        public void Post(NetworkStream stream)
        {
            StreamWriter writer = new StreamWriter(stream);
            writer.WriteLine(String.Format("HTTP/1.1 Test Server\n\n"));
            writer.Flush();
            stream.Write(data, 0, data.Length);
        }
    }
}
