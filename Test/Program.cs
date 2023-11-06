// See https://aka.ms/new-console-template for more information
using System;

Console.WriteLine("Hello, World!");







return;


using HttpClient client = new HttpClient();

//ローカルファイルは読み取れないようだ。
//HttpResponseMessage response2 = await client.GetAsync("C:\\Users\\hiro\\OneDrive\\ドキュメント\\2023\\202310\\japanese-addresses-master\\api\\ja.json");

HttpResponseMessage response = await client.GetAsync("https://geolonia.github.io/japanese-addresses/api/ja.json");
if (response.IsSuccessStatusCode)
{
    

    var a =  await response.Content.ReadAsStringAsync();
    
    //return new Response { await response.Content.ReadAsAsync<object>() };
    //return new Response { json = async () => await response.Content.ReadAsAsync<object>() };
}
else
{
    throw new Exception($"Request failed with status code {response.StatusCode}");
}