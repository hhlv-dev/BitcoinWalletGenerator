using System;
using System.Numerics;
using System.Security.Cryptography;

namespace BitcoinWalletGenerator
{
  internal class BitcoinWallet
  {
    #region Constants

    private const int KeySize = 32;

    private const string Secp256k1 = "secp256k1";
    private const string Secp256k1OrderHex = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFEBAAEDCE6AF48A03BBFD25E8CD0364141";
    private static readonly BigInteger Secp256k1Order = new BigInteger(Format.FromHexadecimalString(Secp256k1OrderHex), isUnsigned: true, isBigEndian: true);

    private const byte BitcoinFullPublicKeyPrefix = 0x04;
    private const byte BitcoinCompressedPublicKeyOddPrefix = 0x03;
    private const byte BitcoinCompressedPublicKeyEvenPrefix = 0x02;

    private const byte MainnetAddressPrefix = 0x00;
    private const byte TestnetAddressPrefix = 0x6F;

    private const byte MainnetWifPrefix = 0x80;
    private const byte TestnetWifPrefix = 0xEF;

    #endregion

    #region Private fields

    private readonly byte[] _privateKeyBytes;
    private byte[] _publicKeyBytes;
    private byte[] _compressedPublicKeyBytes;

    #endregion

    #region Construct

    private BitcoinWallet(byte[] privateKeyBytes)
    {
      _privateKeyBytes = privateKeyBytes;
    }

    /// <summary>
    /// Creates a new wallet using a newly generated random private key.
    /// </summary>
    public static BitcoinWallet CreateNew()
    {
      return new BitcoinWallet(GenerateRandomPrivateKeyBytes());
    }

    /// <summary>
    /// Imports an existing wallet using the Wallet Import Format.
    /// </summary>
    /// <param name="wif">wallet import format string</param>
    public static BitcoinWallet Import(string wif)
    {
      // TODO:
      // - validate wif
      // - import from wif
      throw new NotImplementedException();
    }

    /// <summary>
    /// Creates a wallet from an existing private key.
    /// </summary>
    /// <param name="privateKeyHex">hexadecimal string representation of private key bytes</param>
    public static BitcoinWallet FromPrivateKey(string privateKeyHex)
    {
      return FromPrivateKey(Format.FromHexadecimalString(privateKeyHex));
    }

    /// <summary>
    /// Creates a wallet from an existing private key.
    /// </summary>
    /// <param name="privateKeyBytes">private key bytes</param>
    public static BitcoinWallet FromPrivateKey(byte[] privateKeyBytes)
    {
      if (privateKeyBytes.Length != KeySize)
      {
        throw new InvalidOperationException($"Private key should be {KeySize} bytes.");
      }
      return new BitcoinWallet(privateKeyBytes);
    }

    #endregion

    #region Public methods

    /// <summary>
    /// Returns the private key of this wallet.
    /// </summary>
    public byte[] GetPrivateKey() => _privateKeyBytes;

    /// <summary>
    /// Returns the uncompressed public key of this wallet.
    /// </summary>
    public byte[] GetFullPublicKey() => _publicKeyBytes ??= CalculatePublicKey(_privateKeyBytes);

    /// <summary>
    /// Returns the compressed public key of this wallet.
    /// </summary>
    public byte[] GetCompressedPublicKey() => _compressedPublicKeyBytes ??= CompressPublicKey(GetFullPublicKey());

    /// <summary>
    /// Returns the Wallet Import Format string of this wallet.
    /// </summary>
    public string GetWif()
    {
      using var sha256 = new SHA256Managed();

      // Step 1: add 0x80 in front of private key bytes
      var bytesToHash = new byte[1+KeySize ];
      bytesToHash[0] = MainnetWifPrefix;
      _privateKeyBytes.CopyTo(bytesToHash, 1);

      // Step 2: hash using sha-256
      var singleHashed = sha256.ComputeHash(bytesToHash);

      // Step 3: hash result again
      var doubleHashed = sha256.ComputeHash(singleHashed);

      // Step 4: append first 4 bytes from result step 3 to result of step 1
      var bytesToEncode = new byte[1 + KeySize + 4];
      Array.Copy(bytesToHash, 0, bytesToEncode, 0, 1 + KeySize);
      Array.Copy(doubleHashed, 0, bytesToEncode, 1 + KeySize, 4);

      // Step 6: encode the result using base58check
      return Format.ToBase58Check(bytesToEncode);
    }

    /// <summary>
    /// Returns the public wallet address.
    /// </summary>
    public string GetAddress()
    {
      using var sha256 = new SHA256Managed();
      var ripemd160 = new Ripemd160();

      // Step 1: hash bytes using sha-256
      var sha256Hash = sha256.ComputeHash(GetCompressedPublicKey()); // can also be full public key

      // Step 2: hash result using ripemd-160
      var ripemd160Hash = ripemd160.CalculateHash(sha256Hash);

      // Step 3: prefix network byte
      var withNetwork = new byte[1 + ripemd160Hash.Length];
      withNetwork[0] = MainnetAddressPrefix;
      ripemd160Hash.CopyTo(withNetwork, 1);

      // Step 4: hash this (main/test) key using sha-256
      sha256Hash = sha256.ComputeHash(withNetwork);

      // Step 5: hash result using sha-256
      sha256Hash = sha256.ComputeHash(sha256Hash);

      // Step 6: append first 4 bytes of result of step 5 to result of step 3
      var addressBytes = new byte[withNetwork.Length + 4];
      Array.Copy(withNetwork, 0, addressBytes, 0, withNetwork.Length);
      Array.Copy(sha256Hash, 0, addressBytes, withNetwork.Length, 4);

      // Step 7: encode result using base58check
      var address = Format.ToBase58Check(addressBytes);

      return address;
    }

    #endregion

    #region Private methods

    private static byte[] GenerateRandomPrivateKeyBytes()
    {
      var bytes = new byte[KeySize];
      using (var rng = new RNGCryptoServiceProvider())
      {
        rng.GetBytes(bytes);
      }

      if (new BigInteger(bytes, isUnsigned: true, isBigEndian: true) > Secp256k1Order)
      {
        // highly unlikely but could happen, just do recursive call to try again
        bytes = GenerateRandomPrivateKeyBytes();
      }

      return bytes;
    }

    private static byte[] CalculatePublicKey(byte[] privateKey)
    {
      var secp256k1Curve = ECCurve.CreateFromFriendlyName(Secp256k1);
      secp256k1Curve.Validate();

      var parameters = new ECParameters
      {
        Curve = secp256k1Curve,
        D = privateKey
      };
      parameters.Validate();

      using (var ecdsa = ECDsa.Create(parameters))
      {
        var publicKey = ecdsa.ExportParameters(false).Q;

        // full public key is 0x04 + Q.X + Q.Y
        var publicKeyBytes = new byte[1 + publicKey.X.Length + publicKey.Y.Length];
        publicKeyBytes[0] = BitcoinFullPublicKeyPrefix;
        publicKey.X.CopyTo(publicKeyBytes, 1);
        publicKey.Y.CopyTo(publicKeyBytes, 1 + publicKey.X.Length);

        return publicKeyBytes;
      }
    }

    private static byte[] CompressPublicKey(byte[] publicKey)
    {
      if (publicKey.Length != (2 * KeySize + 1) || publicKey[0] != BitcoinFullPublicKeyPrefix)
      {
        throw new InvalidOperationException("Expected 0x04 + 64 public key bytes");
      }

      // compressed public key takes only Q.X (so first 32 bytes after 0x04) and uses first byte to indicate if Q.Y is even (0x02) or odd (0x03)

      var yIsEven = (publicKey[^1] & 0b1) == 0;
      var compressed = new byte[(publicKey.Length - 1) / 2 + 1];
      compressed[0] = (byte)(yIsEven ? BitcoinCompressedPublicKeyEvenPrefix : BitcoinCompressedPublicKeyOddPrefix);
      Array.Copy(publicKey, 1, compressed, 1, compressed.Length - 1);
      return compressed;
    }

    #endregion
  }
}
