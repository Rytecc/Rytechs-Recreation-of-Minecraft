using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockTypeIndicator : MonoBehaviour
{
    [Tooltip
    (
        "0 = Air \n" +
        "1 = Grass\n" +
        "2 = Dirt\n" +
        "3 = Stone\n" +
        "4 = Bedrock\n" +
        "5 = Sand\n" +
        "6 = Oak_Log\n" +
        "7 = Oak_Leaves"
    )]
    public int blockID;
}
