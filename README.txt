## 프로그램 실행 안내 / Program Execution Guide



===== [ 한국어 / Korean ] =====

※ FSB_BANK_Extractor_CS_GUI.exe가 정상적으로 실행되지 않는다면, 먼저 닷 넷 4.8이 설치되어 있는지 확인해 주십시오. 닷 넷 4.8이 설치되어 있음에도 문제가 지속된다면, 아래의 FMOD DLL 파일 안내를 참고하여 주십시오.

본 프로그램은 FMOD 사운드 라이브러리를 사용합니다.
프로그램이 정상적으로 실행되려면 fmod.dll 파일이 필요하며, 이 파일은 프로그램 실행 파일과 같은 위치에 있어야 합니다.

필수 DLL 파일:
- fmod.dll (버전: 2.03.06 - Studio API minor release (build 149358))

추가 DLL 파일 (오류 발생 시):
- fmodL.dll (버전: 2.03.06 - Studio API minor release (build 149358))
* `fmodL.dll` 파일은 `fmod.dll`로 실행되지 않을 때 추가적으로 필요할 수 있습니다. 두 파일을 함께 사용하는 경우 반드시 동일한 버전이어야 합니다.


● 사용 방법

1. DLL 파일 준비:
   - 프로그램과 함께 제공된 `fmod.dll` 파일을 확인하거나, 직접 FMOD Studio Windows API를 설치하여 dll 파일을 가져옵니다.
   - `fmodL.dll` 파일은 `fmod.dll` 실행 시 오류가 발생할 때만 준비해주세요.

2. DLL 파일 복사:
   - 준비한 `fmod.dll` 파일을 프로그램 실행 파일(.exe)과 같은 폴더에 복사합니다.

   기본적인 파일 위치 예시:

   [프로그램 폴더]
   +-- 프로그램.exe
   +-- fmod.dll

   - `fmod.dll`만으로 오류가 발생한다면, `fmodL.dll` 파일도 함께 복사해주세요.

   오류 발생 시 파일 위치 예시:

   [프로그램 폴더]
   +-- 프로그램.exe
   +-- fmod.dll
   +-- fmodL.dll

3. 프로그램 실행:
   - DLL 파일(`fmod.dll` 또는 `fmod.dll`과 `fmodL.dll`)이 프로그램 실행 파일과 같은 폴더에 있는지 확인하고 프로그램을 실행합니다.


● 오류 발생 시 (DLL 파일 관련 문제 해결 방법)

`fmod.dll` 파일을 프로그램과 같은 위치에 두었는데도 오류가 발생한다면, 다음 방법들을 시도해 보세요.

1. `fmodL.dll` 파일 추가:
   - `fmodL.dll` 파일을 다운로드하여 `fmod.dll`과 마찬가지로 프로그램 실행 파일과 같은 위치에 복사합니다.
   - `fmodL.dll` 파일은 반드시 `fmod.dll` 파일과 동일한 버전(2.03.06)이어야 합니다.
   - `fmodL.dll` 파일을 추가한 후 프로그램을 다시 실행해 봅니다.

2. DLL 파일 버전 확인:
   - `fmod.dll` (및 `fmodL.dll`을 추가했다면 `fmodL.dll`도) 파일이 모두 버전 2.03.06인지 확인합니다. 버전이 다르면 프로그램이 제대로 작동하지 않을 수 있습니다.

3. DLL 파일 누락 확인:
   - `fmod.dll` (및 `fmodL.dll`을 추가했다면 `fmodL.dll`도) 파일이 프로그램 실행 파일과 같은 폴더에 있는지 다시 한번 확인합니다.


● 주의사항:
- `fmod.dll` 파일 (그리고 `fmodL.dll` 파일을 추가했다면 `fmodL.dll` 파일 또한)은 프로그램 실행에 필수적인 파일입니다. 삭제하거나 다른 곳으로 옮기지 않도록 주의하세요.
- DLL 파일 버전이 명시된 버전(2.03.06)과 일치하는지 확인해주세요. 버전이 맞지 않으면 호환성 문제가 발생할 수 있습니다.
- `fmod.dll`과 `fmodL.dll`을 함께 사용하는 경우, 두 파일은 반드시 동일한 버전이어야 합니다. 버전이 다르면 오류가 발생할 수 있습니다.


● 이미지 출처

- 아이콘 이름: Unboxing icons
- 제작자: Graphix's Art
- 제공처: Flaticon
- URL: https://www.flaticon.com/free-icons/unboxing





===== [ English / 영어 ] =====

※ If Program1.exe does not run correctly, please first check if .NET Framework 4.8 is installed. If the issue persists even after .NET Framework 4.8 is installed, please refer to the FMOD DLL File Guide below.

This program uses the FMOD sound library.
For the program to run correctly, the fmod.dll file is required and must be located in the same directory as the program executable.

Required DLL File:
- fmod.dll (Version: 2.03.06 - Studio API minor release (build 149358))

Additional DLL File (If Errors Occur):
- fmodL.dll (Version: 2.03.06 - Studio API minor release (build 149358))
* The `fmodL.dll` file may be additionally required if the program does not run with `fmod.dll`. If using both files, they must be the same version.


● How to Use

1. Prepare DLL Files:
   - Check for the `fmod.dll` file provided with the program, or directly install FMOD Studio Windows API to get the dll file.
   - Only prepare the `fmodL.dll` file if errors occur when running with `fmod.dll`.

2. Copy DLL Files:
   - Copy the prepared `fmod.dll` file to the same folder as the program executable (.exe) file.

   Basic File Location Example:

   [Program Folder]
   +-- Program.exe
   +-- fmod.dll

   - If errors occur with only `fmod.dll`, copy the `fmodL.dll` file as well.

   File Location Example in Case of Errors:

   [Program Folder]
   +-- Program.exe
   +-- fmod.dll
   +-- fmodL.dll

3. Run the Program:
   - Ensure that the DLL file(s) (`fmod.dll` or `fmod.dll` and `fmodL.dll`) are in the same folder as the program executable, and then run the program.


● Troubleshooting (DLL File Related Issues)

If errors occur even after placing the `fmod.dll` file in the same location as the program, try the following methods:

1. Add `fmodL.dll` File:
   - Download the `fmodL.dll` file and copy it to the same location as the program executable, just like `fmod.dll`.
   - The `fmodL.dll` file must be the same version (2.03.06) as the `fmod.dll` file.
   - After adding the `fmodL.dll` file, try running the program again.

2. Check DLL File Versions:
   - Verify that the versions of both `fmod.dll` (and `fmodL.dll` if added) files are version 2.03.06. If the versions are different, the program may not function correctly.

3. Check for Missing DLL Files:
   - Double-check that the `fmod.dll` (and `fmodL.dll` if added) files are in the same folder as the program executable.


● Important Notes:

- The `fmod.dll` file (and the `fmodL.dll` file if added) is essential for program execution. Be careful not to delete or move it to another location.
- Please ensure that the DLL file versions match the specified version (2.03.06). Incompatible versions may cause problems.
- When using both `fmod.dll` and `fmodL.dll` files together, both files must be the same version. Version mismatch can lead to errors.


● Image Attribution

- Icon Name: Unboxing icons
- Creator: Graphix's Art
- Provider: Flaticon
- URL: https://www.flaticon.com/free-icons/unboxing