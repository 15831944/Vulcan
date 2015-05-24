using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vulcan.Core.Config
{
    public interface IDbQuery
    {
        bool Match(DbRow row);
    }
}
