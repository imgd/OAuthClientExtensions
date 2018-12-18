using ClassLibrary1;
using Galaxy.Mercury.OAuthClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApp1
{
    /// <summary>
    /// rbac登陆输入实体
    /// </summary>
    public class RbacLogin
    {
        /// <summary>
        /// 账号
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            OAuthClientExtensions.Init("sasdfdj32%dk&dsk", "http://192.168.1.1//");

            var rbacLoginResult = API.RbacLogin.Request(new RbacLogin
            {
                Name = "admin",
                Password = "132"
            });

            var ssoLoginResult = API.SSOLogin.Request(new
            {
                unionid = "1"
            });
        }
    }

    /// <summary>
    /// 全局API请求配置
    /// </summary>
    public static class API
    {
        /// <summary>
        /// rbac登陆
        /// </summary>
        public static APIConfig<string, RbacLogin> RbacLogin = new APIConfig<string, RbacLogin>("api/rbac/login", HttpMethod.Get);


        /// <summary>
        /// SSO登陆
        /// </summary>
        public static APIConfig<bool> SSOLogin = new APIConfig<bool>("api/sso/login", HttpMethod.Post);
    }

    /// <summary>
    /// API路由配置
    /// </summary>
    /// <typeparam name="T">API请求对象</typeparam>
    public class APIConfig<T>
    {
        public APIConfig(string route, HttpMethod method)
        {
            Route = route;
            Method = method;
        }
        /// <summary>
        /// 请求路由
        /// </summary>
        private string Route { get; set; }

        /// <summary>
        /// 请求方式
        /// </summary>
        private HttpMethod Method { get; set; }

        /// <summary>
        /// API请求
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public Result<T> Request(object arguments = null)
        {
            var results = default(Result<T>);
            switch (Method)
            {
                case HttpMethod.Get:
                    {
                        if (arguments != null)
                        {
                            Route = Route.BuildGetParasRoute(arguments);
                        }

                        results = Route.M5_APIGet<T>();

                        break;
                    }
                case HttpMethod.Post:
                    {
                        results = Route.M5_APIPost<T>(arguments);
                        break;
                    }

            }

            return results;
        }


    }
    /// <summary>
    /// API路由配置 input重载
    /// </summary>
    /// <typeparam name="T">API请求对象</typeparam>
    public class APIConfig<T, TInput>
    {
        public APIConfig(string route, HttpMethod method)
        {
            Route = route;
            Method = method;
        }
        /// <summary>
        /// 请求路由
        /// </summary>
        private string Route { get; set; }

        /// <summary>
        /// 请求方式
        /// </summary>
        private HttpMethod Method { get; set; }

        /// <summary>
        /// API请求
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public Result<T> Request(TInput arguments = default(TInput))
        {
            var results = default(Result<T>);
            switch (Method)
            {
                case HttpMethod.Get:
                    {
                        if (arguments != null)
                        {
                            Route = Route.BuildGetParasRoute(arguments);
                        }

                        results = Route.M5_APIGet<T>();

                        break;
                    }
                case HttpMethod.Post:
                    {
                        results = Route.M5_APIPost<T>(arguments);
                        break;
                    }

            }

            return results;
        }


    }

    /// <summary>
    /// API路由配置扩展方法
    /// </summary>
    public static class APIConfigExtensions
    {
        /// <summary>
        /// 生成get 路由参数
        /// </summary>
        /// <param name="route"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public static string BuildGetParasRoute(this string route, object arguments)
        {
            StringBuilder urlParas = new StringBuilder("");

            var paras = arguments.GetType().GetProperties();
            foreach (var item in paras)
            {
                string key = item.Name;
                string value = item.GetValue(arguments).TryParseString();
                urlParas.AppendFormat("&{0}={1}", key, value);
            }

            urlParas.Remove(0, 1);

            if (route.LastIndexOf('?') < 0)
            {
                urlParas.Insert(0, '?');
            }

            return route + urlParas.ToString();
        }

        /// <summary>
        /// object转换string
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static string TryParseString(this object val)
        {
            if (val == null)
            {
                return string.Empty;
            }
            return val.ToString();
        }
    }

    /// <summary>
    /// 请求方式枚举
    /// </summary>
    public enum HttpMethod
    {
        Get,
        Post
    }
}
