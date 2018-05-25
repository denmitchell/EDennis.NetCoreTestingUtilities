using System;
using System.Runtime.Serialization;

namespace EDennis.NetCoreTestingUtilities.Extensions {
    public class MissingRecordException : Exception {
        public MissingRecordException(string message) : base(message) { }
    }
}