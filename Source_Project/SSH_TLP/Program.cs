using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SSH_TLP
{
    class Program
    {
        #region OneBlockDES
        public static byte[] DesEncryptOneBlock(byte[] plainText, byte[] key)
        {
            //Implementation of DES algorithm for one block
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            des.KeySize = 64;
            des.Key = key;
            des.Mode = CipherMode.ECB;
            des.Padding = PaddingMode.None;

            ICryptoTransform ic = des.CreateEncryptor();

            byte[] enc = ic.TransformFinalBlock(plainText, 0, 8);

            return enc;
        }
        #endregion

        #region CreateByteArray
        //create and initialize array of bytes
        public static byte[] CreateByteArray(int length)
        {
            var arr = new byte[length];
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = 0x20;
            }
            return arr;
        }
        #endregion

        #region PrintArrayOfBytes
        public static void PrintArrayOfBytes(byte[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                Console.Write(array[i] + " ");
            }

            Console.Write("\n");
        }
        #endregion
        static void Main(string[] args)
        {
            int pktl = 0;
            int pdl = 0;
            byte[] keyE = CreateByteArray(8);
            byte[] IV = CreateByteArray(8); ;
            string keyDES;

            //Initialization of IV
            RNGCryptoServiceProvider numIV = new RNGCryptoServiceProvider();
            numIV.GetBytes(IV);

            #region Input values
            /***************************************************************************/
            Console.WriteLine("\nType in a plain text: \n");
            string inputString = Console.ReadLine();
            /***************************************************************************/
            do
            {
                Console.WriteLine("\nType in a key for DES(strictly 8 characters): \n");
                keyDES = Console.ReadLine();
            }
            while (keyDES.Length != 8);

            for (int i = 0; i < 8; i++)
            {
                keyE[i] = (byte)keyDES[i];
            }
            /***************************************************************************/
            Console.WriteLine("\nType in a key for MAC(arbitrary number of characters): \n");
            string keyForMac = Console.ReadLine();

            byte[] keyMAC = CreateByteArray(keyForMac.Length);

            for (int i = 0; i < keyForMac.Length; i++)
            {
                keyMAC[i] = (byte)keyForMac[i];
            }
            /***************************************************************************/
            #endregion

            //padding
            pktl = inputString.Length;

            if ((pktl + 2) % 8 != 0)
            {
                pdl = 8 - ((pktl + 2) % 8);
            }

            int finalLength = pktl + pdl + 2;
            byte[] random = new byte[pdl];

            RNGCryptoServiceProvider num = new RNGCryptoServiceProvider();
            num.GetBytes(random);

            byte[] payload = CreateByteArray(finalLength);
            byte[] finalString = CreateByteArray(finalLength);

            payload[0] = (byte)pktl;
            payload[1] = (byte)pdl;
            int k = 0;

            for (int i = 2; i < finalLength; i++)
            {
                if (i < finalLength - pdl)
                {
                    payload[i] = (byte)inputString[i - 2];
                }
                else
                {
                    payload[i] = random[k];
                    k++;
                }

            }

            Console.WriteLine("The final array of bytes for encoding: ");
            PrintArrayOfBytes(payload);
            /***************************************************************************/

            #region DES
            //Implementation of CBC mode by using DES function for one block
            byte[] helpString = CreateByteArray(8);

            for (int i = 0; i < finalLength; i += 8)
            {
                for(int j = 0; j < 8; j++)
                {
                    helpString[j] = (byte)(payload[i + j] ^ IV[j]);
                }

                IV = DesEncryptOneBlock(helpString, keyE);


                for (int j = 0; j < 8; j++)
                {
                    finalString[i + j] = IV[j];
                }                

            }

            Console.WriteLine("Coded bytes: ");
            PrintArrayOfBytes(finalString);
            #endregion

            #region HMAC
            HMACSHA1 hmac = new HMACSHA1(keyMAC);
            hmac.Initialize();

            Console.WriteLine("HMAC value: " + BitConverter.ToString(hmac.ComputeHash(payload)).Replace("-", "").ToLower());

            #endregion

            Console.ReadLine();
        }
    }
}
