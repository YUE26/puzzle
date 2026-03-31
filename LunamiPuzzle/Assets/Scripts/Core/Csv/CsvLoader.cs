using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Core.Csv
{
    public class CsvLoader
    {
        public Dictionary<int, T> Load<T>(string csvName) where T : new()
        {
            Dictionary<int, T> temp = new Dictionary<int, T>();
        
            if (CsvStore.store.TryGetValue(csvName, out var properties))
            {
                for (int row = 0; row < properties.GetLength(0); row++)
                {
                    var fields = new string[properties.GetLength(1)];
                    for (int col = 0; col < properties.GetLength(1); col++)
                    {
                        fields[col] = properties[row, col];
                    }
        
                    if (csvName.Contains(typeof(T).Name) == false)
                    {
                        Debug.LogError("表名和加载名不匹配，请检查" + csvName);
                        return null;
                    }
        
                    Type type = typeof(T);
                    var infos = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                        .Where(p => !p.GetGetMethod().IsStatic);
                    var i = 0;
                    var instance = new T();
                    foreach (var info in infos)
                    {
                        if (i < fields.Length)
                        {
                            try
                            {
                                if (info.PropertyType == typeof(bool))
                                {
                                    if (fields[i] == "0" || fields[i] == String.Empty)
                                    {
                                        fields[i] = "false";
                                    }
                                    else
                                    {
                                        fields[i] = "true";
                                    }
                                }
        
                                if (info.PropertyType == typeof(Vector3))
                                {
                                    if (fields[i] == "0" || fields[i] == String.Empty)
                                    {
                                        //fields[i] = Vector3.zero.ToString();
                                        info.SetValue(instance, Vector3.zero);
                                    }
                                    else
                                    {
                                        var pos = fields[i].Split("_");
                                        if (pos.Length == 3 && float.TryParse(pos[0], out float x) &&
                                            float.TryParse(pos[1], out float y) &&
                                            float.TryParse(pos[2], out float z))
                                        {
                                            //fields[i] = new Vector3(x, y, z).ToString();
                                            info.SetValue(instance, new Vector3(x, y, z));
                                        }
                                        else
                                        {
                                            throw new FormatException("Invalid Vector3 format. Expected 'x-y-z'");
                                        }
                                    }
                                }else if (info.PropertyType == typeof(int[]))
                                {
                                    var arr = fields[i].Split("&");
                                    var intArr = new int[arr.Length];
                                    for (int j = 0; j < arr.Length; j++)
                                    {
                                        intArr[j] = Int32.Parse(arr[j]);
                                    }
        
                                    info.SetValue(instance, intArr);
                                }
                                else
                                {
                                    var value = Convert.ChangeType(fields[i], info.PropertyType);
                                    info.SetValue(instance, value);
                                }
                            }
                            catch (Exception e)
                            {
                                Debug.LogError($"属性赋值错误: {info.Name}, 值: {fields[i]}, 错误: {e.Message}");
                                throw;
                            }
                        }
        
                        ++i;
                    }
        
                    temp.Add(int.Parse(fields[0]), instance);
                }
        
                return temp;
            }
        
            return null;
        }
    }
}