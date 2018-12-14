using System;
using Newtonsoft.Json;

namespace ClassLibrary1
{
    /// <summary>
    /// json 序列化、反序列化操作 扩展类
    /// </summary>
    public static class JsonExtension
    {
        
        /// <summary>
        /// 对象序列化成Json字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns> 
        public static string M5_ObjectToJson(this object obj)
        {
            if (obj == null)
            {
                return string.Empty;
            }
            return JsonConvert.SerializeObject(obj);
        }

        /// <summary>
        /// 对象序列化成Json字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns> 
        public static string M5_ObjectToJson(this object obj, Formatting format)
        {
            if (obj == null)
            {
                return string.Empty;
            }
            return JsonConvert.SerializeObject(obj, format);
        }


        /// <summary>
        /// Json字符串序列化成对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T M5_JsonToObject<T>(this string obj) where T : class
        {
            if (obj == null)
            {
                return default(T);
            }
            return JsonConvert.DeserializeObject<T>(obj);
        }

    }
}
