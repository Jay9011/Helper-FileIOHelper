using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using FileIOHelper.Interface;

namespace FileIOHelper.Helpers
{
    public class IniFileHelper : IIniFileHelper, IFileIOHelper
    {
        private readonly string _filePath;
        private readonly Dictionary<string, Dictionary<string, string>> _cache; // 캐시 <섹션, <키, 값>>
        private readonly object _lock = new object();
        
        public IniFileHelper(string filePath)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            _cache = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        }
        
        public void WriteValue(string section, string key, string value)
        {
            if (string.IsNullOrEmpty(section) || string.IsNullOrEmpty(key))
            {
                return;
            }
            
            lock (_lock)
            {
                WritePrivateProfileString(section, key, value, _filePath);
                DeleteCacheKey(section, key);
            }
        }
        
        public string ReadValue(string section, string key, string defaultValue = "")
        {
            if (!FileExists(_filePath))
            {
                throw new FileNotFoundException(_filePath);
            }
            
            if (string.IsNullOrEmpty(section) || string.IsNullOrEmpty(key))
            {
                return defaultValue;
            }

            lock (_lock)
            {
                if (LoadCache(section, key) is string cacheValue)
                {
                    return cacheValue;
                }
                
                StringBuilder buffer = new StringBuilder(255);
                int bytesRead = GetPrivateProfileString(section, key, defaultValue, buffer, 255, _filePath);
                string value = buffer.ToString();
                
                SaveCache(section, key, value);
                
                return value;
            }
        }

        public Dictionary<string, string> ReadSection(string section)
        {
            if (!FileExists(_filePath))
            {
                throw new FileNotFoundException(_filePath);
            }
            
            if (string.IsNullOrEmpty(section))
            {
                return new Dictionary<string, string>();
            }

            lock (_lock)
            {
                if (_cache.TryGetValue(section, out var sectionCache))
                {
                    // 섹션 캐시의 키가 비어있지 않다면 캐시 반환
                    if (sectionCache.Count > 0)
                    {
                        return sectionCache;
                    }
                }

                byte[] buffer = new byte[2048];
                int byteRead = GetPrivateProfileString(section, null, null, buffer, 2048, _filePath);
                
                if (byteRead == 0)
                {
                    throw new KeyNotFoundException($"{section} 섹션을 찾을 수 없습니다.");
                }
                
                string rawString = Encoding.Unicode.GetString(buffer, 0, byteRead * 2);
                string[] keys = rawString.Split('\0');
                
                foreach (var key in keys)
                {
                    if (string.IsNullOrEmpty(key))
                    {
                        continue;
                    }
                    
                    string value = ReadValue(section, key);
                }
                
                return _cache[section];
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
                foreach (var pair in pairs)
                {
                    WriteValue(section, pair.Key, pair.Value);
                }
            }
        }

        public string GetFilePath()
        {
            return _filePath;
        }

        public string ReadAllText(string path)
        {
            return File.ReadAllText(path);
        }

        public void WriteAllText(string path, string contents)
        {
            File.WriteAllText(path, contents);
        }

        public bool FileExists()
        {
            return File.Exists(_filePath);
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }
        
        /// <summary>
        /// 캐시에 저장
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        private void SaveCache(string section, string key, string value)
        {
            if (!_cache.TryGetValue(section, out var sectionCache))
            {
                sectionCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                _cache.Add(section, sectionCache);
            }
            
            sectionCache[key] = value;
        }
        
        /// <summary>
        /// 캐시에서 불러오기
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private string LoadCache(string section, string key)
        {
            if (_cache.TryGetValue(section, out var sectionCache))
            {
                if (sectionCache.TryGetValue(key, out var value))
                {
                    return value;
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// 캐시에서 특정 키 혹은 섹션 삭제
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key">null일 경우 해당 섹션 삭제</param>
        /// <returns></returns>
        private bool DeleteCacheKey(string section, string key = null)
        {
            if (key == null)
            {
                return _cache.Remove(section);
            }
            
            if (_cache.TryGetValue(section, out var sectionCache))
            {
                return sectionCache.Remove(key);
            }
            
            return false;
        }
        
        /// <summary>
        /// 캐시 초기화
        /// </summary>
        /// <returns></returns>
        private bool ClearCache()
        {
            lock (_lock)
            {
                _cache.Clear();
            }

            return true;
        }
        
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int GetPrivateProfileSection(string section, byte[] keyValue, int size, string fileName);
        
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int GetPrivateProfileString(string section, string key, string def, byte[] retVal, int size, string fileName);
        
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string fileName);
        
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool WritePrivateProfileString(string section, string key, string val, string fileName);
    }
}
