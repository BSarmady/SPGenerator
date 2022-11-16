using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

public class Reg {

    #region public static string Read(string)
    public static string Read(string KeyName, string valueName, string defaultValue) {
        try {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(KeyName)) {
                string value = key.GetValue(valueName, defaultValue).ToString();
                key.Close();
                return value;
            }

        } catch {
            return defaultValue;
        }
    }
    #endregion

    #region public static bool Write(string)
    public static bool Write(string KeyName, string valueName, string Value) {
        try {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(KeyName)) {
                key.SetValue(valueName, Value, RegistryValueKind.String);
                key.Close();
            }
            return true;
        } catch {
            return false;
        }
    }
    #endregion

    #region public static int Read(int)
    public static int Read(string KeyName, string valueName, int defaultValue) {
        try {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(KeyName)) {
                int value = int.Parse(key.GetValue(valueName, defaultValue).ToString());
                key.Close();
                return value;
            }

        } catch {
            return defaultValue;
        }
    }
    #endregion

    #region public static bool Write(int)
    public static bool Write(string KeyName, string valueName, int Value) {
        try {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(KeyName)) {
                key.SetValue(valueName, Value, RegistryValueKind.DWord);
                key.Close();
            }
            return true;
        } catch {
            return false;
        }
    }
    #endregion

    #region public static object Read(object)
    public static object Read(string KeyName, string valueName, object defaultValue) {
        try {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(KeyName)) {
                object value = key.GetValue(valueName, defaultValue);
                key.Close();
                return value;
            }

        } catch {
            return defaultValue;
        }
    }
    #endregion

    #region public static bool Write(object)
    public static bool Write(string KeyName, string valueName, object Value) {
        try {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(KeyName)) {
                key.SetValue(valueName, Value, RegistryValueKind.Binary);
                key.Close();
            }
            return true;
        } catch {
            return false;
        }
    }
    #endregion

    #region public static string[] Read(string[])
    public static string[] Read(string KeyName, string valueName, string[] defaultValue) {
        try {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(KeyName)) {
                string[] value = (string[]) key.GetValue(valueName, defaultValue);
                key.Close();
                return value;
            }

        } catch {
            return defaultValue;
        }
    }
    #endregion

    #region public static bool Write(string[])
    public static bool Write(string KeyName, string valueName, string[] Value) {
        try {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(KeyName)) {
                key.SetValue(valueName, Value, RegistryValueKind.MultiString);
                key.Close();
            }
            return true;
        } catch {
            return false;
        }
    }
    #endregion

    #region public static string[] SubKeys(...)
    public static string[] SubKeys(string KeyName) {
        try {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(KeyName)) {
                return key.GetSubKeyNames();
            }
        } catch {
            return new string[] { };
        }
    }
    #endregion

    #region public static Dictionary<string, string> ReadAllValues(...)
    public static Dictionary<string, string> ReadAllValues(string KeyName) {
        Dictionary<string, string> Result = new Dictionary<string, string> { };
        try {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(KeyName)) {
                string[] Values = key.GetValueNames();
                foreach (string value in Values) {
                    Result.Add(value, Read(KeyName, value, ""));
                }
            }
        } catch {
        }
        return Result;
    }
    #endregion

    #region public static void WriteAllValues(...)
    public static void WriteAllValues(string KeyName, Dictionary<string, string> Items) {
        try {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(KeyName, true)) {
                string[] Values = key.GetValueNames();

                foreach (KeyValuePair<string, string> item in Items) {
                    key.SetValue(item.Key, item.Value);
                }
            }
        } catch {
        }
    }
    #endregion

    #region public static void LoadWindowPos(...)
    public static void LoadWindowPos(string KeyName, Form form) {
        string[] WindowPos = Reg.Read(KeyName, "WindowPos", "").Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        if (WindowPos.Length == 5) {
            bool Maximized = WindowPos[4] == "1";
            form.Left = Convert.ToInt32(WindowPos[0]);
            form.Top = Convert.ToInt32(WindowPos[1]);
            form.Width = Convert.ToInt32(WindowPos[2]);
            form.Height = Convert.ToInt32(WindowPos[3]);
            form.WindowState = Maximized ? FormWindowState.Maximized : FormWindowState.Normal;
        }
    }
    #endregion

    #region public static void SaveWindowPos(...)
    public static void SaveWindowPos(string KeyName, Form form) {
        bool Maximized = form.WindowState == FormWindowState.Maximized;
        int x = Maximized ? form.RestoreBounds.X : form.Location.X;
        int y = Maximized ? form.RestoreBounds.Y : form.Location.Y;
        int w = Maximized ? form.RestoreBounds.Width : form.Width;
        int h = Maximized ? form.RestoreBounds.Height : form.Height;

        Reg.Write(KeyName, "WindowPos", x + ";" + y + ";" + w + ";" + h + ";" + (Maximized ? "1" : "0"));
    }
    #endregion

}
