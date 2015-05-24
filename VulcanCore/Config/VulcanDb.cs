using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Vulcan.Core.Config
{
    public class VulcanDb
    {
        private Dictionary<string, DbTable> tables = new Dictionary<string, DbTable>();

        public DbTable CreateTable(string name)
        {
            tables[name] = new DbTable();
            return tables[name];
        }

        public DbTable SelectTable(string table)
        {
            return tables[table];
        }

        public void EncodeInto(Stream stream)
        {
            lock (tables)
            {
                using (BinaryWriter bw = new BinaryWriter(stream))
                {
                    bw.Write(tables.Count);

                    foreach (string table in tables.Keys)
                    {
                        bw.Write(table);
                        tables[table].EncodeInto(bw);
                    }
                }
            }
        }

        public void DecodeInto(Stream stream)
        {
            lock(tables)
            {
                using (BinaryReader br = new BinaryReader(stream))
                {
                    int c = br.ReadInt32();

                    for(int i = 0; i < c; i++)
                    {
                        string name = br.ReadString();
                        DbTable table = this.CreateTable(name);
                        table.Decode(br);
                    }
                }
            }
        }
    }
}
