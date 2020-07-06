using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiGateway.Extentions.Authorization.Services
{
    public interface IIdCryptoProvider
    {
        string Decrypt(string encryptedId);
        string Encrypt(string id);
    }
}
