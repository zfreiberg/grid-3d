using System;

[Serializable]
public class MapSettings
{
    public int act              = 1;
    public int difficulty       = 1;    // 1=Normal  2=Hard  3=Expert
    public int seed             = 0;    // 0 = generate a fresh random seed
    public int battleColumns    = 13;   // non-boss columns; total = battleColumns + 1
    public int maxNodesPerCol   = 3;
    public int totalActs        = 3;
}
