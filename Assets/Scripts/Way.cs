public class Way
{
    private Node _nodeDestiny;
    private float _cost;
    private float _aCUMulatedCost;
    public Node NodeDestiny { get => _nodeDestiny; set { _nodeDestiny = value;} }
    public float Cost { get => _cost; set { _cost = value; } }
    public float AcumulatedCost { get => _aCUMulatedCost; set { _aCUMulatedCost = value; } }
    public Way(Node node, float cost)
    {
        _nodeDestiny = node;
        _cost = cost;
    }
}
