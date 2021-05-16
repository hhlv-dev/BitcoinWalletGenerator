using System;
using static BitcoinWalletGenerator.Format;

namespace BitcoinWalletGenerator
{
  internal class Program
	{
    private static void Main()
    {
      PrintTitle();
      if (!Test.RunAll())
      {
        return;
      }

      var exit = false;
      while (!exit)
      {
        exit = ShowMenu();
        PrintTitle();
      }

      // TODO:
      // - allow user to enter or generate private key manually (e.g. dice rolls)
    }

    private static void PrintTitle()
    {
      Console.Clear();
      Console.WriteLine("-- Bitcoin Wallet Generator --");
    }

    private static bool ShowMenu()
    {
      Console.WriteLine();
      Console.WriteLine("Menu options:");
      Console.WriteLine(" 1. Generate new wallet");
      Console.WriteLine(" 2. Show details of existing wallet");
      Console.WriteLine(" 3. Exit");
      Console.WriteLine();
      Console.Write("Select option: ");
      var input = Console.ReadLine();
      switch (input)
      {
        case "1":
          CreateNewWallet();
          break;
        case "2":
          ImportExistingWallet();
          break;
        case "3":
          return true;
      }
      return false;
    }

    private static void CreateNewWallet()
    {
      PrintTitle();
      var wallet = BitcoinWallet.CreateNew();
      ShowDetails(wallet);
      Console.ReadLine();
    }

    private static void ImportExistingWallet()
    {
      PrintTitle();

      Console.WriteLine();
      Console.Write("Enter WIF: ");
      var wif = Console.ReadLine();

      try
      {
        var wallet = BitcoinWallet.Import(wif);
        ShowDetails(wallet);
      }
      catch
      {
        Console.WriteLine("Failed to read wallet");
      }

      Console.ReadLine();
    }

    private static void ShowDetails(BitcoinWallet wallet)
    {
      var privateKeyBytes = wallet.GetPrivateKey();
      var privateKeyWif = wallet.GetWif();

      var publicKeyBytes = wallet.GetFullPublicKey();
      var publicKeyCompressed = wallet.GetCompressedPublicKey();

      var walletAddress = wallet.GetAddress();

      Console.WriteLine();
      Console.WriteLine("Wallet details");
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
    }
  }
}
