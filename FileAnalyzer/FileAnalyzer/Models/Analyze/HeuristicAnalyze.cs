using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using Trinet.Core.IO.Ntfs;
using System.Security.Principal;
using System.Collections.Generic;
using System.Management.Automation;

namespace FileAnalyzer.Models.Analyze
{
    public static class HeuristicAnalyze
    {
        // Эвристический анализ
        public static string Analyze(List<string> filePaths)
        {
            List<string> result = new List<string>();

            for (int i = 0; i < filePaths.Count; i++)
            {
                result.Add($"{i+1}. {filePaths[i]}");
                result.Add(CheckAttributes(filePaths[i]));
                result.Add(CheckName(filePaths[i]));
                result.Add(CheckMaskExtension(filePaths[i]));
                result.Add(CheckSize(filePaths[i]));
                result.Add(CheckBlock(filePaths[i]));
                result.Add(CheckDigitalSignature(filePaths[i]));
                result.Add(CheckFileInfo(filePaths[i]));
            }
            result.RemoveAll(string.IsNullOrWhiteSpace);

            return string.Join("\n", result.ToArray());
        }

        // Проверка скрытости атрибутов
        private static string CheckAttributes(string filePath)
        {
            FileInfo file = new FileInfo(filePath);

            if (file.Attributes.HasFlag(FileAttributes.Hidden))
                return "\t- Обнаружены скрытые атрибуты";

            return string.Empty;
        }

        // Проверка имени файла
        private static string CheckName(string filePath)
        {
            List<string> virusWords = new List<string>
            {"virus", "worm", "trojan", "bootkit", "botnet", "loh", "hack", "malicious",
            "infect", "attack", "rootkit", "backdoor", "exploit", "bomb", "killer",
            "adware", "spyware", "spam", "keylogger", "locker", "hijacker", "popup",
            "bioskit", "fakealert"};
            string name = Path.GetFileNameWithoutExtension(filePath);

            if (virusWords.Contains(name.ToLower()) || name.Equals(string.Empty))
                return "\t- Обнаружено подозрительное название файла";

            return string.Empty;
        }

        // Проверка наличия расширения у файла
        private static string CheckMaskExtension(string filePath)
        {
            string extension = Path.GetExtension(filePath);

            if (extension.Equals(string.Empty))
                return "\t- Обнаружено некорректное расширение";

            return string.Empty;
        }

        // Проверка размера файла (~20Кб)
        private static string CheckSize(string filePath)
        {
            FileInfo file = new FileInfo(filePath);
            if (Path.GetExtension(filePath).ToLower() == ".exe" && file.Length < 20480)
                return "\t- Слишком маленький размер исполняемого файла";

            return string.Empty;
        }

        // Проверка блокировки файла (загружен из интернета)
        private static string CheckBlock(string filePath)
        {
            FileInfo file = new FileInfo(filePath);

            if (file.AlternateDataStreamExists("Zone.Identifier"))
                return "\t- Файл заблокирован (возможно, загружен из сети)";

            return string.Empty;
        }

        // ПРоверка наличия цифровой подписи и ее валидности
        private static string CheckDigitalSignature(string filePath)
        {
            string fullPath = Path.GetFullPath(filePath);

            if (Path.GetExtension(fullPath).ToLower() == ".exe")
                using (var ps = PowerShell.Create())
                {
                    ps.AddCommand("Get-AuthenticodeSignature", true);
                    ps.AddParameter("LiteralPath", fullPath);
                    var results = ps.Invoke();

                    var signature = (Signature)results.Single().BaseObject;

                    if (signature.Status != SignatureStatus.Valid)
                        return "\t- Обнаружено отсутствие цифровой подписи или ее невалидность";
                }

            return string.Empty;
        }

        // Проверка различных атрибутов файла (даты, владелец, тип и пр.)
        private static string CheckFileInfo(string filePath)
        {
            FileInfo file = new FileInfo(filePath);

            var fs = file.GetAccessControl();
            var sid = fs.GetOwner(typeof(SecurityIdentifier));
            string fileType = file.Extension;

            DateTime creationTime = file.CreationTime;
            DateTime lastWriteTime = file.LastWriteTime;

            FileVersionInfo fileMetadata = FileVersionInfo.GetVersionInfo(filePath);

            if (sid == null || fileType.Equals(string.Empty) || creationTime > lastWriteTime
                 || fileMetadata.FileDescription == null || fileMetadata.InternalName == null)
                return "\t- Обнаружены некорректные характеристики файла";

            return string.Empty;
        }
    }
}