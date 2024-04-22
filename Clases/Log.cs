using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Net;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using static System.Net.Mime.MediaTypeNames;

namespace ExpoMR.Clases
{
    public static class Log
    {
        private static long IdLog = 0;
        private static List<SesionLog> ListaLog = new List<SesionLog>();

        public static bool SaveSesion(HttpRequest Request, string UserName, bool IniciaSesion, bool CierreManual = false)

        {
            bool ret = false;
           
            if ((IniciaSesion && UserName.Trim() != "") || (!IniciaSesion && IdLog > 0))
            {

                SqlParameter[] parameters = new SqlParameter[13];

                parameters[0] = new SqlParameter("@IdLog", IniciaSesion ? 0 : IdLog);
                parameters[1] = new SqlParameter("@UserName", UserName);
                parameters[2] = new SqlParameter("@IP", GetIPAddress(Request));
                parameters[3] = new SqlParameter("@PlataformName", GetUserPlatform(Request));
                parameters[4] = new SqlParameter("@BrowserName", GetUserBrowser(Request));
                parameters[5] = new SqlParameter("@BrowserType", GetBrowserType(Request));
                parameters[6] = new SqlParameter("@BrowserVersion", GetUserVersionBrowser(Request));
                parameters[7] = new SqlParameter("@BrowserPlataform", GetBrowserPlataform(Request));
                parameters[8] = new SqlParameter("@BrowserBeta", false);
                parameters[9] = new SqlParameter("@BrowserCrawler", false);
                parameters[10] = new SqlParameter("@BrowserWin16", false);
                parameters[11] = new SqlParameter("@BrowserWin32", false);
                parameters[12] = new SqlParameter("@RetValue", System.Data.SqlDbType.Int);
                parameters[12].Direction = ParameterDirection.ReturnValue;
                DB dB = new DB();
                dB.ExecuteNonQuery("logSesion", ref parameters);

                IdLog = IniciaSesion ? Convert.ToInt64(parameters[12].Value) : IdLog;

                ret = true;
            }
            return ret;
        }

        public static void SaveBusqueda(int cliente, int proveedor, string busqueda)
        {
            SqlParameter[] parameters = new SqlParameter[3];
            parameters[0] = new SqlParameter("@cliente", cliente);
            parameters[1] = new SqlParameter("@proveedor", proveedor);
            parameters[2] = new SqlParameter("@busqueda", busqueda);
            DB dB = new DB();
            dB.ExecuteNonQuery("logBusqueda", ref parameters);
        }

        public static void SaveVisita(int cliente, int proveedor, int producto)
        {
            SqlParameter[] parameters = new SqlParameter[3];
            parameters[0] = new SqlParameter("@cliente", cliente);
            parameters[1] = new SqlParameter("@proveedor", proveedor);
            parameters[2] = new SqlParameter("@producto", producto);
            DB dB = new DB();
            dB.ExecuteNonQuery("logVisita", ref parameters);
        }

        public static string GetUserBrowser(HttpRequest request)
        {
            string ua = request.Headers["User-Agent"].ToString();
            if ((ua.Contains("Mobile")) && (ua.Contains("CriOS"))) return "Chrome";
            if ((ua.Contains("Mobile")) && (ua.Contains("FxiOS"))) return "Firefox";
            if ((ua.Contains("Mobile")) && (ua.Contains("EdgiOS"))) return "Edge";
            if ((ua.Contains("Mobile")) && (ua.Contains("OPiOS"))) return "Opera/Opera Mini";
            if ((ua.Contains("Mobile")) && (ua.Contains("SamsungBrowser"))) return "SamsungBrowser";
            if ((ua.Contains("Mobile")) && (ua.Contains("Focus"))) return "Firefox Focus";
            if ((ua.Contains("Mobile")) && (ua.Contains("Maxthon"))) return "Maxthon";
            if ((ua.Contains("Mobile")) && (ua.Contains("EdgA"))) return "EdgA";
            if ((ua.Contains("Mobile")) && (ua.Contains("Web Explorer"))) return "Web Explorer";
            if ((ua.Contains("Mobile")) && (ua.Contains("OPR"))) return "Opera/Opera Mini";
            if ((ua.Contains("Mobile")) && (ua.Contains("Chrome"))) return "Chrome";
            if ((ua.Contains("Mobile")) && (ua.Contains("Safari"))) return "Safari";
            if (ua.Contains("bingbot") || ua.Contains("Bingbot")) return "Bingbot";
            if (ua.Contains("MSIE") || ua.Contains("Trident")) return "Internet Explorer";
            if (ua.Contains("Edg")) return "Edge";
            if (ua.Contains("SeaMonkey")) return "SeaMonkey";
            if (ua.Contains("Firefox")) return "Firefox";
            if (ua.Contains("OPR")) return "Opera";
            if (ua.Contains("Dragon")) return "Comodo Dragon";
            if (ua.Contains("Vivaldi")) return "Vivaldi";
            if (ua.Contains("ChromePlus")) return "ChromePlus";
            if (ua.Contains("Chrome")) return "Chrome";
            if (ua.Contains("Maxthon")) return "Maxthon";
            if (ua.Contains("Safari")) return "Safari";
            if (ua.Contains("Thunderbird")) return "Thunderbird";
            if (ua.Contains("Netscape")) return "Netscape";
            if (ua.Contains("Konqueror")) return "Konqueror";
            if (ua.Contains("Enterprise Browser")) return "Enterprise Browser";
            //fallback to basic name browser:
            return "";// request.Browser.Browser;
        }

        private static string GetUserVersionBrowser(HttpRequest request)
        {
            string ua = request.Headers["User-Agent"].ToString();
            if ((ua.Contains("Mobile")) && (ua.Contains("CriOS"))) return GetVersion(ua, "CriOS/");
            if ((ua.Contains("Mobile")) && (ua.Contains("FxiOS"))) return GetVersion(ua, "FxiOS/");
            if ((ua.Contains("Mobile")) && (ua.Contains("EdgiOS"))) return GetVersion(ua, "EdgiOS/");
            if ((ua.Contains("Mobile")) && (ua.Contains("OPiOS"))) return GetVersion(ua, "OPiOS/");
            if ((ua.Contains("Mobile")) && (ua.Contains("SamsungBrowser"))) return GetVersion(ua, "SamsungBrowser/");
            if ((ua.Contains("Mobile")) && (ua.Contains("Focus"))) return GetVersion(ua, "Focus/");
            if ((ua.Contains("Mobile")) && (ua.Contains("Maxthon"))) return GetVersion(ua, "Maxthon/");
            if ((ua.Contains("Mobile")) && (ua.Contains("EdgA"))) return GetVersion(ua, "EdgA/");
            if ((ua.Contains("Mobile")) && (ua.Contains("Web Explorer"))) return GetVersion(ua, "Web Explorer/");
            if ((ua.Contains("Mobile")) && (ua.Contains("OPR"))) return GetVersion(ua, "OPR/");
            if ((ua.Contains("Mobile")) && (ua.Contains("Chrome"))) return GetVersion(ua, "Chrome/");
            if ((ua.Contains("Mobile")) && (ua.Contains("Safari"))) return GetVersion(ua, "Safari/");
            if (ua.Contains("bingbot")) return GetVersion(ua, "bingbot/");
            if (ua.Contains("Bingbot")) return GetVersion(ua, "Bingbot/");
            if (ua.Contains("MSIE 5.0")) return "5.0";
            if (ua.Contains("MSIE 6.0")) return "6.0";
            if (ua.Contains("MSIE 7.0")) return "7.0";
            if (ua.Contains("MSIE 8.0") || ua.Contains("Trident/4.0")) return "8.0";
            if (ua.Contains("MSIE 9.0") || ua.Contains("Trident/5.0")) return "9.0";
            if (ua.Contains("MSIE 10.0") || ua.Contains("Trident/6.0")) return "10.0";
            if (ua.Contains("MSIE 11") || ua.Contains("Trident/7.0")) return "11";
            if (ua.Contains("Edg")) return GetVersion(ua, "Edg/");
            if (ua.Contains("SeaMonkey")) return GetVersion(ua, "SeaMonkey/");
            if (ua.Contains("Firefox")) return GetVersion(ua, "Firefox/");
            if (ua.Contains("OPR")) return GetVersion(ua, "OPR/");
            if (ua.Contains("Dragon")) return GetVersion(ua, "Dragon/");
            if (ua.Contains("Vivaldi")) return GetVersion(ua, "Vivaldi/");
            if (ua.Contains("ChromePlus")) return GetVersion(ua, "ChromePlus/");
            if (ua.Contains("Chrome")) return GetVersion(ua, "Chrome/");
            if (ua.Contains("Maxthon")) return GetVersion(ua, "Maxthon/");
            if (ua.Contains("Safari")) return GetVersion(ua, "Safari/");
            if (ua.Contains("Thunderbird")) return GetVersion(ua, "Thunderbird/");
            if (ua.Contains("Netscape")) return GetVersion(ua, "Netscape/");
            if (ua.Contains("Konqueror")) return GetVersion(ua, "Konqeror/");
            if (ua.Contains("Enterprise Browser")) return GetVersion(ua, "Enterprise Browser/");
            //fallback to basic version:
            return "";// request.Browser.Version;
        }

        private static string GetUserPlatform(HttpRequest request)
        {
            var ua = request.Headers["User-Agent"].ToString();
            if (ua.Contains("Android")) return string.Format("Android {0}", GetMobileVersion(ua, "Android"));
            if (ua.Contains("iPad")) return string.Format("iPad OS {0}", GetMobileVersion(ua, "OS"));
            if (ua.Contains("iPhone")) return string.Format("iPhone OS {0}", GetMobileVersion(ua, "OS"));
            if (ua.Contains("Linux") && ua.Contains("KFAPWI")) return "Kindle Fire";
            if (ua.Contains("RIM Tablet") || (ua.Contains("BB") && ua.Contains("Mobile"))) return "Black Berry";
            if (ua.Contains("Windows Phone")) return string.Format("Windows Phone {0}", GetMobileVersion(ua, "Windows Phone"));
            if (ua.Contains("Mac OS")) return "Mac OS";
            if (ua.Contains("Windows NT 5.1") || ua.Contains("Windows NT 5.2")) return "Windows XP";
            if (ua.Contains("Windows NT 6.0")) return "Windows Vista";
            if (ua.Contains("Windows NT 6.1")) return "Windows 7";
            if (ua.Contains("Windows NT 6.2")) return "Windows 8";
            if (ua.Contains("Windows NT 6.3")) return "Windows 8.1";
            if (ua.Contains("Windows NT 10")) return "Windows 10";
            //fallback to basic platform:
            return "" + (ua.Contains("Mobile") ? " Mobile " : "");//request.Browser.Platform + (ua.Contains("Mobile") ? " Mobile " : "");
        }

        public static string GetIPAddress(HttpRequest request)
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        private static string GetMobileVersion(string userAgent, string device)
        {
            var temp = userAgent.Substring(userAgent.IndexOf(device) + device.Length).TrimStart();
            var version = string.Empty;

            foreach (var character in temp)
            {
                var validCharacter = false;
                int test = 0;

                if (Int32.TryParse(character.ToString(), out test))
                {
                    version += character;
                    validCharacter = true;
                }

                if (character == '.' || character == '_')
                {
                    version += '.';
                    validCharacter = true;
                }

                if (validCharacter == false)
                    break;
            }
            return version;
        }

        private static string GetVersion(string useragent, string NameBroswser)
        {
            useragent = useragent.Substring(useragent.IndexOf(NameBroswser) + NameBroswser.Length);

            if (useragent.IndexOf(" ") != -1)
                useragent = useragent.Substring(0, useragent.IndexOf("."));

            return useragent;
        }

        private static string GetBrowserPlataform(HttpRequest request)
        {
            string ua = request.Headers["User-Agent"].ToString();
            if (ua.Contains("Windows NT"))
                return "WinNT";
            else
                return "Unknown";
        }
        private static string GetBrowserType(HttpRequest request)
        {
            string ua = request.Headers["User-Agent"].ToString();
            if ((ua.Contains("Mobile")) && (ua.Contains("CriOS"))) return GetBrowserTypeTrim(ua, "CriOS/");
            if ((ua.Contains("Mobile")) && (ua.Contains("FxiOS"))) return GetBrowserTypeTrim(ua, "FxiOS/");
            if ((ua.Contains("Mobile")) && (ua.Contains("EdgiOS"))) return GetBrowserTypeTrim(ua, "EdgiOS/");
            if ((ua.Contains("Mobile")) && (ua.Contains("OPiOS"))) return GetBrowserTypeTrim(ua, "OPiOS/");
            if ((ua.Contains("Mobile")) && (ua.Contains("SamsungBrowser"))) return GetBrowserTypeTrim(ua, "SamsungBrowser/");
            if ((ua.Contains("Mobile")) && (ua.Contains("Focus"))) return GetBrowserTypeTrim(ua, "Focus/");
            if ((ua.Contains("Mobile")) && (ua.Contains("Maxthon"))) return GetBrowserTypeTrim(ua, "Maxthon/");
            if ((ua.Contains("Mobile")) && (ua.Contains("EdgA"))) return GetBrowserTypeTrim(ua, "EdgA/");
            if ((ua.Contains("Mobile")) && (ua.Contains("Web Explorer"))) return GetBrowserTypeTrim(ua, "Web Explorer/");
            if ((ua.Contains("Mobile")) && (ua.Contains("OPR"))) return GetBrowserTypeTrim(ua, "OPR/");
            if ((ua.Contains("Mobile")) && (ua.Contains("Chrome"))) return GetBrowserTypeTrim(ua, "Chrome/");
            if ((ua.Contains("Mobile")) && (ua.Contains("Safari"))) return GetBrowserTypeTrim(ua, "Safari/");
            if (ua.Contains("bingbot")) return GetBrowserTypeTrim(ua, "bingbot/");
            if (ua.Contains("Bingbot")) return GetBrowserTypeTrim(ua, "Bingbot/");
            if (ua.Contains("MSIE 5.0")) return "MSIE 5.0";
            if (ua.Contains("MSIE 6.0")) return "MSIE 6.0";
            if (ua.Contains("MSIE 7.0")) return "MSIE 7.0";
            if (ua.Contains("MSIE 8.0") || ua.Contains("Trident/4.0")) return "8.0";
            if (ua.Contains("MSIE 9.0") || ua.Contains("Trident/5.0")) return "9.0";
            if (ua.Contains("MSIE 10.0") || ua.Contains("Trident/6.0")) return "10.0";
            if (ua.Contains("MSIE 11") || ua.Contains("Trident/7.0")) return "11";
            if (ua.Contains("Edg")) return GetBrowserTypeTrim(ua, "Edg/");
            if (ua.Contains("SeaMonkey")) return GetBrowserTypeTrim(ua, "SeaMonkey/");
            if (ua.Contains("Firefox")) return GetBrowserTypeTrim(ua, "Firefox/");
            if (ua.Contains("OPR")) return GetBrowserTypeTrim(ua, "OPR/");
            if (ua.Contains("Dragon")) return GetBrowserTypeTrim(ua, "Dragon/");
            if (ua.Contains("Vivaldi")) return GetBrowserTypeTrim(ua, "Vivaldi/");
            if (ua.Contains("ChromePlus")) return GetBrowserTypeTrim(ua, "ChromePlus/");
            if (ua.Contains("Chrome")) return GetBrowserTypeTrim(ua, "Chrome/");
            if (ua.Contains("Maxthon")) return GetBrowserTypeTrim(ua, "Maxthon/");
            if (ua.Contains("Safari")) return GetBrowserTypeTrim(ua, "Safari/");
            if (ua.Contains("Thunderbird")) return GetBrowserTypeTrim(ua, "Thunderbird/");
            if (ua.Contains("Netscape")) return GetBrowserTypeTrim(ua, "Netscape/");
            if (ua.Contains("Konqueror")) return GetBrowserTypeTrim(ua, "Konqeror/");
            if (ua.Contains("Enterprise Browser")) return GetBrowserTypeTrim(ua, "Enterprise Browser/");
            //fallback to basic version:
            return "";// request.Browser.Version;
        }
        private static string GetBrowserTypeTrim(string useragent, string NameBroswser) {

            useragent = useragent.Substring(useragent.IndexOf(NameBroswser) + NameBroswser.Length);
            int index = useragent.IndexOf('.');
            string result = index >= 0 ? useragent.Substring(0, index) : useragent;            
            return NameBroswser.Replace("/", " ") + result;
        }

        public static void LogNotificacion(int Cliente, int tipoNotif)
        {
            SqlParameter[] parameters = new SqlParameter[2];
            parameters[0] = new SqlParameter("@cliente", Cliente);
            parameters[1] = new SqlParameter("@IdTipoNotif", tipoNotif);

            DB dB = new DB();
            dB.ExecuteNonQuery("logNotificacion", ref parameters);
        }
    }

    public class SesionLog
    {
        public long IdLog;
        public string SessionID;

        public SesionLog()
        {
            IdLog = 0;
            SessionID = "";
        }
    }
}
