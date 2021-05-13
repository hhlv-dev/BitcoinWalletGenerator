using System;
using static BitcoinWalletGenerator.Format;

namespace BitcoinWalletGenerator
{
  internal class Program
	{
    private static void Main()
    {
      if (!Test.RunAll())
      {
        return;
      }

      var wallet = BitcoinWallet.CreateNew();

      var privateKeyBytes = wallet.GetPrivateKey();
      var privateKeyWif = wallet.GetWif();

      var publicKeyBytes = wallet.GetFullPublicKey();
      var publicKeyCompressed = wallet.GetCompressedPublicKey();

      var walletAddress = wallet. GetAddress();

      Console.WriteLine();
      Console.WriteLine("Private key hex: {0}", ToHexadecimal(privateKeyBytes));
      Console.WriteLine("Private key wif: {0}", privateKeyWif);
      Console.WriteLine("               : {0}", SeparateGroups(privateKeyWif, 3));
      Console.WriteLine();
      Console.WriteLine("Public key hex : {0}", ToHexadecimal(publicKeyBytes));
      Console.WriteLine("Compressed     : {0}", ToHexadecimal(publicKeyCompressed));
      Console.WriteLine();
      Console.WriteLine("Wallet address : {0}", walletAddress);
      Console.WriteLine("               : {0}", SeparateGroups(walletAddress, 4));
      Console.WriteLine();


      // TODO:
      // - allow user to enter or generate private key manually (e.g. dice rolls)
      // - import wif
    }
  }
}
