using System;
using System.IO;
using UnityEngine;

public static class StandaloneFileBrowser
{
    public static string[] OpenFilePanel(string title, string directory, string[] extensions, bool multiselect)
    {
        try
        {
            string filter = "";
            if (extensions != null && extensions.Length > 0)
            {
                for (int i = 0; i < extensions.Length; i += 2)
                {
                    if (i + 1 < extensions.Length)
                    {
                        filter += extensions[i] + " (*." + extensions[i + 1] + ")|*." + extensions[i + 1];
                        if (i + 2 < extensions.Length) filter += "|";
                    }
                }
            }
            else
            {
                filter = "All Files (*.*)|*.*";
            }

#if UNITY_EDITOR
            string path = UnityEditor.EditorUtility.OpenFilePanel(title, directory, extensions != null && extensions.Length > 1 ? extensions[1] : "");
            return string.IsNullOrEmpty(path) ? new string[0] : new string[] { path };
#else
            // For standalone builds, use Windows file dialog
            return OpenFileDialogWindows(title, directory, filter, multiselect);
#endif
        }
        catch (Exception e)
        {
            Debug.LogError("File browser error: " + e.Message);
            return new string[0];
        }
    }

#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
    [System.Runtime.InteropServices.DllImport("comdlg32.dll", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
    private static extern bool GetOpenFileName(ref OpenFileName ofn);

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
    private struct OpenFileName
    {
        public int lStructSize;
        public IntPtr hwndOwner;
        public IntPtr hInstance;
        public string lpstrFilter;
        public string lpstrCustomFilter;
        public int nMaxCustFilter;
        public int nFilterIndex;
        public string lpstrFile;
        public int nMaxFile;
        public string lpstrFileTitle;
        public int nMaxFileTitle;
        public string lpstrInitialDir;
        public string lpstrTitle;
        public int Flags;
        public short nFileOffset;
        public short nFileExtension;
        public string lpstrDefExt;
        public IntPtr lCustData;
        public IntPtr lpfnHook;
        public string lpTemplateName;
    }

    private static string[] OpenFileDialogWindows(string title, string directory, string filter, bool multiselect)
    {
        OpenFileName ofn = new OpenFileName();
        ofn.lStructSize = System.Runtime.InteropServices.Marshal.SizeOf(ofn);
        ofn.lpstrFilter = filter.Replace("|", "\0") + "\0";
        ofn.lpstrFile = new string(new char[256]);
        ofn.nMaxFile = ofn.lpstrFile.Length;
        ofn.lpstrFileTitle = new string(new char[64]);
        ofn.nMaxFileTitle = ofn.lpstrFileTitle.Length;
        ofn.lpstrInitialDir = directory;
        ofn.lpstrTitle = title;
        ofn.Flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000008;

        if (GetOpenFileName(ref ofn))
        {
            return new string[] { ofn.lpstrFile };
        }
        return new string[0];
    }
#else
    private static string[] OpenFileDialogWindows(string title, string directory, string filter, bool multiselect)
    {
        Debug.LogWarning("File dialog not supported on this platform");
        return new string[0];
    }
#endif
}