public interface IVisitable
{
    void Accept(IVisitor visitor);
}

public interface IVisitor
{
    void Visit(IVisitable visitable);
}
