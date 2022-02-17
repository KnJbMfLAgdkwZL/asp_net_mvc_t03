using System.Net.WebSockets;
using System.Text;
using asp_net_mvc_t03.DTO;
using asp_net_mvc_t03.Enums;
using asp_net_mvc_t03.Interfaces;
using asp_net_mvc_t03.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace asp_net_mvc_t03.Services;

public class WebSocketServer : IWebSocketServer
{
    private WebSocket? WebSocket { set; get; }
    private readonly MasterContext _masterContext;

    private delegate Task CaseHandler(dynamic request, HttpContext httpContext);

    private readonly Dictionary<string, CaseHandler> _cases;

    public WebSocketServer(MasterContext masterContext)
    {
        _masterContext = masterContext;

        _cases = new Dictionary<string, CaseHandler>
        {
            {"GetUsersEmail", GetUsersEmailAsync},
            {"CreateMessage", CreateMessageAsync},
            {"GetTopics", GetTopicsAsync},
            {"GetMessages", GetMessagesAsync}
        };
    }

    public async Task AcceptWebSocketAsync(HttpContext httpContext)
    {
        WebSocket = await httpContext.WebSockets.AcceptWebSocketAsync();
    }

    public async Task EchoAsync(HttpContext httpContext)
    {
        var token = CancellationToken.None;

        const int bufferSize = 1024 * 4;
        var buffer = new byte[bufferSize];
        var result = await WebSocket!.ReceiveAsync(new ArraySegment<byte>(buffer), token);
        while (!result.CloseStatus.HasValue)
        {
            var jsonRequest = Encoding.Default.GetString(buffer);

            Console.WriteLine(jsonRequest);

            await CaseAsync(jsonRequest, httpContext!);
            Array.Clear(buffer, 0, buffer.Length);

            result = await WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), token);
        }

        await WebSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, token);
    }

    public async Task SendAsync(object response)
    {
        var settings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Error = (sender, args) => { args.ErrorContext.Handled = true; },
        };
        var jsonResponse = JsonConvert.SerializeObject(response, Formatting.Indented, settings);
        var bytes = Encoding.ASCII.GetBytes(jsonResponse);
        await WebSocket!.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length),
            WebSocketMessageType.Text, WebSocketMessageFlags.EndOfMessage, CancellationToken.None);
    }

    public async Task CaseAsync(string jsonRequest, HttpContext httpContext)
    {
        var request = JsonConvert.DeserializeObject<dynamic>(jsonRequest);
        if (request != null)
        {
            var type = request.Type.ToString();
            if (_cases.ContainsKey(type))
            {
                await _cases[type](request, httpContext);
            }
            else
            {
                await SendAsync(new WebSocketMessage()
                {
                    Type = type + "Response",
                    Error = "Wrong Operation"
                });
            }
        }
    }

    private async Task GetUsersEmailAsync(dynamic request, HttpContext httpContext)
    {
        var token = CancellationToken.None;

        string search = request.Data.Search;

        var listString = await _masterContext.Users
            .Where(user =>
                EF.Functions.Like(user.Email, $"%{search}%") &&
                user.Status == UserStatus.Unblock.ToString())
            .OrderBy(user => user.Email)
            .Take(5)
            .Select(user => user.Email)
            .ToListAsync(token);

        var response = new WebSocketMessage
        {
            Type = request.Type + "Response",
            Data = listString
        };

        await SendAsync(response);
    }

    private async Task CreateMessageAsync(dynamic request, HttpContext httpContext)
    {
        var response = new WebSocketMessage
        {
            Type = request.Type + "Response",
        };

        if (httpContext.User.Identity!.IsAuthenticated == false)
        {
            response.Error = "Not authorized";
            await SendAsync(response);
            return;
        }

        var token = CancellationToken.None;

        await using var transaction = await _masterContext.Database.BeginTransactionAsync(token);

        var authorName = httpContext!.User.Identity.Name;
        var author = await _masterContext.Users
            .Where(user => user.Email == authorName && user.Status == UserStatus.Unblock.ToString())
            .FirstOrDefaultAsync(token);

        var toUserName = (string) request.Data.ToUser.ToString();
        var toUser = await _masterContext.Users
            .Where(user => user.Email == toUserName)
            .FirstOrDefaultAsync(token);

        if (author == null)
        {
            response.Error = "Author not found";
        }

        if (toUser == null)
        {
            response.Error = "ToUser not found";
        }

        if (author != null && toUser != null)
        {
            var modelMessage = new Message()
            {
                Head = request.Data.Head.ToString(),
                Body = request.Data.Body.ToString(),
                AuthorId = author.Id,
                ToUserId = toUser.Id,
                CreateDate = DateTime.Now,
                New = true,
                ReplyId = null
            };

            var entityEntry = await _masterContext.Messages.AddAsync(modelMessage, token);
            await _masterContext.SaveChangesAsync(token);
            response.Data = entityEntry.Entity;
        }

        await transaction.CommitAsync(token);
        await SendAsync(response);
    }

    private async Task GetTopicsAsync(dynamic request, HttpContext httpContext)
    {
        var response = new WebSocketMessage
        {
            Type = request.Type + "Response",
        };

        if (httpContext.User.Identity!.IsAuthenticated == false)
        {
            response.Error = "Not authorized";
            await SendAsync(response);
            return;
        }

        var token = CancellationToken.None;

        var uName = httpContext.User.Identity!.Name;
        var curUser = await _masterContext.Users
            .Where(u =>
                u.Email == uName &&
                u.Status == UserStatus.Unblock.ToString())
            .FirstOrDefaultAsync(token);

        if (curUser == null)
        {
            response.Error = "Current User not found";
            await SendAsync(response);
            return;
        }

        var topics = _masterContext.Messages
            .Where(message =>
                message.AuthorId == curUser.Id ||
                message.ToUserId == curUser.Id)
            .Include(m => m.Author)
            .Include(m => m.ToUser)
            .AsEnumerable()
            .OrderByDescending(m => m.CreateDate)
            .GroupBy(m => m.Head)
            .Select(m => m.First())
            .Take(10)
            .ToList();

        response.Data = topics;
        await SendAsync(response);
    }

    private async Task GetMessagesAsync(dynamic request, HttpContext httpContext)
    {
        var response = new WebSocketMessage
        {
            Type = request.Type + "Response",
        };

        if (httpContext.User.Identity!.IsAuthenticated == false)
        {
            response.Error = "Not authorized";
            await SendAsync(response);
            return;
        }

        var token = CancellationToken.None;

        var uName = httpContext.User.Identity!.Name;

        var curUser = await _masterContext.Users
            .Where(u =>
                u.Email == uName &&
                u.Status == UserStatus.Unblock.ToString())
            .FirstOrDefaultAsync(token);
        if (curUser == null)
        {
            response.Error = "Current User not found";
            await SendAsync(response);
            return;
        }

        int id = request.Data.Id;
        var message = await _masterContext.Messages
            .Where(m => m.Id == id)
            .Include(m => m.Author)
            .Include(m => m.ToUser)
            .FirstOrDefaultAsync(token);
        if (message == null)
        {
            response.Error = "Message not found";
            await SendAsync(response);
            return;
        }

        if (message.AuthorId == curUser.Id || message.ToUserId == curUser.Id)
        {
            var dialogUsersId = new List<int>()
            {
                message.AuthorId,
                message.ToUserId
            };

            var messages = await _masterContext.Messages
                .Where(m =>
                    dialogUsersId.Contains(m.AuthorId) &&
                    dialogUsersId.Contains(m.ToUserId) &&
                    m.Head == message.Head
                )
                .Include(m => m.Author)
                .Include(m => m.ToUser)
                .OrderBy(m => m.CreateDate)
                .Take(100)
                .ToListAsync(token);

            response.Data = new
            {
                Messages = messages,
                UserId = curUser.Id
            };
            await SendAsync(response);
        }
    }
}