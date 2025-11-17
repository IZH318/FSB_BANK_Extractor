/**
 * @file FSB_BANK_Extractor_CS_GUI.cs
 * @brief Main form for the FSB/BANK Extractor GUI application.
 * @author (Github) IZH318 (https://github.com/IZH318)
 *
 * @details
 * This file contains the implementation of the main form (Form1) for the FSB/BANK Extractor GUI application.
 * This application allows users to extract audio streams from FMOD Sound Bank (.fsb) and Bank (.bank) files and save them as Waveform Audio (.wav) files.
 * It provides a graphical user interface for file selection, output directory configuration, batch processing, and verbose logging.
 *
 * Key Features:
 * - Drag and drop file support for adding FSB and BANK files to the processing list.
 * - Batch processing of multiple FSB and BANK files.
 * - Output directory selection options: same as resource, same as executable, or custom user-defined directory.
 * - Verbose logging option to provide detailed information during the extraction process.
 * - Displays file information (name, channels, sample rate, length) for extracted sub-sounds.
 * - Handles both FSB files directly and BANK files by extracting embedded FSBs first.
 * - User-friendly interface with status updates and progress indication.
 *
 * Note:
 * This code is designed to be user-friendly and maintainable, with clear comments and organized structure.
 * It aims to provide a robust and efficient solution for extracting audio from FSB and BANK files using the FMOD Engine.
 *
 * FMOD Engine & Development Environment Compatibility:
 *
 * Tested Environment:
 *  - FMOD Engine Version: v2.03.06 (Studio API minor release, build 149358)
 *  - Development Environment: Visual Studio 2022 (v143)
 *  - Target Framework: .NET Framework 4.8
 *  - Primary Test Platform: Windows 10 64-bit
 *  - Last Development Date: 2025-11-17
 *
 * Recommendations for Best Results:
 *  - Use FMOD Engine v2.03.06 for development and deployment.
 *  - Develop and build the program within Visual Studio 2022 (v143).
 *  - Ensure your C# compiler is set to target .NET Framework 4.8.
 *
 * Important Notes:
 *  - Compatibility with other FMOD versions or platforms is not guaranteed.
 *  - For troubleshooting, refer to the FMOD Engine v2.03.06 documentation.
 *  - Using different FMOD versions or development environments may lead to unexpected behavior.
 *  - Download FMOD Engine v2.03.06 from the FMOD website archive (if available) or your development sources.
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Globalization;
using System.Runtime.InteropServices;
using FMOD;

namespace FSB_BANK_Extractor_CS_GUI
{
    /**
     * @class Form1
     * @brief Main form class for the FSB/BANK Extractor GUI application.
     *
     * @details
     * This class defines the main form of the application, handling user interactions,
     * file processing, UI updates, and integration with the FMOD library for audio extraction.
     * It includes event handlers for button clicks, drag and drop operations, and list view interactions.
     * The form manages a list of files to be processed and provides controls for output directory and logging options.
     */
    public partial class FSB_BANK_Extractor_CS_GUI : Form
    {
        private List<string> FileList = new List<string>(); // List to store file paths of FSB/BANK files added for processing

        /**
         * @brief Constructor for Form1 class.
         *
         * @details
         * Initializes the main form, sets up drag and drop event handlers for the file list view,
         * and connects a double-click event handler to the list view for displaying file information.
         */
        public FSB_BANK_Extractor_CS_GUI()
        {
            InitializeComponent();

            // Attach event handlers to the listViewFiles control for drag and drop functionality
            this.listViewFiles.DragEnter += new DragEventHandler(listViewFiles_DragEnter); // Event handler for when dragging enters the list view
            this.listViewFiles.DragDrop += new DragEventHandler(listViewFiles_DragDrop);   // Event handler for when files are dropped onto the list view
            this.listViewFiles.DoubleClick += new EventHandler(listViewFiles_DoubleClick); // Event handler for double-clicking an item in the list view to view file information
        }

        #region Constants Class

        /**
         * @class Constants
         * @brief Defines constant values used throughout the application.
         *
         * @details
         * This static class holds constant strings, numbers, and other values that are used in various parts of the application.
         * These constants are related to WAV file format, audio processing, and other fixed parameters.
         * Using constants improves code readability and maintainability.
         */
        static class Constants
        {
            public const string RIFF_HEADER = "RIFF"; // Constant string for the RIFF header identifier in WAV files
            public const string WAVE_FORMAT = "WAVE"; // Constant string for the WAVE format identifier in WAV files
            public const string FMT_CHUNK = "fmt ";   // Constant string for the format chunk identifier in WAV files
            public const string DATA_CHUNK = "data";  // Constant string for the data chunk identifier in WAV files
            public const ushort FORMAT_PCM = 1;         // Constant ushort for PCM format code in WAV header
            public const ushort FORMAT_PCM_FLOAT = 3;   // Constant ushort for PCM float format code in WAV header
            public const int BITS_IN_BYTE = 8;            // Constant integer representing the number of bits in a byte
            public const uint CHUNK_SIZE = 4096;   // Constant unsigned integer representing the default chunk size for reading audio data (in bytes)
            public const float MAX_SAMPLE_VALUE = 32767.0f; // Constant float representing the maximum sample value for 16-bit PCM audio (used for PCM clipping/normalization if needed)
        }

        #endregion

        #region FMOD Related Classes (FMODSystem, FMODSound)

        /**
         * @class FMODSystem
         * @brief Manages the FMOD System object, providing initialization, access, and disposal.
         *
         * @details
         * This class is a wrapper around the FMOD System object, responsible for creating, initializing,
         * providing access to, and properly disposing of the FMOD system. It implements the IDisposable interface
         * to ensure that FMOD resources are released when the FMODSystem object is no longer needed.
         * It also includes version checking to ensure compatibility with the required FMOD library version.
         */
        class FMODSystem : IDisposable
        {
            private FMOD.System system; // Private member variable to hold the FMOD System object

            /**
             * @brief Constructor for FMODSystem class.
             *
             * @details
             * Initializes the FMOD system by creating an FMOD System object, checking the FMOD library version,
             * and initializing the system with default settings.
             * Throws an exception if FMOD initialization fails or if the FMOD library version is incompatible.
             */
            public FMODSystem()
            {
                FMOD.Factory.System_Create(out system); // Creates an FMOD System object using the FMOD Factory
                CheckFMODResult(system.getVersion(out uint version), "FMOD::System::getVersion failed"); // Retrieves the version of the FMOD library and checks for errors

                if (version < VERSION.number) // Checks if the FMOD library version is older than the required header version
                {
                    FSB_BANK_Extractor_CS_GUI.LogMessage($" FMOD lib version {version:X} is less than header version {VERSION.number}"); // Logs a message indicating FMOD version mismatch
                    throw new Exception("FMOD version mismatch"); // Throws an exception to indicate FMOD version incompatibility
                }

                CheckFMODResult(system.init(32, FMOD.INITFLAGS.NORMAL, IntPtr.Zero), "FMOD::System::init failed"); // Initializes the FMOD system with 32 channels and normal initialization flags
            }

            /**
             * @brief Gets the FMOD System object.
             *
             * @return FMOD.System The FMOD System object instance.
             */
            public FMOD.System Get() { return system; } // Public method to access the FMOD System object

            /**
             * @brief Disposes of the FMOD System object and releases resources.
             *
             * @details
             * Implements the Dispose method of the IDisposable interface.
             * Closes and releases the FMOD system to free up resources.
             * Handles potential errors during closing and releasing, logging error messages if necessary.
             */
            public void Dispose()
            {
                if (system.hasHandle()) // Checks if the FMOD System object has a valid handle before attempting to dispose
                {
                    var result = system.close(); // Closes the FMOD system
                    if (result != FMOD.RESULT.OK) // Checks if closing the system was successful
                    {
                        FSB_BANK_Extractor_CS_GUI.LogError($" FMOD::System::close failed: {FMOD.Error.String(result)}"); // Logs an error message if closing fails
                    }
                    result = system.release(); // Releases the FMOD system, freeing allocated resources
                    if (result != FMOD.RESULT.OK) // Checks if releasing the system was successful
                    {
                        FSB_BANK_Extractor_CS_GUI.LogError($" FMOD::System::release failed: {FMOD.Error.String(result)}"); // Logs an error message if releasing fails
                    }
                    system.clearHandle(); // Clears the handle of the FMOD System object
                }
            }
        }

        /**
         * @class FMODSound
         * @brief Manages an FMOD Sound object, loading sound files and handling disposal.
         *
         * @details
         * This class is a wrapper around the FMOD Sound object, responsible for creating, loading a sound file,
         * providing access to, and properly disposing of the FMOD sound. It implements the IDisposable interface
         * to ensure that FMOD sound resources are released when the FMODSound object is no longer needed.
         * It uses FMOD_MODE.CREATESTREAM to load sounds as streams, which is efficient for audio extraction.
         */
        class FMODSound : IDisposable
        {
            private FMOD.Sound sound; // Private member variable to hold the FMOD Sound object

            /**
             * @brief Constructor for FMODSound class.
             *
             * @param system The FMOD System object to use for creating the sound.
             * @param filePath The path to the audio file to load as a sound.
             *
             * @details
             * Creates an FMOD Sound object by loading an audio file from the specified path using the provided FMOD System.
             * Uses FMOD_MODE.CREATESTREAM mode for efficient streaming of audio data, suitable for extraction.
             * Throws an exception if creating the sound fails.
             */
            public FMODSound(FMOD.System system, string filePath)
            {
                FMOD.CREATESOUNDEXINFO exinfo = new FMOD.CREATESOUNDEXINFO(); // Creates extended sound creation info structure
                exinfo.cbsize = Marshal.SizeOf(exinfo); // Sets the size of the extended info structure
                var result = system.createSound(filePath, FMOD.MODE.CREATESTREAM, ref exinfo, out sound); // Creates an FMOD sound object in stream mode from the specified file path
                CheckFMODResult(result, $"FMOD::System::createSound failed for {filePath}"); // Checks if creating the sound was successful, throws exception if not
            }

            /**
             * @brief Gets the FMOD Sound object.
             *
             * @return FMOD.Sound The FMOD Sound object instance.
             */
            public FMOD.Sound Get() { return sound; } // Public method to access the FMOD Sound object

            /**
             * @brief Disposes of the FMOD Sound object and releases resources.
             *
             * @details
             * Implements the Dispose method of the IDisposable interface.
             * Releases the FMOD sound to free up resources.
             * Handles potential errors during releasing, logging error messages if necessary.
             */
            public void Dispose()
            {
                if (sound.hasHandle()) // Checks if the FMOD Sound object has a valid handle before attempting to dispose
                {
                    var result = sound.release(); // Releases the FMOD sound, freeing allocated resources
                    if (result != FMOD.RESULT.OK) // Checks if releasing the sound was successful
                    {
                        FSB_BANK_Extractor_CS_GUI.LogError($" FMOD::Sound::release failed: {FMOD.Error.String(result)}"); // Logs an error message if releasing fails
                    }
                    sound.clearHandle(); // Clears the handle of the FMOD Sound object
                }
            }
        }

        #endregion

        #region Helper Structs and Classes (Pcm24, AudioProcessor, SoundInfo)

        /**
         * @struct Pcm24
         * @brief Empty struct to represent 24-bit PCM audio format.
         *
         * @details
         * This struct is used as a type parameter in generic audio processing methods to specifically handle 24-bit PCM data.
         * It serves as a marker type to differentiate 24-bit PCM processing from other PCM formats.
         */
        public struct Pcm24 { }

        /**
         * @class AudioProcessor
         * @brief Provides static methods for processing audio data, such as writing audio chunks to WAV files.
         *
         * @details
         * This static class contains utility methods for handling audio data processing tasks.
         * Currently, it includes a method for writing audio data chunks from an FMOD sound to a WAV file stream.
         * It supports different PCM formats and handles data conversion and writing to the WAV file format.
         */
        static class AudioProcessor
        {
            /**
             * @brief Writes audio data from an FMOD sub-sound to a WAV file stream in chunks.
             *
             * @typeparam T The data type representing the audio sample format (e.g., byte, short, int, float, Pcm24).
             * @param subSound The FMOD Sound object representing the sub-sound.
             * @param wavFile The Stream to write the WAV file data to.
             * @param soundLengthBytes The total length of the sound data in bytes.
             * @param subSoundIndex The index of the sub-sound being processed (for logging purposes).
             * @param verboseLogEnabled Flag indicating whether verbose logging is enabled.
             * @param logFile StreamWriter for logging messages.
             * @param bw BinaryWriter for writing binary data to the WAV file stream.
             * @return bool True if writing audio data chunks was successful, false otherwise.
             *
             * @details
             * This generic method reads audio data from an FMOD sub-sound in chunks and writes it to a WAV file stream.
             * It handles different PCM formats based on the type parameter T, including byte (PCM8), short (PCM16),
             * int (PCM32), float (PCMFLOAT), and the custom Pcm24 struct for 24-bit PCM.
             * It reads data in chunks of Constants.CHUNK_SIZE bytes to manage memory efficiently and process large audio files.
             * For PCMFLOAT format, it also performs clipping to ensure sample values are within the valid range [-1.0, 1.0].
             */
            internal static bool WriteAudioDataChunk<T>(FMOD.Sound subSound, Stream wavFile, uint soundLengthBytes, int subSoundIndex, ref int chunkCount, bool verboseLogEnabled, StreamWriter logFile, BinaryWriter bw) where T : struct
            {
                ++chunkCount; // Increment chunk counter
                uint totalBytesRead = 0; // Initialize bytes read counter
                int sampleSize = Marshal.SizeOf(typeof(T)); // Get size of sample type
                uint bytesToRead; // Bytes to read per chunk

                try // Add try block to handle potential exceptions
                {
                    while (totalBytesRead < soundLengthBytes) // Loop until all data is read
                    {
                        bytesToRead = Math.Min(Constants.CHUNK_SIZE, soundLengthBytes - totalBytesRead); // Calculate bytes to read for current chunk

                        byte[] byteBuffer = new byte[bytesToRead]; // Allocate managed byte buffer for chunk
                        uint bytesRead; // Variable to store bytes read by FMOD

                        // Call FMOD readData to read audio data into byteBuffer
                        FMOD.RESULT fmodSystemResult = subSound.readData(byteBuffer, out bytesRead);

                        if (fmodSystemResult != FMOD.RESULT.OK) // Check for FMOD API error
                        {
                            WriteLogMessage(logFile, "INFO", "WriteAudioDataChunk", $"Reading chunk {chunkCount} - Bytes to read: {bytesToRead}", verboseLogEnabled); // Log chunk read attempt
                            WriteLogMessage(logFile, "ERROR", "WriteAudioDataChunk", $"FMOD::Sound::readData failed for sub-sound {subSoundIndex}, chunk {chunkCount}: {FMOD.Error.String(fmodSystemResult)}", verboseLogEnabled); // Log FMOD error details
                            Console.Error.WriteLine($" FMOD::Sound::readData failed for sub-sound {subSoundIndex}: {FMOD.Error.String(fmodSystemResult)}"); // Output error to console
                            return false; // Return failure
                        }

                        // Process audio data based on sample type T
                        if (typeof(T) == typeof(byte)) // 8-bit PCM
                        {
                            bw.Write(byteBuffer, 0, (int)bytesRead); // Write byte buffer to WAV file
                        }
                        else if (typeof(T) == typeof(short)) // 16-bit PCM
                        {
                            for (int i = 0; i < bytesRead; i += sampleSize) // Iterate through samples
                            {
                                bw.Write(BitConverter.ToInt16(byteBuffer, i)); // Convert and write short
                            }
                        }
                        else if (typeof(T) == typeof(int)) // 32-bit PCM
                        {
                            for (int i = 0; i < bytesRead; i += sampleSize) // Iterate through samples
                            {
                                bw.Write(BitConverter.ToInt32(byteBuffer, i)); // Convert and write int
                            }
                        }
                        else if (typeof(T) == typeof(float)) // PCMFLOAT
                        {
                            for (int i = 0; i < bytesRead / sampleSize; ++i) // Iterate through float samples
                            {
                                float sampleValue = BitConverter.ToSingle(byteBuffer, i * sampleSize); // Convert to float

                                // Clipping prevention for PCMFLOAT [-1.0f, 1.0f]
                                if (sampleValue > 1.0f) // Upper clipping
                                {
                                    if (verboseLogEnabled)
                                    {
                                        WriteLogMessage(logFile, "WARNING", "WriteAudioDataChunk", $"PCMFLOAT clipping (upper): original={sampleValue}, limited=1.0", verboseLogEnabled);
                                    }
                                    sampleValue = 1.0f; // Apply upper limit
                                }
                                else if (sampleValue < -1.0f) // Lower clipping
                                {
                                    if (verboseLogEnabled)
                                    {
                                        WriteLogMessage(logFile, "WARNING", "WriteAudioDataChunk", $"PCMFLOAT clipping (lower): original={sampleValue}, limited value=-1.0", verboseLogEnabled);
                                    }
                                    sampleValue = -1.0f; // Apply lower limit
                                }

                                bw.Write(sampleValue); // Write float sample
                            }
                        }
                        else if (typeof(T) == typeof(Pcm24)) // 24-bit PCM
                        {
                            for (int i = 0; i < bytesRead; i += 3) // Iterate through 24-bit samples
                            {
                                bw.Write(byteBuffer[i]);     // Write byte 1
                                bw.Write(byteBuffer[i + 1]); // Write byte 2
                                bw.Write(byteBuffer[i + 2]); // Write byte 3
                            }
                        }
                        else // Unsupported type
                        {
                            Console.Error.WriteLine("Unsupported type"); // Error message for unsupported type
                            return false; // Return failure
                        }

                        totalBytesRead += bytesRead; // Update total bytes read counter
                    }

                    return true; // Return success
                }
                catch (Exception e) // Catch any exceptions during chunk writing
                {
                    WriteLogMessage(logFile, "ERROR", "WriteAudioDataChunk", $"Exception during chunk writing: {e.Message}", verboseLogEnabled); // Log exception
                    Console.Error.WriteLine($" Exception during chunk writing: {e.Message}"); // Output exception to console
                    return false; // Return failure due to exception
                }
            }
        }

        /**
         * @struct SoundInfo
         * @brief Structure to hold information about a sound extracted from FSB.
         *
         * @details
         * This struct is used to store various properties of a sub-sound extracted from an FSB file.
         * It includes the sound format, sound type, sample rate, bits per sample, number of channels,
         * sound length in bytes and milliseconds, sub-sound name, and default priority.
         * Instances of this struct are populated by the GetSoundInfo function and used for writing WAV headers and logging.
         */
        public struct SoundInfo
        {
            public SOUND_FORMAT format;     // FMOD sound format (e.g., PCM8, PCM16, PCMFLOAT)
            public SOUND_TYPE soundType;      // FMOD sound type (e.g., WAV, OGGVORBIS, FSB)
            public int sampleRate;          // Sample rate of the sound in Hz
            public int bitsPerSample;       // Bits per sample for the sound
            public int channels;            // Number of channels (1 for mono, 2 for stereo, etc.)
            public uint soundLengthBytes; // Length of the sound data in bytes
            public uint lengthMs;       // Length of the sound in milliseconds
            public string subSoundName;     // Name of the sub-sound (if available)
            public int defaultPriority;    // Default priority of the sound (not directly used in WAV extraction, but retrieved for completeness)
        }

        #endregion

        #region Helper Functions (CheckFMODResult, SanitizeFileName, WriteWAVHeader, WriteLogMessage, GetSoundInfo, ProcessSubSound)

        /**
         * @brief Checks the result of an FMOD API call and throws an exception if it indicates an error.
         *
         * @param result The FMOD.RESULT value returned by an FMOD API function.
         * @param message A descriptive message to include in the exception if an error occurs.
         *
         * @throws Exception Throws an exception if the FMOD.RESULT is not FMOD.RESULT.OK, indicating an error.
         *
         * @details
         * This static helper function is used to simplify error checking after calling FMOD API functions.
         * If the FMOD.RESULT value is not FMOD.RESULT.OK, it means an error occurred during the FMOD API call.
         * In this case, this function throws an exception with a combined error message, including the provided message and the FMOD error string.
         */
        public static void CheckFMODResult(FMOD.RESULT result, string message)
        {
            if (result != FMOD.RESULT.OK) // Check if the FMOD result is not OK (i.e., an error occurred)
            {
                throw new Exception(message + ": " + FMOD.Error.String(result)); // Throw a new exception with a combined error message
            }
        }

        /**
         * @brief Sanitizes a file name by replacing invalid characters with safe Unicode alternatives.
         *
         * @param fileName The original file name string to sanitize.
         * @return string The sanitized file name string with invalid characters replaced.
         *
         * @details
         * This static helper function takes a file name string as input and replaces characters that are typically invalid
         * or problematic in file names (e.g., <, >, :, ", /, \, |, ?, *) with similar-looking but safe Unicode characters.
         * This is done to ensure that generated file names are valid and avoid file system errors when saving extracted audio files.
         */
        static string SanitizeFileName(string fileName)
        {
            // Dictionary mapping invalid characters to their Unicode replacements
            var charMap = new Dictionary<char, string>
            {
                {'<', "〈"}, {'>', "〉"}, {':', "："}, {'\"', "＂"}, {'/', "／"},
                {'\\', "￦"}, {'|', "｜"}, {'?', "？"}, {'*', "＊"}
            };

            string sanitized = fileName; // Create a copy of the input file name to modify

            foreach (var kvp in charMap) // Iterate through the character map
            {
                sanitized = sanitized.Replace(kvp.Key.ToString(), kvp.Value); // Replace each invalid character with its corresponding Unicode replacement
            }
            return sanitized; // Return the sanitized file name
        }

        /**
         * @brief Writes the WAV file header to the provided stream.
         *
         * @param file The Stream to write the WAV header to.
         * @param sampleRate The sample rate of the audio in Hz.
         * @param channels The number of audio channels.
         * @param dataSize The size of the audio data in bytes.
         * @param bitsPerSample The number of bits per sample for the audio.
         * @param format The SOUND_FORMAT of the audio data.
         * @return bool True if the WAV header was written successfully, false otherwise.
         *
         * @details
         * This static helper function writes the standard WAV file header to the given stream.
         * The WAV header includes the RIFF chunk descriptor, WAVE format descriptor, format chunk, and data chunk.
         * It writes necessary information like sample rate, number of channels, data size, and bits per sample
         * to ensure the output file is a valid WAV file that can be recognized and played by audio players.
         */
        static bool WriteWAVHeader(Stream file, int sampleRate, int channels, long dataSize, int bitsPerSample, SOUND_FORMAT format)
        {
            if (!file.CanWrite) // Check if the provided stream is writable
            {
                FSB_BANK_Extractor_CS_GUI.LogError(" Error: Output file is not open."); // Log error if the stream is not writable
                return false; // Return false to indicate failure
            }

            try
            {
                var bw = new BinaryWriter(file); // Create a BinaryWriter to write binary data to the stream

                // Write RIFF header chunk
                bw.Write(Encoding.ASCII.GetBytes(Constants.RIFF_HEADER)); // Write "RIFF" ASCII bytes
                bw.Write((uint)(36 + dataSize)); // Write chunk size (WAV header size + data size), 4 bytes
                bw.Write(Encoding.ASCII.GetBytes(Constants.WAVE_FORMAT)); // Write "WAVE" ASCII bytes

                // Write format 'fmt ' chunk
                bw.Write(Encoding.ASCII.GetBytes(Constants.FMT_CHUNK)); // Write "fmt " ASCII bytes
                bw.Write((uint)16); // Write format chunk size (always 16 for PCM) as uint
                bw.Write((ushort)(format == SOUND_FORMAT.PCMFLOAT ? Constants.FORMAT_PCM_FLOAT : Constants.FORMAT_PCM)); // Write audio format code (PCM or PCM float) as ushort
                bw.Write((ushort)channels); // Write number of channels as ushort
                bw.Write((uint)sampleRate); // Write sample rate as uint
                bw.Write((uint)((long)sampleRate * channels * bitsPerSample / Constants.BITS_IN_BYTE)); // Write byte rate (bytes per second) as uint
                bw.Write((ushort)(channels * bitsPerSample / Constants.BITS_IN_BYTE)); // Write block align (bytes per sample block) as ushort
                bw.Write((ushort)bitsPerSample); // Write bits per sample as ushort

                // Write data 'data' chunk
                bw.Write(Encoding.ASCII.GetBytes(Constants.DATA_CHUNK)); // Write "data" ASCII bytes
                bw.Write((uint)dataSize); // Write data chunk size (audio data size) as uint

                return true; // Return true to indicate successful WAV header writing
            }
            catch (Exception e) // Catch any exceptions that occur during WAV header writing
            {
                FSB_BANK_Extractor_CS_GUI.LogError($" Error writing WAV header: {e.Message}"); // Log error message with exception details
                return false; // Return false to indicate failure
            }
        }

        /**
         * @brief Asynchronously writes a log message to the log file if verbose logging is enabled.
         *
         * @param logFile StreamWriter for the log file.
         * @param level Log level (e.g., "INFO", "WARNING", "ERROR").
         * @param functionName Name of the function where the log message originates.
         * @param message The log message string.
         * @param verboseLogEnabled Flag indicating whether verbose logging is enabled.
         * @param errorCode Optional FMOD.RESULT error code to include in the log message. Defaults to FMOD.RESULT.OK.
         * @return Task Represents the asynchronous operation.
         *
         * @details
         * This static helper function asynchronously writes a formatted log message to the provided StreamWriter
         * if verbose logging is enabled. The log message includes a timestamp, log level, function name, and the message itself.
         * If an FMOD error code is provided (and is not FMOD.RESULT.OK), it is also included in the log message.
         * Asynchronous writing is used to prevent blocking the main thread, especially during potentially long-running logging operations.
         */
        static async Task WriteLogMessageAsync(StreamWriter logFile, string level, string functionName, string message, bool verboseLogEnabled, RESULT errorCode = RESULT.OK)
        {
            if (logFile != null && verboseLogEnabled) // Check if log file is valid and verbose logging is enabled
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"); // Generate timestamp string in yyyy-MM-dd HH:mm:ss.fff format
                await logFile.WriteLineAsync($"[{timestamp}] [{level}] [{functionName}] {message}{(errorCode != RESULT.OK ? $" (Error code: {errorCode})" : "")}"); // Asynchronously write formatted log message to the log file
                await logFile.FlushAsync(); // Asynchronously flush the log file buffer to ensure immediate writing
            }
        }

        /**
         * @brief Writes a log message to the log file if verbose logging is enabled.
         *
         * @param logFile StreamWriter for the log file.
         * @param level Log level (e.g., "INFO", "WARNING", "ERROR").
         * @param functionName Name of the function where the log message originates.
         * @param message The log message string.
         * @param verboseLogEnabled Flag indicating whether verbose logging is enabled.
         * @param errorCode Optional FMOD.RESULT error code to include in the log message. Defaults to FMOD.RESULT.OK.
         *
         * @details
         * This static helper function writes a formatted log message to the provided StreamWriter
         * if verbose logging is enabled. The log message includes a timestamp, log level, function name, and the message itself.
         * If an FMOD error code is provided (and is not FMOD.RESULT.OK), it is also included in the log message.
         * This is a synchronous version of WriteLogMessageAsync, suitable for situations where asynchronous operation is not required or desired.
         */
        static void WriteLogMessage(StreamWriter logFile, string level, string functionName, string message, bool verboseLogEnabled, RESULT errorCode = RESULT.OK)
        {
            if (logFile != null && verboseLogEnabled) // Check if log file is valid and verbose logging is enabled
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"); // Generate timestamp string in yyyy-MM-dd HH:mm:ss.fff format
                logFile.WriteLine($"[{timestamp}] [{level}] [{functionName}] {message}{(errorCode != RESULT.OK ? $" (Error code: {errorCode})" : "")}"); // Write formatted log message to the log file
                logFile.Flush(); // Flush the log file buffer to ensure immediate writing
            }
        }

        /**
         * @brief Retrieves detailed information about a sub-sound from an FMOD Sound object.
         *
         * @param subSound The FMOD Sound object representing the sub-sound.
         * @param subSoundIndex The index of the sub-sound being processed.
         * @param verboseLogEnabled Flag indicating whether verbose logging is enabled.
         * @param logFile StreamWriter for logging messages.
         * @return SoundInfo A SoundInfo struct containing the retrieved sound information.
         *
         * @details
         * This static helper function retrieves various properties of a given FMOD sub-sound, including:
         * - Sound format (SOUND_FORMAT)
         * - Sound type (SOUND_TYPE)
         * - Sample rate
         * - Bits per sample
         * - Number of channels
         * - Sound length in bytes
         * - Sound length in milliseconds
         * - Sub-sound name (if available)
         * It uses FMOD API calls to get this information and logs the process and results if verbose logging is enabled.
         * It also handles FMOD errors and throws exceptions if critical API calls fail.
         */
        public static SoundInfo GetSoundInfo(FMOD.Sound subSound, int subSoundIndex, bool verboseLogEnabled, StreamWriter logFile)
        {
            SoundInfo info = new SoundInfo(); // Create a new SoundInfo struct to store retrieved sound information

            WriteLogMessage(logFile, "INFO", "GetSoundInfo", "Getting sound format...", verboseLogEnabled); // Log informational message about getting sound format
            var fmodSystemResult = subSound.getFormat(out info.soundType, out info.format, out info.channels, out info.bitsPerSample); // Get sound format, type, channels, and bits per sample from the FMOD sub-sound
            if (fmodSystemResult != FMOD.RESULT.OK) // Check if getting sound format was successful
            {
                WriteLogMessage(logFile, "ERROR", "GetSoundInfo", $"FMOD::Sound::getFormat failed for sub-sound {subSoundIndex}: {FMOD.Error.String(fmodSystemResult)}", verboseLogEnabled); // Log error message if getFormat fails
                CheckFMODResult(fmodSystemResult, $"FMOD::Sound::getFormat failed for sub-sound {subSoundIndex}"); // Check FMOD result and throw exception if it's an error
            }
            else // If getFormat was successful
            {
                string formatStr = info.format.ToString(); // Convert SOUND_FORMAT enum to string
                string soundTypeStr = info.soundType.ToString(); // Convert SOUND_TYPE enum to string
                WriteLogMessage(logFile, "INFO", "GetSoundInfo", $"FMOD::Sound::getFormat successful - Sound Type: {soundTypeStr}, Format: {formatStr}, Channels: {info.channels}, Bits Per Sample: {info.bitsPerSample}", verboseLogEnabled); // Log informational message about successful getFormat
            }

            WriteLogMessage(logFile, "INFO", "GetSoundInfo", "Getting default sound parameters...", verboseLogEnabled); // Log informational message about getting default sound parameters
            fmodSystemResult = subSound.getDefaults(out float defaultFrequency, out int defaultPriority); // Get default frequency and priority from the FMOD sub-sound
            if (fmodSystemResult != FMOD.RESULT.OK) // Check if getting default sound parameters was successful
            {
                WriteLogMessage(logFile, "ERROR", "GetSoundInfo", $"FMOD::Sound::getDefaults failed for sub-sound {subSoundIndex}: {FMOD.Error.String(fmodSystemResult)}", verboseLogEnabled); // Log error message if getDefaults fails
                CheckFMODResult(fmodSystemResult, $"FMOD::Sound::getDefaults failed for sub-sound {subSoundIndex}"); // Check FMOD result and throw exception if it's an error
            }
            else // If getDefaults was successful
            {
                WriteLogMessage(logFile, "INFO", "GetSoundInfo", $"FMOD::Sound::getDefaults successful - Default Frequency: {defaultFrequency}, Default Priority: {defaultPriority}", verboseLogEnabled); // Log informational message about successful getDefaults
            }

            info.sampleRate = (defaultFrequency > 0) ? (int)defaultFrequency : 44100; // Set sample rate, using default frequency if available, otherwise use 44100 Hz as default
            WriteLogMessage(logFile, "INFO", "GetSoundInfo", $"Final Sample Rate for WAV header: {info.sampleRate}", verboseLogEnabled); // Log informational message about the final sample rate used for WAV header

            WriteLogMessage(logFile, "INFO", "GetSoundInfo", "Getting sound length in bytes...", verboseLogEnabled); // Log informational message about getting sound length in bytes
            fmodSystemResult = subSound.getLength(out uint soundLengthBytes, FMOD.TIMEUNIT.PCMBYTES); // Get sound length in PCM bytes from the FMOD sub-sound
            if (fmodSystemResult != FMOD.RESULT.OK) // Check if getting sound length in bytes was successful
            {
                WriteLogMessage(logFile, "ERROR", "GetSoundInfo", $"FMOD::Sound::getLength (bytes) failed for sub-sound {subSoundIndex}: {FMOD.Error.String(fmodSystemResult)}", verboseLogEnabled); // Log error message if getLength (bytes) fails
                CheckFMODResult(fmodSystemResult, $"FMOD::Sound::getLength (bytes) failed for sub-sound {subSoundIndex}"); // Check FMOD result and throw exception if it's an error
            }
            else // If getLength (bytes) was successful
            {
                info.soundLengthBytes = soundLengthBytes; // Store the retrieved sound length in bytes
                WriteLogMessage(logFile, "INFO", "GetSoundInfo", $"FMOD::Sound::getLength (bytes) successful - Length: {info.soundLengthBytes} bytes", verboseLogEnabled); // Log informational message about successful getLength (bytes)
            }

            WriteLogMessage(logFile, "INFO", "GetSoundInfo", "Getting sound length in milliseconds...", verboseLogEnabled); // Log informational message about getting sound length in milliseconds
            fmodSystemResult = subSound.getLength(out uint lengthMs, FMOD.TIMEUNIT.MS); // Get sound length in milliseconds from the FMOD sub-sound
            if (fmodSystemResult != FMOD.RESULT.OK) // Check if getting sound length in milliseconds was successful
            {
                WriteLogMessage(logFile, "ERROR", "GetSoundInfo", $"FMOD::Sound::getLength (ms) failed for sub-sound {subSoundIndex}: {FMOD.Error.String(fmodSystemResult)}", verboseLogEnabled); // Log error message if getLength (ms) fails
                CheckFMODResult(fmodSystemResult, $"FMOD::Sound::getLength (ms) failed for sub-sound {subSoundIndex}"); // Check FMOD result and throw exception if it's an error
            }
            else // If getLength (ms) was successful
            {
                info.lengthMs = lengthMs; // Store the retrieved sound length in milliseconds
                WriteLogMessage(logFile, "INFO", "GetSoundInfo", $"FMOD::Sound::getLength (ms) successful - Length: {info.lengthMs} ms", verboseLogEnabled); // Log informational message about successful getLength (ms)
            }

            WriteLogMessage(logFile, "INFO", "GetSoundInfo", "Getting sub-sound name...", verboseLogEnabled); // Log informational message about getting sub-sound name
            fmodSystemResult = subSound.getName(out string subSoundName, 256); // Get sub-sound name from the FMOD sub-sound
            if (fmodSystemResult != FMOD.RESULT.OK && fmodSystemResult != FMOD.RESULT.ERR_TAGNOTFOUND) // Check if getting sub-sound name was successful or if tag was not found (tag not found is not an error)
            {
                WriteLogMessage(logFile, "WARNING", "GetSoundInfo", $"FMOD::Sound::getName failed or tag not found for sub-sound {subSoundIndex}: {FMOD.Error.String(fmodSystemResult)}", verboseLogEnabled); // Log warning message if getName fails or tag is not found
                info.subSoundName = ""; // Set sub-sound name to empty string if getName fails or tag is not found
            }
            else // If getName was successful or tag was not found
            {
                info.subSoundName = subSoundName ?? ""; // Store the retrieved sub-sound name, or empty string if it's null
                WriteLogMessage(logFile, "INFO", "GetSoundInfo", $"FMOD::Sound::getName successful - Name: {info.subSoundName}", verboseLogEnabled); // Log informational message about successful getName
            }
            info.defaultPriority = defaultPriority; // Store the default priority (already retrieved in getDefaults)

            return info; // Return the SoundInfo struct containing all retrieved sound information
        }

        /**
         * @brief Writes WAV data to the WAV file stream based on the sound format.
         *
         * @param subSound The FMOD Sound object representing the sub-sound.
         * @param wavFile The Stream to write the WAV file data to.
         * @param soundInfo The SoundInfo struct containing information about the sub-sound.
         * @param subSoundIndex The index of the sub-sound being processed (for logging purposes).
         * @param verboseLogEnabled Flag indicating whether verbose logging is enabled.
         * @param logFile StreamWriter for logging messages.
         * @param bw BinaryWriter for writing binary data to the WAV file stream.
         * @return Task Represents the asynchronous operation.
         *
         * @details
         * This private asynchronous helper function writes the actual audio data to the WAV file stream.
         * It uses a switch statement based on the sound format (soundInfo.format) to determine the appropriate
         * AudioProcessor.WriteAudioDataChunk method to use for writing data in the correct format.
         * It supports various PCM formats (PCM8, PCM16, PCM24, PCM32, PCMFLOAT) and handles unsupported formats with a warning.
         * The audio data writing operation is performed asynchronously using Task.Run to avoid blocking the main UI thread.
         */
        private async Task WriteWavData(FMOD.Sound subSound, Stream wavFile, SoundInfo soundInfo, int subSoundIndex, bool verboseLogEnabled, StreamWriter logFile, BinaryWriter bw)
        {
            bool writeSuccess = false;
            int chunkCount = 0;

            // Switch based on the sound format to call the appropriate WriteAudioDataChunk method
            switch (soundInfo.format)
            {
                case SOUND_FORMAT.PCM8: // If sound format is PCM8
                    writeSuccess = await Task.Run(() => AudioProcessor.WriteAudioDataChunk<byte>(subSound, wavFile, soundInfo.soundLengthBytes, subSoundIndex, ref chunkCount, verboseLogEnabled, logFile, bw)); // Asynchronously write PCM8 data chunks
                    break;
                case SOUND_FORMAT.PCM16: // If sound format is PCM16
                    writeSuccess = await Task.Run(() => AudioProcessor.WriteAudioDataChunk<short>(subSound, wavFile, soundInfo.soundLengthBytes, subSoundIndex, ref chunkCount, verboseLogEnabled, logFile, bw)); // Asynchronously write PCM16 data chunks
                    break;
                case SOUND_FORMAT.PCM24: // If sound format is PCM24
                    writeSuccess = await Task.Run(() => AudioProcessor.WriteAudioDataChunk<Pcm24>(subSound, wavFile, soundInfo.soundLengthBytes, subSoundIndex, ref chunkCount, verboseLogEnabled, logFile, bw)); // Asynchronously write PCM24 data chunks
                    break;
                case SOUND_FORMAT.PCM32: // If sound format is PCM32
                    writeSuccess = await Task.Run(() => AudioProcessor.WriteAudioDataChunk<int>(subSound, wavFile, soundInfo.soundLengthBytes, subSoundIndex, ref chunkCount, verboseLogEnabled, logFile, bw)); // Asynchronously write PCM32 data chunks
                    break;
                case SOUND_FORMAT.PCMFLOAT: // If sound format is PCMFLOAT
                    writeSuccess = await Task.Run(() => AudioProcessor.WriteAudioDataChunk<float>(subSound, wavFile, soundInfo.soundLengthBytes, subSoundIndex, ref chunkCount, verboseLogEnabled, logFile, bw)); // Asynchronously write PCMFLOAT data chunks
                    break;
                default: // If sound format is not supported
                    WriteLogMessage(logFile, "WARNING", "ProcessSubSound", $"Unsupported format detected: {soundInfo.format}. Processing as PCM16 (potentially incorrect).", verboseLogEnabled); // Log warning about unsupported format
                    FSB_BANK_Extractor_CS_GUI.LogMessage($" Warning: Unsupported format {soundInfo.format}, attempting to extract as PCM16."); // Log warning message to the application's log system
                    writeSuccess = await Task.Run(() => AudioProcessor.WriteAudioDataChunk<short>(subSound, wavFile, soundInfo.soundLengthBytes, subSoundIndex, ref chunkCount, verboseLogEnabled, logFile, bw)); // Asynchronously write data as PCM16 (potentially incorrect)
                    break;
            }

            if (!writeSuccess) // Check if writing WAV data was not successful
            {
                WriteLogMessage(logFile, "ERROR", "ProcessSubSound", $"Error writing audio data to WAV file for sub-sound {subSoundIndex}", verboseLogEnabled); // Log error message about failing to write audio data
                FSB_BANK_Extractor_CS_GUI.LogError($" Error writing audio data to WAV file for sub-sound {subSoundIndex}"); // Log error to the application's log system
                throw new Exception("Failed to write audio data to WAV file"); // Throw exception to indicate failure to write audio data
            }
        }

        /**
         * @brief Creates a WAV file, writes WAV header and data, and handles file operations.
         *
         * @param fullOutputPath The full path to the output WAV file.
         * @param soundInfo The SoundInfo struct containing information about the sub-sound.
         * @param subSoundIndex The index of the sub-sound being processed (for logging purposes).
         * @param logFile StreamWriter for logging messages.
         * @param verboseLogEnabled Flag indicating whether verbose logging is enabled.
         * @param subSound The FMOD Sound object representing the sub-sound.
         * @param baseFileName The base file name (stem of the input FSB file name).
         * @return Task Represents the asynchronous operation.
         *
         * @details
         * This private asynchronous helper function creates a WAV file at the specified fullOutputPath,
         * writes the WAV header using WriteWAVHeader, and then writes the audio data using WriteWavData.
         * It handles file creation, opening, writing header and data, and closing the file streams.
         * It also includes error handling and logging throughout the process.
         */
        private async Task CreateWavFile(string fullOutputPath, SoundInfo soundInfo, int subSoundIndex, StreamWriter logFile, bool verboseLogEnabled, FMOD.Sound subSound, string baseFileName)
        {
            FileStream wavFile = null; // Initialize FileStream for WAV file to null
            BinaryWriter bw = null; // Initialize BinaryWriter for WAV file to null
            try
            {
                wavFile = new FileStream(fullOutputPath, FileMode.Create, FileAccess.Write, FileShare.None); // Create new FileStream for writing WAV file, overwriting if exists, no sharing
                bw = new BinaryWriter(wavFile, Encoding.UTF8, leaveOpen: true); // Create BinaryWriter for writing binary data to the FileStream, using UTF8 encoding, leave stream open

                WriteLogMessage(logFile, "INFO", "ProcessSubSound", $"WAV file opened successfully: {fullOutputPath}", verboseLogEnabled); // Log informational message about successful WAV file opening

                if (!WriteWAVHeader(wavFile, soundInfo.sampleRate, soundInfo.channels, soundInfo.soundLengthBytes, soundInfo.bitsPerSample, soundInfo.format)) // Write WAV header to the WAV file stream
                {
                    WriteLogMessage(logFile, "ERROR", "ProcessSubSound", $"Error writing WAV header to file: {fullOutputPath}", verboseLogEnabled); // Log error message about failing to write WAV header
                    FSB_BANK_Extractor_CS_GUI.LogError($" Error writing WAV header to file: {fullOutputPath}"); // Log error to the application's log system
                    throw new Exception("Failed to write WAV header"); // Throw exception to indicate failure to write WAV header
                }
                WriteLogMessage(logFile, "INFO", "ProcessSubSound", "WAV header written successfully", verboseLogEnabled); // Log informational message about successful WAV header writing

                await WriteWavData(subSound, wavFile, soundInfo, subSoundIndex, verboseLogEnabled, logFile, bw); // Asynchronously write WAV data to the WAV file stream

                WriteLogMessage(logFile, "INFO", "ProcessSubSound", "Sub-sound processing finished successfully", verboseLogEnabled); // Log informational message about successful sub-sound processing
                FSB_BANK_Extractor_CS_GUI.LogMessage(" Status: Success"); // Log success status to the application's log system
            }
            catch (Exception) // Catch any exceptions that occur during WAV file creation and writing
            {
                throw; // Re-throw the caught exception to be handled in the calling method (ProcessSubSound), exception is already logged in ProcessSubSound, just re-throw
            }
            finally // Finally block to ensure resources are released regardless of exceptions
            {
                bw?.Close(); // Close BinaryWriter if it's not null
                wavFile?.Close(); // Close FileStream if it's not null
            }
        }

        /**
         * @brief Logs sub-sound information to the application's log system.
         *
         * @param soundInfo The SoundInfo struct containing information about the sub-sound.
         *
         * @details
         * This static helper function logs the key information about a sub-sound to the application's log system.
         * It logs the sub-sound name, number of channels, sample rate, and length in milliseconds.
         * This function is used to provide a summary of each processed sub-sound in the application's output log.
         */
        static void LogSubSoundInfo(SoundInfo soundInfo)
        {
            FSB_BANK_Extractor_CS_GUI.LogMessage($" Name: {soundInfo.subSoundName}"); // Log sub-sound name to the application's log system
            FSB_BANK_Extractor_CS_GUI.LogMessage($" Channels: {soundInfo.channels}"); // Log number of channels to the application's log system
            FSB_BANK_Extractor_CS_GUI.LogMessage($" Sample Rate: {soundInfo.sampleRate} Hz"); // Log sample rate to the application's log system
            FSB_BANK_Extractor_CS_GUI.LogMessage($" Length: {soundInfo.lengthMs} ms"); // Log length in milliseconds to the application's log system
        }

        /**
         * @brief Gets a unique full output file path for a sub-sound WAV file, handling potential name collisions.
         *
         * @param outputDirectoryPath The base output directory path.
         * @param baseFileName The base file name (stem of the input FSB file name).
         * @param soundInfo The SoundInfo struct containing information about the sub-sound.
         * @param subSoundIndex The index of the sub-sound being processed.
         * @param usedFileNames A HashSet containing file paths already used in the current extraction session to prevent overwrites.
         * @return string The unique full output file path for the WAV file.
         *
         * @details
         * This static helper function constructs a unique full output file path for a sub-sound WAV file.
         * It generates a base file name for each sub-sound, using the sub-sound name if available, or a combination
         * of the base file name and sub-sound index if the sub-sound name is not available.
         * It then checks if the generated file path already exists in the `usedFileNames` set. If a collision is detected,
         * it appends a numeric suffix (e.g., "_1", "_2") to the file name until a unique path is found.
         * The final unique path is added to the `usedFileNames` set and returned.
         */
        static string GetOutputFilePath(string outputDirectoryPath, string baseFileName, SoundInfo soundInfo, int subSoundIndex, HashSet<string> usedFileNames)
        {
            // Generate the initial base name for the output file, without extension
            string outputFileName = string.IsNullOrEmpty(soundInfo.subSoundName) ? SanitizeFileName($"{baseFileName}_{subSoundIndex}") : SanitizeFileName(soundInfo.subSoundName);

            // Construct the initial full path
            string finalPath = Path.Combine(outputDirectoryPath, $"{outputFileName}.wav");
            int counter = 1;

            // Check for file name collisions and append a counter if necessary
            while (usedFileNames.Contains(finalPath))
            {
                string tempFileName = $"{outputFileName}_{counter++}";
                finalPath = Path.Combine(outputDirectoryPath, $"{tempFileName}.wav");
            }

            // Add the guaranteed unique path to the tracking set for this session
            usedFileNames.Add(finalPath);

            return finalPath;
        }

        /**
         * @brief Prepares the output directory by creating it if it doesn't exist.
         *
         * @param outputDirectory The path to the output directory to prepare.
         *
         * @details
         * This static helper function checks if the specified output directory exists.
         * If the directory does not exist, it creates the directory and logs a message indicating the directory creation.
         * If the directory already exists, it does nothing.
         * This ensures that the output directory is ready before saving extracted WAV files.
         */
        static void PrepareOutputDirectory(string outputDirectory)
        {
            if (!Directory.Exists(outputDirectory)) // Check if the output directory does not exist
            {
                Directory.CreateDirectory(outputDirectory); // Create the output directory
                FSB_BANK_Extractor_CS_GUI.LogMessage(" Created directory: " + outputDirectory); // Log informational message about directory creation
            }
        }

        /**
         * @brief Opens a log file in the specified output directory and returns a StreamWriter for it.
         *
         * @param outputDirectory The path to the output directory where the log file will be created.
         * @param baseFileName The base file name (stem of the input FSB file name) to use for the log file name.
         * @param verboseLogEnabled Flag indicating whether verbose logging is enabled.
         * @return StreamWriter A StreamWriter for the opened log file, or null if log file creation fails or verbose logging is disabled.
         *
         * @details
         * This static helper function opens a log file in the specified output directory if verbose logging is enabled.
         * The log file name is generated using the base file name with a "_" prefix and ".log" extension.
         * It creates a new StreamWriter for the log file, overwriting any existing file with the same name.
         * It also writes initial log messages indicating log file opening and processing start for the input file.
         * If log file creation fails (e.g., due to permissions or file access issues), it logs an error message and returns null.
         * If verbose logging is not enabled, it directly returns null without creating or opening a log file.
         */
        static StreamWriter OpenLogFile(string outputDirectory, string baseFileName, bool verboseLogEnabled)
        {
            if (!verboseLogEnabled) return null; // If verbose logging is not enabled, return null without creating log file

            string logFilePath = Path.Combine(outputDirectory, "_" + baseFileName + ".log"); // Construct log file path by combining output directory, base file name, and "_".log" extension
            try
            {
                StreamWriter logFile = new StreamWriter(logFilePath, false, Encoding.UTF8); // Create new StreamWriter for the log file, overwriting if exists, using UTF8 encoding
                WriteLogMessage(logFile, "INFO", "Main", $"Log file opened: {logFilePath}", verboseLogEnabled); // Log informational message about successful log file opening
                WriteLogMessage(logFile, "INFO", "Main", $"Processing file: {Path.GetFullPath(baseFileName)}", verboseLogEnabled); // Log informational message about processing start for the input file
                FSB_BANK_Extractor_CS_GUI.LogMessage(" Log file path: " + logFilePath); // Log log file path to the application's log system
                return logFile; // Return StreamWriter for the opened log file
            }
            catch (Exception ex) // Catch any exceptions that occur during log file creation
            {
                FSB_BANK_Extractor_CS_GUI.LogError($" Error creating log file: {logFilePath} - {ex.Message}"); // Log error message with exception details
                return null; // Return null if log file creation fails
            }
        }

        /**
         * @brief Closes the log file StreamWriter if it's not null.
         *
         * @param logFile The StreamWriter for the log file to close.
         *
         * @details
         * This static helper function closes the provided StreamWriter for the log file if it is not null.
         * It first flushes the buffer to ensure all pending log messages are written to the file before closing.
         * It's important to close the StreamWriter to release file resources and ensure data is properly written to disk.
         * It handles null StreamWriter gracefully, doing nothing if the StreamWriter is already null.
         */
        static void CloseLogFile(StreamWriter logFile)
        {
            logFile?.Flush(); // Flush the log file buffer to write any pending data to the file
            logFile?.Close(); // Close the log file StreamWriter if it's not null
        }

        /**
         * @brief Logs the start of sub-sound processing to the log file and application log.
         *
         * @param subSoundIndex The index of the sub-sound being processed.
         * @param totalSubSounds The total number of sub-sounds in the FSB file.
         * @param logFile StreamWriter for logging messages.
         * @param verboseLogEnabled Flag indicating whether verbose logging is enabled.
         *
         * @details
         * This static helper function logs a message indicating the start of processing for a specific sub-sound.
         * It logs the sub-sound index and the total number of sub-sounds to the log file (if verbose logging is enabled)
         * and also prints a similar message to the application log for user feedback.
         * It adds a newline to the log file before writing the message to separate log entries visually.
         */
        static void LogProcessStart(int subSoundIndex, int totalSubSounds, StreamWriter logFile, bool verboseLogEnabled)
        {
            if (logFile != null) logFile.WriteLine(); // Add a newline to the log file for better readability
            WriteLogMessage(logFile, "INFO", "ProcessSubSound", $"Processing sub-sound {subSoundIndex + 1}/{totalSubSounds}", verboseLogEnabled); // Log informational message about starting sub-sound processing
        }

        /**
         * @brief Seeks to the beginning of the sub-sound data in the FMOD Sound object.
         *
         * @param subSound The FMOD Sound object representing the sub-sound.
         * @param subSoundIndex The index of the sub-sound being processed (for logging purposes).
         * @param logFile StreamWriter for logging messages.
         * @param verboseLogEnabled Flag indicating whether verbose logging is enabled.
         *
         * @details
         * This static helper function uses FMOD API to seek to the beginning of the audio data stream for a given sub-sound.
         * This is necessary to ensure that reading of audio data starts from the correct position for each sub-sound.
         * It calls FMOD::Sound::seekData(0) to reset the read pointer to the start of the sub-sound data.
         * It also logs the seek operation and any FMOD errors that occur during the seek process.
         */
        static void SeekSubSoundData(FMOD.Sound subSound, int subSoundIndex, StreamWriter logFile, bool verboseLogEnabled)
        {
            CheckFMODResult(subSound.seekData(0), $"FMOD::Sound::seekData failed for sub-sound {subSoundIndex}"); // Seek to the beginning of the sub-sound data in the FMOD Sound object, check for errors
            WriteLogMessage(logFile, "INFO", "ProcessSubSound", "FMOD::Sound::seekData successful", verboseLogEnabled); // Log informational message about successful seek operation
        }

        /**
         * @brief Processes a single sub-sound, extracts audio data, and saves it as a WAV file, organizing by FMOD tags.
         *
         * @param fmodSystem The FMOD System object to use for FMOD API calls.
         * @param subSound The FMOD Sound object representing the sub-sound to process.
         * @param totalSubSounds The total number of sub-sounds in the FSB file.
         * @param baseFileName The base file name (stem of the input FSB file name).
         * @param outputDirectoryPath The path to the output directory where WAV file will be saved.
         * @param verboseLogEnabled Flag indicating whether verbose logging is enabled.
         * @param logFile StreamWriter for logging messages.
         * @param subSoundIndex The index of the sub-sound being processed.
         * @param usedFileNames A HashSet containing file paths already used in the current extraction session to prevent overwrites.
         * @return Task Represents the asynchronous operation.
         *
         * @details
         * This private asynchronous helper function orchestrates the processing of a single sub-sound.
         * It attempts to read a "language" tag from the sub-sound to create a subdirectory for organizational purposes.
         * If a tag is found, it creates a corresponding folder. It then generates a unique output file path within the determined directory,
         * logs sub-sound information, and creates the WAV file.
         */
        private async Task ProcessSubSound(FMOD.System fmodSystem, FMOD.Sound subSound, int totalSubSounds, string baseFileName, string outputDirectoryPath, bool verboseLogEnabled, StreamWriter logFile, int subSoundIndex, HashSet<string> usedFileNames)
        {
            LogProcessStart(subSoundIndex, totalSubSounds, logFile, verboseLogEnabled); // Log the start of sub-sound processing
            SeekSubSoundData(subSound, subSoundIndex, logFile, verboseLogEnabled); // Seek to the beginning of sub-sound data

            SoundInfo soundInfo = GetSoundInfo(subSound, subSoundIndex, verboseLogEnabled, logFile); // Get detailed information about the sub-sound

            // Determine final output directory based on FMOD tag "language"
            string finalOutputDirectory = outputDirectoryPath;
            FMOD.TAG tag;
            // Attempt to get the "language" tag. The index is 0 as we are looking for the first occurrence.
            if (subSound.getTag("language", 0, out tag) == FMOD.RESULT.OK)
            {
                // Marshal the tag data (which is an IntPtr) to a string
                string language = Marshal.PtrToStringAnsi(tag.data);
                if (!string.IsNullOrEmpty(language))
                {
                    string languageFolder = Path.Combine(outputDirectoryPath, SanitizeFileName(language));
                    // Create the subdirectory if it doesn't exist
                    PrepareOutputDirectory(languageFolder);
                    finalOutputDirectory = languageFolder;
                }
            }

            string fullOutputPath = GetOutputFilePath(finalOutputDirectory, baseFileName, soundInfo, subSoundIndex, usedFileNames); // Get a unique output file path for the WAV file

            FSB_BANK_Extractor_CS_GUI.LogMessage($"\r\n Processing sub-sound {subSoundIndex + 1}/{totalSubSounds}:"); // Log message to application log indicating sub-sound processing start
            LogSubSoundInfo(soundInfo); // Log detailed information about the sub-sound to the application log

            try
            {
                await CreateWavFile(fullOutputPath, soundInfo, subSoundIndex, logFile, verboseLogEnabled, subSound, baseFileName); // Asynchronously create WAV file and write header and data
            }
            catch (Exception e) // Catch any exceptions that occur during WAV file creation and writing
            {
                WriteLogMessage(logFile, "ERROR", "ProcessSubSound", $"Exception during sub-sound processing: {e.Message}", verboseLogEnabled); // Log error message with exception details
                FSB_BANK_Extractor_CS_GUI.LogError($" Exception during sub-sound processing: {e.Message}"); // Log error to the application's log system
                throw; // Re-throw the exception to be caught in the batch processing loop
            }
        }

        #endregion

        #region GUI event handlers and functions

        /**
         * @brief Event handler for the "Add Files" button click.
         *
         * @param sender The source of the event.
         * @param e The EventArgs instance containing the event data.
         *
         * @details
         * Opens an OpenFileDialog to allow users to select FSB or BANK files to add to the processing list.
         * Allows multiple file selection and filters for FSB and BANK files.
         * Adds selected files to the FileList and listViewFiles if they are not already present.
         */
        private void Button_AddFiles_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog(); // Create a new OpenFileDialog instance
            openFileDialog.Multiselect = true; // Allow multiple files to be selected in the dialog
            // Set filter for file types to show in the dialog: FSB/Bank files, FSB files, Bank files, and All files
            openFileDialog.Filter = "FSB/Bank Files (*.fsb;*.bank)|*.fsb;*.bank|FSB Files (*.fsb)|*.fsb|Bank Files (*.bank)|*.bank|All files (*.*)|*.*";
            openFileDialog.Title = "Add FSB/*.bank Files"; // Set the title of the OpenFileDialog

            if (openFileDialog.ShowDialog() == DialogResult.OK) // Show the OpenFileDialog and check if the user clicked OK
            {
                foreach (string filePath in openFileDialog.FileNames) // Iterate through all selected file paths
                {
                    if (!FileList.Contains(filePath)) // Check if the file path is not already in the FileList
                    {
                        FileList.Add(filePath); // Add the file path to the FileList
                        ListViewItem item = new ListViewItem(Path.GetFileName(filePath)); // Create a new ListViewItem with the file name
                        item.SubItems.Add("Pending"); // Add a "Pending" subitem to indicate processing status
                        item.Tag = filePath; // Store the full file path in the Tag property of the ListViewItem
                        listViewFiles.Items.Add(item); // Add the ListViewItem to the listViewFiles control
                    }
                }
            }
        }

        /**
         * @brief Event handler for the "Add Folder" button click.
         *
         * @param sender The source of the event.
         * @param e The EventArgs instance containing the event data.
         *
         * @details
         * Opens a FolderBrowserDialog to allow users to select a folder to search for FSB and BANK files.
         * Searches the selected folder and all subdirectories for *.fsb and *.bank files.
         * Adds found files to the FileList and listViewFiles if they are not already present.
         */
        private void Button_AddFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog(); // Create a new FolderBrowserDialog instance
            folderBrowserDialog.Description = "Select folder containing FSB/*.bank files"; // Set the description for the FolderBrowserDialog

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK) // Show the FolderBrowserDialog and check if the user clicked OK
            {
                string folderPath = folderBrowserDialog.SelectedPath; // Get the selected folder path from the FolderBrowserDialog
                // Search for *.fsb files in the selected folder and all subdirectories
                string[] fsbFiles = Directory.GetFiles(folderPath, "*.fsb", SearchOption.AllDirectories);
                // Search for *.bank files in the selected folder and all subdirectories
                string[] bankFiles = Directory.GetFiles(folderPath, "*.bank", SearchOption.AllDirectories);
                string[] files = fsbFiles.Concat(bankFiles).ToArray(); // Combine the arrays of FSB and BANK files

                foreach (string filePath in files) // Iterate through all found file paths
                {
                    if (!FileList.Contains(filePath)) // Check if the file path is not already in the FileList
                    {
                        FileList.Add(filePath); // Add the file path to the FileList
                        ListViewItem item = new ListViewItem(Path.GetFileName(filePath)); // Create a new ListViewItem with the file name
                        item.SubItems.Add("Pending"); // Add a "Pending" subitem to indicate processing status
                        item.Tag = filePath; // Store the full file path in the Tag property of the ListViewItem
                        listViewFiles.Items.Add(item); // Add the ListViewItem to the listViewFiles control
                    }
                }
            }
        }

        /**
         * @brief Event handler for the "Remove Selected Files" button click.
         *
         * @param sender The source of the event.
         * @param e The EventArgs instance containing the event data.
         *
         * @details
         * Removes the selected files from the listViewFiles and the FileList.
         * Iterates through the selected items in listViewFiles, removes the corresponding file path from FileList,
         * and then removes the item from listViewFiles.
         */
        private void Button_RemoveSelectedFiles_Click(object sender, EventArgs e)
        {
            if (listViewFiles.SelectedItems.Count > 0) // Check if any items are selected in listViewFiles
            {
                foreach (ListViewItem selectedItem in listViewFiles.SelectedItems) // Iterate through all selected items in listViewFiles
                {
                    string filePath = selectedItem.Tag as string; // Get the file path from the Tag property of the selected ListViewItem
                    if (filePath != null) // Check if the file path is valid
                    {
                        FileList.Remove(filePath); // Remove the file path from the FileList
                    }
                    listViewFiles.Items.Remove(selectedItem); // Remove the selected ListViewItem from listViewFiles
                }
            }
        }

        /**
         * @brief Event handler for the "Clear File List" button click.
         *
         * @param sender The source of the event.
         * @param e The EventArgs instance containing the event data.
         *
         * @details
         * Clears all files from the FileList and listViewFiles.
         * Removes all items from listViewFiles and clears the FileList collection.
         */
        private void Button_ClearFileList_Click(object sender, EventArgs e)
        {
            FileList.Clear(); // Clear the FileList collection
            listViewFiles.Items.Clear(); // Clear all items from the listViewFiles control
        }

        /**
         * @brief Event handler for the "Custom Output Directory" radio button CheckedChanged event.
         *
         * @param sender The source of the event.
         * @param e The EventArgs instance containing the event data.
         *
         * @details
         * Enables or disables the output directory text box and browse button based on the checked state of the
         * "Custom Output Directory" radio button. When checked, the text box and browse button are enabled,
         * allowing users to specify a custom output directory. When unchecked, they are disabled, and output directory is determined by other options.
         */
        private void RadioButton_OutputDirCustom_CheckedChanged(object sender, EventArgs e)
        {
            textBoxOutputDirPath.Enabled = radioButtonOutputDirCustom.Checked; // Enable or disable textBoxOutputDirPath based on radioButtonOutputDirCustom.Checked
            buttonOutputDirBrowse.Enabled = radioButtonOutputDirCustom.Checked; // Enable or disable buttonOutputDirBrowse based on radioButtonOutputDirCustom.Checked
        }

        /**
         * @brief Event handler for the "Browse Output Directory" button click.
         *
         * @param sender The source of the event.
         * @param e The EventArgs instance containing the event data.
         *
         * @details
         * Opens a FolderBrowserDialog to allow users to select a custom output directory.
         * If the user selects a directory and clicks OK, the selected path is set to the textBoxOutputDirPath.
         */
        private void Button_OutputDirBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog(); // Create a new FolderBrowserDialog instance
            folderBrowserDialog.Description = "Select output folder"; // Set the description for the FolderBrowserDialog

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK) // Show the FolderBrowserDialog and check if the user clicked OK
            {
                textBoxOutputDirPath.Text = folderBrowserDialog.SelectedPath; // Set the text of textBoxOutputDirPath to the selected folder path
                textBoxOutputDirPath.SelectionStart = textBoxOutputDirPath.TextLength; // Set the selection start to the end of the text in textBoxOutputDirPath, placing cursor at the end
            }
        }

        #region *.bank file processing related functions

        /**
         * @brief Searches for the "FSB5" signature within a BinaryReader stream.
         *
         * @param reader The BinaryReader stream to search within.
         * @return bool True if the "FSB5" signature is found, false otherwise.
         *
         * @details
         * This function searches for the "FSB5" signature (4 bytes) within the provided BinaryReader stream.
         * It reads bytes from the stream and checks if they match the "FSB5" signature.
         * If the signature is found, it returns true and leaves the stream position at the start of the signature.
         * If the signature is not found after searching the entire stream, it returns false and resets the stream position to the original position.
         * This function is used to identify embedded FSB files within BANK files.
         */
        private bool FindFSB5Signature(BinaryReader reader)
        {
            long startPosition = reader.BaseStream.Position; // Store the starting position of the stream

            while (reader.BaseStream.Position < reader.BaseStream.Length - 3) // Loop until near the end of the stream (to avoid reading past end for 4-byte signature)
            {
                byte[] signature = reader.ReadBytes(4); // Read 4 bytes from the stream
                if (Encoding.ASCII.GetString(signature) == "FSB5") // Check if the read bytes match the "FSB5" signature
                {
                    reader.BaseStream.Seek(-4, SeekOrigin.Current); // Move the stream position back 4 bytes to the start of the "FSB5" signature
                    return true; // Return true to indicate signature found
                }
                reader.BaseStream.Seek(-3, SeekOrigin.Current); // Move the stream position back 3 bytes to search for the signature starting from the next byte
            }
            reader.BaseStream.Position = startPosition; // Reset the stream position to the original starting position if signature not found
            return false; // Return false to indicate signature not found
        }

        /**
         * @brief Extracts FSB files embedded within a BANK file.
         *
         * @param bankFilePath The path to the BANK file to extract FSBs from.
         * @return List<string> A list of temporary file paths for the extracted FSB files.
         *
         * @details
         * This function extracts FSB files that are embedded within a BANK file.
         * It reads the BANK file, searches for "FSB5" signatures, and extracts the FSB data for each signature found.
         * Extracted FSB files are saved as temporary files in the system's temporary folder.
         * The function returns a list of paths to these temporary FSB files.
         * If any error occurs during processing, it logs an error message and continues to the next FSB file (if any).
         */
        private List<string> ExtractFSBsFromBankFile(string bankFilePath)
        {
            List<string> tempFsbFiles = new List<string>(); // Initialize list to store temporary FSB file paths
            string baseBankFileName = Path.GetFileNameWithoutExtension(bankFilePath); // Get the file name of the BANK file without extension
            string tempPath = Path.GetTempPath(); // Get the system's temporary folder path

            try
            {
                using (FileStream fileStream = new FileStream(bankFilePath, FileMode.Open, FileAccess.Read)) // Open the BANK file for reading in a using block for automatic disposal
                using (BinaryReader reader = new BinaryReader(fileStream)) // Create a BinaryReader to read from the BANK file stream in a using block for automatic disposal
                {
                    int fsbCount = 0; // Initialize counter for FSB files extracted from BANK file
                    while (FindFSB5Signature(reader)) // Loop as long as "FSB5" signature is found in the BANK file
                    {
                        fsbCount++; // Increment FSB file counter for each FSB file found
                        string tempFileName; // Declare variable for temporary file name

                        if (fsbCount > 1) // If it's not the first FSB file extracted from the BANK
                        {
                            // Add sequence number to the file name from the second *.fsb file onwards (prevent file name conflicts)
                            tempFileName = $"{baseBankFileName}_{fsbCount}.fsb";
                        }
                        else // If it's the first FSB file extracted from the BANK
                        {
                            // Use the *.bank file name as is for the first *.fsb file
                            tempFileName = $"{baseBankFileName}.fsb";
                        }

                        // Create temporary *.fsb file path
                        string tempFilePath = Path.Combine(tempPath, tempFileName);

                        tempFsbFiles.Add(tempFilePath); // Add the temporary file path to the list of temporary FSB files

                        try
                        {
                            using (FileStream tempFsbStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write)) // Create a temporary FSB file for writing in a using block for automatic disposal
                            using (BinaryWriter writer = new BinaryWriter(tempFsbStream)) // Create a BinaryWriter to write to the temporary FSB file stream in a using block for automatic disposal
                            {
                                // Read FSB header information (based on QuickBMS script)
                                string fsbSign = Encoding.ASCII.GetString(reader.ReadBytes(4)); // FSB_SIGN (must always be "FSB5")
                                uint version = reader.ReadUInt32(); // Version of FSB format
                                uint numSamples = reader.ReadUInt32(); // Number of samples in FSB
                                uint shdrSize = reader.ReadUInt32(); // Size of sample header data
                                uint nameSize = reader.ReadUInt32(); // Size of sample name data
                                uint dataSize = reader.ReadUInt32(); // Size of audio data

                                // Calculate FSB file size based on header information
                                uint fsbFileSize = 0x3C + shdrSize + nameSize + dataSize; // 0x3C = 60 (FSB basic header size)

                                // Read FSB data and write to temporary file
                                reader.BaseStream.Seek(-24, SeekOrigin.Current); // Move back to the header start position (header size: 4 + 4*5 = 24 bytes)
                                byte[] fsbData = reader.ReadBytes((int)fsbFileSize); // Read the entire FSB file data
                                writer.Write(fsbData); // Write the FSB file data to the temporary FSB file
                            }
                        }
                        catch (Exception ex) // Catch any exceptions that occur during temporary FSB file creation or writing
                        {
                            FSB_BANK_Extractor_CS_GUI.LogError($" Error creating temporary *.fsb file: {tempFilePath} - {ex.Message}"); // Log error message with exception details
                            // Remove from temporary file list and delete if error occurs
                            tempFsbFiles.Remove(tempFilePath); // Remove the temporary file path from the list
                            if (File.Exists(tempFilePath)) // Check if the temporary file exists
                            {
                                File.Delete(tempFilePath); // Delete the temporary file if it exists
                            }
                            continue; // Continue to the next iteration of the loop (next FSB file in BANK)
                        }
                        // OFFSET update for next *.fsb file search is handled by stream pointer movement in FindFSB5Signature function
                    }
                }
            }
            catch (Exception ex) // Catch any exceptions that occur during BANK file processing
            {
                FSB_BANK_Extractor_CS_GUI.LogError($" Error processing *.bank file: {bankFilePath} - {ex.Message}"); // Log error message with exception details
            }
            return tempFsbFiles; // Return the list of temporary FSB file paths
        }

        #endregion

        /**
         * @brief Event handler for the "Batch Extract" button click.
         *
         * @param sender The source of the event.
         * @param e The EventArgs instance containing the event data.
         *
         * @details
         * Starts the batch extraction process for all files in the FileList.
         * Validates if FileList is not empty and output directory is specified.
         * Disables UI elements during batch processing, clears log textbox, and initializes progress labels.
         * Iterates through FileList, processes each file using ProcessFile, and updates UI and progress.
         * Enables UI elements back after processing is complete or an exception occurs.
         * Shows a message box indicating completion or error after batch processing.
         */
        private async void Button_BatchExtract_Click(object sender, EventArgs e) // async void -> async void changed
        {
            if (FileList.Count == 0) // Check if the FileList is empty
            {
                MessageBox.Show("Please add *.fsb or *.bank files.", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information); // Show a message box if FileList is empty
                return; // Exit the method if FileList is empty
            }

            string outputDirectoryPath = GetOutputDirectoryPath(); // Get the output directory path based on user selection
            if (string.IsNullOrEmpty(outputDirectoryPath)) // Check if the output directory path is null or empty
            {
                MessageBox.Show("Please specify the output folder.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); // Show a message box if output directory path is not specified
                return; // Exit the method if output directory path is not specified
            }

            bool verboseLogEnabled = checkBoxVerboseLog.Checked; // Get the value of verboseLogEnabled from the checkBoxVerboseLog control
            textBoxLog.Clear(); // Clear the content of the textBoxLog control
            UpdateCombinedProgressLabel(0, FileList.Count, 0, 0); // Initialize the combined progress label
            DisableUIForBatchProcess(); // Disable UI elements to prevent user interaction during batch processing

            FSB_BANK_Extractor_CS_GUI.LogMessage($"\r\n ===== Batch Extraction Process Started =====\r\n"); // Log message to application log indicating batch extraction process started
            FSB_BANK_Extractor_CS_GUI.LogMessage($" Starting processing a total of {FileList.Count} files."); // Log message to application log indicating total number of files to process

            try
            {
                using (FMODSystem fmodSystem = new FMODSystem()) // Create and initialize FMOD system in a using block for automatic disposal
                {
                    for (int i = 0; i < FileList.Count; i++) // Iterate through each file path in the FileList
                    {
                        string inputFilePath = FileList[i]; // Get the current input file path from FileList
                        await ProcessFile(fmodSystem, inputFilePath, outputDirectoryPath, verboseLogEnabled, i, FileList.Count); // Asynchronously process the current file
                    }
                }

                UpdateCombinedProgressLabel(FileList.Count, FileList.Count, FileList.Count, FileList.Count); // Update the combined progress label to indicate completion for all files and sub-sounds
                FSB_BANK_Extractor_CS_GUI.LogMessage($"\r\n\r\n\r\n ===== Batch Extraction Process Completed ====="); // Log message to application log indicating batch extraction process completed
                MessageBox.Show($"Processing completed for a total of {FileList.Count} files!", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information); // Show a message box indicating batch processing completed successfully
            }
            catch (Exception ex) // Catch any exceptions that occur during batch processing
            {
                HandleBatchProcessException(ex); // Handle the batch process exception
            }
            finally
            {
                EnableUIForBatchProcess(); // Enable UI elements back after batch processing is complete or an error occurs
            }
        }

        /**
         * @brief Processes a single FSB or BANK file, extracting sub-sounds and saving them as WAV files.
         *
         * @param fmodSystem The FMOD System object to use for FMOD API calls.
         * @param inputFilePath The path to the input FSB or BANK file.
         * @param outputDirectoryPath The base output directory path.
         * @param verboseLogEnabled Flag indicating whether verbose logging is enabled.
         * @param fileIndex The index of the current file being processed in the batch.
         * @param totalFiles The total number of files in the batch.
         * @return Task Represents the asynchronous operation.
         *
         * @details
         * This private asynchronous helper function processes a single FSB or BANK file.
         * It determines if the input file is a BANK file and extracts FSB files from it if necessary.
         * It prepares the output directory, opens a log file (if verbose logging is enabled), and then iterates through
         * the FSB files (either the original input FSB or extracted FSBs from BANK) and processes each one using ProcessCurrentFile.
         * It updates the file status in the ListView, logs messages to the application log, and handles exceptions during file processing.
         * A new HashSet is created for each file to track used output filenames and prevent overwrites.
         */
        private async Task ProcessFile(FMODSystem fmodSystem, string inputFilePath, string outputDirectoryPath, bool verboseLogEnabled, int fileIndex, int totalFiles)
        {
            string baseFileName = Path.GetFileNameWithoutExtension(inputFilePath); // Get the file name without extension from the input file path
            string outputDirectory = Path.Combine(outputDirectoryPath, baseFileName); // Create output directory path by combining base output directory path and base file name

            UpdateFileStatus(inputFilePath, "Processing..."); // Update the status of the input file in the listViewFiles to "Processing..."
            FSB_BANK_Extractor_CS_GUI.LogMessage($"\r\n\r\n\r\n --- Processing File ({fileIndex + 1}/{totalFiles}): '{Path.GetFileName(inputFilePath)}' Start ---\r\n"); // Log message to application log indicating file processing started

            StreamWriter logFile = null; // Initialize StreamWriter for log file to null
            List<string> currentFileListToProcess = new List<string>(); // Initialize list to store file paths to process for the current input file
            var usedFileNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase); // Tracks used output filenames for this file to prevent overwrites.

            try
            {
                currentFileListToProcess = GetFileListToProcess(inputFilePath); // Get the list of files to process for the current input file (either original file or extracted FSBs from BANK)
                PrepareOutputDirectory(outputDirectory); // Prepare the output directory for saving WAV files
                logFile = OpenLogFile(outputDirectory, baseFileName, verboseLogEnabled); // Open log file if verbose logging is enabled

                foreach (string currentFilePath in currentFileListToProcess) // Iterate through each file path in the list to process
                {
                    await ProcessCurrentFile(fmodSystem, currentFilePath, outputDirectory, verboseLogEnabled, logFile, baseFileName, fileIndex, totalFiles, usedFileNames); // Asynchronously process the current file
                    DeleteTempFsbFile(currentFilePath); // Delete temporary FSB file if it was extracted from a BANK file
                }

                UpdateFileStatus(inputFilePath, "Completed"); // Update the status of the input file in the listViewFiles to "Completed"
            }
            catch (Exception ex) // Catch any exceptions that occur during file processing
            {
                UpdateFileStatus(inputFilePath, "Error"); // Update the status of the input file in the listViewFiles to "Error"
                UpdateCombinedProgressLabel(fileIndex + 1, totalFiles, 0, 0); // Update the combined progress label to indicate error for the current file
                FSB_BANK_Extractor_CS_GUI.LogError($"\r\n ===== Error during file processing for '{Path.GetFileName(inputFilePath)}' ===== "); // Log error message to application log indicating error during file processing
                FSB_BANK_Extractor_CS_GUI.LogError(" Exception caught: " + ex.Message); // Log error message with exception details to application log
                FSB_BANK_Extractor_CS_GUI.LogError("\r\n An error occurred during file processing."); // Log generic error message to application log
                FSB_BANK_Extractor_CS_GUI.LogMessage($" --- Processing File ({fileIndex + 1}/{totalFiles}): '{Path.GetFileName(inputFilePath)}' Error ---"); // Log error message to application log indicating file processing error
                throw; // Re-throw the exception to be caught by BatchExtractClick
            }
            finally
            {
                CloseLogFile(logFile); // Close log file in finally block to ensure it's always closed
                await Task.Delay(50); // Add a small delay for UI responsiveness
            }
        }

        /**
         * @brief Processes a single FSB file, extracting sub-sounds and saving them as WAV files.
         *
         * @param fmodSystem The FMOD System object to use for FMOD API calls.
         * @param currentFilePath The path to the current FSB file being processed.
         * @param outputDirectory The output directory path for saving WAV files.
         * @param verboseLogEnabled Flag indicating whether verbose logging is enabled.
         * @param logFile StreamWriter for logging messages.
         * @param baseFileName The base file name (stem of the input FSB file name).
         * @param fileIndex The index of the current file being processed in the batch.
         * @param totalFiles The total number of files in the batch.
         * @param usedFileNames A HashSet containing file paths already used in the current extraction session to prevent overwrites.
         * @return Task Represents the asynchronous operation.
         *
         * @details
         * This private asynchronous helper function processes a single FSB file.
         * It loads the FSB file using FMODSound, gets the number of sub-sounds, and then iterates through each sub-sound
         * and processes it using ProcessSubSound. It updates the combined progress label for each sub-sound processed.
         * If no sub-sounds are found in the FSB file, it calls HandleNoSubSounds to handle the case of empty FSB files.
         */
        private async Task ProcessCurrentFile(FMODSystem fmodSystem, string currentFilePath, string outputDirectory, bool verboseLogEnabled, StreamWriter logFile, string baseFileName, int fileIndex, int totalFiles, HashSet<string> usedFileNames)
        {
            using (FMODSound soundWrapper = new FMODSound(fmodSystem.Get(), currentFilePath)) // Create and load FMOD sound from current file path in a using block for automatic disposal
            {
                FMOD.Sound sound = soundWrapper.Get(); // Get the FMOD Sound object from the wrapper
                CheckFMODResult(sound.getNumSubSounds(out int numSubSounds), "FMOD::Sound::getNumSubSounds failed"); // Get the number of sub-sounds in the FMOD Sound object, check for errors

                if (numSubSounds > 0) // Check if there are sub-sounds in the FMOD Sound object
                {
                    UpdateCombinedProgressLabel(fileIndex + 1, totalFiles, 0, numSubSounds); // Update the combined progress label to indicate start of sub-sound processing

                    for (int subSoundIndex = 0; subSoundIndex < numSubSounds; ++subSoundIndex) // Iterate through each sub-sound
                    {
                        FMOD.Sound subSound = GetSubSound(sound, subSoundIndex); // Get the FMOD sub-sound object for the current sub-sound index
                        if (subSound.hasHandle()) // Check if the subSound object has a valid handle
                        {
                            await ProcessSubSound(fmodSystem.Get(), subSound, numSubSounds, baseFileName, outputDirectory, verboseLogEnabled, logFile, subSoundIndex, usedFileNames); // Asynchronously process the sub-sound
                            subSound.clearHandle(); // Manually clear handle here, outside using block, as GetSubSound does not create a managed wrapper.
                        }
                        else // If subSound object does not have a valid handle
                        {
                            FSB_BANK_Extractor_CS_GUI.LogError($" Error: subSound is invalid after getSubSound for sub-sound {subSoundIndex}"); // Log error message indicating invalid subSound object
                        }

                        UpdateCombinedProgressLabel(fileIndex + 1, totalFiles, subSoundIndex + 1, numSubSounds); // Update the combined progress label to indicate progress of sub-sound processing
                    }

                    UpdateCombinedProgressLabel(fileIndex + 1, totalFiles, numSubSounds, numSubSounds); // Update the combined progress label to indicate completion of sub-sound processing for the current file
                    FSB_BANK_Extractor_CS_GUI.LogMessage($"\r\n --- Processing File ({fileIndex + 1}/{totalFiles}): '{Path.GetFileName(currentFilePath)}' Completed ---"); // Log message to application log indicating file processing completed
                }
                else // If no sub-sounds are found in the FMOD Sound object
                {
                    HandleNoSubSounds(fileIndex, totalFiles, currentFilePath); // Handle the case where no sub-sounds are found in the FSB file
                }
            } // using soundWrapper - Dispose soundWrapper here, releasing FMOD Sound object.
        }

        /**
         * @brief Gets a specific sub-sound from an FMOD Sound object.
         *
         * @param sound The parent FMOD Sound object.
         * @param subSoundIndex The index of the sub-sound to retrieve.
         * @return FMOD.Sound The retrieved FMOD sub-sound object.
         *
         * @details
         * This private helper function retrieves a specific sub-sound from a parent FMOD Sound object based on its index.
         * It uses FMOD::Sound::getSubSound to get the sub-sound and checks for FMOD errors.
         * If an error occurs, it logs an error message to the application log.
         * The caller is responsible for disposing of the returned FMOD.Sound object when it's no longer needed.
         * Note: This function returns a raw FMOD.Sound object, not wrapped in FMODSound class.
         */
        private FMOD.Sound GetSubSound(FMOD.Sound sound, int subSoundIndex)
        {
            FMOD.Sound subSound = new FMOD.Sound(); // Create a new FMOD.Sound object to store the sub-sound
            FMOD.RESULT result = sound.getSubSound(subSoundIndex, out subSound); // Get the sub-sound from the parent FMOD Sound object based on index

            if (result != FMOD.RESULT.OK) // Check if getting the sub-sound was successful
            {
                FSB_BANK_Extractor_CS_GUI.LogError($" FMOD::Sound::getSubSound failed for sub-sound {subSoundIndex}: {FMOD.Error.String(result)}"); // Log error message if getSubSound fails
            }
            return subSound; // Return the retrieved FMOD sub-sound object (caller is responsible for disposing)
        }

        /**
         * @brief Handles the case where no sub-sounds are found in an audio file.
         *
         * @param fileIndex The index of the current file being processed in the batch.
         * @param totalFiles The total number of files in the batch.
         * @param currentFilePath The path to the current file being processed.
         *
         * @details
         * This private helper function is called when no sub-sounds are found in an audio file (FSB or BANK).
         * It updates the combined progress label to indicate that the file processing is completed with 0 sub-sounds.
         * It also logs a message to the application log and to the console output indicating that no sub-sounds were found.
         */
        private void HandleNoSubSounds(int fileIndex, int totalFiles, string currentFilePath)
        {
            UpdateCombinedProgressLabel(fileIndex + 1, totalFiles, 0, 0); // Update the combined progress label to indicate completion with 0 sub-sounds
            FSB_BANK_Extractor_CS_GUI.LogMessage(" No sub-sounds found in the audio file."); // Log message to application log indicating no sub-sounds found
            UpdateCombinedProgressLabel(fileIndex + 1, totalFiles, 0, 0); // Update the combined progress label again (redundant, but kept for consistency with other completion paths)
            FSB_BANK_Extractor_CS_GUI.LogMessage($" --- Processing File ({fileIndex + 1}/{totalFiles}): '{Path.GetFileName(currentFilePath)}' Completed ---"); // Log message to application log indicating file processing completed (even with no sub-sounds)
        }

        /**
         * @brief Deletes a temporary FSB file if it exists and its path starts with the temporary path.
         *
         * @param currentFilePath The path to the file to check and potentially delete.
         *
         * @details
         * This private helper function checks if the given file path points to a temporary FSB file.
         * It determines if a file is temporary by checking if its path starts with the system's temporary path.
         * If the file is a temporary FSB file and it exists, it attempts to delete the file.
         * It includes error handling to catch exceptions during file deletion and logs error messages if deletion fails.
         */
        private void DeleteTempFsbFile(string currentFilePath)
        {
            if (IsTempFsbFile(currentFilePath) && File.Exists(currentFilePath)) // Check if the file is a temporary FSB file and if it exists
            {
                try
                {
                    File.Delete(currentFilePath); // Delete the temporary FSB file
                    // LogMessage($" 임시 *.fsb 파일 삭제: {currentFilePath}"); // Commented out log message in Korean (previous version)
                }
                catch (Exception ex) // Catch any exceptions that occur during temporary FSB file deletion
                {
                    FSB_BANK_Extractor_CS_GUI.LogError($" Error deleting temporary *.fsb file: {currentFilePath} - {ex.Message}"); // Log error message with exception details
                }
            }
        }

        /**
         * @brief Checks if a file path points to a temporary FSB file.
         *
         * @param filePath The file path to check.
         * @return bool True if the file path points to a temporary FSB file, false otherwise.
         *
         * @details
         * This private static helper function determines if a given file path points to a temporary FSB file.
         * It checks if the file path starts with the system's temporary path (e.g., "C:\Users\Username\AppData\Local\Temp" on Windows).
         * This is used to identify temporary FSB files that were extracted from BANK files and need to be deleted after processing.
         */
        private static bool IsTempFsbFile(string filePath)
        {
            return filePath.ToLower().StartsWith(Path.GetTempPath().ToLower()); // Check if the file path (converted to lowercase) starts with the temporary path (converted to lowercase)
        }

        /**
         * @brief Gets the list of file paths to process based on the input file path.
         *
         * @param inputFilePath The path to the input file (FSB or BANK).
         * @return List<string> A list of file paths to process.
         *
         * @details
         * This private helper function determines the list of file paths to process based on the input file type.
         * If the input file is a BANK file (*.bank), it calls ExtractFSBsFromBankFile to extract embedded FSB files
         * and returns a list of paths to these temporary FSB files.
         * If the input file is an FSB file (*.fsb) or any other file type, it simply returns a list containing only the input file path itself.
         * This function is used to handle both FSB files directly and BANK files by extracting embedded FSBs before processing.
         */
        private List<string> GetFileListToProcess(string inputFilePath)
        {
            if (inputFilePath.ToLower().EndsWith(".bank")) // Check if the input file path ends with ".bank" (case-insensitive)
            {
                return ExtractFSBsFromBankFile(inputFilePath); // If it's a BANK file, extract FSBs from it and return list of temporary FSB file paths
            }
            else // If it's not a BANK file (e.g., FSB file)
            {
                return new List<string> { inputFilePath }; // Return a list containing only the input file path itself
            }
        }

        /**
         * @brief Disables UI elements during batch processing to prevent user interaction.
         *
         * @details
         * This private helper function disables several UI elements to prevent user interaction during batch processing.
         * It disables the "Batch Extract" button, listViewFiles, "Add Files" button, "Add Folder" button,
         * "Remove Selected Files" button, "Clear File List" button, and the output directory group box.
         * This is done to ensure that the user does not interfere with the batch processing while it's running.
         */
        private void DisableUIForBatchProcess()
        {
            buttonBatchExtract.Enabled = false; // Disable the "Batch Extract" button
            listViewFiles.Enabled = false; // Disable the listViewFiles control
            buttonAddFiles.Enabled = false; // Disable the "Add Files" button
            buttonAddFolder.Enabled = false; // Disable the "Add Folder" button
            buttonRemoveSelectedFiles.Enabled = false; // Disable the "Remove Selected Files" button
            buttonClearFileList.Enabled = false; // Disable the "Clear File List" button
            groupBoxOutputDir.Enabled = false; // Disable the output directory group box
        }

        /**
         * @brief Enables UI elements after batch processing is complete or an error occurs.
         *
         * @details
         * This private helper function enables the UI elements that were disabled by DisableUIForBatchProcess.
         * It enables the listViewFiles, "Batch Extract" button, "Add Files" button, "Add Folder" button,
         * "Remove Selected Files" button, "Clear File List" button, and the output directory group box.
         * This is done to restore UI interactivity after batch processing is finished or interrupted.
         */
        private void EnableUIForBatchProcess()
        {
            listViewFiles.Enabled = true; // Enable the listViewFiles control
            buttonBatchExtract.Enabled = true; // Enable the "Batch Extract" button
            buttonAddFiles.Enabled = true; // Enable the "Add Files" button
            buttonAddFolder.Enabled = true; // Enable the "Add Folder" button
            buttonRemoveSelectedFiles.Enabled = true; // Enable the "Remove Selected Files" button
            buttonClearFileList.Enabled = true; // Enable the "Clear File List" button
            groupBoxOutputDir.Enabled = true; // Enable the output directory group box
        }

        /**
         * @brief Handles exceptions that occur during batch processing.
         *
         * @param ex The Exception object representing the exception that occurred.
         *
         * @details
         * This private helper function handles exceptions that are caught during the batch processing.
         * It updates the combined progress label to reset it, logs error messages to the application log,
         * and shows a message box to inform the user about the error that occurred during batch processing.
         */
        private void HandleBatchProcessException(Exception ex)
        {
            UpdateCombinedProgressLabel(0, 0, 0, 0); // Reset the combined progress label
            FSB_BANK_Extractor_CS_GUI.LogError("\r\n ===== Error during batch processing ===== "); // Log error message to application log indicating batch processing error
            FSB_BANK_Extractor_CS_GUI.LogError(" Exception caught: " + ex.Message); // Log error message with exception details to application log
            FSB_BANK_Extractor_CS_GUI.LogError("\r\n An error occurred during the batch processing."); // Log generic error message to application log
            MessageBox.Show("Error occurred during batch processing!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); // Show a message box indicating batch processing error
        }

        /**
         * @brief Gets the output directory path based on the user's selected output directory option.
         *
         * @return string The output directory path.
         *
         * @details
         * This private helper function determines the output directory path based on the user's selection
         * in the output directory option radio buttons.
         * If "Same as resource file" radio button is checked, it returns the directory of the first file in FileList (or executable directory if FileList is empty).
         * If "Same as executable file" radio button is checked, it returns the executable directory.
         * If "Custom output directory" radio button is checked, it returns the path from the textBoxOutputDirPath.
         * If none of the radio buttons are checked (which should not happen under normal UI operation), it returns an empty string.
         */
        private string GetOutputDirectoryPath()
        {
            if (radioButtonOutputDirRes.Checked) // Check if "Same as resource file" radio button is checked
            {
                if (FileList.Count > 0) // Check if FileList is not empty
                {
                    return Path.GetDirectoryName(FileList[0]); // Return the directory of the first file in FileList
                }
                else // If FileList is empty
                {
                    return AppDomain.CurrentDomain.BaseDirectory; // Return the executable directory as a fallback
                }
            }
            else if (radioButtonOutputDirExe.Checked) // Check if "Same as executable file" radio button is checked
            {
                return AppDomain.CurrentDomain.BaseDirectory; // Return the executable directory
            }
            else if (radioButtonOutputDirCustom.Checked) // Check if "Custom output directory" radio button is checked
            {
                return textBoxOutputDirPath.Text; // Return the text from textBoxOutputDirPath as the output directory path
            }
            return string.Empty; // Return empty string if none of the output directory options are selected (should not happen normally)
        }

        /**
         * @brief Updates the status of a file in the listViewFiles control.
         *
         * @param filePath The file path of the file to update status for.
         * @param status The new status string to set for the file.
         *
         * @details
         * This private helper function updates the status subitem of a ListViewItem in listViewFiles
         * that corresponds to the given file path. It iterates through the items in listViewFiles and checks
         * if the Tag property of an item matches the provided file path. If a match is found, it updates the text
         * of the second subitem (index 1) of that ListViewItem with the new status string.
         */
        private void UpdateFileStatus(string filePath, string status)
        {
            foreach (ListViewItem item in listViewFiles.Items) // Iterate through each ListViewItem in listViewFiles
            {
                if (item.Tag as string == filePath) // Check if the Tag property of the current ListViewItem matches the provided file path
                {
                    if (item.SubItems.Count > 1) // Check if the ListViewItem has more than one subitem (status subitem exists)
                    {
                        item.SubItems[1].Text = status; // Update the text of the second subitem (index 1) with the new status string
                    }
                    return; // Exit the method after updating the status (no need to check other items)
                }
            }
        }

        /**
         * @brief Logs a message to the text box log in the UI.
         *
         * @param message The message string to log.
         *
         * @details
         * This static helper function logs a message to the text box log in the UI.
         * It appends the message with a newline character to the textBoxLog_static control.
         * If the call is made from a thread other than the UI thread, it uses Invoke to marshal the call
         * to the UI thread to prevent cross-threading exceptions.
         */
        private static void LogMessage(string message)
        {
            if (textBoxLog_static == null) return; // Check if textBoxLog_static is null (e.g., before form is fully loaded)

            if (textBoxLog_static.InvokeRequired) // Check if the call is being made from a thread other than the UI thread
            {
                textBoxLog_static.Invoke(new Action(() => textBoxLog_static.AppendText(message + "\r\n"))); // Use Invoke to marshal the call to the UI thread and append the message
            }
            else // If the call is being made from the UI thread
            {
                textBoxLog_static.AppendText(message + "\r\n"); // Append the message directly to the text box
            }
        }

        /**
         * @brief Logs an error message to the text box log in the UI, prefixing it with "[ERROR]".
         *
         * @param errorMessage The error message string to log.
         *
         * @details
         * This static helper function logs an error message to the text box log in the UI.
         * It prefixes the error message with "[ERROR] " to visually distinguish error messages in the log.
         * It calls the LogMessage function to append the formatted error message to the textBoxLog_static control,
         * handling cross-threading issues if necessary.
         */
        private static void LogError(string errorMessage)
        {
            FSB_BANK_Extractor_CS_GUI.LogMessage("[ERROR] " + errorMessage); // Call LogMessage to log the error message with "[ERROR] " prefix
        }

        /**
         * @brief Event handler for the Form Load event.
         *
         * @param sender The source of the event.
         * @param e The EventArgs instance containing the event data.
         *
         * @details
         * This event handler is called when the Form1 form is loaded.
         * It sets initial UI states, such as checking the "Same as resource file" radio button, disabling
         * the custom output directory text box and browse button, setting the static textBoxLog_static variable,
         * setting the initial status label text to "Idle", and initializing the combined progress label to "Idle" state.
         */
        private void Form1_Load(object sender, EventArgs e)
        {
            radioButtonOutputDirRes.Checked = true; // Check the "Same as resource file" radio button by default
            textBoxOutputDirPath.Enabled = false; // Disable textBoxOutputDirPath initially
            buttonOutputDirBrowse.Enabled = false; // Disable buttonOutputDirBrowse initially
            textBoxLog_static = textBoxLog; // Set the static textBoxLog_static variable to reference the textBoxLog control
            toolStripStatusLabelProgress.Text = "Idle"; // Set the initial text of the status label to "Idle"
            UpdateCombinedProgressLabel(0, 0, 0, 0); // Initialize the combined progress label to "Idle" state on form load
        }

        /**
         * @brief Updates the combined progress label in the status strip with file and sub-sound processing progress.
         *
         * @param currentFile The index of the current file being processed (0-based).
         * @param totalFiles The total number of files to process.
         * @param currentSubSound The index of the current sub-sound being processed (0-based).
         * @param totalSubSounds The total number of sub-sounds in the current file.
         *
         * @details
         * This private helper function updates the text of the toolStripStatusLabelProgress control in the status strip
         * to display the combined progress of file and sub-sound processing.
         * It takes the current file index, total files, current sub-sound index, and total sub-sounds as input
         * and formats a progress string to display in the status label.
         * If the call is made from a thread other than the UI thread, it uses Invoke to marshal the call to the UI thread.
         */
        private void UpdateCombinedProgressLabel(int currentFile, int totalFiles, int currentSubSound, int totalSubSounds)
        {
            if (toolStripStatusLabelProgress.Owner.InvokeRequired) // Check if the call is being made from a thread other than the UI thread
            {
                toolStripStatusLabelProgress.Owner.Invoke(new Action(() => // Use Invoke to marshal the call to the UI thread
                {
                    toolStripStatusLabelProgress.Text = GetProgressText(currentFile, totalFiles, currentSubSound, totalSubSounds); // Set the text of the status label to the progress string
                }));
            }
            else // If the call is being made from the UI thread
            {
                toolStripStatusLabelProgress.Text = GetProgressText(currentFile, totalFiles, currentSubSound, totalSubSounds); // Set the text of the status label to the progress string directly
            }
        }

        /**
         * @brief Generates the progress text string for the combined progress label.
         *
         * @param currentFile The index of the current file being processed (0-based).
         * @param totalFiles The total number of files to process.
         * @param currentSubSound The index of the current sub-sound being processed (0-based).
         * @param totalSubSounds The total number of sub-sounds in the current file.
         * @return string The progress text string to display in the status label.
         *
         * @details
         * This private helper function generates the progress text string based on the current file and sub-sound processing progress.
         * It takes the current file index, total files, current sub-sound index, and total sub-sounds as input
         * and formats a string to represent the processing status.
         * It handles different cases: "Idle" state when no processing is started, "Completed" state when all files and sub-sounds are processed,
         * and a "Processing" state indicating the current file and sub-sound being processed.
         */
        private string GetProgressText(int currentFile, int totalFiles, int currentSubSound, int totalSubSounds)
        {
            if (currentFile == 0 && totalFiles == 0) // Check if no files are being processed (initial state)
            {
                return "Idle"; // Return "Idle" text for initial state
            }
            else if ((currentFile == totalFiles) & (currentSubSound == totalSubSounds)) // Check if all files and sub-sounds are processed (completed state)
            {
                return "Completed"; // Return "Completed" text for completed state
            }
            else // If processing is in progress
            {
                return $"Processing (File: {currentFile}/{totalFiles}, Sub Sound: {currentSubSound}/{totalSubSounds})"; // Return "Processing" text with current file and sub-sound progress
            }
        }

        #region Drag and Drop Functionality (for listViewFiles only)

        /**
         * @brief Event handler for the DragEnter event of listViewFiles.
         *
         * @param sender The source of the event.
         * @param e The DragEventArgs instance containing the event data.
         *
         * @details
         * Handles the DragEnter event for listViewFiles to enable drag and drop functionality.
         * If the dragged data contains files (DataFormats.FileDrop), it sets the DragDropEffects to Copy,
         * indicating that files can be copied to the listViewFiles control.
         */
        private void listViewFiles_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) // Check if the dragged data contains files (DataFormats.FileDrop)
            {
                e.Effect = DragDropEffects.Copy; // Set the drag effect to Copy, indicating files can be copied
            }
        }

        /**
         * @brief Event handler for the DragDrop event of listViewFiles.
         *
         * @param sender The source of the event.
         * @param e The DragEventArgs instance containing the event data.
         *
         * @details
         * Handles the DragDrop event for listViewFiles to process dropped files and folders.
         * Retrieves dropped files from the DragEventArgs.Data (DataFormats.FileDrop).
         * For each dropped item, checks if it's a file or a directory:
         * - If it's a file and has *.fsb or *.bank extension, adds it to FileList and listViewFiles.
         * - If it's a directory, searches for *.fsb and *.bank files within the directory and its subdirectories,
         *   and adds them to FileList and listViewFiles.
         * Prevents adding duplicate files to the list.
         */
        private async void listViewFiles_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop); // Get the dropped files from DragEventArgs

            if (files != null) // Check if there are dropped files
            {
                foreach (string filePath in files) // Iterate through each dropped file or folder path
                {
                    // Check for *.fsb or *.bank file extensions
                    if (File.Exists(filePath) && (filePath.ToLower().EndsWith(".fsb") || filePath.ToLower().EndsWith(".bank"))) // Check if it's a file and extension is *.fsb or *.bank
                    {
                        if (!FileList.Contains(filePath)) // Check if the file path is not already in FileList
                        {
                            FileList.Add(filePath); // Add the file path to FileList
                            ListViewItem item = new ListViewItem(Path.GetFileName(filePath)); // Create new ListViewItem with file name
                            item.SubItems.Add("Pending"); // Add "Pending" status subitem
                            item.Tag = filePath; // Store file path in Tag property
                            listViewFiles.Items.Add(item); // Add ListViewItem to listViewFiles
                        }
                    }
                    else if (Directory.Exists(filePath)) // Check if it's a directory
                    {
                        // Search for both *.fsb and *.bank files
                        string[] fsbFiles = Directory.GetFiles(filePath, "*.fsb", SearchOption.AllDirectories); // Search for *.fsb files in directory and subdirectories
                        string[] bankFiles = Directory.GetFiles(filePath, "*.bank", SearchOption.AllDirectories); // Search for *.bank files in directory and subdirectories
                        string[] allFiles = fsbFiles.Concat(bankFiles).ToArray(); // Combine FSB and BANK file arrays

                        foreach (string FilePath in allFiles) // Iterate through all found FSB and BANK files in the directory
                        {
                            if (!FileList.Contains(FilePath)) // Check if the file path is not already in FileList
                            {
                                FileList.Add(FilePath); // Add the file path to FileList
                                ListViewItem item = new ListViewItem(Path.GetFileName(FilePath)); // Create new ListViewItem with file name
                                item.SubItems.Add("Pending"); // Add "Pending" status subitem
                                item.Tag = FilePath; // Store file path in Tag property
                                listViewFiles.Items.Add(item); // Add ListViewItem to listViewFiles
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Menu event handlers

        /**
         * @brief Event handler for the "Exit" menu item click.
         *
         * @param sender The source of the event.
         * @param e The EventArgs instance containing the event data.
         *
         * @details
         * Closes the application when the "Exit" menu item is clicked.
         */
        private void 끝내기ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit(); // Exit the application
        }

        /**
         * @brief Event handler for the "Help" menu item click.
         *
         * @param sender The source of the event.
         * @param e The EventArgs instance containing the event data.
         *
         * @details
         * Opens the HelpForm when the "Help" menu item is clicked.
         * Creates a new instance of HelpForm and shows it in a non-modal way, allowing the user to interact with the main application while help is open.
         */
        private void 도움말ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelpForm helpForm = new HelpForm(); // Create a new instance of HelpForm
            helpForm.Show(); // Show the HelpForm in non-modal way, allowing interaction with the main form simultaneously.
        }

        /**
         * @brief Event handler for the "Information" menu item click.
         *
         * @param sender The source of the event.
         * @param e The EventArgs instance containing the event data.
         *
         * @details
         * Shows an information message box when the "Information" menu item is clicked.
         * Displays program name, version, developer information, FMOD Studio API version, and copyright information.
         */
        private void 정보ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("FSB/BANK Extractor GUI\n\n\n" + // Display program name and newlines
                            "Version: 1.1.0\n\n" + // Display version information and newlines
                            "Developer: (GitHub) IZH318\n\n" + // Display developer information and newlines
                            "Using FMOD Studio API version 2.03.06\n" + // Display FMOD Studio API version information
                            " - Studio API minor release (build 149358)\n\n\n" + // Display FMOD Studio API build information and newlines
                            "© 2025 IZH318. All rights reserved.", "Program Information"); // Display copyright information and message box title
        }

        #endregion

        #region ListView Double Click Event Handler (View *.fsb file information)

        /**
         * @brief Event handler for the DoubleClick event of listViewFiles.
         *
         * @param sender The source of the event.
         * @param e The EventArgs instance containing the event data.
         *
         * @details
         * Handles the DoubleClick event for listViewFiles to display FSB file information.
         * When a ListViewItem is double-clicked, it retrieves the file path from the item's Tag property.
         * If the file is a *.bank file, it extracts embedded FSB files, shows FSB information for the first extracted FSB in FSBDetailsForm (modal),
         * and then deletes the temporary FSB files.
         * If the file is a *.fsb file, it directly shows FSB information in FSBDetailsForm (non-modal).
         * If no file is selected or file path is null, it does nothing.
         */
        private void listViewFiles_DoubleClick(object sender, EventArgs e)
        {
            if (listViewFiles.SelectedItems.Count > 0) // Check if any items are selected in listViewFiles
            {
                ListViewItem selectedItem = listViewFiles.SelectedItems[0]; // Get the first selected ListViewItem
                string FilePath = selectedItem.Tag as string; // Get the file path from the Tag property of the selected ListViewItem

                if (FilePath != null) // Check if the file path is valid
                {
                    string extension = Path.GetExtension(FilePath).ToLower(); // Get the file extension in lowercase

                    if (extension == ".bank") // Check if the file extension is ".bank"
                    {
                        // When *.bank file is double-clicked
                        List<string> fsbFilesToDelete = ExtractFSBsFromBankFile(FilePath); // Extract FSB files from the BANK file and get a list of temporary FSB file paths
                        List<string> fsbFilesToShow = new List<string>(fsbFilesToDelete); // Copy list to delete (list to pass to UI)

                        if (fsbFilesToDelete.Count > 0) // Check if any FSB files were extracted from the BANK file
                        {
                            // When *.fsb files are extracted from *.bank file
                            string firstFsbFilePath = fsbFilesToShow[0]; // Get the path of the first extracted FSB file (to display in FSBDetailsForm)

                            using (FSBDetailsForm detailsForm = new FSBDetailsForm()) // Create a new FSBDetailsForm instance in a using block for automatic disposal
                            {
                                detailsForm.ShowLoadingLabel(); // Show loading label in FSBDetailsForm
                                detailsForm.Text = $"{Path.GetFileName(FilePath)} File Information"; // Set the text of FSBDetailsForm to file information
                                detailsForm.LoadFSBInfo(firstFsbFilePath); // Load FSB information for the first extracted FSB file into FSBDetailsForm

                                detailsForm.ShowDialog(); // **Show as Modal Dialog** - Important! Show FSBDetailsForm as a modal dialog, pausing the main application until it's closed.

                                // Delete temporary *.fsb files after FSBDetailsForm is closed
                                foreach (string tempFilePath in fsbFilesToDelete) // Iterate through the list of temporary FSB file paths
                                {
                                    try
                                    {
                                        if (File.Exists(tempFilePath)) // Check if the temporary FSB file exists
                                        {
                                            File.Delete(tempFilePath); // Delete the temporary FSB file
                                            // LogMessage($"Delete temporary *.fsb file: {tempFilePath}"); // Log message if needed (commented out)
                                        }
                                    }
                                    catch (Exception ex) // Catch any exceptions that occur during temporary FSB file deletion
                                    {
                                        // LogError($"Error deleting temporary *.fsb file: {tempFilePath} - {ex.Message}"); // Log error message if needed (commented out)
                                        MessageBox.Show($"Error occurred while deleting temporary *.fsb file: {tempFilePath} - {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); // Show error message box if temporary FSB file deletion fails
                                    }
                                }
                            } // End of using block (detailsForm.Dispose() is called)
                        }
                        else // If no FSB files were extracted from the BANK file
                        {
                            // When *.fsb files are not extracted from *.bank file
                            MessageBox.Show("*.fsb files not found in *.bank file.", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information); // Show message box indicating no FSB files found in BANK file
                        }
                    }
                    else if (extension == ".fsb") // Check if the file extension is ".fsb"
                    {
                        // When *.fsb file is double-clicked (keep existing logic)
                        FSBDetailsForm detailsForm = new FSBDetailsForm(); // Create a new FSBDetailsForm instance
                        detailsForm.ShowLoadingLabel(); // Show loading label in FSBDetailsForm
                        detailsForm.Text = $"{Path.GetFileName(FilePath)} File Information"; // Set the text of FSBDetailsForm to file information
                        detailsForm.Show(); // Show as Non-modal (keep existing method) Show FSBDetailsForm as a non-modal dialog, allowing the user to interact with the main application simultaneously.

                        detailsForm.LoadFSBInfo(FilePath); // Load FSB information for the selected FSB file into FSBDetailsForm
                    }
                }
            }
        }

        #endregion

        // static member variable added (used in LogMessage)
        private static TextBox textBoxLog_static; // Static member variable to hold a reference to the textBoxLog control for static logging methods
    }
}
#endregion