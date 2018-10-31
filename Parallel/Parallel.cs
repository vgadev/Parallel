using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VGA.Tools
{
    struct CommandInfo
    {
        public int ID;
        public string Command;
    }

    class Parallel
    {
        private static Dictionary<int, int> processLookup = new Dictionary<int, int>();
        private static bool show = false;
        private static Object syncObject = new Object();
        static int Main(string[] args)
        {
            // Parse the command line args.
            // Expected usage is either one or more -c "command" and an optional -f "file"
            // The file is expected to contain a list of commands to be executed in parallel.
            List<string> commands = new List<string>();
            List<Task<int>> taskList = new List<Task<int>>();
           
            if (args.Length < 2 || !ProcessArgs(args, ref commands))
            {
                PrintUsage();
                return -1;
            }

            int taskId = 1;
            foreach(string command in commands)
            {
                CommandInfo cmdInfo = new CommandInfo() { ID = taskId, Command = command };
                Func<object, int> func = cmd => RunCommand(cmdInfo);
                var task = Task<int>.Factory.StartNew(func, command);
                taskList.Add(task);
                taskId++;
            }
            
            foreach(var task in taskList)
            {
                task.Wait();
            }

            return 0;
        }

        private static int RunCommand(object info)
        {
            CommandInfo cmdInfo = (CommandInfo)info;
            System.Diagnostics.ProcessStartInfo pinfo = new System.Diagnostics.ProcessStartInfo();
            if (!show)
            {
                pinfo.RedirectStandardOutput = true;
                pinfo.RedirectStandardError = true;
                pinfo.UseShellExecute = false;
            }
            pinfo.FileName = "cmd.exe";
            pinfo.Arguments = "/c " + cmdInfo.Command;
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo = pinfo;
            process.Exited += Process_Exited;
            if (!show)
            {
                process.OutputDataReceived += Process_OutputDataReceived;
                process.ErrorDataReceived += Process_OutputDataReceived;
            }
            process.Start();
            processLookup.Add(process.Id, cmdInfo.ID);
            if (!show)
            {
                process.BeginErrorReadLine();
                process.BeginOutputReadLine();
            }
            process.WaitForExit();
            return process.ExitCode;
        }

        private static void WriteOutput(string data)
        {
            // Sync and write to standard output.
            lock (syncObject)
            {
                Console.WriteLine(data);
            }
        }

        private static void Process_OutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            int taskID = processLookup[(sender as System.Diagnostics.Process).Id];
            WriteOutput(taskID.ToString() + "> " + e.Data);
        }

        private static void Process_Exited(object sender, EventArgs e)
        {
            var p = (sender as System.Diagnostics.Process);
            int taskID = processLookup[p.Id];
            // Log any exit markers.
            WriteOutput(taskID.ToString() + "> ==== Process " + p.Id.ToString() + " has exited with code " + p.ExitCode + " ====");
        }

        private static bool ProcessArgs(string[] args, ref List<string> commands)
        {
            bool status = true;
            for (int ind = 0; ind < args.Length; ind++)
            {
                string arg = args[ind];
                switch(arg)
                {
                    case "-c":
                        ind++;
                        if (ind < args.Length)
                        {
                            commands.Add(args[ind]);
                        }
                        else
                        {
                            status = false;
                        }
                        break;
                    case "-f":
                        ind++;
                        if (ind < args.Length)
                        {
                            using (var file = System.IO.File.OpenRead(args[ind]))
                            {
                                using (System.IO.StreamReader sr = new System.IO.StreamReader(file))
                                {
                                    while (!sr.EndOfStream)
                                    {
                                        commands.Add(sr.ReadLine());
                                    }
                                }
                            }
                        }
                        else
                        {
                            status = false;
                        }
                        break;
                    case "-s":
                    default:
                        show = true;
                        break;
                }
                if (!status)
                {
                    break;
                }
            }
            return status;
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage: parallel.exe -c \"<COMMAND>\" [-f COMMAND_INPUT_FILE] [-s]");
            Console.WriteLine("  -c    Specifies a command as COMMAND, that can be executed from the command line.");
            Console.WriteLine("  -f    Specifies a file (COMMAND_INPUT_FILE) containing a list of commands, each command being on a separate line.");
            Console.WriteLine("  -s    Show each process. Using this switch would open each task with a separate OS shell.");
        }
    }
}
