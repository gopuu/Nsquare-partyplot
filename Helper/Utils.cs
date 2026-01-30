namespace ERPAdmin.Helper
{
    public static class Utils
    {
        public static void deleteOldFiles(string dirPath, string filePattern, int olderThanMinutes=60)
        {
            if (!Directory.Exists(dirPath))
                return;

            var files = Directory.GetFiles(dirPath, filePattern);
            //var cutoffTime = DateTime.Now.AddHours(-1); // 🔹 1 hour ago
            var cutoffTime = DateTime.Now.AddMinutes(-olderThanMinutes);

            foreach (string file in files)
            {
                try
                {
                    FileInfo fileInfo = new FileInfo(file);

                    // Use LastWriteTime (or CreationTime if you prefer)
                    if (fileInfo.LastWriteTime < cutoffTime)
                    {
                        fileInfo.Delete();
                    }
                }
                catch
                {
                    // optional: log exception
                }
            }
        }

    }
}
