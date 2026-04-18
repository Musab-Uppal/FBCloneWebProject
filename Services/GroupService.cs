using semproject.Models;
using semproject.Repositories.Interfaces;
using semproject.Services.Interfaces;

namespace semproject.Services
{
    public class GroupService : IGroupService
    {
        private readonly IGroupRepository _groupRepository;

        public GroupService(IGroupRepository groupRepository)
        {
            _groupRepository = groupRepository;
        }

        public async Task<List<Group>> SearchGroupsAsync(string searchTerm)
        {
            return await _groupRepository.SearchGroupsAsync(searchTerm);
        }

        public async Task<Group> CreateGroupAsync(string name, string description, string createdById, bool isPrivate = false)
        {
            var group = new Group
            {
                Name = name,
                Description = description,
                CreatedById = createdById,
                IsPrivate = isPrivate,
                CreatedAt = DateTime.UtcNow
            };

            return await _groupRepository.AddAsync(group);
        }

        public async Task<Group> GetGroupByIdAsync(int groupId)
        {
            return await _groupRepository.GetByIdAsync(groupId);
        }

        public async Task<List<Group>> GetUserGroupsAsync(string userId)
        {
            return await _groupRepository.GetUserGroupsAsync(userId);
        }

        public async Task<List<Group>> GetPublicGroupsAsync()
        {
            return await _groupRepository.GetPublicGroupsAsync();
        }

        public async Task<Group> GetGroupWithDetailsAsync(int groupId)
        {
            return await _groupRepository.GetGroupWithDetailsAsync(groupId);
        }

        public async Task<bool> AddMemberToGroupAsync(int groupId, string userId)
        {
            return await _groupRepository.AddMemberToGroupAsync(groupId, userId);
        }

        public async Task<bool> AddGroupMemberAsync(int groupId, string userId)
        {
            return await _groupRepository.AddMemberToGroupAsync(groupId, userId);
        }

        public async Task<bool> RemoveMemberFromGroupAsync(int groupId, string userId)
        {
            return await _groupRepository.RemoveMemberFromGroupAsync(groupId, userId);
        }

        public async Task<bool> RemoveGroupMemberAsync(int groupId, string userId, string requestingUserId)
        {
            var isAdmin = await _groupRepository.IsUserAdminAsync(groupId, requestingUserId);
            if (!isAdmin && requestingUserId != userId)
                throw new UnauthorizedAccessException("Only group admins can remove members");

            return await _groupRepository.RemoveMemberFromGroupAsync(groupId, userId);
        }

        public async Task<bool> UpdateGroupAsync(Group group)
        {
            return await _groupRepository.UpdateAsync(group);
        }

        public async Task<bool> UpdateGroupAsync(int groupId, string name, string description, bool isPrivate, string coverImageUrl)
        {
            var group = await _groupRepository.GetByIdAsync(groupId);
            if (group == null)
                return false;

            group.Name = name;
            group.Description = description;
            group.IsPrivate = isPrivate;
            group.CoverImageUrl = coverImageUrl;

            return await _groupRepository.UpdateAsync(group);
        }

        public async Task<bool> DeleteGroupAsync(int groupId)
        {
            return await _groupRepository.DeleteAsync(groupId);
        }

        public async Task<List<GroupMember>> GetGroupMembersAsync(int groupId)
        {
            return await _groupRepository.GetGroupMembersAsync(groupId);
        }

        public async Task<bool> IsUserGroupMemberAsync(int groupId, string userId)
        {
            return await _groupRepository.IsUserGroupMemberAsync(groupId, userId);
        }

        public async Task<bool> IsUserMemberAsync(int groupId, string userId)
        {
            return await _groupRepository.IsUserGroupMemberAsync(groupId, userId);
        }

        public async Task<bool> IsUserAdminAsync(int groupId, string userId)
        {
            return await _groupRepository.IsUserAdminAsync(groupId, userId);
        }

        public async Task<GroupPost> SharePostToGroupAsync(int groupId, int postId, string userId)
        {
            return await _groupRepository.SharePostToGroupAsync(groupId, postId, userId);
        }
    }
}