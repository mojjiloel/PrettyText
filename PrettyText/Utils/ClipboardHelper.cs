using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace PrettyText.Utils
{
    /// <summary>
    /// 剪贴板辅助类。
    /// 剪贴板可能被其他程序占用，直接调用 Clipboard.SetText 会抛 ExternalException，
    /// 这里加入重试，避免"复制失败"或大文本复制时直接崩溃。
    /// </summary>
    public static class ClipboardHelper
    {
        public static bool TrySetText(string text, int retries = 8)
        {
            if (string.IsNullOrEmpty(text)) return false;

            for (int i = 0; i < retries; i++)
            {
                try
                {
                    Clipboard.SetText(text);
                    return true;
                }
                catch (ExternalException)
                {
                    Thread.Sleep(80);
                }
                catch (ArgumentException)
                {
                    return false;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return false;
        }

        public static string TryGetText(int retries = 8)
        {
            for (int i = 0; i < retries; i++)
            {
                try
                {
                    if (!Clipboard.ContainsText()) return null;
                    return Clipboard.GetText();
                }
                catch (ExternalException)
                {
                    Thread.Sleep(80);
                }
                catch (Exception)
                {
                    return null;
                }
            }
            return null;
        }
    }
}
