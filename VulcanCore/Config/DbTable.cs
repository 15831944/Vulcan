using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Vulcan.Core.Config
{
    public class DbTable
    {
        private Dictionary<string, DbType> columns = new Dictionary<string, DbType>();
        private List<DbRow> rows = new List<DbRow>();

        public void AddColumn(string name, DbType type)
        {
            columns[name] = type;
        }


        public DbType this[string key]
        {
            get
            {
                return columns[key];
            }
        }

        public DbRow InsertRow()
        {
            DbRow row = new DbRow(this);
            this.rows.Add(row);
            return row;
        }

        public IList<DbRow> Query(DbQuery query)
        {
            IList<DbRow> res = new List<DbRow>();
            lock(this.rows)
            {
                foreach(DbRow row in this.rows)
                {
                    if (query.Match(row))
                        res.Add(row);
                }
            }
            return res;
        }

        public void Truncate()
        {
            this.rows.Clear();
        }

        public void EncodeInto(BinaryWriter bw)
        {
            bw.Write(columns.Count);

            foreach(string name in columns.Keys)
            {
                bw.Write(name);
                bw.Write((byte)columns[name]);
            }

            lock (rows)
            {
                bw.Write(rows.Count);

                foreach (DbRow row in this.rows)
                {
                    row.EncodeInto(bw);
                }
            }
        }

        public void Decode(BinaryReader br)
        {
            int c = br.ReadInt32();

            for(int i = 0; i < c; i++)
            {
                string name = br.ReadString();
                DbType type = (DbType)br.ReadByte();
                this.AddColumn(name, type);
            }

            lock(rows)
            {
                c = br.ReadInt32();
                for(int i = 0; i < c; i++)
                {
                    DbRow row = this.InsertRow();
                    row.DecodeInto(br);
                }
            }
        }
    }
}
