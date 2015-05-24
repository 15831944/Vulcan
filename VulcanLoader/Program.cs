using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Security.Cryptography;

namespace Vulcan
{
    public class Program
    {

        private static readonly byte[] aesKey = new byte[] { 0x3d, 0x70, 0x47, 0x5e, 0x07, 0x6e, 0x4b, 0x6f, 0xec, 0x66, 0xea, 
            0x11, 0x07, 0x1a, 0xa9, 0xc7, 0x54, 0x30, 0x5c, 0x4d, 0xca, 0x62, 0x75, 0xa2, 0x71, 0x58, 0x91, 0x8f, 0x2b,
            0x68, 0xb6, 0x36 };

        private static readonly byte[] aesIV = new byte[]{0xba, 0xa2, 0x2e, 0xdf, 0x2d, 0x5b, 0xe0, 0x55, 0x5d, 0xc2, 0x51,
            0xa5, 0x96, 0x20, 0xa3, 0x04};

        static void Main(string[] args)
        {
            Assembly coreAssembly = null;

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            using (MemoryStream ms = new MemoryStream())
            {

                DecryptCore(ms);
                byte[] dat = ms.ToArray();
                coreAssembly = Assembly.Load(ms.ToArray());
            }

            foreach (Type t in coreAssembly.GetTypes())
            {
                if (t.GetInterface(typeof(IVulcanEntry).FullName) != null)
                {
                    IVulcanEntry entry = (IVulcanEntry)t.GetConstructor(new Type[] {  }).Invoke(new object[] { });
                    entry.VMain(args);
                }
            }

        }

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Console.WriteLine("Resolve {0}", args.Name);
            foreach (Assembly anAssembly in AppDomain.CurrentDomain.GetAssemblies())
                if (anAssembly.FullName == args.Name)
                    return anAssembly;
            return null;
        }


        static void DecryptCore(Stream decrypt)
        {
            Assembly curAsm = Assembly.GetExecutingAssembly();

            if (File.Exists(Path.GetDirectoryName(curAsm.Location) + "\\.VLCN"))
            {
                try
                {
                    using (Stream stm = File.Open(Path.GetDirectoryName(curAsm.Location) + "\\.VLCN", FileMode.Open))
                    {
                        DecryptAssembly(stm, decrypt);
                    }
                    return;
                }
                catch
                {

                }
            }
            
            using (Stream stm = curAsm.GetManifestResourceStream("Vulcan.VulcanCore.dll.core"))
            {
                DecryptAssembly(stm, decrypt);
            }
            
        }

        private static void DecryptAssembly(Stream instream, Stream ostream)
        {
            using (AesManaged decrypt = new AesManaged())
            {
                decrypt.IV = aesIV;
                decrypt.Key = aesKey;
                decrypt.Padding = PaddingMode.None;

                using (CryptoStream cs = new CryptoStream(instream, decrypt.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    decrypt.KeySize = 256;
                    decrypt.BlockSize = 128;
                    int dat = 0;

                    while ((dat = cs.ReadByte()) != -1)
                    {
                        ostream.WriteByte((byte)dat);
                    }
                }

            }
        }

    }
}
