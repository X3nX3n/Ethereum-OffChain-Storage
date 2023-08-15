using Microsoft.Extensions.Logging;
using Nethereum.Signer;
using System.Security.Cryptography;

namespace OffChainStorage.Services
{


   public class VerifierService : IVerifier
   {
      private static ILogger logger;

      public VerifierService(ILogger<VerifierService> _logger)
      {
         logger = _logger;
      }
      public bool VerifyEthSignature(string filePath, string signature, string signerAddress)
      {
         if (!System.IO.File.Exists(filePath))
            return false;

         byte[] fileBytes;

         // Read file
         using (var fileStream = new FileStream(filePath, FileMode.Open))
         {
            using (var binaryReader = new BinaryReader(fileStream))
            {
               fileBytes = binaryReader.ReadBytes((int)fileStream.Length);
            }
         }

         if (fileBytes.Length == 0)
            return false; // or other logic for processing an empty file
         try
         {
            //Calculate the hash of a file
            using (SHA256 sha256Hash = SHA256.Create())
            {
               byte[] hashBytes = sha256Hash.ComputeHash(fileBytes);

               // Checking the signature
               var signer = new EthereumMessageSigner();
               string recoveredAddress = signer.EcRecover(hashBytes, signature); // recover address from signature

               // Compare the recovered address with the known signature address
               return recoveredAddress.ToLower() == signerAddress.ToLower();
            }
         }
         catch(Exception ex)
         {

            logger.LogError(ex, ex.Message);

            return false;

         }


      }
   }
}
