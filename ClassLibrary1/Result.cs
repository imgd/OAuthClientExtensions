using System;

namespace ClassLibrary1
{
    /// <summary>
    /// 业务消息统一输出实体
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class Result<T>
    {
        /// <summary>
        /// 消息标识code
        /// </summary>
        public int code { get; set; }

        /// <summary>
        /// 消息标识描述
        /// </summary>
        public string message { get; set; }

        /// <summary>
        /// 返回消息内容
        /// </summary>
        public T data { get; set; }


        public Result() { }
        public Result(short code, string message)
        {
            this.code = code;
            this.message = message;
            this.data = default(T);
        }
        public Result(short code, string message, T data)
        {
            this.code = code;
            this.message = message;
            this.data = data;
        }

        public void SetResult(short code, string message)
        {
            this.code = code;
            this.message = message;
            this.data = default(T);
        }
        public void SetResult(short code, string message, T data)
        {
            this.code = code;
            this.message = message;
            this.data = data;
        }
    }

}
