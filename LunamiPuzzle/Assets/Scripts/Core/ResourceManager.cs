using UnityEngine;

namespace Core
{
    public static class ResourceManager<T> where T : Object
    {
        public static T Load(string path)
        {
            return Resources.Load<T>(path);
        }
    }
}