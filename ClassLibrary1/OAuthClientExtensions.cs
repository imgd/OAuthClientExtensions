using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Caching;
using ClassLibrary1;

namespace Galaxy.Mercury.OAuthClient
{
    /// <summary>
    /// OAuth 业务请求扩展方法
    /// </summary>
    public static class OAuthClientExtensions
    {
        //这里请初始化配置参数，或调用Init方法初始化       
        private static string _appSercet = "";
        private static string _oauthDomain = "";
        private static Cache _sercetAndToken = HttpRuntime.Cache;


        /// <summary>
        /// 配置参数初始化
        /// </summary>
        /// <param name="sercet">客户端sercet秘钥</param>
        /// <param name="oauthDomain">服务端API请求域，注意结尾必须有 "/" 例如：http://127.0.0.1/ </param>
        public static void Init(string sercet, string oauthDomain)
        {
            _appSercet = sercet;
            _oauthDomain = oauthDomain;
        }


        /// <summary>
        /// GET方式 业务调用
        /// </summary>
        /// <typeparam name="T">返回Result.data 对象类型</typeparam>
        /// <param name="apiRoute">请求路由</param>
        /// <returns>Result<T></returns>
        public static Result<T> M5_APIGet<T>(this string apiRoute)
        {
            return apiRoute.APITryGet<T>((token, route) =>
             {
                 return route.OAuthGet<T>(token);
             });
        }


        /// <summary>
        /// POST方式业务调用
        /// </summary>
        /// <typeparam name="T">返回Result.data 对象类型</typeparam>
        /// <param name="apiRoute">请求路由</param>
        /// <param name="paras">请求参数匿名类对象</param>
        /// <returns>Result<T></returns>
        public static Result<T> M5_APIPost<T>(this string apiRoute, object paras = null)
        {
            return apiRoute.APITryPost<T>((token, route, arguments) =>
            {
                return route.OAuthPost<T>(arguments, token);
            }, paras.M5_ObjectToJson());
        }


        /// <summary>
        /// POST方式业务调用
        /// </summary>
        /// <typeparam name="T">返回Result.data 对象类型</typeparam>
        /// <typeparam name="TParas">请求参数对象类型</typeparam>
        /// <param name="apiRoute">请求路由</param>
        /// <param name="paras">请求参数对象</param>
        /// <returns>Result<T></returns>
        public static Result<T> M5_APIPost<T, TParas>(this string apiRoute, TParas paras = default(TParas))
        {
            return apiRoute.APITryPost<T>((token, route, arguments) =>
            {
                return route.OAuthPost<T>(arguments, token);
            }, paras.M5_ObjectToJson());
        }


        #region Private Functions


        /// <summary>
        /// OAuth Get请求
        /// </summary>
        /// <typeparam name="T">Result.data 对象类型</typeparam>
        /// <param name="apiRoute">请求路由</param>
        /// <param name="requestFun">参数1：token 参数2：路由 参数3：返回值</param>
        /// <returns>Result<T></returns>
        private static Result<T> APITryGet<T>(this string apiRoute, Func<string, string, Result<T>> requestFun)
        {
            //API请求绝对路由
            apiRoute = _oauthDomain.Trim() + apiRoute;

            var token = _sercetAndToken.Get(_appSercet);
            //缓存失效，重新获取，缓存
            if (token == null)
            {
                token = BuildAndStoreToken();
            }
            //调用业务请求
            var resut = requestFun(token.ToString(), apiRoute);

            //Token过期重试,直到请求成功为止
            if (resut.code.IsTokenExpired())
            {
                token = BuildAndStoreToken();
                return requestFun(token.ToString(), apiRoute);
            }

            return resut;
        }

        /// <summary>
        /// OAuth Post请求
        /// </summary>
        /// <typeparam name="T">Result.data 对象类型</typeparam>
        /// <param name="apiRoute">请求路由</param>
        /// <param name="requestFun">参数1：token 参数2：路由 参数3：请求json字符串参数 参数4：返回值</param>
        /// <param name="paras">请求json字符串参数</param>
        /// <returns>Result<T></returns>
        private static Result<T> APITryPost<T>(this string apiRoute, Func<string, string, string, Result<T>> requestFun, string paras = "")
        {
            //API请求绝对路由
            apiRoute = _oauthDomain.Trim() + apiRoute;

            var token = _sercetAndToken.Get(_appSercet);
            //缓存失效，重新获取，缓存
            if (token == null)
            {
                token = BuildAndStoreToken();
            }
            //调用业务请求
            var resut = requestFun(token.ToString(), apiRoute, paras);

            //Token过期重试,直到请求成功为止
            if (resut.code.IsTokenExpired())
            {
                token = BuildAndStoreToken();
                return requestFun(token.ToString(), apiRoute, paras);
            }

            return resut;
        }

        /// <summary>
        /// 请求获取token，且缓存
        /// </summary>
        /// <returns></returns>
        private static string BuildAndStoreToken()
        {
            //检查配置参数
            CheckInitArguments();

            var cacheToken = _sercetAndToken.Get(_appSercet);
            if (cacheToken != null)
            {
                return cacheToken.ToString();
            }

            Result<OAuthToken> responseResult = $"{_oauthDomain}api/oauth/Token/Create".Post<OAuthToken>(new
            {
                sercet = _appSercet
            });

            //token获取失败
            if (!responseResult.code.IsSuccessed() ||
                responseResult.code.IsRequestException())
            {
                throw new Exception("毁灭性错误：Token 请求失败导致的！请查看日志。");
            }

            string token = responseResult.data.Token;

            _sercetAndToken.Insert(_appSercet, token, null, responseResult.data.ExpireTime, Cache.NoSlidingExpiration, null);
            return token;
        }

        /// <summary>
        /// 判断是否业务请求成功
        /// </summary>
        /// <param name="code">业务编码code</param>
        /// <returns>bool</returns>
        private static bool IsSuccessed(this int code)
        {
            return code == 1;
        }

        /// <summary>
        /// 判断是否请求异常
        /// </summary>
        /// <param name="code">业务编码code</param>
        /// <returns>bool</returns>
        private static bool IsRequestException(this int code)
        {
            return code == 0;
        }

        /// <summary>
        /// 判断是否Token失效
        /// </summary>
        /// <param name="code">业务编码code</param>
        /// <returns>bool</returns>
        private static bool IsTokenExpired(this int code)
        {
            return code == -60003;
        }

        /// <summary>
        /// 检查初始化配置参数
        /// </summary>
        private static void CheckInitArguments()
        {
            if (string.IsNullOrEmpty(_appSercet) ||
                string.IsNullOrEmpty(_oauthDomain)
                )
            {
                throw new ArgumentNullException("OAuthClient未初始化配置参数：secret或oauthdomain 。");
            }
        }
        #endregion

    }


    /// <summary>
    /// WebClient请求扩展
    /// </summary>
    public static class WebClientExtensions
    {
        //请配置
        private static readonly string tokenHeaderKey = "X-SOURCE-TOKEN";

        public static Result<T> Get<T>(this string url)
        {
            try
            {
                WebClient client = BuildWebClient();
                return client.DownloadString(url).M5_JsonToObject<Result<T>>();
            }
            catch (Exception ex)
            {
                //日志记录
                return new Result<T>(0, $"请求异常:{ex.Message}");
            }

        }
        public static Result<T> Post<T>(this string url, object postJsonData)
        {
            try
            {
                WebClient client = BuildWebClient();
                byte[] bytes = Encoding.UTF8.GetBytes(postJsonData.M5_ObjectToJson());
                Encoding enc = Encoding.GetEncoding("UTF-8");
                byte[] responseData = client.UploadData(url, "POST", bytes);
                return enc.GetString(responseData).M5_JsonToObject<Result<T>>();
            }
            catch (Exception ex)
            {
                //日志记录
                return new Result<T>(0, $"请求异常:{ex.Message}");
            }
        }

        public static Result<T> OAuthGet<T>(this string url, string token)
        {
            try
            {
                WebClient client = BuildOAuthWebClient(token);
                return client.DownloadString(url).M5_JsonToObject<Result<T>>();
            }
            catch (Exception ex)
            {
                //日志记录
                return new Result<T>(0, $"请求异常:{ex.Message}");
            }
        }
        public static Result<T> OAuthPost<T>(this string url, object postJsonData, string token)
        {
            try
            {
                WebClient client = BuildOAuthWebClient(token);
                byte[] bytes = Encoding.UTF8.GetBytes(postJsonData.M5_ObjectToJson());
                Encoding enc = Encoding.GetEncoding("UTF-8");
                byte[] responseData = client.UploadData(url, "POST", bytes);
                return enc.GetString(responseData).M5_JsonToObject<Result<T>>();
            }
            catch (Exception ex)
            {
                //日志记录
                return new Result<T>(0, $"请求异常:{ex.Message}");
            }
        }
        public static Result<T> OAuthPost<T, TParas>(this string url, TParas postJsonData, string token)
        {
            try
            {
                WebClient client = BuildOAuthWebClient(token);
                byte[] bytes = Encoding.UTF8.GetBytes(postJsonData.M5_ObjectToJson());
                Encoding enc = Encoding.GetEncoding("UTF-8");
                byte[] responseData = client.UploadData(url, "POST", bytes);
                return enc.GetString(responseData).M5_JsonToObject<Result<T>>();
            }
            catch (Exception ex)
            {
                //日志记录
                return new Result<T>(0, $"请求异常:{ex.Message}");
            }
        }
        private static WebClient BuildWebClient()
        {
            WebClient client = new WebClient();
            client.Headers.Add("Content-Type", "application/json");
            return client;
        }
        private static WebClient BuildOAuthWebClient(string token)
        {
            WebClient client = new WebClient();
            client.Headers.Add("Content-Type", "application/json");
            client.Headers.Add(tokenHeaderKey, token);
            return client;
        }

    }



    /// <summary>
    /// Token实体类
    /// </summary>
    public class OAuthToken
    {
        /// <summary>
        /// token令牌
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// token令牌服务器缓存绝对失效时间
        /// </summary>
        public DateTime ExpireTime { get; set; }
    }
}
