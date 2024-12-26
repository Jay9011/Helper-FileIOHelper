# FileIOHelper 라이브러리 사용 설명서

## 개요

FileIOHelper는 서로 다른 저장소 메커니즘에 대해 일관된 인터페이스를 제공하는 .NET 라이브러리입니다. 현재 다음의 구현체를 지원합니다.

- INI 파일 저장소
- Windows 레지스트리 저장소
- TXT 파일
- LOG 파일

## 설치

라이브러리를 사용하기 위해 다음 네임스페이스를 프로젝트에 포함시킵니다.

```csharp
using FileIOHelper;
using FileIOHelper.Helpers;
```

## 핵심 인터페이스

라이브러리는 설정 데이터 접근을 위한 표준 메서드 세트를 제공하는 `IIOHelper` 인터페이스를 구현합니다.

### 주요 메서드

- `ReadValue`: 섹션/키 쌍에서 단일 값을 읽습니다.
- `WriteValue`: 섹션/키 쌍에 값을 씁니다.
- `ReadSection`: 섹션의 모든 키-값 쌍을 읽습니다.
- `WriteSection`: 섹션에 여러 키-값 쌍을 씁니다.
- `IsExists`: 저장소 위치가 존재하는지 확인합니다.
- `CheckPermission`: 저장소 위치에 대한 접근 권한을 확인합니다.

## INI 파일 구현체

INI 파일 구현체는 `IniFileHelper` 클래스를 통해 제공됩니다. 이 클래스는 `IIOHelper` 인터페이스를 구현합니다.

### 사용 예

```csharp
// 초기화
IIOHelper iniFile = new IniFileHelper("C:\\Temp\\settings.ini"); // 경로/파일.확장자 형식

// 값 쓰기
iniFile.WriteValue("General", "Language", "ko-KR");
iniFile.WriteValue("AppSettings", "Theme", "Dark");

// 값 읽기
string language = iniFile.ReadValue("General", "Language");

// 섹션 읽기
Dictionary<string, string> appSettings = iniFile.ReadSection("AppSettings");
```

## 레지스트리 구현체

레지스트리 구현체는 `RegistryHelper` 클래스를 통해 제공됩니다.

이 클래스는 `IIOHelper` 인터페이스를 구현합니다. `RegistryHelper` 클래스는 Windows 레지스트리 접근 기능을 제공합니다.

### 사용 예

#### 초기화

```csharp
// HKEY_CURRENT_USER가 기본값
var registryHelper = new RegistryHelper(@"Software\MyApp");

// 다른 하이브 지정
var registryHelper = new RegistryHelper(@"Software\MyApp", RegistryHive.LocalMachine);
```
> 지원하는 레지스트리 하이브
> 
> - ClassesRoot (HKCR)
> - CurrentUser (HKCU)
> - LocalMachine (HKLM)
> - Users (HKU)
> - PerformanceData
> - CurrentConfig

#### 값 읽기/쓰기

```csharp
// 값 쓰기
registryHelper.WriteValue("Settings", "LastRun", DateTime.Now.ToString());

// 값 읽기
string lastRun = registryHelper.ReadValue("Settings", "LastRun");

// 여러 값 쓰기
var settings = new Dictionary<string, string>
{
    { "Theme", "Dark" },
    { "Language", "ko-KR" }
};
registryHelper.WriteSection("AppSettings", settings);
```

## TXT 파일 구현체

TXT 파일 구현체는 `TxtFileHelper` 클래스를 통해 제공됩니다.

이 클래스는 `IIOHelper` 인터페이스를 구현합니다. `TxtFileHelper` 클래스는 TXT 파일 쓰기 기능을 제공합니다.

### 사용 예

```csharp
// 초기화
IIOHelper txtFile = new TxtFileHelper("C:\\Temp\\Log_20001231.txt"); // 경로/파일.확장자 형식

// 값 쓰기
txtFile.WriteValue("", "", "내용을 입력합니다.");
txtFile.WriteValue("", "", "예제 입니다.");
```

## LOG 파일 구현체

LOG 파일 구현체는 `LogFileHelper` 클래스를 통해 제공됩니다.

이 클래스는 `IIOHelper` 인터페이스를 구현합니다. `LogFileHelper` 클래스는 LOG 파일 쓰기 기능을 제공합니다.

### 사용 예

```csharp
// 초기화
IIOHelper logFile = new LogFileHelper("C:\\Temp\\Log_19990101.log"); // 경로/파일.확장자 형식

// 값 쓰기
logFile.WriteValue("", "", "내용을 입력합니다.");
logFile.WriteValue("", "", "예제 입니다.");
```

## 오류 처리

구현체 모두 일반적인 시나리오에 대한 포괄적인 오류 처리를 포함합니다.

> - FileNotFoundException: 존재하지 않는 INI 파일을 읽으려 할 때
> - ArgumentNullException: null 경로로 초기화할 때
> - KeyNotFoundException: 존재하지 않는 섹션을 읽으려 할 때
> - UnauthorizedAccessException: 필요한 권한이 없을 때
> - IOException: 파일 작업이 실패할 때

## 제한 사항

- INI 파일 구현체는 Windows API 호출을 사용하므로 크로스 플랫폼 애플리케이션에 적합하지 않을 수 있음
- 레지스트리 구현체는 Windows 전용
- 경로의 경우 INI 파일, TXT 파일, LOG 파일은 절대 경로 사용
- INI 파일 섹션의 최대 버퍼 크기는 2048바이트
- TXT 파일과 LOG 파일은 읽기 기능을 지원하지 않음

## 성능 고려사항

- 구현체 모두 스레드 안전성을 위해 잠금 메커니즘 사용
- 여러 값을 읽을 때는 다수의 ReadValue 호출 대신 ReadSection 사용 고려
- 대용량 데이터셋의 경우 레지스트리 작업이 파일 작업보다 느릴 수 있음