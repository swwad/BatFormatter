using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

/// <summary>
/// Windows 批次指令碼格式化工具
/// </summary>
class BatFormatter
{
    /// <summary>
    /// 程式進入點
    /// </summary>
    /// <param name="args">命令列參數</param>
    static void Main(string[] args)
    {
        // 檢查是否提供了輸入檔案
        if (args.Length == 0)
        {
            Console.WriteLine("用法: BatchScriBatFormatterptFormatter.exe <輸入檔案>");
            Console.WriteLine("輸出檔案將自動命名為 '原檔名_Formatter.bat'");
            return;
        }

        string inputFile = args[0];
        string outputFile = GenerateOutputFileName(inputFile);

        try
        {
            // 檢測輸入檔案的編碼
            Encoding detectedEncoding = DetectFileEncoding(inputFile);
            Console.WriteLine($"檢測到的檔案編碼: {detectedEncoding.EncodingName}");

            // 讀取輸入檔案
            string script = File.ReadAllText(inputFile, detectedEncoding);
            // 格式化指令碼
            string formattedScript = FormatBatchScript(script);

            // 將格式化後的指令碼寫入輸出檔案，使用相同的編碼
            File.WriteAllText(outputFile, formattedScript, detectedEncoding);
            Console.WriteLine($"格式化後的指令碼已儲存到 {outputFile}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"錯誤: {ex.Message}");
        }
    }
    /// <summary>
    /// 檢測檔案編碼
    /// </summary>
    /// <param name="filename">檔案名稱</param>
    /// <returns>檢測到的編碼</returns>
    static Encoding DetectFileEncoding(string filename)
    {
        // 讀取文件的前幾個字節來檢測編碼
        byte[] buffer = new byte[5];
        using (FileStream file = new FileStream(filename, FileMode.Open, FileAccess.Read))
        {
            file.Read(buffer, 0, 5);
        }

        // 檢測 UTF-8 BOM
        if (buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF)
            return Encoding.UTF8;

        // 檢測 UTF-16 LE BOM
        if (buffer[0] == 0xFF && buffer[1] == 0xFE)
            return Encoding.Unicode;

        // 檢測 UTF-16 BE BOM
        if (buffer[0] == 0xFE && buffer[1] == 0xFF)
            return Encoding.BigEndianUnicode;

        // 檢測 UTF-32 LE BOM
        if (buffer[0] == 0xFF && buffer[1] == 0xFE && buffer[2] == 0x00 && buffer[3] == 0x00)
            return Encoding.UTF32;

        // 如果沒有 BOM，假設為 ANSI (Windows-1252)
        // 注意：這可能不適用於所有情況，可能需要更複雜的編碼檢測邏輯
        return Encoding.GetEncoding(1252);
    }

    /// <summary>
    /// 生成輸出檔案名稱
    /// </summary>
    /// <param name="inputFileName">輸入檔案名稱</param>
    /// <returns>輸出檔案名稱</returns>
    static string GenerateOutputFileName(string inputFileName)
    {
        string directory = Path.GetDirectoryName(inputFileName);
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(inputFileName);
        string extension = Path.GetExtension(inputFileName);

        return Path.Combine(directory, $"{fileNameWithoutExtension}_Formatter{extension}");
    }

    /// <summary>
    /// 格式化批次指令碼
    /// </summary>
    /// <param name="script">原始指令碼文字</param>
    /// <returns>格式化後的指令碼文字</returns>
    static string FormatBatchScript(string script)
    {
        string[] lines = script.Split('\n');
        StringBuilder formattedScript = new StringBuilder();
        Stack<int> indentStack = new Stack<int>();
        int currentIndent = 0;
        bool previousLineWasEmpty = false;

        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();

            // 處理空行
            if (string.IsNullOrWhiteSpace(trimmedLine))
            {
                if (!previousLineWasEmpty)
                {
                    formattedScript.AppendLine();
                    previousLineWasEmpty = true;
                }
                continue;
            }
            previousLineWasEmpty = false;

            // 處理註解
            if (trimmedLine.StartsWith("REM", StringComparison.OrdinalIgnoreCase) || trimmedLine.StartsWith("::"))
            {
                formattedScript.AppendLine(new string(' ', currentIndent * 4) + trimmedLine);
                continue;
            }

            // 處理標籤
            if (trimmedLine.StartsWith(":"))
            {
                formattedScript.AppendLine();
                formattedScript.AppendLine(trimmedLine);
                continue;
            }

            // 處理控制結構（如 IF、FOR、SETLOCAL）
            if (Regex.IsMatch(trimmedLine, @"^(IF|FOR|SETLOCAL)\s", RegexOptions.IgnoreCase))
            {
                formattedScript.AppendLine(new string(' ', currentIndent * 4) + trimmedLine);
                indentStack.Push(currentIndent);
                currentIndent++;
                continue;
            }

            // 處理結束括號和某些特定命令（如 ELSE、GOTO、ENDLOCAL）
            if (trimmedLine.Equals(")") || Regex.IsMatch(trimmedLine, @"^(ELSE|GOTO|ENDLOCAL)\s", RegexOptions.IgnoreCase))
            {
                if (indentStack.Count > 0)
                    currentIndent = indentStack.Pop();
                formattedScript.AppendLine(new string(' ', currentIndent * 4) + trimmedLine);
                continue;
            }

            // 處理 CALL 命令
            if (Regex.IsMatch(trimmedLine, @"^CALL\s", RegexOptions.IgnoreCase))
            {
                formattedScript.AppendLine(new string(' ', currentIndent * 4) + trimmedLine);
                continue;
            }

            // 處理一般命令
            formattedScript.AppendLine(new string(' ', currentIndent * 4) + trimmedLine);
        }

        // 移除最後的空行並返回格式化後的指令碼
        return formattedScript.ToString().TrimEnd();
    }
}