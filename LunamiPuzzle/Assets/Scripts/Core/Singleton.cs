namespace Core
{
    public class Singleton<T> where T: class,new()
    {
        private static readonly object _lock = new object();
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (_lock)
                    {
                        instance ??= new T();
                    }
                }
                return instance;
            }
        }

        protected Singleton() { }
    }
}