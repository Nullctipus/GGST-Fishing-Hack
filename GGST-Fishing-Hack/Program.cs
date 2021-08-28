using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Memory;
using System.Diagnostics;

namespace GGST_Fishing_Hack
{
    class Program
    {
        [DllImport("kernel32.dll")]
        static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out int lpNumberOfBytesWritten);

        const string ProcessName = "GGST-Win64-Shipping";

        //Nops money being taken
        static byte[] UnlimitedPulls = { 0x44, 0x89, 0x81, 0x24, 0x35, 0x03, 0x00 }; // mov [rcx+00033524],r8d

        //Skips some jumps checking rare fish
        static byte[] ForceFish = { 0x75, 0x76, 0x80, 0xFA, 0x0A, 0x72, 0x71, }; // jne GGST-Win64-Shipping.exe+C3EA3D
                                                                                 // cmp dl,0A
                                                                                 // jb GGST-Win64-Shipping.exe+C3EA3D

        static void TitleText()
        {
            /*
        ________  ________  ________  _________                     
       |\   ____\|\   ____\|\   ____\|\___   ___\                   
       \ \  \___|\ \  \___|\ \  \___|\|___ \  \_|                   
        \ \  \  __\ \  \  __\ \_____  \   \ \  \                    
         \ \  \|\  \ \  \|\  \|____|\  \   \ \  \                   
          \ \_______\ \_______\____\_\  \   \ \__\                  
           \|_______|\|_______|\_________\   \|__|                  
                              \|_________|                          
                                                                    
                                                                    
 ________ ___  ________  ___  ___  ___  ________   ________         
|\  _____\\  \|\   ____\|\  \|\  \|\  \|\   ___  \|\   ____\        
\ \  \__/\ \  \ \  \___|\ \  \\\  \ \  \ \  \\ \  \ \  \___|        
 \ \   __\\ \  \ \_____  \ \   __  \ \  \ \  \\ \  \ \  \  ___      
  \ \  \_| \ \  \|____|\  \ \  \ \  \ \  \ \  \\ \  \ \  \|\  \     
   \ \__\   \ \__\____\_\  \ \__\ \__\ \__\ \__\\ \__\ \_______\    
    \|__|    \|__|\_________\|__|\|__|\|__|\|__| \|__|\|_______|    
                 \|_________|                                       
                                                                    
                                                                    
             */
            // Created with Find Replace
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("        ________  ________  ________  _________\n       |\\   ____\\|\\   ____\\|\\   ____\\|\\___   ___\\\n       \\ \\  \\___|\\ \\  \\___|\\ \\  \\___|\\|___ \\  \\_|\n        \\ \\  \\  __\\ \\  \\  __\\ \\_____  \\   \\ \\  \\\n         \\ \\  \\|\\  \\ \\  \\|\\  \\|____|\\  \\   \\ \\  \\\n          \\ \\_______\\ \\_______\\____\\_\\  \\   \\ \\__\\\n           \\|_______|\\|_______|\\_________\\   \\|__|\n                              \\|_________|\n\n\n ________ ___  ________  ___  ___  ___  ________   ________\n|\\  _____\\\\  \\|\\   ____\\|\\  \\|\\  \\|\\  \\|\\   ___  \\|\\   ____\\\n\\ \\  \\__/\\ \\  \\ \\  \\___|\\ \\  \\\\\\  \\ \\  \\ \\  \\\\ \\  \\ \\  \\___|\n \\ \\   __\\\\ \\  \\ \\_____  \\ \\   __  \\ \\  \\ \\  \\\\ \\  \\ \\  \\  ___\n  \\ \\  \\_| \\ \\  \\|____|\\  \\ \\  \\ \\  \\ \\  \\ \\  \\\\ \\  \\ \\  \\|\\  \\\n   \\ \\__\\   \\ \\__\\____\\_\\  \\ \\__\\ \\__\\ \\__\\ \\__\\\\ \\__\\ \\_______\\\n    \\|__|    \\|__|\\_________\\|__|\\|__|\\|__|\\|__| \\|__|\\|_______|\n                 \\|_________|\n");
            Console.ResetColor();
        }
        static string BytesToString(byte[] bytes)
        {
            string r = "";
            for(int i =0; i < bytes.Length-1; i++)
            {
                r += bytes[i].ToString("X2") + " ";
            }
            return r + bytes[bytes.Length - 1].ToString("X2");
        }
        static async Task Main(string[] args)
        {
            Stopwatch s = new Stopwatch();
            s.Start();
            TitleText();
            Console.WriteLine("Made For 1.09\n");
            Console.WriteLine("This is not persistant between launches\n");
            Console.WriteLine("Waiting for Game");

            int PID = 0;
            Mem mem = new Mem();

            while (PID <= 0)
            {
                PID = mem.GetProcIdFromName(ProcessName);
                Thread.Sleep(100);
            }
            Console.WriteLine("Process ID is " + PID);
            mem.OpenProcess(PID);
            IEnumerable<long> UP = await mem.AoBScan(BytesToString(UnlimitedPulls), true, true);
            if (UP.Count() > 0)
            {
                Console.WriteLine($"\nAOB Scan Found {UP.First()}");
                mem.WriteBytes((UIntPtr)UP.First(), new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 });
                Console.WriteLine("UnlimitedPulls Patched in");
            }
            else
                Console.WriteLine("\nUnlimitedPulls AOB Not Found");

            IEnumerable<long> FF = await mem.AoBScan(BytesToString(ForceFish), true, true);
            if (FF.Count() > 0)
            {
                Console.WriteLine($"AOB Scan Found {FF.First()}");
                mem.WriteBytes((UIntPtr)FF.First(), new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 });
                Console.WriteLine("ForceFish Patched in");
            }
            else
                Console.WriteLine("ForceFish AOB Not Found");
            s.Stop();
            Console.WriteLine($"\nFinished Patching in {s.Elapsed.TotalSeconds}\n\nPress any key to exit");
            Console.ReadKey();
        }
    }
}
