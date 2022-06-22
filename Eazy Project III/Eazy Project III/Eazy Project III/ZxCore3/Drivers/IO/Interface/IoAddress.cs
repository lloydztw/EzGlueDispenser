/****************************************************************************
 *                                                                          
 * Copyright (c) 2013 LETIAN CHANG All rights reserved.        
 *                                                                          
 ***************************************************************************/

/****************************************************************************
 *
 * VERSION
 *		$Revision:$
 *
 * HISTORY
 *      $Id:$    
 *	        20130711 LeTian Chang : Creation         
 *
 * DESCRIPTION
 *      
 *
 ***************************************************************************/

namespace JetEazy.Drivers.IOCtrl
{
    /// <summary>
    /// IO 點位的定址 <br/>
    /// ( Designed by LeTian Chang 2013 ) <br/>
    /// </summary>   
    public interface IoAddress
    {
        string Name { get; }
        byte PlcID { get; }
        string Category { get; }
        ushort Address { get; }
        byte BitOffset { get; }
        byte Bits { get; }
    }
}
