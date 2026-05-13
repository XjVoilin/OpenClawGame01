using JulyCore.Core;
using JulyCore.Provider.Base;
using JulyCore.Provider.Encryption;

namespace OffTrail.Aot
{
    public class NoEncryptionProvider : ProviderBase, IEncryptionProvider
    {
        public override int Priority => Frameworkconst.PriorityEncryptionProvider;
        protected override LogChannel LogChannel => LogChannel.Encryption;

        public byte[] Encrypt(byte[] data)
        {
            return data;
        }

        public byte[] Decrypt(byte[] encryptedData)
        {
            return encryptedData;
        }
    }
}
