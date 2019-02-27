using System;
using System.Net;
using System.Text;

namespace Hello
{
    class Program
    {
        static void Main(string[] args)
        {
            var listener = new HttpListener();
            listener.Prefixes.Add("http://+:8088/api");
            listener.Start();
            Console.WriteLine("Listening on http://+:8088/api");
            while (listener.IsListening)
            {
                var context = listener.GetContext();
                var request = context.Request;
                Console.WriteLine($"{DateTime.Now}: {request.UserHostAddress}");
                var response = context.Response;
                var buffer = Encoding.UTF8.GetBytes("<html><body><H1>Hello Service Fabric</H1></body></html>");
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
                response.Close();
                Console.WriteLine();
            }
        }
    }
}