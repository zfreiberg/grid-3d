using System;
using System.Collections.Generic;

[Serializable]
public class MapData
{
    public int            act;
    public int            seed;
    public int            totalColumns;
    public List<NodeData> nodes = new();

    public NodeData       GetNode(string id)  => nodes.Find(n => n.id == id);
    public List<NodeData> GetColumn(int col)  => nodes.FindAll(n => n.column == col);
    public List<NodeData> GetStartNodes()     => nodes.FindAll(n => n.column == 0);
    public NodeData       GetBossNode()       => nodes.Find(n => n.type == NodeType.Boss);
}
