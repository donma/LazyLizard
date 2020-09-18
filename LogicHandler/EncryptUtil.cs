using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace LazyLizard.LogicHandler
{
    public static class EncryptUtil
    {
        #region RSA
        public static string EncryptDataRSA(string data, string publicKey, int encryptionBufferSize = 117, int decryptionBufferSize = 128)
        {
            var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(publicKey);
            byte[] dataEncoded = Encoding.UTF8.GetBytes(data);
            using (var ms = new MemoryStream())
            {
                var buffer = new byte[encryptionBufferSize];
                int pos = 0;
                int copyLength = buffer.Length;
                while (true)
                {
                    if (pos + copyLength > dataEncoded.Length)
                    {
                        copyLength = dataEncoded.Length - pos;
                    }
                    buffer = new byte[copyLength];
                    Array.Copy(dataEncoded, pos, buffer, 0, copyLength);
                    pos += copyLength;
                    ms.Write(rsa.Encrypt(buffer, false), 0, decryptionBufferSize);
                    Array.Clear(buffer, 0, copyLength);
                    if (pos >= dataEncoded.Length)
                    {
                        break;
                    }

                }
                //var res = Convert.ToBase64String(ms.ToArray());
                var res = Base32.ToBase32String(ms.ToArray());
                return res;
            }
        }

        public static string DecryptDataRSA(string encryptContent, string privateKey, int decryptionBufferSize = 128)
        {

            try
            {

                //var data = Convert.FromBase64String(encryptContent);
                var data = Base32.FromBase32String(encryptContent);
                var rsa = new RSACryptoServiceProvider();
                rsa.FromXmlString(privateKey);
                using (var ms = new MemoryStream(data.Length))
                {
                    byte[] buffer = new byte[decryptionBufferSize];
                    int pos = 0;
                    int copyLength = buffer.Length;

                    while (true)
                    {
                        Array.Copy(data, pos, buffer, 0, copyLength);
                        pos += copyLength;
                        byte[] resp = rsa.Decrypt(buffer, false);
                        ms.Write(resp, 0, resp.Length);
                        Array.Clear(resp, 0, resp.Length);
                        Array.Clear(buffer, 0, copyLength);
                        if (pos >= data.Length)
                        {
                            break;
                        }
                    }
                    return Encoding.UTF8.GetString(ms.ToArray());

                }

            }
            catch (CryptographicException ce)
            {
                throw ce;
            }

        }

        #endregion

        #region AES

        public static string DescAesString(this string str, string pass = "abcdefg_abcdefg_abcdefg_abcdefg_")
        {
            // var bytes = Convert.FromBase64String(str);
            var bytes = Base32.FromBase32String(str);
            var resS = AesDecryptor(bytes, pass);
            return Encoding.UTF8.GetString(resS);
        }

        public static string EncAesString(this string str, string pass = "abcdefg_abcdefg_abcdefg_abcdefg_")
        {
            var bytes = AesEncryptor(Encoding.UTF8.GetBytes(str), pass);
            // return Convert.ToBase64String(bytes);
            return Base32.ToBase32String(bytes);
        }

        public static byte[] AesEncryptor(this byte[] bsFile, string pass = "abcdefg_abcdefg_abcdefg_abcdefg_")
        {
            RijndaelManaged aes = new RijndaelManaged();

            aes.KeySize = 256;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = Encoding.UTF8.GetBytes(pass);
            aes.IV = Encoding.UTF8.GetBytes(pass.Substring(8, 16));

            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            ICryptoTransform transform = aes.CreateEncryptor();
            return transform.TransformFinalBlock(bsFile, 0, bsFile.Length);
        }

        public static byte[] AesDecryptor(this byte[] bsFile, string pass = "abcdefg_abcdefg_abcdefg_abcdefg_")
        {
            RijndaelManaged aes = new RijndaelManaged();
            aes.KeySize = 256;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = Encoding.UTF8.GetBytes(pass);
            aes.IV = Encoding.UTF8.GetBytes(pass.Substring(8, 16));
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            ICryptoTransform transform = aes.CreateDecryptor();
            return transform.TransformFinalBlock(bsFile, 0, bsFile.Length);
        }



        #endregion

        #region 3DES

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="pass">32 chars</param>
        /// <returns></returns>
        public static string Enc3DesString(this string str, string pass = "abcdefg_abcdefg_abcdefg_abcdefg_")
        {
            byte[] keyArray;
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(str);


            keyArray = new MD5CryptoServiceProvider().ComputeHash(UTF8Encoding.UTF8.GetBytes(pass));

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();



            tdes.Key = keyArray;

            tdes.Mode = CipherMode.ECB;

            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateEncryptor();

            byte[] resultArray =
              cTransform.TransformFinalBlock(toEncryptArray, 0,
              toEncryptArray.Length);
            tdes.Clear();
            // return Convert.ToBase64String(resultArray, 0, resultArray.Length);

            return Base32.ToBase32String(resultArray);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eStr"></param>
        /// <param name="pass">32 chars</param>
        /// <returns></returns>
        public static string Desc3DesString(this string eStr, string pass = "abcdefg_abcdefg_abcdefg_abcdefg_")
        {
            byte[] keyArray;
            //get the byte code of the string

            //byte[] toEncryptArray = Convert.FromBase64String(eStr);
            byte[] toEncryptArray = Base32.FromBase32String(eStr);

            var hashmd5 = new MD5CryptoServiceProvider();
            keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(pass));

            hashmd5.Clear();

            var tdes = new TripleDESCryptoServiceProvider();

            tdes.Key = keyArray;
            tdes.Mode = CipherMode.ECB;

            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(
                                 toEncryptArray, 0, toEncryptArray.Length);

            tdes.Clear();

            return UTF8Encoding.UTF8.GetString(resultArray);

        }

        #endregion


        #region Hash



        public static string HashToSHA256(this string str)
        {
            SHA256 sha256 = new SHA256CryptoServiceProvider();//建立一個SHA256
            byte[] source = Encoding.Default.GetBytes(str);//將字串轉為Byte[]
            byte[] crypto = sha256.ComputeHash(source);//進行SHA256加密
            //string result = Convert.ToBase64String(crypto);//把加密後的字串從Byte[]轉為字串
            string result = Base32.ToBase32String(crypto);
            return result;


        }

        public static string HashToSHA384(this string str)
        {
            SHA384 sha384 = new SHA384CryptoServiceProvider();

            //string resultSha384 = Convert.ToBase64String(sha384.ComputeHash(Encoding.Default.GetBytes(str)));
            string resultSha384 = Base32.ToBase32String(sha384.ComputeHash(Encoding.Default.GetBytes(str)));
            return resultSha384;

        }

        public static string GetMD5Hash(this string input, string salt = "no2don")
        {
            var x = new MD5CryptoServiceProvider();
            byte[] bs = Encoding.UTF8.GetBytes(input + salt);
            bs = x.ComputeHash(bs);
            var s = new System.Text.StringBuilder();
            foreach (byte b in bs)
            {
                s.Append(b.ToString("x2").ToLower());
            }
            string password = s.ToString();
            return password;
        }

        #endregion
    }
}
