using System;
using System.Linq;
using System.Reflection;
using static BitcoinWalletGenerator.Format;

namespace BitcoinWalletGenerator
{
  /// <summary>
  /// Simple test class that executes some code and validates result.
  /// </summary>
  internal static class Test
  {
    public static bool RunAll()
    {
      var overallResult = true;
      var counter = 0;
      var sw = System.Diagnostics.Stopwatch.StartNew();
      foreach (var test in typeof(Test)
        .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
        .Where(m => m.ReturnParameter.ParameterType == typeof(bool) &&
                    m.GetParameters().Length == 0 &&
                    m.Name.Contains("Test")))
      {
        var testResult = Run(() => (bool)test.Invoke(null, null));
        if (!testResult)
        {
          Console.WriteLine("Test failed: {0}", test.Name);
        }
        overallResult &= testResult;
        counter++;
      }

      if (overallResult)
      {
        Console.WriteLine($"All tests succeeded in {sw.ElapsedMilliseconds:N0} ms. Nr of test cases: {counter}");
      }

      return overallResult;
    }

    private static bool Run(Func<bool> test)
    {
      try
      {
        return test();
      }
      catch (Exception ex)
      {
        Console.WriteLine("ERROR: {0}", ex);
        return false;
      }
    }

    private static bool PublicFromPrivateKeyTest()
    {
      var testKeyPairs = new[]
      {
        (
          "FF2E46B3392BF3C2B094585D5C8AAE2302A33A8A0F7BEF9648940172820763CA",
          "04D151039F66301349ACFC2D7C763F7C5B4754D6FEAE7B4E3A888A7B69B68D64C74BB838D0587DB3C55D52B640FEEC0FCC363ED24572C7098344CE9F21B6EE66B0"
        ),
        (
          "0C28FCA386C7A227600B2FE50B7CAE11EC86D3BF1FBE471BE89827E19D72AA1D",
          "04D0DE0AAEAEFAD02B8BDC8A01A1B8B11C696BD3D66A2C5F10780D95B7DF42645CD85228A6FB29940E858E7E55842AE2BD115D1ED7CC0E82D934E929C97648CB0A"
        ),
        (
          "3ED639B6EFD938FED79A903BE7813FE95115870BA9805A881A3399B49DC47EB8",
          "04C03CF1A96CC1433827203339049157CC9735335AD9B1FCAA5CC6D021D23C848C86EB0A9F14F08EE2D3DC56921A11B4808F169F19752396AFDDE3BDD6AB3A7A12"
        ),
        (
          "AC0F9BEA97837D17D01719879528033FC9BDB80B10C960F20AA7D1C597021F13",
          "04372F92CE9855DA5D4E7E370D82E1CD91FC49FA86B370C06F6C38F77B94E89667A3BB11DBE0BD71931933B66DF88E71696522782D8AECBC70B46382AD98BF0495"
        ),
        (
          "90980F14DF67D0F8E143C32B2CEB79F6CDE8EF7092075A0DE0DFC159A846C38A",
          "04888499578B426A3ACB44CADB2998A2EC7B3D5095E49404D4177122DD227A8F7C2884CF6397CDAE2D733119683B353C591B06BD8E680B7FD2B9EB37EAD272D429"
        ),
        (
          "024DBB58D88BD7BD5197612C7936C856BA09D0503C8C1D85119D5CA1611032D6",
          "0427523DD0438B20EB833BA2233139A8A1944DC06B54E065C92E414E8A8F0C4A0DAA1EC57B209016A4A3A8C601ABFEAD830C1A96F306DFE57C8D8A0E4AE38E3E3E"
        ),
        (
          //https://www.freecodecamp.org/news/how-to-create-a-bitcoin-wallet-address-from-a-private-key-eca3ddd9c05f/
          "60cf347dbc59d31c1358c8e5cf5e45b822ab85b79cb32a9f3d98184779a9efc2",
          "041e7bcc70c72770dbb72fea022e8a6d07f814d2ebe4de9ae3f7af75bf706902a7b73ff919898c836396a6b0c96812c3213b99372050853bd1678da0ead14487d7"
        ),
        (
          //https://bitcoin.stackexchange.com/questions/25024/how-do-you-get-a-bitcoin-public-key-from-a-private-key
          "18E14A7B6A307F426A94F8114701E7C8E774E7F9A47E2C2035DB29A206321725",
          "0450863AD64A87AE8A2FE83C1AF1A8403CB53F53E486D8511DAD8A04887E5B23522CD470243453A299FA9E77237716103ABC11A1DF38855ED6F2EE187E9C582BA6"
        ),
        (
          //https://cryptocoinsinfoclub.com/bitcoin/bitcoin-private-key-example
          "2255cb6746a89fa0ce302a48147402437f7f069ffe507efb9deafa26bbd5b640",
          "045e120534846e3c89d914c4720c9b86a156b7ec1384c0a60d5e285fcc631b81d1854c2ed552c900e698b351116b060166fd35da70ed67ba9380ba981a4a94504b"
        ),
      };

      var result = true;

      foreach (var keyPair in testKeyPairs)
      {
        var expectedPublicKey = keyPair.Item2;
        var calculatedPublicKey = ToHexadecimal(BitcoinWallet.FromPrivateKey(FromHexadecimalString(keyPair.Item1)).GetFullPublicKey());

        var success = calculatedPublicKey.Equals(expectedPublicKey, StringComparison.OrdinalIgnoreCase);
        if (!success)
        {
          Console.WriteLine($"ERROR: expected public key {expectedPublicKey} but got {calculatedPublicKey}");
          result = false;
        }
      }

      return result;
    }

    private static bool Ripemd160Test()
    {
      var result = true;

      // 64 bytes (no padding)
      result &= VerifyHash("0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef",
        FromHexadecimalString("6de80618aee1dfe6ab7c7df308aa2d4e9cbe3e46"));

      // 128 bytes (2 blocks, no padding)
      result &= VerifyHash("0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef",
        FromHexadecimalString("c1214e1c9ed55fdc0875eae9f1ef53204c2d1e4d"));

      // 16 bytes (padding required)
      result &= VerifyHash("0123456789abcdef",
        FromHexadecimalString("edf4e38018cd71dd489b9c1e54b32054eb42dfad"));

      // 96 bytes (1.5 block, padding required)
      result &= VerifyHash("0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef",
        FromHexadecimalString("61b25d6e71820acf678ceb1d5cb63fe6ea22746d"));

      // wikipedia example (dog vs cog)
      result &= VerifyHash("The quick brown fox jumps over the lazy dog",
        FromHexadecimalString("37f332f68db77bd9d7edd4969571ad671cf9dd3b"));

      // wikipedia example (dog vs cog, resulting in very different hash)
      result &= VerifyHash("The quick brown fox jumps over the lazy cog",
        FromHexadecimalString("132072df690933835eb8b6ad0b77e7b6f14acad7"));

      // 0 bytes
      result &= VerifyHash("",
        FromHexadecimalString("9c1185a5c5e9fc54612808977ee8f548b2258d31"));

      // 1 byte (padding required)
      result &= VerifyHash("a",
        FromHexadecimalString("0bdc9d2d256b3ee9daae347be6f4dc835a467ffe"));

      return result;
    }

    private static bool VerifyHash(string input, byte[] expectedHash)
    {
      var ripemd160 = new Ripemd160();
      var inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
      var actualHash = ripemd160.CalculateHash(inputBytes);
      if (actualHash.SequenceEqual(expectedHash))
      {
        return true;
      }
      Console.WriteLine($"ERROR: Ripemd160({input}) expected {ToHexadecimal(expectedHash)} but was {ToHexadecimal(actualHash)}");
      return false;
    }

    private static bool PrivateKeyToAddressTest()
    {
      var testData = new[]
      {
        (
          //https://www.freecodecamp.org/news/how-to-create-a-bitcoin-wallet-address-from-a-private-key-eca3ddd9c05f/
          "60cf347dbc59d31c1358c8e5cf5e45b822ab85b79cb32a9f3d98184779a9efc2",
          "17JsmEygbbEUEpvt4PFtYaTeSqfb9ki1F1"
        ),
        (
          //https://learnmeabitcoin.com/beginners/keys_addresses
          "ef235aacf90d9f4aadd8c92e4b2562e1d9eb97f0df9ba3b508258739cb013db2",
          "1EUXSxuUVy2PC5enGXR1a3yxbEjNWMHuem"
        ),
        (
          //https://www.bitaddress.org
          "2D07EA1903CD9D74290E6F299EDC2DC25682FBE242B7B3F5427FBB37A421FDF5",
          "158fWKVcFUk7sh63UmMd7UuHPcNWenB6Dp"
        ),
        (
          "4E5C1B8B3702015DF67F27DA7FFFA85B4F44D547783485B23DD75B201743015E",
          "1HnMUv3REXqcdS3WcmuQBPgmwgzbm9pCy5"
        ),
        (
          "4DA046164ECFBAB6A9987567E609A7DC9DACC0ECC8A8D21F35C78E7B5745B27C",
          "1GSZuPDKVBCAwcyJTM9R2YF25xn195m8m6"
        ),
        (
          "632E29AD797086B10597C443492AA8A6AC3D840A5E962DD7D50492C7229BC56B",
          "13r5hg9N2ydt9rzfYVzhmTHDqtoRoo1jXb"
        ),
        (
          "51FCBF33AAD8E9BFEE4186EBC61345AAA801A1C5EFAD1EBD93550EDB45DB9A91",
          "1BUdWiuHeUhkhKKsK3QuKmbo3aViY8bY17"
        ),
        (
          //https://generate.plus/en/address/coin/bitcoin
          "a94ac682a541d304f7e383aea0c33a34d49a12f6651373cb5e306a1f1c133ed2",
          "129uuWHGXTRDawis9aZAKn4Rrp32fXHGyU"
        ),
        (
          "da2c4b0a910c60f6ff2f9a70514168a2fd64a274f82f7c01f45f2b0fdec83801",
          "1CdNBQNQgKTaA2ffCeRFzDCgCBRa6imeDK"
        ),
        (
          "1792dfa6cc1b563ba9f960f86ab20cacba552a646608e55b3cfe23e5f4d6e88a",
          "1PfczAUgRzkj9UJUpLczu9hCmLXBTh1Bnq"
        ),
      };

      var result = true;

      foreach (var testValues in testData)
      {
        var expectedAddress = testValues.Item2;
        var calculatedAddress = BitcoinWallet.FromPrivateKey(testValues.Item1).GetAddress();

        var success = calculatedAddress == expectedAddress;
        if (!success)
        {
          Console.WriteLine($"ERROR: expected address {expectedAddress} but got {calculatedAddress}");
          result = false;
        }
      }

      return result;
    }

    private static bool PrivateKeyToWalletImportFormatTest()
    {
      var testData = new[]
      {
        (
          //https://www.bitaddress.org
          "2D07EA1903CD9D74290E6F299EDC2DC25682FBE242B7B3F5427FBB37A421FDF5",
          "5JA7qRwGq2NV5j4ARcoCjCWViHMRvMpPANkafPSr2cvSTh5wYeD"
        ),
        (
          "4E5C1B8B3702015DF67F27DA7FFFA85B4F44D547783485B23DD75B201743015E",
          "5JQoBEiw5eopwLkBdvuZUTKBWgJVwvfiZLTVUoJ5jEgqCNhgGXt"
        ),
        (
          "4DA046164ECFBAB6A9987567E609A7DC9DACC0ECC8A8D21F35C78E7B5745B27C",
          "5JQUSCVfjLq19u1qFyaWVPcQNdXZEeRjL3QDPnfcDCtHdu5HfGG"
        ),
        (
          "632E29AD797086B10597C443492AA8A6AC3D840A5E962DD7D50492C7229BC56B",
          "5JZy1X5W6mL11BZbDBDBgPVaw8LR7R4ee92TKhS9sgxBw4SMr3f"
        ),
        (
          "51FCBF33AAD8E9BFEE4186EBC61345AAA801A1C5EFAD1EBD93550EDB45DB9A91",
          "5JSPqVSepDh5kDTn6ZKts9KrpnEjYS3SQMuA4AXAZFVM5RrWqMF"
        )
      };

      var result = true;

      foreach (var testValues in testData)
      {
        var expectedWIF = testValues.Item2;
        var calculatedWIF = BitcoinWallet.FromPrivateKey(testValues.Item1).GetWif();

        var success = calculatedWIF == expectedWIF;
        if (!success)
        {
          Console.WriteLine($"ERROR: expected WIF {expectedWIF} but got {calculatedWIF}");
          result = false;
        }
      }

      return result;
    }

    private static bool NewWalletKeySizeTest()
    {
      var result = true;

      for (int i = 0; i <= 0xff; i++)
      {
        var wallet = BitcoinWallet.CreateNew();

        result &= wallet.GetPrivateKey().Length == 32;
        result &= wallet.GetFullPublicKey().Length == 1 + 64;
        result &= wallet.GetFullPublicKey()[0] == 0x04;
        result &= wallet.GetCompressedPublicKey().Length == 1 + 32;
        result &= wallet.GetCompressedPublicKey()[0] == 0x02 || wallet.GetCompressedPublicKey()[0] == 0x03;
        result &= wallet.GetWif().Length == 51;
        result &= wallet.GetWif()[0] == '5';
        result &= wallet.GetAddress().Length >= 25 || wallet.GetAddress().Length <= 34;
        result &= wallet.GetAddress()[0] == '1';

        result &= wallet.GetPrivateKey().Distinct().Count() > 20; // not really guaranteed but less than 21 is unlikely enough to doubt the 'randomness' of the key
      }
 
      return result;
    }

    private static bool WifImportTest()
    {
      var result = true;

      for (int i = 0; i < 0xff; i++)
      {
        var wallet1 = BitcoinWallet.CreateNew();
        var wif1 = wallet1.GetWif();
        var wallet2 = BitcoinWallet.Import(wif1);
        var wif2 = wallet2.GetWif();

        result &= wallet1.GetPrivateKey().SequenceEqual(wallet2.GetPrivateKey());
        result &= wif1 == wif2;
      }

      return result;
    }

    private static bool FormatHexTest()
    {
      if (!ToHexadecimal(new byte[] { 0x00, 0x01, 0x32, 0x50, 0xfe, 0xff }).Equals("00013250feff", StringComparison.OrdinalIgnoreCase))
      {
        return false;
      }
      if (ToHexadecimal(Array.Empty<byte>()).Length != 0)
      {
        return false;
      }
      if (!FromHexadecimalString("aabbcc").SequenceEqual(new byte[] { 0xaa, 0xbb, 0xcc }))
      {
        return false;
      }
      if (FromHexadecimalString(string.Empty).Length != 0)
      {
        return false;
      }
      return true;
    }

    private static bool FormatBase58CheckTest()
    {
      var hex_b58c = new[]
      {
        (FromHexadecimalString("00f54a5851e9372b87810a8e60cdd2e7cfd80b6e31c7f18fe8"),
          "1PMycacnJaSqwwJqjawXBErnLsZ7RkXUAs"),
        (System.Text.Encoding.ASCII.GetBytes("test"),
          "3yZe7d"),
        (FromHexadecimalString("0000000102030405"),
          "1117bWpTW"),
        (
          FromHexadecimalString(
            "ffeeddccbbaa99887766554433221100" +
            "0102030405060708090a0b0c0d0e0f" +
            "102030405060708090a0b0c0d0e0f0" +
            "f1f2f3f4f5f6f7f8f9fafbfcfdfeff"),
          "4TBfC1zy5CpBtEAQiaA8DZLPBQCtWwMdQufpVTN9rNsbtbQEM9M7nkho12rUwQJ5At365cU8Wfom53KmL9Bc"
        )
      };

      foreach(var item in hex_b58c)
      {
        if (ToBase58Check(item.Item1) != item.Item2)
        {
          return false;
        }

        if (!FromBase58Check(item.Item2).SequenceEqual(item.Item1))
        {
          return false;
        }
      }

      return true;
    }

    private static bool FormatGroupingTest()
    {
      var result = true;
      result &= SeparateGroups("abc", 1) == "a b c";
      result &= SeparateGroups("ABC", 2) == "AB C";
      result &= SeparateGroups("Abc", 3) == "Abc";
      result &= SeparateGroups("00112233445566778899AABBCCDDEEFF", 4) == "0011 2233 4455 6677 8899 AABB CCDD EEFF";
      result &= SeparateGroups("abcdefghijklmnopqrstuvwxyz", 99) == "abcdefghijklmnopqrstuvwxyz";
      return result;
    }
  }
}
