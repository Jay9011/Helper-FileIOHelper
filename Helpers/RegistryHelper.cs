using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;

namespace FileIOHelper.Helpers
{
    public class RegistryHelper : IIOHelper
    {
        private readonly string _registryPath;
        private readonly RegistryKey _baseKey;
        private readonly object _lock = new object();

        public RegistryHelper(string registryPath, RegistryHive hive = RegistryHive.CurrentUser)
        {
            _registryPath = registryPath ?? throw new System.ArgumentNullException(nameof(registryPath));
            
            switch (hive)
            {
                case RegistryHive.ClassesRoot:
                    _baseKey = Registry.ClassesRoot;
                    break;
                case RegistryHive.CurrentUser:
                    _baseKey = Registry.CurrentUser;
                    break;
                case RegistryHive.LocalMachine:
                    _baseKey = Registry.LocalMachine;
                    break;
                case RegistryHive.Users:
                    _baseKey = Registry.Users;
                    break;
                case RegistryHive.PerformanceData:
                    _baseKey = Registry.PerformanceData;
                    break;
                case RegistryHive.CurrentConfig:
                    _baseKey = Registry.CurrentConfig;
                    break;
                default:
                    _baseKey = Registry.CurrentUser;
                    break;
            }
        }


        public string ReadValue(string section, string key, string defaultValue = "")
        {
            if (string.IsNullOrEmpty(section) || string.IsNullOrEmpty(key))
            {
                return defaultValue;
            }

            lock (_lock)
            {
                using (var regKey = _baseKey.OpenSubKey($"{_registryPath}\\{section}"))
                {
                    if (regKey == null)
                    {
                        return defaultValue;
                    }
                    
                    return regKey.GetValue(key, defaultValue)?.ToString();
                }
            }
        }

        public void WriteValue(string section, string key, string value)
        {
            if (string.IsNullOrEmpty(section) || string.IsNullOrEmpty(key))
            {
                return;
            }
            
            lock (_lock)
            {
                using (var regKey = _baseKey.CreateSubKey($"{_registryPath}\\{section}"))
                {
                    regKey?.SetValue(key, value);
                }
            }
        }

        public Dictionary<string, string> ReadSection(string section)
        {
            if (string.IsNullOrEmpty(section))
            {
                return new Dictionary<string, string>();
            }

            lock (_lock)
            {
                using (var regKey = _baseKey.OpenSubKey($"{_registryPath}\\{section}"))
                {
                    if (regKey == null)
                    {
                        throw new KeyNotFoundException($"{section} is not found.");
                    }

                    var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var valueName in regKey.GetValueNames())
                    {
                        var value = regKey.GetValue(valueName)?.ToString() ?? string.Empty;
                        values.Add(valueName, value);
                    }
                    
                    return values;
                }
            }
        }

        public void WriteSection(string section, Dictionary<string, string> pairs)
        {
            if (string.IsNullOrEmpty(section) || pairs == null || pairs.Count == 0)
            {
                return;
            }
            
            lock (_lock)
            {
                using (var regKey = _baseKey.CreateSubKey($"{_registryPath}\\{section}"))
                {
                    foreach (var pair in pairs)
                    {
                        regKey?.SetValue(pair.Key, pair.Value);
                    }
                }
            }
        }

        public bool IsExists(string path = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = _registryPath;
            }

            try
            {
                using (var regKey = _baseKey.OpenSubKey(path))
                {
                    return regKey != null;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        /// <summary>
        /// 해당 위치에 대한 권한이 있는지 확인한다.
        /// </summary>
        /// <param name="path">입출력 위치</param>
        /// <param name="access">조회 권한</param>
        /// <returns><see cref="bool"/></returns>
        /// <exception cref="ArgumentException">입력 파라미터 에러</exception>
        /// <exception cref="UnauthorizedAccessException">권한 없음</exception>
        public bool CheckPermission(string path, FileAccess access)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = _registryPath;
            }

            string parentPath = Path.GetDirectoryName(path)?.Replace('\\', '/');
            if (string.IsNullOrEmpty(parentPath))
            {
                throw new ArgumentException("path is invalid.");
            }

            if (access.HasFlag(FileAccess.Write))
            {
                using (var regKey = _baseKey.CreateSubKey(path))
                {
                    if (regKey == null)
                    {
                        throw new UnauthorizedAccessException("No write permission or path does not exist.");
                    }
                    
                    string testValueName = $"_test_{Guid.NewGuid()}";
                    regKey.SetValue(testValueName, string.Empty);
                    regKey.DeleteValue(testValueName, false);
                }
            }
            
            if (access.HasFlag(FileAccess.Read))
            {
                // path가 존재하지 않는 경우 경로 생성
                using (var regKey = _baseKey.OpenSubKey(path))
                {
                    if (regKey == null
                        && !access.HasFlag(FileAccess.Write))
                    {
                        throw new UnauthorizedAccessException("No read permission or path does not exist.");
                    }
                }
            }

            return true;
        }
    }
}