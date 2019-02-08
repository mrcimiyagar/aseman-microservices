using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite.Internal.ApacheModRewrite;

namespace ServiceManager.Controllers
{
    [Route("api/[controller]")]
    public class ServiceController : Controller
    {
        private const string PassKey = "5hsS6tr4ryj49r8J65r4ayak8kay8k4KkaV6j54wf";
        
        private readonly string[] _serviceTitles = new string[]
        {
            "ApiGateway",
            "BotService",
            "CityService",
            "DesktopService",
            "EntryService",
            "FileService",
            "MessengerService",
            "SearchService",
            "StoreService"
        };

        private string ServicesRoot = @"C:\Users\Administrator\Desktop\Aseman Backend Services\";
        private const string ServicesFiles = @"C:\Users\Administrator\Desktop\Aseman Backend";
        //private const string ServicesRoot = @"F:\Desktop\Aseman Backend\Aseman Backend batches\";
        //private const string ServicesFiles=  @"F:\Desktop\Aseman Backend";

        private readonly object _objectLock = new object();

        [HttpGet("[action]")]
        public IEnumerable<ServiceData> GetServiceData(string token)
        {
            if (PassKey == token)
            {
                lock (_objectLock)
                {
                    var processlist = Process.GetProcesses();

                    var openWindows = new HashSet<string>();

                    foreach (var process in processlist)
                    {
                        if (string.IsNullOrEmpty(process.MainWindowTitle)) continue;
                        if (!openWindows.Contains(process.MainWindowTitle))
                            openWindows.Add(process.MainWindowTitle);
                    }

                    var serviceDataList = new List<ServiceData>();
                    var counter = 0;
                    foreach (var serviceTitle in _serviceTitles)
                    {
                        serviceDataList.Add(new ServiceData()
                        {
                            Id = counter,
                            Name = serviceTitle,
                            State = openWindows.Contains("Administrator:  " + serviceTitle) ? "Running" : "Stopped"
                        });

                        counter++;
                    }

                    return serviceDataList;
                }
            }
            
            return new List<ServiceData>();
        }

        [HttpGet("[action]")]
        public string RunService(string token, long serviceId)
        {
            if (PassKey == token)
            {
                lock (_objectLock)
                {
                    var processlist = Process.GetProcesses();

                    var openWindows = new HashSet<string>();

                    foreach (var process in processlist)
                    {
                        if (string.IsNullOrEmpty(process.MainWindowTitle)) continue;
                        if (!openWindows.Contains(process.MainWindowTitle))
                            openWindows.Add(process.MainWindowTitle);
                    }

                    if (!openWindows.Contains(_serviceTitles[serviceId]))
                    {
                        var pInfo = new ProcessStartInfo
                        {
                            UseShellExecute = true,
                            CreateNoWindow = false,
                            WindowStyle = ProcessWindowStyle.Normal,
                            FileName = ServicesRoot + _serviceTitles[serviceId].ToLower() + ".bat"
                        };
                        Console.WriteLine("Starting Service " + _serviceTitles[serviceId]);
                        Process.Start(pInfo);
                    }

                    return "{ \"status\" : \"success\" }";
                }
            }

            return "{ \"status\" : \"failure\" }";
        }
        
        [HttpGet("[action]")]
        public string StopService(string token, long serviceId)
        {
            if (PassKey == token)
            {
                lock (_objectLock)
                {
                    var processlist = Process.GetProcesses();

                    foreach (var process in processlist)
                    {
                        if (string.IsNullOrEmpty(process.MainWindowTitle)) continue;
                        if (process.MainWindowTitle == ("Administrator:  " + _serviceTitles[serviceId]))
                        {
                            Console.WriteLine("Stopping Service " + _serviceTitles[serviceId]);
                            process.CloseMainWindow();
                        }
                    }

                    return "{ \"status\" : \"success\" }";
                }
            }
            
            return "{ \"status\" : \"failure\" }";
        }

        [HttpPost("[action]")]
        public string UpdateService([FromForm] UploadForm form)
        {
            lock (_objectLock)
            {
                var file = form.File;
                var token = form.Token;
                if (PassKey == token)
                {
                    var serviceDirName = _serviceTitles[form.ServiceId];
                    if (file.Length <= 0) return "{ \"status\" : \"failure\" }";
                    var path = Path.Combine(Path.Combine(ServicesFiles, serviceDirName)
                        , file.FileName);
                    System.IO.File.Delete(path);
                    using (var fs = new FileStream(path, FileMode.Create))
                        file.CopyToAsync(fs).Wait();
                    var redirectUrl = Request.GetDisplayUrl();
                    redirectUrl = redirectUrl.Substring(0, redirectUrl.IndexOf(
                        "/api/Service/UpdateService", StringComparison.Ordinal));
                    redirectUrl = redirectUrl.EndsWith("/")
                        ? redirectUrl.Substring(0, redirectUrl.Length - 1)
                        : redirectUrl;
                    Response.Redirect(redirectUrl + "/" + token);
                    return "{ \"status\" : \"success\" }";
                }
                
                return "{ \"status\" : \"failure\" }";
            }
        }

        public class ServiceData
        {   
            public int Id { get; set; }
            public string Name { get; set; }
            public string State { get; set; }
        }

        public class UploadForm
        {
            public IFormFile File { get; set; }
            public int ServiceId { get; set; }
            public string Token { get; set; }
        }
    }
}