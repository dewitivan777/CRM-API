namespace ApiGateway.Auth
{
    public interface IIdCryptoProvider
    {
        string Decrypt(string encryptedId);
        string Encrypt(string id);
    }
}
