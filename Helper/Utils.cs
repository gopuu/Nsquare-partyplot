namespace ERPAdmin.Helper
{
    public static class Utils
    {
        public static void deleteOldFiles(string dirPath, string filePrefixWithType)
        {
            //string[] existingFiles = Directory.GetFiles(excelDir, "GetReport_CustomerBySearch_*.pdf");
            string[] existingFiles = Directory.GetFiles(dirPath, filePrefixWithType);
            foreach (string file in existingFiles)
            {
                try
                {
                    System.IO.File.Delete(file);
                }
                catch (Exception ex)
                {
                    //Console.WriteLine("Error deleting file: " + file + " - " + ex.Message);
                }
            }
        }
    }
}
