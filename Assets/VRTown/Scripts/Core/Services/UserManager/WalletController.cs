using Cysharp.Threading.Tasks;
using System;
using System.Text;
using Web3Unity.Scripts.Library.Web3Wallet;
using Nethereum.Util;
using Nethereum.Signer;
using System.Runtime.InteropServices;

namespace VRTown.Service
{
    public interface IWalletController
    {
        string GetWalletAddress();

        UniTask ConnectWallet();

        UniTask<string> SignMessage(string message);
    }

    public class WalletController : IWalletController
    {
        private readonly ILogger _logger;

        private string _accountSeedPhrase;
        private string _privateKey = "";
        private string _account;
        private string _url = "";
        private int _chainId;
        private int _expirationTime;

        // private Dictionary<string, Function> _abiFuncs = new Dictionary<string, Function>();

        public WalletController(
            ILogger logger)
        {
            _logger = logger;
        }

        public string GetWalletAddress()
        {
            return _account;
        }

#if UNITY_WEBGL && !UNITY_EDITOR

        //&& !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void Web3Connect();

        [DllImport("__Internal")]
        private static extern string ConnectAccount();

        [DllImport("__Internal")]
        private static extern void SetConnectAccount(string value);

        public async UniTask ConnectWallet()
        {
            Web3Connect();

            _account = ConnectAccount();
            while (string.IsNullOrEmpty(_account))
            {
                await UniTask.Delay(TimeSpan.FromSeconds(1f));
                _account = ConnectAccount();
            };

            // reset login message
            SetConnectAccount("");

            _logger.Log($"ConnectWallet - {_account}");
        }

        public async UniTask<string> SignMessage(string message)
        {
            try
            {
                // sign message
                string signature = await Web3GL.Sign(message);
                return signature;
            }
            catch (Exception ex)
            {
                _logger.Log($"SignMessage {ex.Message}");
                return "";
            }
        }

#else

        public async UniTask ConnectWallet()
        {
            int timestamp = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            // set expiration time
            int expirationTime = timestamp + 60;
            // set message
            string message = expirationTime.ToString();
            // sign message
            string signature = await Web3Wallet.Sign(message);
            // verify account
            string account = SignVerifySignature(signature, message);
            int now = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            // validate
            if (!string.IsNullOrEmpty(account) && expirationTime >= now)
            {
                // save account
                _account = account;
            }
            else
            {
                // TODO: Open ConfirmPopup to Reload
                await ConnectWallet();
            }
        }

        public async UniTask<string> SignMessage(string message)
        {
            // sign message
            string signature = await Web3Wallet.Sign(message);

            // verify account
            string account = SignVerifySignature(signature, message);

            if (account.Length != 42) return "";

            _account = account;
            return signature;
        }

        public string SignVerifySignature(string signatureString, string originalMessage)
        {
            var msg = "\x19" + "Ethereum Signed Message:\n" + originalMessage.Length + originalMessage;
            var msgHash = new Sha3Keccack().CalculateHash(Encoding.UTF8.GetBytes(msg));
            var signature = MessageSigner.ExtractEcdsaSignature(signatureString);
            var key = EthECKey.RecoverFromSignature(signature, msgHash);
            return key.GetPublicAddress();
        }
#endif
    }
}