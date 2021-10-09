using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using LeanCloud.Storage;
using DocoptNet;

namespace ChatStack
{
    class Program
    {

        static async Task Main(string[] args)
        {

            // Parse the arguments using Docopt.
            var arguments = new Docopt().Apply(Usage, args, version: "ChatStack CLI v1.0", exit: true);

            var client = await CheckArgumentsAsync(arguments);

            // CheckArguments() returns null if the command is executed and doesn't need a chat interface.
            if (client is null) return;


            #region Chat interface setup

            Console.Title = "ChatStack CLI";

            // Disable the beed sound if option --mute is set.
            client.Beep = !arguments["--mute"].IsTrue;

            await client.StartChatAsync();

            #endregion


            // TODO: Encrypt and save the username and password.

        }


        static async Task<Client> CheckArgumentsAsync(IDictionary<string, ValueObject> args)
        {

            // TODO: Save login state.
            // string savedClientPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ChatStack", "SavedClient.bin");

            if (args["logout"].IsTrue)
            {
                // Delete the saved client.
                // File.Delete(savedClientPath);

                Console.WriteLine("Logged out.");
                return null;
            }


            #region Commands that require LeanCloud

            // Log LeanCloud debug info.
            // LeanCloud.LCLogger.LogDelegate += Client.DebugLogger;

            // Initialize LeanCloud.
            // NOTE: Add your LeanCloud credentials to 'LeanCloudCredentials.EDIT_ME'.
            Client.InitApp(LeanCloudCredentials.AppID, LeanCloudCredentials.AppKey, LeanCloudCredentials.Url);

            if (args["signup"].IsTrue)
            {
                // Check if the password and password confirmation match.
                if (args["<password>"] == args["<password-confirm>"])
                {
                    // Create a new user.
                    var user = new LCUser();

                    user.Username = args["<username>"].ToString();
                    user.Password = args["<password>"].ToString();

                    // Add an email property to the user if provided one.
                    if (!args["--email"].IsNullOrEmpty)
                        user.Email = args["--email"].ToString();

                    // Sign up the user.
                    await user.SignUp();
                    Console.WriteLine("User created.");

                    if (!args["--email"].IsNullOrEmpty)
                        Console.WriteLine("Verification email sent. Please check your inbox.");
                }
                else
                {
                    Console.WriteLine("Passwords do not match.");
                }

                return null;
            }

            if (args["bind"].IsTrue)
            {
                // Get the user.
                var user = await Client.LoginUserAsync(args["<username>"].ToString(), args["<password>"].ToString());

                // Set the user's email.
                user.Email = args["--email"].ToString();

                await user.Save();

                Console.WriteLine("Verification email sent. Please check your inbox.");
                return null;
            }

            if (args["chpwd"].IsTrue)
            {
                // Check if the password and password confirmation match.
                if (args["<new-password>"] == args["<new-password-confirm>"])
                {
                    // Get the user.
                    var user = await Client.LoginUserAsync(args["<username>"].ToString(), args["<password>"].ToString());

                    // Set the user's password.
                    user.Password = args["<new-password>"].ToString();

                    await user.Save();
                    Console.WriteLine("Password changed.");
                }
                else
                {
                    Console.WriteLine("New passwords do not match.");
                }

                return null;
            }

            if (args["resetpwd"].IsTrue)
            {
                // Request a password reset email.
                await LCUser.RequestPasswordReset(args["--email"].ToString());

                Console.WriteLine("Password reset email sent. Please check your inbox.");
                return null;
            }

            #endregion


            #region Commands that need a chat interface

            // Create a Client object.
            var client = new Client();

            // Check if the login command was called.
            if (args["login"].IsTrue)
            {
                // Log the user in.
                await client.LoginClientAsync(args["<username>"].ToString(), args["<password>"].ToString());

                // Query and get the chatroom.
                await client.GetChatroomAsync();
            }
            else
            {
                // No command was called, so read and deserialize the saved client.
                // TODO: Read from the saved login state.
                // if (File.Exists(savedClientPath))
                // {

                // }
                // else
                // {
                //     Console.WriteLine("You are not logged in. Please use 'chatstack login'.");
                //     return null;
                // }

                Console.WriteLine("Saved login state is not implemented yet. Please use 'chatstack login'.");
            }

            return client;

            #endregion

        }


        static readonly string Usage = @"ChatStack CLI by @HackingHackers

Usage:
  chatstack [--mute]
  chatstack login (<username> | <email-address>) <password> [--mute]
  chatstack logout
  chatstack signup <username> <password> <password-confirm> [--email <email-address>]
  chatstack bind (<username> | <email-address>) <password> --email <email-address>
  chatstack chpwd (<username> | <email-address>) <password> <new-password> <new-password-confirm>
  chatstack resetpwd --email <email-address>
  chatstack -h | --help
  chatstack --version

Options:
  -h --help                Show this help message.
  --version                Show version.
  --mute                   Stop playing the beep sound when you receive a message.
  --email <email-address>  Send a verification or password reset email to the given email address. Check your inbox.";


    }
}
