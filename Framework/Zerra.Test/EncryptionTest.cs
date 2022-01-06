// Copyright � KaKush LLC
// Written By Steven Zawaski
// Licensed to you under the MIT license

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Zerra.Encryption;
using Zerra.Serialization;

namespace Zerra.Test
{
    [TestClass]
    public partial class EncryptionTest
    {
        [TestMethod]
        public void CryptoShiftStreamRead()
        {
            const int blockSize = 256;
            var test = GetTestBytes();

            var shiftmsin = new MemoryStream(test);
            var shiftmsout = new MemoryStream();
            var shiftstream = new CryptoShiftStream(shiftmsin, blockSize, CryptoStreamMode.Read, false, false);
            shiftstream.CopyTo(shiftmsout);
            var result = shiftmsout.ToArray();

            Assert.IsTrue(result.Length - blockSize / 8 == test.Length);

            var unshiftmsin = new MemoryStream(result);
            var unshiftmsout = new MemoryStream();
            var unshift = new CryptoShiftStream(unshiftmsin, blockSize, CryptoStreamMode.Read, true, false);
            unshift.CopyTo(unshiftmsout);
            var unshiftresult = unshiftmsout.ToArray();

            Assert.AreEqual(test.Length, unshiftresult.Length);
            for (var i = 0; i < test.Length; i++)
                Assert.AreEqual(test[i], unshiftresult[i]);
        }

        [TestMethod]
        public void CryptoShiftStreamWrite()
        {
            const int blockSize = 256;
            var test = GetTestBytes();

            var shiftmsout = new MemoryStream();
            var shift = new CryptoShiftStream(shiftmsout, blockSize, CryptoStreamMode.Write, false, false);
            shift.Write(test, 0, test.Length);
            shiftmsout.Position = 0;
            var result = shiftmsout.ToArray();

            Assert.IsTrue(result.Length - blockSize / 8 == test.Length);

            var unshiftmsout = new MemoryStream();
            var unshift = new CryptoShiftStream(unshiftmsout, blockSize, CryptoStreamMode.Write, true, false);
            unshift.Write(result, 0, result.Length);
            unshiftmsout.Position = 0;
            var unshiftresult = unshiftmsout.ToArray();

            Assert.AreEqual(test.Length, unshiftresult.Length);
            for (var i = 0; i < test.Length; i++)
                Assert.AreEqual(test[i], unshiftresult[i]);
        }

        [TestMethod]
        public void CryptoShiftStreamReadAsync()
        {
            var test = GetTestBytes();

            var shiftmsin = new MemoryStream(test);
            var shiftmsout = new MemoryStream();
            var shiftstream = new CryptoShiftStream(shiftmsin, 256, CryptoStreamMode.Read, false, false);
            shiftstream.CopyToAsync(shiftmsout).GetAwaiter().GetResult();
            var result = shiftmsout.ToArray();

            Assert.IsTrue(result.Length > test.Length);

            var unshiftmsin = new MemoryStream(result);
            var unshiftmsout = new MemoryStream();
            var unshift = new CryptoShiftStream(unshiftmsin, 256, CryptoStreamMode.Read, true, false);
            unshift.CopyToAsync(unshiftmsout).GetAwaiter().GetResult();
            var unshiftresult = unshiftmsout.ToArray();

            Assert.AreEqual(test.Length, unshiftresult.Length);
            for (var i = 0; i < test.Length; i++)
                Assert.AreEqual(test[i], unshiftresult[i]);
        }

        [TestMethod]
        public void CryptoShiftStreamWriteAsync()
        {
            var test = GetTestBytes();

            var shiftmsout = new MemoryStream();
            var shift = new CryptoShiftStream(shiftmsout, 256, CryptoStreamMode.Write, false, false);
            shift.WriteAsync(test, 0, test.Length).GetAwaiter().GetResult();
            shiftmsout.Position = 0;
            var result = shiftmsout.ToArray();

            Assert.IsTrue(result.Length > test.Length);

            var unshiftmsout = new MemoryStream();
            var unshift = new CryptoShiftStream(unshiftmsout, 256, CryptoStreamMode.Write, true, false);
            unshift.WriteAsync(result, 0, result.Length).GetAwaiter().GetResult();
            unshiftmsout.Position = 0;
            var unshiftresult = unshiftmsout.ToArray();

            Assert.AreEqual(test.Length, unshiftresult.Length);
            for (var i = 0; i < test.Length; i++)
                Assert.AreEqual(test[i], unshiftresult[i]);
        }

        [TestMethod]
        public void CryptoShiftUnique()
        {
            var test = Convert.ToBase64String(GetTestBytes());
            var key = SymmetricEncryptor.GetKey("test", null, SymmetricKeySize.Bits_256, SymmetricBlockSize.Bits_128);

            var values = new List<string>();
            for (var i = 0; i < 100; i++)
                values.Add(SymmetricEncryptor.Encrypt(SymmetricAlgorithmType.AESwithShift, key, test));

            Assert.IsTrue(values.Distinct().Count() == values.Count);

            for (var i = 0; i < values.Count; i++)
            {
                var decrypted = SymmetricEncryptor.Decrypt(SymmetricAlgorithmType.AESwithShift, key, values[i]);
                Assert.AreEqual(test, decrypted);
            }
        }

        [TestMethod]
        public void SymmetricEncryptorWithShift()
        {
            var test = Convert.ToBase64String(GetTestBytes());
            var key = SymmetricEncryptor.GetKey("test", null, SymmetricKeySize.Bits_256, SymmetricBlockSize.Bits_128);

            var encrypted = SymmetricEncryptor.Encrypt(SymmetricAlgorithmType.AESwithShift, key, test);

            var ms = new MemoryStream(Convert.FromBase64String(encrypted));
            var decryptionStream = SymmetricEncryptor.Decrypt(SymmetricAlgorithmType.AESwithShift, key, ms, false, false);

            var msout = new MemoryStream();
            decryptionStream.CopyTo(msout);
            var result = Encoding.UTF8.GetString(msout.ToArray());

            Assert.AreEqual(test, result);
        }

        [TestMethod]
        public void WithSerializer()
        {
            var key = SymmetricEncryptor.GenerateKey(SymmetricAlgorithmType.AESwithShift);

            var serializer = new ByteSerializer();
            var model1 = Factory.GetTestTypesModel();
            using (var ms = new MemoryStream())
            using (var cryptoStreamWriter = SymmetricEncryptor.Encrypt(SymmetricAlgorithmType.AESwithShift, key, ms, true, false))
            using (var cryptoStreamReader = SymmetricEncryptor.Decrypt(SymmetricAlgorithmType.AESwithShift, key, ms, false, false))
            {
                var expected = serializer.Serialize(model1);
                serializer.SerializeAsync(cryptoStreamWriter, model1).GetAwaiter().GetResult();
                cryptoStreamWriter.FlushFinalBlock();
                ms.Position = 0;
                var bytes = ms.ToArray();
                var model2 = serializer.DeserializeAsync<AllTypesModel>(cryptoStreamReader).GetAwaiter().GetResult();
                Factory.AssertAreEqual(model1, model2);
            }
        }

        private static byte[] GetTestBytes()
        {
            var bytes = new List<byte>();
            for (var i = 0; i < 100000; i++)
                bytes.Add((byte)i);
            var test = bytes.ToArray();
            return test;
        }
    }
}
