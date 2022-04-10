using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Logger
{
    public interface ISovranLogger
    {
        void LogPayload(string payload);
        void LogActivity(string message);
        void LogError(string message);
    }
}
