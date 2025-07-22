using SharpEDL.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using static SharpEDL.Utils.Cmd;
using static SharpEDL.Utils.Constants;

namespace SharpEDL
{
    public class EDL
    {
        // Enums
        public enum MemoryType
        {
            EMMC,
            UFS
        }

        // Attributes
        public QcomPort Port { get; private set; }
        public PortDetail PortInfo { get; private set; }
        private string firehoseFile = string.Empty;
        private bool isConnected = false;
        private int LUN = 0; // Logical Unit Number, default is 0
        private MemoryType memoryType = MemoryType.EMMC;

        // Contructor
        public EDL(QcomPort port, string loaderFile = "")
        {
            Port = port;
            PortInfo = GetPortInfo(Port.ComPort);

            if (PortInfo == null)
                throw new QCOMException($"Cannot open port!!");

            if (!string.IsNullOrEmpty(loaderFile))
                SendLoader(loaderFile, MemoryType.EMMC);
        }

        // Methods

        // Send programhose loader to device
        public bool SendLoader(string loaderFile, MemoryType type)
        {
            if (isConnected)
                return true;
            if (!File.Exists(loaderFile))
                throw new FileNotFoundException("Loader file not found", loaderFile);

            firehoseFile = loaderFile;
            var request = string.Empty;

            if (type == MemoryType.EMMC)
            {
                if (!EmmcdlRun($"-p {Port.ComPort} -f \"{firehoseFile}\"", out request))
                    return false;

                memoryType = MemoryType.EMMC;
            }
            else
            {
                if (!QSaharaRun($"-p \\\\.\\{Port.ComPort} -s 13:\"{loaderFile}\"", out request))
                    return false;

                memoryType = MemoryType.UFS;
            }

            return isConnected = true;
        }

        public bool SendLoader(string loaderFile)
        {
            return SendLoader(loaderFile, MemoryType.EMMC);
        }

        // Erase partition
        public bool ErasePartition(string partition)
        {
            return EmmcdlCommand($"-e {partition}");
        }

        // Read partition
        public bool DumpPartition(string partition, string outputFile)
        {
            return EmmcdlCommand($"-d {partition} -o \"{outputFile}\"");
        }

        // Write partition
        public bool WritePartition(string partition, string filename)
        {
            return EmmcdlCommand($"-b {partition} \"{filename}\"");
        }

        // Flash firmware
        public bool FlashFirmware(string xmlFile, int LUN = 0)
        {
            if (this.memoryType == MemoryType.EMMC)
            {
                return EmmcdlCommand($"-x \"{xmlFile}\"");
            }
            else
            {
                return FHCommand($"--sendxml=\"{xmlFile}\"", LUN);
            }
        }

        // Remove FRP
        public bool RemoveFRP()
        {
            return ErasePartition("config");
        }

        // Reboot to system
        public bool Reboot()
        {
            return FlashFirmware("reboot.xml");
        }

        // EMMCDL methods
        public bool EmmcdlCommand(string arg)
        {
            if (!isConnected)
                return false;
            var type = memoryType == MemoryType.UFS ? "ufs" : "emmc";
            return EmmcdlRun($"-p {Port.ComPort} -MemoryName {type} " +
                $"-f \"{firehoseFile}\" {arg}", out _);
        }

        // QSahara methods
        public bool QSaharaCommand(string arg)
        {
            if (!isConnected)
                return false;
            return QSaharaRun($"-p \\\\.\\{Port.ComPort} {arg}", out _);
        }

        // FH Loader methods
        public bool FHCommand(string arg, int LUN = -1)
        {
            if (!isConnected)
                return false;

            if (LUN == -1)
            {
                LUN = this.LUN;
            }

            var type = memoryType == MemoryType.UFS ? "ufs" : "eMMc";
            return FHRun($"--port=\\\\.\\{Port.ComPort} {arg} " +
                $"--MemoryName={type} " +
                $"--lun={LUN}" +
                "--noprompt --loglevel=0 --nop", out _);
        }

        // Static methods
        public static List<QcomPort> DetectDevices()
        {
            var ports = new List<QcomPort>();

            if (!EmmcdlRun("-l", out var request))
                return ports;

            var sr = new StringReader(request);
            var line = string.Empty;

            while ((line = sr.ReadLine()) != null)
            {
                if (line.Contains(EMMC_LIST))
                {
                    ports.Add(new QcomPort
                    {
                        Description = line.Substring(0, line.LastIndexOf(' ')),
                        ComPort = line.Substring(line.LastIndexOf(' ') + 1)
                         .Trim('(', ')')
                    });
                }
            }

            return ports;
        }

        public static PortDetail GetPortInfo(string comport)
        {
            if (!EmmcdlRun($"-p {comport} -info", out var result))
                return null;

            var info = new PortDetail();
            var sr = new StringReader(result);
            var line = string.Empty;

            while ((line = sr.ReadLine()) != null)
            {
                if (line.Contains(EMMC_HWID))
                {
                    info.HWID = line.Substring(line.IndexOf(':') + 4).Trim();
                }
                else if (line.Contains(EMMC_HASH))
                {
                    info.HASH = line.Substring(line.IndexOf(':') + 4).Trim();
                }
                else if (line.Contains(EMMC_SBL))
                {
                    info.SBL = line.Substring(line.IndexOf(':') + 4).Trim();
                }
            }

            return info;
        }

        // EMMCDL core method
        private static bool EmmcdlRun(string arg, out string result)
        {
            result = CMDRun(EMMCDL_EXE, arg);
            return
                result.Contains(EMMC_INIT) &&
                result.Contains(EMMC_OKAY);
        }

        // QSahara core method
        private static bool QSaharaRun(string arg, out string result)
        {
            result = CMDRun(QSAHARA_EXE, arg);
            return !result.Contains(QSAHARA_ERROR);
        }

        // FH Loader core method
        private static bool FHRun(string arg, out string result)
        {
            result = CMDRun(FH_EXE, arg);
            Console.WriteLine($"FHRun->\n\n{result}"); // Debug output
            return result.Contains(FH_INIT);
        }
    }
}
