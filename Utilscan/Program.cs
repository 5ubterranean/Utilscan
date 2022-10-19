using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace utamtest
{
    internal class Program
    {
        
        //taken from SharpPick calls powershell to get the location of the assembly that we want
        static string RunPS()
        {
            string cmd = "[ref].Assembly.GetType(\"System.Management.Automation.Am{0}ils\" -f \"siUt\").Assembly.Location";
            //Init stuff
            Runspace runspace = RunspaceFactory.CreateRunspace();
            PowerShell ps = PowerShell.Create();
            ps.Runspace = runspace;
            ps.AddScript(cmd, true);
            runspace.Open();
            //settings.ErrorActionPreference = ActionPreference.Continue;
            Collection<PSObject> results = ps.Invoke();
            StringBuilder stringBuilder = new StringBuilder();
            foreach (PSObject obj in results)
            {
                stringBuilder.Append(obj);
            }
            return stringBuilder.ToString().Trim();
        }
        public static Tuple<int, int> GetLineNumber (int StringPosition)
        {
            string[] FileArray = FileContent.Split('\n');
            int TotalChars = 0;
            int LineCount = 0;
            int i = 0;
            int LastTotal = 0;
            while (StringPosition > TotalChars)
            {
                LastTotal = TotalChars;
                TotalChars += FileArray[i].Length + 1;
                LineCount++;
                i++;
            }
            int PosInLine = StringPosition - LastTotal;
            return Tuple.Create(PosInLine, LineCount);
        }

        public static bool RequestScan(string Scansi, int EntryLocation)
        {
            int Length = Scansi.Length;
            string ScansiA = Scansi.Substring(0, Length / 2);
            string ScansiB = Scansi.Substring(Length / 2);
            string ResultA = dllScan.Invoke(null, new[] { ScansiA, "Hi" }).ToString();
            bool RecurseA = false;
            if (ResultA == "AMSI_RESULT_DETECTED")
            {
                RecurseA = RequestScan(ScansiA, EntryLocation);
            }
            string ResultB = dllScan.Invoke(null, new[] { ScansiB, "Hi" }).ToString();
            bool RecurseB = false;
            int BLocation = 0;
            if (ResultB == "AMSI_RESULT_DETECTED")
            {
                BLocation = ScansiA.Length + EntryLocation;
                RecurseB = RequestScan(ScansiB, BLocation);
            }
            string ScansiC = "noline";
            string ResultC = "nothing";
            bool RecurseC = false;
            int CLocation = 0;
            if (ResultA != "AMSI_RESULT_DETECTED" & ResultB != "AMSI_RESULT_DETECTED")
            {
                ScansiC = Scansi.Substring(Length / 4, Length / 2);
                ResultC = dllScan.Invoke(null, new[] { ScansiC, "Hi" }).ToString();
                if (ResultC == "AMSI_RESULT_DETECTED")
                {
                    CLocation = (Length / 4) + EntryLocation;
                    RecurseC = RequestScan(ScansiC, CLocation);
                }
            }
            if (ResultA == "AMSI_RESULT_DETECTED" ^ ResultB == "AMSI_RESULT_DETECTED" ^ ResultC == "AMSI_RESULT_DETECTED")
            {
                if (ResultA == "AMSI_RESULT_DETECTED" & !RecurseA)
                {
                    var GetLines = GetLineNumber(EntryLocation);
                    Console.WriteLine("String Flagged");
                    Console.WriteLine($"Found at line: {GetLines.Item2}");
                    //Console.WriteLine($"At Position: {GetLines.Item1}");
                    Console.WriteLine($"Found at: {EntryLocation}");
                    Console.WriteLine(ScansiA);
                    Console.WriteLine("\n");
                }
                else if (ResultB == "AMSI_RESULT_DETECTED" & !RecurseB)
                {
                    var GetLines = GetLineNumber(EntryLocation);
                    Console.WriteLine("String Flagged");
                    Console.WriteLine($"Found at line: {GetLines.Item2}");
                    //Console.WriteLine($"At Position: {GetLines.Item1}");
                    Console.WriteLine($"Found at: {BLocation}");
                    Console.WriteLine(ScansiB);
                    Console.WriteLine("\n");
                }
                else if (ResultC == "AMSI_RESULT_DETECTED" & !RecurseC)
                {
                    var GetLines = GetLineNumber(EntryLocation);
                    Console.WriteLine("String Flagged");
                    Console.WriteLine($"Found at line: {GetLines.Item2}");
                    //Console.WriteLine($"At Position: {GetLines.Item1}");
                    Console.WriteLine($"Found at: {CLocation}");
                    Console.WriteLine(ScansiC);
                    Console.WriteLine("\n");
                }
                return true;
            }
            else if (ResultA == "AMSI_RESULT_DETECTED" | ResultB == "AMSI_RESULT_DETECTED" | ResultC == "AMSI_RESULT_DETECTED")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static void Main(string[] args)
        {
            FileContent = File.ReadAllText(args[0]);
            
            string Original = dllScan.Invoke(null, new[] { FileContent, "Hi"}).ToString();
            bool FinalResult = RequestScan(FileContent, 0);

            if (Original == "AMSI_RESULT_DETECTED" & !FinalResult)
            {
                Console.WriteLine("The file is blocked by AMSI but can't determine the strings being flagged");
            }
            else if (Original != "AMSI_RESULT_DETECTED")
            {
                Console.WriteLine("No threats found!!");
            }
        }
        public static string FileContent = "";
        static string dllLocation = RunPS();
        static Type dllload = Assembly.LoadFrom(dllLocation).GetType("System.Management.Automation.AmsiUtils");
        static MethodInfo dllScan = dllload.GetMethod("ScanContent", BindingFlags.NonPublic | BindingFlags.Static);

    }

}
