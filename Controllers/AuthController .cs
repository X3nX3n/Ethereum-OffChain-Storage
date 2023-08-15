using Microsoft.AspNetCore.Mvc;
using Nethereum.Signer;
using System.Text;
using Nethereum.Util;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;
using OffChainStorage.Services;

namespace OffChainStorage.Controllers
{
   [ApiController]
   [Route("api/[controller]")]
   public class AuthController : ControllerBase
   {
      // Message that needs to be signed by the user.
      private readonly string _stringToSign;

      private readonly IJwtService _jwtService;

      private readonly IConfiguration _config;

      public AuthController(IJwtService jwtService, IConfiguration config) 
      {
         _jwtService = jwtService;
         _config = config;
         _stringToSign = _config.GetSection("AuthSettings").GetValue<string>("SignPrase");
      }

      /// <summary>
      ///   Returns the phrase to create the signature.
      /// </summary>
      [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(StringToSignResponse))]
      [HttpGet("GetStringToSign")]
      public async Task<IResult> GetStringToSign()
      {
         var response = new StringToSignResponse
         {
            StringToSign = _stringToSign,
         };
         return Results.Ok(response);
      }
      public class StringToSignResponse
      {
         public string StringToSign { get; set; }
      }


      /// <summary>
      /// Restores the wallet address from the signature and compares it to the address parameter.
      /// Returns JWT token.
      /// </summary>
      /// <param name="address">Ethereum wallet with which the phrase is signed.</param>
      /// <param name="signature">Signature obtained from the specific string.</param>
      [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthResponse))]
      [ProducesResponseType(StatusCodes.Status400BadRequest)]
      [HttpPost("EthAuthenticate")]
      public async Task<IResult> EthAuthenticate(string address, string signature)
      {
         // Prefix added to the message according to the Ethereum signature standard.
         string prefix = "\u0019Ethereum Signed Message:\n" + _stringToSign.Length;

         // Combine the prefix and the message.
         string prefixedMessage = prefix + _stringToSign;

         // Hash the prefixed message using SHA3 Keccak algorithm.
         byte[] messageHash = new Sha3Keccack().CalculateHash(Encoding.UTF8.GetBytes(prefixedMessage));

         // Recover sender's address from the message hash and signature.
         string signerAddressRecovered = new MessageSigner().EcRecover(messageHash, signature);

         // Compare the recovered address with the provided address.
         if (address.ToLower() == signerAddressRecovered.ToLower())
         {
            // Create a JWT token
            var token = _jwtService.GenerateToken(address);
            // If addresses match, verification is successful and return a success response.
            // Form the response
            var response = new AuthResponse
            {
               Success = true,
               Message = "Authenticated successfully.",
               AccessToken = token
            };

            return Results.Ok(response);
         }
         else
         {
            // If addresses do not match, verification failed and return an error.
            return Results.BadRequest(new { success = false, message = "Authentication failed." });
         }
      }
      public class AuthResponse
      {
         public bool Success { get; set; }
         public string Message { get; set; }
         public string AccessToken { get; set; }
      }

      /// <summary>
      /// Check authorization.
      /// </summary>
      [Authorize]
      [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CheckAuthResponse))]
      [HttpGet("CheckAuthorization")]
      public async Task<IResult> CheckAuthorization()
      {
         var response =  new CheckAuthResponse
         {
            Success = true,
         };
         return Results.Ok(response);
      }
      public class CheckAuthResponse
      {
         public bool Success { get; set; }
      }
   }
}
