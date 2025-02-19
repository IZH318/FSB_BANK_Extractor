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
     * This form is used to present users with instructions and information on how to use the FSB/BANK Extractor GUI application.
     * It loads and displays help content in a RichTextBox, making it easy for users to understand the application's features and operation.
     */
    public partial class HelpForm : Form
    {
        /**
         * @brief Constructor for HelpForm.
         *
         * @details
         * Initializes the HelpForm component and calls LoadHelpContent to display the help information.
         */
        public HelpForm()
        {
            InitializeComponent(); // Initializes the UI components of the form.
            LoadHelpContent();     // Calls the method to load and display the help text.
        }

        /**
         * @brief Loads and displays the help content in the RichTextBox control.
         *
         * @details
         * This method loads help text, which includes instructions and information about the application in both Korean and English.
         * The help text is then displayed in the richTextBoxHelpContent RichTextBox control.
         */
        private void LoadHelpContent()
        {
            // Help content string (KR) - Help text in Korean
            string helpTextKR =
                " ===== FSB/BANK Extractor GUI 도움말 (KR) =====\n\n" +
                " FSB/BANK Extractor GUI는 *.fsb 파일 및 *.bank 파일에서 사운드를 추출하여 표준 WAV 파일로 변환하는 프로그램입니다.\n\n\n" +

                " ● 주요 기능\n" +
                "   - *.fsb, *.bank 파일 추가: 'Add Files' 또는 'Add Folder' 버튼을 클릭하여 WAV 파일로 추출할 *.fsb 또는 *.bank 파일들을 목록에 추가합니다.\n" +
                "     - *.fsb, *.bank 파일 목록은 프로그램 메인 화면의 파일 목록에 표시됩니다.\n" +
                "     - *.fsb, *.bank 파일을 파일 목록으로 드래그 앤 드롭하여 추가할 수도 있습니다.\n\n" +

                "   - *.fsb 파일 상세 정보 보기: 파일 목록에서 *.fsb 파일을 더블 클릭하면 *.fsb 파일의 상세 정보를 보여주는 창이 열립니다.\n" +
                "     - 파일 포맷, 서브 사운드 정보, 메타데이터 태그, 3D 사운드 정보 등 다양한 정보를 확인할 수 있습니다.\n\n" +

                "   - *.bank 파일 처리: *.bank 파일 내에 포함된 *.fsb 파일을 추출하여 WAV 파일로 변환할 수 있습니다.\n" +
                "     - *.bank 파일을 추가하면 프로그램이 자동으로 *.bank 파일 내의 *.fsb 파일을 찾아 추출합니다.\n\n" +

                "   - WAV 파일 일괄 추출: 'Start Batch Extract' 버튼을 클릭하면 파일 목록에 있는 모든 *.fsb, *.bank 파일을 WAV 파일로 일괄 추출합니다.\n" +
                "     - 추출된 WAV 파일은 지정된 출력 폴더에 저장됩니다.\n\n" +

                "   - 출력 폴더 설정: 'Output Options' 그룹 박스에서 WAV 파일이 저장될 폴더를 지정할 수 있습니다.\n" +
                "     - '원본 파일과 동일한 폴더 (Same path as resource file)', '프로그램과 동일한 폴더 (Same path as program)', '사용자 지정 폴더 (Custon output path)' 중 선택 가능합니다.\n\n" +

                "   - Verbose 로그: 'Verbose Log Save' 체크 박스를 활성화하면 추출 과정에 대한 상세 로그가 로그 파일 (_[파일명].log) 로 출력 폴더에 생성됩니다.\n\n" +

                "   - 파일 목록 관리: 파일 목록에서 파일을 선택하여 'Remove' 버튼으로 제거하거나, 'Clear File List' 버튼으로 전체 목록을 비울 수 있습니다.\n\n\n" +


                " ● 사용 방법\n" +
                "   1. 'Add Files' 또는 'Add Folder' 버튼을 클릭하거나, *.fsb, *.bank 파일 또는 폴더를 파일 목록으로 드래그 앤 드롭하여 파일들을 추가하세요.\n" +
                "   2. 필요에 따라 'Output Options' 그룹 박스에서 출력 폴더를 설정하세요.\n" +
                "   3. (선택 사항) 'Verbose Log Save' 체크 박스를 활성화하여 상세 로그 기록 여부를 선택하세요.\n" +
                "   4. 'Start Batch Extract' 버튼을 클릭하여 *.fsb, *.bank 파일을 WAV 파일로 추출하세요.\n\n\n" +


                " ● 지원 파일 형식\n" +
                "   - *.fsb 파일\n" +
                "   - *.bank 파일\n\n\n" +


                " ● 주의 사항\n" +
                "   - *.fsb 파일은 FMOD 라이브러리에서 지원하는 형식이어야 WAV 파일로 정상적으로 추출할 수 있습니다.\n" +
                "   - *.bank 파일 처리 시 *.bank 파일 내부에 유효한 *.fsb 파일이 있어야 정상적으로 추출됩니다.\n" +
                "   - *.bank 파일 처리 시 FSB5 시그니처 검색에 실패하거나 예상치 못한 위치에서 발견될 경우 추출 결과가 올바르지 않을 수 있습니다.\n" +
                "   - 추출 과정 중 오류가 발생하면 오류 메시지가 로그 텍스트 박스와 로그 파일에 표시됩니다.\n\n\n" +


                " ● 추가 정보\n" +
                "   - 정보(About) 메뉴에서 프로그램 버전 및 개발자 정보를 확인할 수 있습니다.\n\n\n" +

                " ● 라이선스 정보\n" +
                "   - 본 프로그램은 FMOD Engine을 사용하였습니다.\n\n" +
                "   - FMOD Engine 저작권: © Firelight Technologies Pty Ltd.\n" +
                "     - FMOD Engine은 라이선스 계약에 따라 사용되었습니다.\n" +
                "     - 자세한 FMOD Engine 라이선스 조건은 다음 파일을 참조하십시오: FMOD_LICENSE.TXT\n" +
                "     - 일반적인 FMOD 최종 사용자 라이선스 계약 (EULA) 정보는 FMOD 웹사이트에서 확인하실 수 있습니다:\n" +
                "       - URL: https://www.fmod.com/licensing#fmod-end-user-license-agreement\n\n" +

                "   - 본 프로그램에 사용된 'Unboxing icons' 아이콘의 출처는 다음과 같습니다.\n" +
                "     - 아이콘 이름: Unboxing icons\n" +
                "     - 제작자: Graphix's Art\n" +
                "     - 제공처: Flaticon\n" +
                "     - URL: https://www.flaticon.com/free-icons/unboxing\n\n" +

                "   - 본 프로그램의 코드 (FMOD Engine 제외) 는 Apache 2.0 라이선스 하에 배포됩니다.\n" +
                "     - Apache License 버전 2.0\n" +
                "       - 귀하는 본 라이선스를 준수하지 않고서는 이 파일을 사용할 수 없습니다.\n" +
                "       - 특정 언어에 대한 라이선스, 허가 및 제한 사항은 라이선스를 참조하십시오.";


            // Help content string (EN) - Help text in English
            string helpTextEN =
                " \n\n\n---------------------------------------------------\n\n\n" +
                " ===== FSB/BANK Extractor GUI Help (EN) =====\n\n" +
                " FSB/BANK Extractor GUI is a program that extracts sounds from *.fsb and *.bank files and converts them to standard WAV files.\n\n\n" +

                " ● Key Features\n" +
                "   - Add *.fsb, *.bank files: Add *.fsb or *.bank files to the list for WAV extraction by clicking 'Add Files' or 'Add Folder' buttons.\n" +
                "     - The list of *.fsb, *.bank files is displayed in the file list on the main program screen.\n" +
                "     - You can also add *.fsb, *.bank files by dragging and dropping them to the file list.\n\n" +

                "   - View detailed information of *.fsb files: Double-click a *.fsb file in the file list to open a window showing detailed information of the *.fsb file.\n" +
                "     - You can check various information such as file format, sub-sound information, metadata tags, and 3D sound information.\n\n" +

                "   - *.bank file processing: You can extract *.fsb files contained within *.bank files and convert them to WAV files.\n" +
                "     - When you add a *.bank file, the program automatically finds and extracts *.fsb files within the *.bank file.\n\n" +

                "   - Batch WAV file extraction: Click the 'Start Batch Extract' button to batch extract all *.fsb, *.bank files in the file list to WAV files.\n" +
                "     - Extracted WAV files are saved in the specified output folder.\n\n" +

                "   - Output folder settings: You can specify the folder where WAV files will be saved in the 'Output Options' group box.\n" +
                "     - You can choose from 'Same path as resource file', 'Same path as program', and 'Custom output path'.\n\n" +

                "   - Verbose log: If you enable the 'Verbose Log Save' checkbox, detailed logs for the extraction process will be generated as log files (_[filename].log) in the output folder.\n\n" +

                "   - File list management: You can select files in the file list and remove them with the 'Remove' button, or clear the entire list with the 'Clear File List' button.\n\n\n" +


                " ● How to Use\n" +
                "   1. Add files by clicking the 'Add Files' or 'Add Folder' buttons, or drag and drop *.fsb, *.bank files or folders to the file list.\n" +
                "   2. If necessary, set the output folder in the 'Output Options' group box.\n" +
                "   3. (Optional) Enable the 'Verbose Log Save' checkbox to choose whether to record detailed logs.\n" +
                "   4. Click the 'Start Batch Extract' button to extract *.fsb, *.bank files to WAV files.\n\n\n" +


                " ● Supported File Formats\n" +
                "   - *.fsb files\n" +
                "   - *.bank files\n\n\n" +


                " ● Precautions\n" +
                "   - *.fsb files must be in a format supported by the FMOD library to be extracted to WAV files normally.\n" +
                "   - For *.bank file processing, valid *.fsb files must exist inside the *.bank file for normal extraction.\n" +
                "   - When processing *.bank files, if FSB5 signature search fails or is found in an unexpected location, the extraction result may not be correct.\n" +
                "   - If an error occurs during the extraction process, an error message will be displayed in the log text box and log file.\n\n\n" +


                " ● Additional Information\n" +
                "   - You can check the program version and developer information in the 정보(About) menu.\n\n\n" +

                " ● License Information\n" +
                "   - This program uses FMOD Engine.\n\n" +
                "   - FMOD Engine Copyright: © Firelight Technologies Pty Ltd.\n" +
                "     - FMOD Engine is used under license agreement.\n" +
                "     - For detailed FMOD Engine license terms, please refer to the following file: FMOD_LICENSE.TXT\n" +
                "     - General FMOD End User License Agreement (EULA) information can be found on the FMOD website:\n" +
                "       - URL: https://www.fmod.com/licensing#fmod-end-user-license-agreement\n\n" +

                "   - Icon Attribution:\n" +
                "     - Icon Name: Unboxing icons\n" +
                "     - Creator: Graphix's Art\n" +
                "     - Provider: Flaticon\n" +
                "     - URL: https://www.flaticon.com/free-icons/unboxing\n\n" +

                "   - The code of this program (excluding FMOD Engine) is distributed under the Apache 2.0 license.\n" +
                "     - Apache License Version 2.0\n" +
                "       - You may not use this file except in compliance with the License.\n" +
                "       - See the License for the specific language governing permissions and limitations under the License.";


            string combinedHelpText = helpTextKR + helpTextEN; // Combines Korean and English help texts into a single string for display.

            richTextBoxHelpContent.Text = combinedHelpText; // Sets the combined help text to the RichTextBox control.
            richTextBoxHelpContent.Select(0, 0); // Sets the caret position to the start of the RichTextBox.
        }
    }
}