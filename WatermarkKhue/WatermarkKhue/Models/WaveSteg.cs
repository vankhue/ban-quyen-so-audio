using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace WatermarkKhue.Models
{
    public class WaveSteg
    {
        private byte[] riffID; // "riff"
        private uint size;  // Size
        private byte[] wavID;  // Format
        private byte[] fmtID;  // Subchunk1ID
        private uint fmtSize; // Subchunk size
        private ushort format; // format
        private ushort channels; // no of channels
        private uint sampleRate; // Samplerate
        private uint bytePerSec;
        private ushort blockSize;
        private ushort bit;
        private byte[] dataID;// "data"
        private uint dataSize;
        private List<short> leftStream;
        private List<short> rightStream;

        private FileStream fs;
        private BinaryReader br;

        //Khởi tạo đối tượng WaveSteg với file âm thanh 
        public WaveSteg(FileStream filepath)
        {
            this.fs = filepath;
            this.br = new BinaryReader(fs);

            this.riffID = br.ReadBytes(4);
            this.size = br.ReadUInt32();
            this.wavID = br.ReadBytes(4);
            this.fmtID = br.ReadBytes(4);
            this.fmtSize = br.ReadUInt32();
            this.format = br.ReadUInt16();
            this.channels = br.ReadUInt16();
            this.sampleRate = br.ReadUInt32();
            this.bytePerSec = br.ReadUInt32();
            this.blockSize = br.ReadUInt16();
            this.bit = br.ReadUInt16();
            this.dataID = br.ReadBytes(4);
            this.dataSize = br.ReadUInt32();

            this.leftStream = new List<short>();
            this.rightStream = new List<short>();
            for (int i = 0; i < this.dataSize / this.blockSize; i++)
            {
                leftStream.Add((short)br.ReadUInt16());
                rightStream.Add((short)br.ReadUInt16());
            }

            br.Close();
            fs.Close();
        }

        // Nhận luồng âm thanh của kênh bên trái dưới dạng Danh sách các phần tử ngắn.
        public List<short> GetLeftStream()
        {
            return this.leftStream;
        }

        //Nhận luồng âm thanh của kênh bên phải dưới dạng Danh sách các phần tử ngắn.
        public List<short> GetRightStream()
        {
            return this.rightStream;
        }

        // Cập nhật luồng âm thanh trái và phải cho đối tượng WaveAudio này.
        public void UpdateStreams(List<short> leftStream, List<short> rightStream)
        {
            this.leftStream = leftStream;
            this.rightStream = rightStream;
        }

        // Lưu đối tượng WAV file vào 1 tệp
        public void WriteFile(string path)
        {
            this.dataSize = (uint)Math.Max(leftStream.Count, rightStream.Count) * 4;

            FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            BinaryWriter bw = new BinaryWriter(fs);

            bw.Write(this.riffID);
            bw.Write(this.size);
            bw.Write(this.wavID);
            bw.Write(this.fmtID);
            bw.Write(this.fmtSize);
            bw.Write(this.format);
            bw.Write(this.channels);
            bw.Write(this.sampleRate);
            bw.Write(this.bytePerSec);
            bw.Write(this.blockSize);
            bw.Write(this.bit);
            bw.Write(this.dataID);
            bw.Write(this.dataSize);

            for (int i = 0; i < this.dataSize / this.blockSize; i++)
            {
                if (i < this.leftStream.Count)
                    bw.Write((ushort)this.leftStream[i]);
                else
                    bw.Write(0);

                if (i < this.rightStream.Count)
                    bw.Write((ushort)this.rightStream[i]);
                else
                    bw.Write(0);
            }

            fs.Close();
            bw.Close();
        }
    }
}