using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;
using System.Drawing;
using System.Text.RegularExpressions;
using Newtonsoft;
using System.Globalization;
using EdmontonDrawingValidator.Model;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using SharedClasses;
using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Scaffolding;
using System.Net;
using System.Threading;
using System.Windows;
using System.Diagnostics;

//sc delete "DamanDataProcessor"
//SC CREATE "DamanDataProcessor" binpath= "F:\BKPatel\DAMAN-DCR\Daman-SVPAS-DXF-Processor\bin\Release\net6.0\DamanSPVASDXFProcessor.exe"
//

namespace EdmontonDrawingValidator
{
    public class Program
    {
        private static ConcurrentQueue<string> lstFileInProcess = new ConcurrentQueue<string>();
        private static object objLock = new object();
        private static Task[] lstTasks = new Task[5];

        private static bool AlreadyRunning()
        {
            Process[] processes = Process.GetProcesses();
            Process currentProc = Process.GetCurrentProcess();
            //logger.LogDebug("Current proccess: {0}", currentProc.ProcessName);
            foreach (Process process in processes)
            {
                if (currentProc.ProcessName == process.ProcessName && currentProc.Id != process.Id)
                {
                    //logger.LogInformation("Another instance of this process is already running: {pid}", process.Id);
                    return true;
                }
            }
            return false;
        }

        public static void Main(string[] args)
        {
            if (AlreadyRunning())
            {
                Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")} : Application is already running! Exiting the application.");
                Console.WriteLine();
                Console.ReadLine();
                return;
            }

            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

            //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;

            var config = new ConfigurationBuilder().SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location)).AddJsonFile("appsetting.json").Build();

            //AppContext
            //builder.Services.AddDbContext<EPAS_UserInterface.Models.Entities.appsContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("EPAS_UserInterface_Development")));

            var inputDataSection = config.GetSection("INPUT_DATA");

            General.InputFileExtension = inputDataSection["INPUT_FILE_EXTESION"];

            General.InputFolder = inputDataSection["INPUT_FOLDER"];

            General.DrawingDataProcessorInputFolder = inputDataSection["DRAWING_DATA_PROCESSOR_INPUT_FOLDER"]; 

            General.NoOfThreadsToStart = inputDataSection["NUMBER_OF_THREADS"];

            General.SleepTimeInMS = inputDataSection["SLEEP_TIME_IN_MS"];

            General.CreateDrawingFileYesNo = inputDataSection["CREATE_DRAWING_DATA_FILE_YES_NO"] ?? "no";

            General.RuleTesterBaseAPIUrl = inputDataSection["RULE_TESTER_BASE_API_URL"];

            General.RulesCheckingStatusUpdateURL = inputDataSection["RULES_CHECKING_STATUS_UPDATE_URL"];

            General.AddBuildingURL = inputDataSection["ADD_BUILDING_URL"];

            General.svgScale = inputDataSection["SVG_SCALE_VALUE"];

            General.errorHtmlDrawingTemplatePath = inputDataSection["TEMPLATE_HTML"];

            General.geometryPrecision = inputDataSection["GEOMETRI_PRECISION"] ?? "";

            General.AppName = inputDataSection["INSTANCE_NAME"] ?? "";

            //General.DisclaimerTextPositionValue = inputDataSection["DISCLAIMER_POSITION"] ?? "";

            lstTasks = new Task[General.WorkerThreadCount];

            try
            {
                if (lstFileInProcess.IsEmpty || lstFileInProcess.Count == 0)
                {
                    string[] arrInputFiles = Directory.GetFiles(General.InputFolder, General.InputFileExtension);
                    arrInputFiles = arrInputFiles.OrderBy(x => x).ToArray();
                    foreach (string sFile in arrInputFiles)
                    {
                        if (!lstFileInProcess.Contains(sFile))
                            lstFileInProcess.Enqueue(sFile);
                    }
                }
                using var watcher = new FileSystemWatcher(General.InputFolder);

                watcher.NotifyFilter = NotifyFilters.Attributes
                                     | NotifyFilters.CreationTime
                                     | NotifyFilters.DirectoryName
                                     | NotifyFilters.FileName
                                     | NotifyFilters.LastAccess
                                     | NotifyFilters.LastWrite
                                     | NotifyFilters.Security
                                     | NotifyFilters.Size;

                watcher.Changed += OnChanged;
                watcher.Created += OnCreated;
                //watcher.Deleted += OnDeleted;
                //watcher.Renamed += OnRenamed;
                watcher.Error += OnError;

                watcher.Filter = General.InputFileExtension;
                watcher.IncludeSubdirectories = true;
                watcher.EnableRaisingEvents = true;
                  
                Task t = Task.Factory.StartNew(async () => await StartProcess());

                Task.WaitAll(t);

                Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")} : Press enter to exit.");
                Console.ReadLine();
            }
            finally {

            }
        }
        private static void OnCreated(object sender, FileSystemEventArgs e)
        {
            string value = $"Created: {e.FullPath}";
            lock (objLock)
            {
                if (!lstFileInProcess.Contains(e.FullPath))
                    lstFileInProcess.Enqueue(e.FullPath);

            }
            
        }
        private static void OnChanged(object sender, FileSystemEventArgs e)
        {
        }

        private static void OnError(object sender, ErrorEventArgs e) =>
            PrintException(e.GetException());

        private static void PrintException(Exception? ex)
        {
            if (ex != null)
            {
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")} : Stacktrace:");
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine();
                PrintException(ex.InnerException);
            }
        }

        public async static Task StartProcess()
        { 
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($"{General.AppName}");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($" > RUNNING STATUS... <<<<<  ");
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.Write($"QUEUE: {lstFileInProcess.Count}");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"  ,  ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"PROCESS: {lstTasks.Count(x => x != null && !x.IsCompleted)}");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"  ,  ");
                //Console.ForegroundColor = ConsoleColor.Green;
                //Console.Write($"RUNNING THREADS: {lstTasks.Count(x => x != null && !x.IsCompleted)}");
                //Console.ForegroundColor = ConsoleColor.White;
                //Console.Write($"  ,  ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"FREE : {lstTasks.Count(x => x == null || x.IsCompleted)}");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"  >>>>> ");

                if (lstTasks.Count(x => x != null && !x.IsCompleted) == lstTasks.Length)
                {
                    Task.Delay(General.SleepTimeMs).Wait();
                    continue;
                }

                if (lstFileInProcess == null || lstFileInProcess.IsEmpty || lstFileInProcess.Count == 0)
                {
                    Task.Delay(General.SleepTimeMs).Wait();
                    continue;
                }
                 
                for (int iTask = 0; iTask < lstTasks.Length; iTask++)
                {
                    if (lstTasks[iTask] != null && !lstTasks[iTask].IsCompleted)
                        continue;

                    string sFile = "";
                    bool bFound = lstFileInProcess.TryDequeue(out sFile);
                    if (!bFound || string.IsNullOrWhiteSpace(sFile))
                        continue;

                    
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.BackgroundColor= ConsoleColor.White;
                    Console.WriteLine($"Start process by {iTask} - {sFile}");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.Black;
                    lstTasks[iTask] = Task.Factory.StartNew(async () =>
                    {
                        CDXFFileProcess objProcess = new CDXFFileProcess();
                        try
                        {
                            await objProcess.ProcessStart(sFile);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")} : Exception " + ex.Message + "\r\nTrace: " + ex.StackTrace  );
                        }
                        objProcess = null; 
                    });
                    Task.Delay(500).Wait(); 
                } 
                  
                //Task.WaitAny(lstTasks);

                //objDXFFileProcessor = null;
                GC.Collect();
            }

            //return true;
        }

    } // class


}// namespace

