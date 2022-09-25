using System.IO;

namespace FNZ.Shared.Utils
{
    public class FNELogger
    {
        private string m_LogFilePath;
        private string m_LogFileName;

        public FNELogger(string logFilePath)
        {
            m_LogFilePath = logFilePath;
            m_LogFileName = "log_" + System.DateTime.Now.ToString("MM_dd_yyyy_HH_mm_ss") + ".log";
            Directory.CreateDirectory(logFilePath);

            Log(m_LogFileName + " started at time " + System.DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + "\n");
       
        }

        public void Log(System.Exception e)
        {
            Log("ERROR: " + e);
        }

        public void Log(string msg)
        {
            File.AppendAllText(m_LogFilePath + "\\" + m_LogFileName, System.DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss") + " | " + msg + "\n\n");
        }
    }
}