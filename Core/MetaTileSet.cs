﻿using System;
using System.Drawing;
using System.IO;
using static MinishMaker.Core.RoomMetaData;

namespace MinishMaker.Core
{
    public class MetaTileSet
    {
        byte[] metaTileSetData;
        bool isBg1;

        public MetaTileSet(AddrData addrData, bool isBg1, string filePath)
        {
            metaTileSetData = Project.Instance.GetSavedData(filePath, true, addrData.size);

            if (metaTileSetData == null)
            {
                metaTileSetData = DataHelper.GetData(addrData);
            }

            this.isBg1 = isBg1;
        }

        public void DrawMetaTile(ref Bitmap b, Point p, TileSet tset, PaletteSet pset, int tileId, bool overwrite)
        {
            byte[] tileData = new byte[8];
            //write 8 bytes from tileId *8 to tileData
            Array.Copy(metaTileSetData, tileId << 3, tileData, 0, 8);
            try
            {
                DrawTileData(ref b, tileData, p, tset, pset.Palettes, this.isBg1, overwrite);
            }
            catch (ArgumentNullException e)
            {
                throw new ArgumentNullException("Attempt to draw empty metatile. Num: 0x" + tileId.ToString("X"), e);
            }

        }

        public static void DrawTileData(ref Bitmap b, byte[] tileData, Point p, TileSet tset, Color[][] palettes, bool isBg1, bool overwrite)
        {
            if (tileData.Length == 0)
                throw new ArgumentNullException("metaTileData", "Cannot draw empty metatile.");

            int i = 0;
            for (int y = 0; y < 2; y += 1)
            {
                for (int x = 0; x < 2; x += 1)
                {
                    UInt16 data = 0;
                    data = (ushort)(tileData[i] | (tileData[i + 1] << 8));
                    i += 2;

                    int tnum = data & 0x3FF; //bits 1-10

                    if (isBg1)
                        tnum += 0x200;

                    bool hflip = ((data >> 10) & 1) == 1;//is bit 11 set
                    bool vflip = ((data >> 11) & 1) == 1;//is bit 12 set
                    int pnum = data >> 12;//last 4 bits

                    tset.DrawQuarterTile(ref b, new Point(p.X + (x * 8), p.Y + (y * 8)), tnum, palettes[pnum], hflip, vflip, overwrite);
                }
            }
        }

        public byte[] GetTileInfo(int tileNum)
        {
            try
            {
                byte[] tileData = new byte[8];
                //write 8 bytes from tileId *8 to tileData
                Array.Copy(metaTileSetData, tileNum << 3, tileData, 0, 8);
                return tileData;
            }
            catch
            {
                return null;
            }
        }

        public void SetTileInfo(byte[] tileInfo, int tileNum)
        {
            if (tileInfo.Length != 8)
                return;

            var tileStart = tileNum << 3;
            metaTileSetData[tileStart] = tileInfo[0];
            metaTileSetData[tileStart + 1] = tileInfo[1];
            metaTileSetData[tileStart + 2] = tileInfo[2];
            metaTileSetData[tileStart + 3] = tileInfo[3];
            metaTileSetData[tileStart + 4] = tileInfo[4];
            metaTileSetData[tileStart + 5] = tileInfo[5];
            metaTileSetData[tileStart + 6] = tileInfo[6];
            metaTileSetData[tileStart + 7] = tileInfo[7];
        }

        public long GetCompressedMetaTileSet(ref byte[] outdata)
        {
            var compressed = new byte[metaTileSetData.Length];
            long totalSize = 0;
            MemoryStream ous = new MemoryStream(compressed);
            totalSize = DataHelper.Compress(metaTileSetData, ous, false);

            outdata = new byte[totalSize];
            Array.Copy(compressed, outdata, totalSize);

            totalSize |= 0x80000000;

            return totalSize;
        }
    }
}
