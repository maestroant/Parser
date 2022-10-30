using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    // 350345705137862!Model: iPhone XS 64GB Space Gray[A1920] <br>IMEI1: 356168094031825 <br>IMEI2: 356168094199069 <br>Serial Number: C39XK9DFKPFP<br> FMI: OFF<br> Current GSMA Status: CLEAN<br> Carrier: Unlocked<br> SimLock Status: Unlocked
    internal class Order
    {
        public string OrderIMEI { get; set; }
        public string Model { get; set; }
        public string IMEI1 { get; set; }
        public string IMEI2 { get; set; }
        public string SerialNumber { get; set; }
        public string FMI { get; set; }
        public string CurrentGSMAStatus { get; set; }
        public string Carrier { get; set; }
        public string SimLock { get; set; }

    }
}
