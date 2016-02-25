using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Security.Cryptography;
using System.IO;
namespace Mishavad_API.Helpers
{
    public static class EncryptionService
    {
        /// <summary>
        /// This is a "made-up" object to store Key and IV 
        /// I just did have no idea what to name it
        /// </summary>
        private struct BinaryBucket
        {
            public byte[] InitVector;
            public byte[] Key;
        }
        private static BinaryReader binaryReader;


        /// <summary>
        /// Loads the binary file
        /// </summary>
        /// <param name="binaryFilePath"></param>
        public static void LoadBinaryFile(string binaryFilePath)
        {
            binaryReader = new BinaryReader(File.OpenRead(binaryFilePath));

        }

        public static string Encrypt(string input, int BF_Idx)
        {
            var enc_version = "brisk-1.0.0";

            byte[] clearBytes = System.Text.Encoding.UTF8.GetBytes(input);
            byte[] foggyBytes;

            using (RijndaelManaged provider = new RijndaelManaged())
            {
                var bb = GetBinaryBucket(BF_Idx);
                foggyBytes = Transform(clearBytes,
                provider.CreateEncryptor(bb.Key, bb.InitVector));
            }
            // Add leading version with underline
            return (enc_version.Substring(0, 1) + "_" + Convert.ToBase64String(foggyBytes));
        }

        public static string Decrypt(string input, int BF_Idx)
        {
            //Check version and decrypt accordingly
            byte[] foggyBytes;
            if (input.IndexOf('_') > -1)
            {
                var version_letter = input.Substring(0, 1);
                foggyBytes = Convert.FromBase64String(input.Substring(2));
            }
            else {
                foggyBytes = Convert.FromBase64String(input);
            }


            byte[] clearBytes;

            var bb = GetBinaryBucket(BF_Idx);

            using (RijndaelManaged provider = new RijndaelManaged())
            {
                clearBytes = Transform(foggyBytes, provider.CreateDecryptor(bb.Key, bb.InitVector));
            }
            return System.Text.Encoding.UTF8.GetString(clearBytes);
        }

        public static byte[] EncryptBytes(byte[] clearBytes, int BF_Idx)
        {
            byte[] foggyBytes;

            using (RijndaelManaged provider = new RijndaelManaged())
            {
                var bb = GetBinaryBucket(BF_Idx);
                foggyBytes = Transform(clearBytes,
                provider.CreateEncryptor(bb.Key, bb.InitVector));
            }

            byte[] enc_version = { 1, 0, 0, 0 };

            // Add 4 leading bytes for version 
            byte[] output = new byte[4 + foggyBytes.Length];
            System.Buffer.BlockCopy(enc_version, 0, output, 0, 4);
            System.Buffer.BlockCopy(foggyBytes, 0, output, 4, foggyBytes.Length);
            return output;
        }

        public static byte[] DecryptBytes(byte[] input, int BF_Idx)
        {
            //Check version and decrypt accordingly
            byte[] foggyBytes;
            var version = string.Join(".",input.Take(4).Select(b => b.ToString()));
            if (version=="1.0.0.0")
            {
                foggyBytes = new byte[input.Length - 4];
                System.Buffer.BlockCopy(input, 4, foggyBytes, 0, foggyBytes.Length);
            }
            else {
                foggyBytes = input;
            }

            byte[] clearBytes;

            var bb = GetBinaryBucket(BF_Idx);

            using (RijndaelManaged provider = new RijndaelManaged())
            {
                clearBytes = Transform(foggyBytes, provider.CreateDecryptor(bb.Key, bb.InitVector));
            }
            return clearBytes;
        }

        //TODO: Add a decrypt stream method

       

        private static byte[] Transform(byte[] inputBytes, ICryptoTransform transform)
        {
           
                using (var buf = new System.IO.MemoryStream())
                {
                Console.WriteLine("input ZERO Bytes" + inputBytes.Where(b => b == 0).Count().ToString());
                    using (var stream = new CryptoStream(buf, transform, CryptoStreamMode.Write))
                    {
                        stream.Write(inputBytes, 0, inputBytes.Length);
                        stream.FlushFinalBlock();
                        return buf.ToArray();
                    }
                }
            
        }

        

        private static BinaryBucket GetBinaryBucket(long BF_Idx)
        {
            //Each 48 Bytes contains an IV (16 Bytes) and a Key (32 Bytes) 
            binaryReader.BaseStream.Seek(BF_Idx * 48, SeekOrigin.Begin);
            return new BinaryBucket
            {
                InitVector = binaryReader.ReadBytes(128 / 8),   // Converting 128 bits to bytes}
                Key = binaryReader.ReadBytes(256 / 8)          // Converting 256 bits to bytes 
            };
        }

        public static int NewBF_Idx()
        {
            //Generate an index for locating IV and Key in the binary file
            //We generated 100000 chunk of (48 bytes initVector, key)
            var idx = RandomHelper.SimpleRandom.Next(100000);
            return idx;
        }

    }
}