using System.Collections.Generic;
using System.IO;

namespace FileIOHelper
{
    public interface IIOHelper
    {
        /// <summary>
        /// 섹션 - 키 - 값 형태의 설정 파일을 읽어온다.
        /// </summary>
        /// <param name="section">큰 구분</param>
        /// <param name="key">구분자</param>
        /// <param name="defaultValue">기본 값</param>
        /// <returns>string 형태의 설정</returns>
        string ReadValue(string section, string key, string defaultValue = "");
        /// <summary>
        /// 섹션 - 키 - 값 형태의 설정 파일을 쓴다.
        /// </summary>
        /// <param name="section">큰 구분</param>
        /// <param name="key">구분자</param>
        /// <param name="value">값</param>
        void WriteValue(string section, string key, string value);
        /// <summary>
        /// 선택한 섹션의 모든 설정을 읽어온다.
        /// </summary>
        /// <param name="section">큰 구분</param>
        /// <returns>
        /// <see cref="Dictionary{Key,Value}"/>
        /// </returns>
        Dictionary<string, string> ReadSection(string section);
        /// <summary>
        /// 선택한 섹션에 설정을 쓴다.
        /// </summary>
        /// <param name="section">큰 구분</param>
        /// <param name="pairs"><see cref="Dictionary{Key,Value}"/></param>
        void WriteSection(string section, Dictionary<string, string> pairs);
        /// <summary>
        /// 해당 위치가 존재하는지 확인한다.
        /// </summary>
        /// <param name="path">입출력 위치</param>
        /// <returns><see cref="bool"/></returns>
        bool IsExists(string path = null);
        /// <summary>
        /// 해당 위치에 대한 권한이 있는지 확인한다.
        /// </summary>
        /// <param name="path">입출력 위치</param>
        /// <param name="access">조회 권한</param>
        /// <returns><see cref="bool"/></returns>
        /// <exception cref="ArgumentException">입력 파라미터 에러</exception>
        /// <exception cref="UnauthorizedAccessException">권한 없음</exception>
        bool CheckPermission(string path, FileAccess access);
    }
}