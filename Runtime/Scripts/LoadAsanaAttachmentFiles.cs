using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Debug = UnityEngine.Debug;

namespace Feedback {
    public class LoadAsanaAttachmentFiles {
        private AsanaAPISettings settings;
        private string attachmentPath;
        private string logPath;
        private Dictionary<string, string> stringFileRepresentation = new Dictionary<string, string>();

        public LoadAsanaAttachmentFiles(AsanaAPISettings settings) {
            this.settings = settings;
            attachmentPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                .Replace("Roaming", "LocalLow") + settings.AttachmentLocation;
            logPath = attachmentPath + settings.LogLocation;
        }

        public Dictionary<string, string> LoadAttachments(AsanaProject project) {
            stringFileRepresentation.Clear();

            if (project.includeOutputLog) {
                LoadLatestOutputLog();
            }
            if (project.includeSavegame) {
                LoadLatestSavegame();
            }
            if (project.includeGlobalCustomFiles) {
                LoadCustomFileList();
            }
            if (project.includeCustomFiles) {
                LoadCustomFileList(project.CustomFiles);
            }

            return stringFileRepresentation;
        }

        private void LoadCustomFileList() {
            settings.GlobalCustomFiles.ForEach(path => {
                string loc = Path.Combine(attachmentPath, path);
                if (File.Exists(loc)) {
                    string text;
                    using (FileStream logFileStream = new FileStream(loc, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                        using (StreamReader logFileReader = new StreamReader(logFileStream)) {
                            text = logFileReader.ReadToEnd();
                        }
                    }
                    string name = Path.GetFileName(loc);
                    stringFileRepresentation.Add(name, text);
                } else {
                    Debug.LogWarning($"[FeedbackService] File not found ({loc}).");
                }
            });
        }

        private void LoadCustomFileList(List<string> files) {
            files.ForEach(path => {
                string loc = Path.Combine(attachmentPath, path);
                if (File.Exists(loc)) {
                    string text;
                    using (FileStream logFileStream = new FileStream(loc, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                        using (StreamReader logFileReader = new StreamReader(logFileStream)) {
                            text = logFileReader.ReadToEnd();
                        }
                    }
                    string name = Path.GetFileName(loc);
                    stringFileRepresentation.Add(name, text);
                } else {
                    Debug.LogWarning($"[FeedbackService] File not found ({loc}).");
                }
            });
        }

        private void LoadLatestOutputLog() {
            var logDirectory = new DirectoryInfo(logPath);

            if (!logDirectory.Exists) {
                Debug.LogWarning($"[FeedbackService] Log Directory not found ({logPath}).");
                return;
            }

            if (logDirectory.GetFiles().Length == 0) {
                return;
            }
            FileInfo latestLog = logDirectory.GetFiles().OrderByDescending(n => n.LastWriteTime).First();
            string text;
            using (FileStream logFileStream = new FileStream(latestLog.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                using (StreamReader logFileReader = new StreamReader(logFileStream)) {
                    text = logFileReader.ReadToEnd();
                }
            }
            string name = Path.GetFileName(latestLog.FullName);
            stringFileRepresentation.Add(name, text);
        }

        private void LoadLatestSavegame() {
            List<string> savegameDataPaths = settings.Adapter.GetSavegame();

            if (savegameDataPaths == null) {
                return;
            }

            for (int i = 0; i < savegameDataPaths.Count; i++) {
                var savegame = new FileInfo(savegameDataPaths[i]);

                if (!savegame.Exists) {
                    Debug.LogWarning($"[FeedbackService] Savegame Directory not found ({savegame}).");
                    continue;
                }

                stringFileRepresentation.Add(savegame.Name, File.ReadAllText(savegame.FullName));
            }
        }

        //Todo: to send zip files, the content type application/zip is required for Http requests. To use it, adjust the buildAttachment method
        //the generic RequestData object and the implementation in AsanaRequestManager. 
        //https://stackoverflow.com/questions/834527/how-do-i-generate-and-send-a-zip-file-to-a-user-in-c-sharp-asp-net
        private string CreateZipArchive(string fileName, List<FileInfo> sources) {
            try {
                using (var zip = ZipFile.Open(fileName, ZipArchiveMode.Create)) {
                    foreach (var source in sources) {
                        zip.CreateEntryFromFile(source.FullName, source.Name);
                    }
                }
                string f = File.ReadAllBytes(fileName).ToString();
                return f;
            } catch (Exception e) {
                Debug.LogError(e);
            }

            return null;
        }
    }
}