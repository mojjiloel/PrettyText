using Microsoft.Win32;
using System.Drawing;
using AntdUI;

namespace PrettyText.Utils
{
    public static class ThemeHelper
    {
        /// <summary>
        /// 判断系统是否使用浅色主题
        /// </summary>
        public static bool IsLightMode()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    if (key != null)
                    {
                        object value = key.GetValue("AppsUseLightTheme");
                        if (value is int) return (int)value == 1;
                    }
                }
            }
            catch (System.Exception)
            {
                // 读取注册表失败时按浅色处理
            }
            return true;
        }

        /// <summary>
        /// 设置明暗模式。
        /// 旧实现只在深色时设置 IsDark、从不复位，切回浅色后状态会不一致。
        /// </summary>
        public static void SetColorMode(AntdUI.Window window, bool isLight)
        {
            AntdUI.Config.Mode = isLight ? TMode.Light : TMode.Dark;
            if (window != null) window.BackColor = Background(isLight);
        }

        /// <summary>窗口 / 编辑器背景色</summary>
        public static Color Background(bool isLight)
        {
            return isLight ? Color.White : Color.FromArgb(31, 31, 31);
        }

        /// <summary>状态栏背景色</summary>
        public static Color StatusBarBackground(bool isLight)
        {
            return isLight ? Color.FromArgb(250, 250, 250) : Color.FromArgb(38, 38, 38);
        }

        /// <summary>默认前景色</summary>
        public static Color Foreground(bool isLight)
        {
            return isLight ? Color.FromArgb(40, 40, 40) : Color.FromArgb(220, 220, 220);
        }

        /// <summary>占位符颜色</summary>
        public static Color Placeholder(bool isLight)
        {
            return isLight ? Color.FromArgb(160, 160, 160) : Color.FromArgb(120, 120, 120);
        }
    }
}
