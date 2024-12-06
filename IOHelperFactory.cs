using System;
using FileIOHelper.Helpers;
using Microsoft.Win32;

namespace FileIOHelper
{
    public enum IOType
    {
        IniFile,
        Registry,
    }
    public class IOHelperFactory
    {
        public static IIOHelper Create(IOType type, string path, RegistryHive hive = RegistryHive.CurrentUser)
        {
            switch (type)
            {
                case IOType.IniFile:
                    return new IniFileHelper(path);
                case IOType.Registry:
                    return new RegistryHelper(path, hive);
                default:
                    throw new ArgumentException("Not supported IO type", nameof(type));
            }
        }
    }
}