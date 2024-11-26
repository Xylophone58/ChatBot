using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

public class ChatbotForm : Form
{
    private TextBox inputTextBox;
    private Button sendButton;
    private RichTextBox chatTextBox;

    public ChatbotForm()
    {
        // Set up the form
        this.Text = "ChatBot";
        this.Size = new System.Drawing.Size(400, 500);

        // Chat display - Use RichTextBox for colored text
        chatTextBox = new RichTextBox
        {
            ReadOnly = true,
            ScrollBars = (RichTextBoxScrollBars)ScrollBars.Vertical,
            Dock = DockStyle.Top,
            Height = 350,
            Font = new System.Drawing.Font("Arial", 10)
        };
        this.Controls.Add(chatTextBox);

        // Input text box
        inputTextBox = new TextBox
        {
            Dock = DockStyle.Bottom,
            Height = 30
        };
        this.Controls.Add(inputTextBox);

        // Send button
        sendButton = new Button
        {
            Text = "Send",
            Dock = DockStyle.Bottom,
            Height = 30
        };
        sendButton.Click += async (sender, e) => await HandleInput();
        this.Controls.Add(sendButton);
    }

    private async Task HandleInput()
    {
        string userInput = inputTextBox.Text;
        inputTextBox.Clear();

        if (string.IsNullOrWhiteSpace(userInput))
            return;

        // Display user's input in blue
        AppendTextToChatBox($"You: {userInput}\r\n", System.Drawing.Color.Blue);

        string response = await GetChatbotResponse(userInput);

        // Display chatbot's response in red
        AppendTextToChatBox($"Chatbot: {response}\r\n", System.Drawing.Color.Red);
    }

    private void AppendTextToChatBox(string text, System.Drawing.Color color)
    {
        chatTextBox.SelectionStart = chatTextBox.TextLength;
        chatTextBox.SelectionLength = 0;
        chatTextBox.SelectionColor = color;
        chatTextBox.AppendText(text);
        chatTextBox.SelectionColor = chatTextBox.ForeColor;  // Reset to default color
    }

    private async Task<string> GetChatbotResponse(string input)
    {
        input = input.ToLower();

        if (input.Contains("weather"))
        {
            string city = ExtractEntity(input, "weather");
            if (!string.IsNullOrEmpty(city))
            {
                return await GetWeatherAsync(city);
            }
            return "Please specify a city for the weather (e.g., 'What's the weather in London?').";
        }
        else if (input.Contains("stock"))
        {
            string symbol = ExtractEntity(input, "stock");
            if (!string.IsNullOrEmpty(symbol))
            {
                return await GetStockPriceAsync(symbol);
            }
            return "Please specify a stock ticker (e.g., 'What's the price of AAPL?').";
        }

        return "I can help with weather or stock prices. Try asking about 'weather in [city]' or 'stock price of [ticker]'.";
    }

    private string ExtractEntity(string input, string context)
    {
        if (context == "weather")
        {
            // Extract the city name after "in"
            if (input.Contains("in"))
            {
                int startIndex = input.IndexOf("in") + 3;
                return input.Substring(startIndex).Trim();
            }
        }
        else if (context == "stock")
        {
            // Extract stock ticker after "price of" or "stock"
            if (input.Contains("price of"))
            {
                int startIndex = input.IndexOf("price of") + 9;
                return input.Substring(startIndex).Trim().ToUpper();
            }
            if (input.Contains("stock"))
            {
                int startIndex = input.IndexOf("stock") + 6;
                return input.Substring(startIndex).Trim().ToUpper();
            }
        }

        return null;
    }

    private async Task<string> GetWeatherAsync(string city)
    {
        string apiKey = "6d8e396e7b4c9faa2febd1bc48db5666";
        string url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric";

        using (HttpClient client = new HttpClient())
        {
            try
            {
                string response = await client.GetStringAsync(url);
                var weatherData = JObject.Parse(response);
                string description = weatherData["weather"][0]["description"].ToString();
                string temp = weatherData["main"]["temp"].ToString();

                return $"The weather in {city} is {description} with a temperature of {temp}°C.";
            }
            catch
            {
                return $"I couldn't retrieve the weather data for {city}. Please check the city name and try again.";
            }
        }
    }

    private async Task<string> GetStockPriceAsync(string symbol)
    {
        string apiKey = "W4BHR3LXFOU5MRB5";
        string url = $"https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol={symbol}&apikey={apiKey}";

        using (HttpClient client = new HttpClient())
        {
            try
            {
                string response = await client.GetStringAsync(url);
                var stockData = JObject.Parse(response);
                var lastClose = stockData["Time Series (Daily)"].First.First["4. close"].ToString();

                return $"The previous close of {symbol} was ${lastClose}.";
            }
            catch
            {
                return $"I couldn't retrieve the stock data for {symbol}. Please check the ticker and try again.";
            }
        }
    }
}
