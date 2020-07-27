using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models
{
    public enum DataType
    {
        IMPORTWHTOTAL = 1,
        IMPORTVARHTOTAL = 2,
        EXPORTWHTOTAL = 3,
        EXPORTVARHTOTAL = 4
    }

    public enum Units
    {
        KWH = 1,
        KVARH = 2
    }
}
