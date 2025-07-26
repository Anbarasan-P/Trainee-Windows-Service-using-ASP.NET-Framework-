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
                WriteToFile("Service is started at " + DateTime.Now);
                timer = new Timer();
                timer.Interval = 5000;
                timer.Elapsed += OnElapsedTime;
                timer.Start();
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText(@"C:\TraineeServiceLog.txt", $"{DateTime.Now}: ERROR in OnStart - {ex}\r\n");
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
            BackupManager backupManager = new BackupManager();
            backupManager.BackupTraineeTable();
        }
    }

}
