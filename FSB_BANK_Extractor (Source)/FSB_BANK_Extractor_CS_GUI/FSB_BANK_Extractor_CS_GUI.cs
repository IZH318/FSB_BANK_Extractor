/**
 * @file FSB_BANK_Extractor_CS_GUI.cs
 * @brief GUI tool for browsing, playing, and extracting audio from FMOD Sound Bank (.fsb) and Bank (.bank) files.
 * @author (Github) IZH318 (https://github.com/IZH318)
 *
 * @details
 * This application utilizes the FMOD Studio & Core API to analyze, preview, and process FMOD audio containers.
 * It provides a user-friendly interface to explore internal structures of .bank and .fsb files without external tools.
 *
 * Key Features:
 *  - Advanced Analysis: Parses .bank files to identify and extract embedded .fsb containers and sub-sounds.
 *  - Playback System: Integrated audio player with Seek, Loop, and Volume controls for previewing assets.
 *  - Extraction: Converts proprietary FMOD audio streams into standard Waveform Audio (.wav) files.
 *  - Drag & Drop Support:
 *      - Drag files INTO the app to load them.
 *      - Drag nodes OUT of the app to Windows Explorer to extract them immediately.
 *  - Search & Filtering: Real-time recursive search to quickly find Events or Audio files by name.
 *  - Data Export: Supports exporting the file tree structure and metadata to CSV format.
 *  - Logging: Generates detailed TSV (Tab-Separated Values) logs for extraction audits.
 *
 * FMOD Engine & Development Environment Compatibility:
 *
 * Tested Environment:
 *  - FMOD Engine Version: v2.03.06 (Studio API minor release, build 149358)
 *  - Development Environment: Visual Studio 2022 (v143)
 *  - Target Framework: .NET Framework 4.8
 *  - Primary Test Platform: Windows 10 64-bit
 *  - Last Update: 2025-11-25
 *
 * Recommendations for Best Results:
 *  - Use FMOD Engine v2.03.06 libraries (fmod.dll, fmodstudio.dll) for stability.
 *  - Loading the 'Strings Bank' (e.g., Master.strings.bank) is recommended to resolve proper Event paths.
 *
 * Important Notes:
 *  - Compatibility with older FMOD versions (e.g., FMOD Ex) is not guaranteed.
 *  - Ensure the correct architecture (x86/x64) DLLs match your build target.
 */
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FMOD; // Core API
using FMOD.Studio; // Studio API

namespace FSB_BANK_Extractor_CS_GUI
{
    public partial class FSB_BANK_Extractor_CS_GUI : Form
    {
        #region 1. Configuration & Constants

        // Buffer size for file reading operations
        private const int FILE_READ_BUFFER_SIZE = 65536;

        // Maximum number of threads to use when scanning files in parallel
        private const int MAX_PARALLEL_FILES = 4;

        // Header signature to identify FSB5 files
        private const string FSB5_SIGNATURE = "FSB5";

        // Image indexes for the TreeView/ListView icons
        private static class ImageIndex
        {
            public const int File = 0;
            public const int Folder = 1;
            public const int Event = 2;
            public const int Param = 3;
            public const int Bus = 4;
            public const int Vca = 5;
            public const int Audio = 6;
        }

        #endregion

        #region 2. Fields: FMOD Engine & State

        // FMOD System Objects
        private FMOD.Studio.System _studioSystem;
        private FMOD.System _coreSystem;

        // Lock object to ensure thread safety when accessing the Core System
        private static readonly object _coreSystemLock = new object();

        // Playback Objects
        private FMOD.Channel _currentChannel;
        private FMOD.Studio.EventInstance _currentEvent;
        private FMOD.Sound _loadedSound;

        // Playback State
        private bool _isPlaying = false;
        private uint _currentTotalLengthMs = 0;
        private bool _isDraggingSeek = false;

        // UI & Selection State
        private NodeData _currentSelection = null;
        private List<TreeNode> _originalNodes = new List<TreeNode>(); // Backup for search filtering
        private List<string> _tempDirectories = new List<string>();   // Track temp folders for cleanup

        // Timers & Utils
        private readonly System.Windows.Forms.Timer _uiTimer;             // Updates UI (ProgressBar, Time)
        private readonly System.Windows.Forms.Timer _searchDebounceTimer; // Delays search to prevent lag
        private readonly Stopwatch _scanStopwatch = new Stopwatch();      // Measures operation time

        // Process Flags
        private volatile bool _isScanning = false;
        private volatile bool _isClosing = false;
        private bool _isUpdatingChecks = false; // Prevents recursive check events

        // Progress Tracking
        private int _totalFilesToScan = 0;
        private int _processedFilesCount = 0;
        private string _currentProcessingFile = "";

        // Logging Utility
        private LogWriter _logger;

        #endregion

        #region 3. Initialization & Cleanup

        public FSB_BANK_Extractor_CS_GUI()
        {
            InitializeComponent();

            // 1. Setup UI elements (Icons, Menus)
            SetupIcons();
            InitializeUiLogic();

            // 2. Initialize Audio Engine
            InitializeFmod();

            // 3. Setup Event Listeners
            treeViewInfo.AfterCheck += TreeViewInfo_AfterCheck;

            // 4. Start UI Refresh Timer (30ms interval ~ 33fps)
            _uiTimer = new System.Windows.Forms.Timer { Interval = 30 };
            _uiTimer.Tick += UiTimer_Tick;
            _uiTimer.Start();

            // 5. Setup Search Timer (500ms delay)
            _searchDebounceTimer = new System.Windows.Forms.Timer { Interval = 500 };
            _searchDebounceTimer.Tick += SearchDebounceTimer_Tick;

            // 6. Enable Drag & Drop
            this.AllowDrop = true;
            this.DragEnter += (s, e) => e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
            this.DragDrop += MainForm_DragDropAsync;

            // 7. Setup Controls
            trackSeek.Minimum = 0;
            trackSeek.Maximum = 1000;
            chkLoop.CheckedChanged += chkLoop_CheckedChanged;

            // 8. Key Handling
            this.KeyPreview = true;
            this.KeyDown += MainForm_KeyDown;

            // 9. ListView Events
            lvSearchResults.SelectedIndexChanged += LvSearchResults_SelectedIndexChanged;
            lvSearchResults.DoubleClick += LvSearchResults_DoubleClick;
            lvSearchResults.MouseClick += LvSearchResults_MouseClick;
            lvSearchResults.ColumnClick += LvSearchResults_ColumnClick;
        }

        /// <summary>
        /// Populates the ImageList with system icons.
        /// </summary>
        private void SetupIcons()
        {
            if (imageList1.Images.Count == 0)
            {
                imageList1.Images.Add("file", SystemIcons.WinLogo);
                imageList1.Images.Add("folder", SystemIcons.Shield);
                imageList1.Images.Add("event", SystemIcons.Exclamation);
                imageList1.Images.Add("param", SystemIcons.Question);
                imageList1.Images.Add("bus", SystemIcons.Application);
                imageList1.Images.Add("vca", SystemIcons.Hand);
                imageList1.Images.Add("audio", SystemIcons.Information);
            }
        }

        /// <summary>
        /// Initializes the FMOD Studio and Core systems.
        /// </summary>
        private void InitializeFmod()
        {
            try
            {
                CheckFmodResult(FMOD.Studio.System.create(out _studioSystem));
                CheckFmodResult(_studioSystem.getCoreSystem(out _coreSystem));
                CheckFmodResult(_studioSystem.initialize(1024, FMOD.Studio.INITFLAGS.NORMAL, FMOD.INITFLAGS.NORMAL, IntPtr.Zero));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"FMOD Initialization Error: {ex.Message}\nThe application will exit.",
                    "Critical Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        private void InitializeUiLogic()
        {
            SetupAdditionalContextMenu();
            SetupManualBankLoader();
            SetupHelpMenu();
        }

        private void SetupAdditionalContextMenu()
        {
            if (treeViewContextMenu != null)
            {
                // Hide default items if necessary or add new ones
                if (playContextMenuItem != null) playContextMenuItem.Visible = false;
                if (stopContextMenuItem != null) stopContextMenuItem.Visible = false;

                ToolStripMenuItem selectAllItem = new ToolStripMenuItem("Select All");
                selectAllItem.Click += (s, e) => CheckAllInCurrentView();
                treeViewContextMenu.Items.Insert(0, selectAllItem);
            }
        }

        private void SetupManualBankLoader()
        {
            ToolStripMenuItem manualLoadItem = new ToolStripMenuItem("Load Strings Bank (Manual)...");
            manualLoadItem.Click += (s, e) => LoadStringsBankManually();

            // Insert into File menu
            if (menuStrip1.Items.Count > 0 && menuStrip1.Items[0] is ToolStripMenuItem fileMenu)
            {
                fileMenu.DropDownItems.Insert(2, manualLoadItem);
            }
        }

        private void SetupHelpMenu()
        {
            if (menuStrip1 == null || menuStrip1.Items.Count == 0) return;

            if (menuStrip1.Items[0] is ToolStripMenuItem fileMenu)
            {
                ToolStripMenuItem helpItem = new ToolStripMenuItem("Help");
                helpItem.ShortcutKeys = Keys.F1;
                helpItem.Click += (s, e) => ShowHelpForm();

                ToolStripMenuItem aboutItem = new ToolStripMenuItem("About");
                aboutItem.Click += (s, e) => ShowAboutDialog();

                ToolStripSeparator separator = new ToolStripSeparator();

                int exitIndex = fileMenu.DropDownItems.IndexOf(exitToolStripMenuItem);

                if (exitIndex != -1)
                {
                    fileMenu.DropDownItems.Insert(exitIndex, helpItem);
                    fileMenu.DropDownItems.Insert(exitIndex + 1, aboutItem);
                    fileMenu.DropDownItems.Insert(exitIndex + 2, separator);
                }
                else
                {
                    fileMenu.DropDownItems.Add(new ToolStripSeparator());
                    fileMenu.DropDownItems.Add(helpItem);
                    fileMenu.DropDownItems.Add(aboutItem);
                }
            }
        }

        /// <summary>
        /// Cleanup resources when form is closing.
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_isClosing)
            {
                e.Cancel = true; // Already closing
                return;
            }

            e.Cancel = true; // Prevent immediate close to allow cleanup
            _isClosing = true;

            _uiTimer.Stop();
            StopAudio();

            // Dispose Logger
            _logger?.Dispose();

            this.Enabled = false;
            lblStatus.Text = "Closing application... Cleaning up resources...";
            progressBar.Style = ProgressBarStyle.Marquee;
            progressBar.Visible = true;
            Application.DoEvents();

            // Run cleanup on a background task to prevent UI freeze
            Task.Run(() =>
            {
                if (_studioSystem.isValid()) _studioSystem.release();

                // Clean up temporary files created during Drag&Drop
                foreach (var dir in _tempDirectories)
                {
                    try
                    {
                        if (Directory.Exists(dir)) Directory.Delete(dir, true);
                    }
                    catch { /* Ignore cleanup errors */ }
                }

                Environment.Exit(0);
            });
        }

        #endregion

        #region 4. UI Interaction & Input

        // Handle keyboard shortcuts (Ctrl+F for search)
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.F))
            {
                txtSearch.Focus();
                txtSearch.SelectAll();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e) { }

        /// <summary>
        /// Toggles the enabled state of the main UI components.
        /// </summary>
        private void SetUiState(bool enabled)
        {
            if (_isClosing || this.IsDisposed) return;

            menuStrip1.Enabled = enabled;
            panelPlayback.Enabled = enabled;
            treeViewInfo.Enabled = enabled;
            lvSearchResults.Enabled = enabled;
            listViewDetails.Enabled = enabled;
            panelSearch.Enabled = enabled;

            Cursor = enabled ? Cursors.Default : Cursors.WaitCursor;
        }

        /// <summary>
        /// Main UI Loop: Updates status bar, progress, and FMOD engine.
        /// </summary>
        private void UiTimer_Tick(object sender, EventArgs e)
        {
            if (_isClosing) return;

            // 1. Update Scanning Status
            if (_isScanning)
            {
                TimeSpan ts = _scanStopwatch.Elapsed;
                lblElapsedTime.Text = $"Elapsed: {ts.Minutes:D2}:{ts.Seconds:D2}.{ts.Milliseconds / 10:D2}";
                lblStatus.Text = $"Processing... [{_processedFilesCount}/{_totalFilesToScan}] {_currentProcessingFile}";

                if (_totalFilesToScan > 0)
                {
                    int pct = (int)((float)_processedFilesCount / _totalFilesToScan * 100);
                    progressBar.Value = Math.Min(pct, 100);
                }
            }

            // 2. Update FMOD Engine
            if (_studioSystem.isValid()) _studioSystem.update();
            if (_coreSystem.hasHandle()) _coreSystem.update();

            // 3. Update Playback GUI (Seekbar, Time)
            UpdatePlaybackStatus();
        }

        #endregion

        #region 5. File Analysis (Core Logic)

        private async void openFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog { Filter = "FMOD Files|*.bank;*.fsb", Multiselect = true };
            if (ofd.ShowDialog() == DialogResult.OK) await LoadContextAsync(ofd.FileNames);
        }

        private async void openFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK) await LoadContextAsync(new string[] { fbd.SelectedPath });
        }

        private async void MainForm_DragDropAsync(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files.Length > 0) await LoadContextAsync(files);
        }

        /// <summary>
        /// Main entry point for loading and analyzing files/folders.
        /// </summary>
        private async Task LoadContextAsync(IEnumerable<string> inputPaths)
        {
            // --- Preparation Phase ---
            StopAudio();
            _currentTotalLengthMs = 0;
            lblTime.Text = "00:00.000 / 00:00.000";
            SetUiState(false);
            txtSearch.Text = "";

            _isScanning = true;
            _processedFilesCount = 0;
            _totalFilesToScan = 0;
            _currentProcessingFile = "Initializing...";
            _scanStopwatch.Restart();
            lblElapsedTime.Text = "Elapsed: 00:00.00";

            // Clear UI views
            treeViewInfo.BeginUpdate();
            treeViewInfo.Nodes.Clear();
            listViewDetails.Items.Clear();
            _originalNodes.Clear();
            lvSearchResults.Items.Clear();
            lvSearchResults.Visible = false;
            treeViewInfo.Visible = true;

            // Unload previous FMOD banks
            if (_studioSystem.isValid()) _studioSystem.unloadAll();

            try
            {
                // --- Discovery Phase (Threaded) ---
                List<string> allStringsBanks = new List<string>();
                List<string> allContentFiles = new List<string>();

                await Task.Run(() =>
                {
                    foreach (string path in inputPaths)
                    {
                        if (_isClosing) break;

                        if (Directory.Exists(path))
                        {
                            try
                            {
                                allStringsBanks.AddRange(Directory.GetFiles(path, "*.strings.bank", SearchOption.AllDirectories));
                                allContentFiles.AddRange(Directory.GetFiles(path, "*.bank", SearchOption.AllDirectories));
                                allContentFiles.AddRange(Directory.GetFiles(path, "*.fsb", SearchOption.AllDirectories));
                            }
                            catch { /* Ignore access errors */ }
                        }
                        else if (File.Exists(path))
                        {
                            string name = Path.GetFileName(path).ToLower();
                            if (name.EndsWith(".strings.bank")) allStringsBanks.Add(path);
                            else if (name.EndsWith(".bank") || name.EndsWith(".fsb")) allContentFiles.Add(path);
                        }
                    }
                });

                // Remove duplicates
                allStringsBanks = allStringsBanks.Distinct().ToList();
                allContentFiles = allContentFiles
                    .Where(f => !f.ToLower().EndsWith(".strings.bank"))
                    .Distinct()
                    .ToList();

                _totalFilesToScan = allContentFiles.Count;

                // --- Loading Strings Banks (For Name Resolution) ---
                foreach (string sb in allStringsBanks)
                {
                    if (_studioSystem.isValid())
                    {
                        _studioSystem.loadBankFile(sb, LOAD_BANK_FLAGS.NORMAL, out _);
                    }
                }

                // --- Analysis Phase (Parallel) ---
                var resultNodes = new ConcurrentBag<TreeNode>();
                var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = MAX_PARALLEL_FILES };

                await Task.Run(() =>
                {
                    Parallel.ForEach(allContentFiles, parallelOptions, (filePath, loopState) =>
                    {
                        try
                        {
                            if (_isClosing) loopState.Stop();

                            string fileName = Path.GetFileName(filePath);
                            _currentProcessingFile = fileName;

                            TreeNode rootNode = new TreeNode(fileName, ImageIndex.File, ImageIndex.File);
                            string ext = Path.GetExtension(filePath).ToLower();

                            // Analyze specific file type
                            if (ext == ".bank") AnalyzeBankFile(filePath, rootNode);
                            else if (ext == ".fsb") AnalyzeFsbFile(filePath, rootNode);

                            resultNodes.Add(rootNode);
                            Interlocked.Increment(ref _processedFilesCount);
                        }
                        catch
                        {
                            // Count failed files to keep progress bar accurate
                            Interlocked.Increment(ref _processedFilesCount);
                        }
                    });
                });

                if (_isClosing) return;

                // --- UI Population Phase ---
                List<TreeNode> sortedNodes = resultNodes.OrderBy(n => n.Text).ToList();
                treeViewInfo.Nodes.AddRange(sortedNodes.ToArray());

                // Post-process logic for Banks (resolving Events using the Studio System)
                foreach (TreeNode node in treeViewInfo.Nodes)
                {
                    if (_isClosing) return;
                    if (node.Tag is NodeData data && data.Type == NodeType.Bank)
                    {
                        AnalyzeBankLogic((string)data.ExtraInfo, node);
                    }
                }

                // Cache nodes for search
                foreach (TreeNode n in treeViewInfo.Nodes) _originalNodes.Add(n);

                // --- Finalization ---
                _scanStopwatch.Stop();
                _isScanning = false;
                lblElapsedTime.Text = $"Elapsed: {_scanStopwatch.Elapsed:mm\\:ss\\.ff}";
                lblStatus.Text = $"Ready. Processed {_totalFilesToScan} files. (Strings Banks: {allStringsBanks.Count})";
                progressBar.Value = 0;
            }
            catch (Exception ex)
            {
                if (!_isClosing)
                {
                    _isScanning = false;
                    MessageBox.Show($"Error processing files: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            finally
            {
                if (!_isClosing)
                {
                    treeViewInfo.EndUpdate();
                    SetUiState(true);
                }
            }
        }

        private void LoadStringsBankManually()
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "FMOD Strings Bank|*.strings.bank",
                Title = "Select Strings Bank (e.g. Master.strings.bank)"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if (_studioSystem.isValid())
                    {
                        RESULT res = _studioSystem.loadBankFile(ofd.FileName, LOAD_BANK_FLAGS.NORMAL, out Bank sb);
                        if (res == RESULT.OK || res == RESULT.ERR_EVENT_ALREADY_LOADED)
                        {
                            treeViewInfo.BeginUpdate();
                            RefreshNodeNamesRecursive(treeViewInfo.Nodes);
                            treeViewInfo.EndUpdate();

                            MessageBox.Show("Strings Bank loaded. Node names have been refreshed.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            CheckFmodResult(res);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to load strings bank: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void RefreshNodeNamesRecursive(TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Tag is NodeData data)
                {
                    string newName = null;
                    if (data.Type == NodeType.Event && data.Data is EventDescription evt)
                    {
                        evt.getPath(out string p);
                        if (!string.IsNullOrEmpty(p)) newName = p.Substring(p.LastIndexOf('/') + 1);
                    }
                    else if (data.Type == NodeType.Bank && data.Data is Bank bank)
                    {
                        bank.getPath(out string p);
                        if (!string.IsNullOrEmpty(p)) newName = Path.GetFileName(p);
                    }
                    else if (data.Type == NodeType.Bus && data.Data is Bus bus)
                    {
                        bus.getPath(out string p);
                        if (!string.IsNullOrEmpty(p)) newName = p.Substring(p.LastIndexOf('/') + 1);
                    }

                    if (!string.IsNullOrEmpty(newName) && newName != node.Text)
                    {
                        node.Text = newName;
                    }
                }
                if (node.Nodes.Count > 0) RefreshNodeNamesRecursive(node.Nodes);
            }
        }

        /// <summary>
        /// Reads a .bank file to find embedded FSB5 containers.
        /// </summary>
        private void AnalyzeBankFile(string path, TreeNode root)
        {
            if (_isClosing) return;
            root.Tag = new NodeData { Type = NodeType.Bank, ExtraInfo = path };

            try
            {
                var fsbOffsets = new List<uint>();
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    if (fs.Length < 4) return;
                    byte[] buffer = new byte[FILE_READ_BUFFER_SIZE];
                    int bytesRead;
                    long position = 0;

                    // Naive scan for "FSB5" header in binary
                    while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        for (int i = 0; i < bytesRead - 3; i++)
                        {
                            if (buffer[i] == 'F' && buffer[i + 1] == 'S' && buffer[i + 2] == 'B' && buffer[i + 3] == '5')
                            {
                                fsbOffsets.Add((uint)(position + i));
                            }
                        }
                        // Overlap handling
                        if (fs.Position < fs.Length)
                        {
                            fs.Seek(-3, SeekOrigin.Current);
                            position = fs.Position;
                        }
                        else position += bytesRead;
                    }
                }

                if (fsbOffsets.Count == 0) return;

                // Handle Single vs Multiple FSBs inside Bank
                if (fsbOffsets.Count == 1 && CountSubSounds(path, fsbOffsets[0]) > 1)
                {
                    ParseFsbFromSource(path, fsbOffsets[0], root);
                }
                else
                {
                    HashSet<string> usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    int fallbackIndex = 1;

                    foreach (var offset in fsbOffsets)
                    {
                        string rawName = GetFsbInternalName(path, offset);
                        string baseName = !string.IsNullOrEmpty(rawName) ?
                            Path.GetFileNameWithoutExtension(rawName) :
                            Path.GetFileNameWithoutExtension(path);

                        if (string.IsNullOrEmpty(rawName))
                        {
                            if (fallbackIndex > 1) baseName += $"_{fallbackIndex}";
                        }

                        string finalName = baseName + ".fsb";
                        int dupeCounter = 1;

                        // Resolve name collisions
                        while (usedNames.Contains(finalName))
                        {
                            finalName = $"{baseName}_{dupeCounter}.fsb";
                            dupeCounter++;
                        }
                        usedNames.Add(finalName);
                        fallbackIndex++;

                        TreeNode fsbNode = new TreeNode(finalName, ImageIndex.Folder, ImageIndex.Folder);
                        fsbNode.Tag = new NodeData { Type = NodeType.FsbFile, ExtraInfo = path };
                        root.Nodes.Add(fsbNode);
                        ParseFsbFromSource(path, offset, fsbNode);
                    }
                }
            }
            catch { /* File read error */ }
        }

        private void AnalyzeFsbFile(string path, TreeNode root)
        {
            if (_isClosing) return;
            root.Tag = new NodeData { Type = NodeType.FsbFile, ExtraInfo = path };
            ParseFsbFromSource(path, 0, root);
        }

        /// <summary>
        /// Uses FMOD Core API to open the FSB and list sub-sounds.
        /// </summary>
        private void ParseFsbFromSource(string path, uint offset, TreeNode parentNode)
        {
            if (_isClosing) return;

            Sound sound = new Sound();
            Sound subSound = new Sound();

            try
            {
                lock (_coreSystemLock)
                {
                    // Open as a stream, but do not play yet
                    CREATESOUNDEXINFO exinfo = new CREATESOUNDEXINFO { cbsize = Marshal.SizeOf(typeof(CREATESOUNDEXINFO)), fileoffset = offset };

                    if (_coreSystem.createSound(path, MODE.OPENONLY | MODE.CREATESTREAM, ref exinfo, out sound) == RESULT.OK)
                    {
                        sound.getNumSubSounds(out int numSub);
                        if (numSub > 0)
                        {
                            for (int i = 0; i < numSub; i++)
                            {
                                if (_isClosing) break;
                                sound.getSubSound(i, out subSound);
                                AudioInfo info = GetAudioInfo(subSound, i, path, offset);
                                string displayName = string.IsNullOrEmpty(info.Name) ?
                                    $"{Path.GetFileNameWithoutExtension(path)}_{offset}_sub_{i}" : info.Name;

                                TreeNode node = new TreeNode(displayName, ImageIndex.Audio, ImageIndex.Audio)
                                {
                                    Tag = new NodeData { Type = NodeType.AudioData, CachedAudio = info }
                                };
                                parentNode.Nodes.Add(node);
                                SafeRelease(ref subSound);
                            }
                        }
                        else
                        {
                            // Single sound in file
                            AudioInfo info = GetAudioInfo(sound, 0, path, offset);
                            if (info.LengthMs > 0)
                            {
                                string displayName = string.IsNullOrEmpty(info.Name) ?
                                    Path.GetFileNameWithoutExtension(path) : info.Name;

                                TreeNode node = new TreeNode(displayName, ImageIndex.Audio, ImageIndex.Audio)
                                {
                                    Tag = new NodeData { Type = NodeType.AudioData, CachedAudio = info }
                                };
                                parentNode.Nodes.Add(node);
                            }
                        }
                    }
                }
            }
            finally { SafeRelease(ref sound); }
        }

        /// <summary>
        /// Loads the Bank using Studio API to find Events (Logic events, not just audio).
        /// </summary>
        private void AnalyzeBankLogic(string path, TreeNode root)
        {
            if (_isClosing || !_studioSystem.isValid()) return;

            RESULT res = _studioSystem.loadBankFile(path, LOAD_BANK_FLAGS.NORMAL, out Bank bank);

            // Handle case where bank is already loaded
            if (res == RESULT.ERR_EVENT_ALREADY_LOADED)
            {
                _studioSystem.getBankList(out Bank[] loaded);
                foreach (var b in loaded)
                {
                    b.getPath(out string p);
                    if (Path.GetFileName(p) == Path.GetFileName(path))
                    {
                        bank = b;
                        res = RESULT.OK;
                        break;
                    }
                }
            }

            if (res == RESULT.OK)
            {
                if (root.Tag is NodeData nd) nd.Data = bank;

                bank.getEventCount(out int evtCount);
                if (evtCount > 0)
                {
                    TreeNode evtGroup = new TreeNode($"Events ({evtCount})", ImageIndex.Folder, ImageIndex.Folder);
                    root.Nodes.Insert(0, evtGroup);

                    bank.getEventList(out EventDescription[] events);
                    foreach (var evt in events)
                    {
                        evt.getPath(out string p);
                        evt.getID(out GUID id);
                        string name = string.IsNullOrEmpty(p) ? GuidToString(id) : p.Substring(p.LastIndexOf('/') + 1);

                        TreeNode node = new TreeNode(name, ImageIndex.Event, ImageIndex.Event)
                        {
                            Tag = new NodeData { Type = NodeType.Event, Data = evt }
                        };
                        evtGroup.Nodes.Add(node);
                    }
                }
            }
        }

        #endregion

        #region 6. Playback Logic

        private void UpdatePlaybackStatus()
        {
            bool playing = false;
            uint currentPos = 0;

            // Check Channel (Raw Audio)
            if (_currentChannel.hasHandle())
            {
                _currentChannel.isPlaying(out playing);
                if (playing)
                {
                    _currentChannel.getPosition(out currentPos, TIMEUNIT.MS);
                    _currentChannel.getMode(out MODE mode);
                    // Manually handle stopping if not looping
                    if ((mode & MODE.LOOP_NORMAL) == 0 && _currentTotalLengthMs > 0 && currentPos >= _currentTotalLengthMs)
                    {
                        playing = false;
                    }
                }
            }
            // Check Event (Studio Event)
            else if (_currentEvent.isValid())
            {
                _currentEvent.getPlaybackState(out PLAYBACK_STATE state);
                playing = (state == PLAYBACK_STATE.PLAYING || state == PLAYBACK_STATE.STARTING);
                if (playing)
                {
                    _currentEvent.getTimelinePosition(out int pos);
                    currentPos = (uint)pos;
                }
            }

            // Sync Stop State
            if (_isPlaying && !playing) StopAudio();

            // Update Button Text
            if (_isPlaying != playing)
            {
                _isPlaying = playing;
                btnPlay.Text = _isPlaying ? "Pause (||)" : "Play (▶)";
            }

            // Update UI Counters and Seekbar
            if (playing && _currentTotalLengthMs > 0)
            {
                lblTime.Text = $"{TimeSpan.FromMilliseconds(currentPos):mm\\:ss\\.fff} / {TimeSpan.FromMilliseconds(_currentTotalLengthMs):mm\\:ss\\.fff}";
                if (!_isDraggingSeek)
                {
                    int newVal = (int)((float)currentPos / _currentTotalLengthMs * 1000);
                    trackSeek.Value = Math.Min(Math.Max(0, newVal), 1000);
                }
            }
        }

        private void btnPlay_Click(object sender, EventArgs e) => TogglePause();
        private void btnStop_Click(object sender, EventArgs e) => StopAudio();

        private void TogglePause()
        {
            bool playing = false, paused = false;

            // Check State
            if (_currentChannel.hasHandle())
            {
                _currentChannel.isPlaying(out playing);
                _currentChannel.getPaused(out paused);
            }
            else if (_currentEvent.isValid())
            {
                _currentEvent.getPlaybackState(out PLAYBACK_STATE s);
                playing = (s == PLAYBACK_STATE.PLAYING);
                _currentEvent.getPaused(out paused);
            }

            // Toggle Logic
            if (playing && !paused)
            {
                if (_currentChannel.hasHandle()) _currentChannel.setPaused(true);
                if (_currentEvent.isValid()) _currentEvent.setPaused(true);
            }
            else if (paused)
            {
                if (_currentChannel.hasHandle()) _currentChannel.setPaused(false);
                if (_currentEvent.isValid()) _currentEvent.setPaused(false);
            }
            else
            {
                PlaySelection();
            }
        }

        private void PlaySelection()
        {
            if (_currentSelection == null || _isClosing) return;
            StopAudio();

            try
            {
                // Play Raw Audio
                if (_currentSelection.Type == NodeType.AudioData)
                {
                    AudioInfo info = _currentSelection.CachedAudio;
                    _currentTotalLengthMs = info.LengthMs;

                    lock (_coreSystemLock)
                    {
                        CREATESOUNDEXINFO ex = new CREATESOUNDEXINFO { cbsize = Marshal.SizeOf(typeof(CREATESOUNDEXINFO)), fileoffset = (uint)info.FileOffset };
                        SafeRelease(ref _loadedSound);
                        CheckFmodResult(_coreSystem.createSound(info.SourcePath, MODE.CREATESTREAM | MODE.OPENONLY, ref ex, out _loadedSound));

                        Sound soundToPlay;
                        _loadedSound.getNumSubSounds(out int numSub);
                        if (info.Index < numSub) CheckFmodResult(_loadedSound.getSubSound(info.Index, out soundToPlay));
                        else soundToPlay = _loadedSound;

                        CheckFmodResult(_coreSystem.playSound(soundToPlay, new ChannelGroup(IntPtr.Zero), false, out _currentChannel));
                        if (_currentChannel.hasHandle())
                        {
                            _currentChannel.setMode(chkLoop.Checked ? MODE.LOOP_NORMAL : MODE.LOOP_OFF);
                            _currentChannel.setVolume(trackVol.Value / 100.0f);
                        }
                    }
                }
                // Play Event
                else if (_currentSelection.Type == NodeType.Event)
                {
                    EventDescription evt = (EventDescription)_currentSelection.Data;
                    if (evt.isValid())
                    {
                        evt.getLength(out int len);
                        _currentTotalLengthMs = (uint)len;
                        evt.createInstance(out _currentEvent);
                        _currentEvent.setVolume(trackVol.Value / 100.0f);
                        _currentEvent.start();
                    }
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Playback Error: {ex.Message}";
            }
        }

        private void StopAudio()
        {
            if (_currentChannel.hasHandle())
            {
                _currentChannel.stop();
                _currentChannel.clearHandle();
            }
            if (_currentEvent.isValid())
            {
                _currentEvent.stop(STOP_MODE.IMMEDIATE);
                _currentEvent.release();
                _currentEvent.clearHandle();
            }
            SafeRelease(ref _loadedSound);

            _isPlaying = false;
            if (!IsDisposed)
            {
                btnPlay.Text = "Play (▶)";
                trackSeek.Value = 0;
                lblTime.Text = $"00:00.000 / {TimeSpan.FromMilliseconds(_currentTotalLengthMs):mm\\:ss\\.fff}";
            }
        }

        private void trackVol_Scroll(object sender, EventArgs e)
        {
            lblVol.Text = $"Volume: {trackVol.Value}%";
            float vol = trackVol.Value / 100.0f;
            if (_currentChannel.hasHandle()) _currentChannel.setVolume(vol);
            if (_currentEvent.isValid()) _currentEvent.setVolume(vol);
        }

        private void trackSeek_MouseDown(object sender, MouseEventArgs e) => _isDraggingSeek = true;

        private void trackSeek_MouseUp(object sender, MouseEventArgs e)
        {
            if (_currentTotalLengthMs > 0)
            {
                uint newPos = (uint)((float)trackSeek.Value / 1000 * _currentTotalLengthMs);
                if (_currentChannel.hasHandle()) _currentChannel.setPosition(newPos, TIMEUNIT.MS);
                else if (_currentEvent.isValid()) _currentEvent.setTimelinePosition((int)newPos);
            }
            _isDraggingSeek = false;
        }

        private void chkLoop_CheckedChanged(object sender, EventArgs e)
        {
            if (_currentChannel.hasHandle())
            {
                _currentChannel.setMode(chkLoop.Checked ? MODE.LOOP_NORMAL : MODE.LOOP_OFF);
            }
        }

        #endregion

        #region 7. Search Logic

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            // Reset timer on keystroke (Debounce)
            _searchDebounceTimer.Stop();
            _searchDebounceTimer.Start();
        }

        private void SearchDebounceTimer_Tick(object sender, EventArgs e)
        {
            _searchDebounceTimer.Stop();
            PerformSearch(txtSearch.Text);
        }

        private async void PerformSearch(string query)
        {
            string lowerQuery = query.ToLower();
            if (string.IsNullOrWhiteSpace(lowerQuery))
            {
                // Restore TreeView if search is empty
                lvSearchResults.Visible = false;
                treeViewInfo.Visible = true;
                return;
            }

            // Switch to List View
            treeViewInfo.Visible = false;
            lvSearchResults.Visible = true;
            lvSearchResults.Items.Clear();
            lvSearchResults.Clear(); // Clears items AND columns

            lvSearchResults.Columns.Add("", 20, HorizontalAlignment.Center);
            lvSearchResults.Columns.Add("Name", 220);
            lvSearchResults.Columns.Add("Type", 80);
            lvSearchResults.Columns.Add("Path", 300);

            if (_originalNodes.Count > 0)
            {
                SetUiState(false);

                // Perform search on background thread
                List<ListViewItem> results = await Task.Run(() =>
                {
                    List<ListViewItem> items = new List<ListViewItem>();
                    SearchNodesRecursiveToList(_originalNodes, lowerQuery, items);
                    return items;
                });

                SetUiState(true);

                lvSearchResults.BeginUpdate();
                lvSearchResults.Items.AddRange(results.ToArray());
                lvSearchResults.EndUpdate();
            }
        }

        private void SearchNodesRecursiveToList(IEnumerable<TreeNode> nodes, string query, List<ListViewItem> results)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Text.ToLower().Contains(query))
                {
                    ListViewItem item = new ListViewItem("");
                    item.Checked = node.Checked;
                    item.SubItems.Add(node.Text);
                    string type = "Unknown";
                    if (node.Tag is NodeData data) type = data.Type.ToString();
                    item.SubItems.Add(type);
                    item.SubItems.Add(node.FullPath);
                    item.Tag = node.Tag;
                    results.Add(item);
                }
                if (node.Nodes.Count > 0) SearchNodesRecursiveToList(node.Nodes.Cast<TreeNode>(), query, results);
            }
        }

        #endregion

        #region 8. TreeView & Selection Logic

        private void TreeViewInfo_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (_isUpdatingChecks) return;
            _isUpdatingChecks = true;
            CheckAllChildren(e.Node, e.Node.Checked);
            _isUpdatingChecks = false;
        }

        private void CheckAllChildren(TreeNode node, bool isChecked)
        {
            foreach (TreeNode child in node.Nodes)
            {
                child.Checked = isChecked;
                CheckAllChildren(child, isChecked);
            }
        }

        private void treeViewInfo_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                treeViewInfo.SelectedNode = e.Node;
                if (e.Node.Tag is NodeData data) SetupContextMenu(data);
            }
        }

        private void SetupContextMenu(NodeData data)
        {
            bool isAudio = (data.Type == NodeType.AudioData);
            bool hasGuid = (data.Type == NodeType.Event || data.Type == NodeType.Bank);
            extractContextMenuItem.Enabled = isAudio;
            copyGuidContextMenuItem.Enabled = hasGuid;
        }

        private void treeViewInfo_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (_isClosing) return;
            StopAudio();
            _currentSelection = e.Node.Tag as NodeData;
            UpdateDetailsView();
        }

        private void treeViewInfo_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e) => PlaySelection();

        private void LvSearchResults_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvSearchResults.SelectedItems.Count > 0)
            {
                _currentSelection = lvSearchResults.SelectedItems[0].Tag as NodeData;
                UpdateDetailsView();
            }
        }

        private void LvSearchResults_DoubleClick(object sender, EventArgs e)
        {
            if (lvSearchResults.SelectedItems.Count > 0) PlaySelection();
        }

        private void LvSearchResults_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && lvSearchResults.FocusedItem != null && lvSearchResults.FocusedItem.Bounds.Contains(e.Location))
            {
                if (lvSearchResults.FocusedItem.Tag is NodeData data)
                {
                    SetupContextMenu(data);
                    treeViewContextMenu.Show(Cursor.Position);
                }
            }
        }

        private void LvSearchResults_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column == 0) CheckAllInCurrentView();
        }

        /// <summary>
        /// Selects all items in the currently visible view (Tree or List).
        /// </summary>
        private void CheckAllInCurrentView()
        {
            if (lvSearchResults.Visible)
            {
                bool anyUnchecked = false;
                foreach (ListViewItem item in lvSearchResults.Items) if (!item.Checked) { anyUnchecked = true; break; }

                lvSearchResults.BeginUpdate();
                foreach (ListViewItem item in lvSearchResults.Items) item.Checked = anyUnchecked;
                lvSearchResults.EndUpdate();
            }
            else
            {
                treeViewInfo.BeginUpdate();
                _isUpdatingChecks = true;
                bool anyUnchecked = false;
                foreach (TreeNode node in treeViewInfo.Nodes) if (!node.Checked) { anyUnchecked = true; break; }

                foreach (TreeNode node in treeViewInfo.Nodes)
                {
                    node.Checked = anyUnchecked;
                    CheckAllChildren(node, anyUnchecked);
                }
                _isUpdatingChecks = false;
                treeViewInfo.EndUpdate();
            }
        }

        #endregion

        #region 9. Details View & Properties

        private void UpdateDetailsView()
        {
            if (_currentSelection == null) return;

            listViewDetails.Items.Clear();
            listViewDetails.Groups.Clear();
            listViewDetails.BeginUpdate();

            switch (_currentSelection.Type)
            {
                case NodeType.Bank:
                    if (_currentSelection.Data is Bank bank) ShowBankDetails(bank, _currentSelection.ExtraInfo);
                    break;
                case NodeType.Event:
                    if (_currentSelection.Data is EventDescription desc) ShowEventDetails(desc);
                    break;
                case NodeType.AudioData:
                    ShowAudioDetails(_currentSelection.CachedAudio);
                    break;
                case NodeType.Bus:
                    if (_currentSelection.Data is Bus bus) ShowBusDetails(bus);
                    break;
            }

            listViewDetails.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listViewDetails.EndUpdate();

            // Calculate duration for display
            uint len = 0;
            if (_currentSelection.Type == NodeType.AudioData)
            {
                len = _currentSelection.CachedAudio.LengthMs;
            }
            else if (_currentSelection.Type == NodeType.Event && _currentSelection.Data is EventDescription evt)
            {
                evt.getLength(out int l);
                len = (uint)l;
            }
            _currentTotalLengthMs = len;
            lblTime.Text = $"00:00.000 / {TimeSpan.FromMilliseconds(_currentTotalLengthMs):mm\\:ss\\.fff}";

            if (chkAutoPlay.Checked) PlaySelection();
        }

        private void AddDetailItem(string groupName, string propName, string value)
        {
            ListViewGroup grp = listViewDetails.Groups.Cast<ListViewGroup>().FirstOrDefault(x => x.Header == groupName);
            if (grp == null)
            {
                grp = new ListViewGroup(groupName, groupName);
                listViewDetails.Groups.Add(grp);
            }
            var item = new ListViewItem(new[] { groupName, propName, value }) { Group = grp };
            listViewDetails.Items.Add(item);
        }

        private void ShowAudioDetails(AudioInfo info)
        {
            AddDetailItem("Audio Info", "Name", info.Name);
            AddDetailItem("Audio Info", "Source File", Path.GetFileName(info.SourcePath));
            AddDetailItem("Audio Info", "Sub-Sound Index", info.Index.ToString());
            AddDetailItem("Format", "Encoding", info.Type.ToString());
            AddDetailItem("Format", "Container", info.Format.ToString());
            AddDetailItem("Format", "Channels", info.Channels.ToString());
            AddDetailItem("Format", "Bits", info.Bits.ToString());
            AddDetailItem("Time", "Duration (ms)", info.LengthMs.ToString());
            bool hasLoop = (info.Mode & MODE.LOOP_NORMAL) != 0 || (info.LoopStart != 0 || info.LoopEnd != 0);
            AddDetailItem("Looping", "Has Loop Points", hasLoop.ToString());
            AddDetailItem("Looping", "Loop Range (ms)", $"{info.LoopStart} - {info.LoopEnd}");
        }

        private void ShowBankDetails(Bank bank, string path)
        {
            bank.getID(out GUID id);
            AddDetailItem("Bank", "Path", path);
            AddDetailItem("Bank", "GUID", GuidToString(id));
        }

        private void ShowEventDetails(EventDescription evt)
        {
            evt.getID(out GUID id);
            AddDetailItem("Event", "GUID", GuidToString(id));
        }

        private void ShowBusDetails(Bus bus)
        {
            bus.getID(out GUID id);
            AddDetailItem("Bus", "GUID", GuidToString(id));
        }

        #endregion

        #region 10. Export & Extraction

        private void exportCsvToolStripMenuItem_Click(object sender, EventArgs e) => ExportToCsv();
        private void extractCheckedToolStripMenuItem_Click(object sender, EventArgs e) => PerformExtraction(onlyChecked: true);
        private void extractAllToolStripMenuItem_Click(object sender, EventArgs e) => PerformExtraction(onlyChecked: false);
        private void exitToolStripMenuItem_Click(object sender, EventArgs e) => Close();

        /// <summary>
        /// Exports the current tree structure to a CSV file.
        /// </summary>
        private void ExportToCsv()
        {
            if (treeViewInfo.Nodes.Count == 0)
            {
                MessageBox.Show("Nothing to export.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string defaultName = $"FmodExport_{DateTime.Now:yyyy-MM-dd_HH-mm-ss-fff}.csv";
            SaveFileDialog sfd = new SaveFileDialog { Filter = "CSV|*.csv", FileName = defaultName };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("Type,Path,Name,Duration(ms),Source,Index,Encoding,Container,Channels,Bits,LoopStart,LoopEnd,Mode,GUID");
                    ExportNodesRecursive(treeViewInfo.Nodes, sb, "");
                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Export Completed Successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Export Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ExportNodesRecursive(TreeNodeCollection nodes, StringBuilder sb, string path)
        {
            foreach (TreeNode node in nodes)
            {
                string currentPath = string.IsNullOrEmpty(path) ? node.Text : $"{path}/{node.Text}";

                if (node.Tag is NodeData data)
                {
                    string type = data.Type.ToString();
                    string name = SanitizeCsvField(node.Text);
                    string treePath = SanitizeCsvField(currentPath);
                    string duration = "", source = "", index = "", encoding = "", container = "";
                    string channels = "", bits = "", loopStart = "", loopEnd = "", mode = "", guid = "";

                    if (data.Type == NodeType.AudioData)
                    {
                        AudioInfo info = data.CachedAudio;
                        duration = info.LengthMs.ToString();
                        source = SanitizeCsvField(Path.GetFileName(info.SourcePath));
                        index = info.Index.ToString();
                        encoding = info.Type.ToString();
                        container = info.Format.ToString();
                        channels = info.Channels.ToString();
                        bits = info.Bits.ToString();
                        loopStart = info.LoopStart.ToString();
                        loopEnd = info.LoopEnd.ToString();
                        mode = SanitizeCsvField(info.Mode.ToString());
                    }
                    else if (data.Type == NodeType.Event && data.Data is EventDescription evt)
                    {
                        evt.getID(out GUID id);
                        guid = GuidToString(id);
                        evt.getLength(out int len);
                        duration = len.ToString();
                    }
                    else if (data.Type == NodeType.Bank && data.Data is Bank bank)
                    {
                        bank.getID(out GUID id);
                        guid = GuidToString(id);
                        source = SanitizeCsvField(data.ExtraInfo);
                    }

                    sb.AppendLine($"{type},{treePath},{name},{duration},{source},{index},{encoding},{container},{channels},{bits},{loopStart},{loopEnd},{mode},{guid}");
                }
                if (node.Nodes.Count > 0) ExportNodesRecursive(node.Nodes, sb, currentPath);
            }
        }

        private string SanitizeCsvField(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Contains(",") ? $"\"{s}\"" : s;
        }

        /// <summary>
        /// Orchestrates the bulk extraction of audio files.
        /// </summary>
        private async void PerformExtraction(bool onlyChecked)
        {
            int count = 0;
            List<NodeData> extractList = new List<NodeData>();

            // Calculate scope
            if (lvSearchResults.Visible)
            {
                foreach (ListViewItem item in lvSearchResults.CheckedItems)
                {
                    if (item.Tag is NodeData d && d.Type == NodeType.AudioData) extractList.Add(d);
                }
                count = extractList.Count;
            }
            else
            {
                if (treeViewInfo.Nodes.Count == 0) return;
                count = CountAudioNodes(treeViewInfo.Nodes, onlyChecked);
            }

            if (count == 0)
            {
                MessageBox.Show("No items selected for extraction.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                string target = fbd.SelectedPath;
                StopAudio();
                SetUiState(false);

                // --- Init Logger (TSV/CSV Optimized) ---
                if (chkVerboseLog.Checked)
                {
                    string logFile = Path.Combine(target, $"ExtractionLog_{DateTime.Now:yyyyMMdd_HHmmss}.log");
                    _logger = new LogWriter(logFile);
                    _logger.WriteRaw("[INFO] === Extraction Session Started ===");
                    _logger.WriteRaw($"[INFO] App Version: 2.0.0 | FMOD Version: 2.03.06");
                    _logger.WriteRaw($"[INFO] Target Directory: {target}");
                    _logger.WriteRaw($"[INFO] Total Files Queue: {count}");
                    _logger.WriteRaw("");
                    // TSV Header
                    _logger.WriteRaw("Timestamp\tLevel\tSourceFile\tEventName\tResult\tFormat\tDuration(ms)\tOutputPath\tTimeTaken(ms)");
                }

                _isScanning = true;
                _processedFilesCount = 0;
                _totalFilesToScan = count;
                int failedCount = 0;
                long totalExtractedBytes = 0;

                _scanStopwatch.Restart();

                await Task.Run(() =>
                {
                    if (lvSearchResults.Visible)
                    {
                        // Flat List Extraction
                        foreach (var d in extractList)
                        {
                            string sourceName = Path.GetFileName(d.CachedAudio.SourcePath);
                            string safeSourceName = SanitizeFileName(sourceName);
                            string subDir = Path.Combine(target, safeSourceName);
                            Directory.CreateDirectory(subDir);

                            ExtractItemWithLog(d, subDir, ref failedCount, ref totalExtractedBytes);
                            Interlocked.Increment(ref _processedFilesCount);
                        }
                    }
                    else
                    {
                        // Tree Recursive Extraction
                        ExtractNodesToWavRecursive(treeViewInfo.Nodes, target, onlyChecked, ref failedCount, ref totalExtractedBytes);
                    }
                });

                // Cleanup and Reporting
                _isScanning = false;
                _scanStopwatch.Stop();

                if (_logger != null)
                {
                    _logger.WriteRaw("");
                    _logger.WriteRaw("[INFO] === Extraction Session Finished ===");
                    _logger.WriteRaw($"[INFO] Total: {_totalFilesToScan} | Success: {_totalFilesToScan - failedCount} | Failed: {failedCount}");
                    _logger.WriteRaw($"[INFO] Total Output Size: {totalExtractedBytes / 1024.0 / 1024.0:F2} MB");
                    _logger.WriteRaw($"[INFO] Total Elapsed Time: {_scanStopwatch.Elapsed.TotalSeconds:F2} seconds");
                    _logger.Dispose();
                    _logger = null;
                }

                progressBar.Value = 100;
                SetUiState(true);

                lblStatus.Text = $"Processing... [{_totalFilesToScan}/{_totalFilesToScan}] Finished.";
                lblElapsedTime.Text = $"Elapsed: {_scanStopwatch.Elapsed:mm\\:ss\\.ff}";
                Application.DoEvents();

                string reportMessage = $"Process Complete!\n\n" +
                                       $"Total Processed: {_totalFilesToScan}\n" +
                                       $"Success: {_totalFilesToScan - failedCount}\n" +
                                       $"Failed: {failedCount}\n\n" +
                                       $"Elapsed Time: {_scanStopwatch.Elapsed:mm\\:ss\\.ff}";

                MessageBoxIcon icon = failedCount > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information;
                MessageBox.Show(reportMessage, "Extraction Report", MessageBoxButtons.OK, icon);

                progressBar.Value = 0;
                lblStatus.Text = "Ready.";
                lblElapsedTime.Text = "Elapsed: 00:00.00";
            }
        }

        private void ExtractItemWithLog(NodeData data, string outputDir, ref int failedCount, ref long totalBytes)
        {
            string p = Path.Combine(outputDir, SanitizeFileName(data.CachedAudio.Name) + ".wav");
            Stopwatch sw = Stopwatch.StartNew();

            // Return file size if success, -1 if failed
            long writtenBytes = ExtractSingleWav(data.CachedAudio, p);
            sw.Stop();

            if (writtenBytes >= 0)
            {
                Interlocked.Add(ref totalBytes, writtenBytes);

                string sourceName = Path.GetFileName(data.CachedAudio.SourcePath);
                string formatInfo = $"{data.CachedAudio.Format}/{data.CachedAudio.Channels}ch/{data.CachedAudio.Bits}bit";

                _logger?.LogTSV(LogLevel.INFO, sourceName, data.CachedAudio.Name, "OK", formatInfo, data.CachedAudio.LengthMs.ToString(), p, sw.ElapsedMilliseconds.ToString());
            }
            else
            {
                Interlocked.Increment(ref failedCount);
                string sourceName = Path.GetFileName(data.CachedAudio.SourcePath);
                _logger?.LogTSV(LogLevel.ERROR, sourceName, data.CachedAudio.Name, "FAIL", "-", "-", "Error Code Logged", sw.ElapsedMilliseconds.ToString());
            }
        }

        private int CountAudioNodes(TreeNodeCollection nodes, bool check)
        {
            int c = 0;
            foreach (TreeNode n in nodes)
            {
                if (n.Tag is NodeData d && d.Type == NodeType.AudioData && (!check || n.Checked)) c++;
                c += CountAudioNodes(n.Nodes, check);
            }
            return c;
        }

        private void ExtractNodesToWavRecursive(TreeNodeCollection nodes, string path, bool check, ref int failedCount, ref long totalBytes)
        {
            foreach (TreeNode n in nodes)
            {
                string next = path;
                // If not audio data (folder/bank), append to path
                if (!(n.Tag is NodeData d && d.Type == NodeType.AudioData))
                {
                    next = Path.Combine(path, SanitizeFileName(n.Text));
                }

                if (n.Tag is NodeData data && data.Type == NodeType.AudioData && (!check || n.Checked))
                {
                    Directory.CreateDirectory(next);
                    string p = Path.Combine(next, SanitizeFileName(n.Text) + ".wav");

                    Stopwatch sw = Stopwatch.StartNew();
                    long writtenBytes = ExtractSingleWav(data.CachedAudio, p);
                    sw.Stop();

                    if (writtenBytes >= 0)
                    {
                        Interlocked.Add(ref totalBytes, writtenBytes);
                        string sourceName = Path.GetFileName(data.CachedAudio.SourcePath);
                        string formatInfo = $"{data.CachedAudio.Format}/{data.CachedAudio.Channels}ch/{data.CachedAudio.Bits}bit";
                        _logger?.LogTSV(LogLevel.INFO, sourceName, data.CachedAudio.Name, "OK", formatInfo, data.CachedAudio.LengthMs.ToString(), p, sw.ElapsedMilliseconds.ToString());
                    }
                    else
                    {
                        Interlocked.Increment(ref failedCount);
                        string sourceName = Path.GetFileName(data.CachedAudio.SourcePath);
                        _logger?.LogTSV(LogLevel.ERROR, sourceName, data.CachedAudio.Name, "FAIL", "-", "-", "See Error Above", sw.ElapsedMilliseconds.ToString());
                    }

                    Interlocked.Increment(ref _processedFilesCount);
                }

                if (n.Nodes.Count > 0) ExtractNodesToWavRecursive(n.Nodes, next, check, ref failedCount, ref totalBytes);
            }
        }

        /// <summary>
        /// Reads audio data from FMOD and writes a valid WAV file.
        /// Returns bytes written or -1 on failure.
        /// </summary>
        private long ExtractSingleWav(AudioInfo info, string outputPath)
        {
            Sound s = new Sound();
            Sound sub = new Sound();
            long bytesWritten = -1;

            try
            {
                lock (_coreSystemLock)
                {
                    CREATESOUNDEXINFO ex = new CREATESOUNDEXINFO { cbsize = Marshal.SizeOf(typeof(CREATESOUNDEXINFO)), fileoffset = (uint)info.FileOffset };

                    if (_coreSystem.createSound(info.SourcePath, MODE.CREATESTREAM | MODE.OPENONLY, ref ex, out s) != RESULT.OK)
                    {
                        _logger?.WriteRaw($"[ERROR] FMOD createSound failed for {info.SourcePath}");
                        return -1;
                    }

                    s.getNumSubSounds(out int num);
                    if (info.Index < num) s.getSubSound(info.Index, out sub);
                    else sub = s;

                    // Get Raw Data info
                    sub.getLength(out uint lenBytes, TIMEUNIT.PCMBYTES);
                    sub.getFormat(out _, out SOUND_FORMAT fmt, out int ch, out int bits);
                    sub.getDefaults(out float rate, out _);

                    using (FileStream fs = new FileStream(outputPath, FileMode.Create))
                    using (BinaryWriter bw = new BinaryWriter(fs))
                    {
                        // Write WAV Header
                        WriteWavHeader(bw, (int)lenBytes, (int)rate, ch, bits > 0 ? bits : 16, fmt == SOUND_FORMAT.PCMFLOAT);

                        // Decode and Write Data
                        sub.seekData(0);
                        byte[] buf = new byte[4096];
                        uint totalRead = 0;

                        while (totalRead < lenBytes)
                        {
                            sub.readData(buf, out uint read);
                            if (read == 0) break;
                            bw.Write(buf, 0, (int)read);
                            totalRead += read;
                        }
                        bytesWritten = fs.Length;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.WriteRaw($"[ERROR] Exception in ExtractSingleWav: {ex.Message} | File: {info.Name}");
                bytesWritten = -1;
            }
            finally
            {
                // Release sub-sound handle if distinct
                if (sub.hasHandle() && sub.handle != s.handle) sub.release();
                SafeRelease(ref s);
            }
            return bytesWritten;
        }

        private void WriteWavHeader(BinaryWriter bw, int length, int rate, int channels, int bits, bool isFloat)
        {
            bw.Write(Encoding.ASCII.GetBytes("RIFF"));
            bw.Write(36 + length);
            bw.Write(Encoding.ASCII.GetBytes("WAVE"));
            bw.Write(Encoding.ASCII.GetBytes("fmt "));
            bw.Write(16);
            bw.Write((ushort)(isFloat ? 3 : 1));
            bw.Write((short)channels);
            bw.Write(rate);
            bw.Write(rate * channels * bits / 8);
            bw.Write((short)(channels * bits / 8));
            bw.Write((short)bits);
            bw.Write(Encoding.ASCII.GetBytes("data"));
            bw.Write(length);
        }

        private string SanitizeFileName(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars()) name = name.Replace(c, '_');
            return name;
        }

        #endregion

        #region 11. Drag & Drop Out (TreeView to Explorer)

        private void treeViewInfo_ItemDrag(object sender, ItemDragEventArgs e)
        {
            TreeNode draggedNode = e.Item as TreeNode;
            if (draggedNode == null) return;

            try
            {
                List<string> filesToDrag = PrepareDragData(draggedNode);
                if (filesToDrag != null && filesToDrag.Count > 0)
                {
                    DataObject data = new DataObject(DataFormats.FileDrop, filesToDrag.ToArray());
                    treeViewInfo.DoDragDrop(data, DragDropEffects.Copy);
                }
            }
            catch { /* Drag init failed */ }
        }

        private List<string> PrepareDragData(TreeNode node)
        {
            // Create a temp directory for this drag operation
            string tempDirectoryPath = Path.Combine(Path.GetTempPath(), "FmodAnalyzer_Drag_" + Guid.NewGuid());
            Directory.CreateDirectory(tempDirectoryPath);
            _tempDirectories.Add(tempDirectoryPath);

            var fileList = new List<string>();

            if (node.Tag is NodeData data && data.Type == NodeType.AudioData)
            {
                string fileName = SanitizeFileName(node.Text) + ".wav";
                string tempFilePath = Path.Combine(tempDirectoryPath, fileName);
                ExtractSingleWav(data.CachedAudio, tempFilePath);
                fileList.Add(tempFilePath);
            }
            else
            {
                List<TreeNode> audioNodes = new List<TreeNode>();
                FindAudioNodesRecursive(node, audioNodes);

                if (audioNodes.Count > 0)
                {
                    string targetFolder = Path.Combine(tempDirectoryPath, SanitizeFileName(node.Text));
                    Directory.CreateDirectory(targetFolder);

                    foreach (var audioNode in audioNodes)
                    {
                        if (audioNode.Tag is NodeData audioData)
                        {
                            string fileName = SanitizeFileName(audioNode.Text) + ".wav";
                            string tempFilePath = Path.Combine(targetFolder, fileName);
                            ExtractSingleWav(audioData.CachedAudio, tempFilePath);
                        }
                    }
                    fileList.Add(targetFolder);
                }
            }
            return fileList;
        }

        private void FindAudioNodesRecursive(TreeNode startNode, List<TreeNode> foundNodes)
        {
            if (startNode.Tag is NodeData data && data.Type == NodeType.AudioData) foundNodes.Add(startNode);
            foreach (TreeNode childNode in startNode.Nodes) FindAudioNodesRecursive(childNode, foundNodes);
        }

        #endregion

        #region 12. Context Menus & Dialogs

        private void expandAllToolStripMenuItem_Click(object sender, EventArgs e) => treeViewInfo.ExpandAll();
        private void collapseAllToolStripMenuItem_Click(object sender, EventArgs e) => treeViewInfo.CollapseAll();

        private void copyNameContextMenuItem_Click(object sender, EventArgs e)
        {
            if (treeViewInfo.SelectedNode != null) Clipboard.SetText(treeViewInfo.SelectedNode.Text);
        }

        private void copyPathContextMenuItem_Click(object sender, EventArgs e)
        {
            if (treeViewInfo.SelectedNode != null) Clipboard.SetText(treeViewInfo.SelectedNode.FullPath);
        }

        private void copyGuidContextMenuItem_Click(object sender, EventArgs e)
        {
            if (treeViewInfo.SelectedNode?.Tag is NodeData d)
            {
                if (d.Data is EventDescription evt) { evt.getID(out GUID id); Clipboard.SetText(GuidToString(id)); }
                else if (d.Data is Bank bank) { bank.getID(out GUID id); Clipboard.SetText(GuidToString(id)); }
            }
        }

        private void playContextMenuItem_Click(object sender, EventArgs e) => TogglePause();
        private void stopContextMenuItem_Click(object sender, EventArgs e) => StopAudio();

        private async void extractContextMenuItem_Click(object sender, EventArgs e)
        {
            if (treeViewInfo.SelectedNode?.Tag is NodeData data && data.Type == NodeType.AudioData)
            {
                SaveFileDialog sfd = new SaveFileDialog { Filter = "WAV|*.wav", FileName = SanitizeFileName(treeViewInfo.SelectedNode.Text) + ".wav" };
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    bool success = await Task.Run(() => ExtractSingleWav(data.CachedAudio, sfd.FileName) >= 0);
                    if (!success)
                    {
                        MessageBox.Show("Failed to extract audio file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ShowHelpForm()
        {
            HelpForm helpForm = new HelpForm();
            helpForm.Show();
        }

        private void ShowAboutDialog()
        {
            MessageBox.Show("FSB/BANK Extractor GUI\n" +
                            "Version: 2.0.0\n" +
                            "Update: 2025-11-25\n\n" +
                            "Developer: (GitHub) IZH318\n" +
                            "Website: https://github.com/IZH318\n\n" +
                            "Using FMOD Studio API version 2.03.06\n" +
                            " - Studio API minor release (build 149358)\n\n" +
                            "© 2025 (GitHub) IZH318. All rights reserved.", "Program Information");
        }

        #endregion

        #region 13. Helper Methods & Classes

        private void CheckFmodResult(RESULT result)
        {
            if (result != RESULT.OK) throw new Exception($"FMOD Error [{result}]: {FMOD.Error.String(result)}");
        }

        /// <summary>
        /// Safely releases an FMOD Sound handle to prevent memory leaks.
        /// </summary>
        private void SafeRelease(ref Sound sound)
        {
            if (sound.hasHandle())
            {
                sound.release();
                sound.clearHandle();
            }
        }

        private string GuidToString(GUID g) => $"{g.Data1:X8}-{g.Data2:X4}-{g.Data3:X4}-{g.Data4:X8}";

        private int CountSubSounds(string path, uint offset)
        {
            int numSub = 0;
            Sound sound = new Sound();
            try
            {
                lock (_coreSystemLock)
                {
                    CREATESOUNDEXINFO exinfo = new CREATESOUNDEXINFO { cbsize = Marshal.SizeOf(typeof(CREATESOUNDEXINFO)), fileoffset = offset };
                    if (_coreSystem.createSound(path, MODE.OPENONLY | MODE.CREATESTREAM, ref exinfo, out sound) == RESULT.OK)
                    {
                        sound.getNumSubSounds(out numSub);
                    }
                }
            }
            finally { SafeRelease(ref sound); }
            return numSub;
        }

        private string GetFsbInternalName(string path, uint offset)
        {
            string name = "";
            Sound sound = new Sound();
            try
            {
                lock (_coreSystemLock)
                {
                    CREATESOUNDEXINFO exinfo = new CREATESOUNDEXINFO { cbsize = Marshal.SizeOf(typeof(CREATESOUNDEXINFO)), fileoffset = offset };
                    if (_coreSystem.createSound(path, MODE.OPENONLY | MODE.CREATESTREAM, ref exinfo, out sound) == RESULT.OK)
                    {
                        sound.getName(out name, 256);
                    }
                }
            }
            finally { SafeRelease(ref sound); }
            return name;
        }

        private static AudioInfo GetAudioInfo(Sound sub, int index, string path, long offset)
        {
            AudioInfo info = new AudioInfo { Index = index, SourcePath = path, FileOffset = offset };
            sub.getName(out info.Name, 256);
            sub.getLength(out info.LengthMs, TIMEUNIT.MS);
            sub.getLength(out info.LengthPcm, TIMEUNIT.PCM);
            sub.getFormat(out info.Type, out info.Format, out info.Channels, out info.Bits);
            sub.getLoopPoints(out info.LoopStart, TIMEUNIT.MS, out info.LoopEnd, TIMEUNIT.MS);
            sub.getMode(out info.Mode);
            return info;
        }

        // --- Data Structures ---

        /// <summary>
        /// Represents the data attached to a TreeView Node or ListView Item.
        /// </summary>
        private class NodeData
        {
            public NodeType Type { get; set; }
            public object Data { get; set; } // Can be Bank, EventDescription, or Bus
            public string ExtraInfo { get; set; } // Used for paths
            public AudioInfo CachedAudio { get; set; } // Used if Type is AudioData
        }

        /// <summary>
        /// Snapshot of audio properties to avoid calling FMOD continuously.
        /// </summary>
        public struct AudioInfo
        {
            public string Name;
            public uint LengthMs;
            public uint LengthPcm;
            public SOUND_TYPE Type;
            public SOUND_FORMAT Format;
            public int Channels;
            public int Bits;
            public uint LoopStart;
            public uint LoopEnd;
            public MODE Mode;
            public int Index;
            public string SourcePath;
            public long FileOffset;
        }

        private enum NodeType { Bank, Event, Bus, VCA, FsbFile, SubSound, AudioData }

        // --- Enhanced Logging Helper Class ---
        public enum LogLevel { INFO, WARNING, ERROR }

        public class LogWriter : IDisposable
        {
            private StreamWriter _writer;
            private object _lock = new object();

            public LogWriter(string path)
            {
                try
                {
                    _writer = new StreamWriter(path, false, Encoding.UTF8) { AutoFlush = true };
                }
                catch { /* Logging Init Failed */ }
            }

            // Standard Message Log
            public void WriteRaw(string message)
            {
                if (_writer == null) return;
                try
                {
                    lock (_lock) { _writer.WriteLine(message); }
                }
                catch { }
            }

            // TSV Structured Log for Spreadsheet Import
            public void LogTSV(LogLevel level, string sourceFile, string eventName, string result, string format, string duration, string output, string timeTaken)
            {
                if (_writer == null) return;
                try
                {
                    string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    string line = $"{timestamp}\t{level}\t{sourceFile}\t{eventName}\t{result}\t{format}\t{duration}\t{output}\t{timeTaken}";
                    lock (_lock) { _writer.WriteLine(line); }
                }
                catch { }
            }

            public void Dispose()
            {
                if (_writer != null)
                {
                    try { _writer.Close(); _writer.Dispose(); } catch { }
                    _writer = null;
                }
            }
        }

        #endregion
    }
}