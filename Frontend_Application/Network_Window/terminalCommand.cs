using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network_Window
{
    public class TerminalCommand
    {
        private String command;
        private String output;
        private String error;
        public void commandShell() {
            
            Process process = new Process();

            ProcessStartInfo processStart = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{command}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            process.StartInfo = processStart;


            process.Start();

            process.WaitForExit();

            setOutput(process.StandardOutput.ReadToEnd());
            setError(process.StandardError.ReadToEnd());

            process.WaitForExit();

        }
        public void setCommand(String command)
        {
            this.command = command;
        }
        public String getCommand()
        {
            return command;
        }
        
        public void setOutput(String output)
        {
            this.output = output;
        }
        public String getOutput()
        {
            return output;
        }
        public void setError(String error)
        {
            this.error = error;
        }
        public String getError()
        {
            return error;
        }
    }
}
