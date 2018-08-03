using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcpForwarderClient
{

    public interface IDataAnalyzer
    {
        byte[] Analyze(byte[] input, string timeAsString, string direction);
    }

    
}
