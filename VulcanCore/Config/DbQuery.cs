using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vulcan.Core.Config
{
    public class DbQuery : IDbQuery
    {
        class DbFilter
        {
            public string Key
            {
                private set;
                get;
            }

            public object Value
            {
                private set;
                get;
            }

            public DbFilter(string key, object val)
            {
                this.Key = key;
                this.Value = val;
            }
        }


        private List<DbFilter> filters = new List<DbFilter>();

        public void AddFilter(string key, object val)
        {
            filters.Add(new DbFilter(key, val));
        }

        public bool Match(DbRow row)
        {
            bool res = true;
            foreach(DbFilter filter in this.filters)
            {
                if (row[filter.Key].ToString() != filter.Value.ToString())
                    res = false;
            }
            return res;
        }
    }
}
