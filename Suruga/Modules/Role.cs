using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Suruga.Services;

namespace Suruga.Modules
{
    public class Role : BaseCommandModule
    {
        private readonly RoleService roleService;

        public Role(RoleService roleservice)
            => roleService = roleservice;

        [Command("role_create")]
        [Description("Creates a role in a guild.")]
        [RequireBotPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.ManageRoles)]
        [RequireUserPermissions(Permissions.AccessChannels | Permissions.SendMessages | Permissions.ManageRoles)]
        public async Task CreateRoleCommand(CommandContext commandContext, [RemainingText] string roleName)
            => await roleService.CreateRoleAsync(commandContext.Channel, commandContext.Member, commandContext.Guild, roleName);
    }
}
