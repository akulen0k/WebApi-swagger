using System.Net;
using System.Text;
using System.Web;
using System;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => "Hello World!");

app.MapGet("/getRandomWord/{chars?}", async (string? chars) =>
{
    if (chars is null || chars.Length == 0)
        return Results.BadRequest("String cant be empty");

    var charArr = chars.ToCharArray();
    Array.Sort(charArr);
    for (int i = 1; i < charArr.Length; ++i)
        if (charArr[i - 1] == charArr[i])
            return Results.BadRequest("String must contain only unique characters");

    var strB = new StringBuilder("");
    var rnd = new Random((int)((DateTime.Now.Ticks) % Int32.MaxValue));
    int n = rnd.Next(5, 100);
    for (int i = 0; i < n; ++i)
    {
        strB.Append(charArr[rnd.Next(charArr.Length)]);
    }
    
    return Results.Ok(strB.ToString());
});

app.MapGet("/getAllLinks/{link}", async (string link) =>
{
    if (link is null || link.Length == 0)
        return Results.BadRequest("Link cant be null");

    link = HttpUtility.UrlDecode(link);
    try
    { 
        WebRequest request = (HttpWebRequest)WebRequest.Create(link);
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();

        if (response.StatusCode == HttpStatusCode.OK)
        {
            Stream receiveStream = response.GetResponseStream();
            StreamReader readStream = null;
            if (String.IsNullOrWhiteSpace(response.CharacterSet))
                readStream = new StreamReader(receiveStream);
            else
                readStream = new StreamReader(receiveStream,
                    Encoding.GetEncoding(response.CharacterSet));
            string data = readStream.ReadToEnd();
            response.Close();
            readStream.Close();
            
            var pattern = "href=\"[^\"]+\"";
            var allLinks = new List<string>();
            foreach (Match v in Regex.Matches(data, pattern))
                allLinks.Add(v.Value);
                
            return Results.Ok(allLinks.ToArray());
        }

        throw new Exception("Invalid response");
    }
    catch (Exception e)
    {
        return Results.BadRequest(e.Message);
    }
});

app.Run();
