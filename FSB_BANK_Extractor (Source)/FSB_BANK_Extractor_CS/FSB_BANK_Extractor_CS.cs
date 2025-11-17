/**
 * @file FSB_BANK_Extractor_CS.cs
 * @brief Extracts audio streams from FMOD Sound Bank (.fsb) and Bank (.bank) files and saves them as Waveform Audio (.wav) files.
 * @author (Github) IZH318 (https://github.com/IZH318)
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
using System.IO;
using System.Text;
using System.Globalization;
using System.Runtime.InteropServices;
using FMOD;

namespace FSB_BANK_Extractor_CS
{
    /**
     * @class Constants
     * @brief Defines constant values used in the FSB Extractor program.
     *
     * @details
     * This static class holds constant strings, numbers, and identifiers that are used throughout the FSB Extractor program.
     * These constants are related to WAV file format, chunk sizes, and other program-specific settings.
     */
    static class Constants
    {
        public const string RIFF_HEADER = "RIFF"; // RIFF header identifier for WAV files
        public const string WAVE_FORMAT = "WAVE"; // WAVE format identifier for WAV files
        public const string FMT_CHUNK = "fmt ";   // Format chunk identifier in WAV files
        public const string DATA_CHUNK = "data";  // Data chunk identifier in WAV files
        public const ushort FORMAT_PCM = 1;         // PCM format code for WAV header
        public const ushort FORMAT_PCM_FLOAT = 3;   // PCM float format code for WAV header
        public const int BITS_IN_BYTE = 8;            // Number of bits in a byte
        public const uint CHUNK_SIZE = 4096;   // Default chunk size for reading audio data from FSB files (in bytes)
        public const float MAX_SAMPLE_VALUE = 32767.0f; // Maximum sample value for 16-bit PCM (not directly used in core logic, might be for future scaling or normalization)
    }

    class Program
    {
        /**
         * @brief Displays simple usage instructions to the console.
         */
        static void Usage_Simple()
        {
            Console.Error.WriteLine("\n\n");
            Console.Error.WriteLine(" ===== Welcome to FSB/BANK Extractor =====");
            Console.Error.WriteLine(" This program extracts sounds from *.fsb and *.bank files and saves them as *.wav files.");
            Console.Error.WriteLine("\n");
            Console.Error.WriteLine(" Usage: program <audio_file_path> [Options]");
            Console.Error.WriteLine("        (* If you omit the option, the '-res' option is applied by default.)");
            Console.Error.WriteLine("        (** For detailed usage instructions, please refer to `program -h` or `program -help`.)");
            Console.Error.WriteLine("\n");
            Console.Error.WriteLine("   <audio_file_path> : Path to the *.fsb or *.bank file");
            Console.Error.WriteLine("");
            Console.Error.WriteLine("   [Options]         : -res                  : Save wav files in the same folder as fsb/bank file (default)");
            Console.Error.WriteLine("                       -exe                  : Save wav files in the same folder as program file");
            Console.Error.WriteLine("                       -o <output_directory> : Save wav files in the user-specified folder");
            Console.Error.WriteLine("                       -v                    : Enable verbose logging for chunk processing verification");
        }

        /**
         * @brief Displays detailed usage instructions to the console.
         */
        static void Usage_Detail()
        {
            Console.Error.WriteLine("\n\n");
            Console.Error.WriteLine(" ===== Welcome to FSB/BANK Extractor =====");
            Console.Error.WriteLine(" This program extracts sounds from *.fsb and *.bank files and saves them as *.wav files.");
            Console.Error.WriteLine("\n");
            Console.Error.WriteLine(" Usage: program <audio_file_path> [Options]");
            Console.Error.WriteLine("        (* If you omit the option, the '-res' option is applied by default.)");
            Console.Error.WriteLine("");
            Console.Error.WriteLine(" <audio_file_path> : This is the required path to the *.fsb or *.bank file.");
            Console.Error.WriteLine("                     (* Example: \"C:\\sounds\\music.fsb\" or \"audio.bank\")");
            Console.Error.WriteLine("\n");
            Console.Error.WriteLine(" [Options] : These are optional settings. You can choose one of the following options to specify the output folder.");
            Console.Error.WriteLine("");
            Console.Error.WriteLine("   -res    : Save *.wav files in the same folder as the *.fsb or *.bank file. (Default option)");
            Console.Error.WriteLine("");
            Console.Error.WriteLine("             If the *.fsb/bank file path is 'C:\\sounds\\music.fsb' or 'C:\\sounds\\audio.bank',");
            Console.Error.WriteLine("               output files will be saved in the 'C:\\sounds' folder.");
            Console.Error.WriteLine("");
            Console.Error.WriteLine("             This is generally useful when you want to manage output files together within the resource folder.");
            Console.Error.WriteLine("\n");
            Console.Error.WriteLine("   -exe    : Save *.wav files in the folder where the program file is located.");
            Console.Error.WriteLine("");
            Console.Error.WriteLine("             If the program is 'D:\\tools\\audio_extractor.exe' and you use the '-exe' option,");
            Console.Error.WriteLine("               output files will be saved in the 'D:\\tools' folder.");
            Console.Error.WriteLine("");
            Console.Error.WriteLine("             This is useful when you want to manage output files in the same location as the executable.");
            Console.Error.WriteLine("\n");
            Console.Error.WriteLine("   -o <output_directory>");
            Console.Error.WriteLine("           : Save *.wav files in the user-specified directory.");
            Console.Error.WriteLine("");
            Console.Error.WriteLine("             You need to enter the path of the folder where you want to save the *.wav files");
            Console.Error.WriteLine("               in the <output_directory> place. (* Example: -o \"C:\\output\" or -o \"output_wav\")");
            Console.Error.WriteLine("");
            Console.Error.WriteLine("             The path can be an absolute path or a relative path based on the current execution location.");
            Console.Error.WriteLine("\n");
            Console.Error.WriteLine("   -v      : Enable verbose logging to verify chunk processing.");
            Console.Error.WriteLine("");
            Console.Error.WriteLine("             When this option is enabled, detailed information about each audio chunk");
            Console.Error.WriteLine("               read from the FSB file will be logged to a file (*.log).");
            Console.Error.WriteLine("");
            Console.Error.WriteLine("             This is helpful for developers to verify if the audio data is being read and processed correctly.");
            Console.Error.WriteLine("\n");
            Console.Error.WriteLine(" Usage Examples:");
            Console.Error.WriteLine("   program audio.fsb                           (Default option: same as -res)");
            Console.Error.WriteLine("   program music.bank -res                     (Save in the same folder as the *.fsb file)");
            Console.Error.WriteLine("   program sounds.fsb -exe                     (Save in the same folder as the executable file)");
            Console.Error.WriteLine("   program voices.bank -o \"C:\\output\\audio\"    (Save in the absolute path folder)");
            Console.Error.WriteLine("   program effects.fsb -o \"output_wav\"         (Save in the relative path folder)");
            Console.Error.WriteLine("   program music.bank -v                       (Enable verbose logging)");
        }

        /**
         * @brief Checks the result of an FMOD API call and throws an exception if it's not FMOD.RESULT.OK.
         *
         * @param result FMOD.RESULT returned by an FMOD API function.
         * @param message Error message to include in the exception if the result is not FMOD.RESULT.OK.
         *
         * @throws System.Exception if the FMOD.RESULT is not FMOD.RESULT.OK.
         */
        static void CheckFMODResult(FMOD.RESULT result, string message)
        {
            if (result != FMOD.RESULT.OK) // Checks if the FMOD result is not FMOD.RESULT.OK (indicating an error)
            {
                throw new Exception(message + ": " + FMOD.Error.String(result)); // Throws a runtime error exception with the provided message and FMOD error string
            }
        }

        /**
         * @class FMODSystem
         * @brief RAII wrapper for FMOD System object, managing initialization and release.
         *
         * @details
         * This class encapsulates the FMOD System object, ensuring proper initialization when an instance is created
         * and automatic release and close of the system when the instance goes out of scope.
         * It handles FMOD system creation, version checking, and initialization.
         */
        class FMODSystem : IDisposable
        {
            private FMOD.System system; // Private member to store the FMOD System object

            /**
             * @brief Constructor for FMODSystem.
             *
             * @details
             * Initializes the FMOD system and checks for version compatibility.
             * Throws System.Exception if FMOD system creation or initialization fails, or if version mismatch is detected.
             */
            public FMODSystem()
            {
                FMOD.Factory.System_Create(out system); // Creates the main FMOD system object
                CheckFMODResult(system.getVersion(out uint version), "FMOD::System::getVersion failed"); // Checks if system creation was successful

                if (version < VERSION.number) // Compares the library version with the header version (VERSION.number macro)
                {
                    Console.Error.WriteLine($" FMOD lib version {version:X} is less than header version {VERSION.number}");
                    throw new Exception("FMOD version mismatch"); // Throws an exception if library version is older than header version
                }

                CheckFMODResult(system.init(32, FMOD.INITFLAGS.NORMAL, IntPtr.Zero), "FMOD::System::init failed"); // Initializes the FMOD system with 32 channels and default settings
            }

            /**
             * @brief Returns the raw FMOD System object.
             *
             * @return FMOD.System The FMOD System object.
             */
            public FMOD.System Get() { return system; } // Getter to access the FMOD System object

            /**
             * @brief Dispose method for FMODSystem (implements IDisposable).
             *
             * @details
             * Closes and releases the FMOD system object to free resources when the object is disposed.
             * Error messages are printed to Console.Error if closing or releasing fails, but no exception is thrown in Dispose.
             */
            public void Dispose()
            {
                if (system.hasHandle()) // Checks if the FMOD system object is valid
                {
                    var result = system.close(); // Closes the FMOD system
                    if (result != FMOD.RESULT.OK) // Checks if closing was successful
                    {
                        Console.Error.WriteLine($" FMOD::System::close failed: {FMOD.Error.String(result)}"); // Prints error message if closing fails
                    }
                    result = system.release(); // Releases the FMOD system object, freeing allocated memory
                    if (result != FMOD.RESULT.OK) // Checks if releasing was successful
                    {
                        Console.Error.WriteLine($" FMOD::System::release failed: {FMOD.Error.String(result)}"); // Prints error message if releasing fails
                    }
                    system.clearHandle(); // Clears the handle to the FMOD system object
                }
            }
        }

        /**
         * @class FMODSound
         * @brief RAII wrapper for FMOD Sound object, managing sound loading and release.
         *
         * @details
         * This class encapsulates the FMOD Sound object, ensuring proper sound creation when an instance is created
         * and automatic release of the sound when the instance goes out of scope.
         * It handles loading a sound from a file path using FMOD.
         */
        class FMODSound : IDisposable
        {
            private FMOD.Sound sound; // Private member to store the FMOD Sound object

            /**
             * @brief Constructor for FMODSound.
             *
             * @param system Pointer to the initialized FMOD System object.
             * @param filePath Path to the audio file to be loaded.
             *
             * @details
             * Creates an FMOD Sound object from the specified file path using the provided FMOD System.
             * Throws System.Exception if sound creation fails.
             */
            public FMODSound(FMOD.System system, string filePath)
            {
                FMOD.CREATESOUNDEXINFO exinfo = new FMOD.CREATESOUNDEXINFO(); // Creates extended sound info structure (not strictly needed here, but good practice)
                exinfo.cbsize = Marshal.SizeOf(exinfo); // Sets the size of the structure
                var result = system.createSound(filePath, FMOD.MODE.CREATESTREAM, ref exinfo, out sound); // Creates an FMOD sound object from the given file path, using stream mode
                CheckFMODResult(result, $"FMOD::System::createSound failed for {filePath}"); // Checks if sound creation was successful
            }

            /**
             * @brief Returns the raw FMOD Sound object.
             *
             * @return FMOD.Sound The FMOD Sound object.
             */
            public FMOD.Sound Get() { return sound; } // Getter to access the FMOD Sound object

            /**
             * @brief Dispose method for FMODSound (implements IDisposable).
             *
             * @details
             * Releases the FMOD Sound object to free resources when the object is disposed.
             * Error messages are printed to Console.Error if releasing fails, but no exception is thrown in Dispose.
             */
            public void Dispose()
            {
                if (sound.hasHandle()) // Checks if the FMOD sound object is valid
                {
                    var result = sound.release(); // Releases the FMOD sound object, freeing allocated memory
                    if (result != FMOD.RESULT.OK) // Checks if releasing was successful
                    {
                        Console.Error.WriteLine($" FMOD::Sound::release failed: {FMOD.Error.String(result)}"); // Prints error message if releasing fails
                    }
                    sound.clearHandle(); // Clears the handle to the FMOD sound object
                }
            }
        }

        /**
         * @brief Sanitizes a file name by replacing invalid characters with safe alternatives.
         *
         * @param fileName The original file name string.
         * @return string Sanitized file name string.
         *
         * @details
         * Replaces characters that are typically invalid or problematic in file names (like <, >, :, ", /, \, |, ?, *)
         * with similar-looking but safe Unicode characters. This helps to avoid file system errors when creating output files.
         */
        static string SanitizeFileName(string fileName)
        {
            var charMap = new Dictionary<char, string> // Static map to store invalid characters and their replacements
            {
                {'<', "〈"}, {'>', "〉"}, {':', "："}, {'\"', "＂"}, {'/', "／"},
                {'\\', "￦"}, {'|', "｜"}, {'?', "？"}, {'*', "＊"}
            };

            string sanitized = fileName; // Creates a copy of the input file name to sanitize
            foreach (var kvp in charMap) // Iterates through each character in the file name
            {
                sanitized = sanitized.Replace(kvp.Key.ToString(), kvp.Value); // Replaces the invalid character with its corresponding safe replacement from the map
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
        static bool WriteWAVHeader(Stream file, int sampleRate, int channels, long dataSize, int bitsPerSample, SOUND_FORMAT format)
        {
            if (!file.CanWrite) // Checks if the output file stream is writeable
            {
                Console.Error.WriteLine(" Error: Output file is not open."); // Prints error message to Console.Error if file is not writeable
                return false; // Returns false to indicate failure
            }

            try
            {
                var bw = new BinaryWriter(file); // Creates a BinaryWriter to write binary data to the file stream

                bw.Write(Encoding.ASCII.GetBytes(Constants.RIFF_HEADER)); // Writes "RIFF" identifier (4 bytes)
                bw.Write((uint)(36 + dataSize)); // Writes chunk size (WAV header size + data size), 4 bytes
                bw.Write(Encoding.ASCII.GetBytes(Constants.WAVE_FORMAT)); // Writes "WAVE" identifier (4 bytes)
                bw.Write(Encoding.ASCII.GetBytes(Constants.FMT_CHUNK)); // Writes "fmt " chunk identifier (4 bytes)
                bw.Write((uint)16); // Writes format chunk size (always 16 for PCM), 4 bytes
                bw.Write((ushort)(format == SOUND_FORMAT.PCMFLOAT ? Constants.FORMAT_PCM_FLOAT : Constants.FORMAT_PCM)); // Writes audio format code (PCM or PCM float), 2 bytes
                bw.Write((ushort)channels); // Writes number of channels, 2 bytes
                bw.Write((uint)sampleRate); // Writes sample rate, 4 bytes
                bw.Write((uint)((long)sampleRate * channels * bitsPerSample / Constants.BITS_IN_BYTE)); // Writes byte rate (bytes per second), 4 bytes
                bw.Write((ushort)(channels * bitsPerSample / Constants.BITS_IN_BYTE)); // Writes block align (bytes per sample block), 2 bytes
                bw.Write((ushort)bitsPerSample); // Writes bits per sample, 2 bytes
                bw.Write(Encoding.ASCII.GetBytes(Constants.DATA_CHUNK)); // Writes "data" chunk identifier (4 bytes)
                bw.Write((uint)dataSize); // Writes data chunk size (audio data size), 4 bytes

                return true; // Returns true to indicate successful header writing
            }
            catch (Exception e) // Catches file stream exceptions
            {
                Console.Error.WriteLine($" Error writing WAV header: {e.Message}"); // Prints error message to Console.Error if exception occurred during header writing
                return false; // Returns false to indicate failure
            }
        }

        /**
         * @brief Writes a log message to the log file if verbose logging is enabled.
         *
         * @param logFile Output file stream for the log file.
         * @param level Log level (e.g., "INFO", "WARNING", "ERROR").
         * @param functionName Name of the function where the log message originates.
         * @param message The log message string.
         * @param verboseLogEnabled Flag indicating if verbose logging is enabled.
         * @param errorCode FMOD.RESULT error code (optional, FMOD.RESULT.OK if no error).
         *
         * @details
         * This function writes a formatted log message to the specified log file if verbose logging is enabled.
         * The log message includes a timestamp, log level, function name, and the message itself.
         * If an FMOD error code is provided (not FMOD.RESULT.OK), it's also included in the log message.
         */
        static void WriteLogMessage(StreamWriter logFile, string level, string functionName, string message, bool verboseLogEnabled, RESULT errorCode = RESULT.OK)
        {
            if (logFile != null && verboseLogEnabled) // Checks if log file is open and verbose logging is enabled
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"); // Gets current timestamp in yyyy-MM-dd HH:mm:ss.fff format
                logFile.WriteLine($"[{timestamp}] [{level}] [{functionName}] {message}{(errorCode != RESULT.OK ? $" (Error code: {errorCode})" : "")}"); // Writes formatted log message to the log file
            }
        }

        /**
         * @namespace AudioProcessor
         * @brief Namespace for audio data processing classes and functions.
         *
         * @details
         * This namespace contains functionalities for processing audio data,
         * specifically for writing audio data chunks to WAV files based on different PCM formats.
         */
        static class AudioProcessor
        {
            /**
             * @brief Writes audio data chunks from an FMOD sub-sound to a WAV file.
             *
             * @typeparam T Data type for audio samples: byte, short, int, float, or Pcm24 (value type).
             * @param subSound FMOD Sound object representing the sub-sound.
             * @param wavFile Stream to write the WAV file data to.
             * @param soundLengthBytes Total bytes to read from the subSound.
             * @param subSoundIndex Index of the sub-sound being processed.
             * @param chunkCount Chunk counter (passed by reference for logging).
             * @param verboseLogEnabled Flag to enable verbose logging.
             * @param logFile StreamWriter for verbose logging output.
             * @param bw BinaryWriter for writing WAV data to the wavFile stream.
             * @returns True if audio data chunks were written successfully, false otherwise.
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
         * @struct Pcm24
         * @brief Empty struct used as a type marker for 24-bit PCM data handling.
         *
         * @details
         * This struct is defined to specifically handle 24-bit PCM format in the generic `WriteAudioDataChunk` function.
         * It doesn't contain any members and is solely used for type checking and specialization of 24-bit PCM data writing logic.
         */
        public struct Pcm24 { } // Empty struct for Pcm24 format identification

        /**
         * @struct SoundInfo
         * @brief Structure to hold information about a sound extracted from FSB.
         *
         * @details
         * This structure is used to store various properties of a sub-sound, such as format, sample rate, channels, length, and name.
         * It is populated by the GetSoundInfo function and used when writing the WAV file header and processing audio data.
         */
        struct SoundInfo
        {
            public SOUND_FORMAT format;     // FMOD sound format (e.g., PCM8, PCM16, PCMFLOAT)
            public SOUND_TYPE soundType;      // FMOD sound type (e.g., WAV, OGGVORBIS, FSB)
            public int sampleRate;          // Sample rate of the sound in Hz
            public int bitsPerSample;       // Bits per sample for the sound
            public int channels;            // Number of channels (1 for mono, 2 for stereo, etc.)
            public uint soundLengthBytes; // Length of the sound data in bytes
            public uint lengthMs;       // Length of the sound in milliseconds
            public string subSoundName;  // Name of the sub-sound (if available)
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
        static SoundInfo GetSoundInfo(FMOD.Sound subSound, int subSoundIndex, bool verboseLogEnabled, StreamWriter logFile)
        {
            SoundInfo info = new SoundInfo(); // Structure to store sound information

            WriteLogMessage(logFile, "INFO", "GetSoundInfo", "Getting sound format...", verboseLogEnabled); // Logs attempt to get sound format
            var fmodSystemResult = subSound.getFormat(out info.soundType, out info.format, out info.channels, out info.bitsPerSample); // Gets sound format information from FMOD Sound object
            if (fmodSystemResult != FMOD.RESULT.OK) // Checks if getting format failed
            {
                WriteLogMessage(logFile, "ERROR", "GetSoundInfo", $"FMOD::Sound::getFormat failed for sub-sound {subSoundIndex}: {FMOD.Error.String(fmodSystemResult)}", verboseLogEnabled); // Logs FMOD error (ERROR level)
                CheckFMODResult(fmodSystemResult, $"FMOD::Sound::getFormat failed for sub-sound {subSoundIndex}"); // Throws exception on error
            }
            else
            {
                string formatStr = info.format.ToString(); // Converts FMOD_SOUND_FORMAT enum to string
                string soundTypeStr = info.soundType.ToString(); // Converts FMOD_SOUND_TYPE enum to string
                WriteLogMessage(logFile, "INFO", "GetSoundInfo", $"FMOD::Sound::getFormat successful - Sound Type: {soundTypeStr}, Format: {formatStr}, Channels: {info.channels}, Bits Per Sample: {info.bitsPerSample}", verboseLogEnabled); // Logs successful format retrieval (INFO level)
            }

            WriteLogMessage(logFile, "INFO", "GetSoundInfo", "Getting default sound parameters...", verboseLogEnabled); // Logs attempt to get default parameters
            fmodSystemResult = subSound.getDefaults(out float defaultFrequency, out int defaultPriority); // Gets default frequency and priority from FMOD Sound object
            if (fmodSystemResult != FMOD.RESULT.OK) // Checks if getting defaults failed
            {
                WriteLogMessage(logFile, "ERROR", "GetSoundInfo", $"FMOD::Sound::getDefaults failed for sub-sound {subSoundIndex}: {FMOD.Error.String(fmodSystemResult)}", verboseLogEnabled); // Logs FMOD error (ERROR level)
                CheckFMODResult(fmodSystemResult, $"FMOD::Sound::getDefaults failed for sub-sound {subSoundIndex}"); // Throws exception on error
            }
            else
            {
                WriteLogMessage(logFile, "INFO", "GetSoundInfo", $"FMOD::Sound::getDefaults successful - Default Frequency: {defaultFrequency}, Default Priority: {defaultPriority}", verboseLogEnabled); // Logs successful defaults retrieval (INFO level)
            }

            info.sampleRate = (defaultFrequency > 0) ? (int)defaultFrequency : 44100; // Sets sample rate, using default frequency if available, otherwise defaults to 44100 Hz
            WriteLogMessage(logFile, "INFO", "GetSoundInfo", $"Final Sample Rate for WAV header: {info.sampleRate}", verboseLogEnabled); // Logs final sample rate used for WAV header

            WriteLogMessage(logFile, "INFO", "GetSoundInfo", "Getting sound length in bytes...", verboseLogEnabled); // Logs attempt to get sound length in bytes
            fmodSystemResult = subSound.getLength(out uint soundLengthBytes, FMOD.TIMEUNIT.PCMBYTES); // Gets sound length in bytes
            if (fmodSystemResult != FMOD.RESULT.OK) // Checks if getting length failed
            {
                WriteLogMessage(logFile, "ERROR", "GetSoundInfo", $"FMOD::Sound::getLength (bytes) failed for sub-sound {subSoundIndex}: {FMOD.Error.String(fmodSystemResult)}", verboseLogEnabled); // Logs FMOD error (ERROR level)
                CheckFMODResult(fmodSystemResult, $"FMOD::Sound::getLength (bytes) failed for sub-sound {subSoundIndex}"); // Throws exception on error
            }
            else
            {
                info.soundLengthBytes = soundLengthBytes; // Stores sound length in bytes
                WriteLogMessage(logFile, "INFO", "GetSoundInfo", $"FMOD::Sound::getLength (bytes) successful - Length: {info.soundLengthBytes} bytes", verboseLogEnabled); // Logs successful length retrieval in bytes (INFO level)
            }

            WriteLogMessage(logFile, "INFO", "GetSoundInfo", "Getting sound length in milliseconds...", verboseLogEnabled); // Logs attempt to get sound length in milliseconds
            fmodSystemResult = subSound.getLength(out uint lengthMs, FMOD.TIMEUNIT.MS); // Gets sound length in milliseconds
            if (fmodSystemResult != FMOD.RESULT.OK) // Checks if getting length failed
            {
                WriteLogMessage(logFile, "ERROR", "GetSoundInfo", $"FMOD::Sound::getLength (ms) failed for sub-sound {subSoundIndex}: {FMOD.Error.String(fmodSystemResult)}", verboseLogEnabled); // Logs FMOD error (ERROR level)
                CheckFMODResult(fmodSystemResult, $"FMOD::Sound::getLength (ms) failed for sub-sound {subSoundIndex}"); // Throws exception on error
            }
            else
            {
                info.lengthMs = lengthMs; // Stores sound length in milliseconds
                WriteLogMessage(logFile, "INFO", "GetSoundInfo", $"FMOD::Sound::getLength (ms) successful - Length: {info.lengthMs} ms", verboseLogEnabled); // Logs successful length retrieval in milliseconds (INFO level)
            }

            WriteLogMessage(logFile, "INFO", "GetSoundInfo", "Getting sub-sound name...", verboseLogEnabled); // Logs attempt to get sub-sound name
            fmodSystemResult = subSound.getName(out string subSoundName, 256); // Gets sub-sound name
            if (fmodSystemResult != FMOD.RESULT.OK && fmodSystemResult != FMOD.RESULT.ERR_TAGNOTFOUND) // Checks if getting name failed (but ignores FMOD.RESULT.ERR_TAGNOTFOUND, which is expected if no name tag exists)
            {
                WriteLogMessage(logFile, "WARNING", "GetSoundInfo", $"FMOD::Sound::getName failed or tag not found for sub-sound {subSoundIndex}: {FMOD.Error.String(fmodSystemResult)}", verboseLogEnabled); // Logs warning if getting name failed or tag is not found (WARNING level)
                info.subSoundName = ""; // Sets subSoundName to empty string if name retrieval fails or tag is not found
            }
            else
            {
                info.subSoundName = subSoundName ?? ""; // Stores sub-sound name, using null-coalescing operator to handle null case
                WriteLogMessage(logFile, "INFO", "GetSoundInfo", $"FMOD::Sound::getName successful - Name: {info.subSoundName}", verboseLogEnabled); // Logs successful name retrieval (INFO level)
            }

            return info; // Returns the SoundInfo structure containing retrieved information
        }

        // Added from GUI version to handle duplicate filenames
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

        // Added from GUI version to prepare subdirectories
        static void PrepareOutputDirectory(string outputDirectory)
        {
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
                Console.WriteLine(" Created directory: " + outputDirectory);
            }
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
         * @param usedFileNames Added to track filenames and prevent overwrites.
         *
         * @details
         * This function orchestrates the process of extracting audio data from a given FMOD sub-sound and saving it as a WAV file.
         * It retrieves sound information, constructs the output file path, writes the WAV header, and then writes the audio data chunks
         * based on the sound format. It also handles error logging and console output for progress and status.
         */
        static void ProcessSubSound(FMOD.System fmodSystem, FMOD.Sound subSound, int subSoundIndex, int totalSubSounds, string baseFileName, string outputDirectoryPath, bool verboseLogEnabled, StreamWriter logFile, HashSet<string> usedFileNames)
        {
            logFile?.WriteLine(); // Null conditional operator to avoid NullReferenceException if logFile is null, adds a newline for readability
            WriteLogMessage(logFile, "INFO", "ProcessSubSound", $"Processing sub-sound {subSoundIndex + 1}/{totalSubSounds}", verboseLogEnabled); // Logs start of sub-sound processing
            CheckFMODResult(subSound.seekData(0), $"FMOD::Sound::seekData failed for sub-sound {subSoundIndex}"); // Seeks to the beginning of the sub-sound data
            WriteLogMessage(logFile, "INFO", "ProcessSubSound", "FMOD::Sound::seekData successful", verboseLogEnabled); // Logs successful seek operation

            SoundInfo soundInfo = GetSoundInfo(subSound, subSoundIndex, verboseLogEnabled, logFile); // Retrieves sound information for the current sub-sound

            // Added from GUI version: Tag-based directory creation
            string finalOutputDirectory = outputDirectoryPath;
            FMOD.TAG tag;
            if (subSound.getTag("language", 0, out tag) == FMOD.RESULT.OK)
            {
                string language = Marshal.PtrToStringAnsi(tag.data);
                if (!string.IsNullOrEmpty(language))
                {
                    string languageFolder = Path.Combine(outputDirectoryPath, SanitizeFileName(language));
                    // Create the subdirectory if it doesn't exist
                    PrepareOutputDirectory(languageFolder);
                    finalOutputDirectory = languageFolder;
                }
            }

            // Using GetOutputFilePath to prevent overwrites
            string fullOutputPath = GetOutputFilePath(finalOutputDirectory, baseFileName, soundInfo, subSoundIndex, usedFileNames);

            Console.WriteLine(); // Adds a newline to console output for readability
            Console.WriteLine($" Processing sub-sound {subSoundIndex + 1}/{totalSubSounds}:"); // Prints processing start message to console
            Console.WriteLine($" Name: {soundInfo.subSoundName}"); // Prints sub-sound name to console
            Console.WriteLine($" Channels: {soundInfo.channels}"); // Prints number of channels to console
            Console.WriteLine($" Sample Rate: {soundInfo.sampleRate} Hz"); // Prints sample rate to console
            Console.WriteLine($" Length: {soundInfo.lengthMs} ms"); // Prints length in milliseconds to console
            Console.WriteLine($" Output: {fullOutputPath}"); // Show the final output path

            FileStream wavFile = null; // FileStream for output WAV file, initialized to null
            BinaryWriter bw = null; // BinaryWriter for writing binary data to WAV file, initialized to null

            try
            {
                wavFile = new FileStream(fullOutputPath, FileMode.Create, FileAccess.Write, FileShare.None); // Opens output WAV file in create mode, write access, and no sharing
                bw = new BinaryWriter(wavFile, Encoding.UTF8, leaveOpen: true); // Creates BinaryWriter for the FileStream, using UTF8 encoding (though WAV data is binary, not text) and leaving stream open after writer disposal

                WriteLogMessage(logFile, "INFO", "ProcessSubSound", $"WAV file opened successfully: {fullOutputPath}", verboseLogEnabled); // Logs successful file open (INFO level)


                if (!WriteWAVHeader(wavFile, soundInfo.sampleRate, soundInfo.channels, soundInfo.soundLengthBytes, soundInfo.bitsPerSample, soundInfo.format)) // Writes WAV header to the file
                {
                    WriteLogMessage(logFile, "ERROR", "ProcessSubSound", $"Error writing WAV header to file: {fullOutputPath}", verboseLogEnabled); // Logs header write error (ERROR level)
                    Console.Error.WriteLine($" Error writing WAV header to file: {fullOutputPath}"); // Prints error to Console.Error
                    throw new Exception("Failed to write WAV header"); // Throws exception on error
                }
                WriteLogMessage(logFile, "INFO", "ProcessSubSound", "WAV header written successfully", verboseLogEnabled); // Logs successful header write (INFO level)

                int chunkCount = 0; // Initializes chunk counter for logging
                bool writeSuccess = false; // Flag to track success of audio data writing

                switch (soundInfo.format) // Switch statement based on sound format to determine data writing function
                {
                    case SOUND_FORMAT.PCM8: // 8-bit PCM format
                        writeSuccess = AudioProcessor.WriteAudioDataChunk<byte>(subSound, wavFile, soundInfo.soundLengthBytes, subSoundIndex, ref chunkCount, verboseLogEnabled, logFile, bw); // Writes 8-bit PCM data
                        break;
                    case SOUND_FORMAT.PCM16: // 16-bit PCM format
                        writeSuccess = AudioProcessor.WriteAudioDataChunk<short>(subSound, wavFile, soundInfo.soundLengthBytes, subSoundIndex, ref chunkCount, verboseLogEnabled, logFile, bw); // Writes 16-bit PCM data
                        break;
                    case SOUND_FORMAT.PCM24: // 24-bit PCM format
                        writeSuccess = AudioProcessor.WriteAudioDataChunk<Pcm24>(subSound, wavFile, soundInfo.soundLengthBytes, subSoundIndex, ref chunkCount, verboseLogEnabled, logFile, bw); // Writes 24-bit PCM data
                        break;
                    case SOUND_FORMAT.PCM32: // 32-bit PCM format
                        writeSuccess = AudioProcessor.WriteAudioDataChunk<int>(subSound, wavFile, soundInfo.soundLengthBytes, subSoundIndex, ref chunkCount, verboseLogEnabled, logFile, bw); // Writes 32-bit PCM data
                        break;
                    case SOUND_FORMAT.PCMFLOAT: // PCM float format
                        writeSuccess = AudioProcessor.WriteAudioDataChunk<float>(subSound, wavFile, soundInfo.soundLengthBytes, subSoundIndex, ref chunkCount, verboseLogEnabled, logFile, bw); // Writes PCM float data
                        break;
                    default: // Unsupported format
                        WriteLogMessage(logFile, "WARNING", "ProcessSubSound", $"Unsupported format detected: {soundInfo.format}. Processing as PCM16 (potentially incorrect).", verboseLogEnabled); // Logs warning for unsupported format (WARNING level)
                        Console.WriteLine($"Warning: Unsupported format {soundInfo.format}, attempting to extract as PCM16."); // Prints warning to console
                        writeSuccess = AudioProcessor.WriteAudioDataChunk<short>(subSound, wavFile, soundInfo.soundLengthBytes, subSoundIndex, ref chunkCount, verboseLogEnabled, logFile, bw); // Falls back to writing as 16-bit PCM (potential data loss or incorrect output)
                        break;
                }

                if (!writeSuccess) // Checks if audio data writing failed
                {
                    WriteLogMessage(logFile, "ERROR", "ProcessSubSound", $"Error writing audio data to WAV file for sub-sound {subSoundIndex}", verboseLogEnabled); // Logs data write error (ERROR level)
                    Console.Error.WriteLine($" Error writing audio data to WAV file for sub-sound {subSoundIndex}"); // Prints error to Console.Error
                    throw new Exception("Failed to write audio data to WAV file"); // Throws exception on error
                }

                WriteLogMessage(logFile, "INFO", "ProcessSubSound", "Sub-sound processing finished successfully", verboseLogEnabled); // Logs successful sub-sound processing (INFO level)

                Console.WriteLine(" Status: Success"); // Prints success status to console
            }
            catch (Exception e) // Catches exceptions during sub-sound processing
            {
                WriteLogMessage(logFile, "ERROR", "ProcessSubSound", $"Exception during sub-sound processing: {e.Message}", verboseLogEnabled); // Logs exception message (ERROR level)
                Console.Error.WriteLine($" Exception during sub-sound processing: {e.Message}"); // Prints exception message to Console.Error
                throw; // Re-throws the exception to be caught in the main function
            }
            finally // Ensure resources are released regardless of exceptions
            {
                bw?.Close(); // Null conditional operator to avoid NullReferenceException if bw is null, closes the BinaryWriter
                wavFile?.Close(); // Null conditional operator to avoid NullReferenceException if wavFile is null, closes the FileStream
            }
        }


        /**
         * @brief Searches for the "FSB5" signature within a BinaryReader stream.
         *
         * @param reader The BinaryReader stream to search within.
         * @return bool True if the "FSB5" signature is found, false otherwise.
         *
         * @details
         * This function searches for the "FSB5" signature (4 bytes) within the provided BinaryReader stream.
         * It reads 4 bytes at a time and checks if they match the "FSB5" signature.
         * If the signature is found, the stream pointer is rewound to the beginning of the signature.
         * The search is performed by moving one byte at a time through the stream.
         * If the signature is not found by the end of the stream, the stream pointer is reset to its original position.
         */
        private static bool FindFSB5Signature(BinaryReader reader)
        {
            long startPosition = reader.BaseStream.Position; // Store the initial position of the stream to reset later if signature is not found.
            while (reader.BaseStream.Position < reader.BaseStream.Length - 3) // Loop as long as there are at least 4 bytes left in the stream to read for the signature.
            {
                // Check for "FSB5" signature (4 bytes)
                byte[] signature = reader.ReadBytes(4); // Read 4 bytes from the stream into a byte array, which could be the signature.
                if (Encoding.ASCII.GetString(signature) == "FSB5") // Convert the 4 bytes to an ASCII string and compare it with "FSB5".
                {
                    reader.BaseStream.Seek(-4, SeekOrigin.Current); // If "FSB5" signature is found, rewind the stream position by 4 bytes to point to the start of the signature.
                    return true; // Return true indicating that the "FSB5" signature has been found.
                }
                reader.BaseStream.Seek(-3, SeekOrigin.Current); // If not "FSB5", move the stream position back by 3 bytes to check the next possible 4-byte sequence.
            }
            reader.BaseStream.Position = startPosition; // If the loop finishes without finding the signature, reset the stream position to the original starting position.
            return false; // Return false indicating that the "FSB5" signature was not found in the stream.
        }


        /**
         * @brief Extracts FSB files embedded within a BANK file.
         *
         * @param bankFilePath Path to the BANK file to extract FSBs from.
         * @return List<string> List of temporary file paths for the extracted FSB files.
         *         Returns an empty list if no FSB files are found or if an error occurs.
         *
         * @details
         * This function scans a BANK file for embedded FSB (FMOD Sound Bank) files.
         * It searches for the "FSB5" signature to identify the start of each FSB file within the BANK file.
         * For each FSB found, it extracts the FSB data into a temporary file in the system's temporary folder.
         * The function returns a list of paths to these temporary FSB files.
         * If any error occurs during file processing or FSB extraction, error messages are printed to the console,
         * and the function continues to process any remaining FSB files in the BANK.
         * Temporary FSB files are named based on the BANK file name, with a sequence number appended if multiple FSBs are extracted.
         */
        private static List<string> ExtractFSBsFromBankFile(string bankFilePath)
        {
            List<string> tempFsbFiles = new List<string>(); // Initialize a list to store paths to the temporary FSB files.
            string baseBankFileName = Path.GetFileNameWithoutExtension(bankFilePath); // Get the base file name (without extension) of the input BANK file path.
            string tempPath = Path.GetTempPath(); // Get the system's temporary directory path where extracted FSB files will be stored.

            try // Begin a try-catch block to handle potential exceptions during file operations.
            {
                using (FileStream fileStream = new FileStream(bankFilePath, FileMode.Open, FileAccess.Read)) // Open the BANK file for reading using a FileStream. Ensure the file is disposed of properly using 'using'.
                using (BinaryReader reader = new BinaryReader(fileStream)) // Create a BinaryReader to read binary data from the BANK file stream. Ensure the reader is disposed of properly using 'using'.
                {
                    int fsbCount = 0; // Initialize a counter to keep track of the number of FSB files extracted from the BANK file.
                    while (FindFSB5Signature(reader)) // Loop as long as the FindFSB5Signature function finds an "FSB5" signature in the stream.
                    {
                        fsbCount++; // Increment the FSB file counter each time an "FSB5" signature is found, indicating a new FSB file.
                        string tempFileName; // Declare a variable to store the name of the temporary FSB file.

                        if (fsbCount > 1) // Check if this is not the first FSB file being extracted.
                        {
                            // For the second FSB file onwards, append a sequence number to the filename to avoid naming conflicts if multiple FSBs have the same base name.
                            tempFileName = $"{baseBankFileName}_{fsbCount}.fsb";
                        }
                        else // If it is the first FSB file being extracted.
                        {
                            // For the first FSB file, use the base name of the BANK file as the temporary FSB file name.
                            tempFileName = $"{baseBankFileName}.fsb";
                        }

                        // Construct the full path for the temporary FSB file by combining the temporary directory path and the generated file name.
                        string tempFilePath = Path.Combine(tempPath, tempFileName);

                        tempFsbFiles.Add(tempFilePath); // Add the full path of the temporary FSB file to the list of temporary FSB files.

                        try // Begin a nested try-catch block to handle exceptions specifically during the creation and writing of temporary FSB files.
                        {
                            using (FileStream tempFsbStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write)) // Create a FileStream for the temporary FSB file in create mode for writing. Ensure stream disposal using 'using'.
                            using (BinaryWriter writer = new BinaryWriter(tempFsbStream)) // Create a BinaryWriter to write binary data to the temporary FSB file stream. Ensure writer disposal using 'using'.
                            {
                                // Read FSB header information (structure based on QuickBMS script analysis)
                                string fsbSign = Encoding.ASCII.GetString(reader.ReadBytes(4)); // Read the 4-byte FSB signature from the BANK file and convert it to an ASCII string. (Should be "FSB5").
                                uint version = reader.ReadUInt32(); // Read the version number of the FSB file (4 bytes).
                                uint numSamples = reader.ReadUInt32(); // Read the number of samples in the FSB file (4 bytes).
                                uint shdrSize = reader.ReadUInt32(); // Read the size of the sound header data in the FSB file (4 bytes).
                                uint nameSize = reader.ReadUInt32(); // Read the size of the name table data in the FSB file (4 bytes).
                                uint dataSize = reader.ReadUInt32(); // Read the size of the audio data in the FSB file (4 bytes).

                                // Calculate the total size of the FSB file data within the BANK file based on header information.
                                uint fsbFileSize = 0x3C + shdrSize + nameSize + dataSize; // 0x3C (60 in decimal) is the size of the basic FSB5 header structure.

                                // Read FSB data from the BANK file and write it to the temporary FSB file.
                                reader.BaseStream.Seek(-24, SeekOrigin.Current); // Seek back in the BANK file stream by 24 bytes. This moves the stream pointer back to the beginning of the FSB header. (Header size is 4 + 4*5 = 24 bytes).
                                byte[] fsbData = reader.ReadBytes((int)fsbFileSize); // Read the entire FSB file data (header + data) from the BANK file based on the calculated file size.
                                writer.Write(fsbData); // Write the read FSB data to the temporary FSB file.
                            }
                        }
                        catch (Exception ex) // Catch any exceptions that occur during temporary FSB file creation or writing.
                        {
                            Console.Error.WriteLine($"Error creating temporary *.fsb file: {tempFilePath} - {ex.Message}"); // Print an error message to the console if temporary FSB file creation fails.
                            // On error, remove the temporary file path from the list and attempt to delete the partially created temporary file.
                            tempFsbFiles.Remove(tempFilePath);
                            if (File.Exists(tempFilePath)) // Check if the temporary file exists before attempting to delete it.
                            {
                                File.Delete(tempFilePath); // Delete the temporary file if it exists.
                            }
                            continue; // Continue to the next iteration of the loop to process any subsequent FSB files within the BANK file.
                        }
                        // Stream pointer movement in FindFSB5Signature function handles offset updates for finding subsequent FSB files within the BANK. No explicit offset update needed here.
                    }
                }
            }
            catch (Exception ex) // Catch any exceptions that occur during the processing of the BANK file itself (e.g., opening, reading errors).
            {
                Console.Error.WriteLine($"*.bank file processing error: {bankFilePath} - {ex.Message}"); // Print an error message to the console if BANK file processing fails.
            }
            return tempFsbFiles; // Return the list of paths to the successfully extracted temporary FSB files.
        }


        /**
         * @brief Main function: Entry point of the FSB Extractor program.
         *
         * @param args Argument array from the command line.
         * @return int Program exit code (0 for success, 1 for error).
         *
         * @details
         * Orchestrates the FSB extraction process.
         * Parses command-line arguments for input file, output directory options (-res, -exe, -o), verbose logging (-v), and help (-h, -help).
         * Initializes FMOD Engine, loads FSB, and extracts sub-sounds as WAV files to the specified output directory.
         * Implements verbose logging and error handling. Returns 0 on success, 1 on error or incorrect usage.
         */
        static int Main(string[] args)
        {
#if _WIN32
            Console.OutputEncoding = Encoding.UTF8; // Sets console output encoding to UTF-8 on Windows to support Unicode characters in console output.
#endif
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US"); // Sets the default thread culture to en-US for consistent number formatting.

            if (args.Length < 1) // Checks if the number of command-line arguments is less than 1.
            {
                Usage_Simple(); // If less than 1 argument, it means no input file path is provided. Display simple usage instructions.
                return 1;       // Return 1 to indicate an error (incorrect usage - missing input file).
            }

            if (args.Length == 1) // If there is exactly one command-line argument (program name and one argument).
            {
                if (args[0] == "-h" || args[0] == "-help") // Check if the argument is "-h" or "-help" (help option).
                {
                    Usage_Detail(); // Display detailed usage instructions if help option is used alone.
                    return 0;       // Return 0 to indicate successful execution (help information displayed).
                }
            }
            if (args.Length > 1) // If there are more than one command-line arguments.
            {
                for (int i = 0; i < args.Length; ++i) // Iterate through the arguments, starting from the first argument (index 0).
                {
                    string arg = args[i]; // Get the current argument.
                    if (arg == "-h" || arg == "-help") // Check if any argument is "-h" or "-help".
                    {
                        Console.Error.WriteLine(" Error: Help option (-h, -help) must be used alone, e.g., `program -h` or `program -help`."); // Display error message if help option is used with other options.
                        Usage_Simple(); // Display simple usage instructions.
                        return 1;       // Return 1 to indicate an error (incorrect usage - help option with other options).
                    }
                }
            }


            string inputFilePath = null;      // Variable to store the path to the input FSB file.
            string outputDirectoryPath = null; // Variable to store the path to the output directory for WAV files.
            bool verboseLogEnabled = false;    // Flag to enable or disable verbose logging.
            StreamWriter logFile = null;     // Output file stream for writing log messages to a file (if verbose logging is enabled).
            int optionCount = 0;              // Counter to track the number of output directory options used (should be at most one).
            bool helpOptionUsed = false;       // Flag to indicate if the help option (-h or -help) was used.

            List<string> tempFilesToDelete = new List<string>(); // List to store temporary FSB file paths

            try // Begin of try block to catch exceptions that might occur during program execution.
            {
                using (FMODSystem fmodSystem = new FMODSystem()) // Create an instance of FMODSystem class, which initializes the FMOD engine. RAII wrapper ensures system is disposed automatically.
                {
                    inputFilePath = args[0]; // Get the input file path from the first command-line argument (args[0]).

                    if (!File.Exists(inputFilePath)) // Check if the input file specified by inputFilePath exists.
                    {
                        Console.Error.WriteLine($"Error: File not found: {inputFilePath}"); // Display error message if the input file does not exist.
                        Usage_Simple(); // Display simple usage instructions.
                        return 1;       // Return 1 to indicate an error (input file not found).
                    }
                    outputDirectoryPath = Path.GetDirectoryName(inputFilePath); // Set the default output directory path to be the same directory as the input FSB file.
                    if (string.IsNullOrEmpty(outputDirectoryPath)) // If the input file path is just a file name (no directory info).
                    {
                        outputDirectoryPath = AppDomain.CurrentDomain.BaseDirectory; // Set the default output directory to the executable directory.
                    }


                    for (int i = 1; i < args.Length; ++i) // Loop through the command-line arguments starting from the second argument (index 1).
                    {
                        string arg = args[i]; // Get the current argument.
                        switch (arg) // Switch statement to handle different command-line options.
                        {
                            case "-res": // Check if the argument is "-res" (output to resource directory option).
                                outputDirectoryPath = Path.GetDirectoryName(inputFilePath); // Set the output directory to the parent directory of the input FSB file.
                                optionCount++; // Increment the output directory option counter.
                                break;
                            case "-exe": // Check if the argument is "-exe" (output to executable directory option).
                                outputDirectoryPath = AppDomain.CurrentDomain.BaseDirectory; // Set the output directory to the current working directory (where the executable is located).
                                optionCount++; // Increment the output directory option counter.
                                break;
                            case "-o": // Check if the argument is "-o" (output to user-specified directory option).
                                if (i + 1 < args.Length) // Check if there is another argument following "-o" (which should be the output directory path).
                                {
                                    outputDirectoryPath = args[++i]; // Get the next argument as the output directory path. Increment 'i' to move to the next argument.
                                    optionCount++; // Increment the output directory option counter.
                                }
                                else // If "-o" is used but no output directory path is provided.
                                {
                                    Console.Error.WriteLine(" Error: -o option requires an output directory path."); // Display error message.
                                    return 1;       // Return 1 to indicate an error (missing output directory for -o option).
                                }
                                break;
                            case "-v": // Check if the argument is "-v" (verbose logging option).
                                verboseLogEnabled = true; // Enable verbose logging.
                                break;
                            case "-h": // Check if the argument is "-h" or "-help" (help option).
                            case "-help":
                                helpOptionUsed = true; // Set the help option used flag to true.
                                break;
                            default: // If an unrecognized command-line argument is encountered.
                                Console.Error.WriteLine(" Error: Invalid option: " + arg); // Display error message for invalid option.
                                Usage_Simple(); // Display simple usage instructions.
                                return 1;       // Return 1 to indicate an error (invalid command-line option).
                        }
                    }

                    if (optionCount > 1 && !helpOptionUsed) // Check if more than one output directory option was used and help option was not used.
                    {
                        Console.Error.WriteLine(" Error: Only one output directory option (-res, -exe, -o <output_directory>) can be used."); // Display error message for using multiple output directory options.
                        Usage_Simple(); // Display simple usage instructions.
                        return 1;       // Return 1 to indicate an error (multiple output directory options used).
                    }

                    if (helpOptionUsed) // If the help option was used.
                    {
                        if (optionCount > 0) // Check if help option was used along with output directory options.
                        {
                            Console.Error.WriteLine(" Error: Cannot use help option (-h, -help) with output directory options (-res, -exe, -o)."); // Display error message.
                            Usage_Simple(); // Display simple usage instructions.
                            return 1;       // Return 1 to indicate an error (help option used with other options).
                        }
                        else // If only help option was used (and no output directory options).
                        {
                            Usage_Detail(); // Display detailed usage instructions.
                            return 0;       // Return 0 to indicate successful execution (help information displayed).
                        }
                    }

                    List<string> filesToProcess = new List<string>();
                    if (inputFilePath.ToLower().EndsWith(".bank"))
                    {
                        filesToProcess = ExtractFSBsFromBankFile(inputFilePath);
                        tempFilesToDelete = filesToProcess; // Store temporary file paths for deletion in finally block
                        if (filesToProcess.Count == 0)
                        {
                            Console.WriteLine($"No FSB files found inside bank file: {inputFilePath}");
                            return 0; // Exit gracefully if no FSBs are found in the bank.
                        }
                    }
                    else
                    {
                        filesToProcess.Add(inputFilePath);
                    }

                    // Added from GUI version: HashSet to track used filenames
                    var usedFileNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    foreach (string currentInputFilePath in filesToProcess)
                    {
                        using (FMODSound soundWrapper = new FMODSound(fmodSystem.Get(), currentInputFilePath)) // Create an instance of FMODSound to load the FSB file using FMOD. RAII wrapper ensures sound is released automatically.
                        {
                            FMOD.Sound sound = soundWrapper.Get(); // Get the raw FMOD.Sound object from the FMODSound wrapper.

                            CheckFMODResult(sound.getNumSubSounds(out int numSubSounds), "FMOD::Sound::getNumSubSounds failed"); // Get the number of sub-sounds contained within the loaded FSB file.

                            if (numSubSounds > 0) // If the FSB file contains one or more sub-sounds.
                            {
                                Console.WriteLine("");
                                if (!string.IsNullOrEmpty(currentInputFilePath))
                                {
                                    Console.WriteLine($" ===== '{Path.GetFileName(currentInputFilePath)}' Processing Start ====="); // Display start processing message in console.
                                }
                                else
                                {
                                    Console.WriteLine(" ===== Processing Start ====="); // Display generic start processing message in console if input file path is empty.
                                }
                                Console.WriteLine();

                                string baseFileName = Path.GetFileNameWithoutExtension(inputFilePath); // Use original input file name for base
                                string outputDirectory = Path.Combine(outputDirectoryPath, baseFileName); // Create a subdirectory within the specified output directory, named after the input FSB file (without extension).
                                string logFilePath; // Variable to store the path for the log file (if verbose logging is enabled).


                                try // Try to create the output directory.
                                {
                                    if (!Directory.Exists(outputDirectory)) // Check if the output directory already exists.
                                    {
                                        Directory.CreateDirectory(outputDirectory); // Create the output directory if it doesn't exist.
                                    }
                                    Console.WriteLine(" Created directory: " + Path.GetFullPath(outputDirectory)); // Display message in console if directory is successfully created.
                                }
                                catch (Exception ex) // Catch any exceptions that might occur during directory creation.
                                {
                                    Console.Error.WriteLine($"Error creating directory: {outputDirectory} - {ex.Message}"); // Display error message if directory creation fails.
                                    outputDirectory = outputDirectoryPath; // If directory creation fails, fallback to using the originally specified output directory path.
                                }

                                if (verboseLogEnabled) // If verbose logging is enabled.
                                {
                                    logFilePath = Path.Combine(outputDirectory, "_" + baseFileName + ".log"); // Construct the log file path within the output directory. Log file name is based on FSB file name.

                                    try // Try to create and open the log file.
                                    {
                                        logFile = new StreamWriter(logFilePath, false, Encoding.UTF8); // Open the log file in create mode (overwrite if it already exists) with UTF-8 encoding.

                                        WriteLogMessage(logFile, "INFO", "Main", $"Log file opened: {Path.GetFullPath(logFilePath)}", verboseLogEnabled); // Write an info message to the log file indicating it was opened.
                                        WriteLogMessage(logFile, "INFO", "Main", $"Processing FSB file: {Path.GetFullPath(currentInputFilePath)}", verboseLogEnabled); // Write an info message to the log file indicating FSB file processing started.

                                        Console.WriteLine(" Log file path: " + Path.GetFullPath(logFilePath)); // Display the log file path in console.
                                    }
                                    catch (Exception ex) // Catch any exceptions that might occur during log file creation or opening.
                                    {
                                        Console.Error.WriteLine($"Error creating log file: {logFilePath} - {ex.Message}"); // Display error message if log file creation fails.
                                        verboseLogEnabled = false; // Disable verbose logging if the log file cannot be opened.
                                        logFile = null; // Set logFile to null as it failed to open.
                                    }
                                }

                                for (int i = 0; i < numSubSounds; ++i) // Loop through each sub-sound in the FSB file.
                                {
                                    FMOD.Sound subSound = new FMOD.Sound(); // Pointer to store the current sub-sound object.
                                    FMOD.RESULT result = sound.getSubSound(i, out subSound); // Get the i-th sub-sound from the FSB file.

                                    if (result != FMOD.RESULT.OK) // Check if getting the sub-sound was successful.
                                    {
                                        Console.Error.WriteLine($" FMOD::Sound::getSubSound failed for sub-sound {i}: {FMOD.Error.String(result)}"); // Display error message if getting sub-sound fails.
                                        continue; // If getting sub-sound fails, continue to the next iteration (next sub-sound).
                                    }

                                    if (result == FMOD.RESULT.OK) // If getting the sub-sound was successful.
                                    {
                                        // Pass usedFileNames to ProcessSubSound
                                        ProcessSubSound(fmodSystem.Get(), subSound, i, numSubSounds, baseFileName, outputDirectory, verboseLogEnabled, logFile, usedFileNames); // Process the current sub-sound (extract audio and save as WAV).
                                        subSound.clearHandle(); // Release the current sub-sound object handle to free up resources after processing.
                                    }
                                    else // If subSound is not valid after getSubSound (shouldn't happen if result is not FMOD.RESULT.OK, but as a safety check)
                                    {
                                        Console.Error.WriteLine($"Error: subSound is invalid after getSubSound for sub-sound {i}"); // Display error message indicating subSound is invalid.
                                    }
                                }
                            }
                            else // If no sub-sounds were found in the FSB file.
                            {
                                Console.WriteLine(" No sub-sounds found in the audio file."); // Display message in console indicating no sub-sounds found.
                            }
                        }
                        // No need to delete temporary files here, moved to finally block
                        Console.WriteLine();
                        Console.WriteLine($" ===== '{Path.GetFileName(inputFilePath)}' Processing End ====="); // Display end processing message in console.
                    }


                }
            }
            catch (Exception e) // Catch any standard exceptions that might be thrown within the try block.
            {
                Console.Error.WriteLine("\n\n");
                Console.Error.WriteLine(" ===== ERROR ===== "); // Display error section header in console.
                Console.Error.WriteLine(" Exception caught: " + e.Message); // Display the exception message.
                Console.Error.WriteLine("");
                Console.Error.WriteLine(" Press any key to continue..."); // Prompt the user to press any key to continue after an error occurred.
                Console.ReadKey(); // Wait for user input (press any key) before exiting, allowing user to read the error message.
                return 1;       // Return 1 to indicate program termination due to an error.
            }
            finally // Ensure resources are released regardless of exceptions in the main function.
            {
                if (logFile != null) // Check if the log file is open (meaning verbose logging was enabled and log file was successfully opened).
                {
                    logFile.WriteLine(); // Add a newline at the end of the log file for better formatting.
                    WriteLogMessage(logFile, "INFO", "Main", $"Processing finished for input file: {Path.GetFileName(inputFilePath)}", verboseLogEnabled, FMOD.RESULT.OK); // Write an info message to the log file indicating FSB file processing finished.
                    logFile.Close(); // Close the log file.
                }

                // Delete temporary FSB files if BANK file was processed
                if (inputFilePath != null && inputFilePath.ToLower().EndsWith(".bank"))
                {
                    foreach (string tempFile in tempFilesToDelete)
                    {
                        if (File.Exists(tempFile))
                        {
                            try
                            {
                                File.Delete(tempFile);
                            }
                            catch (Exception ex)
                            {
                                Console.Error.WriteLine($" \r\n Error deleting temporary FSB file: {tempFile} - {ex.Message}");
                            }
                        }
                    }
                }
            }


            return 0; // Return 0 to indicate successful program execution.
        }
    }
}