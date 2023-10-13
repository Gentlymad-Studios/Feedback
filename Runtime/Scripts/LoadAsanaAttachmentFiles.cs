using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Debug = UnityEngine.Debug;

public class LoadAsanaAttachmentFiles {
    private AsanaAPISettings settings;
    private string attachmentPath;
    private string savegamePath;
    private string logPath;
    private string tempDirPath;
    private string tempDirName = "\\_temp_savegame.zip";
    private Dictionary<string, string> stringFileRepresentation = new Dictionary<string, string>();

    public LoadAsanaAttachmentFiles(AsanaAPISettings settings) {
        this.settings = settings;
        attachmentPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
            .Replace("Roaming", "LocalLow") + settings.AttachmentLocation;
        savegamePath = attachmentPath + settings.SavegameLocation;
        tempDirPath = savegamePath + tempDirName;
        logPath = attachmentPath + settings.LogLocation;
    }

    public Dictionary<string, string> LoadAttachments(AsanaProject project) {
        stringFileRepresentation.Clear();

        if (project.includeLatesOutputLog) { LoadLatestOutputLog();  }
        if (project.includeLatestSavegame) { LoadLatestSavegame();  }
        if (project.includeGlobalCustomFiles) { LoadCustomFileList(); }
        if (project.includeCustomFiles) { LoadCustomFileList(project.CustomFiles); }

        return stringFileRepresentation;
    }

    private void LoadCustomFileList() {
        settings.GlobalCustomFiles.ForEach(path => {
            string loc = Path.Combine(attachmentPath, path);
            if (File.Exists(loc)) {
                string text = File.ReadAllText(loc);
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
                string text = File.ReadAllText(loc);
                string name = Path.GetFileName(loc);
                stringFileRepresentation.Add(name, text);
            } else {
                Debug.LogWarning($"[FeedbackService] File not found ({loc}).");
            }
        });
    }

    private void LoadLatestOutputLog(){
        var logDirectory = new DirectoryInfo(logPath);

        if (!logDirectory.Exists) {
            Debug.LogWarning($"[FeedbackService] Log Directory not found ({logPath}).");
            return;
        }

        if (logDirectory.GetFiles().Length == 0) {
            return;
        }
        FileInfo latestLog = logDirectory.GetFiles().OrderByDescending(n => n.LastWriteTime).First();
        string text = File.ReadAllText(latestLog.FullName);
        string name = Path.GetFileName(latestLog.FullName);
        stringFileRepresentation.Add(name, text);
    }

    private void LoadLatestSavegame() {
        var savegameDirectory = new DirectoryInfo(savegamePath);

        if (!savegameDirectory.Exists) {
            Debug.LogWarning($"[FeedbackService] Savegame Directory not found ({savegamePath}).");
            return;
        }

        if (savegameDirectory.GetFiles().Length == 0) {
            return;
        }
        List<FileInfo> orderdFileInfoList = savegameDirectory.GetFiles().OrderByDescending(n => n.LastWriteTime).ToList();

        string firstName = orderdFileInfoList.Find(d => d.Name.Contains(".savegame")).Name;
        List<FileInfo> saveGamesFileInfo = orderdFileInfoList.FindAll(d => d.Name.Contains(firstName));

        foreach (FileInfo fileInfo in saveGamesFileInfo) {
            stringFileRepresentation.Add(fileInfo.Name, File.ReadAllText(fileInfo.FullName));
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

    public Dictionary<string, string> GetFileRepresentatios() {
        return stringFileRepresentation;
    }

    public void DeleteDirectroy() {
        if (Directory.Exists(tempDirPath)) {
            Directory.Delete(tempDirPath, true);
        }
    }
}
