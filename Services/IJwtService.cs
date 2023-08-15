namespace OffChainStorage.Services
{
   public interface IJwtService
   {
      string GenerateToken(string input);
   }
}
