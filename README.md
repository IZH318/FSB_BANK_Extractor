# FSB/BANK Extractor

![캡처_2025_02_19_13_50_51_945](https://github.com/user-attachments/assets/a6eca308-23af-4068-ac3a-75543cc6411f) <BR> <BR>
![캡처_2025_02_19_13_51_10_306](https://github.com/user-attachments/assets/66ae8852-e84b-4cb5-99de-f46239863769) <BR>

⚠️ **본 FSB/BANK Extractor는 zenhax.com 포럼의 id-daemon 이 작성한 게시글 ([FMOD FSB files extractor (through their API)](https://zenhax.com/viewtopic.php@t=1901.html)) 에서 제공 된 `fsb_aud_extr.exe` 프로그램에서 아이디어를 얻어 제작되었습니다.** <BR> <BR>

FMOD Sound Bank (.fsb) 및 Bank (.bank) 파일에서 오디오 스트림을 추출하여 Waveform Audio (.wav) 파일로 저장하는 프로그램입니다. <BR> <BR>

명령줄 (CLI) 버전과 그래픽 사용자 인터페이스 (GUI) 버전을 모두 제공합니다.

<BR>

## 🔍 주요 기능 및 개선 사항

- **공통 개선 사항**

   - **확장된 파일 처리 기능:**
       - **Bank 파일 지원 (.bank):**
           - **FSB, Bank 파일 모두 지원.** (기존 프로그램은 FSB 파일만 지원) <BR> <BR>

   - **향상된 출력 제어:**
       - **다양한 출력 디렉토리 옵션:** 명령줄 인수/GUI 옵션으로 WAV 저장 위치 유연하게 선택 가능 (`-res`, `-exe`, `-o` 옵션).
       - **자동 하위 폴더 생성:** 원본 파일명 기반 하위 폴더 자동 생성 및 WAV 파일 분류 저장.
       - **개선된 WAV 파일 이름 생성:** Sub-Sound 이름 활용으로 파일 식별성 및 워크플로우 효율 향상.
       - **사용자 맞춤형 출력, 체계적인 파일 구성, 효율적인 워크플로우 지원.** <BR> <BR>

   - **강력한 오류 처리 및 검증:**
       - **Verbose Logging:** 상세 로그 (명령줄 인수 `-v` or GUI 체크박스 활성화) 로 심층 분석 및 디버깅 지원.
       - **로그 레벨 구분:** INFO, WARNING, ERROR 레벨로 로그 분류, 효율적인 문제 식별.
       - **로그 메시지에 함수 이름 명시:** 오류 발생 함수 이름 로그 기록, 디버깅 시간 단축.
       - **진행률 표시 (CLI & GUI):** CLI 텍스트, GUI 시각적 진행률 표시로 작업 상태 명확히 제공.
       - **향상된 디버깅, 오류 추적, 사용자 피드백 강화.** <BR> <BR>

   - **국제화 지원:**
       - **유니코드 완벽 지원:** UTF-8 인코딩으로 다국어 파일 완벽 호환.
       - **파일명 호환성 강화:** 파일 이름에 사용 할 수 없는 특수 기호를 호환 형태로 변환하여 파일 시스템 오류 예방
       - **글로벌 호환성, 데이터 손실 방지, 폭넓은 사용자 지원.** <BR> <BR>

   - **향상된 코드 품질 및 유지보수성:**
       - **최신 언어 (C++, C#) 및 OOP 디자인:** 깔끔하고 확장 용이한 코드 기반.
       - **자동 리소스 관리 (RAII/using):** 메모리 누수 방지 및 안정성 향상.
       - **템플릿/제네릭 프로그래밍:** 코드 재사용성 및 타입 안전성 향상.
       - **최신 FMOD Engine 버전 사용:** 최신 FMOD Engine (v2.03.06) 을 사용하여 최신 기능 및 개선 사항 활용.
       - **향상된 코드 품질, 쉬운 유지보수, 프로그램 안정성 증대, 최신 FMOD 엔진 기능 활용.** <BR> <BR>

- **CLI 버전 개선 사항**

   - **명령줄 옵션을 통한 출력 제어:** `-res`, `-exe`, `-o` 명령줄 인수를 통해 유연한 출력 디렉토리 선택 기능 제공.
   - **텍스트 기반 진행률 표시기:** 명령줄 출력에 텍스트 기반 진행률 업데이트 제공.
   - **명령줄 제어 강화, CLI 환경 피드백 개선, 명령줄 워크플로우 및 자동화 작업 최적화.** <BR> <BR>

- **GUI 버전 개선 사항**

   - **그래픽 사용자 인터페이스 (GUI):** 명령줄 지식 없이 누구나 쉽게 사용 가능한 사용자 친화적 인터페이스.
   - **시각적 파일 목록 (ListView):** 드래그 앤 드롭, 상태 표시 기능으로 직관적인 파일 관리.
   - **드래그 앤 드롭 파일 및 폴더 추가:** 간편한 드래그 앤 드롭으로 파일 추가, 사용자 경험 향상.
   - **메뉴 기반 인터페이스:** 메뉴를 통한 프로그램 기능 접근 용이.
   - **GUI 기반 실시간 로깅:** GUI 텍스트 상자에 로그 메시지 실시간 출력, 즉각적인 피드백 제공.
   - **GUI 오류 메시지 상자:** 팝업 창으로 오류 즉시 알림 및 문제 해결 지원.
   - **도움말 메뉴 및 상세 도움말 정보:** GUI 내에서 프로그램 사용법 정보에 쉽게 접근.
   - **프로그램 정보 메뉴:** GUI 내에서 프로그램 버전, 개발자 정보 등 확인 용이.
   - **시각적 진행률 표시기:** 시각적 진행률 막대 및 상태 레이블로 작업 진행 상황 명확히 제공.
   - **다중 파일 일괄 처리 (Batch Processing):** GUI 환경에서 편리한 일괄 처리 지원.
   - **폴더 단위 파일 추가 (GUI Drag & Drop):** 폴더 Drag & Drop으로 폴더 내 파일 자동 추가, 폴더 기반 작업 효율 극대화.
   - **사용자 친화적, 직관적인 작동, 향상된 시각적 피드백, 모든 사용자에게 높은 접근성, 편리한 GUI 기반 일괄 처리 및 폴더 작업 지원.**

<BR>

## 💾 다운로드 <BR>
| Program                                | URL                                                | 필수여부 | 비고                                                                                           |
|----------------------------------------|----------------------------------------------------|----------|------------------------------------------------------------------------------------------------|
| `Visual Studio 2022 (v143)`            | [Download](https://visualstudio.microsoft.com/)   | 필수     | ◼ 솔루션(프로젝트) 작업 |
| `FMOD Engine (v2.03.06)`             | [Download](https://www.fmod.com/download#fmodengine)   | 필수     | ◼ FMOD API 사용 |

<BR>

## 🛠️ 개발 환경

**[ 공통 ]**

1. **OS: Windows 10 Pro 22H2 (x64)** <BR>

2. **IDE: Visual Studio 2022 (v143)** <BR>

3. **API: FMOD Engine (v2.03.06)** <BR> <BR>

**[ C++ CLI 버전 ]**

- C++ 를 사용한 데스크톱 개발 워크로드 필요 <BR>
- C++ 컴파일러는 ISO C++17 표준으로 설정 <BR>
- Windows SDK 버전 10.0 (최신 설치 버전) <BR> <BR>

**[ C# CLI 버전 및 GUI 버전 ]**

- .NET 데스크톱 개발 워크로드 필요 <BR>
- C# 컴파일러는 .NET Framework 4.8 타겟으로 설정

<BR>

## ⏩ 사용 방법

**[ ===== FSB_BANK_Extractor_CLI (C++ 및 C# CLI 버전) ===== ]**

![캡처_2025_02_19_13_50_51_945](https://github.com/user-attachments/assets/a6eca308-23af-4068-ac3a-75543cc6411f) <BR> <BR>

**1. 명령 프롬프트 (cmd.exe) 또는 PowerShell** 을 실행합니다. <BR> <BR>

**2. 프로그램이 위치한 디렉토리로 이동**합니다. <BR>  `cd <프로그램_파일_경로>` 명령어 사용 (예: `cd D:\tools\FSB_BANK_Extractor`) <BR> <BR>

**3. 다음 명령어를 입력하여 프로그램 실행**: <BR>

   - **기본 사용법**: `프로그램.exe <audio_file_path>` <BR>
   
   - **옵션과 함께 사용**: `프로그램.exe <audio_file_path> [Options]` <BR>
   
       - **※ `프로그램.exe`는 C++ CLI exe 파일 또는 C# CLI exe 파일을 지칭합니다.** <BR>
           - C++ 버전: `FSB_BANK_Extractor_CPP_CLI.exe` <BR>
           - C# 버전: `FSB_BANK_Extractor_CS_CLI.exe` <BR> <BR>

   - `<audio_file_path>`: **필수**,  처리할 FSB 또는 Bank 파일의 경로를 입력합니다. <BR>
     **FSB 또는 Bank 파일의 경로**를 입력해야 합니다. <BR>
     (* 예시: `C:\sounds\music.fsb` 또는 `audio.bank` *) <BR> <BR>

   - `[Options]`: **선택 사항**, 필요에 따라 다음 옵션을 선택적으로 사용할 수 있습니다. 각 옵션은 `<audio_file_path>` 뒤에 공백으로 구분하여 추가합니다. <BR>
     - `-res`: **WAV 파일을 FSB/Bank 파일과 동일한 폴더에 저장**합니다. (기본 옵션, 옵션 생략 시 `-res` 와 동일하게 동작) <BR>
       **사용 예시**: `프로그램.exe audio.fsb -res` (* `-res` 옵션 생략 가능, `프로그램.exe audio.fsb` 와 동일 *) <BR>

     - `-exe`: **WAV 파일을 프로그램 실행 파일과 동일한 폴더에 저장**합니다. <BR>
       **사용 예시**: `프로그램.exe sounds.fsb -exe` <BR>

     - `-o <output_directory>`: **WAV 파일을 사용자가 지정한 폴더에 저장**합니다. `<output_directory>` 에는 WAV 파일을 저장할 폴더의 경로를 입력해야 합니다. <BR>
       **사용 예시 (절대 경로)**: `프로그램.exe voices.bank -o "C:\output\audio"` <BR>
       **사용 예시 (상대 경로)**: `프로그램.exe effects.fsb -o "output_wav"` <BR>

     - `-v`: **Verbose Logging 기능을 활성화**합니다. <BR>
       **사용 예시**: `프로그램.exe music.bank -v` <BR> <BR>

   - **[ 💡 사용 팁 ]**
     - **기본 옵션**: 옵션을 생략하고 `프로그램.exe <audio_file_path>` 와 같이 실행하면, `-res` 옵션이 적용됩니다. <BR>
     - **출력 폴더 옵션 중 하나만 선택**: `-res`, `-exe`, `-o <output_directory>` 옵션은 **동시에 사용할 수 없습니다**. <BR>
     - **Verbose Logging 옵션과 조합**: `-v` 옵션은 출력 폴더 옵션과 **함께 사용 가능**합니다. <BR>
     - **-h 또는 -help 옵션**: `프로그램.exe -h` 또는 `프로그램.exe -help` 를 입력하면 도움말을 볼 수 있습니다. <BR> <BR> <BR>



**[ ===== FSB_BANK_Extractor_CS_GUI (C# GUI 버전) ===== ]**

![캡처_2025_02_19_13_51_10_306](https://github.com/user-attachments/assets/66ae8852-e84b-4cb5-99de-f46239863769) <BR> <BR>

1. `FSB_BANK_Extractor_CS_GUI.exe` 파일을 실행합니다. <BR> <BR>

2. **GUI 조작**:

   - **FSB/Bank 파일 추가**:
      - "파일 추가" 또는 "폴더 추가" 버튼을 클릭하여 파일 또는 폴더를 선택합니다.
      - 또는, ListView 에 FSB/Bank 파일을 드래그 앤 드롭합니다. <BR> <BR>
   
   - **출력 디렉토리 선택**:
      - "리소스 파일과 동일한 폴더", "프로그램 파일과 동일한 폴더", "사용자 지정 폴더" 중 원하는 출력 디렉토리 옵션을 선택합니다.
      - "사용자 지정 폴더" 선택 시, "폴더 찾아보기" 버튼을 클릭하여 출력 디렉토리를 지정합니다. <BR> <BR>
      
   - **Verbose Logging 설정 (선택 사항)**: "Verbose Logging" 체크 박스를 선택하여 활성화합니다. <BR> <BR>
      
   - **작업 시작**: "Start Batch Extract" 버튼을 클릭하여 파일 추출을 시작합니다. <BR> <BR>

   - **[ 💡 참고 ]** 파일 목록에서 항목을 더블 클릭하면 FSB/Bank 파일 정보를 확인할 수 있습니다. <BR> <BR>
   ![캡처_2025_02_19_13_58_44_74](https://github.com/user-attachments/assets/9dc6e3c4-afbb-4bd7-bc6a-66856c56285d)

<BR>

## ⚖️ 라이선스

- **FMOD**
   - 본 프로젝트는 개인적, 비상업적 용도로 제작되었으며, Firelight Technologies Pty Ltd.에서 제공하는 **FMOD 엔진 라이선스 계약**의 적용을 받는 FMOD 엔진을 포함하고 있습니다.
   
   - 본 프로젝트에 대한 **FMOD 엔진 라이선스 계약 전문**은 **FMOD_LICENSE.TXT** 파일에 포함되어 있습니다.
   
   - **본 프로젝트에 적용되는 FMOD 엔진 라이선스의 구체적인 조건 및 조항은 FMOD_LICENSE.TXT 파일을 참조하시기 바랍니다.**
   
   - FMOD 라이선스에 대한 일반적인 정보는 공식 FMOD 웹사이트 ([FMOD Licensing](https://www.fmod.com/licensing)) 및 일반적인 **FMOD 최종 사용자 라이선스 계약 (EULA)** ([FMOD End User License Agreement](https://www.fmod.com/licensing#fmod-end-user-license-agreement)) 에서 확인하실 수 있습니다.
   
   - **본 프로젝트에서 FMOD 엔진 사용과 관련된 주요 사항 (요약 - 자세한 내용은 FMOD_LICENSE.TXT 파일 참조):**
     
      - **라이선스:** **FMOD_LICENSE.TXT** 파일은 본 프로젝트의 FMOD 엔진 라이선스에 대한 최종적인 라이선스 조건을 담고 있습니다.
      - **비상업적 용도:** 본 프로젝트는 개인적, 교육적 또는 취미 목적으로만 사용될 수 있으며, 첨부된 **FMOD_LICENSE.TXT** 파일의 조건에 따라 비상업적 용도로 라이선스가 허여되었습니다. 상업적 사용, 수익 창출 또는 어떠한 형태의 금전적 이익을 위한 용도로는 사용될 수 없습니다.
      - **저작자 표시 (프로그램 배포 시):** 라이선스에서 허용하는 비상업적 목적으로 FMOD 엔진으로 빌드된 프로그램을 배포하는 경우, 일반적인 FMOD EULA 및 **FMOD_LICENSE.TXT** 파일에 명시된 바에 따라 프로그램 내에 "FMOD" 및 "Firelight Technologies Pty Ltd." 저작자 표시를 포함해야 합니다.
      - **재배포 제한:** 본 프로젝트에서 FMOD 엔진 구성 요소의 배포는 **FMOD_LICENSE.TXT** 파일 및 일반적인 FMOD EULA에 명시된 조건을 따릅니다. 일반적으로 비상업적 맥락에서 런타임 라이브러리만 재배포가 허용됩니다. <BR> <BR>

- **본 프로젝트에서 사용 된 아이콘:**

  - **아이콘 이름:** Unboxing icons
   - **제작자:** Graphix's Art
   - **제공처:** Flaticon
   - **URL:** https://www.flaticon.com/free-icons/unboxing <BR> <BR>

- **프로젝트 코드 라이선스**

   - FMOD Engine 및 아이콘 자체를 제외한 본 프로젝트의 코드는 **Apache 2.0 License** 하에 라이선스가 부여됩니다.
