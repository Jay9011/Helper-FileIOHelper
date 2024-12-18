using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace FileIOHelper.Helpers
{
    public class IniFileHelper : IIOHelper
    {
        private readonly string _filePath;
        private readonly Dictionary<string, Dictionary<string, string>> _cache; // 캐시 <섹션, <키, 값>>
        private readonly object _lock = new object();
        
        public IniFileHelper(string filePath)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            _cache = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        }
        
        public string ReadValue(string section, string key, string defaultValue = "")
        {
            if (!IsExists(_filePath))
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

        public void WriteValue(string section, string key, string value)
        {
            if (string.IsNullOrEmpty(section) || string.IsNullOrEmpty(key))
            {
                return;
            }

            var dictionary = Path.GetDirectoryName(_filePath);
            
            lock (_lock)
            {
                try
                {
                    if (!string.IsNullOrEmpty(dictionary) && !Directory.Exists(dictionary))
                    {
                        Directory.CreateDirectory(dictionary);
                    }
                
                    WritePrivateProfileString(section, key, value, _filePath);
                }
                catch (Exception e)
                {
                    throw new IOException("Failed to write ini file.", e);
                }
                
                DeleteCacheKey(section, key);
            }
        }
        
        public Dictionary<string, string> ReadSection(string section)
        {
            if (!IsExists(_filePath))
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

        public bool IsExists(string path = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = _filePath;
            }
            
            return File.Exists(path);
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
                path = _filePath;
            }
            string directory = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(directory))
            {
                throw new ArgumentException("path is invalid.");
            }
            
            // 디렉토리 권한 체크
            if (access.HasFlag(FileAccess.Write))
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                // 디렉토리에 쓰기 권한이 있는지 확인
                string testFile = Path.Combine(directory, Path.GetRandomFileName());
                File.WriteAllText(testFile, string.Empty);
                File.Delete(testFile);
            }
            
            // 파일 권한 테스트
            if (access.HasFlag(FileAccess.Write) || access.HasFlag(FileAccess.Read))
            {
                if (File.Exists(path))
                {
                    if (access.HasFlag(FileAccess.Read))
                    {
                        using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                        {
                            // 파일에 읽기 권한이 있는지 확인
                        }
                    }

                    if (access.HasFlag(FileAccess.Write))
                    {
                        using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Write))
                        {
                            // 파일에 쓰기 권한이 있는지 확인
                        }
                    }
                }
                else
                {
                    if (access.HasFlag(FileAccess.Write))
                    {
                        File.WriteAllText(path, string.Empty);
                        File.Delete(path);
                    }
                }
            }

            return true;
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
