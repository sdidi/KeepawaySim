using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Keepaway
{
    public interface PolicyInterface
    {
        /// <summary>
        /// Choose an action.
        /// </summary>
        /// 
        /// <param name="actionEstimates">Action estimates.</param>
        /// 
        /// <returns>Returns selected action.</returns>
        /// 
        /// <remarks>The method chooses an action depending on the provided estimates. The
        /// estimates can be any sort of estimate, which values usefulness of the action
        /// (expected summary reward, discounted reward, etc).</remarks>
        /// 
        int ChooseAction(double[] actionEstimates); // Sabre adjusted it
       
    }
}
