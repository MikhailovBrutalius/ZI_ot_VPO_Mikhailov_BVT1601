using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace AVTest
{
    class AVLoader
    {
        static string dbName = "AVBase";
        static byte[] name = Encoding.ASCII.GetBytes("DBTest1");

        static public void initDB()
        {
            if (File.Exists(dbName)) return;
            FileStream db = File.Create(dbName);
            db.Write(name, 0, name.Length);
            db.WriteByte(0);
            db.Close();
        }

        static private void writeEntry(AVEntryTreeNode entry, FileStream db)
        {
            if (entry == null) return;
            db.Write(entry.getData(), 0, AVEntry.ENTRY_LEN);
            writeEntry(entry.lNode, db);
            writeEntry(entry.rNode, db);
        }

        static public void saveEntries(AVEntryTree entries)
        {
            FileStream db = File.Open(dbName, FileMode.Open);
            db.Seek(7, SeekOrigin.Begin);
            db.WriteByte((byte)entries.size);
            writeEntry(entries.root, db);
            db.Close();
        }

        static public AVEntryTree loadEntries()
        {
            AVEntryTree resList = new AVEntryTree();
            FileStream db = File.Open(dbName, FileMode.Open);
            byte[] bytes = new byte[7];
            db.Read(bytes, 0, 7);
            if (Encoding.ASCII.GetString(bytes) == Encoding.ASCII.GetString(name))
            {
                int nEntries = db.ReadByte();
                byte[] virusName = new byte[20];
                byte[] signature = new byte[8];
                byte[] hash = new byte[16];
                byte[] type = new byte[5];
                for (int i = 0; i < nEntries; i++)
                {
                    db.Read(virusName, 0, 20);
                    AVEntry entry = new AVEntry(Encoding.ASCII.GetString(virusName));
                    int lOffset = db.ReadByte();
                    int rOffset = db.ReadByte();
                    int length = db.ReadByte();
                    db.Read(signature, 0, 8);
                    db.Read(hash, 0, 16);
                    db.Read(type, 0, 5);
                    entry.setSignatureInfo(lOffset, rOffset, length, signature, hash, Encoding.ASCII.GetString(type));
                    resList.insertNode(entry);
                }
                db.Close();
                return resList;
            }
            db.Close();
            return null;
        }

    }
}
