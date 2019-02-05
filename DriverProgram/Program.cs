using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace DriverProgram
{
    class Program
    {
        static void Main(string[] args)
        {
            const string rootPath = @"C:\Users\Administrator\Desktop\Aseman Backend\";
            //const string rootPath = @"F:\Desktop\Aseman Backend\";
            
            var paths = new[]
            {
                new KeyValuePair<string, string>(
                    "apigateway", "apigateway.dll"),
                new KeyValuePair<string, string>(
                    "botservice", "botplatform.dll"),
                new KeyValuePair<string, string>(
                    "cityservice", "cityplatform.dll"),
                new KeyValuePair<string, string>(
                    "desktopservice", "desktopplatform.dll"),
                new KeyValuePair<string, string>(
                    "entryservice", "entryplatform.dll"),
                new KeyValuePair<string, string>(
                    "fileservice", "fileservice.dll"),
                new KeyValuePair<string, string>(
                    "messengerservice", "messengerplatform.dll"),
                new KeyValuePair<string, string>(
                    "searchservice", "searchplatform.dll"),
                new KeyValuePair<string, string>(
                    "storeservice", "storeplatform.dll")
            };

            foreach (var pair in paths)
            {
                new Thread(() =>
                {
                    var cmd = Process.Start("cmd.exe", "/C " + 
                                             rootPath.Substring(0, 2) + "&" + 
                                             "cd " + rootPath + pair.Key + "&" +
                                             "dotnet " + pair.Value);
                    cmd?.WaitForExit();
                }).Start();
            }
        }
    }
}