// Code based on rmd160.c and rmd160.h from KU Leuven website
// https://homes.esat.kuleuven.be/~bosselae/ripemd160.html
// https://homes.esat.kuleuven.be/~bosselae/ripemd160/ps/AB-9601/rmd160.c
// https://homes.esat.kuleuven.be/~bosselae/ripemd160/ps/AB-9601/rmd160.h
// https://homes.esat.kuleuven.be/~bosselae/ripemd160/ps/AB-9601/hashtest.c

namespace BitcoinWalletGenerator
{
  using System;

  /// <summary>
  /// 160-bit RIPE Message Digest
  /// </summary>
  internal class Ripemd160
  {
    #region Constants

    private const int BytesPerDword = 4;
    private const int DwordsPerBlock = 16;
    private const int BytesPerBlock = BytesPerDword * DwordsPerBlock;

    #endregion

    #region Main hash method

    /// <summary>
    /// Calculates the RIPEMD-160 hash of the given byte array.
    /// </summary>
    public byte[] CalculateHash(byte[] inputBytes)
    {
      // convert to dwords
      var inputDwords = new uint[inputBytes.Length / BytesPerDword];
      var dwordBytes = new byte[BytesPerDword];
      for (int i = 0; i < inputDwords.Length; i++)
      {
        Array.Copy(inputBytes, i * BytesPerDword, dwordBytes, 0, BytesPerDword);
        inputDwords[i] = BYTES_TO_DWORD(dwordBytes);
      }

      // init
      var buffer = new uint[5]; // 160 bits
      InitializeBuffer(buffer);

      // compress
      var block = new uint[DwordsPerBlock];
      for (int i = 0; i < inputBytes.Length / BytesPerBlock; i++)
      {
        Array.Copy(inputDwords, i * DwordsPerBlock, block, 0, DwordsPerBlock);
        CompressBlock(buffer, block);
      }

      // finish
      var lswlen = (uint)(0xffffffff & inputBytes.LongLength);
      var mswlen = (uint)(0xffffffff & (inputBytes.LongLength >> 32));
      Finish(buffer, inputBytes, lswlen, mswlen);

      // convert back to bytes
      var output = DWORDS_TO_BYTES(buffer);
      return output;
    }

    #endregion

    #region rmd160.c (original)

    // /********************************************************************\
    //  *
    //  *      FILE:     rmd160.c
    //  *
    //  *      CONTENTS: A sample C-implementation of the RIPEMD-160
    //  *                hash-function.
    //  *      TARGET:   any computer with an ANSI C compiler
    //  *
    //  *      AUTHOR:   Antoon Bosselaers, ESAT-COSIC
    //  *      DATE:     1 March 1996
    //  *      VERSION:  1.0
    //  *
    //  *      Copyright (c) 1996 Katholieke Universiteit Leuven
    //  *
    //  *      Permission is hereby granted, free of charge, to any person 
    //  *      obtaining a copy of this software and associated documentation 
    //  *      files (the "Software"), to deal in the Software without restriction, 
    //  *      including without limitation the rights to use, copy, modify, merge, 
    //  *      publish, distribute, sublicense, and/or sell copies of the Software, 
    //  *      and to permit persons to whom the Software is furnished to do so, 
    //  *      subject to the following conditions:
    //  *
    //  *      The above copyright notice and this permission notice shall be 
    //  *      included in all copies or substantial portions of the Software.
    //  *
    //  *      THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
    //  *      EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
    //  *      MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
    //  *      IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY 
    //  *      CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, 
    //  *      TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
    //  *      SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
    //  *
    // \********************************************************************/
    // 
    // /*  header files */
    // #include <stdio.h>
    // #include <stdlib.h>
    // #include <string.h>
    // #include "rmd160.h"      
    // 
    // /********************************************************************/
    // 
    // void MDinit(dword *MDbuf)
    // {
    //    MDbuf[0] = 0x67452301UL;
    //    MDbuf[1] = 0xefcdab89UL;
    //    MDbuf[2] = 0x98badcfeUL;
    //    MDbuf[3] = 0x10325476UL;
    //    MDbuf[4] = 0xc3d2e1f0UL;
    // 
    //    return;
    // }
    // 
    // /********************************************************************/
    // 
    // void compress(dword *MDbuf, dword *X)
    // {
    //    dword aa = MDbuf[0],  bb = MDbuf[1],  cc = MDbuf[2],
    //          dd = MDbuf[3],  ee = MDbuf[4];
    //    dword aaa = MDbuf[0], bbb = MDbuf[1], ccc = MDbuf[2],
    //          ddd = MDbuf[3], eee = MDbuf[4];
    // 
    //    /* round 1 */
    //    FF(aa, bb, cc, dd, ee, X[ 0], 11);
    //    FF(ee, aa, bb, cc, dd, X[ 1], 14);
    //    FF(dd, ee, aa, bb, cc, X[ 2], 15);
    //    FF(cc, dd, ee, aa, bb, X[ 3], 12);
    //    FF(bb, cc, dd, ee, aa, X[ 4],  5);
    //    FF(aa, bb, cc, dd, ee, X[ 5],  8);
    //    FF(ee, aa, bb, cc, dd, X[ 6],  7);
    //    FF(dd, ee, aa, bb, cc, X[ 7],  9);
    //    FF(cc, dd, ee, aa, bb, X[ 8], 11);
    //    FF(bb, cc, dd, ee, aa, X[ 9], 13);
    //    FF(aa, bb, cc, dd, ee, X[10], 14);
    //    FF(ee, aa, bb, cc, dd, X[11], 15);
    //    FF(dd, ee, aa, bb, cc, X[12],  6);
    //    FF(cc, dd, ee, aa, bb, X[13],  7);
    //    FF(bb, cc, dd, ee, aa, X[14],  9);
    //    FF(aa, bb, cc, dd, ee, X[15],  8);
    //                              
    //    /* round 2 */
    //    GG(ee, aa, bb, cc, dd, X[ 7],  7);
    //    GG(dd, ee, aa, bb, cc, X[ 4],  6);
    //    GG(cc, dd, ee, aa, bb, X[13],  8);
    //    GG(bb, cc, dd, ee, aa, X[ 1], 13);
    //    GG(aa, bb, cc, dd, ee, X[10], 11);
    //    GG(ee, aa, bb, cc, dd, X[ 6],  9);
    //    GG(dd, ee, aa, bb, cc, X[15],  7);
    //    GG(cc, dd, ee, aa, bb, X[ 3], 15);
    //    GG(bb, cc, dd, ee, aa, X[12],  7);
    //    GG(aa, bb, cc, dd, ee, X[ 0], 12);
    //    GG(ee, aa, bb, cc, dd, X[ 9], 15);
    //    GG(dd, ee, aa, bb, cc, X[ 5],  9);
    //    GG(cc, dd, ee, aa, bb, X[ 2], 11);
    //    GG(bb, cc, dd, ee, aa, X[14],  7);
    //    GG(aa, bb, cc, dd, ee, X[11], 13);
    //    GG(ee, aa, bb, cc, dd, X[ 8], 12);
    // 
    //    /* round 3 */
    //    HH(dd, ee, aa, bb, cc, X[ 3], 11);
    //    HH(cc, dd, ee, aa, bb, X[10], 13);
    //    HH(bb, cc, dd, ee, aa, X[14],  6);
    //    HH(aa, bb, cc, dd, ee, X[ 4],  7);
    //    HH(ee, aa, bb, cc, dd, X[ 9], 14);
    //    HH(dd, ee, aa, bb, cc, X[15],  9);
    //    HH(cc, dd, ee, aa, bb, X[ 8], 13);
    //    HH(bb, cc, dd, ee, aa, X[ 1], 15);
    //    HH(aa, bb, cc, dd, ee, X[ 2], 14);
    //    HH(ee, aa, bb, cc, dd, X[ 7],  8);
    //    HH(dd, ee, aa, bb, cc, X[ 0], 13);
    //    HH(cc, dd, ee, aa, bb, X[ 6],  6);
    //    HH(bb, cc, dd, ee, aa, X[13],  5);
    //    HH(aa, bb, cc, dd, ee, X[11], 12);
    //    HH(ee, aa, bb, cc, dd, X[ 5],  7);
    //    HH(dd, ee, aa, bb, cc, X[12],  5);
    // 
    //    /* round 4 */
    //    II(cc, dd, ee, aa, bb, X[ 1], 11);
    //    II(bb, cc, dd, ee, aa, X[ 9], 12);
    //    II(aa, bb, cc, dd, ee, X[11], 14);
    //    II(ee, aa, bb, cc, dd, X[10], 15);
    //    II(dd, ee, aa, bb, cc, X[ 0], 14);
    //    II(cc, dd, ee, aa, bb, X[ 8], 15);
    //    II(bb, cc, dd, ee, aa, X[12],  9);
    //    II(aa, bb, cc, dd, ee, X[ 4],  8);
    //    II(ee, aa, bb, cc, dd, X[13],  9);
    //    II(dd, ee, aa, bb, cc, X[ 3], 14);
    //    II(cc, dd, ee, aa, bb, X[ 7],  5);
    //    II(bb, cc, dd, ee, aa, X[15],  6);
    //    II(aa, bb, cc, dd, ee, X[14],  8);
    //    II(ee, aa, bb, cc, dd, X[ 5],  6);
    //    II(dd, ee, aa, bb, cc, X[ 6],  5);
    //    II(cc, dd, ee, aa, bb, X[ 2], 12);
    // 
    //    /* round 5 */
    //    JJ(bb, cc, dd, ee, aa, X[ 4],  9);
    //    JJ(aa, bb, cc, dd, ee, X[ 0], 15);
    //    JJ(ee, aa, bb, cc, dd, X[ 5],  5);
    //    JJ(dd, ee, aa, bb, cc, X[ 9], 11);
    //    JJ(cc, dd, ee, aa, bb, X[ 7],  6);
    //    JJ(bb, cc, dd, ee, aa, X[12],  8);
    //    JJ(aa, bb, cc, dd, ee, X[ 2], 13);
    //    JJ(ee, aa, bb, cc, dd, X[10], 12);
    //    JJ(dd, ee, aa, bb, cc, X[14],  5);
    //    JJ(cc, dd, ee, aa, bb, X[ 1], 12);
    //    JJ(bb, cc, dd, ee, aa, X[ 3], 13);
    //    JJ(aa, bb, cc, dd, ee, X[ 8], 14);
    //    JJ(ee, aa, bb, cc, dd, X[11], 11);
    //    JJ(dd, ee, aa, bb, cc, X[ 6],  8);
    //    JJ(cc, dd, ee, aa, bb, X[15],  5);
    //    JJ(bb, cc, dd, ee, aa, X[13],  6);
    // 
    //    /* parallel round 1 */
    //    JJJ(aaa, bbb, ccc, ddd, eee, X[ 5],  8);
    //    JJJ(eee, aaa, bbb, ccc, ddd, X[14],  9);
    //    JJJ(ddd, eee, aaa, bbb, ccc, X[ 7],  9);
    //    JJJ(ccc, ddd, eee, aaa, bbb, X[ 0], 11);
    //    JJJ(bbb, ccc, ddd, eee, aaa, X[ 9], 13);
    //    JJJ(aaa, bbb, ccc, ddd, eee, X[ 2], 15);
    //    JJJ(eee, aaa, bbb, ccc, ddd, X[11], 15);
    //    JJJ(ddd, eee, aaa, bbb, ccc, X[ 4],  5);
    //    JJJ(ccc, ddd, eee, aaa, bbb, X[13],  7);
    //    JJJ(bbb, ccc, ddd, eee, aaa, X[ 6],  7);
    //    JJJ(aaa, bbb, ccc, ddd, eee, X[15],  8);
    //    JJJ(eee, aaa, bbb, ccc, ddd, X[ 8], 11);
    //    JJJ(ddd, eee, aaa, bbb, ccc, X[ 1], 14);
    //    JJJ(ccc, ddd, eee, aaa, bbb, X[10], 14);
    //    JJJ(bbb, ccc, ddd, eee, aaa, X[ 3], 12);
    //    JJJ(aaa, bbb, ccc, ddd, eee, X[12],  6);
    // 
    //    /* parallel round 2 */
    //    III(eee, aaa, bbb, ccc, ddd, X[ 6],  9); 
    //    III(ddd, eee, aaa, bbb, ccc, X[11], 13);
    //    III(ccc, ddd, eee, aaa, bbb, X[ 3], 15);
    //    III(bbb, ccc, ddd, eee, aaa, X[ 7],  7);
    //    III(aaa, bbb, ccc, ddd, eee, X[ 0], 12);
    //    III(eee, aaa, bbb, ccc, ddd, X[13],  8);
    //    III(ddd, eee, aaa, bbb, ccc, X[ 5],  9);
    //    III(ccc, ddd, eee, aaa, bbb, X[10], 11);
    //    III(bbb, ccc, ddd, eee, aaa, X[14],  7);
    //    III(aaa, bbb, ccc, ddd, eee, X[15],  7);
    //    III(eee, aaa, bbb, ccc, ddd, X[ 8], 12);
    //    III(ddd, eee, aaa, bbb, ccc, X[12],  7);
    //    III(ccc, ddd, eee, aaa, bbb, X[ 4],  6);
    //    III(bbb, ccc, ddd, eee, aaa, X[ 9], 15);
    //    III(aaa, bbb, ccc, ddd, eee, X[ 1], 13);
    //    III(eee, aaa, bbb, ccc, ddd, X[ 2], 11);
    // 
    //    /* parallel round 3 */
    //    HHH(ddd, eee, aaa, bbb, ccc, X[15],  9);
    //    HHH(ccc, ddd, eee, aaa, bbb, X[ 5],  7);
    //    HHH(bbb, ccc, ddd, eee, aaa, X[ 1], 15);
    //    HHH(aaa, bbb, ccc, ddd, eee, X[ 3], 11);
    //    HHH(eee, aaa, bbb, ccc, ddd, X[ 7],  8);
    //    HHH(ddd, eee, aaa, bbb, ccc, X[14],  6);
    //    HHH(ccc, ddd, eee, aaa, bbb, X[ 6],  6);
    //    HHH(bbb, ccc, ddd, eee, aaa, X[ 9], 14);
    //    HHH(aaa, bbb, ccc, ddd, eee, X[11], 12);
    //    HHH(eee, aaa, bbb, ccc, ddd, X[ 8], 13);
    //    HHH(ddd, eee, aaa, bbb, ccc, X[12],  5);
    //    HHH(ccc, ddd, eee, aaa, bbb, X[ 2], 14);
    //    HHH(bbb, ccc, ddd, eee, aaa, X[10], 13);
    //    HHH(aaa, bbb, ccc, ddd, eee, X[ 0], 13);
    //    HHH(eee, aaa, bbb, ccc, ddd, X[ 4],  7);
    //    HHH(ddd, eee, aaa, bbb, ccc, X[13],  5);
    // 
    //    /* parallel round 4 */   
    //    GGG(ccc, ddd, eee, aaa, bbb, X[ 8], 15);
    //    GGG(bbb, ccc, ddd, eee, aaa, X[ 6],  5);
    //    GGG(aaa, bbb, ccc, ddd, eee, X[ 4],  8);
    //    GGG(eee, aaa, bbb, ccc, ddd, X[ 1], 11);
    //    GGG(ddd, eee, aaa, bbb, ccc, X[ 3], 14);
    //    GGG(ccc, ddd, eee, aaa, bbb, X[11], 14);
    //    GGG(bbb, ccc, ddd, eee, aaa, X[15],  6);
    //    GGG(aaa, bbb, ccc, ddd, eee, X[ 0], 14);
    //    GGG(eee, aaa, bbb, ccc, ddd, X[ 5],  6);
    //    GGG(ddd, eee, aaa, bbb, ccc, X[12],  9);
    //    GGG(ccc, ddd, eee, aaa, bbb, X[ 2], 12);
    //    GGG(bbb, ccc, ddd, eee, aaa, X[13],  9);
    //    GGG(aaa, bbb, ccc, ddd, eee, X[ 9], 12);
    //    GGG(eee, aaa, bbb, ccc, ddd, X[ 7],  5);
    //    GGG(ddd, eee, aaa, bbb, ccc, X[10], 15);
    //    GGG(ccc, ddd, eee, aaa, bbb, X[14],  8);
    // 
    //    /* parallel round 5 */
    //    FFF(bbb, ccc, ddd, eee, aaa, X[12] ,  8);
    //    FFF(aaa, bbb, ccc, ddd, eee, X[15] ,  5);
    //    FFF(eee, aaa, bbb, ccc, ddd, X[10] , 12);
    //    FFF(ddd, eee, aaa, bbb, ccc, X[ 4] ,  9);
    //    FFF(ccc, ddd, eee, aaa, bbb, X[ 1] , 12);
    //    FFF(bbb, ccc, ddd, eee, aaa, X[ 5] ,  5);
    //    FFF(aaa, bbb, ccc, ddd, eee, X[ 8] , 14);
    //    FFF(eee, aaa, bbb, ccc, ddd, X[ 7] ,  6);
    //    FFF(ddd, eee, aaa, bbb, ccc, X[ 6] ,  8);
    //    FFF(ccc, ddd, eee, aaa, bbb, X[ 2] , 13);
    //    FFF(bbb, ccc, ddd, eee, aaa, X[13] ,  6);
    //    FFF(aaa, bbb, ccc, ddd, eee, X[14] ,  5);
    //    FFF(eee, aaa, bbb, ccc, ddd, X[ 0] , 15);
    //    FFF(ddd, eee, aaa, bbb, ccc, X[ 3] , 13);
    //    FFF(ccc, ddd, eee, aaa, bbb, X[ 9] , 11);
    //    FFF(bbb, ccc, ddd, eee, aaa, X[11] , 11);
    // 
    //    /* combine results */
    //    ddd += cc + MDbuf[1];               /* final result for MDbuf[0] */
    //    MDbuf[1] = MDbuf[2] + dd + eee;
    //    MDbuf[2] = MDbuf[3] + ee + aaa;
    //    MDbuf[3] = MDbuf[4] + aa + bbb;
    //    MDbuf[4] = MDbuf[0] + bb + ccc;
    //    MDbuf[0] = ddd;
    // 
    //    return;
    // }
    // 
    // /********************************************************************/
    // 
    // void MDfinish(dword *MDbuf, byte *strptr, dword lswlen, dword mswlen)
    // {
    //    unsigned int i;                                 /* counter       */
    //    dword        X[16];                             /* message words */
    // 
    //    memset(X, 0, 16*sizeof(dword));
    // 
    //    /* put bytes from strptr into X */
    //    for (i=0; i<(lswlen&63); i++) {
    //       /* byte i goes into word X[i div 4] at pos.  8*(i mod 4)  */
    //       X[i>>2] ^= (dword) *strptr++ << (8 * (i&3));
    //    }
    // 
    //    /* append the bit m_n == 1 */
    //    X[(lswlen>>2)&15] ^= (dword)1 << (8*(lswlen&3) + 7);
    // 
    //    if ((lswlen & 63) > 55) {
    //       /* length goes to next block */
    //       compress(MDbuf, X);
    //       memset(X, 0, 16*sizeof(dword));
    //    }
    // 
    //    /* append length in bits*/
    //    X[14] = lswlen << 3;
    //    X[15] = (lswlen >> 29) | (mswlen << 3);
    //    compress(MDbuf, X);
    // 
    //    return;
    // }
    // 
    // /************************ end of file rmd160.c **********************/

    #endregion

    #region rmd160.c (csharp)

    /// <summary>
    /// Initializes MDbuffer to "magic constants"
    /// </summary>
    private void InitializeBuffer(uint[] buffer)
    {
      buffer[0] = 0x67452301U;
      buffer[1] = 0xefcdab89U;
      buffer[2] = 0x98badcfeU;
      buffer[3] = 0x10325476U;
      buffer[4] = 0xc3d2e1f0U;
    }

    /// <summary>
    /// the compression function.
    /// transforms MDbuf using message bytes X[0] through X[15]
    /// </summary>
    private void CompressBlock(uint[] buffer, uint[] block)
    {
      uint aa = buffer[0], bb = buffer[1], cc = buffer[2], dd = buffer[3], ee = buffer[4];
      uint aaa = buffer[0], bbb = buffer[1], ccc = buffer[2], ddd = buffer[3], eee = buffer[4];

      // round 1
      FF(ref aa, bb, ref cc, dd, ee, block[00], 11);
      FF(ref ee, aa, ref bb, cc, dd, block[01], 14);
      FF(ref dd, ee, ref aa, bb, cc, block[02], 15);
      FF(ref cc, dd, ref ee, aa, bb, block[03], 12);
      FF(ref bb, cc, ref dd, ee, aa, block[04], 05);
      FF(ref aa, bb, ref cc, dd, ee, block[05], 08);
      FF(ref ee, aa, ref bb, cc, dd, block[06], 07);
      FF(ref dd, ee, ref aa, bb, cc, block[07], 09);
      FF(ref cc, dd, ref ee, aa, bb, block[08], 11);
      FF(ref bb, cc, ref dd, ee, aa, block[09], 13);
      FF(ref aa, bb, ref cc, dd, ee, block[10], 14);
      FF(ref ee, aa, ref bb, cc, dd, block[11], 15);
      FF(ref dd, ee, ref aa, bb, cc, block[12], 06);
      FF(ref cc, dd, ref ee, aa, bb, block[13], 07);
      FF(ref bb, cc, ref dd, ee, aa, block[14], 09);
      FF(ref aa, bb, ref cc, dd, ee, block[15], 08);

      // round 2
      GG(ref ee, aa, ref bb, cc, dd, block[07], 07);
      GG(ref dd, ee, ref aa, bb, cc, block[04], 06);
      GG(ref cc, dd, ref ee, aa, bb, block[13], 08);
      GG(ref bb, cc, ref dd, ee, aa, block[01], 13);
      GG(ref aa, bb, ref cc, dd, ee, block[10], 11);
      GG(ref ee, aa, ref bb, cc, dd, block[06], 09);
      GG(ref dd, ee, ref aa, bb, cc, block[15], 07);
      GG(ref cc, dd, ref ee, aa, bb, block[03], 15);
      GG(ref bb, cc, ref dd, ee, aa, block[12], 07);
      GG(ref aa, bb, ref cc, dd, ee, block[00], 12);
      GG(ref ee, aa, ref bb, cc, dd, block[09], 15);
      GG(ref dd, ee, ref aa, bb, cc, block[05], 09);
      GG(ref cc, dd, ref ee, aa, bb, block[02], 11);
      GG(ref bb, cc, ref dd, ee, aa, block[14], 07);
      GG(ref aa, bb, ref cc, dd, ee, block[11], 13);
      GG(ref ee, aa, ref bb, cc, dd, block[08], 12);

      // round 3
      HH(ref dd, ee, ref aa, bb, cc, block[03], 11);
      HH(ref cc, dd, ref ee, aa, bb, block[10], 13);
      HH(ref bb, cc, ref dd, ee, aa, block[14], 06);
      HH(ref aa, bb, ref cc, dd, ee, block[04], 07);
      HH(ref ee, aa, ref bb, cc, dd, block[09], 14);
      HH(ref dd, ee, ref aa, bb, cc, block[15], 09);
      HH(ref cc, dd, ref ee, aa, bb, block[08], 13);
      HH(ref bb, cc, ref dd, ee, aa, block[01], 15);
      HH(ref aa, bb, ref cc, dd, ee, block[02], 14);
      HH(ref ee, aa, ref bb, cc, dd, block[07], 08);
      HH(ref dd, ee, ref aa, bb, cc, block[00], 13);
      HH(ref cc, dd, ref ee, aa, bb, block[06], 06);
      HH(ref bb, cc, ref dd, ee, aa, block[13], 05);
      HH(ref aa, bb, ref cc, dd, ee, block[11], 12);
      HH(ref ee, aa, ref bb, cc, dd, block[05], 07);
      HH(ref dd, ee, ref aa, bb, cc, block[12], 05);

      // round 4
      II(ref cc, dd, ref ee, aa, bb, block[01], 11);
      II(ref bb, cc, ref dd, ee, aa, block[09], 12);
      II(ref aa, bb, ref cc, dd, ee, block[11], 14);
      II(ref ee, aa, ref bb, cc, dd, block[10], 15);
      II(ref dd, ee, ref aa, bb, cc, block[00], 14);
      II(ref cc, dd, ref ee, aa, bb, block[08], 15);
      II(ref bb, cc, ref dd, ee, aa, block[12], 09);
      II(ref aa, bb, ref cc, dd, ee, block[04], 08);
      II(ref ee, aa, ref bb, cc, dd, block[13], 09);
      II(ref dd, ee, ref aa, bb, cc, block[03], 14);
      II(ref cc, dd, ref ee, aa, bb, block[07], 05);
      II(ref bb, cc, ref dd, ee, aa, block[15], 06);
      II(ref aa, bb, ref cc, dd, ee, block[14], 08);
      II(ref ee, aa, ref bb, cc, dd, block[05], 06);
      II(ref dd, ee, ref aa, bb, cc, block[06], 05);
      II(ref cc, dd, ref ee, aa, bb, block[02], 12);

      // round 5
      JJ(ref bb, cc, ref dd, ee, aa, block[04], 09);
      JJ(ref aa, bb, ref cc, dd, ee, block[00], 15);
      JJ(ref ee, aa, ref bb, cc, dd, block[05], 05);
      JJ(ref dd, ee, ref aa, bb, cc, block[09], 11);
      JJ(ref cc, dd, ref ee, aa, bb, block[07], 06);
      JJ(ref bb, cc, ref dd, ee, aa, block[12], 08);
      JJ(ref aa, bb, ref cc, dd, ee, block[02], 13);
      JJ(ref ee, aa, ref bb, cc, dd, block[10], 12);
      JJ(ref dd, ee, ref aa, bb, cc, block[14], 05);
      JJ(ref cc, dd, ref ee, aa, bb, block[01], 12);
      JJ(ref bb, cc, ref dd, ee, aa, block[03], 13);
      JJ(ref aa, bb, ref cc, dd, ee, block[08], 14);
      JJ(ref ee, aa, ref bb, cc, dd, block[11], 11);
      JJ(ref dd, ee, ref aa, bb, cc, block[06], 08);
      JJ(ref cc, dd, ref ee, aa, bb, block[15], 05);
      JJ(ref bb, cc, ref dd, ee, aa, block[13], 06);

      // parallel round 1
      JJJ(ref aaa, bbb, ref ccc, ddd, eee, block[05], 08);
      JJJ(ref eee, aaa, ref bbb, ccc, ddd, block[14], 09);
      JJJ(ref ddd, eee, ref aaa, bbb, ccc, block[07], 09);
      JJJ(ref ccc, ddd, ref eee, aaa, bbb, block[00], 11);
      JJJ(ref bbb, ccc, ref ddd, eee, aaa, block[09], 13);
      JJJ(ref aaa, bbb, ref ccc, ddd, eee, block[02], 15);
      JJJ(ref eee, aaa, ref bbb, ccc, ddd, block[11], 15);
      JJJ(ref ddd, eee, ref aaa, bbb, ccc, block[04], 05);
      JJJ(ref ccc, ddd, ref eee, aaa, bbb, block[13], 07);
      JJJ(ref bbb, ccc, ref ddd, eee, aaa, block[06], 07);
      JJJ(ref aaa, bbb, ref ccc, ddd, eee, block[15], 08);
      JJJ(ref eee, aaa, ref bbb, ccc, ddd, block[08], 11);
      JJJ(ref ddd, eee, ref aaa, bbb, ccc, block[01], 14);
      JJJ(ref ccc, ddd, ref eee, aaa, bbb, block[10], 14);
      JJJ(ref bbb, ccc, ref ddd, eee, aaa, block[03], 12);
      JJJ(ref aaa, bbb, ref ccc, ddd, eee, block[12], 06);

      // parallel round 02
      III(ref eee, aaa, ref bbb, ccc, ddd, block[06], 09);
      III(ref ddd, eee, ref aaa, bbb, ccc, block[11], 13);
      III(ref ccc, ddd, ref eee, aaa, bbb, block[03], 15);
      III(ref bbb, ccc, ref ddd, eee, aaa, block[07], 07);
      III(ref aaa, bbb, ref ccc, ddd, eee, block[00], 12);
      III(ref eee, aaa, ref bbb, ccc, ddd, block[13], 08);
      III(ref ddd, eee, ref aaa, bbb, ccc, block[05], 09);
      III(ref ccc, ddd, ref eee, aaa, bbb, block[10], 11);
      III(ref bbb, ccc, ref ddd, eee, aaa, block[14], 07);
      III(ref aaa, bbb, ref ccc, ddd, eee, block[15], 07);
      III(ref eee, aaa, ref bbb, ccc, ddd, block[08], 12);
      III(ref ddd, eee, ref aaa, bbb, ccc, block[12], 07);
      III(ref ccc, ddd, ref eee, aaa, bbb, block[04], 06);
      III(ref bbb, ccc, ref ddd, eee, aaa, block[09], 15);
      III(ref aaa, bbb, ref ccc, ddd, eee, block[01], 13);
      III(ref eee, aaa, ref bbb, ccc, ddd, block[02], 11);

      // parallel round 03
      HHH(ref ddd, eee, ref aaa, bbb, ccc, block[15], 09);
      HHH(ref ccc, ddd, ref eee, aaa, bbb, block[05], 07);
      HHH(ref bbb, ccc, ref ddd, eee, aaa, block[01], 15);
      HHH(ref aaa, bbb, ref ccc, ddd, eee, block[03], 11);
      HHH(ref eee, aaa, ref bbb, ccc, ddd, block[07], 08);
      HHH(ref ddd, eee, ref aaa, bbb, ccc, block[14], 06);
      HHH(ref ccc, ddd, ref eee, aaa, bbb, block[06], 06);
      HHH(ref bbb, ccc, ref ddd, eee, aaa, block[09], 14);
      HHH(ref aaa, bbb, ref ccc, ddd, eee, block[11], 12);
      HHH(ref eee, aaa, ref bbb, ccc, ddd, block[08], 13);
      HHH(ref ddd, eee, ref aaa, bbb, ccc, block[12], 05);
      HHH(ref ccc, ddd, ref eee, aaa, bbb, block[02], 14);
      HHH(ref bbb, ccc, ref ddd, eee, aaa, block[10], 13);
      HHH(ref aaa, bbb, ref ccc, ddd, eee, block[00], 13);
      HHH(ref eee, aaa, ref bbb, ccc, ddd, block[04], 07);
      HHH(ref ddd, eee, ref aaa, bbb, ccc, block[13], 05);

      // parallel round 04
      GGG(ref ccc, ddd, ref eee, aaa, bbb, block[08], 15);
      GGG(ref bbb, ccc, ref ddd, eee, aaa, block[06], 05);
      GGG(ref aaa, bbb, ref ccc, ddd, eee, block[04], 08);
      GGG(ref eee, aaa, ref bbb, ccc, ddd, block[01], 11);
      GGG(ref ddd, eee, ref aaa, bbb, ccc, block[03], 14);
      GGG(ref ccc, ddd, ref eee, aaa, bbb, block[11], 14);
      GGG(ref bbb, ccc, ref ddd, eee, aaa, block[15], 06);
      GGG(ref aaa, bbb, ref ccc, ddd, eee, block[00], 14);
      GGG(ref eee, aaa, ref bbb, ccc, ddd, block[05], 06);
      GGG(ref ddd, eee, ref aaa, bbb, ccc, block[12], 09);
      GGG(ref ccc, ddd, ref eee, aaa, bbb, block[02], 12);
      GGG(ref bbb, ccc, ref ddd, eee, aaa, block[13], 09);
      GGG(ref aaa, bbb, ref ccc, ddd, eee, block[09], 12);
      GGG(ref eee, aaa, ref bbb, ccc, ddd, block[07], 05);
      GGG(ref ddd, eee, ref aaa, bbb, ccc, block[10], 15);
      GGG(ref ccc, ddd, ref eee, aaa, bbb, block[14], 08);

      // parallel round 05
      FFF(ref bbb, ccc, ref ddd, eee, aaa, block[12], 08);
      FFF(ref aaa, bbb, ref ccc, ddd, eee, block[15], 05);
      FFF(ref eee, aaa, ref bbb, ccc, ddd, block[10], 12);
      FFF(ref ddd, eee, ref aaa, bbb, ccc, block[04], 09);
      FFF(ref ccc, ddd, ref eee, aaa, bbb, block[01], 12);
      FFF(ref bbb, ccc, ref ddd, eee, aaa, block[05], 05);
      FFF(ref aaa, bbb, ref ccc, ddd, eee, block[08], 14);
      FFF(ref eee, aaa, ref bbb, ccc, ddd, block[07], 06);
      FFF(ref ddd, eee, ref aaa, bbb, ccc, block[06], 08);
      FFF(ref ccc, ddd, ref eee, aaa, bbb, block[02], 13);
      FFF(ref bbb, ccc, ref ddd, eee, aaa, block[13], 06);
      FFF(ref aaa, bbb, ref ccc, ddd, eee, block[14], 05);
      FFF(ref eee, aaa, ref bbb, ccc, ddd, block[00], 15);
      FFF(ref ddd, eee, ref aaa, bbb, ccc, block[03], 13);
      FFF(ref ccc, ddd, ref eee, aaa, bbb, block[09], 11);
      FFF(ref bbb, ccc, ref ddd, eee, aaa, block[11], 11);

      // combine the results
      ddd += cc + buffer[1];
      buffer[1] = buffer[2] + dd + eee;
      buffer[2] = buffer[3] + ee + aaa;
      buffer[3] = buffer[4] + aa + bbb;
      buffer[4] = buffer[0] + bb + ccc;
      buffer[0] = ddd;
    }

    /// <summary>
    /// puts bytes from strptr into X and pad out; appends length 
    /// and finally, compresses the last block(s)
    /// note: length in bits == 8 * (lswlen + 2^32 mswlen).
    /// note: there are(lswlen mod 64) bytes left in strptr.
    /// </summary>
    private void Finish(uint[] buffer, byte[] inputBytes, uint lswlen, uint mswlen)
    {
      var x = new uint[16];

      uint i;
      for (i = 0; i < (lswlen & 63); i++)
      {
        x[i >> 2] ^= (uint)(inputBytes[i] << (8 * (byte)(i & 3)));
      }

      x[(lswlen >> 2) & 15] ^= (uint)1 << ((8 * (byte)(lswlen & 3)) + 7);

      if ((lswlen & 63) > 55)
      {
        CompressBlock(buffer, x);
        x = new uint[16];
      }

      x[14] = lswlen << 3;
      x[15] = (lswlen >> 29) | (mswlen << 3);
      CompressBlock(buffer, x);
    }

    #endregion

    #region rmd160.h (original)

    // /********************************************************************\
    //  *
    //  *      FILE:     rmd160.h
    //  *
    //  *      CONTENTS: Header file for a sample C-implementation of the
    //  *                RIPEMD-160 hash-function. 
    //  *      TARGET:   any computer with an ANSI C compiler
    //  *
    //  *      AUTHOR:   Antoon Bosselaers, ESAT-COSIC
    //  *      DATE:     1 March 1996
    //  *      VERSION:  1.0
    //  *
    //  *      Copyright (c) 1996 Katholieke Universiteit Leuven
    //  *
    //  *      Permission is hereby granted, free of charge, to any person
    //  *      obtaining a copy of this software and associated documentation
    //  *      files (the "Software"), to deal in the Software without restriction,
    //  *      including without limitation the rights to use, copy, modify, merge,
    //  *      publish, distribute, sublicense, and/or sell copies of the Software,
    //  *      and to permit persons to whom the Software is furnished to do so,
    //  *      subject to the following conditions:
    //  *
    //  *      The above copyright notice and this permission notice shall be 
    //  *      included in all copies or substantial portions of the Software.
    //  *
    //  *      THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
    //  *      EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
    //  *      MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
    //  *      IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY 
    //  *      CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, 
    //  *      TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
    //  *      SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
    //  *
    // \********************************************************************/
    // 
    // #ifndef  RMD160H           /* make sure this file is read only once */
    // #define  RMD160H
    // 
    // /********************************************************************/
    // 
    // /* typedef 8 and 32 bit types, resp.  */
    // /* adapt these, if necessary, 
    //    for your operating system and compiler */
    // typedef    unsigned char        byte;
    // typedef    unsigned long        dword;
    // 
    // /* if this line causes a compiler error, 
    //    adapt the defintion of dword above */
    // typedef int the_correct_size_was_chosen [sizeof (dword) == 4? 1: -1];
    // 
    // /********************************************************************/
    // 
    // /* macro definitions */
    // 
    // /* collect four bytes into one word: */
    // #define BYTES_TO_DWORD(strptr)                    \
    //             (((dword) *((strptr)+3) << 24) | \
    //              ((dword) *((strptr)+2) << 16) | \
    //              ((dword) *((strptr)+1) <<  8) | \
    //              ((dword) *(strptr)))
    // 
    // /* ROL(x, n) cyclically rotates x over n bits to the left */
    // /* x must be of an unsigned 32 bits type and 0 <= n < 32. */
    // #define ROL(x, n)        (((x) << (n)) | ((x) >> (32-(n))))
    // 
    // /* the five basic functions F(), G() and H() */
    // #define F(x, y, z)        ((x) ^ (y) ^ (z)) 
    // #define G(x, y, z)        (((x) & (y)) | (~(x) & (z))) 
    // #define H(x, y, z)        (((x) | ~(y)) ^ (z))
    // #define I(x, y, z)        (((x) & (z)) | ((y) & ~(z))) 
    // #define J(x, y, z)        ((x) ^ ((y) | ~(z)))
    //   
    // /* the ten basic operations FF() through III() */
    // #define FF(a, b, c, d, e, x, s)        {\
    //       (a) += F((b), (c), (d)) + (x);\
    //       (a) = ROL((a), (s)) + (e);\
    //       (c) = ROL((c), 10);\
    //    }
    // #define GG(a, b, c, d, e, x, s)        {\
    //       (a) += G((b), (c), (d)) + (x) + 0x5a827999UL;\
    //       (a) = ROL((a), (s)) + (e);\
    //       (c) = ROL((c), 10);\
    //    }
    // #define HH(a, b, c, d, e, x, s)        {\
    //       (a) += H((b), (c), (d)) + (x) + 0x6ed9eba1UL;\
    //       (a) = ROL((a), (s)) + (e);\
    //       (c) = ROL((c), 10);\
    //    }
    // #define II(a, b, c, d, e, x, s)        {\
    //       (a) += I((b), (c), (d)) + (x) + 0x8f1bbcdcUL;\
    //       (a) = ROL((a), (s)) + (e);\
    //       (c) = ROL((c), 10);\
    //    }
    // #define JJ(a, b, c, d, e, x, s)        {\
    //       (a) += J((b), (c), (d)) + (x) + 0xa953fd4eUL;\
    //       (a) = ROL((a), (s)) + (e);\
    //       (c) = ROL((c), 10);\
    //    }
    // #define FFF(a, b, c, d, e, x, s)        {\
    //       (a) += F((b), (c), (d)) + (x);\
    //       (a) = ROL((a), (s)) + (e);\
    //       (c) = ROL((c), 10);\
    //    }
    // #define GGG(a, b, c, d, e, x, s)        {\
    //       (a) += G((b), (c), (d)) + (x) + 0x7a6d76e9UL;\
    //       (a) = ROL((a), (s)) + (e);\
    //       (c) = ROL((c), 10);\
    //    }
    // #define HHH(a, b, c, d, e, x, s)        {\
    //       (a) += H((b), (c), (d)) + (x) + 0x6d703ef3UL;\
    //       (a) = ROL((a), (s)) + (e);\
    //       (c) = ROL((c), 10);\
    //    }
    // #define III(a, b, c, d, e, x, s)        {\
    //       (a) += I((b), (c), (d)) + (x) + 0x5c4dd124UL;\
    //       (a) = ROL((a), (s)) + (e);\
    //       (c) = ROL((c), 10);\
    //    }
    // #define JJJ(a, b, c, d, e, x, s)        {\
    //       (a) += J((b), (c), (d)) + (x) + 0x50a28be6UL;\
    //       (a) = ROL((a), (s)) + (e);\
    //       (c) = ROL((c), 10);\
    //    }
    // 
    // /********************************************************************/
    // 
    // /* function prototypes */
    // 
    // void MDinit(dword *MDbuf);
    // /*
    //  *  initializes MDbuffer to "magic constants"
    //  */
    // 
    // void compress(dword *MDbuf, dword *X);
    // /*
    //  *  the compression function.
    //  *  transforms MDbuf using message bytes X[0] through X[15]
    //  */
    // 
    // void MDfinish(dword *MDbuf, byte *strptr, dword lswlen, dword mswlen);
    // /*
    //  *  puts bytes from strptr into X and pad out; appends length 
    //  *  and finally, compresses the last block(s)
    //  *  note: length in bits == 8 * (lswlen + 2^32 mswlen).
    //  *  note: there are (lswlen mod 64) bytes left in strptr.
    //  */
    // 
    // #endif  /* RMD160H */
    // 
    // /*********************** end of file rmd160.h ***********************/

    #endregion

    #region rmd160.h (csharp)

    /// <summary>
    /// collect four bytes into one word
    /// </summary>
    private static uint BYTES_TO_DWORD(byte[] strptr) =>
      (((uint)strptr[3]) << 24) |
      (((uint)strptr[2]) << 16) |
      (((uint)strptr[1]) << 08) |
      (((uint)strptr[0]) << 00);

    private static byte[] DWORDS_TO_BYTES(uint[] buffer)
    {
      var bytes = new byte[buffer.LongLength * BytesPerDword];
      for (long iBuffer = 0, iByte = 0; iBuffer < buffer.LongLength; iBuffer++)
      {
        bytes[iByte++] = (byte)((buffer[iBuffer] >> 00) & 0xff);
        bytes[iByte++] = (byte)((buffer[iBuffer] >> 08) & 0xff);
        bytes[iByte++] = (byte)((buffer[iBuffer] >> 16) & 0xff);
        bytes[iByte++] = (byte)((buffer[iBuffer] >> 24) & 0xff);
      }
      return bytes;
    }

    /// <summary>
    /// ROL(x, n) cyclically rotates x over n bits to the left
    /// x must be of an unsigned 32 bits type and 0 <= n< 32
    /// </summary>
    private static uint ROL(uint x, int n) => (x << n) | (x >> (32 - n));

    private static uint F(uint x, uint y, uint z) => x ^ y ^ z;
    private static uint G(uint x, uint y, uint z) => (x & y) | (~x & z);
    private static uint H(uint x, uint y, uint z) => (x | ~y) ^ z;
    private static uint I(uint x, uint y, uint z) => (x & z) | (y & ~z);
    private static uint J(uint x, uint y, uint z) => x ^ (y | ~z);

    private static void FF(ref uint a, uint b, ref uint c, uint d, uint e, uint x, int s)
    {
      a += F(b, c, d) + x;
      a = ROL(a, s) + e;
      c = ROL(c, 10);
    }
    private static void GG(ref uint a, uint b, ref uint c, uint d, uint e, uint x, int s)
    {
      a += G(b, c, d) + x + 0x5a827999U;
      a = ROL(a, s) + e;
      c = ROL(c, 10);
    }
    private static void HH(ref uint a, uint b, ref uint c, uint d, uint e, uint x, int s)
    {
      a += H(b, c, d) + x + 0x6ed9eba1U;
      a = ROL(a, s) + e;
      c = ROL(c, 10);
    }
    private static void II(ref uint a, uint b, ref uint c, uint d, uint e, uint x, int s)
    {
      a += I(b, c, d) + x + 0x8f1bbcdcU;
      a = ROL(a, s) + e;
      c = ROL(c, 10);
    }
    private static void JJ(ref uint a, uint b, ref uint c, uint d, uint e, uint x, int s)
    {
      a += J(b, c, d) + x + 0xa953fd4eU;
      a = ROL(a, s) + e;
      c = ROL(c, 10);
    }

    private static void FFF(ref uint a, uint b, ref uint c, uint d, uint e, uint x, int s)
    {
      a += F(b, c, d) + x;
      a = ROL(a, s) + e;
      c = ROL(c, 10);
    }
    private static void GGG(ref uint a, uint b, ref uint c, uint d, uint e, uint x, int s)
    {
      a += G(b, c, d) + x + 0x7a6d76e9U;
      a = ROL(a, s) + e;
      c = ROL(c, 10);
    }
    private static void HHH(ref uint a, uint b, ref uint c, uint d, uint e, uint x, int s)
    {
      a += H(b, c, d) + x + 0x6d703ef3U;
      a = ROL(a, s) + e;
      c = ROL(c, 10);
    }
    private static void III(ref uint a, uint b, ref uint c, uint d, uint e, uint x, int s)
    {
      a += I(b, c, d) + x + 0x5c4dd124U;
      a = ROL(a, s) + e;
      c = ROL(c, 10);
    }
    private static void JJJ(ref uint a, uint b, ref uint c, uint d, uint e, uint x, int s)
    {
      a += J(b, c, d) + x + 0x50a28be6U;
      a = ROL(a, s) + e;
      c = ROL(c, 10);
    }

    #endregion
  }
}
