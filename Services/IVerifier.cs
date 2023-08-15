namespace OffChainStorage.Services
{
   public interface IVerifier
   {
      /// Ethereum uses the ECDSA (Elliptic Curve Digital Signature Algorithm) algorithm for signing and verifying signatures.
      public bool VerifyEthSignature(string filePath, string signature, string signerAddress);
   }
}
