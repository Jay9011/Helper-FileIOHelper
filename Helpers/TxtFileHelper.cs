using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FileIOHelper.Helpers
{
    internal class TxtFileHelper : IIOHelper
    {
        private readonly string _filePath;
        private readonly object _lock = new object();

        public TxtFileHelper(string filePath)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        }

        public string ReadValue(string section, string key, string defaultValue = "")
        {
            return "";
        }

        public void WriteValue(string section, string key, string value)
        {
            var dictionary = Path.GetDirectoryName(_filePath);

            lock (_lock)
            {
                try
                {
                    if (!string.IsNullOrEmpty(dictionary) && !Directory.Exists(dictionary))
                    {
                        Directory.CreateDirectory(dictionary);
                    }

                    using (StreamWriter sw = File.AppendText(_filePath))
                    {
                        sw.WriteLine(value);
                    }
                }
                catch (Exception e)
                {
                    throw new IOException("Failed to write txt file.", e);
                }
            }
        }

        public Dictionary<string, string> ReadSection(string section)
        {
            return new Dictionary<string, string>();
        }

        public void WriteSection(string section, Dictionary<string, string> pairs)
        {

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
    }
}
