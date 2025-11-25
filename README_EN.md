# FSB/BANK Extractor

![Capture_2025_02_19_13_50_51_945](https://github.com/user-attachments/assets/a6eca308-23af-4068-ac3a-75543cc6411f) <BR> <BR>
<img width="786" height="593" alt="Capture_2025_11_25_19_07_50_901" src="https://github.com/user-attachments/assets/fdd07d47-476e-4b7e-b0cb-fb6732294283" /> <BR>



⚠️ **This FSB/BANK Extractor was inspired by the `fsb_aud_extr.exe` program provided in the post **[FMOD FSB files extractor (through their API)](https://zenhax.com/viewtopic.php@t=1901.html)** written by id-daemon on the zenhax.com forum.** <BR> <BR>

This program extracts audio streams from FMOD Sound Bank (.fsb) and Bank (.bank) files and saves them as Waveform Audio (.wav) files. <BR> <BR>

It offers both a Command Line Interface (CLI) version and a Graphical User Interface (GUI) version. <BR> <BR>

📢 **Development Status Notice** <BR>
Development of the **C++ (CLI)** and **C# (CLI)** versions is currently **paused**. <BR>
If you require usage in a CLI environment, please use **[v1.1.0](https://github.com/IZH318/FSB_BANK_Extractor/releases/tag/v1.1.0)**. <BR>

We will provide an update via this README if development on the CLI versions resumes in the future.

<BR>

## 🔍 Key Features and Improvements

- **Common Improvements**

   - **Extended File Handling:**
       - **Bank File Support (.bank):**
           - **Supports both FSB and Bank files.** (The original program only supported FSB files) <BR> <BR>

   - **Enhanced Output Control:**
       - **Various Output Directory Options:** Flexibly select WAV save locations via command line arguments or GUI options (`-res`, `-exe`, `-o` options).
       - **Auto Sub-folder Generation:** Automatically creates sub-folders based on original filenames and sorts WAV files.
       - **Improved WAV Filename Generation:** Uses Sub-Sound names to improve file identification and workflow efficiency.
       - **Supports customized output, organized file structure, and efficient workflow.** <BR> <BR>

   - **Robust Error Handling and Verification:**
       - **Verbose Logging:** Detailed logs (via `-v` argument or GUI checkbox) support in-depth analysis and debugging.
       - **Log Levels:** Logs are classified into INFO, WARNING, and ERROR levels for efficient issue identification.
       - **Function Names in Logs:** Logs record the function name where the error occurred, reducing debugging time.
       - **Progress Indicators (CLI & GUI):** Clear status updates via text in CLI and visual progress bars in GUI.
       - **Enhanced debugging, error tracking, and user feedback.** <BR> <BR>

   - **Internationalization Support:**
       - **Full Unicode Support:** Perfect compatibility with multi-language files using UTF-8 encoding.
       - **Enhanced Filename Compatibility:** Converts special characters unusable in filenames into compatible forms to prevent file system errors.
       - **Global compatibility, data loss prevention, and broad user support.** <BR> <BR>

   - **Improved Code Quality and Maintainability:**
       - **Modern Languages (C++, C#) & OOP Design:** Clean and easily extensible code base.
       - **Automatic Resource Management (RAII/using):** Prevents memory leaks and improves stability.
       - **Template/Generic Programming:** Enhances code reusability and type safety.
       - **Latest FMOD Engine:** Utilizes the latest FMOD Engine (v2.03.06) to leverage the newest features and improvements.
       - **Enhanced code quality, easy maintenance, increased program stability, and utilization of modern FMOD engine features.** <BR> <BR>

- **CLI Version Improvements**

   - **Output Control via Command Line Options:** Flexible output directory selection via `-res`, `-exe`, and `-o` arguments.
   - **Text-based Progress Indicator:** Provides text-based progress updates in the command line output.
   - **Enhanced command line control, improved CLI feedback, and optimization for CLI workflows and automation.** <BR> <BR>

- **GUI Version Improvements**

   - **Completely New Explorer-Style Interface:** 
       - Abandoned the simple file list method (ListView) and introduced a **TreeView system** utilizing the **FMOD Studio API** to visually display the Event, Group, Bus, and Audio hierarchy within Bank files.
   - **Audio Preview System:** 
       - Instantly Play, Pause, and Stop audio within the program without needing to extract.
       - Features a **Seek Bar**, **Volume Control**, and **Force Loop** options for precise audio data verification.
   - **Strings Bank Integration:** 
       - Automatically detects or allows manual loading of `.strings.bank` files to convert encrypted GUIDs (e.g., `{a1b2...}`) into developer-assigned **real event names**.
   - **Real-time Search and Filtering:** 
       - A search bar with a debounce timer filters through thousands of audio nodes, displaying only matching items in a list format.
   - **Integrated Details Panel:** 
       - Eliminate the need for popup windows; clicking an item immediately displays metadata such as Format (PCM, ADPCM, etc.), Channels, Bitrate, Loop points, GUID, and original path in the right panel.
   - **Data Management and Export:** 
       - **CSV Export:** Export the structure and detailed properties of all currently loaded files to a CSV file.
       - **Checkbox-based Extraction:** Select specific items via checkboxes to batch extract only what you need.
   - **Performance Optimization:** 
       - **Parallel Scanning:** Introduced `Parallel.ForEach` to analyze large quantities of files/folders at high speed without UI freezing.
       - **Resource Management:** Safely releases the FMOD system upon program exit and automatically cleans up temporary resources.

<BR>

## 🔄 Update History

### v2.0.0 (2025-11-25) - GUI Only
The GUI version has been revamped from a simple 'extractor' to a comprehensive **'FMOD Audio Analysis Tool'**. **(No changes to CLI versions)**

-   #### **🖥️ Interface and Experience**
    -   **Structure Explorer Introduced**: Replaced the flat list view with a tree view interface that perfectly visualizes the internal hierarchy of FMOD Banks.
    -   **Integrated Main Window**: Integrated detailed information (formerly separate Details Form) into the right panel of the main window for simultaneous navigation and information viewing.
    -   **Icon System**: Applied specific icons for files, folders, events, parameters, and audio nodes to improve visibility.
    -   **Enhanced Status Bar**: Displays the currently processing filename, overall progress, elapsed time, and volume status in real-time.

-   #### **🔊 Audio Playback and Control**
    -   **In-App Player**: Preview sounds directly via the FMOD engine without extraction.
    -   **Playback Control**: Supports Play/Pause/Stop buttons and seeking via a Seek Bar.
    -   **Loop Support**: Test loop behavior using the `Force Loop` option if loop points exist in the source file.
    -   **Auto-Play**: Automatically plays audio upon clicking an item when `Auto-Play on Select` is enabled.

-   #### **💾 Data Processing and Extraction**
    -   **Strings Bank Support**: Added mapping logic to restore obfuscated GUIDs to actual event names by loading `.strings.bank` files. (Supports manual load menu).
    -   **CSV Export**: Added functionality to save detailed info (file list, path, format, length, GUID, etc.) as Excel-compatible CSV files.
    -   **Enhanced Selective Extraction**: Extract only files selected via checkboxes or extract only search results.

-   #### **⚡ Performance and Optimization**
    -   **Parallel Loading System**: Applied `Parallel.ForEach` multi-threading for folder loading to drastically reduce analysis time for thousands of files.
    -   **Search Optimization**: Applied an input delay (Debounce) timer to reduce unnecessary calculations and improve response speed during search input.
    -   **Memory Leak Prevention**: Strengthened the cleanup process to forcibly release FMOD system resources and clear temporary resources upon program exit (`OnFormClosing`).
    -   **FMOD Studio API Integration**: Upgraded the engine to use Studio API alongside the Core API to analyze event structures in Bank files.

-   #### **⌨️ Convenience Features**
    -   **Shortcuts**: `Ctrl+O` (Open File), `Ctrl+Shift+O` (Open Folder), `Ctrl+E` (Extract Checked), `Ctrl+Shift+E` (Extract All), `Ctrl+Shift+C` (Export CSV), `Ctrl+F` (Search), `F1` (Help).
    -   **Context Menu**: Right-click tree nodes to access Play, Stop, Extract, and Copy Name/Path/GUID options.

-   #### **ℹ️ Misc**
    -   **Program Info Update**: Program version updated to `2.0.0`, and some developer information display has been modified.

<br>

<details>
<summary>📜 Previous Updates - Click to Expand</summary>
<br>
<details>
<summary>v1.1.0 (2025-11-18)</summary>
This update focused on preventing data loss during file extraction and significantly improving the organization of extracted files.

-   #### **✨ New Features**
    -   **FMOD Tag-based Auto Folder Generation**: Reads "language" tags included in FMOD sound files to automatically create sub-folders matching language codes (e.g., 'EN', 'JP') and saves files there. This allows for more systematic management of multi-language audio.
-   #### **🛠️ Improvements and Fixes**
    -   **File Overwrite Prevention**: Previously, if multiple sub-sounds had the same name within a single FSB/BANK file, files would be overwritten, causing data loss. Now, numeric suffixes like `_1`, `_2` are automatically appended to ensure all sounds are safely extracted with unique filenames.
    -   **Extraction Logic Refactoring**: Refactored filename generation and path handling logic to increase stability and robustly support new features (tag-based folder creation, overwrite prevention).
    -   **Program Info Update**: Program version updated to `1.1.0`, and developer information display has been modified.
-   #### **📄 License**
    -   **License Change**: The project license has been changed to **GNU General Public License v3.0**.

</details>
<details>
<summary>v1.0.0 (2025-02-19)</summary>
   
-   #### **Misc**
    -   Initial release of `FSB/BANK Extractor`.

</details>
</details>

<BR>

## 💾 Download <BR>
| Program                                | URL                                                | Required | Remarks                                                                                        |
|----------------------------------------|----------------------------------------------------|----------|------------------------------------------------------------------------------------------------|
| `.NET Framework 4.8`             | [Download](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net48)   | Optional | ◼ (Install if errors occur) For GUI use |
| `Visual Studio 2022 (v143)`            | [Download](https://visualstudio.microsoft.com/)   | Optional | ◼ (Developers only) For Solution/Project work |
| `FMOD Engine (v2.03.06)`             | [Download](https://www.fmod.com/download#fmodengine)   | Optional | ◼ (Developers only) For using FMOD API |

<BR>

## 🛠️ Development Environment

**[ Common ]**

1. **OS: Windows 10 Pro 22H2 (x64)** <BR>

2. **IDE: Visual Studio 2022 (v143)** <BR>

3. **API: FMOD Engine (v2.03.06)** <BR> <BR>

**[ C++ CLI Version ]**

- Desktop development with C++ workload required <BR>
- C++ Compiler set to ISO C++17 Standard <BR>
- Windows SDK Version 10.0 (Latest installed version) <BR> <BR>

**[ C# CLI Version and GUI Version ]**

- .NET desktop development workload required <BR>
- C# Compiler targeted for .NET Framework 4.8

<BR>

## ⏩ Usage

**[ ===== FSB_BANK_Extractor_CLI (C++ and C# CLI Versions) ===== ]**

![Capture_2025_02_19_13_50_51_945](https://github.com/user-attachments/assets/a6eca308-23af-4068-ac3a-75543cc6411f) <BR> <BR>

**1. Run Command Prompt (cmd.exe) or PowerShell.** <BR> <BR>

**2. Navigate to the directory where the program is located.** <BR>  Use the `cd <program_file_path>` command (e.g., `cd D:\tools\FSB_BANK_Extractor`) <BR> <BR>

**3. Execute the program by entering the following command**: <BR>

   - **Basic Usage**: `program.exe <audio_file_path>` <BR>
   
   - **Usage with Options**: `program.exe <audio_file_path> [Options]` <BR>
   
       - **※ `program.exe` refers to the C++ CLI exe file or C# CLI exe file.** <BR>
           - C++ Version: `FSB_BANK_Extractor_CPP_CLI.exe` <BR>
           - C# Version: `FSB_BANK_Extractor_CS_CLI.exe` <BR> <BR>

   - `<audio_file_path>`: **Required**, Enter the path of the FSB or Bank file to process. <BR>
     You must enter the **path to the FSB or Bank file**. <BR>
     (* Example: `C:\sounds\music.fsb` or `audio.bank` *) <BR> <BR>

   - `[Options]`: **Optional**, You can selectively use the following options as needed. Each option is added after `<audio_file_path>`, separated by spaces. <BR>
     - `-res`: **Saves WAV files in the same folder as the FSB/Bank file.** (Default option; behaves like `-res` if omitted) <BR>
       **Usage Example**: `program.exe audio.fsb -res` (* `-res` can be omitted, same as `program.exe audio.fsb` *) <BR>

     - `-exe`: **Saves WAV files in the same folder as the program executable.** <BR>
       **Usage Example**: `program.exe sounds.fsb -exe` <BR>

     - `-o <output_directory>`: **Saves WAV files in a user-specified folder.** You must enter the path for the folder to save WAV files in `<output_directory>`. <BR>
       **Usage Example (Absolute Path)**: `program.exe voices.bank -o "C:\output\audio"` <BR>
       **Usage Example (Relative Path)**: `program.exe effects.fsb -o "output_wav"` <BR>

     - `-v`: **Enables Verbose Logging.** <BR>
       **Usage Example**: `program.exe music.bank -v` <BR> <BR>

   - **[ 💡 Tips ]**
     - **Default Option**: If you run `program.exe <audio_file_path>` without options, the `-res` option is applied. <BR>
     - **Select Only One Output Folder Option**: The `-res`, `-exe`, and `-o <output_directory>` options **cannot be used simultaneously**. <BR>
     - **Combine with Verbose Logging**: The `-v` option **can be used together** with output folder options. <BR>
     - **-h or -help Option**: Enter `program.exe -h` or `program.exe -help` to view help. <BR> <BR> <BR>



**[ ===== FSB_BANK_Extractor_CS_GUI (C# GUI Version) ===== ]**

<img width="786" height="593" alt="Capture_2025_11_25_19_07_50_901" src="https://github.com/user-attachments/assets/fdd07d47-476e-4b7e-b0cb-fb6732294283" /> <BR> <BR>

1. Run the `FSB_BANK_Extractor_CS_GUI.exe` file. <BR> <BR>

2. **GUI Operation**:

   - **Loading Files and Folders**:
      - Click **`File` > `Open File...`** or **`Open Folder...`** in the top menu to load files.
      - Alternatively, **Drag and Drop** FSB/Bank files from Windows Explorer onto the program window.
      - **[ 💡 Note ]** If filenames appear as encrypted GUIDs, load a `.strings.bank` file along with them, or use the **`File` > `Load Strings Bank (Manual)...`** menu. <BR> <BR>

   - **Navigation and Preview**:
      - **Structure Explorer**: Check the internal event structure of the Bank in the left Tree View.
      - **Search Filter**: Enter text in the top **Search** bar to filter the list and show only matching items.
      - **Details**: Click an item to view information such as format, channels, and loop points in the right **Details** panel.
      - **Audio Playback**: Use the `Play(▶)`, `Stop(■)` buttons, volume slider, and `Force Loop` option in the bottom panel to verify sounds before extraction. <BR> <BR>

   - **File Extraction**:
      - **Selective Extraction**: Select the **checkboxes** of desired items in the **Structure Explorer (Tree View)** or **Search Result List**. Then click **`File` > `Extract Checked...`** to specify a save folder. (Shortcut: `Ctrl + E`)
      - **Extract All**: Click **`File` > `Extract All...`** to extract all currently loaded items at once. (Shortcut: `Ctrl + Shift + E`) <BR> <BR>

   - **Other Options**:
      - **CSV Export**: Save the file list as an Excel-compatible file via **`File` > `Export List to CSV...`**. (Shortcut: `Ctrl + Shift + C`)
      - **Verbose Logging**: Enable the **`Verbose Log Save`** checkbox at the bottom to save detailed logs of the extraction process to a file. <BR> <BR>

<BR>

## ⚖️ License

- **FMOD**
   - This project was created for personal, non-commercial use and includes the FMOD Engine, which is subject to the **FMOD Engine License Agreement** provided by Firelight Technologies Pty Ltd.
   
   - The full text of the **FMOD Engine License Agreement** for this project is included in the **FMOD_LICENSE.TXT** file.
   
   - **Please refer to the FMOD_LICENSE.TXT file for the specific terms and conditions of the FMOD Engine license applicable to this project.**
   
   - General information about FMOD licensing can be found on the official FMOD website ([FMOD Licensing](https://www.fmod.com/licensing)) and in the general **FMOD End User License Agreement (EULA)** ([FMOD End User License Agreement](https://www.fmod.com/licensing#fmod-end-user-license-agreement)).
   
   - **Key points regarding the use of the FMOD Engine in this project (Summary - see FMOD_LICENSE.TXT for details):**
     
      - **License:** The **FMOD_LICENSE.TXT** file contains the definitive license terms for the FMOD Engine in this project.
      - **Non-Commercial Use:** This project may be used only for personal, educational, or hobby purposes, and is licensed for non-commercial use under the terms of the attached **FMOD_LICENSE.TXT**. It cannot be used for commercial purposes, revenue generation, or any form of monetary gain.
      - **Attribution (When Distributing the Program):** If you distribute a program built with the FMOD Engine for non-commercial purposes permitted by the license, you must include the "FMOD" and "Firelight Technologies Pty Ltd." attribution within the program as specified in the general FMOD EULA and **FMOD_LICENSE.TXT** file.
      - **Redistribution Restrictions:** Redistribution of FMOD Engine components in this project follows the terms specified in the **FMOD_LICENSE.TXT** file and the general FMOD EULA. generally, only runtime libraries are allowed for redistribution in a non-commercial context. <BR> <BR>

- **Icons Used in This Project:**

  - **Icon Name:** Unboxing icons
   - **Creator:** Graphix's Art
   - **Source:** Flaticon
   - **URL:** https://www.flaticon.com/free-icons/unboxing <BR> <BR>

- **Project Code License**

   - The code for this project, excluding the FMOD Engine and the icons themselves, is licensed under the **GNU General Public License v3.0**.

<BR>

## 👏 Special Thanks To & References

-   **[FMOD FSB files extractor (through their API)](https://zenhax.com/viewtopic.php@t=1901.html)**
    -   The `fsb_aud_extr.exe` created by **id-daemon** on the zenhax.com forum was a crucial reference that provided the core idea for this tool.
-   **[Redelax](https://github.com/Redelax)**
    -   Reported the issue where data was overwritten and lost when filenames were duplicated. Thanks to this, we were able to improve the program to be more stable.
