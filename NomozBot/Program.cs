using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Net.Http;
using System.Text.Json;

class Program
{
    private static string Token = ""; // BotFather dan olgan tokeningizni qo'ying
    private static ITelegramBotClient bot = new TelegramBotClient(Token);

    static async Task Main(string[] args)
    {
        Console.WriteLine("Bot ishga tushdi...");

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = { } // Har qanday update turini qabul qiladi
        };

        bot.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions);
        Console.ReadLine(); // Console yopilmasligi uchun
    }

    private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Type == UpdateType.Message && update.Message is not null)
        {
            var message = update.Message;

            if (message.Text == "/start")
            {
                // Locatsiya so'rash
                var replyMarkup = new ReplyKeyboardMarkup(new[]
                {
                    new KeyboardButton("Locatsiya yuborish") { RequestLocation = true }
                })
                {
                    ResizeKeyboard = true,
                    OneTimeKeyboard = true
                };

                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Locatsiyangizni yuboring:",
                    replyMarkup: replyMarkup,
                    cancellationToken: cancellationToken
                );
            }
            else if (message.Location is not null)
            {
                var location = message.Location;

                // Locatsiyadan shahar nomini olish
                string cityName = await GetCityNameAsync((float)location.Latitude, (float)location.Longitude);

                // Sayt URL
                string websiteUrl = "https://irmuhammadjon.github.io/NomozVaqti3/";

                // Inline tugma yuborish
                var inlineKeyboard = new InlineKeyboardMarkup(new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithWebApp("Saytga o'tish", websiteUrl)
                });

                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: $"Sizning locatsiyangiz: {cityName}\nMana saytga o'ting:",
                    replyMarkup: inlineKeyboard,
                    cancellationToken: cancellationToken
                );
            }
        }
    }

    private static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Xato yuz berdi: {exception.Message}");
    }

    private static async Task<string> GetCityNameAsync(float latitude, float longitude)
    {
        using var httpClient = new HttpClient();
        string apiUrl = $"https://geocode.xyz/{latitude},{longitude}?geoit=json";

        var response = await httpClient.GetStringAsync(apiUrl);
        var jsonDoc = JsonDocument.Parse(response);

        if (jsonDoc.RootElement.TryGetProperty("city", out var city))
        {
            return city.GetString();
        }

        return "Shahar aniqlanmadi";
    }
}
