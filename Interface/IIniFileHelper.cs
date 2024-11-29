using System.Collections.Generic;

namespace FileIOHelper.Interface
{
    public interface IIniFileHelper
    {
        /// <summary>
        /// ini파일에 값을 쓴다.
        /// </summary>
        /// <param name="section">설정 그룹(섹션)</param>
        /// <param name="key">설정 키</param>
        /// <param name="value">설정 값</param>
        void WriteValue(string section, string key, string value);
        /// <summary>
        /// ini파일에서 값을 읽어온다.
        /// </summary>
        /// <param name="section">설정 그룹(섹션)</param>
        /// <param name="key">설정 키</param>
        /// <returns>설정 값</returns>
        string ReadValue(string section, string key, string defaultValue = "");
        /// <summary>
        /// ini파일에서 섹션을 읽어온다.
        /// </summary>
        /// <param name="section"></param>
        /// <exception cref="FileNotFoundException">파일을 찾지 못한 경우</exception>
        /// <exception cref="KeyNotFoundException">파일에서 섹션을 찾지 못한 경우</exception>
        /// <returns></returns>
        Dictionary<string, string> ReadSection(string section);
        /// <summary>
        /// ini파일에 섹션을 작성한다.
        /// </summary>
        /// <param name="section">설정 그룹(섹션)</param>
        /// <param name="pairs">설정 키-값 쌍 Dictionary</param>
        void WriteSection(string section, Dictionary<string, string> pairs);
        /// <summary>
        /// 파일 위치를 반환한다.
        /// </summary>
        /// <returns></returns>
        string GetFilePath();
    }
}
