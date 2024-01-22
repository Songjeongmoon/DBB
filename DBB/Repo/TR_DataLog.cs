using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBB.Repo
{
    class TR_DataLog
    {
        public int LogNo { get; set; }

        public DateTime LogDT { get; set; }

        public int AmrId { get; set; }

        public string Cmd { get; set; }

        public int TRMode { get; set; }

        public int Mode { get; set; }

        public int MissionId { get; set; }

        public int JobId { get; set; }

        public byte Succ { get; set; }

        public string Data { get; set; }
    }
}
