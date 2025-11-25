## 프로그램 실행 안내 / Program Execution Guide



===== [ 한국어 / Korean ] =====

※ FSB_BANK_Extractor_CS_GUI.exe가 정상적으로 실행되지 않는다면, 먼저 .NET Framework 4.8이 설치되어 있는지 확인해 주십시오.

본 프로그램은 배포된 ZIP 파일 내에 실행에 필요한 모든 파일(FMOD 라이브러리 등)이 포함되어 있습니다.
압축을 풀고 파일들을 분리하지 않은 상태에서 실행해야 합니다.

■ 필수 구성 파일 (ZIP 파일 포함 내역):
- FSB_BANK_Extractor_CS_GUI.exe (실행 파일)
- fmod.dll (필수 라이브러리)
- fmodstudio.dll (필수 라이브러리)

※ FMOD 버전 정보: 2.03.06 - Studio API minor release (build 149358)


● 사용 방법

1. 압축 풀기:
   - 다운로드한 ZIP 파일의 압축을 원하시는 폴더에 풉니다.

2. 파일 확인:
   - 압축이 풀린 폴더 안에 실행 파일(.exe)과 DLL 파일들(`fmod.dll`, `fmodstudio.dll`)이 모두 함께 있는지 확인합니다.

   [올바른 폴더 구조 예시]
   [폴더]
   +-- FSB_BANK_Extractor_CS_GUI.exe
   +-- fmod.dll
   +-- fmodstudio.dll

3. 프로그램 실행:
   - `FSB_BANK_Extractor_CS_GUI.exe`를 실행합니다.


● 오류 발생 시 (실행이 안 될 경우)

1. 파일 위치 확인:
   - 실행 파일(.exe)만 바탕화면 등으로 따로 꺼내면 실행되지 않습니다.
   - 반드시 `fmod.dll` 및 `fmodstudio.dll` 파일이 실행 파일과 **같은 폴더**에 있어야 합니다.

2. .NET Framework 확인:
   - 윈도우 기능 켜기/끄기 또는 마이크로소프트 홈페이지에서 .NET Framework 4.8이 설치되어 있는지 확인해 주세요.

3. 윈도우 보안 차단 해제:
   - 간혹 윈도우 보안 설정으로 인해 DLL 파일 로드가 차단될 수 있습니다.
   - DLL 파일 우클릭 -> 속성 -> 하단 '차단 해제' 체크 후 적용해 보세요.


● 주의사항:

- 포함된 DLL 파일의 이름이나 확장자를 임의로 변경하지 마십시오.
- DLL 파일들을 삭제하면 프로그램이 작동하지 않습니다.


● 이미지 출처

- 아이콘 이름: Unboxing icons
- 제작자: Graphix's Art
- 제공처: Flaticon
- URL: https://www.flaticon.com/free-icons/unboxing



===== [ English ] =====

※ If FSB_BANK_Extractor_CS_GUI.exe does not launch correctly, please check if .NET Framework 4.8 is installed first.

This program comes as a ZIP package containing all necessary files (including FMOD libraries) required for execution.
Please extract the archive and run the program without separating the files.

■ Included Files (Do Not Delete):
- FSB_BANK_Extractor_CS_GUI.exe (Executable)
- fmod.dll (Required Library)
- fmodstudio.dll (Required Library)

※ FMOD Version: 2.03.06 - Studio API minor release (build 149358)


● How to Use

1. Extract the ZIP:
   - Extract the downloaded ZIP file to a folder of your choice.

2. Verify Files:
   - Ensure that the executable (.exe) and the DLL files (`fmod.dll`, `fmodstudio.dll`) are located in the SAME folder.

   [Correct Folder Structure]
   [Folder]
   +-- FSB_BANK_Extractor_CS_GUI.exe
   +-- fmod.dll
   +-- fmodstudio.dll

3. Run the Program:
   - Launch `FSB_BANK_Extractor_CS_GUI.exe`.


● Troubleshooting

1. Check File Location:
   - Do NOT move the .exe file alone to the Desktop or another location.
   - It must remain in the same folder as `fmod.dll` and `fmodstudio.dll`.

2. Check .NET Framework:
   - Ensure .NET Framework 4.8 is installed on your system.

3. Unblock Files:
   - Sometimes Windows Security blocks downloaded DLL files.
   - Right-click the DLL files -> Properties -> Check 'Unblock' at the bottom -> Apply.


● Credits

- Icon Name: Unboxing icons
- Author: Graphix's Art
- Source: Flaticon
- URL: https://www.flaticon.com/free-icons/unboxing