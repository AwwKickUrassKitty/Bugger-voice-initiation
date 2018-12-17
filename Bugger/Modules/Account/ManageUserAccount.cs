using System;
using System.Linq;
using System.Threading.Tasks;
using Bugger.Extensions;
using Bugger.Features.GlobalAccounts;
using Bugger.Helpers;
using Bugger.Entities;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Bugger.Modules.Account
{
    [Group("Konto"), Alias("U�ytkownik", "uZYTKOWNIK", "User", "Account")]
    public class ManageUserAccount : ModuleBase<MiunieCommandContext>
    {
        [Command("Info"), Alias("Informacje", "Informations"), Remarks("Poka�� tyle, co o tym kim� wiem.")]
        public async Task AccountInformation(SocketGuildUser user = null)
        {
            user = user ?? (SocketGuildUser) Context.User;

            var userAccount = GlobalUserAccounts.GetUserAccount(user);
            
            var embed = new EmbedBuilder()
                .WithAuthor($"Informacje o {user.Username}", user.GetAvatarUrl())
                .AddField("Do��czy�: ", user.JoinedAt.Value.DateTime.ToString())
                .AddField("**Ostatnia wiadomo��:**", userAccount.LastMessage.ToString(), true)
                .AddField("**Ostatne Daily:**", userAccount.LastDaily.ToString(), true)
                .AddField("**Punkty Szcz�cia**:", userAccount.Miunies, true)
                .AddField("**Ilo�� prywatnych tag�w**:", userAccount.Tags.Count, true)
                .AddField("**Ilo�� aktywnych przypomnie�**: ", userAccount.Reminders.Count, true)
                .WithColor(0,255,0)
                .WithCurrentTimestamp()
                .WithFooter($"Na zlecenie: {Context.User.Username}")
                .Build();
            
            await Context.Channel.SendMessageAsync("", false, embed);
        }

        [Command("HistoriaPolece�"), Alias("P", "HP", "Polecenia", "Komendy", "Poka�Histori�Polece�", "CommandHistory", "ShowCommandHistory", "HistoriaPolecen", "PokazHistoriePolecen"), Remarks("Zwr�c� histori� polece� danego u�ytkownika!")]
        public async Task ShowCommandHistory()
        {            
            await Context.Channel.SendMessageAsync(GetCommandHistory(Context.UserAccount));
        }
        
        //Could be in the extended ModuleBase, with a few changes
        private string GetCommandHistory(GlobalUserAccount userAccount)
        {
            var commandHistory = userAccount.CommandHistory.Select(cH => $"{cH.UsageDate.ToString("G")} {cH.Command}");
            return String.Join("\n", commandHistory); //Return the command history separated by line
        }
        
        [Command("Dane"), Alias("MojeDane", "DajMojeDane", "GetMyData", "MyData", "GetAllMyAccountData"), Remarks("Wy�l� plik z tym, co o Tb wiem. (a nawet jeszcze wi�cej)")]
        public async Task GetAccountFile()
        {
            var userFilePath = GlobalUserAccounts.GetAccountFilePath(Context.User.Id);
            if (String.IsNullOrEmpty(userFilePath))
            {
                Context.Channel.SendMessageAsync("Nie znam Ci�, kim jeste�???");
                return;
            }

            await Context.User.SendFileAsync(userFilePath, $"Masz tu, wszyystko...");
            await Context.Channel.SendMessageAsync($"{Context.User.Mention} **DM**es wys�any!");
        }

        [Command("Usu�WszystkieMojeDane", RunMode = RunMode.Async), Alias("UsunWszystkieMojeDane", "DeleteAllMyAccountData"), Remarks("Jako, i� jestem :heart: **___RODO___** :heart:-Friendly, usun� wszystkie zgromadzone Twoje dane.")]
        public async Task DeleteAccount()
        {
            var response = await AwaitMessageYesNo("Jeste� pewnien? Stracisz wszystko!!!", "Tak, usu�", "Nie, nie usuwaj");
            if (response is null)
            {
                await Context.Channel.SendMessageAsync($"Zdecydowa�e� si� ju� {Context.User.Mention}?");
            }
            else
            {
                await EvaluateResponse(response, "Tak");
            }
        }

        
        private async Task EvaluateResponse(SocketMessage response, string optionYes)
        {
            var message = "";
            if (response.Content.ToLower().Contains(optionYes.ToLower()))
            {
                message = GlobalUserAccounts.DeleteAccountFile(Context.User.Id)
                    ? "Wymaza�em wszystkie Twoje dan... czekaj... a kto� Ty?"
                    : "Nie znam Ci�, wi�c nic nie usun� :smile:";
            }
            else
            {
                message = "Huhh... nigdy Ci� nie zapomn�!";
            }

            await Context.Channel.SendMessageAsync(Context.User.Mention + " " + message);
        }

        private async Task<SocketMessage> AwaitMessageYesNo(string message, string optionYes, string optionNo)
        {
            await Context.Channel.SendMessageAsync(
                $"{Context.User.Mention} {message} \nOdpowiedz `{optionYes}` albo `{optionNo}`");
            var response = await Context.Channel.AwaitMessage(msg => EvaluateResponse(msg, optionYes, optionNo));
            return response;
        }

        private bool EvaluateResponse(SocketMessage arg, params String[] options)
            => options.Any(option => arg.Content.ToLower().Contains(option.ToLower()) && arg.Author == Context.User);
    }
}
