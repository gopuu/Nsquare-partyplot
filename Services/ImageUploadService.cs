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
            //Restrict Extensions
            var allowedTypes = new[] { ".jpg", ".jpeg", ".png", ".webp", ".pdf" };
            if (!allowedTypes.Contains(fileType.ToLower()))
            {
                throw new Exception("Invalid file type.");
            }
            folderName = folderName?.TrimStart('/', '\\');
            // Define the folder path where the image will be saved
            //string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", folderName);
            string uploadFolder = Path.Combine(_env.WebRootPath, "uploads", folderName);
            // Ensure the directory exists
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            if (!string.IsNullOrEmpty(base64image))
            {
                try
                {
                    // Decode the base64 string
                    byte[] imageBytes = Convert.FromBase64String(base64image);

                    // Generate a unique file name
                    //string fileName = Guid.NewGuid().ToString() + ".jpg"; // You can change the extension as needed (e.g., .png)
                    //fileName = fileName + fileType; // You can change the extension as needed (e.g., .png)
                    fileName = fileName + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + fileType;

                    // Define the full file path
                    string filePath = Path.Combine(uploadFolder, fileName);

                    // Save the image to the path
                    System.IO.File.WriteAllBytes(filePath, imageBytes);

                    // Optionally, return the file path or URL to the saved image
                    //string imageUrl = $"uploads/{folderName}/{fileName}";
                    //string imageUrl = $"{fileName}";
                    return fileName;
                }
                catch (Exception ex)
                {
                    return ex.Message;
                    //Console.WriteLine($"An error occurred while saving the image: {ex.Message}");
                }
            }
            return "";
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


        public string GetFileTypeFromBase64(string base64StringOfFile)
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

        public string GetFileTypeFromBase64_OLD(string base64StringOfFile)
        {
            string imgType = ".jpg"; //default
            if (!string.IsNullOrEmpty(base64StringOfFile))
            {
                // Extract MIME type if Base64 includes it
                Match match = Regex.Match(base64StringOfFile, @"data:image/(?<type>[a-zA-Z]+);base64,", RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    imgType = "." + match.Groups["type"].Value; // Extract file extension
                }
                else
                {
                    // If MIME type is not present, decode and check magic numbers (optional)
                    byte[] bytes = Convert.FromBase64String(base64StringOfFile.Contains(",") ? base64StringOfFile.Split(',')[1] : base64StringOfFile);

                    // Check file signature (magic bytes)
                    if (bytes.Length > 4)
                    {
                        imgType = bytes[0] == 0xFF && bytes[1] == 0xD8 ? ".jpg" :
                                  bytes[0] == 0x89 && bytes[1] == 0x50 ? ".png" :
                                  bytes[0] == 0x47 && bytes[1] == 0x49 ? ".gif" :
                                  bytes[0] == 0x42 && bytes[1] == 0x4D ? ".bmp" :
                                  ".jpg"; // Default fallback
                    }
                }
            }
            return imgType;
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

    }
}
