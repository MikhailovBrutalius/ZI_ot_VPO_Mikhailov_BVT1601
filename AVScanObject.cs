using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.MemoryMappedFiles;
namespace AVTest
{
    class AVScanObject
    {
        private MemoryMappedFile mappedFile;
        private MemoryMappedViewAccessor fileAccessor;
        private string path;
        public AVScanObject(MemoryMappedViewAccessor accessor, MemoryMappedFile mmFile, string path)
        {
            mappedFile = mmFile;
            fileAccessor = accessor;
            this.path = path;
        }

        public long Length { get { return fileAccessor.Capacity; } }


        public string getPath()
        {
            return path;
        }

        public void Close()
        {
            if (fileAccessor != null) fileAccessor.Flush();
        }

        public byte[] GetBytes(int offset, int len)
        {
            byte[] buffer = new byte[len];
            fileAccessor.ReadArray(offset, buffer, 0, len);
            string bufferStr = Encoding.ASCII.GetString(buffer);
            buffer = Encoding.ASCII.GetBytes(bufferStr);
            return buffer;
        }
    }
}
