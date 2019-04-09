namespace HES.Web.Helpers.Interfaces
{
    public interface IAesCryptography
    {
        byte[] EncryptObject(object toEncrypt, byte[] password);
        T DecryptObject<T>(byte[] toDecrypt, byte[] password);
    }
}