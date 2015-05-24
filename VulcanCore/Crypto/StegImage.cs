using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;

namespace Vulcan.Core.Crypto
{
    public class StegImage
    {

        public byte[] Contents
        {
            private set;
            get;
        }

        public StegImage(string name)
        {
            decode(name);
        }

        private void decode(string image)
        {
            int ptr = 0;

            byte[] contents = new byte[1024 * 1024];

            using (FileStream fs = new FileStream(image, FileMode.Open))
            {
                Bitmap bmp = new Bitmap(Bitmap.FromStream(fs));

                for (int x = 0; x < bmp.Width; x++)
                {
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        Color c = bmp.GetPixel(x, y);
                        byte r = (byte)(c.R & 3);
                        byte g = (byte)(c.G & 7);
                        byte b = (byte)(c.B & 7);

                        byte dat = (byte)(r | (g << 2) | (b << 5));
                        contents[ptr++] = dat;
                    }
                }

            }

            ptr = 0;

            using (BinaryReader br = new BinaryReader(new MemoryStream(contents)))
            {
                short length = br.ReadInt16();
                byte[] data = br.ReadBytes(length);
                this.Contents = new byte[length];
                for (ptr = 0; ptr < length; ptr++)
                {
                    this.Contents[ptr] = data[ptr];
                }
            }
        }
    }
}
