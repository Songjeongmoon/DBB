using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBB.Repo
{
    class TR_ChangeLog
    {
        int LogNo { get; set; }

        DateTime LogDT { get; set; }

        int Type { get; set; }

        string Id { get; set; }

        string Data { get; set; }
    }
}
