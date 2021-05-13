using System;
using System.Linq;
using System.Numerics;
using System.Text;

namespace BitcoinWalletGenerator
{
  internal static class Format
  {
    #region Constants

    private const string Base64Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
    private const string Base58Characters = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz123456789";
    private const string Base58CheckCharacters = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

    #endregion

    #region Public methods

    public static string ToHexadecimal(byte[] bytes) =>
      string.Join(string.Empty, bytes.Select(b => Convert.ToString(b, 16).ToUpper().PadLeft(2, '0')));

    public static byte[] FromHexadecimalString(string hex)
    {
      var bytes = new byte[hex.Length / 2];
      for (int i = 0; i < bytes.Length; i++)
      {
        bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
      }
      return bytes;
    }

    public static string ToBase58Check(byte[] bytes)
    {
      var sb = new StringBuilder();
      var value = new BigInteger(bytes, isUnsigned: true, isBigEndian: true);

      while (value > 0)
      {
        var digit = (int)(value % Base58CheckCharacters.Length);
        sb.Insert(0, Base58CheckCharacters[digit]);
        value /= Base58CheckCharacters.Length;
      }

      // prefix leading zeroes using 0-th base character
      sb.Insert(0, bytes.TakeWhile(b => b == 0).Select(_ => Base58CheckCharacters[0]).ToArray());

      return sb.ToString();
    }

    public static byte[] FromBase58Check(string baseString)
    {
      var value = new BigInteger();
      foreach (var c in baseString)
      {
        value *= Base58CheckCharacters.Length;
        value += Base58CheckCharacters.IndexOf(c);
      }
      var bytes = value.ToByteArray(isUnsigned: true, isBigEndian: true);
      var withPadding = new byte[bytes.Length + baseString.TakeWhile(c => c == Base58CheckCharacters[0]).Count()];
      Array.Copy(bytes, 0, withPadding, withPadding.Length - bytes.Length, bytes.Length);
      return withPadding;
    }

    /// <summary>
    /// Turns "0123456789abcdef" into "0123 4567 89ab cdef".
    /// </summary>
    public static string SeparateGroups(string text, int groupSize)
    {
      var i = 0;
      return text.Aggregate("",
        (accumulate, current) => accumulate += (i++ % groupSize == 0) ? " " + current : current,
        result => result.Trim());
    }

    #endregion

    #region Private methods

    //private static string ToBase64(byte[] bytes, bool logDetails = false)
    //{
    //  const char PaddingChar = '=';

    //  var sb = new StringBuilder();
    //  for (int index = 0; index < bytes.Length; index += 3)
    //  {
    //    /*
    //       |-----------------------------------|
    //       | byte[i+0] | byte[i+1] | byte[i+2] | bytes
    //       |-----------------------------------|
    //       | 0000 0000 | 0000 0000 | 0000 0000 | octets
    //       |-----------------------------------|
    //       | 1111 11   |           |           | sextets
    //       |        22 | 2222      |           |
    //       |           |      3333 | 33        |
    //       |           |           |   44 4444 |
    //       |-----------------------------------|
    //    */

    //    var remainingBytes = bytes.Length - index;

    //    var byte1 = bytes[index];
    //    var byte2 = remainingBytes < 2 ? 0 : bytes[index + 1];
    //    var byte3 = remainingBytes < 3 ? 0 : bytes[index + 2];

    //    var bits_01_06 = 0b111111 & ((byte1 >> 2));
    //    var bits_07_12 = 0b111111 & ((byte1 << 4) | (byte2 >> 4));
    //    var bits_13_18 = 0b111111 & ((byte2 << 2) | (byte3 >> 6));
    //    var bits_19_24 = 0b111111 & ((byte3 >> 0));

    //    sb.Append(Base64Characters[bits_01_06]);
    //    sb.Append(Base64Characters[bits_07_12]);
    //    sb.Append(remainingBytes > 1 ? Base64Characters[bits_13_18] : PaddingChar);
    //    sb.Append(remainingBytes > 2 ? Base64Characters[bits_19_24] : PaddingChar);
    //  }

    //  return sb.ToString();
    //}

    #endregion
  }
}
