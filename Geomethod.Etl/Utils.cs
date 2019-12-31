using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geomethod.Etl
{
    public class GeomethodEtlException : Exception
    {
        public GeomethodEtlException() : base() { }
        public GeomethodEtlException(string message) : base(message) { }
        public GeomethodEtlException(string message, Exception innerException) : base(message, innerException) { }
    }

}
