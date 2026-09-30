using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;

namespace PrettyText.Utils
{
    /// <summary>
    /// 轻量配置持久化：主题、字体、窗口尺寸、历史记录。
    ///
    /// 配置保存为 %AppData%\PrettyText\config.ini，键名与旧版本保持一致
    /// （Width / Height / X / Y / Splitter / Dark / Wrap / FontSize / FontColor / FontFamily），
    /// 因此老配置文件可以直接沿用；删除该目录即可恢复默认值。
    /// </summary>
    public class AppSettings
    {
        public const int MaxHistoryEntries = 20;

        /// <summary>单条历史记录超过该长度则不写入磁盘（避免配置文件失控）</summary>
        private const int MaxHistoryEntryChars = 256 * 1024;

        public bool? IsLight;
        public string FontFamily = "Consolas";
        public float FontSize = 10.5f;
        public int FontColorArgb = Color.FromArgb(40, 40, 40).ToArgb();
        public bool WordWrap = true;
        public int WindowWidth = 1360;
        public int WindowHeight = 640;
        public int WindowX = int.MinValue;
        public int WindowY = int.MinValue;
        public int SplitterDistance = 618;
        public List<string> History = new List<string>();

        private static string Folder
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PrettyText"); }
        }

        private static string SettingsPath
        {
            get { return Path.Combine(Folder, "config.ini"); }
        }

        private static string HistoryPath
        {
            get { return Path.Combine(Folder, "history.txt"); }
        }

        public static AppSettings Load()
        {
            var settings = new AppSettings();

            try
            {
                if (File.Exists(SettingsPath))
                {
                    foreach (var line in File.ReadAllLines(SettingsPath))
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        int split = line.IndexOf('=');
                        if (split <= 0) continue;

                        string key = line.Substring(0, split).Trim();
                        string value = line.Substring(split + 1).Trim();

                        switch (key.ToLowerInvariant())
                        {
                            case "dark":
                                settings.IsLight = value == "False" || value == "0";
                                break;
                            case "islight":
                                settings.IsLight = value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase);
                                break;
                            case "fontfamily":
                                if (!string.IsNullOrEmpty(value)) settings.FontFamily = value;
                                break;
                            case "fontsize":
                                settings.FontSize = ParseFloat(value, settings.FontSize);
                                break;
                            case "fontcolor":
                                settings.FontColorArgb = ParseColor(value, settings.FontColorArgb);
                                break;
                            case "wrap":
                            case "wordwrap":
                                settings.WordWrap = value == "True" || value == "1";
                                break;
                            case "width":
                            case "windowwidth":
                                settings.WindowWidth = ParseInt(value, settings.WindowWidth);
                                break;
                            case "height":
                            case "windowheight":
                                settings.WindowHeight = ParseInt(value, settings.WindowHeight);
                                break;
                            case "x":
                            case "windowx":
                                settings.WindowX = ParseInt(value, settings.WindowX);
                                break;
                            case "y":
                            case "windowy":
                                settings.WindowY = ParseInt(value, settings.WindowY);
                                break;
                            case "splitter":
                            case "splitterdistance":
                                settings.SplitterDistance = ParseInt(value, settings.SplitterDistance);
                                break;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // 配置损坏时使用默认值
            }

            try
            {
                if (File.Exists(HistoryPath))
                {
                    foreach (var line in File.ReadAllLines(HistoryPath))
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        try
                        {
                            settings.History.Add(Encoding.UTF8.GetString(Convert.FromBase64String(line)));
                        }
                        catch (Exception)
                        {
                            // 忽略单条损坏的历史记录
                        }
                        if (settings.History.Count >= MaxHistoryEntries) break;
                    }
                }
            }
            catch (Exception)
            {
                // 历史记录损坏时忽略
            }

            return settings;
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Folder);

                var sb = new StringBuilder();
                sb.AppendLine("Width=" + WindowWidth.ToString(CultureInfo.InvariantCulture));
                sb.AppendLine("Height=" + WindowHeight.ToString(CultureInfo.InvariantCulture));
                sb.AppendLine("X=" + WindowX.ToString(CultureInfo.InvariantCulture));
                sb.AppendLine("Y=" + WindowY.ToString(CultureInfo.InvariantCulture));
                sb.AppendLine("Splitter=" + SplitterDistance.ToString(CultureInfo.InvariantCulture));
                sb.AppendLine("Dark=" + (IsLight.HasValue && IsLight.Value ? "False" : "True"));
                sb.AppendLine("Wrap=" + (WordWrap ? "True" : "False"));
                sb.AppendLine("FontSize=" + FontSize.ToString(CultureInfo.InvariantCulture));
                sb.AppendLine("FontColor=" + FormatColor());
                sb.AppendLine("FontFamily=" + FontFamily);
                File.WriteAllText(SettingsPath, sb.ToString(), Encoding.UTF8);

                var lines = new List<string>();
                foreach (var item in History)
                {
                    if (string.IsNullOrEmpty(item) || item.Length > MaxHistoryEntryChars) continue;
                    lines.Add(Convert.ToBase64String(Encoding.UTF8.GetBytes(item)));
                }
                File.WriteAllLines(HistoryPath, lines.ToArray(), Encoding.ASCII);
            }
            catch (Exception)
            {
                // 配置写入失败不影响使用
            }
        }

        public void AddHistory(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            int existing = History.FindIndex(h => string.Equals(h, text, StringComparison.Ordinal));
            if (existing >= 0) History.RemoveAt(existing);

            History.Insert(0, text);
            while (History.Count > MaxHistoryEntries) History.RemoveAt(History.Count - 1);
        }

        private string FormatColor()
        {
            var color = Color.FromArgb(FontColorArgb);
            return color.R + "," + color.G + "," + color.B;
        }

        /// <summary>支持 "R,G,B" 与整数 ARGB 两种写法</summary>
        private static int ParseColor(string value, int fallback)
        {
            if (value.IndexOf(',') > 0)
            {
                var parts = value.Split(',');
                if (parts.Length >= 3)
                {
                    int r, g, b;
                    if (int.TryParse(parts[0].Trim(), out r) &&
                        int.TryParse(parts[1].Trim(), out g) &&
                        int.TryParse(parts[2].Trim(), out b))
                    {
                        return Color.FromArgb(Clamp(r), Clamp(g), Clamp(b)).ToArgb();
                    }
                }
                return fallback;
            }
            return ParseInt(value, fallback);
        }

        private static int Clamp(int value)
        {
            if (value < 0) return 0;
            if (value > 255) return 255;
            return value;
        }

        private static int ParseInt(string value, int fallback)
        {
            int result;
            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result) ? result : fallback;
        }

        private static float ParseFloat(string value, float fallback)
        {
            float result;
            return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result) ? result : fallback;
        }
    }
}
