using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using LeanCloud;
using LeanCloud.Storage;
using LeanCloud.Realtime;

namespace ChatStack
{
    public class Client
    {

        public bool Beep = true;

        LCIMClient UserClient;
        LCIMConversation Chatroom;

        Dictionary<string, string> UsernameIndex = new Dictionary<string, string>();

        // TODO: Detect if the user is typing.
        // Queue<LCIMTextMessage> MessageBuffer = new Queue<LCIMTextMessage>();


        public static void InitApp(string appId, string appKey, string url = null)
        {
            // Initialize the app with the LeanCloud credentials.
            LCApplication.Initialize(appId, appKey, url);
        }

        public static async Task<LCUser> LoginUserAsync(string username, string password)
        {
            // Check if the username is an email address.
            if (new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(username))
            {
                return await LCUser.LoginByEmail(username, password);
            }
            return await LCUser.Login(username, password);
        }

        public async Task LoginClientAsync(string username, string password)
        {
            // Log in to client using LCUser.
            UserClient = new LCIMClient(await LoginUserAsync(username, password));

            await InitClientAsync();
        }

        public async Task InitClientAsync()
        {
            // Initiate the client with the server.
            await UserClient.Open();
        }

        public async Task GetChatroomAsync()
        {
            // Query the ChatStack chatroom.
            var conversationQuery = UserClient.GetQuery()
                .WhereEqualTo("name", "ChatStack")
                .WhereEqualTo("tr", true);
            var queryResult = await conversationQuery.Find();

            Chatroom = queryResult[0];
        }

        public async Task StartChatAsync()
        {

            // Join the chat.
            await Chatroom.Join();

            // TODO: Fetch previous messages.

            // Print the prompt and the number of online users in the chatroom.
            Console.WriteLine("You are now in the chat. Type /exit or /quit to exit.");
            Console.WriteLine(await Chatroom.GetMembersCount() + " people online.");

            // Listen for messages.
            UserClient.OnMessage = async (_, message) =>
            {
                // If the message received is text, assign it to textMessage.
                if (message is LCIMTextMessage textMessage)
                {

                    if (!UsernameIndex.ContainsKey(textMessage.FromClientId))
                    {
                        // Query and save the sender's username if it's not in the index.
                        LCQuery<LCUser> senderQuery = LCUser.GetQuery();
                        UsernameIndex[textMessage.FromClientId] = (await senderQuery.Get(textMessage.FromClientId)).Username;
                    }

                    // TODO: Detect if the user is typing.
                    // // Peek the input stream without reading it.
                    // if ((new StreamReader(Console.OpenStandardInput()).Peek() == -1))
                    // {
                    //     // If the user hasn't typed anything, print the message.
                    //     PrintMessage(textMessage);

                    //     // Print the prompt again.
                    //     Console.Write("> ");
                    // }
                    // else
                    // {
                    //     // If the user is typing, save the message to the buffer.
                    //     MessageBuffer.Enqueue(textMessage);
                    // }

                    PrintMessage(textMessage);

                }
            };

            // Ask for message input constantly.
            await AskForMessageInputAsync();

        }

        async Task AskForMessageInputAsync()
        {
            while (true)
            {

                // Print prompt and get input.
                Console.Write("> ");
                string input = await Task.Run(() => Console.ReadLine().Trim());

                // TODO: Detect if the user is typing.
                // Clear message buffer.
                // while (MessageBuffer.Count > 0)
                //     PrintMessage(MessageBuffer.Dequeue());

                // Check the input for exit commands.
                if (input == "/exit" || input == "/quit") break;

                // Send the message if the input is not empty.
                if (!string.IsNullOrWhiteSpace(input))
                {
                    _ = await Chatroom.Send(new LCIMTextMessage(input));
                }

            }
        }

        void PrintMessage(LCIMTextMessage message)
        {

            // Play a beep sound if beep is enabled.
            if (Beep)
                Console.Beep();

            // TODO: Detect if the user is typing.
            // Set cursor position to the beginning of the line.
            // Console.SetCursorPosition(0, Console.CursorTop);
            Console.WriteLine();

            // Print the message sender's username and the message content.
            Console.WriteLine($"* {UsernameIndex[message.FromClientId]}: {message.Text}");

            // Print the prompt again.
            Console.Write("> ");

        }

        // public static void DebugLogger(LCLogLevel level, string info)
        // {
        //     switch (level)
        //     {
        //         case LCLogLevel.Debug:
        //             Console.WriteLine("[DEBUG] " + info);
        //             break;
        //         case LCLogLevel.Warn:
        //             Console.WriteLine("[WARNING] " + info);
        //             break;
        //         case LCLogLevel.Error:
        //             Console.WriteLine("[ERROR] " + info);
        //             break;
        //         default:
        //             Console.WriteLine(info);
        //             break;
        //     }
        // }

    }
}