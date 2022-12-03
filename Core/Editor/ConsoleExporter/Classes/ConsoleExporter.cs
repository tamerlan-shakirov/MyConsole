/* ================================================================
   ---------------------------------------------------
   Project   :    MyConsole
   Publisher :    Renowned Games
   Developer :    Tamerlan Shakirov
   ---------------------------------------------------
   Copyright 2022 Renowned Games All rights reserved.
   ================================================================ */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace RenownedGames.MyConsoleEditor
{
    [Serializable]
    public sealed class ConsoleExporter
    {
        [Flags]
        public enum InfoFilter
        {
            None = 0,
            DeviceName = 1 << 0,
            DeviceType = 1 << 1,
            OS = 1 << 2,
            Memory = 1 << 3,
            Display = 1 << 4,
            CPU = 1 << 5,
            GPU = 1 << 6,
            Unity = 1 << 7,
            Packages = 1 << 8,
            Everything = ~0
        }

        [SerializeField]
        private InfoFilter infoFilter = InfoFilter.Everything;

        [SerializeField]
        private string separator = "---------------------";

        public ConsoleExporter(InfoFilter systemInfoFilter)
        {
            this.infoFilter = systemInfoFilter;
        }

        public ConsoleExporter(InfoFilter infoFilter, string separator) : this(infoFilter)
        {
            this.separator = separator;
        }

        public void ExportLogs(string path, List<Log> logs)
        {
            using (StreamWriter writer = new StreamWriter(path, false))
            {
                writer.WriteLine("MyConsole exporter by Renowned Games.");
                writer.WriteLine("\n\n");

                if ((infoFilter & InfoFilter.None) == 0)
                {
                    writer.WriteLine("SYSTEM INFORMATION");
                    foreach (Enum value in Enum.GetValues(infoFilter.GetType()))
                    {
                        if (infoFilter.HasFlag(value))
                        {
                            writer.WriteLine(GetSystemValue((InfoFilter)value));
                        }
                    }
                }

                writer.WriteLine("\n\n");

                writer.WriteLine("LOG LIST");
                writer.WriteLine();
                for (int i = 0; i < logs.Count; i++)
                {
                    Log log = logs[i];
                    writer.WriteLine($"Time: {log.GetTime()} | Log type: {log.GetLogType()}");
                    writer.WriteLine($"Condition: {log.GetCondition()}");
                    writer.WriteLine($"Stack trace: {log.GetStackTrace()}");

                    if (i < logs.Count - 1)
                    {
                        writer.WriteLine(separator);
                        writer.WriteLine();
                    }
                }
            }
        }

        private string GetSystemValue(InfoFilter systemInfo)
        {
            switch (systemInfo)
            {
                default:
                case InfoFilter.None:
                case InfoFilter.Everything:
                    return string.Empty;
                case InfoFilter.DeviceName:
                    return $"Name: {SystemInfo.deviceName}";
                case InfoFilter.DeviceType:
                    return $"Type: {SystemInfo.deviceType}";
                case InfoFilter.OS:
                    return $"OS: {SystemInfo.operatingSystemFamily}, version {SystemInfo.operatingSystem}";
                case InfoFilter.Memory:
                    return $"RAM: {SystemInfo.systemMemorySize} GB";
                case InfoFilter.Display:
                    return $"Display: {Screen.currentResolution.width}x{Screen.currentResolution.height} {Screen.currentResolution.refreshRate}Hz";
                case InfoFilter.CPU:
                    return $"Processor: {SystemInfo.processorType} {SystemInfo.processorFrequency} Mhz ({SystemInfo.processorCount} Threads)";
                case InfoFilter.GPU:
                    return $"Graphics Card: {SystemInfo.graphicsDeviceVendor} {SystemInfo.graphicsDeviceName} {SystemInfo.graphicsMemorySize} (API {SystemInfo.graphicsDeviceType}, Shader Level {SystemInfo.graphicsShaderLevel})";
                case InfoFilter.Unity:
                    return $"Unity {Application.unityVersion} ({Application.platform})";
                case InfoFilter.Packages:
                    string packages = "Packages: ";
                    List<PackageInfo> packageInfos = AssetDatabase.FindAssets("package")
                                     .Select(AssetDatabase.GUIDToAssetPath)
                                     .Where(x => AssetDatabase.LoadAssetAtPath<TextAsset>(x) != null)
                                     .Select(PackageInfo.FindForAssetPath)
                                     .ToList();
                    if(packageInfos.Count == 0)
                    {
                        packages += "Empty";
                    }
                    else
                    {
                        packages += $"          {packageInfos[0].displayName} (v{packageInfos[0].version})";
                        for (int i = 1; i < packageInfos.Count; i++)
                        {
                            packages += $"\n          {packageInfos[i].displayName} (v{packageInfos[i].version})";
                        }
                    }
                    return packages;
            }
        }
    }
}