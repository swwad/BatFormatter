# BatFormatter
用 C# 編寫的 Windows 批次指令碼格式化工具，可自動格式化 Windows Batch Script

## 功能特點

- 自動縮進代碼塊
- 保持原始文件編碼
- 處理註釋、標籤和特殊命令
- 支持多種控制結構（如 IF、FOR、SETLOCAL 等）
- 自動生成格式化後的輸出文件

## 使用方法

1. 編譯程序：
   ```
   csc BatFormatter.cs
   ```

2. 運行程序：
   ```
   BatFormatter.exe <輸入檔案路徑>
   ```

輸出文件將自動生成,命名為 "原檔名_Formatter.bat"。

## 編碼支持

程序會自動檢測輸入文件的編碼,支持以下編碼：

- UTF-8 (帶 BOM)
- UTF-16 LE
- UTF-16 BE
- UTF-32 LE
- ANSI (Windows-1252, 默認)

## 系統需求

- .NET Framework 4.5 或更高版本

## 示例

輸入文件 `example.bat`:

```batch
@echo off
if "%1"=="" goto usage
echo Hello, %1!
goto end
:usage
echo Usage: example.bat <name>
:end
```

運行命令：

```
BatFormatter.exe example.bat
```

輸出文件 `example_Formatter.bat`:

```batch
@echo off
if "%1"=="" goto usage
    echo Hello, %1!
    goto end

:usage
    echo Usage: example.bat <name>

:end
```