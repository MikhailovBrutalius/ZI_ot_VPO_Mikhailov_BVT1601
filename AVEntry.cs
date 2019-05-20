using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
namespace AVTest
{
    
    class AVEntry
    {

        public static int WINDOW = 8; 
        public static int ENTRY_LEN = 52; 
        private string title; // Имя записи
        private string type;  // Типы файла
        // Свойства сигнатуры
        private int signLOffset;
        private int signROffset;
        private int signLength;
        private byte[] signStartBits;
        private byte[] signHash;   
        
        //
        // Функции-инициализаторы записи
        // 

        public AVEntry(string title)
        {
            this.title = title;
        }

        public void setSignatureInfo(int lOffset, int rOffset, string signature, string type)
        {
            signLOffset = lOffset; signROffset = rOffset;
            signLength = signature.Length;
            signStartBits = Encoding.ASCII.GetBytes(signature.Substring(0, WINDOW));
            signHash = makeHash(signature);
            this.type = type;
        }

        public void setSignatureInfo(int lOffset, int rOffset, int length, byte[] begin, byte[] hash, string type)
        {
            signLOffset = lOffset; signROffset = rOffset;
            signLength = length;
            signStartBits = new byte[begin.Length];
            begin.CopyTo(signStartBits,0);
            signHash = new byte[hash.Length];
            hash.CopyTo(signHash,0);
            this.type = type;
        }

        //
        // Геттеры
        //

        public string getTitle()
        {

            return title.TrimEnd('\0');
        }


        public byte[] getHash()
        {
            return signHash;
        }

        public string getPossibleType()
        {
            return type.TrimEnd('\0');
        }

        public byte[] getStartBits()
        {
            return signStartBits;
        }
        //
        // Функции, приводящие данные в вид, пригодный для файла базы данных
        //

        public byte[] getByteTitle()
        {
            byte[] bTitle = new byte[20];
            Encoding.ASCII.GetBytes(this.title).CopyTo(bTitle,0);
            for (int i = title.Length; i < 20; i++) bTitle[i] = (byte)'\0';
            return bTitle;
        }

        public byte[] getSignatureOffset()
        {
            byte[] result = new byte[2];
            result[0] = (byte)signLOffset;
            result[1] = (byte)signROffset;
            return result;
        }

        public byte[] getSignatureInfo()
        {
            byte[] info = new byte[25];
            info[0] = (byte)signLength;
            signStartBits.CopyTo(info, 1);
            signHash.CopyTo(info, 9);
            return info;
        }

        public byte[] getByteType()
        {
            byte[] bType = new byte[5];
            Encoding.ASCII.GetBytes(this.type).CopyTo(bType, 0);
            for (int i = type.Length; i < 5; i++) bType[i] = (byte)'\0';
            return bType;
        }

        //
        // Вспомогательные функции
        //

        private byte[] makeHash(string input)
        {
            MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(Encoding.ASCII.GetBytes(input));
            return hash;
        }

        public string getHashText()
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in signHash)
                sb.Append(b.ToString("X2"));
            return sb.ToString();
        }
    }
}
