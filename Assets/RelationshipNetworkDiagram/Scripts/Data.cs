using System.Collections.Generic;

public class Node
{
    public string Name;
    public List<Node> Children;
}

public class Line
{
    public string Name;
    public Node Node_1;
    public Node Node_2;
}
