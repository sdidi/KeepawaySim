using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Keepaway
{
    [Serializable]
    public enum NodeType 
    {
        Bias,
        Input,
        Hidden,
        Output,
    }
}
