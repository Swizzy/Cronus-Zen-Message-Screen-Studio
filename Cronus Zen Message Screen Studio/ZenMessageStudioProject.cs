using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CronusZenMessageScreenStudio
{
    public class ZenMessageStudioProject
    {
        public bool[,] PixelData { get; }

        private ZenMessageStudioProject()
        {
            PixelData = new bool[128, 64];
        }

        public ZenMessageStudioProject(List<PixelControl> pixelsList) : this()
        {
            for (int x = 0; x < 128; x++)
            {
                for (int y = 0; y < 64; y++)
                {
                    PixelData[x, y] = pixelsList.SingleOrDefault(p => p.X == x && p.Y == y)?.Color == true;
                }
            }
        }

        public void SaveToFile(string path)
        {
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite))
            {
                fs.WriteByte((byte)'Z');
                fs.WriteByte((byte)'M');
                fs.WriteByte((byte)'S');
                fs.WriteByte((byte)'P');
                fs.WriteByte(1);
                int data = 0;
                int bitCounter = 0;
                for (int y = 0; y < 64; y++)
                {
                    for (int x = 0; x < 128; x++)
                    {
                        if (PixelData[x, y])
                        {
                            data |= 1;
                        }
                        bitCounter++;
                        if (bitCounter == 8)
                        {
                            fs.WriteByte((byte)(data & 0xFF));
                            bitCounter = 0;
                        }
                        data <<= 1;
                    }
                }
                fs.Flush();
            }
        }

        public static ZenMessageStudioProject ReadFromFile(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                using (var br = new BinaryReader(fs))
                {
                    // Verify header
                    if (br.ReadByte() == (byte) 'Z'
                        &&
                        br.ReadByte() == (byte) 'M'
                        &&
                        br.ReadByte() == (byte) 'S'
                        &&
                        br.ReadByte() == (byte) 'P')
                    {
                        switch (br.ReadByte())
                        {
                            case 1:
                                return ReadV1(br);
                        }
                    }
                }
            }
            return null;
        }

        private static ZenMessageStudioProject ReadV1(BinaryReader br)
        {
            var toReturn = new ZenMessageStudioProject();
            int data = 0;
            int bitCounter = 0;
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 128; x++)
                {
                    if (bitCounter == 0)
                    {
                        data = br.ReadByte();
                        bitCounter = 8;
                    }
                    bitCounter--;
                    toReturn.PixelData[x, y] = (data & (1 << bitCounter)) != 0;
                }
            }
            return toReturn;
        }
    }
}