using ClassLibrary1;
using Galaxy.Mercury.OAuthClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            OAuthClientExtensions.Init("dfsaf", "http://192.168.1.1//");

            var rbacLoginResult = API.RbacLogin.Request(new { name = "admin", pwd = "123" });
            var ssoLoginResult = API.SSOLogin.Request(new { unionid = "1" });
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
        public static APIConfig<string> RbacLogin = new APIConfig<string>("api/rbac/login", HttpMethod.Get);


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
                            Route = BuildGetParasRoute(Route, arguments);
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

        /// <summary>
        /// 生成get 路由参数
        /// </summary>
        /// <param name="route"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        private string BuildGetParasRoute(string route, object arguments)
        {
            StringBuilder urlParas = new StringBuilder("");

            var paras = arguments.GetType().GetProperties();
            foreach (var item in paras)
            {
                string key = item.Name;
                string value = (string)item.GetValue(arguments);
                urlParas.AppendFormat("&{0}={1}", key, value);
            }

            urlParas.Remove(0, 1);

            if (route.LastIndexOf('?') < 0)
            {
                urlParas.Insert(0, '?');
            }

            return route + urlParas.ToString();
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
