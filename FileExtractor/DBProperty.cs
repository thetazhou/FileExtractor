using System;

namespace FileExtractor
{
    [Serializable]
    public struct DBProperty
    {
        public string conn;
        public string ExtractSQL;
        public string ID;
        public string Name;
        public string File;
        public string Group;

        public void Initialize()
        {
            conn = string.Empty;
            ExtractSQL = string.Empty;
            ID = string.Empty;
            Name = string.Empty;
            File = string.Empty;
            Group = string.Empty;
        }
    }
}
