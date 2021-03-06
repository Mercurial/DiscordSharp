﻿using DiscordSharp;
using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordSharpTestApplication
{
    class Program
    {
        static DiscordClient client = new DiscordClient();
        public static void Main(string[] args)
        {
            Console.WriteLine("DiscordSharp Tester");
            client.LoginInformation = new DiscordLoginInformation();
            Console.Write("Please enter your email: ");
            string email = Console.ReadLine();
            client.LoginInformation.email[0] = email;

            Console.Write("Now, your password (visible): ");
            string pass = Console.ReadLine();
            client.LoginInformation.password[0] = pass;

            Console.WriteLine("Attempting login..");
            client.MessageReceived += (sender, e) =>
            {
                Console.WriteLine(e.message);
            };
            client.URLMessageAutoUpdate += (sender, e) =>
            {
                string message = "*URL(s) Submitted:*\n";
                for (int i = 0; i < e.embeds.Count; i++)
                    message += string.Format("{0}: Title: {1}, URL: {2}, Type: {3}, Description: {4}, Provider URL: {5}, Provider Name: {6}\n",
                        i, e.embeds[i].title, e.embeds[i].url, e.embeds[i].type, e.embeds[i].description, e.embeds[i].provider_url, e.embeds[i].provider_name);
                client.SendMessageToChannel(message, e.channel);
            };

            client.UserTypingStart += (sender, e) =>
            {
            };
            client.MessageEdited += (sender, e) =>
            {
                if (e.author.user.username == "Axiom")
                    client.SendMessageToChannel("What the fuck, <@" + e.author.user.id + "> you can't event type your message right. (\"" + e.MessageEdited.content + "\")", e.Channel);
            };
            client.ChannelCreated += (sender, e) =>
            {
                var parentServer = client.GetServersList().Find(x => x.channels.Find(y => y.id == e.ChannelCreated.id) != null);
                if (parentServer != null)
                    Console.WriteLine("Channel {0} created in {1}!", e.ChannelCreated.name, parentServer.name);
            };
            client.PrivateChannelCreated += (sender, e) =>
            {
                Console.WriteLine("Private channel started with {0}", e.ChannelCreated.recipient.username);
            };
            client.PrivateMessageReceived += (sender, e) =>
            {
                client.SendMessageToUser("Pong!", e.author);
            };
            client.MentionReceived += (sender, e) =>
            {
                if (e.author.user.id != client.Me.user.id)
                    client.SendMessageToChannel("Heya, @" + e.author.user.username, e.Channel);
            };
            client.MessageReceived += (sender, e) =>
            {
                DiscordServer fromServer = client.GetServersList().Find(x => x.channels.Find(y => y.id == e.Channel.id) != null);
                Console.WriteLine("[- Message from {0} in {1} on {2}: {3}", e.author.user.username, e.Channel.name, fromServer.name, e.message);
                if (e.message.StartsWith("?status"))
                    client.SendMessageToChannel("I work ;)", e.Channel);
                else if (e.message.StartsWith("?notify"))
                {
                    string[] split = e.message.Split(new char[] { ' ' }, 2);
                }
                else if (e.message.StartsWith("?whereami"))
                {
                    DiscordServer server = client.GetServersList().Find(x => x.channels.Find(y => y.id == e.Channel.id) != null);
                    string owner = "";
                    foreach (var member in server.members)
                        if (member.user.id == server.owner_id)
                            owner = member.user.username;
                    string whereami = String.Format("I am currently in *#{0}* ({1}) on server *{2}* ({3}) owned by {4}.", e.Channel.name, e.Channel.id, server.name, server.id, owner);
                    client.SendMessageToChannel(whereami, e.Channel);
                }
                else if (e.message.StartsWith("?everyone"))
                {
                    DiscordServer server = client.GetServersList().Find(x => x.channels.Find(y => y.id == e.Channel.id) != null);
                    string[] split = e.message.Split(new char[] { ' ' }, 2);
                    if (split.Length > 1)
                    {
                        string message = "";
                        foreach (var user in server.members)
                        {
                            if (user.user.id == client.Me.user.id)
                                continue;
                            if (user.user.username == "Blank")
                                continue;
                            message += "@" + user.user.username + " ";
                        }
                        message += ": " + split[1];
                        client.SendMessageToChannel(message, e.Channel);
                    }
                }
                else if (e.message.StartsWith("?lastfm"))
                {
#if __MONOCS__
                        client.SendMessageToChannel("Sorry, not on Mono :(", e.Channel);
#else
                    string[] split = e.message.Split(new char[] { ' ' }, 2);
                    if (split.Length > 1)
                    {
                        using (var lllfclient = new LastfmClient("4de0532fe30150ee7a553e160fbbe0e0", "0686c5e41f20d2dc80b64958f2df0f0c", null, null))
                        {
                            try
                            {
                                var recentScrobbles = lllfclient.User.GetRecentScrobbles(split[1], null, 0, 1);
                                LastTrack lastTrack = recentScrobbles.Result.Content[0];
                                client.SendMessageToChannel(string.Format("*{0}* last listened to _{1}_ by _{2}_", split[1], lastTrack.Name, lastTrack.ArtistName), e.Channel);
                            }
                            catch
                            {
                                client.SendMessageToChannel(string.Format("User _*{0}*_ not found!", split[1]), e.Channel);
                            }
                        }
                    }
                    else
                        client.SendMessageToChannel("Who??", e.Channel);
#endif
                }
                else if (e.message.StartsWith("?whois"))
                {
                    //?whois <@01393408>
                    Regex r = new Regex("\\d+");
                    Match m = r.Match(e.message);
                    Console.WriteLine("WHOIS INVOKED ON: " + m.Value);
                    var foundServer = client.GetServersList().Find(x => x.channels.Find(y => y.id == e.Channel.id) != null);
                    if (foundServer != null)
                    {
                        var foundMember = foundServer.members.Find(x => x.user.id == m.Value);
                        client.SendMessageToChannel(string.Format("<@{0}>: {1}, {2}", foundMember.user.id, foundMember.user.id, foundMember.user.username), e.Channel);
                    }
                }
                else if (e.message.StartsWith("?quoththeraven"))
                    client.SendMessageToChannel("nevermore", e.Channel);
                else if (e.message.StartsWith("?quote"))
                    client.SendMessageToChannel("Luigibot does what Reta don't.", e.Channel);
                else if (e.message.StartsWith("?selfdestruct"))
                {
                    if (e.author.user.username == "Axiom")
                        client.SendMessageToChannel("riparoni and cheese", e.Channel);
                    Environment.Exit(0);
                }
            };
            client.Connected += (sender, e) =>
            {
                Console.WriteLine("Connected! User: " + e.user.user.username);
            };
            client.SocketClosed += (sender, e) =>
            {
                Console.WriteLine("Closed ({0}): {1}", e.Code, e.Reason);
            };
            ConnectStuff();
            while (true) ;
        }

        private static async void ConnectStuff()
        {
            if(await client.SendLoginRequestAsync() != null)
            {
                Console.WriteLine("Logged in..async!");
                client.ConnectAndReadMessages();
            }
        }
    }
}
