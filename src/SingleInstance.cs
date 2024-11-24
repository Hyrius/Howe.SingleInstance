using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TinyIpc.Messaging;

namespace SingleInstanceCore
{
	public static class SingleInstance
	{
		private const string channelNameSufflix = ":SingeInstanceIPCChannel";
		//For detecting if mutex is locked (first instance is already up then)
		private static Mutex singleMutex;
		//Message bus for communication between instances
		private static TinyMessageBus messageBus;

        /// <summary>
        /// Intended to be on app startup
        /// Initializes service if the call is from first instance.
        /// Signals the first instance if it already exists
        /// </summary>
        /// <param name="uniqueName">A unique name for IPC channel</param>
        /// <returns>Whether the call is from application's first instance</returns>
        public static bool InitializeAsFirstInstance<T>(this T instance, string uniqueName) where T : ISingleInstance
        {
            var applicationIdentifier = $"{uniqueName}{Environment.UserName}";
            var channelName = $"{applicationIdentifier}{channelNameSufflix}";
            singleMutex = new Mutex(true, applicationIdentifier, out var firstInstance);

            if (firstInstance)
            {
                CreateRemoteService(instance, channelName);
            }
            else
            {
                SignalFirstInstance(channelName, Environment.GetCommandLineArgs());
            }

            return firstInstance;
        }

        public static async ValueTask<bool> InitializeAsFirstInstanceAsync<T>(this T instance, string uniqueName) where T:ISingleInstance
		{
			var applicationIdentifier = $"{uniqueName}{Environment.UserName}";
			var channelName = $"{applicationIdentifier}{channelNameSufflix}";
			singleMutex = new Mutex(true, applicationIdentifier, out var firstInstance);

			if (firstInstance)
			{
                CreateRemoteService(instance, channelName);
            }
			else
			{
                await SignalFirstInstanceAsync(channelName, Environment.GetCommandLineArgs());
            }

			return firstInstance;
		}

		private static async Task SignalFirstInstanceAsync(string channelName, IList<string> commandLineArgs)
		{
            using TinyMessageBus bus = new(channelName);
            await bus.PublishAsync(commandLineArgs.Serialize());
        }

        private static void SignalFirstInstance(string channelName, IList<string> commandLineArgs)
        {
            using TinyMessageBus bus = new(channelName);
            bus.PublishAsync(commandLineArgs.Serialize()).GetAwaiter().GetResult();
        }

        private static void CreateRemoteService(ISingleInstance instance, string channelName)
		{
			messageBus = new TinyMessageBus(channelName);
			messageBus.MessageReceived += (_, e) =>
			{
				instance.OnInstanceInvoked(e.Message.Deserialize<string[]>());
			};
		}

		public static void Cleanup()
		{
			if (messageBus is not null)
			{
				messageBus.Dispose();
				messageBus = null;
			}
			if (singleMutex is not null)
			{
				singleMutex.Close();
				singleMutex = null;
			}
		}
	}
}