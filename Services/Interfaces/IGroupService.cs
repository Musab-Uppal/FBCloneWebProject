using semproject.Models;

namespace semproject.Services.Interfaces
{
    public interface IGroupService
    {
        Task<List<Group>> SearchGroupsAsync(string searchTerm);
        Task<Group> CreateGroupAsync(string name, string description, string createdById, bool isPrivate = false);
        Task<Group> GetGroupByIdAsync(int groupId);
        Task<List<Group>> GetUserGroupsAsync(string userId);
        Task<List<Group>> GetPublicGroupsAsync();
        Task<Group> GetGroupWithDetailsAsync(int groupId);
        Task<bool> AddMemberToGroupAsync(int groupId, string userId);
        Task<bool> AddGroupMemberAsync(int groupId, string userId);
        Task<bool> RemoveMemberFromGroupAsync(int groupId, string userId);
        Task<bool> RemoveGroupMemberAsync(int groupId, string userId, string requestingUserId);
        Task<bool> UpdateGroupAsync(Group group);
        Task<bool> UpdateGroupAsync(int groupId, string name, string description, bool isPrivate, string coverImageUrl);
        Task<bool> DeleteGroupAsync(int groupId);
        Task<List<GroupMember>> GetGroupMembersAsync(int groupId);
        Task<bool> IsUserGroupMemberAsync(int groupId, string userId);
        Task<bool> IsUserMemberAsync(int groupId, string userId);
        Task<bool> IsUserAdminAsync(int groupId, string userId);
        Task<GroupPost> SharePostToGroupAsync(int groupId, int postId, string userId);
    }
}