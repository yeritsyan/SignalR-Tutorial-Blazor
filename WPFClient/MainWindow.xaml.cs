using System.Text;
using System.Windows;
using System;
using Microsoft.AspNetCore.SignalR.Client;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace WPFClient;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private HubConnection _connection;

    public MainWindow()
    {
        InitializeComponent();

        _connection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7289/chatHub")
            .WithAutomaticReconnect()
            .Build();   

        _connection.Reconnecting += (sender) =>         
        {
            this.Dispatcher.Invoke(() =>
            {
                var newMessage = "Attempting to reconnect...";
                this.messageList.Items.Add(newMessage);
            });

            return Task.CompletedTask;
        };

        _connection.Reconnected += (sender) =>
        {
            this.Dispatcher.Invoke(() =>
            {
                var newMessage = "Reconnected to the server!";
                this.messageList.Items.Clear();
                this.messageList.Items.Add(newMessage);
            });

            return Task.CompletedTask;
        };

        _connection.Closed += (sender) =>
        {
            this.Dispatcher.Invoke(() =>
            {
                var newMessage = "Connection closed!";
                this.messageList.Items.Add(newMessage);
                openConnection.IsEnabled = true;
                sendMessage.IsEnabled = false;
            });

            return Task.CompletedTask;
        };
    }

    private async void openConnection_Click(object sender, RoutedEventArgs e)
    {
        _connection.On<string, string>("ReceiveMessage", (user, message) =>
        {
            this.Dispatcher.Invoke(() =>
            {
                var newMessage = $"{user}: {message}";
                this.messageList.Items.Add(newMessage);
            });
        });

        try
        {
            await _connection.StartAsync();
            messageList.Items.Add("Connection started!");
            openConnection.IsEnabled = false;
            sendMessage.IsEnabled = true;
        }
        catch (Exception ex)
        {
            messageList.Items.Add(ex.Message);
        }
    }

    private async void sendMessage_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await _connection.InvokeAsync("SendMessage", "WPF Client", this.messageInput.Text);
        }
        catch (Exception ex)
        {

            messageList.Items.Add(ex.Message);
        }
    }
}