using LiteDB;
using Microsoft.AspNetCore.Mvc;
using System.Dynamic;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Glutspeicher.Server.Mapping;

public static partial class Api
{
    public static class Passwords
    {
        static ILiteCollection<Model.Password> Collection(LiteDbContext liteDbContext)
        {
            return liteDbContext.Database.GetCollection<Model.Password>();
        }

        public static IApiResult GetAll(LiteDbContext liteDbContext)
        {
            return ApiResult.Ok(
                Collection(liteDbContext).FindAll().OrderBy(x => x.Name).ThenBy(x => x.Username).ThenBy(x => x.Uri).ToList()
            );
        }

        public static IResult Export(LiteDbContext liteDbContext)
        {
            var data = Collection(liteDbContext).FindAll().ToList();
            return Results.File(
                ToCsv(data),
                "application/octet-stream",
                $"{nameof(Glutspeicher)} {nameof(Passwords)} {Now:yyyy-MM-dd HH-mm-ss}.csv"
            );
        }

        public static IApiResult Get(LiteDbContext liteDbContext, long id)
        {
            var result = Collection(liteDbContext).FindById(id);
            if (result is null)
            {
                return ApiResult.Error();
            }

            var name = result.Name ?? string.Empty;
            var username = result.Username ?? string.Empty;
            var password = result.GeneratedPassword ?? string.Empty;
            password = password == string.Empty ? (result.StaticPassword ?? string.Empty) : password;

            dynamic glutLink_AutoType = new ExpandoObject();

            glutLink_AutoType.type = "AutoType";
            glutLink_AutoType.title = name;
            glutLink_AutoType.text = new[] { username, password };

            var relay = liteDbContext.Database.GetCollection<Model.Relay>().FindOne(x => x.Id == result.RelayId);

            var uri = result.Uri ?? string.Empty;
            if (!uri.Contains("://"))
            {
                uri = $"https://{uri}";
            }

            string schemeType;

            if (uri.StartsWith("rdp://"))
            {
                schemeType = "rdp";
            }
            else if (uri.StartsWith("ssh://"))
            {
                schemeType = "ssh";
            }
            else if (uri.StartsWith("http://"))
            {
                schemeType = "web";
            }
            else if (uri.StartsWith("https://"))
            {
                schemeType = "web";
            }
            else
            {
                return ApiResult.Ok(result);
            }

            dynamic glutLink_Connect = new ExpandoObject();

            string[] hostnameAndPort;

            void UseRelay()
            {
                glutLink_Connect.relayHostname = relay.Hostname;
                glutLink_Connect.relaySshPort = relay.SshPort;
                glutLink_Connect.relaySshUsername = relay.SshUsername;
                glutLink_Connect.relaySshPassword = relay.SshPassword;
                glutLink_Connect.relayMinPort = relay.MinPort;
                glutLink_Connect.relayMaxPort = relay.MaxPort;
            }

            switch (schemeType)
            {
                case "rdp":
                    hostnameAndPort = uri[6..].Split(':', 2);
                    glutLink_Connect.type = "Mstsc";
                    glutLink_Connect.hostname = hostnameAndPort[0];
                    glutLink_Connect.port = hostnameAndPort.Length < 2 ? 3389 : int.Parse(hostnameAndPort[1]);
                    glutLink_Connect.username = username;
                    glutLink_Connect.password = password;
                    if (relay is not null)
                    {
                        UseRelay();
                    }
                    break;

                case "ssh":
                    hostnameAndPort = uri[6..].Split(':', 2);
                    glutLink_Connect.type = "Ssh";
                    glutLink_Connect.hostname = hostnameAndPort[0];
                    glutLink_Connect.port = hostnameAndPort.Length < 2 ? 22 : int.Parse(hostnameAndPort[1]);
                    glutLink_Connect.username = username;
                    glutLink_Connect.password = password;
                    if (relay is not null)
                    {
                        UseRelay();
                    }
                    break;

                case "web":
                    hostnameAndPort = schemeType == "http"
                        ? uri[7..].Split(':', 2)
                        : uri[8..].Split(':', 2);
                    glutLink_Connect.type = "Web";
                    glutLink_Connect.hostname = hostnameAndPort[0];
                    glutLink_Connect.port = hostnameAndPort.Length < 2
                        ? (schemeType == "http" ? 80 : 443)
                        : int.Parse(hostnameAndPort[1]);
                    glutLink_Connect.name = name;
                    glutLink_Connect.uri = uri;
                    if (relay is not null)
                    {
                        var webCommandLine = relay.WebCommandLine ?? string.Empty;
                        if (webCommandLine == string.Empty)
                        {
                            UseRelay();
                        }
                        else
                        {
                            glutLink_Connect.webCommandLine = webCommandLine;
                        }
                    }
                    break;

                default:
                    return ApiResult.Ok(result);
            }

            result.GlutLinks = new()
            {
                AutoType = $"glut://{Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(glutLink_AutoType))}",
                Connect = $"glut://{Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(glutLink_Connect))}"
            };
            return ApiResult.Ok(result);
        }

        public static IApiResult Post(LiteDbContext liteDbContext, [FromBody] Model.Password data)
        {
            data ??= new();
            Collection(liteDbContext).Insert(data);
            liteDbContext.SetDirty();
            return ApiResult.Ok(data);
        }

        public static IApiResult Put(LiteDbContext liteDbContext, [FromBody] Model.Password data)
        {
            var result = Collection(liteDbContext).Update(data);
            liteDbContext.SetDirty();
            return ApiResult.OkIfTrue(result);
        }

        public static IApiResult Delete(LiteDbContext liteDbContext, long id)
        {
            var result = Collection(liteDbContext).Delete(id);
            liteDbContext.SetDirty();
            return ApiResult.OkIfTrue(result);
        }
    }
}
