using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVTest
{
    class AVEntryTreeNode
    {
        private AVEntry data;
        public AVEntryTreeNode lNode;
        public AVEntryTreeNode rNode;
        public byte height;
        public AVEntryTreeNode(AVEntry entry, byte height = 0) {
            data = entry; lNode = null; rNode = null; this.height = height;
        }
        public byte[] getKey()
        {
            return data.getStartBits();
        }

        public byte[] getData()
        {
            byte[] resData = new byte[AVEntry.ENTRY_LEN];
            data.getByteTitle().CopyTo(resData, 0);             // Максимальная длина имени - 20 символов 
            data.getSignatureOffset().CopyTo(resData, 20);      // Информация о диапазоне занимает 2 байта
            data.getSignatureInfo().CopyTo(resData, 22);        // Записываем информацию о сигнатуре, 25 байт
            data.getByteType().CopyTo(resData, 47);             // Информация о типах - 5 байт
            return resData;
        }

        public string getInfo()
        {
            string info = "";
            info += "Title: " + data.getTitle() + "\n";
            info += "Offset: (" + data.getSignatureOffset()[0] + ";" + data.getSignatureOffset()[1] + ")\n";
            info += "Hash: " + data.getHashText() + "\n";
            info += "Type: " + data.getPossibleType() + "\n";
            return info;
        }

        public string getName()
        {
            return data.getTitle() + ":" + Encoding.ASCII.GetString(getKey());
        }

        public byte[] getOffset()
        {
            return data.getSignatureOffset();
        }

        public byte getSignatureLength()
        {
            return data.getSignatureInfo()[0];
        }

        public byte[] getHash()
        {
            return data.getHash();
        }
    }

    class AVEntryTree
    {
        public AVEntryTreeNode root;
        public int size;
        public AVEntryTree(){ root = null; size = 0; }
        public AVEntryTree(AVEntry root)
        {
            this.root = new AVEntryTreeNode(root);
            size = 1;
        }

        public List<string> getAllNames(AVEntryTreeNode entry)
        {
            List<string> names = new List<string>();
            List<string> namesLeft, namesRight;
            if (entry == null) return null;
            names.Add(entry.getName());
            if ((namesLeft = getAllNames(entry.lNode)) != null) names.AddRange(namesLeft);
            if ((namesRight = getAllNames(entry.rNode)) != null) names.AddRange(namesRight);
            return names;
        }

        private void fixHeight(AVEntryTreeNode node)
        {
            int lHeight = (node.lNode == null) ? 0 : node.lNode.height;
            int rHeight = (node.rNode == null) ? 0 : node.rNode.height;
            node.height = (byte)((lHeight > rHeight ? lHeight : rHeight)+1);
        }

        private AVEntryTreeNode rotateRight(AVEntryTreeNode p)
        {
            AVEntryTreeNode q = p.lNode;
            p.lNode = q.rNode;
            q.rNode = p;
            fixHeight(p);
            fixHeight(q);
            return q;
        }

        private AVEntryTreeNode rotateLeft(AVEntryTreeNode p)
        {
            AVEntryTreeNode q = p.rNode;
            p.rNode = q.lNode;
            q.lNode = p;
            fixHeight(p);
            fixHeight(q);
            return q;
        }

        private int heightDiff (AVEntryTreeNode node)
        {
            if (node.lNode == null && node.rNode == null) return 0;
            if (node.lNode == null) return 0 - node.rNode.height;
            else if (node.rNode == null) return node.lNode.height;
            else return node.lNode.height - node.rNode.height;
        }

        private int keyComp(byte[] k1, byte[] k2)
        {
            for (int i = 0; i < AVEntry.WINDOW; i++)
            {
                if (k2[i] > k1[i]) return 1;
                else if (k1[i] > k2[i]) return -1;
            }
            return 0;
        }

        private AVEntryTreeNode treeBalance(AVEntryTreeNode node)
        {
            fixHeight(node);
            if (heightDiff(node) == 2)
            {
                if (node.lNode != null && heightDiff(node.lNode) < 0)
                    node.lNode = rotateLeft(node.lNode);
                node = rotateRight(node);

            }
            if (heightDiff(node) == -2)
            {
                if (node.rNode != null && heightDiff(node.rNode) > 0)
                    node.rNode = rotateRight(node.rNode);
                node = rotateLeft(node);
                
            }
            return node;
        }

        private AVEntryTreeNode insertNode(AVEntryTreeNode node, AVEntry entry)
        {

            if (node == null) { size++; return new AVEntryTreeNode(entry); }
            if (keyComp(node.getKey(), entry.getStartBits()) == 1) // entry > node
                node.rNode = insertNode(node.rNode, entry);
            else
                node.lNode = insertNode(node.lNode, entry);
            node = treeBalance(node);
            return node;
        }

        public void insertNode(AVEntry entry)
        {
            if (root == null) { root = new AVEntryTreeNode(entry); size++; return; }
            if (keyComp(root.getKey(), entry.getStartBits()) == 1) // entry > root
                root.rNode = insertNode(root.rNode, entry);
            else
                root.lNode = insertNode(root.lNode, entry);
            root = treeBalance(root);
        }

        private AVEntryTreeNode findNode(byte[] key, AVEntryTreeNode node)
        {
            int res = keyComp(node.getKey(), key);
            if (res == 0) { return node; }
            else if (res == 1 && node.rNode != null) { return findNode(key, node.rNode); }
            else if (res == -1 && node.lNode != null) { return findNode(key, node.lNode); }
            else return null;
        }

        public AVEntryTreeNode findNode(string key)
        {
            byte[] bKey = Encoding.ASCII.GetBytes(key);
            int res = keyComp(root.getKey(), bKey);
            if (res == 0) { return root; }
            else if (res == 1 && root.rNode != null) { return findNode(bKey, root.rNode); }
            else if (res == -1 && root.lNode != null) { return findNode(bKey, root.lNode); }
            else return null;
        }
    }
}
