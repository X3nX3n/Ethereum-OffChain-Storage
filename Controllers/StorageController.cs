using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using System.Security.Claims;
using Nethereum.Signer;
using System.Security.Cryptography;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.Extensions.Hosting;
using OffChainStorage.Services;

namespace OffChainStorage.Controllers
{
   [Route("api/[controller]")]
   [ApiController]
   public class StorageController : ControllerBase
   {
      private static IVerifier verifier;

      public StorageController(IVerifier _verifier)
      {
         verifier = _verifier;
      }

      /// <summary>
      /// Returns a list of all files in the user's storage.
      /// </summary>
      [Authorize]
      [HttpGet("GetAllFilesList")]
      public IActionResult GetAllFilesList()
      {
         string userName = User.FindFirst(ClaimTypes.NameIdentifier).Value;
         // Build the full path to the root "storage" folder
         var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "storage", userName);

         // Check if the directory exists
         if (!Directory.Exists(fullPath))
         {
            return NotFound("Directory not found.");
         }

         // Get the list of all files, including subdirectories
         var files = Directory.GetFiles(fullPath, "*.*", SearchOption.AllDirectories);

         // Convert absolute paths to relative paths (relative to the root "storage" folder)
         var relativePaths = files.Select(file => file.Replace(Directory.GetCurrentDirectory() + "\\storage", "storage")).ToList();

         // Return the list of files in JSON format
         return Ok(relativePaths);
      }

      /// <summary>
      /// Uploads a list of files to the user's storage.
      /// </summary>
      /// <param name="files">List of file paths.</param>
      /// <param name="pathToSave">The path must represent the format: myfolder1/myfolder2/</param>
      [Authorize]
      [HttpPost("UploadFiles")]
      public async Task<IActionResult> UploadFiles(List<IFormFile> files, string pathToSave)
      {
         if (files == null || files.Count == 0)
            return BadRequest("Files are not selected");

         string userName = User.FindFirst(ClaimTypes.NameIdentifier).Value;

         // Build the full path to the folder where files should be saved
         var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "storage", userName, pathToSave);

         // If the folder doesn't exist, create it
         if (!Directory.Exists(fullPath))
         {
            Directory.CreateDirectory(fullPath);
         }

         var filePaths = new List<string>();

         foreach (var file in files)
         {
            // Create the full path to the file
            var filePath = Path.Combine(fullPath, file.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
               await file.CopyToAsync(stream);
            }

            filePaths.Add(filePath);
         }

         return Ok(new { paths = filePaths, success = true, message = "Upload done!" });
      }

      /// <summary>
      /// Downloads a file from the user's storage.
      /// </summary>
      /// <param name="filePath">Full file path with name. Example: my_folder/my_file.txt</param>
      /// <param name="IsSignatureCheck">Checks if the sha256 hash of the file is signed with the Ethereum wallet key. The signature file must be in the .sig format and also have the same path and name as the file for which it is intended.For example, a file at my_folder/my_file.txt would have a signature file at my_folder/my_file.txt.sig</param>
      [Authorize]
      [HttpGet("DownloadFile")]
      public IActionResult DownloadFile(string filePath, bool IsSignatureCheck = false)
      {
         string userName = User.FindFirst(ClaimTypes.NameIdentifier).Value;
         var path = Path.Combine(Directory.GetCurrentDirectory(), userName, filePath);

         if (!System.IO.File.Exists(path))
            return NotFound("File not found.");
         // Read address from file
         string signerAddress;
         try
         {
            signerAddress = User.FindFirst(ClaimTypes.NameIdentifier).Value;
         }
         catch (Exception ex)
         {
            return BadRequest($"Address file could not be read: {ex.Message}");
         }

         if (IsSignatureCheck)
         {
            // Path to .sig file
            var signaturePath = path + ".sig";

            // Read from .sig file
            string signature;
            try
            {
               signature = System.IO.File.ReadAllText(signaturePath);
            }
            catch (Exception ex)
            {
               return BadRequest($"Signature file could not be read: {ex.Message}");
            }

            if (!verifier.VerifyEthSignature(path, signature, signerAddress))
               return Unauthorized("Signature verification failed");
         }

         var stream = new FileStream(path, FileMode.Open);
         var ext = Path.GetExtension(path).ToLowerInvariant();
         var contentType = MimeTypes.GetMimeType(ext); // Make sure this line correctly determines the MIME type
         var fileName = Path.GetFileName(filePath);

         return File(stream, contentType, fileName);
      }

      /// <summary>
      /// Rename a file in the user's storage.
      /// </summary>
      /// <param name="filePath">Full file path with name. Example: my_folder/my_old_file.txt</param>
      /// <param name="newFileName">New name of file. Example: my_new_file.txt</param>
      [Authorize]
      [HttpPost("RenameFile")]
      public IActionResult RenameFile(string filePath, string newFileName)
      {
         string userName = User.FindFirst(ClaimTypes.NameIdentifier).Value;
         var originalPath = Path.Combine(Directory.GetCurrentDirectory(), userName, filePath);

         if (!System.IO.File.Exists(originalPath))
            return NotFound("File not found.");

         var directory = Path.GetDirectoryName(originalPath);
         var newPath = Path.Combine(directory, newFileName);

         try
         {
            System.IO.File.Move(originalPath, newPath);
         }
         catch (Exception ex)
         {
            return BadRequest($"File could not be renamed: {ex.Message}");
         }

         return Ok($"File renamed to {newFileName}.");
      }

      /// <summary>
      /// Move a file from old path to new path. If the directory does not exist, create it.
      /// </summary>
      /// <param name="oldFilePath">Old file path. Example: my_folder/my_old_file.txt</param>
      /// <param name="newFilePath">New file path. Example: my_folder/my_new_file.txt</param>
      [Authorize]
      [HttpPost("MoveFile")]
      public IActionResult MoveFile(string oldFilePath, string newFilePath)
      {
         string userName = User.FindFirst(ClaimTypes.NameIdentifier).Value;
         var originalPath = Path.Combine(Directory.GetCurrentDirectory(), userName, oldFilePath);

         if (!System.IO.File.Exists(originalPath))
            return NotFound("File not found.");

         var newPath = Path.Combine(Directory.GetCurrentDirectory(), userName, newFilePath);

         try
         {
            // Check if the new directory exists, if not, create it.
            var newDirectory = Path.GetDirectoryName(newPath);
            if (!Directory.Exists(newDirectory))
            {
               Directory.CreateDirectory(newDirectory);
            }

            System.IO.File.Move(originalPath, newPath);
         }
         catch (Exception ex)
         {
            return BadRequest($"File could not be moved: {ex.Message}");
         }

         return Ok($"File moved to {newFilePath}.");
      }

      /// <summary>
      /// Copy a file from old path to new path. If the directory does not exist, create it.
      /// </summary>
      /// <param name="oldFilePath">Old file path. Example: my_folder/my_old_file.txt</param>
      /// <param name="newFilePath">New file path. Example: my_folder/my_new_file.txt</param>
      [Authorize]
      [HttpPost("CopyFile")]
      public IActionResult CopyFile(string oldFilePath, string newFilePath)
      {
         string userName = User.FindFirst(ClaimTypes.NameIdentifier).Value;
         var originalPath = Path.Combine(Directory.GetCurrentDirectory(), userName, oldFilePath);

         if (!System.IO.File.Exists(originalPath))
            return NotFound("File not found.");

         var newPath = Path.Combine(Directory.GetCurrentDirectory(), userName, newFilePath);

         try
         {
            // Check if the new directory exists, if not, create it.
            var newDirectory = Path.GetDirectoryName(newPath);
            if (!Directory.Exists(newDirectory))
            {
               Directory.CreateDirectory(newDirectory);
            }

            System.IO.File.Copy(originalPath, newPath);
         }
         catch (Exception ex)
         {
            return BadRequest($"File could not be copied: {ex.Message}");
         }

         return Ok($"File copied to {newFilePath}.");
      }


      /// <summary>
      /// Removes a list of files from the user's storage.
      /// </summary>
      /// <param name="filePaths">List of file paths.</param>
      [Authorize]
      [HttpDelete("DeleteFiles")]
      public IActionResult DeleteFiles([FromBody] string[] filePaths)
      {
         if (filePaths == null || !filePaths.Any())
            return BadRequest("No file paths provided.");

         string userName = User.FindFirst(ClaimTypes.NameIdentifier).Value;

         foreach (var filePath in filePaths)
         {
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "storage", userName, filePath);

            if (System.IO.File.Exists(fullPath))
            {
               System.IO.File.Delete(fullPath);
            }
            else
            {
               // Optionally, you can return a NotFound or Bad Request response for non-existing files.
               return NotFound($"File not found: {filePath}");
            }
         }

         return Ok(new { success = true, message = "Files successfully deleted." });
      }

   }


}
