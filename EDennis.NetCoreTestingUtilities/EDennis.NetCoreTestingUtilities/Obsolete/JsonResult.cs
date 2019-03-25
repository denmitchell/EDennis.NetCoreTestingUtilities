using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EDennis.NetCoreTestingUtilities.Json {

    /// <summary>
    /// Holds JSON from a SQL Server FOR JSON query
    /// </summary>
    [Obsolete]
    public class JsonResult {

        [Key]
        public string Json { get; set; }
    }
}
