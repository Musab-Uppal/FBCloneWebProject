using semproject.Models;

namespace semproject.Repositories.Interfaces
{
    public interface IGroupRepository : IRepository<Group>
    {
        Task<List<Group>> SearchGroupsAsync(string searchTerm);
        Task<List<Group>> GetUserGroupsAsync(string userId);
        Task<List<Group>> GetPublicGroupsAsync();
        Task<Group> GetGroupWithDetailsAsync(int groupId);
        Task<List<GroupMember>> GetGroupMembersAsync(int groupId);
        Task<bool> AddMemberToGroupAsync(int groupId, string userId);
        Task<bool> RemoveMemberFromGroupAsync(int groupId, string userId);
        Task<bool> IsUserGroupMemberAsync(int groupId, string userId);
        Task<bool> IsUserAdminAsync(int groupId, string userId);
        Task<GroupPost> SharePostToGroupAsync(int groupId, int postId, string userId);
    }
}