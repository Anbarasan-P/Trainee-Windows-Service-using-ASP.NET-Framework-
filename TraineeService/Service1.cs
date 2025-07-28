using System;
using System.ServiceProcess;
using System.Timers;


namespace TraineeService
{
    public partial class Service1 : ServiceBase
    {
        Timer timer;
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                WriteToFile("Service started: " + DateTime.Now);
                timer = new Timer();
                timer.Interval = 60000; // 10 minutes
                timer.Elapsed += OnElapsedTime;
                timer.AutoReset = true;
                timer.Enabled = true;
                WriteToFile("Timer started.");
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText(@"D:\TraineeServiceErrorLog.txt", $"{DateTime.Now}: Error in OnStart - {ex.Message}\r\n");
                throw;
            }
        }


        protected override void OnStop()
        {
            WriteToFile("Service is stopped at "+ DateTime.Now);
            timer.Stop();
        }

        public void WriteToFile(string message)
        {
            string filePath = @"D:\TraineeServiceLogs.txt";
            System.IO.File.AppendAllText(filePath, $"{DateTime.Now}: {message}\r\n");
        }

        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            try
            {
                BackupManager backupManager = new BackupManager();
                backupManager.BackupTraineeTable();
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText(@"D:\TraineeServiceBackupErrorLog.txt", $"[{DateTime.Now}] Error in OnElapsedTime: {ex}\r\n");
            }
        }

    }

}
