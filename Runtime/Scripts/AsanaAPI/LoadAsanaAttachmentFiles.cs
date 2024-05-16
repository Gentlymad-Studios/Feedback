using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = System.Random;

namespace Feedback {
    public class LoadAsanaAttachmentFiles {
        private AsanaAPISettings settings;
        private string attachmentPath;
        private string tempPath;
        private List<AsanaTicketRequest.Attachment> attachments = new List<AsanaTicketRequest.Attachment>();

        public LoadAsanaAttachmentFiles(AsanaAPISettings settings) {
            this.settings = settings;
            attachmentPath = Path.Combine(Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)).FullName, string.Format("LocalLow/{0}/{1}/", Application.companyName, Application.productName));
            tempPath = Path.Combine(attachmentPath, "Temp");
        }

        public List<AsanaTicketRequest.Attachment> LoadAttachments(AsanaProject project, List<Texture2D> images, ErrorHandler errorHandler) {
            attachments.Clear();

            LoadImages(images);

            if (project.includeErrorLog) {
                LoadFirstErrors(errorHandler, project.errorLogCount);
            }
            if (project.includePlayerLog) {
                LoadFileList(new List<string> { "Player.log" });
            }
            if (project.includeCustomLog) {
                LoadLog();
            }
            if (project.includeSavegame) {
                LoadSavegame();
            }
            if (project.includeGlobalFiles) {
                LoadFileList(settings.Files);
                LoadArchiveFiles(settings.ArchivedFiles);
            }
            if (project.includeProjectFiles) {
                LoadFileList(project.Files);
                LoadArchiveFiles(project.ArchivedFiles);
            }

            return attachments;
        }

        public List<string> LoadAttachmentsDummy(AsanaProject project, ErrorHandler errorHandler) {
            List<string> data = new List<string>();

            //Sreenshot
            data.Add("screenshot.jpg");

            //ErrorLog
            if (project.includeErrorLog && errorHandler != null && errorHandler.ErrorList.Count != 0) {
                data.Add("firstErrors.log");
            }

            //Player Log
            if (project.includePlayerLog) {
                data.Add("Player.log");
            }

            //Custom Log
            if (project.includeCustomLog) {
                data.AddRange(LoadLog(true));
            }

            //Savegame
            if (project.includeSavegame) {
                data.AddRange(LoadSavegame(true));
            }

            //Global Files
            if (project.includeGlobalFiles) {
                data.AddRange(settings.Files);
                for (int i = 0; i < settings.ArchivedFiles.Count; i++) {
                    data.Add(settings.ArchivedFiles[i].name + ".zip");
                }
            }

            //Project Files
            if (project.includeProjectFiles) {
                data.AddRange(project.Files);

                for (int i = 0; i < project.ArchivedFiles.Count; i++) {
                    data.Add(project.ArchivedFiles[i].name + ".zip");
                }
            }

            return data;
        }

        private void LoadImages(List<Texture2D> images) {
            for (int i = 0; i < images.Count; i++) {
                AsanaTicketRequest.Attachment attachment = new AsanaTicketRequest.Attachment();
                attachment.filename = "screenshot.jpg";
                attachment.contentType = AsanaTicketRequest.ContentTypes.Image;

                byte[] jpg = images[i].EncodeToJPG();
                if (jpg.LongLength > settings.maxFileSize) {
                    Debug.Log($"File is to large {attachment.filename}.");
                } else {
                    attachment.content = Convert.ToBase64String(jpg);
                    attachments.Add(attachment);
                }
            }
        }

        private void LoadFileList(List<string> files) {
            files.ForEach(path => {
                string loc = Path.Combine(attachmentPath, path);
                AsanaTicketRequest.Attachment attachment = LoadAttachment(loc, AsanaTicketRequest.ContentTypes.Text, true);
                if (attachment != null) {
                    attachments.Add(attachment);
                }
            });
        }

        private void LoadArchiveFiles(List<ArchivedFiles> files) {
            files.ForEach(archive => {
                List<string> paths = new List<string>();
                for (int i = 0; i < archive.Files.Count; i++) {
                    paths.Add(Path.Combine(attachmentPath, archive.Files[i]));
                }

                AsanaTicketRequest.Attachment attachment = CreateZipArchive(archive.name, paths);
                if (attachment != null) {
                    attachments.Add(attachment);
                }
            });
        }

        private void LoadFirstErrors(ErrorHandler errorHandler, int errorCount) {
            if (errorHandler.ErrorList.Count == 0) {
                return;
            }

            if (!Directory.Exists(tempPath)) {
                Directory.CreateDirectory(tempPath);
            }

            string file = Path.Combine(tempPath, "firstErrors.log");

            if (errorCount == 0) {
                errorCount = errorHandler.ErrorList.Count;
            } else {
                errorCount = Math.Min(errorCount, errorHandler.ErrorList.Count);
            }

            using (StreamWriter writer = new StreamWriter(file, false)) {
                for (int i = 0; i < errorCount; i++) {
                    writer.WriteLine(errorHandler.ErrorList[i].LogString);
                    writer.WriteLine(errorHandler.ErrorList[i].StackTrace + "\n");
                }
            }

            AsanaTicketRequest.Attachment attachment = LoadAttachment(file, AsanaTicketRequest.ContentTypes.Text, true);
            if (attachment != null) {
                attachments.Add(attachment);
            }
        }

        private List<string> LoadLog(bool dummy = false) {
            List<string> logDataPaths = settings.Adapter.GetLog(out bool archive, out string archiveName);
            List<string> dummyList = new List<string>();

            if (logDataPaths == null) {
                return dummyList;
            }

            if (archive) {
                if (dummy) {
                    dummyList.Add(archiveName + ".zip");
                } else {
                    AsanaTicketRequest.Attachment attachment = CreateZipArchive(archiveName, logDataPaths);
                    if (attachment != null) {
                        attachments.Add(attachment);

                    }
                }
            } else {
                for (int i = 0; i < logDataPaths.Count; i++) {
                    if (dummy) {
                        dummyList.Add(logDataPaths[i]);
                    } else {
                        AsanaTicketRequest.Attachment attachment = LoadAttachment(logDataPaths[i], AsanaTicketRequest.ContentTypes.Text, true);
                        if (attachment != null) {
                            attachments.Add(attachment);
                        }
                    }
                }
            }

            return dummyList;
        }

        private List<string> LoadSavegame(bool dummy = false) {
            List<string> savegameDataPaths = settings.Adapter.GetSavegame(out bool archive, out string archiveName);
            List<string> dummyList = new List<string>();

            if (savegameDataPaths == null) {
                return dummyList;
            }

            if (archive) {
                if (dummy) {
                    dummyList.Add(archiveName + ".zip");
                } else {
                    AsanaTicketRequest.Attachment attachment = CreateZipArchive(archiveName, savegameDataPaths);
                    if (attachment != null) {
                        attachments.Add(attachment);
                    }
                }
            } else {
                for (int i = 0; i < savegameDataPaths.Count; i++) {
                    if (dummy) {
                        dummyList.Add(savegameDataPaths[i]);
                    } else {
                        AsanaTicketRequest.Attachment attachment = LoadAttachment(savegameDataPaths[i], AsanaTicketRequest.ContentTypes.Text, false);
                        if (attachment != null) {
                            attachments.Add(attachment);
                        }
                    }
                }
            }

            return dummyList;
        }

        //Todo: to send zip files, the content type application/zip is required for Http requests. To use it, adjust the buildAttachment method
        //the generic RequestData object and the implementation in AsanaRequestManager. 
        //https://stackoverflow.com/questions/834527/how-do-i-generate-and-send-a-zip-file-to-a-user-in-c-sharp-asp-net
        private AsanaTicketRequest.Attachment CreateZipArchive(string fileName, List<string> files) {
            AsanaTicketRequest.Attachment attachment = new AsanaTicketRequest.Attachment();
            fileName += ".zip";

            List<FileInfo> sources = new List<FileInfo>();
            for (int i = 0; i < files.Count; i++) {
                FileInfo fileInfo = new(files[i]);
                if (fileInfo.Exists) {
                    sources.Add(fileInfo);
                } else {
                    Debug.LogWarning($"[FeedbackService] File not found ({files[i]}).");
                }
            }

            if (sources.Count == 0) {
                return null;
            }

            if (!Directory.Exists(tempPath)) {
                Directory.CreateDirectory(tempPath);
            }

            string file = Path.Combine(tempPath, fileName);
            if (File.Exists(file)) {
                Random rnd = new Random();
                file = Path.Combine(tempPath, fileName + "_" + rnd.Next().ToString("x"));
            }

            try {
                using (ZipArchive zip = ZipFile.Open(file, ZipArchiveMode.Create)) {
                    foreach (FileInfo source in sources) {
                        zip.CreateEntryFromFile(source.FullName, source.Name);
                    }
                }
                attachment.filename = fileName;
                attachment.contentType = AsanaTicketRequest.ContentTypes.Zip;
                byte[] bytes = File.ReadAllBytes(file);
                if (bytes.LongLength > settings.maxFileSize) {
                    Debug.Log($"File is to large {attachment.filename}.");
                } else {
                    attachment.content = Convert.ToBase64String(bytes);
                    return attachment;
                }
            } catch (Exception e) {
                Debug.LogError(e);
            }

            return null;
        }

        public void ClearTemp() {
            if (Directory.Exists(tempPath)) {
                Directory.Delete(tempPath, true);
            }
        }

        private AsanaTicketRequest.Attachment LoadAttachment(string path, string contentType, bool tryReduce) {
            AsanaTicketRequest.Attachment attachment = new AsanaTicketRequest.Attachment();
            attachment.filename = Path.GetFileName(path);
            attachment.contentType = contentType;

            if (File.Exists(path)) {
                using (FileStream logFileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                    long maxFileSize = tryReduce ? settings.maxFileSizeReducible : settings.maxFileSize;

                    byte[] bytes = StreamToByteArray(logFileStream, attachment.filename, maxFileSize, tryReduce);
                    attachment.content = Convert.ToBase64String(bytes);

                    return attachment;
                }
            } else {
                Debug.LogWarning($"[FeedbackService] File not found ({path}).");
            }

            return null;
        }

        private static byte[] StreamToByteArray(Stream input, string filename, long maxSize, bool tryReduce) {
            long fileSize = input.Length;

            if (fileSize > maxSize) {
                if (!tryReduce) {
                    Debug.Log($"File is to large {filename} and was discarded.");
                    return null;
                }

                byte[] bytes = new byte[maxSize];
                input.Seek(-maxSize, SeekOrigin.End);
                input.Read(bytes, 0, (int)maxSize);

                Debug.Log($"File is to large {filename} and was reduced.");
                return bytes;
            } else {
                using (MemoryStream ms = new MemoryStream()) {
                    input.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }
    }
}