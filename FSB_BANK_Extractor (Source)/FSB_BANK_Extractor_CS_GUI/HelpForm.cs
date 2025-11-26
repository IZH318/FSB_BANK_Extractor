// HelpForm.cs
using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace FSB_BANK_Extractor_CS_GUI
{
    /**
     * @class HelpForm
     * @brief Form to display help content for the FSB/BANK Extractor GUI application.
     *
     * @details
     * This form presents instructions based on the visible UI elements and includes mandatory license and attribution information.
     */
    public partial class HelpForm : Form
    {
        public HelpForm()
        {
            InitializeComponent();
            LoadHelpContent();
        }

        private void LoadHelpContent()
        {
            // Help content string (KR)
            string helpTextKR =
                " ===== FSB/BANK Extractor GUI 도움말 (KR) =====\n\n" +
                " 본 프로그램은 FMOD의 *.bank 및 *.fsb 파일을 분석하고, 포함된 오디오 데이터를 WAV로 추출하는 도구입니다.\n\n\n" +

                " ● 1. 파일 불러오기 (File Menu)\n" +
                "   - 파일 추가: 상단 메뉴 'File' -> 'Open File...'을 클릭하거나 파일을 목록으로 드래그합니다.\n" +
                "   - 폴더 추가: 상단 메뉴 'File' -> 'Open Folder...'를 클릭하여 폴더 내의 모든 지원 파일을 불러옵니다.\n" +
                "   - Strings Bank: 파일 목록에 *.strings.bank 파일이 포함되면 자동으로 인식하여 GUID를 이름으로 변환합니다.\n" +
                "     (자동 인식이 안 될 경우 'File' 메뉴의 'Load Strings Bank (Manual)...'을 사용하세요.)\n\n" +

                " ● 2. 구조 탐색 (Structure Explorer)\n" +
                "   - 화면 좌측의 'Structure Explorer' 그룹 박스에서 Bank 파일 내부의 계층 구조를 확인할 수 있습니다.\n" +
                "   - 검색: 상단 'Search:' 텍스트 박스에 검색어를 입력하면 리스트 뷰로 전환되며 이름이 일치하는 항목만 표시됩니다.\n" +
                "     * 팁: 검색 결과에서 우클릭 후 'Open File Location'을 선택하면 트리 구조의 원본 위치로 이동합니다.\n" +
                "   - 보기 설정: 상단 메뉴 'View' -> 'Expand All' 또는 'Collapse All'을 통해 트리 구조를 한 번에 펼치거나 접을 수 있습니다.\n\n" +

                " ● 3. 상세 정보 (Details)\n" +
                "   - 'Structure Explorer'에서 항목을 선택하면, 우측 'Details' 그룹 박스에 상세 정보가 표시됩니다.\n" +
                "   - 표시 항목: Category(분류), Property(속성), Value(값) - 포맷, 채널, 루프 구간, GUID, Sub-Sound Index 등.\n\n" +

                " ● 4. 재생 제어 (Playback & Options)\n" +
                "   - 하단 패널에서 오디오를 제어할 수 있습니다.\n" +
                "   - 버튼: 'Play (▶)', 'Stop (■)' 버튼으로 재생 및 정지가 가능합니다.\n" +
                "   - 볼륨: 우측의 'Volume: XX%' 슬라이더로 음량을 조절합니다.\n" +
                "   - Auto-Play on Select: 이 옵션을 체크하면 항목을 선택할 때마다 자동으로 재생합니다.\n" +
                "   - Force Loop: 이 옵션을 체크하면 오디오의 루프 정보에 따라 무한 반복 재생합니다.\n" +
                "   - Verbose Log Save: 이 옵션을 체크하면 추출 과정의 상세 로그를 파일로 저장합니다.\n\n" +

                " ● 5. 추출 및 내보내기 (Extraction)\n" +
                "   - 선택 추출: 체크박스로 원하는 항목을 선택한 뒤, 'File' -> 'Extract Checked...'를 실행합니다.\n" +
                "   - 전체 추출: 'File' -> 'Extract All...'을 실행하여 목록의 모든 항목을 WAV로 변환합니다.\n" +
                "   - CSV 저장: 'File' -> 'Export List to CSV...'를 클릭하면 현재 목록의 메타데이터를 엑셀 파일로 저장합니다.\n" +
                "   - 드래그 추출: 'Structure Explorer'의 항목을 윈도우 탐색기로 드래그 앤 드롭하여 즉시 추출할 수 있습니다.\n\n" +

                " ● 6. 인덱스 도구 (Index Tools)\n" +
                "   - FSB/Bank 폴더를 우클릭하여 'Index Tools...' 메뉴를 사용할 수 있습니다.\n" +
                "   - Jump to Index: 특정 번호(Sub-Sound Index)를 입력하여 해당 오디오 파일로 즉시 이동합니다.\n" +
                "   - Select Range: '100-200, 305'와 같이 범위나 쉼표를 사용하여 여러 항목을 한 번에 체크(Check)할 수 있습니다.\n\n" +

                " ● 7. 단축키 (Keyboard Shortcuts)\n" +
                "   - Ctrl + O : 파일 열기 (Open File)\n" +
                "   - Ctrl + Shift + O : 폴더 열기 (Open Folder)\n" +
                "   - Ctrl + E : 체크 항목 추출 (Extract Checked)\n" +
                "   - Ctrl + Shift + E : 전체 추출 (Extract All)\n" +
                "   - Ctrl + Shift + C : CSV로 내보내기 (Export CSV)\n" +
                "   - Ctrl + F : 검색창으로 이동 (Focus Search)\n" +
                "   - F1 : 도움말 (Help)\n\n" +

                " ● 8. 우클릭 메뉴 (Context Menu)\n" +
                "   - 공통 기능: 'Select All'(전체 선택), 'Play/Stop'(재생 제어), 'Extract'(추출), 'Copy'(정보 복사) 기능을 수행합니다.\n" +
                "   - 폴더 전용: 'Index Tools'를 통해 인덱스 번호로 하위 항목을 일괄 선택하거나 특정 위치로 즉시 이동합니다.\n" +
                "   - 검색 전용: 'Open File Location'을 통해 검색된 항목이 위치한 실제 트리 경로를 찾아 보여줍니다.\n\n" +

                " ● 9. 라이선스 및 저작권 정보 (License Information)\n" +
                "   - FMOD Engine: 본 프로그램은 FMOD Engine (Core/Studio API) 2.03.06 버전을 사용하였습니다.\n" +
                "     - FMOD Engine 저작권: © Firelight Technologies Pty Ltd.\n" +
                "     - FMOD Engine은 라이선스 계약에 따라 사용되었습니다. 자세한 사항은 FMOD_LICENSE.TXT를 참조하십시오.\n" +
                "   - 아이콘 출처: 'Unboxing icons' created by Graphix's Art - Flaticon.\n" +
                "     - URL: https://www.flaticon.com/free-icons/unboxing\n" +
                "   - 프로그램 라이선스: 본 프로그램의 소스 코드(FMOD 엔진 제외)는 GNU General Public License v3.0 하에 배포됩니다.\n";


            // Help content string (EN)
            string helpTextEN =
                " \n\n---------------------------------------------------\n\n\n" +
                " ===== FSB/BANK Extractor GUI Help (EN) =====\n\n" +
                " This program analyzes FMOD *.bank and *.fsb files and extracts audio data to WAV format.\n\n\n" +

                " ● 1. File Menu\n" +
                "   - Open File...: Load specific *.bank or *.fsb files.\n" +
                "   - Open Folder...: Recursively load all supported files from a selected folder.\n" +
                "   - Load Strings Bank: Use 'File' -> 'Load Strings Bank (Manual)...' if event names are missing (appearing as GUIDs).\n\n" +

                " ● 2. Structure Explorer\n" +
                "   - The left panel displays the hierarchy of the loaded FMOD banks.\n" +
                "   - Search: Type in the 'Search:' text box. The view switches to a list, showing only matching items.\n" +
                "     * Tip: Right-click a search result and select 'Open File Location' to jump to its original position in the tree.\n" +
                "   - View Menu: Use 'View' -> 'Expand All' or 'Collapse All' to manage the tree view.\n\n" +

                " ● 3. Details Panel\n" +
                "   - Select any node in the explorer to view metadata in the 'Details' panel on the right.\n" +
                "   - Columns: Category, Property, Value (Format, Channels, Loop Points, GUID, Sub-Sound Index, etc.).\n\n" +

                " ● 4. Playback & Options\n" +
                "   - Controls: Use 'Play (▶)', 'Stop (■)', and the seek bar at the bottom.\n" +
                "   - Volume: Adjust the slider next to the 'Volume: XX%' label.\n" +
                "   - Auto-Play on Select: If checked, audio plays immediately upon selection.\n" +
                "   - Force Loop: If checked, playback respects loop points defined in the audio file.\n" +
                "   - Verbose Log Save: If checked, creates a detailed log file during extraction.\n\n" +

                " ● 5. Extraction Commands (File Menu)\n" +
                "   - Extract Checked...: Exports only the items currently checked in the list.\n" +
                "   - Extract All...: Exports every audio item found in the tree.\n" +
                "   - Export List to CSV...: Saves a summary of all loaded nodes to a .csv file.\n" +
                "   - Drag & Drop: You can drag items from 'Structure Explorer' directly to Windows Explorer to extract them.\n\n" +

                " ● 6. Index Tools (Right-Click Folder)\n" +
                "   - Jump to Index: Enter a Sub-Sound Index number to instantly scroll to and select that audio file.\n" +
                "   - Select Range: Supports multi-range input (e.g., '100-200, 305') to batch check items by their index.\n\n" +

                " ● 7. Keyboard Shortcuts\n" +
                "   - Ctrl + O : Open File\n" +
                "   - Ctrl + Shift + O : Open Folder\n" +
                "   - Ctrl + E : Extract Checked Items\n" +
                "   - Ctrl + Shift + E : Extract All Items\n" +
                "   - Ctrl + Shift + C : Export to CSV\n" +
                "   - Ctrl + F : Focus Search Box\n" +
                "   - F1 : Help\n\n" +

                " ● 8. Context Menu (Right-Click)\n" +
                "   - Common Actions: Perform 'Select All', 'Play/Stop', 'Extract', and 'Copy' metadata.\n" +
                "   - On Folders: 'Index Tools' allow batch selection or navigation based on sub-sound indexes.\n" +
                "   - On Search Results: 'Open File Location' reveals the item's position within the main directory tree.\n\n" +

                " ● 9. License Information\n" +
                "   - FMOD Engine: This program uses FMOD Engine (Core/Studio API) version 2.03.06.\n" +
                "     - Copyright © Firelight Technologies Pty Ltd.\n" +
                "     - Used under license agreement. Refer to FMOD_LICENSE.TXT for details.\n" +
                "   - Icon Attribution: 'Unboxing icons' created by Graphix's Art - Flaticon.\n" +
                "     - URL: https://www.flaticon.com/free-icons/unboxing\n" +
                "   - Source Code License: The code for this program (excluding FMOD Engine) is distributed under the GNU General Public License v3.0.";

            string combinedHelpText = helpTextKR + helpTextEN;

            richTextBoxHelpContent.Text = combinedHelpText;
            richTextBoxHelpContent.Select(0, 0);
        }
    }
}