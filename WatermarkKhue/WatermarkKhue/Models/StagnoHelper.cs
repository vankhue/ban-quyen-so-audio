using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WatermarkKhue.Models
{
    public class StagnoHelper
    {
        private WaveSteg file;

        //Khởi tạo StagnoHelper
        public StagnoHelper(WaveSteg file)
        {
            this.file = file;
        }

        //Gắn message vào file
        public void HideMessage(string message)
        {
            //Lưu luồng kênh âm thanh bộ nhớ cache cục bộ từ đối tượng WaveAudio
            List<short> leftStream = file.GetLeftStream();
            List<short> rightStream = file.GetRightStream();

            //Giấu tin
            byte[] bufferMessage = System.Text.Encoding.UTF8.GetBytes(message); //Đổi chuổi mess thành byte
            short tempBit;
            int bufferIndex = 0; //Đặt bộ đếm chỉ mục
            int bufferLength = bufferMessage.Length; //Lấy chiều dài của luồng mess
            int channelLength = leftStream.Count; //Lấy độ dài của luồng âm thanh, trái phải bằng nhau
            int storageBlock = (int)Math.Ceiling((double)bufferLength / (channelLength * 2)); //Lấy phạm vi khối lưu trữ dựa trên độ dài của luồng âm thanh và luồng mess

            //Lưu trữ thông tin độ dài tin nhắn trong phần tử đầu tiên của luồng trái và phải
            leftStream[0] = (short)(bufferLength / 32767); //Lưu trữ Định mức của kích thước thực trong phần tử đầu tiên của luồng âm thanh.
            rightStream[0] = (short)(bufferLength % 32767); //Lưu trữ Phần còn lại của kích thước thực trong phần tử đầu tiên của luồng âm thanh.
            for (int i = 1; i < leftStream.Count; i++) //Lặp lại độ dài của luồng kênh âm thanh, bỏ qua phần tử đầu tiên vì nó chứa độ dài tin nhắn, lưu trữ các bit tin nhắn vào các luồng âm thanh trái và phải.
            {
                if (i < leftStream.Count) 
                {
                    if (bufferIndex < bufferLength && i % 8 > 7 - storageBlock && i % 8 <= 7) //Điều kiện để nhắm mục tiêu các phần tử từ vị trí cuối cùng của mỗi khối âm thanh 8 bit (được tính toán dựa trên storageBlock).
                    {
                        tempBit = (short)bufferMessage[bufferIndex++]; //Lấy mess bit
                        leftStream.Insert(i, tempBit); //Thay thế bit âm thanh bằng mess bit
                    }
                }
                if (i < rightStream.Count)
                {
                    if (bufferIndex < bufferLength && i % 8 > 7 - storageBlock && i % 8 <= 7)
                    {
                        tempBit = (short)bufferMessage[bufferIndex++];
                        rightStream.Insert(i, tempBit);
                    }
                }
            }

            file.UpdateStreams(leftStream, rightStream); 
        }

        //Kiểm tra mess
        public string ExtractMessage()
        {
            //Lưu luồng kênh âm thanh bộ nhớ cache cục bộ từ đối tượng WaveAudio
            List<short> leftStream = file.GetLeftStream();
            List<short> rightStream = file.GetRightStream();

            //Extract
            int bufferIndex = 0; //Set message stream index counter.
            int messageLengthQuotient = leftStream[0]; //Lấy chiều dài luồng
            int messageLengthRemainder = rightStream[0]; //Lấy chiều dài phần còn lại
            int channelLength = leftStream.Count; //Lấy chiều dài kênh âm thanh

            int bufferLength = 32767 * messageLengthQuotient + messageLengthRemainder; //Tính chiều dài mess ban đầu
            int storageBlock = (int)Math.Ceiling((double)bufferLength / (channelLength * 2)); //Lấy lượng lưu trữ ban đầu từ chiều dài của audio và mess

            byte[] bufferMessage = new byte[bufferLength]; //Tạo số byte từ lượng mess thu được
            for (int i = 1; i < leftStream.Count; i++) //Lặp lại theo chiều dài của luồng kênh âm thanh.
            {
                if (bufferIndex < bufferLength && i % 8 > 7 - storageBlock && i % 8 <= 7) //Điều kiện để target các phần tử từ vị trí cuối cùng của mỗi khối âm thanh 8 bit (được tính toán dựa trên storageBlock).
                {
                    //Nhận bit mess trái và phải rồi lưu trữ
                    bufferMessage[bufferIndex++] = (byte)leftStream[i];
                    if (bufferIndex < bufferLength) //Kiểm tra xem bufferIndex có vượt quá tổng chiều dài không.
                        bufferMessage[bufferIndex++] = (byte)rightStream[i];
                }
            }

            return System.Text.Encoding.UTF8.GetString(bufferMessage);
        }
    }
}