using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace FileAnalyzer.Models.Analyze
{
    public static class SignatureAnalyze
    {
        // Сигнатурный анализ
        public static string Analyze(List<string> filePaths)
        {
            List<string> result = new List<string>();

            for (int i = 0; i < filePaths.Count; i++)
            {
                result.Add($"{i + 1}. {filePaths[i]}");

                byte[] fileData = GetFileData(filePaths[i]);
                result.Add(CommonCheckSum(fileData));
                result.Add(FindSubBytes(fileData));
            }
            result.RemoveAll(string.IsNullOrWhiteSpace);

            return string.Join("\n", result.ToArray());
        }

        // Получение сигнатуры файла
        private static byte[] GetFileData(string filePath)
        {
            byte[] fileData = Array.Empty<byte>();

            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                long fileSize = fileStream.Length;
                fileData = new byte[fileSize];
                int bytesRead = fileStream.Read(fileData, 0, (int)fileSize);
            }

            return fileData;
        }

        // Поиск полмассива в массиве байт
        private static int FindLast(byte[] haystack, byte[] needle)
        {
            for (var i = haystack.Length - 1; i >= needle.Length - 1; i--)
            {
                var found = true;
                for (var j = needle.Length - 1; j >= 0 && found; j--)
                    found = haystack[i - (needle.Length - 1 - j)] == needle[j];
                if (found)
                    return i - (needle.Length - 1);
            }

            return -1;
        }

        // Подсчет контрольной суммы файла (SHA256)
        private static string CommonCheckSum(byte[] fileData)
        {
            SHA256 sha256 = SHA256.Create();
            var rawHashSha256 = sha256.ComputeHash(fileData);
            string hashSha256 = BitConverter.ToString(rawHashSha256).Replace("-", "");

            if (hashSha256.Equals(SignatureData.hashSha256))
                return "\t- Обнаружена вирусная контрольная сумма";

            return string.Empty;
        }

        // Поиск вредоносной сигнатуры в файлах
        private static string FindSubBytes(byte[] fileData)
        {
            int countPositions = 0;
            int countBlocks = 16;

            List<byte[]> rawData = SignatureData.rawData
                .GroupBy(_ => countBlocks++ / 16)
                .Select(v => v.ToArray())
                .ToList();

            for (int i = 0; i < rawData.Count; i++)
            {
                int position = FindLast(fileData, rawData[i]);

                if (position != -1)
                    countPositions += 1;
            }

            if (countPositions >= rawData.Count / 2)
                return $"\t- Обнаружены части вирусного кода ({countPositions} из {rawData.Count})";

            return string.Empty;
        }
    }
}
