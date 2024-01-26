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
            attachmentPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("Roaming", "LocalLow") + settings.AttachmentLocation;
            tempPath = Path.Combine(attachmentPath, "Temp");
        }

        public List<AsanaTicketRequest.Attachment> LoadAttachments(AsanaProject project, List<Texture2D> images) {
            attachments.Clear();

            LoadImages(images);

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

        private void LoadImages(List<Texture2D> images) {
            for (int i = 0; i < images.Count; i++) {
                AsanaTicketRequest.Attachment attachment = new AsanaTicketRequest.Attachment();
                attachment.filename = "image.jpg";
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
                AsanaTicketRequest.Attachment attachment = LoadAttachment(loc, AsanaTicketRequest.ContentTypes.Text);
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

        private void LoadLog() {
            List<string> logDataPaths = settings.Adapter.GetLog(out bool archive, out string archiveName);

            if (logDataPaths == null) {
                return;
            }

            if (archive) {
                AsanaTicketRequest.Attachment attachment = CreateZipArchive(archiveName, logDataPaths);
                if (attachment != null) {
                    attachments.Add(attachment);
                }
            } else {
                for (int i = 0; i < logDataPaths.Count; i++) {
                    AsanaTicketRequest.Attachment attachment = LoadAttachment(logDataPaths[i], AsanaTicketRequest.ContentTypes.Text);
                    if (attachment != null) {
                        attachments.Add(attachment);
                    }
                }
            }
        }

        private void LoadSavegame() {
            List<string> savegameDataPaths = settings.Adapter.GetSavegame(out bool archive, out string archiveName);

            if (savegameDataPaths == null) {
                return;
            }

            if (archive) {
                AsanaTicketRequest.Attachment attachment = CreateZipArchive(archiveName, savegameDataPaths);
                if (attachment != null) {
                    attachments.Add(attachment);
                }
            } else {
                for (int i = 0; i < savegameDataPaths.Count; i++) {
                    AsanaTicketRequest.Attachment attachment = LoadAttachment(savegameDataPaths[i], AsanaTicketRequest.ContentTypes.Text);
                    if (attachment != null) {
                        attachments.Add(attachment);
                    }
                }
            }
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
            if(Directory.Exists(tempPath)) {
                Directory.Delete(tempPath, true);
            }
        }

        private AsanaTicketRequest.Attachment LoadAttachment(string path, string contentType) {
            AsanaTicketRequest.Attachment attachment = new AsanaTicketRequest.Attachment();
            attachment.filename = Path.GetFileName(path);
            attachment.contentType = contentType;

            if (File.Exists(path)) {
                using (FileStream logFileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                    byte[] bytes = StreamToByteArray(logFileStream);

                    if (bytes.LongLength > settings.maxFileSize) {
                        Span<byte> span = bytes.AsSpan();
                        Span<byte> slice = span.Slice((int)(bytes.LongLength - settings.maxFileSize));
                        bytes = slice.ToArray();
                        Debug.Log($"File is to large {attachment.filename} and was reduced.");
                    }

                    attachment.content = Convert.ToBase64String(bytes);

                    return attachment;
                }
            } else {
                Debug.LogWarning($"[FeedbackService] File not found ({path}).");
            }

            return null;
        }

        private static byte[] StreamToByteArray(Stream input) {
            using (MemoryStream ms = new MemoryStream()) {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
}