namespace Licenta.Commons.Math
{
    public interface IOperative { IOperative Clone(); }    
    public interface IAdditive<in T> : IOperative { IOperative Add(T x); }
    public interface ISubtrative<in T> : IOperative { IOperative Subtract(T x); }
    public interface IMultiplicative<in T> : IOperative { IOperative Multiply(T x); }
    public interface IDivisive<in T> : IOperative { IOperative Divide(T x); }    
    public interface INumber 
        : IAdditive<INumber>, ISubtrative<INumber>, IMultiplicative<INumber>, IDivisive<INumber>
    {
        new INumber Add(INumber x);
        new INumber Subtract(INumber x);
        new INumber Multiply(INumber x);
        new INumber Divide(INumber x);        
    }    
}
