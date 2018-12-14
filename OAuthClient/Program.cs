using Galaxy.Mercury.OAuthClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            OAuthClientExtensions.Init("dfsaf", "http://192.168.1.1//");
            string url = "api/get/1";
            var a = url.M5_APIGet<string>();
        }
    }
}
