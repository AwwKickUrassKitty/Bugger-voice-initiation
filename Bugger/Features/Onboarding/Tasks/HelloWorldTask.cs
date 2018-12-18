using Discord;

namespace Bugger.Features.Onboarding.Tasks
{
    public class HelloWorldTask : IOnboardingTask
    {
        private readonly Logger logger;

        public HelloWorldTask(Logger logger)
        {
            this.logger = logger;
        }

        public async void OnJoined(IGuild guild)
        {
            var defaultChannel = await guild.GetDefaultChannelAsync();
            
            if(defaultChannel is null)
            {
                await logger.Log(LogSeverity.Error, "Onboarding > HelloWorldTask", $"Serwer ({guild.Name}) nie ma domy�lnego kana�u.");
                return;
            }
            
            await defaultChannel.SendMessageAsync("Siemka! Jestem **Bugger** :punch::boom: najlepszy polskoj�zyczny bot discordowy :exclamation: _(bo jedyny xd)_ Nie chc� si� chwiali� czy co�, ale opr�cz przedstawienia si� potrafi� te� np. reagowa� na ludzkie komendy! Je�li tylko b�dziecie gotowi oznaczcie mnie i napiszcie `pomoc` Mi�ej zabawy!");
        }
    }
}
