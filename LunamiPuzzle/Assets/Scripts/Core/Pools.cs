using System;
using System.Collections.Generic;

namespace Core
{
    public class Pool<T> where T : class
    {
        private readonly Stack<T> _poolStack;
        private readonly Func<T> _objectGenerator; // 用于创建新对象的方法
        private readonly Action<T> _onGet; // 从池中获取对象时的回调
        private readonly Action<T> _onRelease; // 将对象释放回池时的回调
        private readonly int _initialCapacity; // 初始容量
        private readonly bool _autoExpand; // 是否允许池自动扩展

        public int Count => _poolStack.Count; // 当前池中对象数量I

        /// <summary>
        /// 创建一个泛型对象池
        /// </summary>
        /// <param name="objectGenerator">对象创建方法</param>
        /// <param name="onGet">对象获取时的操作</param>
        /// <param name="onRelease">对象释放时的操作</param>
        /// <param name="initialCapacity">初始容量</param>
        /// <param name="autoExpand">是否允许自动扩展</param>
        public Pool(Func<T> objectGenerator, Action<T> onGet = null, Action<T> onRelease = null,
            int initialCapacity = 10, bool autoExpand = true)
        {
            _objectGenerator = objectGenerator ?? throw new ArgumentNullException(nameof(objectGenerator));
            _onGet = onGet;
            _onRelease = onRelease;
            _initialCapacity = initialCapacity;
            _autoExpand = autoExpand;
            _poolStack = new Stack<T>(initialCapacity);

            // 初始化池
            for (int i = 0; i < initialCapacity; i++)
            {
                _poolStack.Push(_objectGenerator());
            }
        }

        /// <summary>
        /// 从对象池中获取一个对象
        /// </summary>
        /// <returns>获取的对象</returns>
        public T Get()
        {
            T obj;
            if (_poolStack.Count > 0)
            {
                obj = _poolStack.Pop();
            }
            else if (_autoExpand)
            {
                obj = _objectGenerator();
            }
            else
            {
                throw new InvalidOperationException("对象池已空，且自动扩展被禁用！");
            }

            _onGet?.Invoke(obj);
            return obj;
        }

        /// <summary>
        /// 将对象释放回对象池
        /// </summary>
        /// <param name="obj">需要释放的对象</param>
        public void Release(T obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj), "释放的对象不能为null！");
            }

            _onRelease?.Invoke(obj);
            _poolStack.Push(obj);
        }

        public void Clear()
        {
            // 释放所有对象的资源
            while (_poolStack.Count > 0)
            {
                T obj = _poolStack.Pop();
                _onRelease?.Invoke(obj);

                // 如果对象实现了IDisposable，则调用Dispose
                if (obj is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }
    }
}