using System;

namespace simple_web_server2
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Server is starting...");
            HTTPserver myserver = new HTTPserver(8080);
            myserver.Start();
        }
    }
}
