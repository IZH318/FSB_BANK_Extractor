/**
 * @file FSB_BANK_Extractor_CPP.cpp
 * @brief Extracts audio streams from FMOD Sound Bank (.fsb) and Bank (.bank) files and saves them as Waveform Audio (.wav) files.
 * @author Github IZH318 (https://github.com/IZH318)
 *
 * @details
 * This program utilizes the FMOD Engine API to load and process FSB audio files.
 * It is designed to extract individual sounds (sub-sounds) from FSB files and save each sound as a separate WAV file.
 * Additionally, it can process Bank (.bank) files, identify and extract embedded FSB files within them, and then proceed to extract sounds from these embedded FSBs.
 * The program offers several output options to customize where the extracted WAV files are saved:
 *  - Saving in the same directory as the source file (default).
 *  - Saving in the same directory as the executable.
 *  - Saving in a user-specified directory.
 *
 * Verbose logging is also available to provide detailed information about the extraction process,
 * which can be helpful for debugging or verifying the program's operation.
 *
 * FMOD Engine & Development Environment Compatibility:
 *
 * Tested Environment:
 *  - FMOD Engine Version: v2.03.06 (Studio API minor release, build 149358)
 *  - Development Environment: Visual Studio 2022 (v143)
 *  - C++ Standard: ISO C++17 (/std:c++17)
 *  - Windows SDK Version: 10.0 (latest installed version)
 *  - Primary Test Platform: Windows 10 64-bit
 *  - Last Development Date: 2025-02-17
 *
 * Recommendations for Best Results:
 *  - Use FMOD Engine v2.03.06 for development and deployment.
 *  - Develop and build the program within Visual Studio 2022 (v143).
 *  - Ensure your compiler is set to use the ISO C++17 standard.
 *
 * Important Notes:
 *  - Compatibility with other FMOD versions or platforms is not guaranteed.
 *  - For troubleshooting, refer to the FMOD Engine v2.03.06 documentation.
 *  - Using different FMOD versions or development environments may lead to unexpected behavior.
 *  - Download FMOD Engine v2.03.06 from the FMOD website archive (if available) or your development sources.
 */

#include <iostream> // For standard input/output streams like std::cout, std::cerr, std::cin
#include <fstream>  // For file input/output operations, used for reading and writing files
#include <string>   // For using string objects to handle text data
#include <vector>   // For using dynamic arrays (vectors) to store collections of data
#include <cstring>  // For C-style string manipulation functions, like strlen
#include <cstdlib>  // For general utilities, including memory allocation, random numbers, and environment control
#include <filesystem> // For working with file paths and directories in a platform-independent way (C++17 and later)
#include <algorithm> // For standard algorithms like std::min, std::transform, etc.
#include <cmath>    // For mathematical functions, though not heavily used in this specific code snippet
#include <memory>   // For smart pointers like std::unique_ptr, std::shared_ptr (not directly used in this snippet but good practice)
#include <unordered_map> // For hash-based associative containers, used for character sanitization
#include <locale>   // For locale-specific information, used for UTF-8 support
#include <codecvt>  // For code conversion facets, used for UTF-8 support (deprecated in C++17, alternatives exist)
#include <chrono>   // For time-related functionalities, used for timestamping log messages
#include <sstream>  // For string stream operations, used for formatting log timestamps
#include <iomanip>  // For input/output manipulators, used for formatting log timestamps

#ifdef _WIN32
#include <windows.h> // For Windows-specific API, used here to set console output code page to UTF-8
#endif

#include <fmod.hpp>       // Main header for the FMOD Engine API
#include <fmod_errors.h>  // Header for FMOD error codes and error string conversion

namespace Constants {
    constexpr const char* RIFF_HEADER = "RIFF"; // RIFF header identifier for WAV files
    constexpr const char* WAVE_FORMAT = "WAVE"; // WAVE format identifier for WAV files
    constexpr const char* FMT_CHUNK = "fmt ";   // Format chunk identifier in WAV files
    constexpr const char* DATA_CHUNK = "data";  // Data chunk identifier in WAV files
    constexpr uint16_t FORMAT_PCM = 1;         // PCM format code for WAV header
    constexpr uint16_t FORMAT_PCM_FLOAT = 3;   // PCM float format code for WAV header
    constexpr int BITS_IN_BYTE = 8;            // Number of bits in a byte
    constexpr unsigned int CHUNK_SIZE = 4096;   // Default chunk size for reading audio data from FSB files (in bytes)
    constexpr float MAX_SAMPLE_VALUE = 32767.0f; // Maximum sample value for 16-bit PCM (not directly used in core logic, might be for future scaling or normalization)
}

void Usage_Simple(); // Function declaration for displaying simple usage instructions in the console
void Usage_Detail(); // Function declaration for displaying detailed usage instructions in the console
void CheckFMODResult(FMOD_RESULT result, const std::string& message); // Function declaration to check FMOD API call results and throw exceptions on errors

/**
 * @class FMODSystem
 * @brief RAII wrapper for FMOD System object, managing initialization and release.
 *
 * @details
 * This class encapsulates the FMOD System object, ensuring proper initialization when an instance is created
 * and automatic release and close of the system when the instance goes out of scope.
 * It handles FMOD system creation, version checking, and initialization.
 */
class FMODSystem {
public:
    /**
     * @brief Constructor for FMODSystem.
     *
     * @details
     * Initializes the FMOD system and checks for version compatibility.
     * Throws std::runtime_error if FMOD system creation or initialization fails, or if version mismatch is detected.
     */
    FMODSystem() : system_(nullptr) {
        FMOD_RESULT result = FMOD::System_Create(&system_); // Creates the main FMOD system object
        CheckFMODResult(result, "FMOD::System_Create failed"); // Checks if system creation was successful

        unsigned int version;
        result = system_->getVersion(&version); // Gets the version of the FMOD library being used
        CheckFMODResult(result, "FMOD::System::getVersion failed"); // Checks if getting version was successful

        if (version < FMOD_VERSION) { // Compares the library version with the header version (FMOD_VERSION macro)
            std::cerr << " FMOD lib version " << std::hex << version << std::dec
                << " is less than header version " << FMOD_VERSION << std::endl;
            throw std::runtime_error("FMOD version mismatch"); // Throws an exception if library version is older than header version
        }

        result = system_->init(32, FMOD_INIT_NORMAL, nullptr); // Initializes the FMOD system with 32 channels and default settings
        CheckFMODResult(result, "FMOD::System::init failed"); // Checks if initialization was successful
    }

    /**
     * @brief Destructor for FMODSystem.
     *
     * @details
     * Closes and releases the FMOD system object to free resources.
     * Error messages are printed to std::cerr if closing or releasing fails, but no exception is thrown in destructor.
     */
    ~FMODSystem() {
        if (system_) {
            FMOD_RESULT result = system_->close(); // Closes the FMOD system
            if (result != FMOD_OK) {
                std::cerr << " FMOD::System::close failed: " << FMOD_ErrorString(result) << std::endl; // Prints error message if closing fails
            }
            result = system_->release(); // Releases the FMOD system object, freeing allocated memory
            if (result != FMOD_OK) {
                std::cerr << " FMOD::System::release failed: " << FMOD_ErrorString(result) << std::endl; // Prints error message if releasing fails
            }
        }
    }

    /**
     * @brief Returns the raw FMOD System pointer.
     *
     * @return FMOD::System* Pointer to the FMOD System object.
     */
    FMOD::System* get() const { return system_; } // Getter to access the FMOD System pointer
private:
    FMOD::System* system_; // Private member to store the FMOD System object pointer
};

/**
 * @class FMODSound
 * @brief RAII wrapper for FMOD Sound object, managing sound loading and release.
 *
 * @details
 * This class encapsulates the FMOD Sound object, ensuring proper sound creation when an instance is created
 * and automatic release of the sound when the instance goes out of scope.
 * It handles loading a sound from a file path using FMOD.
 */
class FMODSound {
public:
    /**
     * @brief Constructor for FMODSound.
     *
     * @param system Pointer to the initialized FMOD System object.
     * @param filePath Path to the audio file to be loaded.
     *
     * @details
     * Creates an FMOD Sound object from the specified file path using the provided FMOD System.
     * Throws std::runtime_error if sound creation fails.
     */
    FMODSound(FMOD::System* system, const std::string& filePath) : sound_(nullptr) {
        FMOD_RESULT result = system->createSound(filePath.c_str(), FMOD_CREATESTREAM, nullptr, &sound_); // Creates an FMOD sound object from the given file path, using stream mode
        CheckFMODResult(result, "FMOD::System::createSound failed for " + filePath); // Checks if sound creation was successful
    }

    /**
     * @brief Destructor for FMODSound.
     *
     * @details
     * Releases the FMOD Sound object to free resources.
     * Error messages are printed to std::cerr if releasing fails, but no exception is thrown in destructor.
     */
    ~FMODSound() {
        if (sound_) {
            FMOD_RESULT result = sound_->release(); // Releases the FMOD sound object, freeing allocated memory
            if (result != FMOD_OK) {
                std::cerr << " FMOD::Sound::release failed: " << FMOD_ErrorString(result) << std::endl; // Prints error message if releasing fails
            }
        }
    }

    /**
     * @brief Returns the raw FMOD Sound pointer.
     *
     * @return FMOD::Sound* Pointer to the FMOD Sound object.
     */
    FMOD::Sound* get() const { return sound_; } // Getter to access the FMOD Sound pointer
private:
    FMOD::Sound* sound_; // Private member to store the FMOD Sound object pointer
};

std::string SanitizeFileName(const std::string& fileName); // Function declaration to sanitize file names by replacing invalid characters
bool WriteWAVHeader(std::ofstream& file, int sampleRate, int channels, size_t dataSize, int bitsPerSample, FMOD_SOUND_FORMAT format); // Function declaration to write WAV file header
void WriteLogMessage(std::ofstream& logFile, const std::string& level, const std::string& functionName, const std::string& message, bool verboseLogEnabled, FMOD_RESULT errorCode); // Function declaration to write log messages

namespace AudioProcessor {
    template <typename BufferType>
    bool WriteAudioDataChunk(FMOD::Sound* subSound, std::ofstream& wavFile, size_t soundLengthBytes, int subSoundIndex, int& chunkCount, bool verboseLogEnabled, std::ofstream& logFile); // Template function declaration to write audio data chunks for various PCM formats
    bool WritePCM24DataChunk(FMOD::Sound* subSound, std::ofstream& wavFile, size_t soundLengthBytes, int subSoundIndex, int& chunkCount, bool verboseLogEnabled, std::ofstream& logFile); // Function declaration to handle writing 24-bit PCM data chunks (special case handling might be needed)
    bool WritePCMFloatDataChunk(FMOD::Sound* subSound, std::ofstream& wavFile, size_t soundLengthBytes, int subSoundIndex, int& chunkCount, bool verboseLogEnabled, std::ofstream& logFile); // Function declaration to handle writing PCM float data chunks
}

/**
 * @struct SoundInfo
 * @brief Structure to hold information about a sound extracted from FSB.
 *
 * @details
 * This structure is used to store various properties of a sub-sound, such as format, sample rate, channels, length, and name.
 * It is populated by the GetSoundInfo function and used when writing the WAV file header and processing audio data.
 */
struct SoundInfo {
    FMOD_SOUND_FORMAT format;     // FMOD sound format (e.g., PCM8, PCM16, PCMFLOAT)
    FMOD_SOUND_TYPE soundType;      // FMOD sound type (e.g., WAV, OGGVORBIS, FSB)
    int sampleRate = 0;          // Sample rate of the sound in Hz
    int bitsPerSample = 0;       // Bits per sample for the sound
    int channels = 0;            // Number of channels (1 for mono, 2 for stereo, etc.)
    unsigned int soundLengthBytes = 0; // Length of the sound data in bytes
    unsigned int lengthMs = 0;       // Length of the sound in milliseconds
    char subSoundName[256] = { 0 };  // Name of the sub-sound (if available, null-terminated C-style string)
};

SoundInfo GetSoundInfo(FMOD::Sound* subSound, int subSoundIndex, bool verboseLogEnabled, std::ofstream& logFile); // Function declaration to retrieve sound information from an FMOD Sound object
void ProcessSubSound(FMOD::System* fmodSystem, FMOD::Sound* subSound, int subSoundIndex, int totalSubSounds, const std::string& baseFileName, const std::filesystem::path& outputDirectoryPath, bool verboseLogEnabled, std::ofstream& logFile); // Function declaration to process a single sub-sound and save it as a WAV file


namespace BANKtoFSBExtractor {

    /**
     * @brief Searches for the "FSB5" signature within a BinaryReader stream (ported from C#).
     *
     * @param reader The std::ifstream to search within.
     * @return bool True if the "FSB5" signature is found, false otherwise.
     */
    bool FindFSB5Signature(std::ifstream& reader) {
        long long startPosition = reader.tellg(); // Store the initial position of the stream

        while (reader.peek() != EOF) {
            char signature[4];
            reader.read(signature, 4);
            if (reader.gcount() == 4) { // Ensure 4 bytes were read
                if (std::string(signature, 4) == "FSB5") {
                    reader.seekg(-4, std::ios::cur); // Rewind by 4 bytes
                    return true;
                }
            }
            else {
                break; // Reached end of file prematurely
            }
            reader.seekg(-3, std::ios::cur); // Move back 3 bytes to check next sequence
        }
        reader.seekg(startPosition); // Reset stream position if not found
        return false;
    }


    /**
     * @brief Extracts FSB files embedded within a BANK file (ported from C#).
     *
     * @param bankFilePath Path to the BANK file to extract FSBs from.
     * @return std::vector<std::filesystem::path> List of temporary file paths for the extracted FSB files.
     *         Returns an empty list if no FSB files are found or if an error occurs.
     */
    std::vector<std::filesystem::path> ExtractFSBsFromBankFile(const std::filesystem::path& bankFilePath) {
        std::vector<std::filesystem::path> tempFsbFiles;
        std::string baseBankFileName = bankFilePath.stem().string();
        std::filesystem::path tempPath = std::filesystem::temp_directory_path();

        try {
            std::ifstream bankFileStream(bankFilePath, std::ios::binary);
            if (!bankFileStream.is_open()) {
                std::cerr << "Error opening bank file: " << bankFilePath.u8string() << std::endl;
                return tempFsbFiles; // Return empty vector on error
            }

            int fsbCount = 0;
            while (FindFSB5Signature(bankFileStream)) {
                fsbCount++;
                std::string tempFileName;
                if (fsbCount > 1) {
                    tempFileName = baseBankFileName + "_" + std::to_string(fsbCount) + ".fsb";
                }
                else {
                    tempFileName = baseBankFileName + ".fsb";
                }
                std::filesystem::path tempFilePath = tempPath / tempFileName;
                tempFsbFiles.push_back(tempFilePath);

                try {
                    std::ofstream tempFsbStream(tempFilePath, std::ios::binary | std::ios::trunc);
                    if (!tempFsbStream.is_open()) {
                        std::cerr << "Error creating temporary *.fsb file: " << tempFilePath.u8string() << std::endl;
                        tempFsbFiles.pop_back(); // Remove from list
                        continue;
                    }

                    // Read FSB header information (structure based on QuickBMS script analysis)
                    char fsbSign[4];
                    uint32_t version, numSamples, shdrSize, nameSize, dataSize;

                    bankFileStream.read(fsbSign, 4);
                    bankFileStream.read(reinterpret_cast<char*>(&version), sizeof(version));
                    bankFileStream.read(reinterpret_cast<char*>(&numSamples), sizeof(numSamples));
                    bankFileStream.read(reinterpret_cast<char*>(&shdrSize), sizeof(shdrSize));
                    bankFileStream.read(reinterpret_cast<char*>(&nameSize), sizeof(nameSize));
                    bankFileStream.read(reinterpret_cast<char*>(&dataSize), sizeof(dataSize));

                    uint32_t fsbFileSize = 0x3C + shdrSize + nameSize + dataSize;

                    bankFileStream.seekg(-24, std::ios::cur); // Seek back to the beginning of FSB header
                    std::vector<char> fsbData(fsbFileSize);
                    bankFileStream.read(fsbData.data(), fsbFileSize);
                    tempFsbStream.write(fsbData.data(), fsbFileSize);


                }
                catch (const std::exception& ex) {
                    std::cerr << "Error creating temporary *.fsb file: " << tempFilePath.u8string() << " - " << ex.what() << std::endl;
                    tempFsbFiles.pop_back(); // Remove from list
                    std::filesystem::remove(tempFilePath); // Delete the temporary file if it exists
                    continue;
                }
                // No explicit offset update needed, FindFSB5Signature handles stream position
            }
        }
        catch (const std::exception& ex) {
            std::cerr << "*.bank file processing error: " << bankFilePath.u8string() << " - " << ex.what() << std::endl;
        }
        return tempFsbFiles;
    }
}


/**
 * @brief Main entry point of the FSB Extractor program.
 *
 * @param argc The number of command-line arguments.
 * @param argv An array of command-line argument strings.
 * @return int Returns 0 on successful execution, or 1 on error.
 *
 * @details
 * This function parses command-line arguments, initializes the FMOD system,
 * processes input FSB or BANK files, extracts sub-sounds, and saves them as WAV files.
 * It handles various options for output directory selection and verbose logging.
 * Error handling and resource management are also performed within this function.
 */
int main(int argc, const char** argv) {
#ifdef _WIN32
    SetConsoleOutputCP(CP_UTF8); // Sets the console output code page to UTF-8 on Windows for Unicode characters in console output.
#endif
    std::locale::global(std::locale(".UTF-8")); // Sets the global locale to UTF-8 to ensure UTF-8 character handling throughout the program.
    std::cout.imbue(std::locale()); // Imbues the standard output stream (std::cout) with the global locale for UTF-8 output.

    if (argc < 2) { // Checks if the number of command-line arguments is less than 2
        Usage_Simple(); // Display simple usage instructions if no input file path is provided
        return 1;       // Return 1 to indicate an error (incorrect usage - missing input file)
    }

    // Improved argument processing starts
    if (argc == 2) { // If there are exactly two command-line arguments (program name and one argument)
        std::string arg = argv[1]; // Get the first argument (after the program name)
        if (arg == "-h" || arg == "-help") { // Check if the argument is "-h" or "-help" (help option)
            Usage_Detail(); // Display detailed usage instructions if help option is used alone
            return 0;       // Return 0 to indicate successful execution (help information displayed)
        }
    }
    if (argc > 2) { // If there are more than two command-line arguments
        for (int i = 1; i < argc; ++i) { // Iterate through the arguments, starting from the first argument (index 1)
            std::string arg = argv[i]; // Get the current argument
            if (arg == "-h" || arg == "-help") { // Check if any argument is "-h" or "-help"
                std::cerr << " Error: Help option (-h, -help) must be used alone, e.g., `program -h` or `program -help`." << std::endl; // Display error message if help option is used with other options
                Usage_Simple(); // Display simple usage instructions
                return 1;       // Return 1 to indicate an error (incorrect usage - help option with other options)
            }
        }
    }

    std::filesystem::path inputFilePath;      // Variable to store the path to the input FSB/BANK file
    std::filesystem::path outputDirectoryPath; // Variable to store the path to the output directory for WAV files
    std::string filename_start;               // (Not used in current code) Intended for filename manipulation, but currently unused.
    std::string filename_end;                 // (Not used in current code) Intended for filename manipulation, but currently unused.
    int option_count = 0;                     // Counter to track the number of output directory options used (should be at most one)
    bool help_option_used = false;            // Flag to indicate if the help option (-h or -help) was used
    bool verboseLogEnabled = false;           // Flag to enable or disable verbose logging
    std::ofstream logFile;                    // Output file stream for writing log messages to a file (if verbose logging is enabled)
    std::vector<std::filesystem::path> tempFilesToDelete; // Vector to store paths of temp files to delete.

    try { // Begin of try block to catch exceptions that might occur during program execution
        FMODSystem fmodSystem; // Create an instance of FMODSystem class, which initializes the FMOD engine

        inputFilePath = std::filesystem::u8path(argv[1]); // Get the input file path from the first command-line argument (argv[1]) and convert it to a filesystem path, handling UTF-8 encoding
        if (!std::filesystem::exists(inputFilePath)) { // Check if the input file specified by inputFilePath exists
            std::cerr << "Error: File not found: " << inputFilePath.u8string() << std::endl; // Display error message if the input file does not exist
            Usage_Simple(); // Display simple usage instructions
            return 1;       // Return 1 to indicate an error (input file not found)
        }
        outputDirectoryPath = inputFilePath.parent_path(); // Set the default output directory path to be the same directory as the input FSB/BANK file

        for (int i = 2; i < argc; ++i) { // Loop through the command-line arguments starting from the second argument (index 2)
            std::string arg = argv[i]; // Get the current argument
            if (arg == "-res") { // Check if the argument is "-res" (output to resource directory option)
                outputDirectoryPath = inputFilePath.parent_path(); // Set the output directory to the parent directory of the input FSB/BANK file
                option_count++; // Increment the output directory option counter
            }
            else if (arg == "-exe") { // Check if the argument is "-exe" (output to executable directory option)
                outputDirectoryPath = std::filesystem::current_path(); // Set the output directory to the current working directory (where the executable is located)
                option_count++; // Increment the output directory option counter
            }
            else if (arg == "-o") { // Check if the argument is "-o" (output to user-specified directory option)
                if (i + 1 < argc) { // Check if there is another argument following "-o" (which should be the output directory path)
                    outputDirectoryPath = std::filesystem::u8path(argv[++i]); // Get the next argument as the output directory path and convert it to a filesystem path, handling UTF-8 encoding. Increment 'i' to move to the next argument in the next iteration (skipping the directory path argument in the loop).
                    option_count++; // Increment the output directory option counter
                }
                else { // If "-o" is used but no output directory path is provided
                    std::cerr << " Error: -o option requires an output directory path." << std::endl; // Display error message
                    return 1;       // Return 1 to indicate an error (missing output directory for -o option)
                }
            }
            else if (arg == "-v") { // Check if the argument is "-v" (verbose logging option)
                verboseLogEnabled = true; // Enable verbose logging
            }
            else if (arg == "-h" || arg == "-help") { // Check if the argument is "-h" or "-help" (help option)
                help_option_used = true; // Set the help option used flag to true
            }
            else { // If an unrecognized command-line argument is encountered
                std::cerr << " Error: Invalid option: " << arg << std::endl; // Display error message for invalid option
                Usage_Simple(); // Display simple usage instructions
                return 1;       // Return 1 to indicate an error (invalid command-line option)
            }
        }

        if (option_count > 1 && !help_option_used) { // Check if more than one output directory option was used and help option was not used
            std::cerr << " Error: Only one output directory option (-res, -exe, -o <output_directory>) can be used." << std::endl; // Display error message for using multiple output directory options
            Usage_Simple(); // Display simple usage instructions
            return 1;       // Return 1 to indicate an error (multiple output directory options used)
        }

        if (help_option_used) { // If the help option was used
            if (option_count > 0) { // Check if help option was used along with output directory options
                std::cerr << " Error: Cannot use help option (-h, -help) with output directory options (-res, -exe, -o)." << std::endl; // Display error message
                Usage_Simple(); // Display simple usage instructions
                return 1;       // Return 1 to indicate an error (help option used with other options)
            }
            else { // If only help option was used (and no output directory options)
                Usage_Detail(); // Display detailed usage instructions
                return 0;       // Return 0 to indicate successful execution (help information displayed)
            }
        }

        std::vector<std::filesystem::path> filesToProcess; // Vector to store paths of files to be processed (FSB or extracted FSBs from BANK)
        std::string inputFilePathLower = inputFilePath.string(); // Convert input file path to lowercase string for extension check
        std::transform(inputFilePathLower.begin(), inputFilePathLower.end(), inputFilePathLower.begin(), static_cast<int(*)(int)>(&std::tolower)); // Use std::tolower with explicit cast

        if (inputFilePathLower.length() >= 5 && inputFilePathLower.substr(inputFilePathLower.length() - 5) == ".bank") { // If the input file is a BANK file
            filesToProcess = BANKtoFSBExtractor::ExtractFSBsFromBankFile(inputFilePath); // Extract embedded FSB files from the BANK file
            if (filesToProcess.empty()) { // If no FSB files were found inside the BANK file
                std::cout << "No FSB files found inside bank file: " << inputFilePath.u8string() << std::endl; // Output message to console
                return 0; // Exit gracefully if no FSBs are found in the bank.
            }
            // Add extracted temp files to the deletion list.
            tempFilesToDelete.insert(tempFilesToDelete.end(), filesToProcess.begin(), filesToProcess.end());

        }
        else { // If the input file is an FSB file
            filesToProcess.push_back(inputFilePath); // Add the input FSB file path to the processing list
        }


        for (const auto& currentInputFilePath : filesToProcess) { // Loop through each file to process (could be original FSB or extracted FSB from BANK)
            FMODSound soundWrapper(fmodSystem.get(), currentInputFilePath.string()); // Create FMODSound object to load the FSB file, using RAII for resource management
            FMOD::Sound* sound = soundWrapper.get(); // Get the raw FMOD::Sound pointer from the wrapper

            int numSubSounds = 0;
            CheckFMODResult(sound->getNumSubSounds(&numSubSounds), "FMOD::Sound::getNumSubSounds failed"); // Get the number of sub-sounds within the loaded FSB file

            bool allSubSoundsProcessed = true; // Flag to track if all sub-sounds of the CURRENT FSB were processed successfully.

            if (numSubSounds > 0) { // If the FSB file contains one or more sub-sounds
                std::cout << std::endl << " ===== '" << currentInputFilePath.filename().u8string() << "' Processing Start =====" << std::endl << std::endl; // Display processing start message in console

                std::filesystem::path outputDirectory; // Filesystem path for the output directory
                std::filesystem::path logFilePath;     // Filesystem path for the log file

                outputDirectory = outputDirectoryPath / currentInputFilePath.stem(); // Output directory is base output path + FSB filename (without extension)
                std::error_code ec; // Error code for directory creation
                if (!std::filesystem::create_directory(outputDirectory, ec) && ec) { // Attempt to create the output directory, check for errors other than directory already existing
                    std::cerr << "Error creating directory: " << outputDirectory << " - " << ec.message() << std::endl; // Display error message if directory creation fails
                    outputDirectory = outputDirectoryPath; // Fallback to the base output directory if creation fails
                }
                else if (ec) { /* directory already exists or other non-error */ } // If directory exists already or non-critical error, ignore
                else { std::cout << " Created directory: " << std::filesystem::absolute(outputDirectory).u8string() << std::endl; } // Display directory creation success message

                if (verboseLogEnabled) { // If verbose logging is enabled
                    logFilePath = outputDirectory / ("_" + currentInputFilePath.stem().string() + ".log"); // Log file path is output directory + _FSBfilename.log
                    std::cout << " Log file path: " << std::filesystem::absolute(logFilePath).u8string() << std::endl; // Display log file path in console
                    logFile.open(logFilePath, std::ios::trunc); // Open log file in truncate mode (overwrite existing)
                    if (!logFile.is_open()) { // Check if log file opening failed
                        std::cerr << "Error creating log file: " << logFilePath << std::endl; // Display error message if log file creation fails
                        verboseLogEnabled = false; // Disable verbose logging if log file can't be opened
                    }
                    else { // If log file opened successfully
                        WriteLogMessage(logFile, "INFO", __FUNCTION__, "Log file opened: " + std::filesystem::absolute(logFilePath).u8string(), verboseLogEnabled, FMOD_OK); // Log message for log file opened
                        WriteLogMessage(logFile, "INFO", __FUNCTION__, "Processing FSB file: " + std::filesystem::absolute(currentInputFilePath).u8string(), verboseLogEnabled, FMOD_OK); // Log message for FSB processing started
                    }
                }

                for (int i = 0; i < numSubSounds; ++i) { // Loop through each sub-sound in the FSB file
                    FMOD::Sound* subSound = nullptr; // Pointer to hold the sub-sound object
                    FMOD_RESULT result = sound->getSubSound(i, &subSound); // Get the i-th sub-sound from the FSB file
                    if (result != FMOD_OK) { // Check if getting sub-sound failed
                        std::cerr << " FMOD::Sound::getSubSound failed for sub-sound " << i << ": " << FMOD_ErrorString(result) << std::endl; // Display error message if getting sub-sound fails
                        allSubSoundsProcessed = false; // FAIL: Set flag, as a subsound failed.
                        continue; // Skip to the next sub-sound if this one failed
                    }
                    try {
                        ProcessSubSound(fmodSystem.get(), subSound, i, numSubSounds, currentInputFilePath.stem().string(), outputDirectory, verboseLogEnabled, std::ref(logFile)); // Process the sub-sound (extract to WAV)
                    }
                    catch (const std::exception& ex) {
                        std::cerr << " Exception caught while processing sub-sound " << i << ": " << ex.what() << std::endl;
                        allSubSoundsProcessed = false; // FAIL: Set flag on exception.
                    }
                    subSound->release(); // Release the sub-sound object after processing
                }
            }
            else { // If no sub-sounds are found in the FSB file
                std::cout << " No sub-sounds found in the audio file." << std::endl; // Display message if no sub-sounds found
            }
        } // End of filesToProcess loop.

        // All FSBs (or the single FSB) have been processed at this point.

    }
    catch (const std::exception& e) { // Catch any standard exceptions during program execution
        std::cerr << "\n\n\n";
        std::cerr << " ===== ERROR ===== " << std::endl; // Display error section header in console
        std::cerr << " Exception caught: " << e.what() << std::endl; // Display the exception message
        std::cerr << "\n";
        std::cerr << " Press any key to continue..." << std::endl; // Prompt user to press any key to continue after error
        std::cin.get(); // Wait for user input (press any key)
        return 1;       // Return 1 to indicate program termination due to an error
    }

    if (logFile.is_open()) { // If the log file is open (verbose logging was enabled)
        logFile << std::endl; // Add a newline at the end of the log file for better formatting
        WriteLogMessage(logFile, "INFO", __FUNCTION__, "Processing finished for Input file: " + inputFilePath.filename().u8string(), verboseLogEnabled, FMOD_OK); // Write log message indicating processing finished
        logFile.close(); // Close the log file
    }
    std::cout << std::endl << " ===== '" << inputFilePath.filename().u8string() << "' Processing End =====" << std::endl << std::endl; // Display program processing end message in console


    // Delete temporary files after printing the final message.
    for (const auto& tempFile : tempFilesToDelete) {
        if (std::filesystem::exists(tempFile)) { // Double-check existence.
            try {
                std::filesystem::remove(tempFile);
            }
            catch (const std::exception& ex) {
                std::cerr << std::endl << " Error deleting temporary FSB file: " << tempFile.u8string() << " - " << ex.what() << std::endl;
            }
        }
    }

    return 0; // Return 0 to indicate successful program execution
}


/**
 * @brief Displays simple usage instructions to the console.
 */
void Usage_Simple() {
    std::cerr << "\n\n\n";
    std::cerr << " ===== Welcome to FSB/BANK Extractor =====" << std::endl;
    std::cerr << " This program extracts sounds from *.fsb and *.bank files and saves them as *.wav files." << std::endl;
    std::cerr << "\n\n";
    std::cerr << " Usage: program <audio_file_path> [Options]" << std::endl;
    std::cerr << "        (* If you omit the option, the '-res' option is applied by default.)" << std::endl;
    std::cerr << "        (** For detailed usage instructions, please refer to `program -h` or `program -help`.)" << std::endl;
    std::cerr << "\n\n";
    std::cerr << "   <audio_file_path> : Path to the *.fsb or *.bank file" << std::endl;
    std::cerr << "\n";
    std::cerr << "   [Options]         : -res                  : Save wav files in the same folder as fsb/bank file (default)" << std::endl;
    std::cerr << "                       -exe                  : Save wav files in the same folder as program file" << std::endl;
    std::cerr << "                       -o <output_directory> : Save wav files in the user-specified folder" << std::endl;
    std::cerr << "                       -v                    : Enable verbose logging for chunk processing verification" << std::endl;
}

/**
 * @brief Displays detailed usage instructions to the console.
 */
void Usage_Detail() {
    std::cerr << "\n\n\n";
    std::cerr << " ===== Welcome to FSB/BANK Extractor =====" << std::endl;
    std::cerr << " This program extracts sounds from *.fsb and *.bank files and saves them as *.wav files." << std::endl;
    std::cerr << "\n\n";
    std::cerr << " Usage: program <audio_file_path> [Options]" << std::endl;
    std::cerr << "        (* If you omit the option, the '-res' option is applied by default.)" << std::endl;
    std::cerr << "\n";
    std::cerr << " <audio_file_path> : This is the required path to the *.fsb or *.bank file." << std::endl;
    std::cerr << "                     (* Example: \"C:\\sounds\\music.fsb\" or \"audio.bank\")" << std::endl;
    std::cerr << "\n\n";
    std::cerr << " [Options] : These are optional settings. You can choose one of the following options to specify the output folder." << std::endl;
    std::cerr << "\n";
    std::cerr << "   -res    : Save *.wav files in the same folder as the *.fsb or *.bank file. (Default option)" << std::endl;
    std::cerr << "\n";
    std::cerr << "             If the *.fsb/bank file path is 'C:\\sounds\\music.fsb' or 'C:\\sounds\\audio.bank'," << std::endl;
    std::cerr << "               output files will be saved in the 'C:\\sounds' folder." << std::endl;
    std::cerr << "\n";
    std::cerr << "             This is generally useful when you want to manage output files together within the resource folder." << std::endl;
    std::cerr << "\n\n";
    std::cerr << "   -exe    : Save *.wav files in the folder where the program file is located." << std::endl;
    std::cerr << "\n";
    std::cerr << "             If the program is 'D:\\tools\\audio_extractor.exe' and you use the '-exe' option," << std::endl;
    std::cerr << "               output files will be saved in the 'D:\\tools' folder." << std::endl;
    std::cerr << "\n";
    std::cerr << "             This is useful when you want to manage output files in the same location as the executable." << std::endl;
    std::cerr << "\n\n";
    std::cerr << "   -o <output_directory>" << std::endl;
    std::cerr << "           : Save *.wav files in the user-specified directory." << std::endl;
    std::cerr << "\n";
    std::cerr << "             You need to enter the path of the folder where you want to save the *.wav files" << std::endl;
    std::cerr << "               in the <output_directory> place. (* Example: -o \"C:\\output\" or -o \"output_wav\")" << std::endl;
    std::cerr << "\n";
    std::cerr << "             The path can be an absolute path or a relative path based on the current execution location." << std::endl;
    std::cerr << "\n\n";
    std::cerr << "   -v      : Enable verbose logging to verify chunk processing." << std::endl;
    std::cerr << "\n";
    std::cerr << "             When this option is enabled, detailed information about each audio chunk" << std::endl;
    std::cerr << "               read from the FSB file will be logged to a file (*.log)." << std::endl;
    std::cerr << "\n";
    std::cerr << "             This is helpful for developers to verify if the audio data is being read and processed correctly." << std::endl;
    std::cerr << "\n\n";
    std::cerr << " Usage Examples:" << std::endl;
    std::cerr << "   program audio.fsb                           (Default option: same as -res)" << std::endl;
    std::cerr << "   program music.bank -res                     (Save in the same folder as the *.fsb file)" << std::endl;
    std::cerr << "   program sounds.fsb -exe                     (Save in the same folder as the executable file)" << std::endl;
    std::cerr << "   program voices.bank -o \"C:\\output\\audio\"    (Save in the absolute path folder)" << std::endl;
    std::cerr << "   program effects.fsb -o \"output_wav\"         (Save in the relative path folder)" << std::endl;
    std::cerr << "   program music.bank -v                       (Enable verbose logging)" << std::endl;
}

/**
 * @brief Checks the result of an FMOD API call and throws an exception if it's not FMOD_OK.
 *
 * @param result FMOD_RESULT returned by an FMOD API function.
 * @param message Error message to include in the exception if the result is not FMOD_OK.
 *
 * @throws std::runtime_error if the FMOD_RESULT is not FMOD_OK.
 */
void CheckFMODResult(FMOD_RESULT result, const std::string& message) {
    if (result != FMOD_OK) { // Checks if the FMOD result is not FMOD_OK (indicating an error)
        throw std::runtime_error(message + ": " + FMOD_ErrorString(result)); // Throws a runtime error exception with the provided message and FMOD error string
    }
}


/**
 * @brief Sanitizes a file name by replacing invalid characters with safe alternatives.
 *
 * @param fileName The original file name string.
 * @return std::string Sanitized file name string.
 *
 * @details
 * Replaces characters that are typically invalid or problematic in file names (like <, >, :, ", /, \, |, ?, *)
 * with similar-looking but safe Unicode characters. This helps to avoid file system errors when creating output files.
 */
std::string SanitizeFileName(const std::string& fileName) {
    static const std::unordered_map<char, std::string> charMap = { // Static map to store invalid characters and their replacements
        {'<', "¡´"}, {'>', "¡µ"}, {':', "£º"}, {'\"', "£¢"}, {'/', "£¯"},
        {'\\', "£Ü"}, {'|', "£ü"}, {'?', "£¿"}, {'*', "£ª"}
    };
    std::string sanitized = fileName; // Creates a copy of the input file name to sanitize
    for (size_t i = 0; i < sanitized.length(); ++i) { // Iterates through each character in the file name
        if (auto it = charMap.find(sanitized[i]); it != charMap.end()) { // Checks if the current character is in the invalid characters map
            sanitized.replace(i, 1, it->second); // Replaces the invalid character with its corresponding safe replacement from the map
            i += it->second.length() - 1; // Adjusts the loop index to account for the replacement string length (if longer than 1)
        }
    }
    return sanitized; // Returns the sanitized file name
}


/**
 * @brief Writes the WAV file header to the output file stream.
 *
 * @param file Output file stream to write the WAV header to.
 * @param sampleRate Sample rate of the audio in Hz.
 * @param channels Number of audio channels.
 * @param dataSize Size of the audio data in bytes.
 * @param bitsPerSample Bits per sample of the audio.
 * @param format FMOD_SOUND_FORMAT of the audio data.
 * @return bool True if header writing was successful, false otherwise.
 *
 * @details
 * Writes the standard WAV file header (RIFF, WAVE, fmt, data chunks) to the provided output file stream.
 * This header contains information about the audio format, sample rate, channels, and data size,
 * which is necessary for WAV files to be correctly recognized and played by audio players.
 */
bool WriteWAVHeader(std::ofstream& file, int sampleRate, int channels, size_t dataSize, int bitsPerSample, FMOD_SOUND_FORMAT format) {
    if (!file.is_open()) { // Checks if the output file stream is open
        std::cerr << " Error: Output file is not open." << std::endl; // Prints error message to std::cerr if file is not open
        return false; // Returns false to indicate failure
    }

    auto write_data = [&](const auto& data) { // Lambda function to write data to the file stream, handling byte conversion
        file.write(reinterpret_cast<const char*>(&data), sizeof(data)); // Writes raw data to the file stream, size determined by data type
        };

    try {
        file.write(Constants::RIFF_HEADER, 4); // Writes "RIFF" identifier (4 bytes)
        write_data(static_cast<uint32_t>(36 + dataSize)); // Writes chunk size (WAV header size + data size), 4 bytes
        file.write(Constants::WAVE_FORMAT, 4); // Writes "WAVE" identifier (4 bytes)
        file.write(Constants::FMT_CHUNK, 4); // Writes "fmt " chunk identifier (4 bytes)
        write_data(static_cast<uint32_t>(16)); // Writes format chunk size (always 16 for PCM), 4 bytes
        write_data(format == FMOD_SOUND_FORMAT_PCMFLOAT ? Constants::FORMAT_PCM_FLOAT : Constants::FORMAT_PCM); // Writes audio format code (PCM or PCM float), 2 bytes
        write_data(static_cast<uint16_t>(channels)); // Writes number of channels, 2 bytes
        write_data(static_cast<uint32_t>(sampleRate)); // Writes sample rate, 4 bytes
        write_data(static_cast<uint32_t>(static_cast<uint64_t>(sampleRate) * channels * bitsPerSample / Constants::BITS_IN_BYTE)); // Writes byte rate (bytes per second), 4 bytes
        write_data(static_cast<uint16_t>(channels * bitsPerSample / Constants::BITS_IN_BYTE)); // Writes block align (bytes per sample block), 2 bytes
        write_data(static_cast<uint16_t>(bitsPerSample)); // Writes bits per sample, 2 bytes
        file.write(Constants::DATA_CHUNK, 4); // Writes "data" chunk identifier (4 bytes)
        write_data(static_cast<uint32_t>(dataSize)); // Writes data chunk size (audio data size), 4 bytes

    }
    catch (const std::ios_base::failure& e) { // Catches file stream exceptions
        std::cerr << " Error writing WAV header: " << e.what() << std::endl; // Prints error message to std::cerr if exception occurred during header writing
        return false; // Returns false to indicate failure
    }
    catch (const std::exception& e) { // Catches other standard exceptions
        std::cerr << " Error writing WAV header: " << e.what() << std::endl; // Prints error message to std::cerr if exception occurred during header writing
        return false; // Returns false to indicate failure
    }
    return true; // Returns true to indicate successful header writing
}


/**
 * @brief Writes a log message to the log file if verbose logging is enabled.
 *
 * @param logFile Output file stream for the log file.
 * @param level Log level (e.g., "INFO", "WARNING", "ERROR").
 * @param functionName Name of the function where the log message originates.
 * @param message The log message string.
 * @param verboseLogEnabled Flag indicating if verbose logging is enabled.
 * @param errorCode FMOD_RESULT error code (optional, FMOD_OK if no error).
 *
 * @details
 * This function writes a formatted log message to the specified log file if verbose logging is enabled.
 * The log message includes a timestamp, log level, function name, and the message itself.
 * If an FMOD error code is provided (not FMOD_OK), it's also included in the log message.
 */
void WriteLogMessage(std::ofstream& logFile, const std::string& level, const std::string& functionName, const std::string& message, bool verboseLogEnabled, FMOD_RESULT errorCode) {
    if (logFile.is_open() && verboseLogEnabled) { // Checks if log file is open and verbose logging is enabled
        auto now = std::chrono::system_clock::now(); // Gets current system time

        std::time_t time_t_value = std::chrono::system_clock::to_time_t(now); // Converts system time to time_t (C-style time)

        std::tm time_components; // Struct to store time components (year, month, day, hour, minute, second)
        localtime_s(&time_components, &time_t_value); // Converts time_t to local time and fills time_components struct (thread-safe version for Windows)

        auto milliseconds = std::chrono::duration_cast<std::chrono::milliseconds>(now.time_since_epoch()) % 1000; // Extracts milliseconds part from current time

        std::stringstream timestampStream; // String stream to format the timestamp
        timestampStream << std::put_time(&time_components, "%Y-%m-%d %H:%M:%S"); // Formats date and time
        timestampStream << "." << std::setfill('0') << std::setw(3) << milliseconds.count(); // Formats milliseconds with leading zeros

        logFile << "[" << timestampStream.str() << "] "; // Writes timestamp to log file
        logFile << "[" << level << "] "; // Writes log level to log file
        logFile << "[" << functionName << "] "; // Writes function name to log file
        logFile << message; // Writes log message to log file
        if (errorCode != FMOD_OK) { // If an FMOD error code is provided (not FMOD_OK)
            logFile << " (Error code: " << errorCode << ")"; // Appends error code to the log message
        }
        logFile << std::endl; // Writes newline character to log file
    }
}


namespace AudioProcessor {
    /**
     * @brief Writes audio data chunks to the WAV file for PCM formats (template function).
     *
     * @tparam BufferType Data type of the audio buffer (e.g., unsigned char, short, int).
     * @param subSound FMOD Sound object representing the sub-sound.
     * @param wavFile Output file stream for the WAV file.
     * @param soundLengthBytes Total length of the sub-sound data in bytes.
     * @param subSoundIndex Index of the sub-sound being processed.
     * @param chunkCount Counter for chunks processed (for logging).
     * @param verboseLogEnabled Flag indicating if verbose logging is enabled.
     * @param logFile Output file stream for the log file.
     * @return bool True if writing data chunks was successful, false otherwise.
     *
     * @details
     * This template function reads audio data from the FMOD sub-sound in chunks and writes it to the WAV file.
     * It reads data in chunks of Constants::CHUNK_SIZE and iterates until all audio data is written.
     * It supports various PCM integer formats based on the BufferType template parameter.
     * PCM float format is handled by WritePCMFloatDataChunk function.
     */
    template <typename BufferType>
    bool WriteAudioDataChunk(FMOD::Sound* subSound, std::ofstream& wavFile, size_t soundLengthBytes, int subSoundIndex, int& chunkCount, bool verboseLogEnabled, std::ofstream& logFile) {
        // Calculate buffer size based on chunk size and data type
        std::vector<BufferType> buffer(Constants::CHUNK_SIZE / sizeof(BufferType));
        unsigned int totalBytesRead = 0; // Initialize total bytes read counter

        // Loop until all sound data is read
        while (totalBytesRead < soundLengthBytes) {
            // Determine how many bytes to read in this chunk, ensuring not to read beyond sound length or chunk size
            unsigned int bytesToRead = std::min<unsigned int>(Constants::CHUNK_SIZE, soundLengthBytes - totalBytesRead);

            ++chunkCount; // Increment chunk counter before processing current chunk
            unsigned int bytesRead = 0; // Initialize bytes read for current chunk
            // Read data from FMOD sub-sound into buffer
            FMOD_RESULT fmodSystemResult = subSound->readData(buffer.data(), bytesToRead, &bytesRead);
            if (fmodSystemResult != FMOD_OK) {
                WriteLogMessage(logFile, "INFO", __FUNCTION__, "Reading chunk " + std::to_string(chunkCount) + " - Bytes to read: " + std::to_string(bytesToRead), verboseLogEnabled, FMOD_OK);
                WriteLogMessage(logFile, "ERROR", __FUNCTION__, "FMOD::Sound::readData failed for sub-sound " + std::to_string(subSoundIndex) + ", chunk " + std::to_string(chunkCount) + ": " + FMOD_ErrorString(fmodSystemResult) + " (Result code: " + std::to_string(fmodSystemResult) + ")", verboseLogEnabled, fmodSystemResult);
                std::cerr << " FMOD::Sound::readData failed for sub-sound " << subSoundIndex << ": " << FMOD_ErrorString(fmodSystemResult) << std::endl;
                return false; // Return false to indicate failure
            }

            try {
                // Write the buffer data to the WAV file
                wavFile.write(reinterpret_cast<const char*>(buffer.data()), bytesRead);
            }
            catch (const std::ios_base::failure& e) {
                WriteLogMessage(logFile, "ERROR", __FUNCTION__, "Error writing WAV data for chunk " + std::to_string(chunkCount) + ": " + e.what(), verboseLogEnabled, FMOD_OK);
                std::cerr << " Error writing WAV data: " << e.what() << std::endl;
                return false; // Return false to indicate failure
            }
            totalBytesRead += bytesRead; // Update total bytes read counter
        }
        return true; // Return true to indicate success after writing all data chunks
    }


    /**
     * @brief Writes audio data chunks to the WAV file for 24-bit PCM format.
     *
     * @param subSound FMOD Sound object representing the sub-sound.
     * @param wavFile Output file stream for the WAV file.
     * @param soundLengthBytes Total length of the sub-sound data in bytes.
     * @param subSoundIndex Index of the sub-sound being processed.
     * @param chunkCount Counter for chunks processed (for logging).
     * @param verboseLogEnabled Flag indicating if verbose logging is enabled.
     * @param logFile Output file stream for the log file.
     * @return bool True if writing data chunks was successful, false otherwise.
     *
     * @details
     * This function specifically handles 24-bit PCM data.
     * FMOD reads 24-bit PCM data as packed 3-byte samples.
     * This function iterates through the read buffer and writes each 3-byte sample individually to maintain WAV compatibility.
     * WAV format expects 24-bit PCM as 3 bytes per sample in little-endian byte order.
     */
    bool WritePCM24DataChunk(FMOD::Sound* subSound, std::ofstream& wavFile, size_t soundLengthBytes, int subSoundIndex, int& chunkCount, bool verboseLogEnabled, std::ofstream& logFile) {
        std::vector<unsigned char> buffer(Constants::CHUNK_SIZE);
        unsigned int totalBytesRead = 0;

        while (totalBytesRead < soundLengthBytes) {
            unsigned int bytesToRead = std::min<unsigned int>(Constants::CHUNK_SIZE, soundLengthBytes - totalBytesRead);

            ++chunkCount; // Increment chunk counter before processing current chunk
            unsigned int bytesRead = 0; // Initialize bytes read for current chunk
            // Read data from FMOD sub-sound into buffer
            FMOD_RESULT fmodSystemResult = subSound->readData(buffer.data(), bytesToRead, &bytesRead);
            if (fmodSystemResult != FMOD_OK) {
                WriteLogMessage(logFile, "INFO", __FUNCTION__, "Reading chunk " + std::to_string(chunkCount) + " (PCM24) - Bytes to read: " + std::to_string(bytesToRead), verboseLogEnabled, FMOD_OK);
                WriteLogMessage(logFile, "ERROR", __FUNCTION__, "FMOD::Sound::readData failed for sub-sound " + std::to_string(subSoundIndex) + ", chunk " + std::to_string(chunkCount) + " (PCM24): " + FMOD_ErrorString(fmodSystemResult) + " (Result code: " + std::to_string(fmodSystemResult) + ")", verboseLogEnabled, fmodSystemResult);
                std::cerr << " FMOD::Sound::readData failed for sub-sound " << subSoundIndex << ": " << FMOD_ErrorString(fmodSystemResult) << std::endl;
                return false; // Return false to indicate failure
            }

            try {
                // Iterate through the buffer, writing 3 bytes at a time for each 24-bit sample
                for (unsigned int i = 0; i < bytesRead; i += 3) {
                    // Ensure there are enough bytes left to read a complete 24-bit sample
                    if (i + 2 < bytesRead) {
                        // Write each byte of the 24-bit sample individually
                        wavFile.write(reinterpret_cast<const char*>(&buffer[i]), 1);
                        wavFile.write(reinterpret_cast<const char*>(&buffer[i + 1]), 1);
                        wavFile.write(reinterpret_cast<const char*>(&buffer[i + 2]), 1);
                    }
                }
            }
            catch (const std::ios_base::failure& e) {
                WriteLogMessage(logFile, "ERROR", __FUNCTION__, "Error writing WAV data for chunk " + std::to_string(chunkCount) + " (PCM24): " + e.what(), verboseLogEnabled, FMOD_OK);
                std::cerr << " Error writing WAV data: " << e.what() << std::endl;
                return false; // Return false to indicate failure
            }
            totalBytesRead += bytesRead; // Update total bytes read counter
        }
        return true; // Return true to indicate success after writing all data chunks
    }


    /**
     * @brief Writes audio data chunks to the WAV file for PCM float format.
     *
     * @param subSound FMOD Sound object representing the sub-sound.
     * @param wavFile Output file stream for the WAV file.
     * @param soundLengthBytes Total length of the sub-sound data in bytes.
     * @param subSoundIndex Index of the sub-sound being processed.
     * @param chunkCount Counter for chunks processed (for logging).
     * @param verboseLogEnabled Flag indicating if verbose logging is enabled.
     * @param logFile Output file stream for the log file.
     * @return bool True if writing data chunks was successful, false otherwise.
     *
     * @details
     * This function handles writing PCM float audio data to the WAV file.
     * It first reads audio data as float samples from the FMOD sub-sound.
     * To prevent clipping, it then clamps these float samples to the range of -1.0f to 1.0f.
     * Finally, it writes the clamped float sample data to the WAV file in binary float format.
     * The WAV float format utilizes IEEE 754 single-precision floating-point numbers.
     */
    bool AudioProcessor::WritePCMFloatDataChunk(FMOD::Sound* subSound, std::ofstream& wavFile, size_t soundLengthBytes, int subSoundIndex, int& chunkCount, bool verboseLogEnabled, std::ofstream& logFile) {
        // Calculate buffer size for float data based on chunk size
        std::vector<float> floatBuffer(Constants::CHUNK_SIZE / sizeof(float));
        unsigned int totalBytesRead = 0;

        // Loop until all sound data is read
        while (totalBytesRead < soundLengthBytes) {
            // Determine how many bytes to read in this chunk, ensuring not to read beyond sound length or chunk size
            unsigned int bytesToRead = std::min<unsigned int>(Constants::CHUNK_SIZE, soundLengthBytes - totalBytesRead);

            ++chunkCount; // Increment chunk counter before processing current chunk
            unsigned int bytesRead = 0; // Initialize bytes read for current chunk
            // Read float data from FMOD sub-sound into float buffer
            FMOD_RESULT fmodSystemResult = subSound->readData(floatBuffer.data(), bytesToRead, &bytesRead);
            if (fmodSystemResult != FMOD_OK) {
                WriteLogMessage(logFile, "INFO", __FUNCTION__, "Reading chunk " + std::to_string(chunkCount) + " (PCMFLOAT) - Bytes to read: " + std::to_string(bytesToRead), verboseLogEnabled, FMOD_OK);
                WriteLogMessage(logFile, "ERROR", __FUNCTION__, "FMOD::Sound::readData failed for sub-sound " + std::to_string(subSoundIndex) + ", chunk " + std::to_string(chunkCount) + " (PCMFLOAT): " + FMOD_ErrorString(fmodSystemResult) + " (Result code: " + std::to_string(fmodSystemResult) + ")", verboseLogEnabled, fmodSystemResult);
                std::cerr << " FMOD::Sound::readData failed for sub-sound " << subSoundIndex << ": " << FMOD_ErrorString(fmodSystemResult) << std::endl;
                return false; // Return false to indicate failure
            }

            // **Clamping Implementation Start**
            for (int i = 0; i < bytesRead / sizeof(float); ++i) {
                floatBuffer[i] = std::clamp(floatBuffer[i], -1.0f, 1.0f);
            }
            // **Clamping Implementation End**


            try {
                // Write the float buffer data directly to the WAV file
                wavFile.write(reinterpret_cast<const char*>(floatBuffer.data()), bytesRead);
            }
            catch (const std::ios_base::failure& e) {
                WriteLogMessage(logFile, "ERROR", __FUNCTION__, "Error writing WAV data for chunk " + std::to_string(chunkCount) + " (PCMFLOAT): " + e.what(), verboseLogEnabled, FMOD_OK);
                std::cerr << " Error writing WAV data: " << e.what() << std::endl;
                return false; // Return false to indicate failure
            }
            totalBytesRead += bytesRead; // Update total bytes read counter
        }
        return true; // Return true to indicate success after writing all data chunks
    }
}


/**
 * @brief Retrieves detailed information about a sub-sound from an FMOD Sound object.
 *
 * @param subSound FMOD Sound object representing the sub-sound.
 * @param subSoundIndex Index of the sub-sound being processed.
 * @param verboseLogEnabled Flag indicating if verbose logging is enabled.
 * @param logFile Output file stream for the log file.
 * @return SoundInfo Structure containing information about the sub-sound.
 *
 * @details
 * This function retrieves various properties of the given FMOD sub-sound, such as format, sound type,
 * sample rate, bits per sample, number of channels, length in bytes and milliseconds, and sub-sound name.
 * It logs each step of information retrieval if verbose logging is enabled and throws an exception if critical FMOD API calls fail.
 */
SoundInfo GetSoundInfo(FMOD::Sound* subSound, int subSoundIndex, bool verboseLogEnabled, std::ofstream& logFile) {
    SoundInfo info; // Structure to store sound information
    FMOD_RESULT fmodSystemResult; // Variable to store FMOD API call results
    float defaultFrequency; // Variable to store default frequency
    int defaultPriority;    // Variable to store default priority

    WriteLogMessage(logFile, "INFO", __FUNCTION__, "Getting sound format...", verboseLogEnabled, FMOD_OK); // Logs attempt to get sound format
    fmodSystemResult = subSound->getFormat(&info.soundType, &info.format, &info.channels, &info.bitsPerSample); // Gets sound format information from FMOD Sound object
    if (fmodSystemResult != FMOD_OK) { // Checks if getting format failed
        WriteLogMessage(logFile, "ERROR", __FUNCTION__, "FMOD::Sound::getFormat failed for sub-sound " + std::to_string(subSoundIndex) + ": " + FMOD_ErrorString(fmodSystemResult), verboseLogEnabled, FMOD_OK); // Logs FMOD error (ERROR level)
        CheckFMODResult(fmodSystemResult, "FMOD::Sound::getFormat failed for sub-sound " + std::to_string(subSoundIndex)); // Throws exception on error
    }
    else {
        std::string formatStr, soundTypeStr; // Strings to store format and sound type names for logging
#define FORMAT_TO_STR(fmt) case fmt: formatStr = #fmt; break; // Macro to convert FMOD_SOUND_FORMAT enum to string
#define SOUND_TYPE_TO_STR(type) case type: soundTypeStr = #type; break; // Macro to convert FMOD_SOUND_TYPE enum to string

        switch (info.format) { // Switch statement to convert FMOD_SOUND_FORMAT to string
            FORMAT_TO_STR(FMOD_SOUND_FORMAT_PCM8)
                FORMAT_TO_STR(FMOD_SOUND_FORMAT_PCM16)
                FORMAT_TO_STR(FMOD_SOUND_FORMAT_PCM24)
                FORMAT_TO_STR(FMOD_SOUND_FORMAT_PCM32)
                FORMAT_TO_STR(FMOD_SOUND_FORMAT_PCMFLOAT)
                FORMAT_TO_STR(FMOD_SOUND_FORMAT_BITSTREAM)
                FORMAT_TO_STR(FMOD_SOUND_FORMAT_MAX)
        default: formatStr = "Unknown"; break;
        }
        switch (info.soundType) { // Switch statement to convert FMOD_SOUND_TYPE to string
            SOUND_TYPE_TO_STR(FMOD_SOUND_TYPE_UNKNOWN)
                SOUND_TYPE_TO_STR(FMOD_SOUND_TYPE_AIFF)
                SOUND_TYPE_TO_STR(FMOD_SOUND_TYPE_ASF)
                SOUND_TYPE_TO_STR(FMOD_SOUND_TYPE_FLAC)
                SOUND_TYPE_TO_STR(FMOD_SOUND_TYPE_FSB)
                SOUND_TYPE_TO_STR(FMOD_SOUND_TYPE_IT)
                SOUND_TYPE_TO_STR(FMOD_SOUND_TYPE_MIDI)
                SOUND_TYPE_TO_STR(FMOD_SOUND_TYPE_MOD)
                SOUND_TYPE_TO_STR(FMOD_SOUND_TYPE_MPEG)
                SOUND_TYPE_TO_STR(FMOD_SOUND_TYPE_OGGVORBIS)
                SOUND_TYPE_TO_STR(FMOD_SOUND_TYPE_PLAYLIST)
                SOUND_TYPE_TO_STR(FMOD_SOUND_TYPE_RAW)
                SOUND_TYPE_TO_STR(FMOD_SOUND_TYPE_S3M)
                SOUND_TYPE_TO_STR(FMOD_SOUND_TYPE_USER)
                SOUND_TYPE_TO_STR(FMOD_SOUND_TYPE_WAV)
                SOUND_TYPE_TO_STR(FMOD_SOUND_TYPE_XM)
                SOUND_TYPE_TO_STR(FMOD_SOUND_TYPE_XMA)
                SOUND_TYPE_TO_STR(FMOD_SOUND_TYPE_AUDIOQUEUE)
                SOUND_TYPE_TO_STR(FMOD_SOUND_TYPE_MAX)
        default: soundTypeStr = "Unknown"; break;
        }
        WriteLogMessage(logFile, "INFO", __FUNCTION__, "FMOD::Sound::getFormat successful - Sound Type: " + soundTypeStr + ", Format: " + formatStr + ", Channels: " + std::to_string(info.channels) + ", Bits Per Sample: " + std::to_string(info.bitsPerSample), verboseLogEnabled, FMOD_OK); // Logs successful format retrieval (INFO level)
    }

    WriteLogMessage(logFile, "INFO", __FUNCTION__, "Getting default sound parameters...", verboseLogEnabled, FMOD_OK); // Logs attempt to get default parameters
    fmodSystemResult = subSound->getDefaults(&defaultFrequency, &defaultPriority); // Gets default frequency and priority from FMOD Sound object
    if (fmodSystemResult != FMOD_OK) { // Checks if getting defaults failed
        WriteLogMessage(logFile, "ERROR", __FUNCTION__, "FMOD::Sound::getDefaults failed for sub-sound " + std::to_string(subSoundIndex) + ": " + FMOD_ErrorString(fmodSystemResult), verboseLogEnabled, FMOD_OK); // Logs FMOD error (ERROR level)
        CheckFMODResult(fmodSystemResult, "FMOD::Sound::getDefaults failed for sub-sound " + std::to_string(subSoundIndex)); // Throws exception on error
    }
    else {
        WriteLogMessage(logFile, "INFO", __FUNCTION__, "FMOD::Sound::getDefaults successful - Default Frequency: " + std::to_string(defaultFrequency) + ", Default Priority: " + std::to_string(defaultPriority), verboseLogEnabled, FMOD_OK); // Logs successful defaults retrieval (INFO level)
    }

    info.sampleRate = (defaultFrequency > 0) ? static_cast<int>(defaultFrequency) : 44100; // Sets sample rate, using default frequency if available, otherwise defaults to 44100 Hz
    WriteLogMessage(logFile, "INFO", __FUNCTION__, "Final Sample Rate for WAV header: " + std::to_string(info.sampleRate), verboseLogEnabled, FMOD_OK); // Logs final sample rate used for WAV header

    WriteLogMessage(logFile, "INFO", __FUNCTION__, "Getting sound length in bytes...", verboseLogEnabled, FMOD_OK); // Logs attempt to get sound length in bytes
    fmodSystemResult = subSound->getLength(&info.soundLengthBytes, FMOD_TIMEUNIT_PCMBYTES); // Gets sound length in bytes
    if (fmodSystemResult != FMOD_OK) { // Checks if getting length failed
        WriteLogMessage(logFile, "ERROR", __FUNCTION__, "FMOD::Sound::getLength (bytes) failed for sub-sound " + std::to_string(subSoundIndex) + ": " + FMOD_ErrorString(fmodSystemResult), verboseLogEnabled, FMOD_OK); // Logs FMOD error (ERROR level)
        CheckFMODResult(fmodSystemResult, "FMOD::Sound::getLength (bytes) failed for sub-sound " + std::to_string(subSoundIndex)); // Throws exception on error
    }
    else {
        WriteLogMessage(logFile, "INFO", __FUNCTION__, "FMOD::Sound::getLength (bytes) successful - Length: " + std::to_string(info.soundLengthBytes) + " bytes", verboseLogEnabled, FMOD_OK); // Logs successful length retrieval in bytes (INFO level)
    }

    WriteLogMessage(logFile, "INFO", __FUNCTION__, "Getting sound length in milliseconds...", verboseLogEnabled, FMOD_OK); // Logs attempt to get sound length in milliseconds
    fmodSystemResult = subSound->getLength(&info.lengthMs, FMOD_TIMEUNIT_MS); // Gets sound length in milliseconds
    if (fmodSystemResult != FMOD_OK) { // Checks if getting length failed
        WriteLogMessage(logFile, "ERROR", __FUNCTION__, "FMOD::Sound::getLength (ms) failed for sub-sound " + std::to_string(subSoundIndex) + ": " + FMOD_ErrorString(fmodSystemResult), verboseLogEnabled, FMOD_OK); // Logs FMOD error (ERROR level)
        CheckFMODResult(fmodSystemResult, "FMOD::Sound::getLength (ms) failed for sub-sound " + std::to_string(subSoundIndex)); // Throws exception on error
    }
    else {
        WriteLogMessage(logFile, "INFO", __FUNCTION__, "FMOD::Sound::getLength (ms) successful - Length: " + std::to_string(info.lengthMs) + " ms", verboseLogEnabled, FMOD_OK); // Logs successful length retrieval in milliseconds (INFO level)
    }

    WriteLogMessage(logFile, "INFO", __FUNCTION__, "Getting sub-sound name...", verboseLogEnabled, FMOD_OK); // Logs attempt to get sub-sound name
    fmodSystemResult = subSound->getName(info.subSoundName, sizeof(info.subSoundName) - 1); // Gets sub-sound name
    if (fmodSystemResult != FMOD_OK && fmodSystemResult != FMOD_ERR_TAGNOTFOUND) { // Checks if getting name failed (but ignores FMOD_ERR_TAGNOTFOUND, which is expected if no name tag exists)
        WriteLogMessage(logFile, "WARNING", __FUNCTION__, "FMOD::Sound::getName failed or tag not found for sub-sound " + std::to_string(subSoundIndex) + ": " + FMOD_ErrorString(fmodSystemResult), verboseLogEnabled, FMOD_OK); // Logs warning if getting name failed or tag not found (WARNING level)
    }
    else {
        WriteLogMessage(logFile, "INFO", __FUNCTION__, "FMOD::Sound::getName successful - Name: " + std::string(info.subSoundName), verboseLogEnabled, FMOD_OK); // Logs successful name retrieval (INFO level)
    }
    return info; // Returns the SoundInfo structure containing retrieved information
}


/**
 * @brief Processes a single sub-sound, extracts audio data, and saves it as a WAV file.
 *
 * @param fmodSystem Pointer to the initialized FMOD System object.
 * @param subSound FMOD Sound object representing the sub-sound to process.
 * @param subSoundIndex Index of the sub-sound being processed.
 * @param totalSubSounds Total number of sub-sounds in the FSB file.
 * @param baseFileName Base file name (stem of the input FSB file name).
 * @param outputDirectoryPath Path to the output directory where WAV file will be saved.
 * @param verboseLogEnabled Flag indicating if verbose logging is enabled.
 * @param logFile Output file stream for the log file.
 *
 * @details
 * This function orchestrates the process of extracting audio data from a given FMOD sub-sound and saving it as a WAV file.
 * It retrieves sound information, constructs the output file path, writes the WAV header, and then writes the audio data chunks
 * based on the sound format. It also handles error logging and console output for progress and status.
 */
void ProcessSubSound(FMOD::System* fmodSystem, FMOD::Sound* subSound, int subSoundIndex, int totalSubSounds, const std::string& baseFileName, const std::filesystem::path& outputDirectoryPath, bool verboseLogEnabled, std::ofstream& logFile) {

    logFile << std::endl; // Adds a newline to the log file for better readability
    WriteLogMessage(logFile, "INFO", __FUNCTION__, "Processing sub-sound " + std::to_string(subSoundIndex + 1) + "/" + std::to_string(totalSubSounds), verboseLogEnabled, FMOD_OK); // Logs start of sub-sound processing
    CheckFMODResult(subSound->seekData(0), "FMOD::Sound::seekData failed for sub-sound " + std::to_string(subSoundIndex)); // Seeks to the beginning of the sub-sound data
    WriteLogMessage(logFile, "INFO", __FUNCTION__, "FMOD::Sound::seekData successful", verboseLogEnabled, FMOD_OK); // Logs successful seek operation

    SoundInfo soundInfo = GetSoundInfo(subSound, subSoundIndex, verboseLogEnabled, logFile); // Retrieves sound information for the current sub-sound

    std::string outputFileName = std::strlen(soundInfo.subSoundName) > 0 ? SanitizeFileName(soundInfo.subSoundName) : SanitizeFileName(baseFileName + "_" + std::to_string(subSoundIndex)); // Constructs output file name, using sub-sound name if available, otherwise using base file name and sub-sound index, sanitizing the name
    std::filesystem::path fullOutputPath = outputDirectoryPath / (outputFileName + ".wav"); // Creates the full output file path by combining output directory, file name, and ".wav" extension

    std::cout << std::endl << " Processing sub-sound " << subSoundIndex + 1 << "/" << totalSubSounds << ":" << std::endl; // Prints processing start message to console
    std::cout << " Name: " << (std::strlen(soundInfo.subSoundName) > 0 ? soundInfo.subSoundName : "") << std::endl; // Prints sub-sound name to console (if available)
    std::cout << " Channels: " << soundInfo.channels << std::endl; // Prints number of channels to console
    std::cout << " Sample Rate: " << soundInfo.sampleRate << " Hz" << std::endl; // Prints sample rate to console
    std::cout << " Length: " << soundInfo.lengthMs << " ms" << std::endl; // Prints length in milliseconds to console

    std::ofstream wavFile(fullOutputPath.wstring(), std::ios::binary | std::ios::trunc); // Opens output WAV file in binary truncate mode (overwrite if exists)
    if (!wavFile.is_open()) { // Checks if WAV file opening failed
        WriteLogMessage(logFile, "ERROR", __FUNCTION__, "Error opening output WAV file: " + fullOutputPath.u8string(), verboseLogEnabled, FMOD_OK); // Logs file open error (ERROR level)
        std::cerr << " Error opening output WAV file: " << fullOutputPath.u8string() << std::endl; // Prints error to std::cerr
        throw std::runtime_error("Failed to open output WAV file"); // Throws exception on error
    }
    WriteLogMessage(logFile, "INFO", __FUNCTION__, "WAV file opened successfully: " + fullOutputPath.u8string(), verboseLogEnabled, FMOD_OK); // Logs successful file open (INFO level)

    if (!WriteWAVHeader(wavFile, soundInfo.sampleRate, soundInfo.channels, soundInfo.soundLengthBytes, soundInfo.bitsPerSample, soundInfo.format)) { // Writes WAV header to the file
        WriteLogMessage(logFile, "ERROR", __FUNCTION__, "Error writing WAV header to file: " + fullOutputPath.u8string(), verboseLogEnabled, FMOD_OK); // Logs header write error (ERROR level)
        std::cerr << " Error writing WAV header to file: " << fullOutputPath.u8string() << std::endl; // Prints error to std::cerr
        throw std::runtime_error("Failed to write WAV header"); // Throws exception on error
    }
    WriteLogMessage(logFile, "INFO", __FUNCTION__, "WAV header written successfully", verboseLogEnabled, FMOD_OK); // Logs successful header write (INFO level)

    int chunkCount = 0; // Initializes chunk counter for logging
    bool writeSuccess = false; // Flag to track success of audio data writing

    switch (soundInfo.format) { // Switch statement based on sound format to determine data writing function
    case FMOD_SOUND_FORMAT_PCM8: writeSuccess = AudioProcessor::WriteAudioDataChunk<unsigned char>(subSound, wavFile, soundInfo.soundLengthBytes, subSoundIndex, chunkCount, verboseLogEnabled, logFile); break; // Writes 8-bit PCM data
    case FMOD_SOUND_FORMAT_PCM16: writeSuccess = AudioProcessor::WriteAudioDataChunk<short>(subSound, wavFile, soundInfo.soundLengthBytes, subSoundIndex, chunkCount, verboseLogEnabled, logFile); break; // Writes 16-bit PCM data
    case FMOD_SOUND_FORMAT_PCM24: writeSuccess = AudioProcessor::WritePCM24DataChunk(subSound, wavFile, soundInfo.soundLengthBytes, subSoundIndex, chunkCount, verboseLogEnabled, logFile); break; // Writes 24-bit PCM data
    case FMOD_SOUND_FORMAT_PCM32: writeSuccess = AudioProcessor::WriteAudioDataChunk<int>(subSound, wavFile, soundInfo.soundLengthBytes, subSoundIndex, chunkCount, verboseLogEnabled, logFile); break; // Writes 32-bit PCM data
    case FMOD_SOUND_FORMAT_PCMFLOAT: writeSuccess = AudioProcessor::WritePCMFloatDataChunk(subSound, wavFile, soundInfo.soundLengthBytes, subSoundIndex, chunkCount, verboseLogEnabled, logFile); break; // Writes PCM float data
    default:
        WriteLogMessage(logFile, "WARNING", __FUNCTION__, "Unsupported format detected: " + std::to_string(soundInfo.format) + ". Processing as PCM16 (potentially incorrect).", verboseLogEnabled, FMOD_OK); // Logs warning for unsupported format (WARNING level)
        writeSuccess = AudioProcessor::WriteAudioDataChunk<short>(subSound, wavFile, soundInfo.soundLengthBytes, subSoundIndex, chunkCount, verboseLogEnabled, logFile); // Falls back to writing as 16-bit PCM (potential data loss or incorrect output)
        break;
    }

    if (!writeSuccess) { // Checks if audio data writing failed
        WriteLogMessage(logFile, "ERROR", __FUNCTION__, "Error writing audio data to WAV file for sub-sound " + std::to_string(subSoundIndex), verboseLogEnabled, FMOD_OK); // Logs data write error (ERROR level)
        std::cerr << " Error writing audio data to WAV file for sub-sound " << subSoundIndex << std::endl; // Prints error to std::cerr
        throw std::runtime_error("Failed to write audio data to WAV file"); // Throws exception on error
    }

    WriteLogMessage(logFile, "INFO", __FUNCTION__, "Sub-sound processing finished successfully", verboseLogEnabled, FMOD_OK); // Logs successful sub-sound processing (INFO level)
    std::cout << " Status: Success" << std::endl; // Prints success status to console
}