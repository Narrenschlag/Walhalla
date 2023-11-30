using System.Text.Encodings;
using System.Text.Json;

using D = System.IO.Directory;
using F = System.IO.File;

namespace Walhalla
{
    public static class IO
    {
        public static string ApplicationPath => AppContext.BaseDirectory;

        public static void MkDir(this string path) => D.CreateDirectory(path);
        public static bool FileExists(this string path) => F.Exists(path);

        #region JSON Utility
        public static string json(this object? source) => JsonSerializer.Serialize(source);
        public static T? json<T>(this string? json) => json == null || json.IsEmpty() ? default(T) : JsonSerializer.Deserialize<T>(json);
        #endregion
    }

    public struct Road
    {
        #region Values
        public string Directory { get; private set; }
        public string File { get; private set; }
        #endregion

        #region Construction
        public Road(string path, bool mkDir = false, bool isLocalPath = true)
        {
            // Apply application path to local path
            if (isLocalPath && !path.StartsWith(IO.ApplicationPath))
                path = IO.ApplicationPath + path;

            // Assign directory
            Directory = path.TrimToDirectory(false);

            // Assign file name
            if (Directory.Length < path.Length - 1)
                File = path.Substring(Directory.Length + 1);
            else
                File = "";

            // Create directory of non existant yet
            if (mkDir)
                this.mkDir();
        }
        #endregion

        #region Utility
        public string path() => $"{Directory}/{File}";
        public override string ToString() => path();

        public void mkDir() => D.CreateDirectory(Directory);
        public bool FileExists() => F.Exists(path());
        #endregion

        #region Write
        public void Write(byte[] buffer, bool async = false)
        {
            if (!FileExists()) mkDir();

            if (async) F.WriteAllBytesAsync(path(), buffer);
            else F.WriteAllBytes(path(), buffer);
        }

        public void Write(object jsonObject, bool async = false) => Write(jsonObject.json(), async);
        public void Write(string text, bool async = false)
        {
            if (!FileExists()) mkDir();

            if (async) F.WriteAllTextAsync(path(), text);
            else F.WriteAllText(path(), text);
        }
        #endregion

        #region Read
        public bool TryRead(out byte[]? buffer)
        {
            if (FileExists())
            {
                buffer = F.ReadAllBytes(path());
                return true;
            }

            buffer = null;
            return false;
        }

        public bool TryRead<T>(out T? jsonObject)
        {
            if (TryRead(out string? json) && json != null && json.NotEmpty())
            {
                jsonObject = json.json<T>();
                return true;
            }

            jsonObject = default;
            return false;
        }

        public bool TryRead(out string? text)
        {
            if (FileExists())
            {
                text = F.ReadAllText(path());
                return true;
            }

            text = null;
            return false;
        }
        #endregion
    }
}