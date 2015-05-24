using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Vulcan.Core.Config
{
    public class DbRow
    {

        private Dictionary<string, object> values = new Dictionary<string, object>();
        private DbTable parent;

        public DbRow(DbTable table)
        {
            this.parent = table;
        }

        public object this[string key]
        {
            get
            {
                lock (values)
                {
                    if (values.ContainsKey(key))
                        return values[key];
                    return null;
                }
            }

            set
            {
                lock (values)
                {
                    values[key] = value;
                }
            }
        }
        
        public void EncodeInto(BinaryWriter bw)
        {
            bw.Write(values.Count);

            lock (values)
            {
                foreach (string key in values.Keys)
                {
                    bw.Write(key);
                    object obj = values[key];
                    switch (parent[key])
                    {
                        case DbType.Boolean:
                            bw.Write((bool)obj);
                            break;
                        case DbType.Integer:
                            bw.Write((int)obj);
                            break;
                        case DbType.String:
                            bw.Write(obj.ToString());
                            break;
                    }
                }
            }
        }

        public void DecodeInto(BinaryReader br)
        {
            int c = br.ReadInt32();

            lock(values)
            {
                for(int i = 0; i < c; i++)
                {
                    string key = br.ReadString();
                    DbType type = parent[key];
                    switch(type)
                    {
                        case DbType.Boolean:
                            this[key] = br.ReadBoolean();
                            break;
                        case DbType.Integer:
                            this[key] = br.ReadInt32();
                            break;
                        case DbType.String:
                            this[key] = br.ReadString();
                            break;
                    }
                }
            }
        }
    }
}
