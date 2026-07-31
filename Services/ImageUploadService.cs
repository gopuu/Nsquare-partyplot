using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Data;
using System.Dynamic;
using ERPAdmin.Helper;
using ERPAdmin.Models;
using ERPAdmin.Services;
using System.Xml.Linq;

namespace ERPAdmin.Services
{
    public class ImageUploadService
    {
        private readonly IWebHostEnvironment _env;
        public ImageUploadService(IWebHostEnvironment env) { 
            _env = env;
        }
        public string UploadImage(string base64image, string folderName, string fileName, string fileType)
        {
            if (string.IsNullOrWhiteSpace(base64image))
                return "";

            folderName = folderName?.TrimStart('/', '\\') ?? "";
            string uploadFolder = Path.Combine(_env.WebRootPath, "uploads", folderName);

            if (!Directory.Exists(uploadFolder))
                Directory.CreateDirectory(uploadFolder);

            string base64Data = base64image.Contains(",")
                ? base64image.Substring(base64image.IndexOf(',') + 1)
                : base64image;

            byte[] fileBytes = Convert.FromBase64String(base64Data);

            fileName = $"{fileName}_{DateTime.Now:yyyyMMdd_HHmmss}{fileType}";

            string filePath = Path.Combine(uploadFolder, fileName);

            System.IO.File.WriteAllBytes(filePath, fileBytes);

            return fileName;
        }

        public string GetImage(string foldername, string filename)
        {
            // Check if inputs are valid
            if (string.IsNullOrEmpty(foldername) || string.IsNullOrEmpty(filename))
                return null ?? ""; // Return null if inputs are invalid

            try
            {
                // Get the full path of the image
                string fullPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", foldername, filename));

                // Ensure the path is inside the allowed directory
                string baseDirectory = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads"));
                if (!fullPath.StartsWith(baseDirectory))
                    throw new UnauthorizedAccessException("Invalid file path access attempt!");

                // Check if the file exists
                //if (!System.IO.File.Exists(fullPath))
                //    return null; // Return null if file does not exist

                // Read the file as a byte array and convert to Base64
                byte[] imageBytes = System.IO.File.ReadAllBytes(fullPath);
                return Convert.ToBase64String(imageBytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error converting image to Base64: {ex.Message}");
                return null ?? ""; // Return null in case of an error
            }
        }

        public string GetImage2(string foldername, string filename)
        {
            // Initialize a variable to store the base64 string
            string base64Image = "";
            if (!string.IsNullOrEmpty(foldername) && !string.IsNullOrEmpty(filename))
            {
                try
                {
                    // Get the full path of the image
                    string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads/" + foldername + "/" + filename);
                    // Check if the file exists
                    if (System.IO.File.Exists(fullPath))
                    {
                        // Read the file as a byte array
                        byte[] imageBytes = System.IO.File.ReadAllBytes(fullPath);
                        // Convert the byte array to a base64 string
                        base64Image = Convert.ToBase64String(imageBytes);
                    }
                    else
                    {
                        //Console.WriteLine("File does not exist at the specified path.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error converting image to base64: {ex.Message}");
                }
            }
            return base64Image;
        }


        public string GetFileTypeFromBase64_OLD_31july26(string base64StringOfFile)
        {
            string fileType = ".jpg"; // Default file type

            if (!string.IsNullOrEmpty(base64StringOfFile))
            {
                // Extract MIME type from Base64
                Match match = Regex.Match(base64StringOfFile, @"data:(?<type>[\w\/\-]+);base64,", RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    string mimeType = match.Groups["type"].Value;

                    if (mimeType.StartsWith("image/"))
                    {
                        fileType = "." + mimeType.Split('/')[1]; // Extract image format (jpg, png, gif, etc.)
                    }
                    else if (mimeType == "application/pdf")
                    {
                        fileType = ".pdf"; // PDF detected from MIME type
                    }
                }
                else
                {
                    // If MIME type is missing, decode and check magic numbers
                    byte[] bytes = Convert.FromBase64String(base64StringOfFile.Contains(",") ? base64StringOfFile.Split(',')[1] : base64StringOfFile);

                    if (bytes.Length > 4)
                    {
                        // Check for PDF file signature: "%PDF-" (25 50 44 46 in hex)
                        if (bytes[0] == 0x25 && bytes[1] == 0x50 && bytes[2] == 0x44 && bytes[3] == 0x46)
                        {
                            fileType = ".pdf";
                        }
                        else
                        {
                            // Detect common image formats
                            fileType = bytes[0] == 0xFF && bytes[1] == 0xD8 ? ".jpg" :
                                       bytes[0] == 0x89 && bytes[1] == 0x50 ? ".png" :
                                       bytes[0] == 0x47 && bytes[1] == 0x49 ? ".gif" :
                                       bytes[0] == 0x42 && bytes[1] == 0x4D ? ".bmp" :
                                       ".jpg"; // Default fallback
                        }
                    }
                }
            }

            return fileType;
        }

        public void DeleteImages(string folderPath, string fileNamePrefix)
        {
            folderPath = folderPath?.TrimStart('/', '\\');
            //string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", folderPath);
            string fullPath = Path.Combine(_env.WebRootPath, "uploads", folderPath);
            if (Directory.Exists(fullPath))
            {
                var filesToDelete = Directory.GetFiles(fullPath, $"{fileNamePrefix}*"); // Find matching files

                foreach (string file in filesToDelete)
                {
                    File.Delete(file); // Delete each file
                }
            }
        }

        public void ValidateBase64File(string base64File, string[] allowedTypes, int maxSizeMB = 1)
        {
            if (string.IsNullOrWhiteSpace(base64File))
                throw new Exception("File data is empty.");

            if (allowedTypes == null || allowedTypes.Length == 0)
                throw new Exception("Allowed file types are not configured.");

            string base64Data = base64File.Contains(",")
                ? base64File.Substring(base64File.IndexOf(',') + 1)
                : base64File;

            byte[] fileBytes;

            try
            {
                fileBytes = Convert.FromBase64String(base64Data);
            }
            catch (FormatException)
            {
                throw new Exception("Invalid file data.");
            }

            if (fileBytes.Length == 0)
                throw new Exception("File is empty.");

            long maxBytes = maxSizeMB * 1024L * 1024L;

            if (fileBytes.Length > maxBytes)
                throw new Exception($"File size must not exceed {maxSizeMB} MB.");

            string actualFileType = GetFileTypeFromBytes(fileBytes);

            string[] normalizedAllowedTypes = allowedTypes
                .Select(x => x.StartsWith(".") ? x.ToLowerInvariant() : "." + x.ToLowerInvariant())
                .ToArray();

            if (!normalizedAllowedTypes.Contains(actualFileType))
                throw new Exception($"Invalid file type. Allowed types: {string.Join(", ", normalizedAllowedTypes)}");
        }

        public string GetFileTypeFromBase64(string base64File)
        {
            if (string.IsNullOrWhiteSpace(base64File))
                throw new Exception("File data is empty.");

            string base64Data = base64File.Contains(",")
                ? base64File.Substring(base64File.IndexOf(',') + 1)
                : base64File;

            try
            {
                byte[] bytes = Convert.FromBase64String(base64Data);
                return GetFileTypeFromBytes(bytes);
            }
            catch (FormatException)
            {
                throw new Exception("Invalid file data.");
            }
        }

        private string GetFileTypeFromBytes(byte[] bytes)
        {
            if (bytes == null || bytes.Length < 4)
                throw new Exception("Invalid or unsupported file.");

            // PDF: %PDF
            if (bytes.Length >= 4 &&
                bytes[0] == 0x25 &&
                bytes[1] == 0x50 &&
                bytes[2] == 0x44 &&
                bytes[3] == 0x46)
                return ".pdf";

            // JPG/JPEG: FF D8 FF
            if (bytes.Length >= 3 &&
                bytes[0] == 0xFF &&
                bytes[1] == 0xD8 &&
                bytes[2] == 0xFF)
                return ".jpg";

            // PNG: 89 50 4E 47 0D 0A 1A 0A
            if (bytes.Length >= 8 &&
                bytes[0] == 0x89 &&
                bytes[1] == 0x50 &&
                bytes[2] == 0x4E &&
                bytes[3] == 0x47 &&
                bytes[4] == 0x0D &&
                bytes[5] == 0x0A &&
                bytes[6] == 0x1A &&
                bytes[7] == 0x0A)
                return ".png";

            // GIF
            if (bytes.Length >= 6 &&
                bytes[0] == 0x47 &&
                bytes[1] == 0x49 &&
                bytes[2] == 0x46 &&
                bytes[3] == 0x38)
                return ".gif";

            // BMP
            if (bytes.Length >= 2 &&
                bytes[0] == 0x42 &&
                bytes[1] == 0x4D)
                return ".bmp";

            // WEBP: RIFF....WEBP
            if (bytes.Length >= 12 &&
                bytes[0] == 0x52 &&
                bytes[1] == 0x49 &&
                bytes[2] == 0x46 &&
                bytes[3] == 0x46 &&
                bytes[8] == 0x57 &&
                bytes[9] == 0x45 &&
                bytes[10] == 0x42 &&
                bytes[11] == 0x50)
                return ".webp";

            throw new Exception("Unsupported or invalid file type.");
        }

    }
}
