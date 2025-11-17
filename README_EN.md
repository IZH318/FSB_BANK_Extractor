# FSB/BANK Extractor

![캡처_2025_02_19_13_50_51_945](https://github.com/user-attachments/assets/a6eca308-23af-4068-ac3a-75543cc6411f) <BR> <BR>
![캡처_2025_02_19_13_51_10_306](https://github.com/user-attachments/assets/66ae8852-e84b-4cb5-99de-f46239863769) <BR>

⚠️ **This FSB/BANK Extractor was created based on the ideas from the `fsb_aud_extr.exe` program provided in a post by id-daemon on the zenhax.com forum: [FMOD FSB files extractor (through their API)](https://zenhax.com/viewtopic.php@t=1901.html).** <BR> <BR>

This program extracts audio streams from FMOD Sound Bank (.fsb) and Bank (.bank) files and saves them as Waveform Audio (.wav) files. <BR> <BR>

Both Command-Line Interface (CLI) and Graphical User Interface (GUI) versions are provided.

<BR>

## 🔍 Key Features and Improvements

- **Common Improvements**

   - **Extended File Processing Capabilities:**
       - **Bank File Support (.bank):**
           - **Supports both FSB and Bank files.** (The original program only supported FSB files) <BR> <BR>

   - **Enhanced Output Control:**
       - **Various Output Directory Options:** Flexibly choose the WAV save location with command-line arguments/GUI options (`-res`, `-exe`, `-o` options).
       - **Automatic Subfolder Creation:** Automatically creates subfolders based on the original filenames and saves WAV files categorized within them.
       - **Improved WAV Filename Generation:** Enhances file identifiability and workflow efficiency by utilizing Sub-Sound names.
       - **Supports customized output, systematic file organization, and efficient workflow.** <BR> <BR>

   - **Robust Error Handling and Verification:**
       - **Verbose Logging:** Supports in-depth analysis and debugging with detailed logs (command-line argument `-v` or GUI checkbox activation).
       - **Log Level Differentiation:** Classifies logs into INFO, WARNING, and ERROR levels for efficient problem identification.
       - **Function Name Indication in Log Messages:** Records the name of the function where the error occurred in the log, reducing debugging time.
       - **Progress Display (CLI & GUI):** Clearly provides task status with CLI text and GUI visual progress indicators.
       - **Enhanced debugging, error tracking, and user feedback.** <BR> <BR>

   - **Internationalization Support:**
       - **Full Unicode Support:** Fully compatible with multilingual files using UTF-8 encoding.
       - **Filename Compatibility Enhancement:** Prevents file system errors by converting special characters unusable in filenames into compatible forms.
       - **Global compatibility, data loss prevention, and broad user support.** <BR> <BR>

   - **Improved Code Quality and Maintainability:**
       - **Latest Languages (C++, C#) and OOP Design:** Clean and easily extensible code base.
       - **Automatic Resource Management (RAII/using):** Prevents memory leaks and improves stability.
       - **Template/Generic Programming:** Enhances code reusability and type safety.
       - **Using the Latest FMOD Engine Version:** Utilizes the latest FMOD Engine (v2.03.06) to leverage the latest features and improvements.
       - **Improved code quality, easy maintenance, increased program stability, and utilization of the latest FMOD engine features.** <BR> <BR>

- **CLI Version Improvements**

   - **Output Control via Command-Line Options:** Provides flexible output directory selection via `-res`, `-exe`, and `-o` command-line arguments.
   - **Text-Based Progress Indicator:** Provides text-based progress updates in the command-line output.
   - **Enhanced command-line control, improved CLI environment feedback, and optimized command-line workflow and automation tasks.** <BR> <BR>

- **GUI Version Improvements**

   - **Graphical User Interface (GUI):** User-friendly interface that anyone can easily use without command-line knowledge.
   - **Visual File List (ListView):** Intuitive file management with drag-and-drop and status display features.
   - **Drag & Drop File and Folder Addition:** Improves user experience with easy drag-and-drop file addition.
   - **Menu-Based Interface:** Easy access to program functions through menus.
   - **GUI-Based Real-Time Logging:** Real-time output of log messages to a GUI text box, providing immediate feedback.
   - **GUI Error Message Boxes:** Immediately notifies errors with pop-up windows and supports troubleshooting.
   - **Help Menu and Detailed Help Information:** Easy access to program usage information within the GUI.
   - **Program Information Menu:** Easy to check program version, developer information, etc., within the GUI.
   - **Visual Progress Indicator:** Clearly provides task progress with visual progress bars and status labels.
   - **Batch Processing for Multiple Files:** Convenient batch processing support in the GUI environment.
   - **Folder-Wise File Addition (GUI Drag & Drop):** Automatic addition of files within a folder by dragging and dropping a folder, maximizing efficiency for folder-based tasks.
   - **User-friendly, intuitive operation, enhanced visual feedback, high accessibility for all users, and convenient GUI-based batch processing and folder task support.**

<BR>

## 🔄 Update History

### v1.1.0 (2025-11-17)
This update focuses on preventing data loss that can occur during file extraction and significantly improving the convenience of organizing extracted files.
-   #### **✨ New Features**
    -   **Automatic Folder Creation Based on FMOD Tags**: Reads the "language" tag within FMOD sound files to automatically create subfolders named with language codes like 'EN', 'JP', etc., and saves the files into the corresponding folders. This allows for more systematic management of files containing multilingual audio.
-   #### **🛠️ Improvements and Fixes**
    -   **Added File Overwrite Protection**: Previously, if multiple sub-sounds with the same name existed within a single FSB/BANK file, they would be overwritten, resulting in data loss. Now, a numeric suffix like `_1`, `_2` is automatically appended to ensure all sounds are safely extracted with unique filenames.
    -   **Extraction Logic Refactoring**: Refactored the filename generation and path handling logic to improve stability and provide robust support for new features (tag-based folder creation, overwrite protection).
    -   **Program Information Update**: The program version has been updated to `1.1.0`, and some developer information has been revised.
-   #### **📄 License**
    -   **License Change**: The project's license has been changed to **GPL-3.0**.

<br>

<details>
<summary>📜 Previous Update History - Click to expand</summary>
<br>
<details>
<summary>v1.0.0 (2025-02-19)</summary>
   
-   #### **Other**
    -   `FSB/BANK Extractor` released

</details>
</details>

<BR>

## 💾 Download <BR>
| Program                                | URL                                                | Required | Remarks                                                                                           |
|----------------------------------------|----------------------------------------------------|----------|------------------------------------------------------------------------------------------------|
| `.NET Framework 4.8`             | [Download](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net48)   | Optional | ◼ (Install if errors occur) For GUI use |
| `Visual Studio 2022 (v143)`            | [Download](https://visualstudio.microsoft.com/)   | Optional | ◼ (For developers only) Solution (Project) Work |
| `FMOD Engine (v2.03.06)`             | [Download](https://www.fmod.com/download#fmodengine)   | Optional | ◼ (For developers only) Using FMOD API |

<BR>

## 🛠️ Development Environment

**[ Common ]**

1. **OS: Windows 10 Pro 22H2 (x64)** <BR>

2. **IDE: Visual Studio 2022 (v143)** <BR>

3. **API: FMOD Engine (v2.03.06)** <BR> <BR>

**[ C++ CLI Version ]**

- Desktop development with C++ workload is required <BR>
- C++ compiler is set to ISO C++17 standard <BR>
- Windows SDK Version 10.0 (latest installed version) <BR> <BR>

**[ C# CLI Version and GUI Version ]**

- .NET desktop development workload is required <BR>
- C# compiler is set to target .NET Framework 4.8

<BR>

## ⏩ How to Use

**[ ===== FSB_BANK_Extractor_CLI (C++ and C# CLI Versions) ===== ]**

![캡처_2025_02_19_13_50_51_945](https://github.com/user-attachments/assets/a6eca308-23af-4068-ac3a-75543cc6411f) <BR> <BR>

**1. Run Command Prompt (cmd.exe) or PowerShell.** <BR> <BR>

**2. Navigate to the directory where the program is located.** <BR> Use the `cd <program_file_path>` command (e.g., `cd D:\tools\FSB_BANK_Extractor`) <BR> <BR>

**3. Execute the program by entering the following command**: <BR>

   - **Basic Usage**: `program.exe <audio_file_path>` <BR>

   - **Usage with Options**: `program.exe <audio_file_path> [Options]` <BR>

       - **※ `program.exe` refers to either the C++ CLI exe file or the C# CLI exe file.** <BR>
           - C++ Version: `FSB_BANK_Extractor_CPP_CLI.exe` <BR>
           - C# Version: `FSB_BANK_Extractor_CS_CLI.exe` <BR> <BR>

   - `<audio_file_path>`: **Required**, enter the path to the FSB or Bank file to be processed. <BR>
     You must enter the **path to the FSB or Bank file**. <BR>
     (* Example: `C:\sounds\music.fsb` or `audio.bank` *) <BR> <BR>

   - `[Options]`: **Optional**, you can selectively use the following options as needed. Add each option after `<audio_file_path>` separated by a space. <BR>
     - `-res`: **Saves WAV files in the same folder as the FSB/Bank file.** (Default option, behaves the same as `-res` if the option is omitted) <BR>
       **Usage Example**: `program.exe audio.fsb -res` (* `-res` option can be omitted, same as `program.exe audio.fsb` *) <BR>

     - `-exe`: **Saves WAV files in the same folder as the program executable file.** <BR>
       **Usage Example**: `program.exe sounds.fsb -exe` <BR>

     - `-o <output_directory>`: **Saves WAV files in the folder specified by the user.**  `<output_directory>` must be the path to the folder where WAV files will be saved. <BR>
       **Usage Example (Absolute Path)**: `program.exe voices.bank -o "C:\output\audio"` <BR>
       **Usage Example (Relative Path)**: `program.exe effects.fsb -o "output_wav"` <BR>

     - `-v`: **Activates Verbose Logging feature.** <BR>
       **Usage Example**: `program.exe music.bank -v` <BR> <BR>

   - **[ 💡 Usage Tips ]**
     - **Default Option**: If you omit options and run it like `program.exe <audio_file_path>`, the `-res` option is applied. <BR>
     - **Select Only One Output Folder Option**: The options `-res`, `-exe`, and `-o <output_directory>` **cannot be used simultaneously**. <BR>
     - **Combination with Verbose Logging Option**: The `-v` option can be used **together with** output folder options. <BR>
     - **-h or -help Options**: You can view help information by entering `program.exe -h` or `program.exe -help`. <BR> <BR> <BR>



**[ ===== FSB_BANK_Extractor_CS_GUI (C# GUI Version) ===== ]**

![캡처_2025_02_19_13_51_10_306](https://github.com/user-attachments/assets/66ae8852-e84b-4cb5-99de-f46239863769) <BR> <BR>

1. Run the `FSB_BANK_Extractor_CS_GUI.exe` file. <BR> <BR>

2. **GUI Operations**:

   - **Add FSB/Bank Files**:
      - Click the "Add File" or "Add Folder" button to select files or folders.
      - Or, drag and drop FSB/Bank files onto the ListView. <BR> <BR>

   - **Select Output Directory**:
      - Choose the desired output directory option among "Same folder as resource file", "Same folder as program file", and "Custom folder".
      - If "Custom folder" is selected, specify the output directory by clicking the "Browse Folder" button. <BR> <BR>

   - **Verbose Logging Setting (Optional)**: Activate Verbose Logging by checking the "Verbose Logging" checkbox. <BR> <BR>

   - **Start Task**: Click the "Start Batch Extract" button to start file extraction. <BR> <BR>

   - **[ 💡 Note ]** You can check FSB/Bank file information by double-clicking an item in the file list. <BR> <BR>
   ![캡처_2025_02_19_13_58_44_74](https://github.com/user-attachments/assets/9dc6e3c4-afbb-4bd7-bc6a-66856c56285d)

<BR>

## ⚖️ License

- **FMOD**
   - This project is created for personal, non-commercial use and includes the FMOD Engine, which is subject to the **FMOD Engine License Agreement** provided by Firelight Technologies Pty Ltd.

   - The **full text of the FMOD Engine License Agreement** for this project is included in the **FMOD_LICENSE.TXT** file.

   - **Please refer to the FMOD_LICENSE.TXT file for the specific terms and conditions of the FMOD Engine License applicable to this project.**

   - General information about the FMOD License can be found on the official FMOD website ([FMOD Licensing](https://www.fmod.com/licensing)) and the general **FMOD End User License Agreement (EULA)** ([FMOD End User License Agreement](https://www.fmod.com/licensing#fmod-end-user-license-agreement)).

   - **Key points regarding the use of the FMOD Engine in this project (Summary - For detailed information, refer to the FMOD_LICENSE.TXT file):**

      - **License:** The **FMOD_LICENSE.TXT** file contains the final license terms for the FMOD Engine license for this project.
      - **Non-Commercial Use:** This project may only be used for personal, educational, or hobby purposes, and is licensed for non-commercial use according to the terms of the attached **FMOD_LICENSE.TXT** file. It may not be used for commercial purposes, profit generation, or any form of monetary gain.
      - **Attribution (When distributing the program):** If distributing programs built with the FMOD Engine for non-commercial purposes permitted by the license, you must include the "FMOD" and "Firelight Technologies Pty Ltd." attribution within the program, as specified in the general FMOD EULA and the **FMOD_LICENSE.TXT** file.
      - **Redistribution Restrictions:** Redistribution of FMOD Engine components in this project is subject to the conditions specified in the **FMOD_LICENSE.TXT** file and the general FMOD EULA. Generally, only runtime libraries are permitted for redistribution in a non-commercial context. <BR> <BR>

- **Icons used in this project:**

  - **Icon Name:** Unboxing icons
   - **Author:** Graphix's Art
   - **Provider:** Flaticon
   - **URL:** https://www.flaticon.com/free-icons/unboxing <BR> <BR>

- **Project Code License**

   - The code of this project, excluding the FMOD Engine and icons themselves, is licensed under **GNU General Public License v3.0**.

<BR>

## 👏 Special Thanks To & References

-   **[FMOD FSB files extractor (through their API)](https://zenhax.com/viewtopic.php@t=1901.html)**
    -   The `fsb_aud_extr.exe` created by **id-daemon** on the zenhax.com forum is a crucial reference that provided the core idea for this tool.
-   **[Redelax](https://github.com/Redelax)**
    -   Reported an issue where data was lost due to file overwriting when filenames were duplicated. Thanks to this report, the program could be improved to be more stable.
