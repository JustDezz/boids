namespace Tools.Pool
{
    public interface IPool<T>
    {
        public T Get();
        public void Return(T toReturn);
    }
}